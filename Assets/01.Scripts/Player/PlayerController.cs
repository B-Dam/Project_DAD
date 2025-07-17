using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;

    [Header("🌫️ 이동 이펙트")]
    public GameObject dustEffectPrefab;
    public float dustSpawnInterval = 0.3f;
    private float dustTimer = 0f;

    [Header("🔍 상호작용")]
    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    public bool isPushingInputHeld { get; private set; }

    public Vector2 lastMoveInput { get; private set; }


    [Header("막힘 알림")]
    public GameObject blockIndicatorPrefab;
    public LayerMask obstacleLayer;
    public LayerMask playerBlockerLayer;
    private Canvas canvas;

    private Coroutine walkSfxCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        //게임 멈췄을 땐 아무것도 하지 않음
        if (Time.timeScale == 0f) return;

        HandleMovementInput();
        UpdateAnimation(moveInput);
        HandleInteractionInput();

        // 이동 가능 여부 확인
        CanMove();

        // 박스가 있는지 확인하고 밀기 시도
        TryAutoPushBox();

        if (moveInput.magnitude > 0.01f)
        {
            dustTimer += Time.deltaTime;
            if (dustTimer >= dustSpawnInterval)
            {
                SpawnDustEffect();
                dustTimer = 0f;
            }
        }
        else
        {
            dustTimer = 0f;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        CircleCollider2D circleCol = GetComponent<CircleCollider2D>();
        if (circleCol != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(circleCol.offset, circleCol.radius);
        }
    }
#endif
    private void FixedUpdate()
    {
        if (CanMove())
            MovePlayer();

        // 앞에 장애물 감지 + UI 표시
        TryShowObstacleIndicator();
    }
    private bool CanMove()
    {
        // 대화 중이면 이동/상호작용 입력 차단
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            MoveMentReset();
            return false;
        }

        // 맵 이동 트랜지션 중에 이동 차단
        if (MapManager.Instance.fadeManager.fadeCoroutine != null)
        {
            MoveMentReset();
            StopWalkingSFX();
            return false;
        }

        // 전투 중에 이동 차단
        if (CombatManager.Instance != null && (CombatManager.Instance.IsInCombat))
        {
            MoveMentReset();
            StopWalkingSFX();
            return false;
        }
        
        return true;
    }
    public void MoveMentReset()
    {
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        UpdateAnimation(Vector2.zero);
    }
    private void TryShowObstacleIndicator()//P
    {

        // 조건 1: 방향 입력 중일 때만
        if (moveInput == Vector2.zero) return;

        // 너무 짧은 입력 무시
        if (lastMoveInput == Vector2.zero) return;

        // 박스를 밀고 있다면 BoxPush에서 처리하게 둔다
        Vector2 origin = transform.position;
        Vector2 dir = lastMoveInput.normalized;
        float distance = 0.7f;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, obstacleLayer | playerBlockerLayer);
        if (hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;

            // 박스를 만났다면 BoxPush가 판단
            if (hitObj.CompareTag("Box") && hitObj.GetComponent<BoxPush>() != null) return;

            // 장애물이거나, BoxPush 없는 오브젝트일 경우 → UI 띄움
            ShowBlockIndicator(transform.position + new Vector3(0f, 1.2f, 0f));
        }
    }
    private void TryAutoPushBox()
    {
        if (moveInput == Vector2.zero) return;

        Vector2 origin = transform.position;
        Vector2 dir = moveInput.normalized;
        float distance = 0.6f;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, LayerMask.GetMask("Box"));
        if (hit.collider != null)
        {
            GameObject hitBox = hit.collider.gameObject;
            BoxPush box = hitBox.GetComponent<BoxPush>();
            if (box != null)
            {

            }
        }
    }
    private void ShowBlockIndicator(Vector3 worldPos)//P
    {
        // 캔버스 찾기
        if (canvas == null)
        {
            canvas = GameObject.Find("UICanvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[PlayerController] Canvas를 찾을 수 없습니다!");
                return;
            }
        }

        if (canvas == null) return;
        if (Camera.main == null) return;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        GameObject icon = Instantiate(blockIndicatorPrefab, canvas.transform);
        icon.GetComponent<RectTransform>().position = screenPos;
        Destroy(icon, 0.2f);
    }

    private void HandleMovementInput()
    {
        if (CanMove())
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;

            // 입력 유지 상태 감지
            isPushingInputHeld = moveX != 0 || moveY != 0;

            if (moveInput != Vector2.zero)
            {
                lastMoveDirection = moveInput;
                lastMoveInput = moveInput;
            }
        }
    }

    private void MovePlayer()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;

        float speed = direction.magnitude;
        animator.SetFloat("Run", speed); // 핵심 부분

        if (speed > 0.01f)
        {
            lastMoveDirection = direction;
            StartWalkingSFX();
        }
        else
        {
            StopWalkingSFX();
        }

        // 좌우 방향 전환
        if (lastMoveDirection.x < 0)
            transform.localScale = new Vector3(1, 1, 1); // 왼쪽
        else if (lastMoveDirection.x > 0)
            transform.localScale = new Vector3(-1, 1, 1); // 오른쪽
    }


    private void SpawnDustEffect()
    {
        if (dustEffectPrefab == null) return;

        Vector3 offset = (Vector3)(-lastMoveDirection.normalized * 0.3f) + new Vector3(0f, -0.2f, 0f);
        Vector3 spawnPos = transform.position + offset;

        GameObject effect = Instantiate(dustEffectPrefab, spawnPos, Quaternion.identity);
        Destroy(effect, 1f);
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 origin = transform.position;
            Vector2 direction = lastMoveDirection.normalized;

            Debug.DrawRay(origin, direction * interactRange, Color.red, 0.5f); // 시각화

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactRange, interactLayer);

            if (hit.collider != null)
            {
                GameObject target = hit.collider.gameObject;

                if (target.CompareTag("NPC") || target.CompareTag("Door") || target.CompareTag("Item") || target.CompareTag("Interact"))
                {
                    var interactable = target.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                    else
                    {
                        Debug.Log("⚠️ 충돌했지만 IInteractable 없음: " + target.name);
                    }
                }
            }
            else
            {
                Debug.Log("❌ 상호작용 대상 없음");
            }
        }
    }

    private void StartWalkingSFX()
    {
        if (walkSfxCoroutine == null)
            walkSfxCoroutine = StartCoroutine(PlayWalkingSFX());
    }

    private IEnumerator PlayWalkingSFX()
    {
        while (true)
        {
            AudioManager.Instance.PlaySFX("Walk");
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopWalkingSFX()
    {
        if (walkSfxCoroutine != null)
        {
            StopCoroutine(walkSfxCoroutine);
            walkSfxCoroutine = null;
        }
    }
}
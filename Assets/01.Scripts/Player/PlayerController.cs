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
    public LayerMask obstacleLayer;
    public LayerMask playerBlockerLayer;
    private float lastIndicatorTime = -10f;

    private Coroutine walkSfxCoroutine;

    // 박스 미리 유지 시간 관련 변수
    public float boxPushHoldTime = 0.5f;
    private float pushTimer = 0f;
    private bool isTryingToPush = false;
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
        if (!enabled) return;

        HandleMovementInput();
        UpdateAnimation(moveInput);
        HandleInteractionInput();

        // 이동 가능 여부 확인
        CanMove();

        CheckPushHoldAndTry();

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
    public bool CanMove()
    {
        // 대화 중이면 이동/상호작용 입력 차단
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            MoveMentReset();
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
        if (Time.time - lastIndicatorTime < boxPushHoldTime) return;

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

            BlockIndicatorManager.Instance?.ShowIndicator(transform.position + new Vector3(0f, 1.2f, 0f));
            lastIndicatorTime = Time.time; // 마지막 표시 시간 갱신
        }
    }
    private void TryAutoPushBox()
    {
        if (moveInput == Vector2.zero) return;

        Vector2 origin = transform.position;
        Vector2 dir = moveInput.normalized;


        // 수평 또는 수직으로 정제
        Vector2 fixedDir = GetCardinalDirection(dir);

        float distance = 0.6f;

        RaycastHit2D hit = Physics2D.Raycast(origin, fixedDir, distance, LayerMask.GetMask("Box"));
        if (hit.collider != null)
        {
            GameObject hitBox = hit.collider.gameObject;
            BoxPush box = hitBox.GetComponent<BoxPush>();
            if (box != null)
            {
                box.TryPush(fixedDir);
            }
        }
    }
    private Vector2 GetCardinalDirection(Vector2 input)//대각 방향을 수평/수직으로 정제하는 함수
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0 ? Vector2.right : Vector2.left;
        else
            return input.y > 0 ? Vector2.up : Vector2.down;
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

            //Debug.DrawRay(origin, direction * interactRange, Color.red, 0.5f); // 시각화

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
                        //Debug.Log("⚠️ 충돌했지만 IInteractable 없음: " + target.name);
                    }
                }
            }
            else
            {
                //Debug.Log("❌ 상호작용 대상 없음");
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
            AudioManager.Instance.PlaySFX("PlayerWalk");
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

    private void CheckPushHoldAndTry()
    {
        if (isPushingInputHeld)
        {
            if (!isTryingToPush)
            {
                isTryingToPush = true;
                pushTimer = 0f;
            }

            pushTimer += Time.deltaTime;

            if (pushTimer >= boxPushHoldTime)
            {
                TryAutoPushBox();
                isTryingToPush = false;
            }
        }
        else
        {
            isTryingToPush = false;
            pushTimer = 0f;
        }
    }
  
}
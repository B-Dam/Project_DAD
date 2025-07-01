using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public float moveSpeed = 5f;
    public Transform playerTransform;
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

    [Header("박스 밀기 쿨타임")]
     public float boxPushCooldown = 0.3f; // 밀기 쿨타임
    [HideInInspector] public float lastPushTime = -10f;//박스 밀 때 쿨타임용


    public Vector2 lastMoveInput { get; private set; }


    [Header("막힘 알림")]
    public GameObject blockIndicatorPrefab;
    public LayerMask obstacleLayer;
    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerTransform = transform;
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

        // 대화 중이면 이동/상호작용 입력 차단
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            return;
        }

        HandleMovementInput();
        UpdateAnimation(moveInput);
        HandleInteractionInput();

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

    private void OnDrawGizmos()
    {
     
        // 2. 콜라이더 크기 그리기 (BoxCollider2D 기준)
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
    private void FixedUpdate()
    {
        MovePlayer();

        // 앞에 장애물 감지 + UI 표시
        TryShowObstacleIndicator();
    }
    private void TryShowObstacleIndicator()//P
    {

        if (Time.time - lastPushTime < boxPushCooldown) return; // 쿨타임 내엔 실행 X

        // 조건 1: 방향 입력 중일 때만
        if (moveInput == Vector2.zero) return;

        // 너무 짧은 입력 무시
        if (lastMoveInput == Vector2.zero) return;

        // 박스를 밀고 있다면 BoxPush에서 처리하게 둔다
        Vector2 origin = transform.position;
        Vector2 dir = lastMoveInput.normalized;
        float distance = 0.7f;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, obstacleLayer);
        if (hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;

            // 박스를 만났다면 BoxPush가 판단
            if (hitObj.CompareTag("Box") && hitObj.GetComponent<BoxPush>() != null) return;

            // 장애물이거나, BoxPush 없는 오브젝트일 경우 → UI 띄움
            ShowBlockIndicator(transform.position + new Vector3(0f, 1.2f, 0f));
            lastPushTime = Time.time;
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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
            lastMoveInput = moveInput;
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
            lastMoveDirection = direction;

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

                if (target.CompareTag("NPC") || target.CompareTag("Item") || target.CompareTag("Interact"))
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

}

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

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
        }
    }

    private void MovePlayer()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;

        bool isMoving = direction.magnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
            lastMoveDirection = direction;

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

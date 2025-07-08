using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NPCMovementController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastMoveDirection = Vector2.left; // ✅ 기본 왼쪽으로 설정

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimation(rb.linearVelocity); // ✅ 실제 움직임 기반으로 애니메이션 갱신
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection != Vector2.zero)
            lastMoveDirection = moveDirection;
    }

    public void Stop()
    {
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    private void UpdateAnimation(Vector2 velocity)
    {
        float speed = velocity.magnitude;
        animator.SetFloat("Run", speed);

        if (speed > 0.01f)
            lastMoveDirection = velocity.normalized;

        // 기본 방향이 왼쪽인 경우: 오른쪽 갈 때 1, 왼쪽 갈 때 -1
        if (lastMoveDirection.x < 0)
            transform.localScale = new Vector3(1, 1, 1); // 왼쪽
        else if (lastMoveDirection.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);  // 오른쪽
    }
}

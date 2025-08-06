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
    }

    public void Destroy()
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

        // 현재 스케일 가져오기
        Vector3 scale = transform.localScale;

        // 왼쪽(기본) / 오른쪽 방향 판정
        if (lastMoveDirection.x < 0)
            scale.x = Mathf.Abs(scale.x);  // 왼쪽이면 양수
        else if (lastMoveDirection.x > 0)
            scale.x = -Mathf.Abs(scale.x); // 오른쪽이면 음수

        transform.localScale = scale; // Y/Z 값은 그대로 유지
    }

}

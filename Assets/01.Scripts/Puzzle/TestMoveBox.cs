using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TestMoveBox : MonoBehaviour
{
    public float pushForce = 3f;
    public float maxSpeed = 1.5f;

    private Rigidbody2D rb;
    private bool isPlayerTouching = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // 조건: 플레이어가 붙어 있고, 입력이 있을 때만 힘 가함
        if (isPlayerTouching)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h != 0 || v != 0)
            {
                Vector2 pushDir = new Vector2(h, v).normalized;
                rb.AddForce(pushDir * pushForce, ForceMode2D.Force);
            }
        }

        // 항상 속도 제한
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerTouching = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerTouching = false; 
            // 박스 속도 강제 정지!
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}

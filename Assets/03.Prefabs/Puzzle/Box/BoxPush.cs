using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoxPush : MonoBehaviour
{
    public float moveDistance = 1f;//한 블럭 거리
    public float moveSpeed = 5f;
    public LayerMask obstacleLayer; // 장애물 레이어

    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector2 targetPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;//직접 위치 이동 할 거니까
    }
    private void Update()
    {
        if (isMoving)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));

            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                rb.position = targetPosition;//위치를 즉시 바꿔 버림
                isMoving = false;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMoving) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        //충돌 지점 평균 구하기
        Vector2 contactPoint = Vector2.zero;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            contactPoint += contact.point;
        }
        contactPoint /= collision.contactCount;

        Vector2 boxPos = rb.position;
        //충돌 지점과 박스의 중심 위치 차이 계산
        Vector2 localoffset = contactPoint - boxPos;

        Vector2 pushDirection = Vector2.zero;
        float threshold = 0.2f; //얼마나 중심에 가까워야 인정할지

        if (Mathf.Abs(localoffset.x) > Mathf.Abs(localoffset.y))
        {
            //좌우면 y값이 작아야 중심 근처
            if (Mathf.Abs(localoffset.y) < threshold)
            {
                pushDirection = new Vector2(-Mathf.Sign(localoffset.x), 0f);
            }
        }
        else
        {
            //상하면 x값이 작아야 중심 근처
            if (Mathf.Abs(localoffset.x) < threshold)
            {
                pushDirection = new Vector2(0f, -Mathf.Sign(localoffset.y));
            }
        }
        if (pushDirection == Vector2.zero) return;//중앙이 아닌 구석 밀면 무시

        Vector2 nextPos = rb.position + pushDirection * moveDistance;

        //장애물 감지
        RaycastHit2D hit = Physics2D.Raycast(rb.position, pushDirection, moveDistance, obstacleLayer);
        if ((hit.collider != null))
        {
            return; //장애물 있으면 이동 안함    
        }
        //이동시작
        targetPosition = nextPos;
        isMoving = true;
    }
}
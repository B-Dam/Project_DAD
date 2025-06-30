using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoxPush : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveDistance = 1f; // 박스가 한 번에 이동할 거리 (한 칸)
    public float moveSpeed = 5f;// 이동 속도

    [Header("충돌 감지 레이어")]
    public LayerMask obstacleLayer; // 장애물 레이어
    public LayerMask boxLayer;

    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector2 targetPosition;

    [Header("막힘 알림 설정")]
    public GameObject blockIndicatorPrefab; // 못 미는 경우 뜨는 UI 아이콘 프리팹
    private Canvas canvas;
    private GameObject currentIndicator;// 현재 표시 중인 아이콘
    private float indicatorLifetime = 0.2f;// 아이콘 유지 시간
    //private float indicatorTimer = 0f;

    private void Start()
    {
        // Rigidbody2D 설정: 직접 위치 이동할 것이므로 Kinematic으로 설정
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;//직접 위치 이동 할 거니까

    }
    private void Update()
    {
        // 이동 중이면 목표 위치까지 부드럽게 이동
        if (isMoving)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));

            // 목표 위치에 도달하면 이동 종료
            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                rb.position = targetPosition;
                isMoving = false;
            }
        }
    }

    // 플레이어가 박스를 밀려고 접촉 중일 때 호출됨
    void OnCollisionStay2D(Collision2D collision)
    {
        // 이미 이동 중이거나 플레이어가 아니라면 무시
        if (isMoving || !collision.gameObject.CompareTag("Player")) return;

        // 어느 방향으로 밀었는지 판단
        Vector2 pushDirection = GetPushDirection(collision);
        if (pushDirection == Vector2.zero) return;

        // 다음 위치 계산
        Vector2 nextPos = rb.position + pushDirection * moveDistance;

        // 이동 방향에 장애물이나 다른 박스가 있다면 알림 표시 후 중단
        if (IsBlocked(nextPos, pushDirection))
        {
            // 이동 수행
            ShowBlockIndicator();
            return;
        }

        targetPosition = nextPos;
        isMoving = true;
    }

    // 충돌 지점을 기반으로 플레이어가 어느 방향으로 밀었는지 계산
    Vector2 GetPushDirection(Collision2D collision)
    {
        // 접촉 지점 평균 계산
        Vector2 contactPoint = Vector2.zero;
        foreach (var contact in collision.contacts)
            contactPoint += contact.point;
        contactPoint /= collision.contactCount;

        Vector2 offset = contactPoint - rb.position;
        float threshold = 0.2f; // 사선으로 민 경우를 무시하기 위한 임계값

        // 밀 방향 판단 (좌우 or 상하)
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (Mathf.Abs(offset.y) < threshold)
                return new Vector2(-Mathf.Sign(offset.x), 0f);
        }
        else
        {
            if (Mathf.Abs(offset.x) < threshold)
                return new Vector2(0f, -Mathf.Sign(offset.y));
        }

        return Vector2.zero;// 유효한 방향이 없으면 무시
    }

    //앞이 막혀 있는지 확인
    bool IsBlocked(Vector2 nextPos, Vector2 direction)
    {
        // 앞에 장애물이 있으면 true 반환
        if (Physics2D.Raycast(rb.position, direction, moveDistance, obstacleLayer))
            return true;

        // 다른 박스가 있으면 true 반환
        Collider2D overlap = Physics2D.OverlapPoint(nextPos, boxLayer);
        return overlap != null && overlap.gameObject != gameObject;
    }

    //실패 알림 아이콘 생성
    void ShowBlockIndicator()
    {
        // Canvas가 없으면 찾아오기 (이름이 "UICanvas"인 객체에서 가져옴)
        if (canvas == null)
        {
            //canvas = FindFirstObjectByType<Canvas>();
            canvas = GameObject.Find("UICanvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[BoxPush] Canvas를 찾을 수 없습니다!");
            }
        }

        // 프리팹 또는 캔버스가 없거나, 이미 표시 중이면 중단
        if (blockIndicatorPrefab == null || canvas == null || currentIndicator != null) return;
        Debug.Log($"[BlockUI] Camera.main = {(Camera.main != null ? Camera.main.name : "null")}");

        // 플레이어 객체 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;



        // 카메라 찾기 (UI 위치 계산용)
        Camera cam = Camera.main;
        //canvas.worldCamera ?? Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[BlockUI] 카메라 없음");
            return;
        }

        //플레이어 위치를 기반으로 "UI를 띄울 월드 좌표"를 구함
        Vector3 worldPos = GetIndicatorWorldPosition(player.transform.position);

        //위에서 구한 월드 위치를 카메라 기준의 스크린 좌표로 변환
        Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
        Debug.Log($"[BlockUI] screenPos = {screenPos}");
        //blockIndicatorPrefab을 Canvas 안에 복제해서 생성
        currentIndicator = Instantiate(blockIndicatorPrefab, canvas.transform);

        //생성된 UI 이미지의 스크린 상 위치를 설정
        currentIndicator.GetComponent<RectTransform>().position = screenPos;

        //일정 시간 후(indicatorLifetime, 기본값은 0.2초)에
        //UI 이미지를 자동으로 제거해서 화면에서 사라지게 함
        Destroy(currentIndicator, indicatorLifetime);
    }

    //아이콘 위치 계산 (플레이어 머리 위)
    Vector3 GetIndicatorWorldPosition(Vector3 playerPos)
    {
        //민 방향으로 뜨지만 바로 방향 바꿔도 뜬 곳에 그대로 있어서 보류
        //Vector2 dir = (rb.position - (Vector2)playerPos).normalized;
        //Vector3 offset = Vector3.up * 0.8f;

        //if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        //    offset += Vector3.right * Mathf.Sign(dir.x) * 1f;
        //else
        //    offset += Vector3.up * 0.5f;

        //return playerPos + offset;

        return playerPos + new Vector3(0f, 1.2f, 0f);//단순 머리위 고정
    }
}
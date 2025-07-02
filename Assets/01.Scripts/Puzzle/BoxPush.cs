using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Rigidbody2D))]
public class BoxPush : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveDistance = 1f; // 박스가 한 번에 이동할 거리 (한 칸)
    public float moveSpeed = 5f;// 이동 속도

    [Header("충돌 감지 레이어")]
    public LayerMask boxLayer;
    public LayerMask obstacleLayer; // 장애물 레이어

    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector2 targetPosition;

    [Header("막힘 알림 설정")]
    public GameObject blockIndicatorPrefab; // 못 미는 경우 뜨는 UI 아이콘 프리팹
    private Canvas canvas;
    private GameObject currentIndicator;// 현재 표시 중인 아이콘
    private float indicatorLifetime = 0.2f;// 아이콘 유지 시간

    //stay 조금 지연 시키기
    private float collisionProcessCooldown = 0.3f; // 최소 간격 0.3초
    private float lastCollisionProcessTime = -999f; // 초기값은 아주 과거 시간

    public float directionDominanceRatioX = 2.5f;
    public float directionDominanceRatioY = 2.5f;


    private void Start()
    {
        // Rigidbody2D 설정: 직접 위치 이동할 것이므로 Kinematic으로 설정
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;//직접 위치 이동 할 거니까
    }
    private void Update()
    {
        if (isMoving)
        {
            Vector2 moveDir = (targetPosition - rb.position).normalized;

            if (moveDir == Vector2.zero || IsBlockedWhileMoving(moveDir))
            {
                //ShowBlockIndicator();
                isMoving = false;
                return;
            }

            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));

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
        //  0.3초마다 한 번만 처리
        if (Time.time - lastCollisionProcessTime < collisionProcessCooldown) return;
        lastCollisionProcessTime = Time.time;

        // 이미 이동 중이거나 플레이어가 아니라면 무시
        if (isMoving || !collision.gameObject.CompareTag("Player")) return;

        // 쿨타임 중이면 무시
        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        if (pc == null) return;
        if (Time.time - pc.lastPushTime < pc.boxPushCooldown) return;

        // 어느 방향으로 밀었는지 판단
        Vector2 pushDirection = GetPushDirection(collision);

        if (pushDirection == Vector2.zero || IsBlockedForStart(pushDirection))
        {
            ShowBlockIndicator();
            pc.lastPushTime = Time.time;
            return;
        }
        // 다음 위치 계산
        Vector2 nextPos = rb.position + pushDirection * moveDistance;

        targetPosition = nextPos;
        isMoving = true;
        pc.lastPushTime = Time.time; // 성공해도 쿨타임 적용

    }

    // 충돌 지점을 기반으로 플레이어가 어느 방향으로 밀었는지 계산
    Vector2 GetPushDirection(Collision2D collision)
    {
        GameObject player = collision.gameObject;
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) return Vector2.zero;

        Vector2 input = pc.lastMoveInput;

        // 대각선이면 무시
        if (Mathf.Abs(input.x) > 0.1f && Mathf.Abs(input.y) > 0.1f)
            return Vector2.zero;


        // 기준을 콜라이더 중심으로!
        Vector2 boxCenter = GetComponent<Collider2D>().bounds.center;
        Vector2 playerCenter = collision.collider.bounds.center;
        Vector2 delta = boxCenter - playerCenter;

        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);
        //Debug.Log($"[BoxPush] delta = {delta}, absX = {absX}, absY = {absY}, input = {input}");
        // 좌우 방향이 더 뚜렷한 경우만 좌우 입력 허용
        if (absX > absY * directionDominanceRatioX && absX > 0.1f)
        {
            if (delta.x > 0 && input.x > 0.1f) return Vector2.right;
            if (delta.x < 0 && input.x < -0.1f) return Vector2.left;
        }

        // 상하 방향이 더 뚜렷한 경우만 상하 입력 허용
        if (absY > absX * directionDominanceRatioY && absY > 0.1f)
        {
            if (delta.y > 0 && input.y > 0.1f) return Vector2.up;
            if (delta.y < 0 && input.y < -0.1f) return Vector2.down;
        }

        return Vector2.zero;
    }

    bool IsBlockedForStart(Vector2 direction) => IsBlocked(direction, true);
    bool IsBlockedWhileMoving(Vector2 direction) => IsBlocked(direction, false);
    bool IsBlocked(Vector2 direction, bool showDebug)//showDebug는 디버그용
    {
        if (direction == Vector2.zero) return true;

        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        Vector2 boxSize = boxCol.bounds.size;
        Vector2 boxCenter = boxCol.bounds.center;

        float thickness = 0.01f;
        Vector2 castSize;

        if (Mathf.Abs(direction.x) > 0.1f)
            castSize = new Vector2(1f, Mathf.Max(0.05f, boxSize.y - thickness));
        else
            castSize = new Vector2(Mathf.Max(0.05f, boxSize.x - thickness), 1f - 0.95f);

        Vector2 castCenter = boxCenter + direction * (1f * 0.5f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(castCenter, castSize, 0f, obstacleLayer | boxLayer);
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject)
            {
                int hitLayer = hit.gameObject.layer;

                bool isUnmovable =
                  hitLayer == LayerMask.NameToLayer("Obstacle") ||
                  hitLayer == LayerMask.NameToLayer("PlayerBlocker")||
                   hit.CompareTag("Box"); ;

                if (showDebug)
                    Debug.Log($"[BlockCheck] 감지됨: {hit.name}, 레이어: {LayerMask.LayerToName(hitLayer)}");

                // 이미지 띄우는 건 못 미는 경우만
                if (isUnmovable && showDebug)
                {
                    //ShowBlockIndicator();
                }


                return true;
            }
        }

        return false;
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
                return;
            }
        }

        // 프리팹 또는 캔버스가 없거나, 이미 표시 중이면 중단
        if (blockIndicatorPrefab == null || canvas == null || currentIndicator != null) return;

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
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Handles.color = Color.blue;
            Handles.matrix = transform.localToWorldMatrix;

            Vector2 half = col.size * 0.5f;
            Vector2 offset1 = col.offset;

            Vector3[] corners = new Vector3[]
            {
            offset1 + new Vector2(-half.x, -half.y),
            offset1 + new Vector2(-half.x,  half.y),
            offset1 + new Vector2( half.x,  half.y),
            offset1 + new Vector2( half.x, -half.y),
            offset1 + new Vector2(-half.x, -half.y)
            };
            Handles.DrawAAPolyLine(10f, corners);

        }
#endif
    }
}
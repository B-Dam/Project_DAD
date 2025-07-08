using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Rigidbody2D))]
public class BoxPush : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveDistance = 1f; // 박스가 한 번에 이동할 거리 (한 칸)
    public float moveSpeed = 5f;// 이동 속도

    //충돌 감지 레이어 (장애물, 다른 박스)
    [Header("충돌 감지 레이어")]
    public LayerMask boxLayer;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private bool isMoving = false; // 이동 중 여부
    private Vector2 targetPosition;  // 이동 목표 위치

    // 이동 불가 시 표시할 인디케이터
    [Header("막힘 알림 설정")]
    public GameObject blockIndicatorPrefab; 
    private Canvas canvas;
    private GameObject currentIndicator;// 현재 표시 중인 아이콘
    private float indicatorLifetime = 0.2f;// 아이콘 유지 시간

    // OnCollisionStay2D가 너무 자주 호출되는 걸 막음
    private float collisionProcessCooldown = 0.3f; // 최소 간격 0.3초
    private float lastCollisionProcessTime = -999f; // 초기값은 아주 과거 시간

    //대각선 오립력을 막기 위함 X차이(absX)가 Y차이(absY)의 2.5배 이상일 때만 → 좌우 방향 입력을 유효하다고 인정
    private float directionDominanceRatioX = 2.7f;
    private float directionDominanceRatioY = 1.4f;

    private void Start()
    {
        // Rigidbody2D 설정: 직접 위치 이동할 것이므로 Kinematic으로 설정
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;//직접 위치 이동 할 거니까

        // 콜라이더 크기 고정
        BoxCollider2D col = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if (isMoving)
        {
            //이동 중일 때 목표 위치까지 보간 이동
            Vector2 moveDir = (targetPosition - rb.position).normalized;

            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));

            // 목표 도착 판정
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

        //이동 불가: 방향 없음 or 막힘
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

    //감지 시작 시 막힘 체크
    bool IsBlockedForStart(Vector2 direction) => IsBlocked(direction, true);
    //이동 중 막힘 체크 (현재는 미사용)
    bool IsBlockedWhileMoving(Vector2 direction) => IsBlocked(direction, false);
    //주어진 방향에 OverlapBox로 충돌 체크
    bool IsBlocked(Vector2 direction, bool showDebug)//showDebug는 디버그용
    {
        if (direction == Vector2.zero) return true;

        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        Vector2 boxSize = boxCol.bounds.size;
        Vector2 boxCenter = boxCol.bounds.center;

        float thickness = 0.01f;
        Vector2 castSize;
        float sizeX = boxCol.bounds.size.x;
        float sizeY = boxCol.bounds.size.y;
        if (Mathf.Abs(direction.x) > 0.1f)
            castSize = new Vector2(sizeX, sizeY); // 가로 감지
        else
            castSize = new Vector2(sizeX, sizeY+1f); // 세로 감지

        Vector2 castCenter = boxCenter + direction * (1f * 0.5f);

        if (showDebug)
        {
            Debug.Log($"[BoxPush][Cast Info]");
            Debug.Log($"- boxSize = {boxSize}");
            Debug.Log($"- direction = {direction}");
            Debug.Log($"- castSize = {castSize}");
            Debug.Log($"- castCenter = {castCenter}");
        }
        //충돌 체크
        Collider2D[] hits = Physics2D.OverlapBoxAll(castCenter, castSize, 0f, obstacleLayer | boxLayer);//0f는 회전값
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject)
            {
                int hitLayer = hit.gameObject.layer;

                bool isUnmovable =
                  hitLayer == LayerMask.NameToLayer("Obstacle") ||
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
        return playerPos + new Vector3(0f, 1.2f, 0f);//단순 머리위 고정
    }
    //    private void OnDrawGizmos()
    //    {
    //#if UNITY_EDITOR
    //        BoxCollider2D col = GetComponent<BoxCollider2D>();
    //        if (col != null)
    //        {
    //            Handles.color = Color.blue;
    //            Handles.matrix = transform.localToWorldMatrix;

    //            Vector2 half = col.size * 0.5f;
    //            Vector2 offset1 = col.offset;

    //            Vector3[] corners = new Vector3[]
    //            {
    //            offset1 + new Vector2(-half.x, -half.y),
    //            offset1 + new Vector2(-half.x,  half.y),
    //            offset1 + new Vector2( half.x,  half.y),
    //            offset1 + new Vector2( half.x, -half.y),
    //            offset1 + new Vector2(-half.x, -half.y)
    //            };
    //            Handles.DrawAAPolyLine(10f, corners);

    //        }
    //#endif
    //    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        //float a = 2f / 1.2f * 100f;
        //float b = 1f / 1.2f * 100f;
        //col.size = new Vector2((Mathf.Floor(a)/100)-0.01f, (Mathf.Floor(b)/100)-0.03f);
        if (col != null)
        {
            // 🔵 기존 콜라이더 파란색 라인
            Handles.color = Color.blue;
            //Handles.matrix = transform.localToWorldMatrix;
            Handles.matrix = Matrix4x4.identity;
            Vector2 half = col.bounds.size * 0.5f;
            //Vector2 offset1 = col.offset;
            Vector2 offset1 = col.bounds.center;

            Vector3[] corners = new Vector3[]
            {
            offset1 + new Vector2(-half.x, -half.y),
            offset1 + new Vector2(-half.x,  half.y),
            offset1 + new Vector2( half.x,  half.y),
            offset1 + new Vector2( half.x, -half.y),
            offset1 + new Vector2(-half.x, -half.y)
            };
            Handles.DrawAAPolyLine(10f, corners);

            // 🟡 박스올 감지 영역 (4방향)
            Vector2 boxSize = col.bounds.size;
            Vector2 boxCenter = col.bounds.center;

            Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
            foreach (var dir in directions)
            {
                Vector2 castSize = (Mathf.Abs(dir.x) > 0.1f)
                    ? new Vector2(col.bounds.size.x, col.bounds.size.y)
                    : new Vector2(col.bounds.size.x, col.bounds.size.y+1f);
                //: new Vector2(2f, 1f);

                //Vector2 castCenter = boxCenter + dir* (1f * 1f);
                //Debug.Log($"sss{dir}");
                Vector2 castCenter = boxCenter + dir * ((Mathf.Abs(dir.x) > 0.1f) ? col.bounds.size.x/ 2 : col.bounds.size.y );

                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(castCenter, castSize);


            }
        }
#endif
    }


}
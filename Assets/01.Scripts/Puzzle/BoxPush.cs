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

    //// 이동 불가 시 표시할 인디케이터
    //[Header("막힘 알림 설정")]
    //public GameObject blockIndicatorPrefab;
    //private Canvas canvas;
    //private GameObject currentIndicator;// 현재 표시 중인 아이콘
    //private float indicatorLifetime = 0.2f;// 아이콘 유지 시간



    //[Header("일정시간 밀어야 밀림")]
    //public float requiredHoldTime = 0.5f; // 0.5초 이상 유지 시 허용

    //private Coroutine pushCoroutine; // 현재 실행 중인 코루틴

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


    public void TryPush(Vector2 direction)
    {
        if (isMoving) return;

        if (IsBlocked(direction))
        {
            // 막힘 알림은 BlockIndicatorManager를 통해 출력
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 indicatorPos = player.transform.position + new Vector3(0f, 1.2f, 0f);
                BlockIndicatorManager.Instance?.ShowIndicator(indicatorPos);
            }
            return;
        }

        targetPosition = rb.position + direction.normalized * moveDistance;
        isMoving = true;
        AudioManager.Instance?.PlaySFX("Puzzle/Puzzle_Box_drrr");
    }

   
    //주어진 방향에 OverlapBox로 충돌 체크
    bool IsBlocked(Vector2 direction)
    {
      BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        Vector2 boxCenter = boxCol.bounds.center;
        Vector2 castSize = (Mathf.Abs(direction.x) > 0.1f)
            ? new Vector2(boxCol.bounds.size.x, boxCol.bounds.size.y)
            : new Vector2(boxCol.bounds.size.x, boxCol.bounds.size.y + 0.6f);

        Vector2 castCenter = boxCenter + direction * 0.5f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(castCenter, castSize, 0f, boxLayer | obstacleLayer);

        foreach(var hit in hits)
        {
            if (hit !=null && hit.gameObject != gameObject) // 자기 자신은 제외
            {
                if(hit.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||hit.CompareTag("Box"))
                {
                    AudioManager.Instance.PlaySFX("Puzzle_Beep sound effect");
                    //Debug.Log($"충돌 감지: {hit.gameObject.name}");
                    return true; // 충돌이 있으면 막힘
                }
            }
        }
        return false; 
    }


    ////실패 알림 아이콘 생성
    //void ShowBlockIndicator()
    //{
    //    // Canvas가 없으면 찾아오기 (이름이 "UICanvas"인 객체에서 가져옴)
    //    if (canvas == null)
    //    {
    //        //canvas = FindFirstObjectByType<Canvas>();
    //        canvas = GameObject.Find("UICanvas")?.GetComponent<Canvas>();
    //        if (canvas == null)
    //        {
    //            Debug.LogError("[BoxPush] Canvas를 찾을 수 없습니다!");
    //            return;
    //        }
    //    }

    //    // 프리팹 또는 캔버스가 없거나, 이미 표시 중이면 중단
    //    if (blockIndicatorPrefab == null || canvas == null || currentIndicator != null) return;

    //    // 플레이어 객체 찾기
    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    if (player == null) return;



    //    // 카메라 찾기 (UI 위치 계산용)
    //    Camera cam = Camera.main;
    //    //canvas.worldCamera ?? Camera.main;
    //    if (cam == null)
    //    {
    //        Debug.LogWarning("[BlockUI] 카메라 없음");
    //        return;
    //    }

    //    //플레이어 위치를 기반으로 "UI를 띄울 월드 좌표"를 구함
    //    Vector3 worldPos = GetIndicatorWorldPosition(player.transform.position);

    //    //위에서 구한 월드 위치를 카메라 기준의 스크린 좌표로 변환
    //    Vector2 screenPos = cam.WorldToScreenPoint(worldPos);

    //    //blockIndicatorPrefab을 Canvas 안에 복제해서 생성
    //    currentIndicator = Instantiate(blockIndicatorPrefab, canvas.transform);

    //    //생성된 UI 이미지의 스크린 상 위치를 설정
    //    currentIndicator.GetComponent<RectTransform>().position = screenPos;

    //    //일정 시간 후(indicatorLifetime, 기본값은 0.2초)에
    //    //UI 이미지를 자동으로 제거해서 화면에서 사라지게 함
    //    Destroy(currentIndicator, indicatorLifetime);
    //}

    ////아이콘 위치 계산 (플레이어 머리 위)
    //Vector3 GetIndicatorWorldPosition(Vector3 playerPos)
    //{
    //    return playerPos + new Vector3(0f, 1.2f, 0f);//단순 머리위 고정
    //}
    public void ForceStop()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(rb.position); // 혹시 모를 보간 중단
    }
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
                    : new Vector2(col.bounds.size.x, col.bounds.size.y + 0.6f);
                //: new Vector2(2f, 1f);

                //Vector2 castCenter = boxCenter + dir* (1f * 1f);
                //Debug.Log($"sss{dir}");
                Vector2 castCenter = boxCenter + dir * ((Mathf.Abs(dir.x) > 0.1f) ? col.bounds.size.x / 2 : col.bounds.size.y);

                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(castCenter, castSize);


            }
        }
#endif
    }


}
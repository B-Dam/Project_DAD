using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class HintMode : MonoBehaviour
{
    [Header("설정값")]
    public float hintDuration = 10f;           // 힌트 지속 시간
    public float hintCooldown = 30f;           // 힌트 쿨다운 시간
    public int maxHintCount = 3;               // 맵별 최대 힌트 사용 가능 횟수

    [Header("오브젝트 연결")]
    public Transform player;                   // 플레이어 위치
    public LayerMask targetLayer;              // 힌트 타겟이 될 수 있는 레이어
    public LayerMask answerLayer;              // 정답 오브젝트가 속한 레이어

    [Header("시각 효과")]
    public Volume grayscaleVolume;             // 후처리 볼륨
    private ColorAdjustments colorAdjustments; // 그레이스케일 효과 제어

    [Header("파티클 연결 프리팹")]
    public HintLineConnector playerToTargetLinePrefab; // 플레이어 → 타겟 연결용
    public HintLineConnector targetToAnswerLinePrefab; // 타겟 → 정답 연결용

    private HintLineConnector playerToTargetLine;
    private HintLineConnector targetToAnswerLine;

    [Header("UI 관련")]
    public Button hintButton;                      // 힌트 사용 버튼
    public TextMeshProUGUI durationText;           // 힌트 지속시간 텍스트
    public TextMeshProUGUI cooldownText;           // 쿨다운 시간 텍스트
    public Image[] hintIcons;                      // 남은 힌트 수 아이콘
    public Image durationBar;                      // 힌트 지속시간 바
    public Image cooldownBar;                      // 쿨다운 진행 바

    private Dictionary<string, int> hintCounts = new(); // 맵별 힌트 남은 횟수 저장

    private float durationTimer = 0f;
    private float cooldownTimer = 0f;

    private bool isHintActive = false;
    private bool isCooldown = false;

    private string currentMapID => MapManager.Instance.currentMapID;

    void Start()
    {
        // 후처리 볼륨에서 색상 조절 효과 가져오기
        if (grayscaleVolume != null)
            grayscaleVolume.profile.TryGet(out colorAdjustments);

        // 프리팹 복사해서 파티클 객체 생성
        playerToTargetLine = Instantiate(playerToTargetLinePrefab);
        targetToAnswerLine = Instantiate(targetToAnswerLinePrefab);

        // 시작 시 파티클은 비활성화
        playerToTargetLine.SetActiveParticle(false);
        targetToAnswerLine.SetActiveParticle(false);

        // 모든 퍼즐 맵에 대해 힌트 횟수 초기화
        foreach (string mapID in PuzzleManager.Instance.GetAllPuzzleMapIDs())
            hintCounts[mapID] = maxHintCount;

        // 버튼 클릭 이벤트 연결
        if (hintButton != null)
            hintButton.onClick.AddListener(UseHint);

        UpdateUI();
    }

    void Update()
    {
        // 퍼즐 맵이 아닐 경우 힌트 무효 처리
        if (!PuzzleManager.Instance.IsPuzzleMap(currentMapID))
        {
            DeactivateHint();
            return;
        }
        // E 키로 힌트 사용
        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseHint();
        }
        // 힌트 지속시간 타이머
        if (isHintActive)
        {
            durationTimer += Time.deltaTime;
            if (durationTimer >= hintDuration)
                DeactivateHint();
        }

        // 쿨다운 타이머
        if (isCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= hintCooldown)
                isCooldown = false;
        }

        UpdateUI();
    }

    /// <summary>
    /// 힌트 사용 버튼 클릭 시 호출
    /// </summary>
    void UseHint()
    {
        if (!hintCounts.ContainsKey(currentMapID) || hintCounts[currentMapID] <= 0)
        {
            Debug.Log("힌트를 모두 사용했습니다.");
            return;
        }

        if (isHintActive || isCooldown)
        {
            Debug.Log("힌트 사용 불가: 쿨타임 중이거나 이미 사용 중");
            return;
        }

        // 가장 가까운 타겟 찾기
        Transform closest = FindClosestTarget();
        // 타겟 기준 정답 오브젝트 찾기
        Transform answer = FindAnswerForTarget(closest);

        if (closest == null || answer == null)
        {
            Debug.Log("힌트 대상 또는 정답이 없음");
            return;
        }
        // 찾지 못했거나 이미 정답과 붙어 있으면 실패
        float touchingThreshold = 0.2f; // 실제로 "닿았다고 판단할 거리"
        float realDistance = Vector2.Distance(closest.position, answer.position);
        if (closest == null || answer == null || realDistance < touchingThreshold)
        {
            Debug.Log($"정답과 너무 가까워서 힌트 생략됨. 거리: {realDistance:F2}");
            return;
        }

        // 연결 설정
        playerToTargetLine.targetA = player;
        playerToTargetLine.targetB = closest;
        targetToAnswerLine.targetA = closest;
        targetToAnswerLine.targetB = answer;

        // 파티클 보이게
        playerToTargetLine.SetActiveParticle(true);
        targetToAnswerLine.SetActiveParticle(true);

        // 화면 흑백 효과
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = -100f;

        // 상태 갱신
        isHintActive = true;
        isCooldown = true;
        durationTimer = 0f;
        cooldownTimer = 0f;

        hintCounts[currentMapID]--; // 사용 횟수 차감
        Debug.Log($"힌트 사용됨! [{currentMapID}] 남은 횟수: {hintCounts[currentMapID]}");
    }

    /// <summary>
    /// 힌트 지속시간 종료 시 처리
    /// </summary>
    void DeactivateHint()
    {
        isHintActive = false;

        // 화면 원래대로
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = 0f;

        // 파티클 숨기기
        playerToTargetLine.SetActiveParticle(false);
        targetToAnswerLine.SetActiveParticle(false);
    }

    /// <summary>
    /// 플레이어 기준 가장 가까운 힌트 대상 찾기
    /// </summary>
    Transform FindClosestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, 200f, targetLayer);
        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(player.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// 대상 기준 가장 가까운 정답 오브젝트 찾기
    /// </summary>
    Transform FindAnswerForTarget(Transform target)
    {
        if (target == null) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(target.position, 200f, answerLayer);

        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.transform == target) continue;

            float distance = Vector2.Distance(target.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = hit.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// 두 오브젝트의 콜라이더가 맞닿아 있는지 검사
    /// </summary>
    bool IsTouching(Transform a, Transform b)
    {
        Collider2D colA = a.GetComponent<Collider2D>();
        Collider2D colB = b.GetComponent<Collider2D>();
        if (colA && colB)
            return colA.IsTouching(colB);
        return false;
    }

    /// <summary>
    /// UI 업데이트 (바, 텍스트, 아이콘)
    /// </summary>
    void UpdateUI()
    {
        if (durationBar != null)
            durationBar.fillAmount = isHintActive ? Mathf.Clamp01((hintDuration - durationTimer) / hintDuration) : 0f;

        if (cooldownBar != null)
            cooldownBar.fillAmount = isCooldown ? Mathf.Clamp01(cooldownTimer / hintCooldown) : 1f;

        if (durationText != null)
            durationText.text = isHintActive ? $"Dur: {hintDuration - durationTimer:F1}" : "--";

        if (cooldownText != null)
            cooldownText.text = isCooldown ? $"CD: {hintCooldown - cooldownTimer:F1}" : "Hint";

        int count = hintCounts.ContainsKey(currentMapID) ? hintCounts[currentMapID] : 0;
        for (int i = 0; i < hintIcons.Length; i++)
        {
            hintIcons[i].gameObject.SetActive(i < count);
        }
    }
    public void ForceResetHintState()
    {
        isHintActive = false;
        isCooldown = false;
        durationTimer = 0f;
        cooldownTimer = 0f;

        if (colorAdjustments != null)
            colorAdjustments.saturation.value = 0f;

        playerToTargetLine.SetActiveParticle(false);
        targetToAnswerLine.SetActiveParticle(false);
    }
}
//    private bool isGrayscale = false;

//    private const int maxHintCount = 3;// 힌트 최대 사용 가능 횟수 (변경되지 않으므로 const로 선언)

//    // 맵 ID별 힌트 남은 횟수 저장 (각 맵마다 별도 관리)
//    private Dictionary<string, int> hintCountsByMap = new Dictionary<string, int>();

//    private float hintDuration = 10f; // 힌트 지속 시간 
//    private float hintDurationTimer = 0f;//지속시간 체크

//    private float hintCooldown = 30f; // 힌트 재사용 대기 시간
//    private float hintCooldownTimer = 0f; // 힌트 쿨타임 경과 시간

//    private bool isInCooldown = false;

//    public TextMeshProUGUI hintDurationTxt;
//    public TextMeshProUGUI hintCooldownTxt;

//    public Image[] hintIcons; // Inspector에서 3개 등록

//    public Image hintDurationTimeBar;
//    public Image hintCooldownTimeBar;

//    public Button HintBtn;

//    public HintLineConnector[] hintLineRenderer;

//    private string lastMapID = "";

//    private void Awake()
//    {
//        //InitializeHintCounts();
//    }
//    private void Start()
//    {
//        if (hintVolume == null)
//            hintVolume = Object.FindFirstObjectByType<Volume>();

//        InitializeHintCounts();

//        //Volume에서 Coloradjustments 컴포넌트를 가져옴
//        hintVolume.profile.TryGet(out colorAdjustments);
//        //ResetHintCount();
//        //  모든 맵 힌트 카운트 초기화
//        ResetAllHintCounts();

//        // 버튼이 할당되어 있으면 클릭 이벤트 연결
//        if (HintBtn != null)
//        {
//            HintBtn.onClick.AddListener(TryUseHintButton);
//        }
//    }
//    private void Update()
//    {
//        string currentMapID = MapManager.Instance.currentMapID;
//        // 맵 ID 변경 감지
//        if (currentMapID != lastMapID)
//        {
//            lastMapID = currentMapID;

//            // 힌트 사용 가능한 맵일 경우 UI 강제 갱신
//            if (PuzzleManager.Instance.IsPuzzleMap(currentMapID))
//            {
//                UpdateHintIcon();
//                UpdateHintUI();
//                UpdateHintTimeBar();
//            }
//        }

//        if (Input.GetKeyDown(KeyCode.E) && (PuzzleManager.Instance.IsPuzzleMap(currentMapID)))
//        {
//            TryUseHint();
//        }

//        UpdateHintDuration();
//        UpdateCooldown();
//        UpdateHintUI();
//        UpdateHintTimeBar();

//        if (!PuzzleManager.Instance.IsPuzzleMap(currentMapID))
//        {
//            ForceResetHintState();
//        }
//        else if (isGrayscale)
//        {
//            foreach (var connector in hintLineRenderer)
//            {
//                //connector.UpdateLineRenderer();
//            }
//        }
//    }

//    private void InitializeHintCounts()
//    {
//        hintCountsByMap.Clear();
//        if (PuzzleManager.Instance == null)
//        {
//            Debug.LogWarning("PuzzleManager가 아직 초기화되지 않았습니다.");
//            return;
//        }
//        var puzzleIDs = PuzzleManager.Instance.GetAllPuzzleMapIDs();
//        foreach (string id in puzzleIDs)
//        {
//            hintCountsByMap[id] = maxHintCount;
//        }
//    }

//    void TryUseHintButton()
//    {
//        TryUseHint();
//    }

//    void TryUseHint()
//    {
//        string currentMapID = MapManager.Instance.currentMapID;

//        if (!isGrayscale)
//        {
//            //if (hintCount > 0 && !isInCooldown)
//            if (hintCountsByMap.ContainsKey(currentMapID) && hintCountsByMap[currentMapID] > 0 && !isInCooldown)
//            {
//                //ActivateHint();
//                ActivateHint(currentMapID); //  currentMapID 전달
//            }
//            else if (isInCooldown)
//            {
//                Debug.Log(" 힌트는 아직 쿨타임 중입니다!");
//            }
//            else
//            {
//                Debug.Log(" 힌트는 더 이상 사용할 수 없습니다!");
//            }
//        }
//        else
//        {
//            DeactivateHint();

//        }
//    }
//    void UpdateHintTimeBar()
//    {
//        //힌트 지속 시간 바 업데이트
//        if (hintDurationTimeBar != null)
//        {
//            if (isGrayscale)
//            {
//                float ratio = Mathf.Clamp01((hintDuration - hintDurationTimer) / hintDuration);
//                hintDurationTimeBar.fillAmount = ratio; // 지속 시간 바 업데이트

//            }
//            else
//            {
//                hintDurationTimeBar.fillAmount = 0f; // 그레이스케일이 아닐 때는 0으로 설정
//            }
//        }
//        if (hintCooldownTimeBar != null)
//        {
//            //힌트 쿨타임 바 업데이트
//            if (isInCooldown)
//            {
//                //float ratio = Mathf.Clamp01((hintCooldown - hintCooldownTimer)/ hintCooldown);//바 점점 줄어듬
//                float ratio = Mathf.Clamp01(hintCooldownTimer / hintCooldown); // 바가 점점 늘어남
//                hintCooldownTimeBar.fillAmount = ratio; // 쿨타임 바 업데이트
//            }
//            else
//            {
//                hintCooldownTimeBar.fillAmount = 1f; // 쿨타임이 아닐 때는 1으로 설정
//            }
//        }
//    }

//    //void ActivateHint()
//    void ActivateHint(string mapID) //  맵 ID 기반으로 처리
//    {
//        isGrayscale = true;
//        hintDurationTimer = 0f;
//        hintCooldownTimer = 0f;
//        isInCooldown = true;

//        colorAdjustments.saturation.value = -100f; // 그레이스케일 효과 적용


//        //  힌트 활성화가 성공했을 때만 선 켜기
//        foreach (var connector in hintLineRenderer)
//        {
//            connector.SetActiveParticle(true);
//        }

//        //hintCount--; // 힌트 사용
//        hintCountsByMap[mapID]--; //  해당 맵 힌트 감소
//        Debug.Log($"힌트 사용! [{mapID}] 남은 힌트: {hintCountsByMap[mapID]}");
//        //Debug.Log($"힌트 사용! 남은 힌트: {hintCount}");
//        UpdateHintIcon();
//    }

//    void UpdateHintDuration()
//    {
//        if (!isGrayscale) return;
//        hintDurationTimer += Time.deltaTime;
//        if (hintDurationTimer >= hintDuration)
//        {
//            DeactivateHint();
//            Debug.Log("힌트 지속 시간 종료!");
//        }
//    }

//    void UpdateCooldown()
//    {
//        if (!isInCooldown) return;
//        hintCooldownTimer += Time.deltaTime;
//        if (hintCooldownTimer >= hintCooldown)
//        {
//            isInCooldown = false;
//            Debug.Log("힌트 다시 사용 가능!");
//        }
//    }

//    void DeactivateHint()
//    {
//        isGrayscale = false;
//        colorAdjustments.saturation.value = 0f; // 그레이스케일 효과 해제

//        // 힌트 비활성화 시 선도 꺼주기
//        foreach (var connector in hintLineRenderer)
//        {
//            connector.SetActiveParticle(false);
//        }
//    }

//    void UpdateHintUI()
//    {
//        if (hintDurationTxt != null)
//        {
//            if (isGrayscale)
//            {
//                hintDurationTxt.text = $"Hint Dur: {Mathf.Max(0, hintDuration - hintDurationTimer):F1}";

//            }
//            else
//            {
//                hintDurationTxt.text = "--";
//            }
//        }
//        if (hintCooldownTxt != null)
//        {
//            if (isInCooldown)
//            {
//                hintCooldownTxt.text = $"Hint Cool: {Mathf.Max(0, hintCooldown - hintCooldownTimer):F1}";
//            }
//            else
//            {
//                hintCooldownTxt.text = "Hint";
//            }
//        }
//    }

//    void UpdateHintIcon()
//    {
//        string currentMapID = MapManager.Instance.currentMapID;
//        int currentCount = hintCountsByMap.ContainsKey(currentMapID) ? hintCountsByMap[currentMapID] : 0;
//        for (int i = 0; i < hintIcons.Length; i++)
//        {
//            if (hintIcons[i] != null)
//            {
//                hintIcons[i].gameObject.SetActive(i < currentCount); // 남은 힌트 개수에 따라 아이콘 활성화
//            }
//        }
//    }

//    void ResetAllHintCounts() //  맵별 힌트 전부 초기화
//    {
//       foreach(string mapID in PuzzleManager.Instance.GetAllPuzzleMapIDs())
//        {
//            hintCountsByMap[mapID] = maxHintCount; // 각 맵의 힌트 카운트를 초기화
//        }
//    }

//    public void ForceResetHintState()
//    {
//        isGrayscale = false;
//        isInCooldown = false;
//        hintDurationTimer = 0f;
//        hintCooldownTimer = 0f;

//        if (colorAdjustments != null)
//            colorAdjustments.saturation.value = 0f;

//        //foreach (var connector in hintLineRenderer)
//        //    connector.SetActive(false);

//        //ResetAllHintCounts(); //  힌트 전체 초기화

//        UpdateHintIcon();
//        UpdateHintUI();
//        UpdateHintTimeBar();
//    }

//}

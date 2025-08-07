using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleHintManager : MonoBehaviour
{
    public static PuzzleHintManager Instance { get; private set; }

    [Header("연결 컴포넌트")]
    public PuzzleHintConnector connectorPrefab;// 연결선(힌트 선)을 그릴 프리팹

    private PuzzleHintConnector activePlayerTargetToPlayer;// 박스 → 플레이어
    private PuzzleHintConnector activeAnswertToTarge;// 정답 → 박스

    [Header("탐색 및 후처리")]
    public PuzzleHintTargetFinder finder;
    public PuzzleHintEffectController effect;
    public PuzzleHintUIController ui;

    [Header("힌트 설정")]
    public float hintDuration = 15f;
    public float hintCooldown = 30f;
    public int maxHintCount = 3;

    private float durationTimer;
    private float cooldownTimer;

    private bool isActive;// 현재 힌트가 활성 상태인지
    private bool isCooldown;// 쿨다운 중인지

    [Header("Puzzle Hint 버튼")]
    public Button puzzleHintButton;

    // 각 맵 ID별로 남은 힌트 횟수를 저장
    private Dictionary<string, int> hintCounts = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 퍼즐 맵 ID별로 힌트 횟수 초기화
        foreach (string mapID in PuzzleManager.Instance.GetAllPuzzleMapIDs())
        {
            hintCounts[mapID] = maxHintCount;
            Debug.Log($"{mapID}[HintManager] 초기 힌트 횟수 설정됨: {maxHintCount}회");
        }

        if (puzzleHintButton != null)
        {
            puzzleHintButton.onClick.AddListener(TryActivateHint);
        }
        else
        {
            Debug.LogWarning("[HintManager] puzzleHintButton이 할당되지 않았습니다.");

        }
    }
    private void OnEnable()
    {
        MapTransition.OnMapTransitionComplete += UpdateUIHintIcons;
    }

    private void OnDisable()
    {
        MapTransition.OnMapTransitionComplete -= UpdateUIHintIcons;
    }
    private void UpdateUIHintIcons()
    {
        int currentHintCount = GetCurrentHintCount();
        ui?.UpdateHintIcons(currentHintCount);

        // 퍼즐 맵이 아니면 힌트 비활성화
        if (!PuzzleManager.Instance.IsPuzzleMap(CurrentMapID()))
        {
            DeactivateHint();
            return;
        }
    }
    private float updateInterval = 1f;
    private float timer = 0f;
    private void Update()
    {


        if (Input.GetKeyDown(KeyCode.E))
        {
            TryActivateHint();
        }

        if (isActive)
        {
            durationTimer += Time.deltaTime;
            timer += Time.deltaTime;
            Transform target = null;
            Transform answer = null;
            if (timer >= updateInterval)
            {
                timer = 0f;
                // 0.1초 마다 가장 가까운 대상과 정답 갱신
                target = finder.FindClosestTarget();
                answer = finder.FindAnswerForTarget(target);
            }
            //// 매 프레임마다 가장 가까운 대상과 정답 갱신
            //var target = finder.FindClosestTarget();
            //var answer = finder.FindAnswerForTarget(target);

            if (target != null && answer != null)
            {
                float dist1 = Vector2.Distance(finder.player.position, target.position);
                float dist2 = Vector2.Distance(target.position, answer.position);

                activePlayerTargetToPlayer?.SetTargets(finder.player, target, dist1);
                activeAnswertToTarge?.SetTargets(target, answer, dist2);
            }

            // 지속 시간이 다 되면 자동 종료
            if (durationTimer >= hintDuration)
                DeactivateHint();
        }
        // 쿨다운 처리
        if (isCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= hintCooldown)
                isCooldown = false;
        }
        // 힌트 UI 바 갱신
        ui?.UpdateBars(isActive, durationTimer, hintDuration, isCooldown, cooldownTimer, hintCooldown);
    }
    // 힌트를 시도하려고 할 때 호출
    public void TryActivateHint()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || Time.timeScale == 0f || !PlayerController.Instance.CanMove())
        {
            Debug.LogWarning("플레이어 이동 불가 상태 혹은 게임 멈춘상태");
            return;
        }
        // 힌트가 이미 활성화되어 있으면 → 종료
        if (isActive)
        {
            Debug.Log("[HintManager] 힌트 중 E키 or 버튼 눌림 → 힌트 종료");
            DeactivateHint();
            return;
        }
        if (!CanUseHint())
        {
            AudioManager.Instance.PlaySFX("ImpossibleFeedback");
            return;
        }
       
        var target = finder.FindClosestTarget(); // 플레이어 기준 가장 가까운 오브젝트
        var answer = finder.FindAnswerForTarget(target); // 해당 오브젝트의 정답 위치



        if (target == null || answer == null)
        {
            Debug.LogWarning("[HintManager] 타겟 또는 정답이 없습니다.");
            return;
        }
        float dist1 = Vector2.Distance(finder.player.position, target.position);
        float dist2 = Vector2.Distance(target.position, answer.position);

        // 너무 가까우면 힌트 생략
        float touchingThreshold = 0.2f;
        if (dist1 < touchingThreshold)
        {
            Debug.Log($"[HintManager] 박스와 너무 가까움. 힌트 생략됨. 거리: {dist1:F2}");
            return;
        }
        if (dist2 < touchingThreshold)
        {
            Debug.Log($"[HintManager] 정답과 너무 가까움. 힌트 생략됨. 거리: {dist2:F2}");
            return;
        }
        // 힌트 활성화
        isActive = true;
        isCooldown = true;
        durationTimer = 0;
        cooldownTimer = 0;

        // 연결 오브젝트 생성 (타겟→플레이어, 정답→타겟)
        activePlayerTargetToPlayer = Instantiate(connectorPrefab, transform);
        activeAnswertToTarge = Instantiate(connectorPrefab, transform);

        // 최초 타겟 설정 (거리 기반)

        activePlayerTargetToPlayer.SetTargets(finder.player, target, dist1);
        activeAnswertToTarge.SetTargets(target, answer, dist2);

        // 파티클 및 선 활성화
        activePlayerTargetToPlayer.SetHintActive(true);
        activeAnswertToTarge.SetHintActive(true);

        //흑백 효과 적용
        effect?.SetGrayscale(true);

        // 횟수 차감
        hintCounts[CurrentMapID()]--;
        ui?.UpdateHintIcons(GetCurrentHintCount());

        Debug.Log($"[HintManager] 힌트 사용됨 - 남은 횟수: {hintCounts[CurrentMapID()]}");
    }
    // 힌트 비활성화 처리
    public void DeactivateHint()
    {
        Debug.Log($"[HintManager] 힌트 비활성화 호출됨 (duration: {durationTimer:F2})");
        isActive = false;

        if (activePlayerTargetToPlayer != null)
            Destroy(activePlayerTargetToPlayer.gameObject);

        if (activeAnswertToTarge != null)
            Destroy(activeAnswertToTarge.gameObject);

        activePlayerTargetToPlayer = null;
        activeAnswertToTarge = null;

        effect?.SetGrayscale(false);// 효과 제거
    }
    // 힌트를 사용할 수 있는 조건 확인
    public bool CanUseHint()
    {
        return !isActive && !isCooldown && GetCurrentHintCount() > 0;
    }
    // 현재 맵에서 남은 힌트 횟수 반환
    public int GetCurrentHintCount()
    {
        return hintCounts.TryGetValue(CurrentMapID(), out int count) ? count : 0;
    }
    // 현재 맵 ID 조회
    private string CurrentMapID() => MapManager.Instance.currentMapID;
}

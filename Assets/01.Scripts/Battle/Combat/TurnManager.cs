using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum Phase { Player, EnemyPreview, Enemy }

    public Phase currentPhase { get; private set; }
    
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Animator enemyAnimator;

    // 각 페이즈 진입 시점에 구독할 이벤트
    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemySkillPreview;
    public event Action OnEnemyTurnStart;
    
    [Tooltip("Enemy 스킬 프리뷰 후 실제 적 턴 시작까지의 대기 시간(초)")]
    public float enemyPreviewDuration = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 전투 시작을 외부에서 호출, 지금은 사용 안됨.
    /// </summary>
    public void StartCombat()
    {
        StartPlayerTurn();
    }

    /// <summary>
    /// 플레이어 턴 시작
    /// </summary>
    public void StartPlayerTurn()
    {
        currentPhase = Phase.Player;
        OnPlayerTurnStart?.Invoke();
        
        // 턴 종료 버튼 활성화
        endTurnButton.enabled = true;
    }
    
    /// <summary>
    /// 플레이어가 “턴 종료” 버튼을 눌렀을 때
    /// </summary>
    public void EndPlayerTurn()
    {
        currentPhase = Phase.EnemyPreview;
        OnPlayerTurnEnd?.Invoke();
        OnEnemySkillPreview?.Invoke();

        // 턴 종료 버튼 비활성화
        endTurnButton.enabled = false;
        
        StartCoroutine(DoEnemyTurnAfterDelay());
    }
    
    IEnumerator DoEnemyTurnAfterDelay()
    {
        yield return new WaitForSeconds(enemyPreviewDuration);
        StartEnemyTurn();
    }

    /// <summary>
    /// 적 턴 실제 시작 (프리뷰 후 수초 뒤에 호출)
    /// </summary>
    public void StartEnemyTurn()
    {
        currentPhase = Phase.Enemy;
        OnEnemyTurnStart?.Invoke();
        StartCoroutine(EnemyAnimationDelay());
    }

    IEnumerator EnemyAnimationDelay()
    {
        // 한 프레임 대기해서 트리거가 제대로 들어간 애니메이터 상태로 업데이트되도록 함
        yield return null;

        // 현재 플레이 중인 애니메이션 클립 길이 계산
        var state = enemyAnimator.GetCurrentAnimatorStateInfo(0);
        float duration = state.length / state.speed;

        // 실제 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(duration);

        // 플레이어 턴 시작
        StartPlayerTurn();
    }
}
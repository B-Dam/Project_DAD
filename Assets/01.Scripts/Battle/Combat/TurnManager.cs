using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum Phase { Player, EnemyPreview, Enemy }

    public Phase currentPhase { get; private set; }

    // 각 페이즈 진입 시점에 구독할 이벤트
    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemySkillPreview;
    public event Action OnEnemyTurnStart;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 전투 시작을 외부에서 호출
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
    }
    
    /// <summary>
    /// 플레이어가 “턴 종료” 버튼을 눌렀을 때
    /// </summary>
    public void EndPlayerTurn()
    {
        currentPhase = Phase.EnemyPreview;
        OnPlayerTurnEnd?.Invoke();
        OnEnemySkillPreview?.Invoke();
    }

    /// <summary>
    /// 적 턴 실제 시작 (프리뷰 후 수초 뒤에 호출)
    /// </summary>
    public void StartEnemyTurn()
    {
        currentPhase = Phase.Enemy;
        OnEnemyTurnStart?.Invoke();
    }
}
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class TimedModifier
{
    public int value;
    public int remainingTurns;
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("현재 HP")]
    public int playerHp { get; private set; }
    public int enemyHp  { get; private set; }
    
    [Header("현재 보호막")]
    public int playerShield { get; private set; }
    public int enemyShield  { get; private set; }

    [Header("버프, 디버프")]
    public int playerAtkMod { get; private set; }
    public int enemyAtkMod  { get; private set; }
    
    List<TimedModifier> playerAttackMods = new List<TimedModifier>();
    List<TimedModifier> enemyAttackMods  = new List<TimedModifier>();

    // 캐릭터 공격력, 방어력 가져오기
    public int PlayerBaseAtk => DataManager.Instance.playerData.atk;
    int EnemyBaseAtk  => DataManager.Instance.enemyData.atk;
    
    public event Action OnCombatStart;
    public event Action OnStatsChanged;
    

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart += OnPlayerTurnStart;
        }
        else Debug.Log("CombatManager에서 TrunManager 이벤트 구독 실패");
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= OnPlayerTurnStart;
            TurnManager.Instance.OnEnemyTurnStart -= OnEnemyTurnStart;
        }
    }

    /// <summary>전투 개시</summary>
    public void StartCombat()
    {
        // HP 초기화
        playerHp = DataManager.Instance.playerData.maxHP;
        enemyHp  = DataManager.Instance.enemyData.maxHP;
        
        // 쉴드 초기화
        playerShield = 0;
        enemyShield  = 0;
        
        // 상태이상 값 초기화
        playerAttackMods.Clear();
        enemyAttackMods.Clear();
        
        // 모디파이어 갱신
        RecalculateModifiers();
        
        //
        OnCombatStart?.Invoke();
        
        // 첫 턴 시작
        TurnManager.Instance.StartPlayerTurn();
    }
    
    /// <summary>모디파이어 리스트에서 턴 감소, 만료된 모디파이어 제거</summary>
    void UpdateModifiers(List<TimedModifier> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (--list[i].remainingTurns <= 0)
                list.RemoveAt(i);
        }
    }
    
    // 모디파이어 합산
    void RecalculateModifiers()
    {
        playerAtkMod = 0;
        foreach (var m in playerAttackMods) playerAtkMod += m.value;
        enemyAtkMod  = 0;
        foreach (var m in enemyAttackMods)  enemyAtkMod  += m.value;
    }

    void OnPlayerTurnStart()
    {
        // 버프 / 디버프 턴 감소
        UpdateModifiers(playerAttackMods);
        // 모디파이어 합산
        RecalculateModifiers();
        // 플레이어 보호막 초기화
        playerShield = 0;
        // UI 갱신
        OnStatsChanged?.Invoke();
    }

    void OnEnemyTurnStart()
    {
        // 적 전용 모디파이어 업데이트
        UpdateModifiers(enemyAttackMods);
        RecalculateModifiers();
        
        // 적 스킬 사용
        var skills = DataManager.Instance.GetEnemySkills();
        var skill = skills[Random.Range(0, skills.Length)];
        ApplySkill(skill, false);

        // 다음 Player 턴으로
        TurnManager.Instance.StartPlayerTurn();
    }

    /// <summary>
    /// 카드 또는 스킬 적용
    /// </summary>
    /// <param name="data">카드/스킬 데이터</param>
    /// <param name="isPlayer">플레이어가 사용했으면 true, 적이면 false</param>
    public void ApplySkill(CardData data, bool isPlayer)
    {
        // 기본 공격력 + 공격 모디파이어
        int baseAtk   = isPlayer ? PlayerBaseAtk : EnemyBaseAtk;
        int modAtk    = isPlayer ? playerAtkMod   : enemyAtkMod;
        int rawAttack = baseAtk + data.effectAttackValue + modAtk;
        
        Debug.Log($"[ApplySkill] {(isPlayer?"Player":"Enemy")} uses {data.displayName} → base:{baseAtk} + effect:{data.effectAttackValue} + mod:{modAtk} = rawAttack:{rawAttack}");

        // 공격 계수가 0보다 클 때만 데미지 계산
        if (data.effectAttackValue > 0)
        {
            if (isPlayer)
            {
                int shielded = Mathf.Min(enemyShield, rawAttack);
                enemyShield -= shielded;
                enemyHp = Mathf.Max(0, enemyHp - (rawAttack - shielded));
            }
            else
            {
                int shielded = Mathf.Min(playerShield, rawAttack);
                playerShield -= shielded;
                playerHp = Mathf.Max(0, playerHp - (rawAttack - shielded));
            }
        }

        // 보호막 효과
        if (data.effectShieldValue > 0)
        {
            if (isPlayer) playerShield += data.effectShieldValue;
            else          enemyShield  += data.effectShieldValue;
        }

        // 공격력 버프/디버프
        if (data.effectAttackIncreaseValue != 0 && data.effectTurnValue > 0)
            AddAttackModifier(isPlayer, data.effectAttackIncreaseValue, data.effectTurnValue);

        if (data.effectAttackDebuffValue != 0 && data.effectTurnValue > 0)
        {
            int debuffAmount = Mathf.Abs(data.effectAttackDebuffValue);
            AddAttackModifier(!isPlayer, -debuffAmount, data.effectTurnValue);
        }

        OnStatsChanged?.Invoke();
        RecalculateModifiers();
        CheckEnd();
    }

    
    //  모디파이어 추가
    void AddAttackModifier(bool targetIsPlayer, int value, int turns)
    {
        var list = targetIsPlayer ? playerAttackMods : enemyAttackMods;
        list.Add(new TimedModifier { value = value, remainingTurns = turns });
    }

    // 적이나 아군 체력을 확인하고 종료 로직 작동
    void CheckEnd()
    {
        if (enemyHp <= 0)
        {
            Debug.Log("Victory!");
            // 승리 시 추가 로직
        }
        else if (playerHp <= 0)
        {
            Debug.Log("Defeat...");
            // 패배 시 추가 로직
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum StatType
{
    Attack,
    Defense
}

public class TimedModifier
{
    public StatType statType;
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
    public int playerDefMod { get; private set; }
    public int enemyDefMod  { get; private set; }
    
    List<TimedModifier> playerModifiers = new List<TimedModifier>();
    List<TimedModifier> enemyModifiers  = new List<TimedModifier>();

    // 캐릭터 공격력, 방어력 가져오기
    public int PlayerBaseAtk => DataManager.Instance.playerData.atk;
    int PlayerBaseDef => DataManager.Instance.playerData.def;
    int EnemyBaseAtk  => DataManager.Instance.enemyData.atk;
    int EnemyBaseDef  => DataManager.Instance.enemyData.def;
    

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
            TurnManager.Instance.OnEnemyTurnStart += OnEnemyTurnStart;
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
        playerModifiers.Clear();
        enemyModifiers.Clear();
        
        // 모디파이어 갱신
        RecalculateModifiers();
        
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
        playerAtkMod = playerModifiers.Where(m => m.statType == StatType.Attack).Sum(m => m.value);
        playerDefMod = playerModifiers.Where(m => m.statType == StatType.Defense).Sum(m => m.value);
        enemyAtkMod  = enemyModifiers.Where(m => m.statType == StatType.Attack).Sum(m => m.value);
        enemyDefMod  = enemyModifiers.Where(m => m.statType == StatType.Defense).Sum(m => m.value);
    }

    void OnPlayerTurnStart()
    {
        UpdateModifiers(playerModifiers);
        RecalculateModifiers();
        // UI에 PlayerHp 갱신, AP 초기화 등
    }

    void OnEnemyTurnStart()
    {
        // 적 전용 모디파이어 업데이트
        UpdateModifiers(enemyModifiers);
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

        // 방어력(Def + defMod) 적용
        int defenderDef = (isPlayer ? EnemyBaseDef : PlayerBaseDef)
                          + (isPlayer ? enemyDefMod : playerDefMod);
        int afterDef    = Mathf.Max(0, rawAttack - defenderDef);

        // 방어막(Shield) 적용
        if (isPlayer)
        {
            int shielded = Mathf.Min(enemyShield, afterDef);
            enemyShield -= shielded;
            enemyHp     = Mathf.Max(0, enemyHp - (afterDef - shielded));
        }
        else
        {
            int shielded      = Mathf.Min(playerShield, afterDef);
            playerShield     -= shielded;
            playerHp         = Mathf.Max(0, playerHp - (afterDef - shielded));
        }

        // 방어 효과: 지속 버프 vs 즉시 쉴드
        if (data.effectDefenseValue > 0)
        {
            if (data.effectTurnValue > 0)
                AddModifier(!isPlayer, StatType.Defense, data.effectDefenseValue, data.effectTurnValue);
            else if (isPlayer)
                playerShield   += data.effectDefenseValue;
            else
                enemyShield    += data.effectDefenseValue;
        }

        // 공격력 디버프(+버프) 처리
        if (data.effectDebuffValue != 0 && data.effectTurnValue > 0)
        {
            AddModifier(!isPlayer, StatType.Attack, -data.effectDebuffValue, data.effectTurnValue);
        }

        RecalculateModifiers();
        CheckEnd();
    }

    // 모디파이어 추가
    void AddModifier(bool targetIsPlayer, StatType stat, int value, int turns)
    {
        var list = targetIsPlayer ? playerModifiers : enemyModifiers;
        list.Add(new TimedModifier { statType = stat, value = value, remainingTurns = turns });
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
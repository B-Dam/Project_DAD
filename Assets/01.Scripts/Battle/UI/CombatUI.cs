using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class CombatUI : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] TMP_Text playerHPText;
    [SerializeField] TMP_Text playerShieldText;
    [SerializeField] TMP_Text playerAPText;

    [Header("적 스탯")]
    [SerializeField] TMP_Text enemyHPText;
    [SerializeField] TMP_Text enemyShieldText;
    
    public static CombatUI instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 전투 시작과 턴 전환 이벤트 구독
        CombatManager.Instance.OnCombatStart       += OnCombatStart;
        CombatManager.Instance.OnStatsChanged      += UpdateUI;
        TurnManager.Instance.OnPlayerTurnStart     += OnPlayerTurnStart;
        TurnManager.Instance.OnEnemySkillPreview   += OnEnemySkillPreview;
        TurnManager.Instance.OnEnemyTurnStart      += OnEnemyTurnStart;

        // 씬 로드 직후(전투 시작 전일 수도 있지만) 한번 갱신
        StartCoroutine(DelayedUpdateUI());
    }

    void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart  -= OnCombatStart;
            CombatManager.Instance.OnStatsChanged -= UpdateUI;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart     -= OnPlayerTurnStart;
            TurnManager.Instance.OnEnemySkillPreview   -= OnEnemySkillPreview;
            TurnManager.Instance.OnEnemyTurnStart      -= OnEnemyTurnStart;
        }
    }

    void OnCombatStart()       => StartCoroutine(DelayedUpdateUI());
    void OnPlayerTurnStart()   => StartCoroutine(DelayedUpdateUI());
    void OnEnemySkillPreview() => StartCoroutine(DelayedUpdateUI());
    void OnEnemyTurnStart()    => StartCoroutine(DelayedUpdateUI());

    IEnumerator DelayedUpdateUI()
    {
        // HandManager/AP 세팅, CombatManager/playerHp 세팅이 끝난 뒤에 호출될 수 있도록
        yield return null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        var cm = CombatManager.Instance;
        if (cm == null) return;

        // 플레이어
        playerHPText.text     = $"HP: {cm.playerHp}/{DataManager.Instance.playerData.maxHP}";
        playerShieldText.text = $"Shield: {cm.playerShield}";
        playerAPText.text     = $"AP: {HandManager.Instance.currentAP}/3";

        // 적
        enemyHPText.text      = $"HP: {cm.enemyHp}/{DataManager.Instance.enemyData.maxHP}";
        enemyShieldText.text  = $"Shield: {cm.enemyShield}";
    }
}
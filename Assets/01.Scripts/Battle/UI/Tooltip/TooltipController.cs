using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }
    
    [Header("툴팁 패널")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 초기 숨김
        panelRoot.SetActive(false);
    }

    /// <summary>
    /// PreviewUI.currentSkill로부터 정보를 가져와 화면에 고정 표시
    /// </summary>
    public void ShowCurrentSkill()
    {
        var skill = PreviewUI.Instance.CurrentSkill;
        if (skill == null) return;

        nameText.text = skill.displayName;

        // 포맷팅
        string formatted = TextFormatter.Format(
            skill.effectText,
            new System.Collections.Generic.Dictionary<string,string> {
                { "damage", (CombatManager.Instance.EnemyBaseAtk
                             + skill.effectAttackValue
                             + CombatManager.Instance.enemyAtkMod).ToString() },
                { "turns",  skill.effectTurnValue.ToString() },
                { "shield", skill.effectShieldValue.ToString() },
                { "debuff", skill.effectAttackDebuffValue.ToString() },
                { "buff",   skill.effectAttackIncreaseValue.ToString() }
            }
        );
        descText.text = formatted;

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
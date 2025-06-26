using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PreviewUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static PreviewUI Instance { get; private set; }
    
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;

    private CardData currentSkill;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(CardData skill)
    {
        currentSkill = skill;
        iconImage.sprite = skill.icon;
        nameText.text    = skill.displayName;
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSkill == null) return;

        // 툴팁에 표시할 원본 텍스트
        string raw = currentSkill.effectText;

        // 포맷팅(예: {damage}, {shield} 치환)
        string formatted = TextFormatter.Format(
            raw,
            new Dictionary<string, string> {
                { "damage", (CombatManager.Instance.PlayerBaseAtk
                             + currentSkill.effectAttackValue
                             + CombatManager.Instance.playerAtkMod)
                    .ToString() },
                { "turns",  currentSkill.effectTurnValue.ToString() },
                { "shield", currentSkill.effectShieldValue.ToString() },
                { "debuff", currentSkill.effectAttackDebuffValue.ToString() },
                { "buff",   currentSkill.effectAttackIncreaseValue.ToString() }
            }
        );

        // 툴팁 컨트롤러에 텍스트와 마우스 위치 전달
        TooltipController.Instance.Show(formatted);
    }

    // 마우스가 떠나면 툴팁 숨기기
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}
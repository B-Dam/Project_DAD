using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpecialAbilityPanel : MonoBehaviour
{
    [Header("버튼 3개 (Inspector에서 크기 3으로)")]
    [SerializeField] private Button[] abilityButtons;

    void OnEnable()
    {
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            var btn = abilityButtons[i];

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAbilitySelected(idx));

            // (필요하면) 아이콘/텍스트 세팅
        }
    }

    void OnAbilitySelected(int idx)
    {
        ExecuteSkill(idx);

        // 스킬 실행 뒤에만 게이지 초기화
        CombatManager.Instance.ConsumeSpecialGauge();

        // 버튼 비활성화
        SpecialGaugeUI.Instance.DisableSpecialButton();

        // 패널 닫기
        gameObject.SetActive(false);
    }
    
    void ExecuteSkill(int idx)
    {
        switch(idx)
        {
            case 0:
                CombatManager.Instance.SpecialAttack(50);
                break;
            case 1:
                CombatManager.Instance.SpecialShield(50);
                break;
            case 2:
                CombatManager.Instance.SpecialDebuff(20, 3);
                break;
        }
    }
}
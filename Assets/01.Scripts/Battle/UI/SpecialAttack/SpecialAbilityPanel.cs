using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpecialAbilityPanel : MonoBehaviour
{
    [Header("버튼 3개 (Inspector에서 크기 3으로)")]
    [SerializeField] private Button[] abilityButtons;
    
    [Header("필살기 사용 후 컷씬")]
    [SerializeField] private Animator cutsceneAnimator; // 컷씬용 Animator
    [SerializeField] private CombatAnimationController animCtrl; // 스킬 애니메이션 컨트롤러
    
    void OnEnable()
    {
        for (int i = 0; i < abilityButtons.Length; i++)
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
        SpecialAbilityUI.Instance.HideSpecialPanel();
    }
    
    IEnumerator DoSpecial(int idx)
    {
        // UI 잠금 + 패널 사라짐
        SpecialGaugeUI.Instance.DisableSpecialButton();
        SpecialAbilityUI.Instance.HideSpecialPanel();

        // 컷씬 연출
        cutsceneAnimator.SetTrigger("Play");
        
        // 컷씬 애니메이션 길이만큼 대기 (시간은 수정)
        yield return new WaitForSeconds(1.5f);

        // 캐릭터 스킬 애니메이션 트리거
        switch (idx)
        {
            case 0: animCtrl.TriggerSpecialAttack(); break;
            case 1: animCtrl.TriggerSpecialShield(); break;
            case 2: animCtrl.TriggerSpecialStun();   break;
        }

        // 스킬 애니메이션 종료를 대기
        /*yield return new WaitUntil(() => animCtrl.IsSkillAnimationDone);*/

        // 실제 효과 적용
        ExecuteSkill(idx);
        CombatManager.Instance.ConsumeSpecialGauge();
    }
    
    void ExecuteSkill(int idx)
    {
        var cm = CombatManager.Instance;
        switch(idx)
        {
            case 0:
                cm.SpecialAttack(60);
                break;
            case 1:
                cm.SpecialShield(50);
                cm.AddReflectBuff(0.5f, 1);
                break;
            case 2:
                cm.SpecialStun(2);
                break;
        }
    }
}
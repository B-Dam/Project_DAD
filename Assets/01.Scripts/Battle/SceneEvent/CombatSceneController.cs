using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CombatSceneController : MonoBehaviour
{
    [Header("배경")]
    [SerializeField] private Image backgroundImage;

    [Header("적")]
    [SerializeField] private GameObject enemyObject; // 적 오브젝트 (애니메이터 포함)
    [SerializeField] private Animator enemyAnimator;

    [Header("GameUI 그룹")]
    [SerializeField] private CanvasGroup gameUIGroup; // 첫 시작시 제어할 게임 UI
    [SerializeField] private CanvasGroup battleStartGroup;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float startAnimDuration = 0.6f;
    [SerializeField] private float uiFadeDuration   = 0.3f;
    [SerializeField] private float battleDisplayTime = 1.2f;
    [SerializeField] private float staggerInterval  = 0.1f;

    
    private void Start()
    {
        CombatSetupData data = CombatDataHolder.GetData();
        
        if (data != null)
            SetupCombat(data);
        else
            Debug.LogWarning("전투 세팅 데이터가 없습니다.");
        
        var animCtrl = enemyObject.GetComponentInChildren<CombatAnimationController>();
        if (animCtrl != null)
            animCtrl.enemyAnimator = this.enemyAnimator;
        
        // 전투 데이터 로드 후에 호출
        PlayBattleStartSequence();
    }

    // 받아오는 데이터를 기반으로 보스 및 배경 설정
    private void SetupCombat(CombatSetupData data)
    {
        // 배경 설정
        if (backgroundImage != null && data.backgroundSprite != null)
            backgroundImage.sprite = data.backgroundSprite;

        // 적 애니메이터 설정
        if (enemyAnimator != null && data.animatorController != null)
            enemyAnimator.runtimeAnimatorController = data.animatorController;

        // 적 데이터 설정
        if (data.enemyCharacterSO != null)
            DataManager.Instance.SetEnemy(data.enemyCharacterSO);
    }
    
    // 전투 시작시 자연스러운 UI 세팅
    public void PlayBattleStartSequence()
    {
        // 1) BattleStartPanel 활성화 및 초기 상태
        battleStartGroup.gameObject.SetActive(true);
        battleStartGroup.alpha = 0f;
        battleStartGroup.transform.localScale = Vector3.zero;

        // 2) GameUI 초기 숨김
        gameUIGroup.alpha = 0f;
        gameUIGroup.interactable = gameUIGroup.blocksRaycasts = false;

        // 3) DOTween 시퀀스 구성
        var seq = DOTween.Sequence();
        
        // 3-1) BattleStartPanel 나타내기 (스케일 + 페이드)
        seq.Append(battleStartGroup.DOFade(1f, startAnimDuration));
        seq.Join(battleStartGroup.transform
                                 .DOScale(1f, startAnimDuration)
                                 .SetEase(Ease.OutBack));
        
        // 3-2) 패널 뜨자마자 전투 시작
        seq.AppendCallback(() =>
        {
            CombatManager.Instance.StartCombat();
        });
        
        // 3-3) 잠시 대기
        seq.AppendInterval(battleDisplayTime);

        // 3-4) BattleStartPanel 사라지기
        seq.Append(battleStartGroup.DOFade(0f, uiFadeDuration));
        seq.Join(battleStartGroup.transform
                                 .DOScale(0.8f, uiFadeDuration));

        // 3-5) 완전 비활성화
        seq.AppendCallback(() =>
        {
            battleStartGroup.gameObject.SetActive(false);
        });

        // 3-6) GameUI 각 요소를 스태거(stagger)로 등장시키기
        seq.AppendCallback(ShowGameUIWithStagger);

        // 3-7) 시퀀스 완료 후 게임 시작 콜백
        seq.AppendCallback(() =>
        {
            gameUIGroup.interactable = gameUIGroup.blocksRaycasts = true;
        });
    }
    
    private void ShowGameUIWithStagger()
    {
        // GameUI 하위에 CanvasGroup이 붙은 요소들 가져오기
        var uiElements = gameUIGroup.GetComponentsInChildren<CanvasGroup>(true);
        float delay = 0f;

        foreach (var cg in uiElements)
        {
            // 각 요소를 순차적으로 페이드 인
            cg.alpha = 0f;
            cg.DOFade(1f, uiFadeDuration)
              .SetDelay(delay)
              .SetEase(Ease.Linear);
            delay += staggerInterval;
        }
    }
}
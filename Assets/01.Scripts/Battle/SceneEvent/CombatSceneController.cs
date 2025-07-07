using UnityEngine;
using UnityEngine.UI;

public class CombatSceneController : MonoBehaviour
{
    [Header("배경")]
    [SerializeField] private Image backgroundImage;

    [Header("적")]
    [SerializeField] private GameObject enemyObject; // 적 오브젝트 (애니메이터 포함)
    [SerializeField] private Animator enemyAnimator;

    private void Start()
    {
        CombatSetupData data = CombatDataHolder.GetData();
        
        if (data != null)
        {
            SetupCombat(data);
        }
        else
        {
            Debug.LogWarning("전투 세팅 데이터가 없습니다.");
        }
    }

    private void SetupCombat(CombatSetupData data)
    {
        // 배경 설정
        if (backgroundImage != null && data.backgroundSprite != null)
            backgroundImage.sprite = data.backgroundSprite;

        // 적 애니메이터 설정
        if (enemyAnimator != null && data.enemyAnimator != null)
            enemyAnimator.runtimeAnimatorController = data.enemyAnimator;

        // 적 데이터 설정
        if (data.enemyCharacterSO != null)
            DataManager.Instance.SetEnemy(data.enemyCharacterSO);
    }
}
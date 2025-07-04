using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatTriggerEvent : MonoBehaviour
{
    [Header("전투 정보")]
    public CombatSetupData setupData;

    public void TriggerCombat()
    {
        // 전투 데이터 전달
        CombatDataHolder.SetData(setupData);

        // 전투 씬 로딩
        SceneManager.LoadSceneAsync("Battle", LoadSceneMode.Additive);
    }
}
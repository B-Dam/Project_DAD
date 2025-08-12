using UnityEngine;

public class BattleNightSkinRemove : MonoBehaviour
{
  public  GameObject nightSkin;
    // Update is called once per frame
    void Update()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.IsInCombat)
        {
            // 전투 중일 때 BattleNightSkin 오브젝트를 비활성화
            nightSkin.SetActive(false);
        }
        else
        {
            // 전투 중이 아닐 때 BattleNightSkin 오브젝트를 활성화
            nightSkin.SetActive(true);
        }
    }
}

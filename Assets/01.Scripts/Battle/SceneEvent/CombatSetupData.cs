using UnityEngine;

[System.Serializable]
public class CombatSetupData
{
    public string enemyName; // 
    public Sprite backgroundSprite; // 전투 씬 유동적 배경 설정용
    public CharacterDataSO enemyCharacterSO; // 해당 적의 데이터
    public RuntimeAnimatorController animatorController;  // 애니메이터 컨트롤러 에셋
}
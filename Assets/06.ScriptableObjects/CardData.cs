using UnityEngine;

public enum CardTypePrimary { 공격, 약화, 방어 }
public enum CardTypeSecondary { None, 공격, 약화, 방어 }

[CreateAssetMenu(menuName = "Battle/CardData")]
public class CardData : ScriptableObject
{
    [Header("매핑된 키값")]
    public string cardId;              // 예: "SC001"
    public string displayName;         // 예: "물기"

    [Header("소유자 타입")]
    public OwnerType ownerType;           // "player", "captainCat" 등

    [Header("카드 분류")]
    public CardTypePrimary typePrimary;         // "공격", "약화", "방어"
    public CardTypeSecondary typeSecondary;       // 보조 분류

    [Header("효과 수치")]
    public int effectAttackValue;      // 공격 계수
    public int effectDefenseValue;     // 방어 계수
    public int effectDebuffValue;      // 디버프 계수
    public int effectTurnValue;        // 지속 턴 수

    [TextArea(2,4), Header("효과 텍스트")]
    public string effectText;          // 효과 설명

    [Header("등급")]
    public int rank;                   // 카드의 등급 (1성=1, 2성=2)
    
    [Header("애니메이션")]
    public string animationID;         // 재생할 애니메이션 트리거
    
    [Header("소모 AP")]
    public int costAP;                 // 기본적으로는 1만을 소모하지만 확장성을 위해 추가
    
    [Header("UI 리소스")]
    public Sprite icon; 
}
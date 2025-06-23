using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("SO 로드")]
    [SerializeField] CharacterData defaultEnemy; 
    public CharacterData playerData { get; private set; }
    public CharacterData enemyData { get; private set; }
    public CardData[] allCards { get; private set; }   // 모든 카드 SO

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllData();
        }
        else Destroy(gameObject);
    }

    void LoadAllData()
    {
        // 모든 카드 SO 로드
        allCards = Resources.LoadAll<CardData>("ScriptableObjects/Cards");

        // 2) 모든 캐릭터 SO 로드
        var chars = Resources.LoadAll<CharacterData>("ScriptableObjects/Characters");
        // 플레이어는 항상 Mono로 고정
        playerData = chars.First(c => c.characterId == "Mono");

        // Enemy: Inspector에서 지정된 defaultEnemy가 있으면 사용, 없으면 첫번째 또는 예외 처리
        if (defaultEnemy != null)
            enemyData = defaultEnemy;
        else
            enemyData = chars.FirstOrDefault(c => c.ownerType != OwnerType.Player)
                        ?? throw new System.Exception("Enemy CharacterData가 없습니다!");
        
        Debug.Log("[DM] allCards 로드 개수: " + allCards.Length);
    }
    
    /// <summary>
    /// 런타임에 적을 변경하고 싶을 때 호출
    /// </summary>
    public void SetEnemy(CharacterData newEnemy)
    {
        enemyData = newEnemy;
    }

    /// <summary>
    /// 플레이어 1성 카드만 반환 (초기 덱 구성 등)
    /// </summary>
    public CardData[] GetPlayerCards()
    {
        return allCards
               .Where(c => c.ownerType == OwnerType.Player && c.rank == 1)
               .ToArray();
    }

    /// <summary>
    /// 현재 enemyData.ownerType에 해당하는 스킬 풀 반환
    /// </summary>
    public CardData[] GetEnemySkills()
    {
        return allCards
               .Where(c => c.ownerType == enemyData.ownerType)
               .ToArray();
    }

    /// <summary>
    /// 합성용: 특정 카드 ID와 별 등급에 해당하는 CardData 반환
    /// </summary>
    public CardData GetCard(string cardId, int rank)
    {
        return allCards
            .FirstOrDefault(c => c.cardId == cardId && c.rank == rank);
    }
}
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

        // 모든 캐릭터 SO 로드
        var chars = Resources.LoadAll<CharacterData>("ScriptableObjects/Characters");
        // 플레이어는 항상 Mono로 고정
        playerData = chars.First(c => c.characterId == "Mono");

        // Enemy: Inspector에서 지정된 defaultEnemy가 있으면 사용, 없으면 첫번째 또는 예외 처리
        if (defaultEnemy != null)
            enemyData = defaultEnemy;
        else
            enemyData = chars.First(c => c.characterId != playerData.characterId);
    }
    
    /// <summary>
    /// 런타임에 적을 변경하고 싶을 때 호출
    /// </summary>
    public void SetEnemy(CharacterData newEnemy)
    {
        enemyData = newEnemy;
    }

    /// <summary>
    /// 플레이어 덱 구성용: ownerID가 playerData의 ID와 같고, 1성 카드만 반환
    /// </summary>
    public CardData[] GetPlayerCards()
    {
        return allCards
               .Where(c => c.ownerID == playerData.ownerID && c.rank == 1)
               .ToArray();
    }

    /// <summary>
    /// 적 스킬용: ownerID가 enemyData의 ID와 같은 모든 카드 반환
    /// </summary>
    public CardData[] GetEnemySkills()
    {
        return allCards
               .Where(c => c.ownerID == enemyData.ownerID)
               .ToArray();
    }

    /// <summary>
    /// 합성용: 특정 displayName과 rank에 해당하는 CardData 반환
    /// </summary>
    public CardData GetCard(string displayName, int rank)
    {
        var card = allCards
            .FirstOrDefault(c => 
                c.displayName == displayName &&
                c.rank        == rank);
        if (card == null)
            Debug.LogError($"GetCard 실패: '{displayName}' rank={rank} 카드가 없습니다.");
        return card;
    }
}
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("SO 로드")]
    [SerializeField] CharacterData defaultEnemy;
    
    [Serializable]
    public class SaveMetadata
    {
        public DateTime timestamp; // 저장용 시간
        public string chapterName; // 저장용 챕터 이름
        public string questName;   // 저장용 퀘스트 이름
    }
    
    [Serializable]
    public class GameState
    {
        public string sceneName;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public string currentChapter;
        public string currentQuest;
        public string[] completedQuestIds;
        // 필요하면 추가
    }
    
    public CharacterData playerData { get; private set; }
    public CharacterData enemyData { get; private set; }
    public CardData[] allCards { get; private set; }   // 모든 카드 SO
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSkillData();
        }
        else Destroy(gameObject);
    }

    void LoadAllSkillData()
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
    
    
    // 메타 정보만 불러오기
    public static SaveMetadata GetSaveMetadata(int slot)
    {
        string key = (slot == 0) ? "AutoSlot" : $"ManualSlot{slot}";
        if (!PlayerPrefs.HasKey(key + "_Timestamp")) return null;

        long ticks = Convert.ToInt64(PlayerPrefs.GetString(key + "_Timestamp"));
        var meta = new SaveMetadata
        {
            timestamp = new DateTime(ticks),
            chapterName = PlayerPrefs.GetString(key + "_Chapter"),
            questName = PlayerPrefs.GetString(key + "_Quest")
        };
        return meta;
    }

    // 실제 저장
    public static void SaveGame(int slot)
    {
        string key = (slot == 0) ? "AutoSlot" : $"ManualSlot{slot}";
        
        // 메타 정보
        PlayerPrefs.SetString(key + "_Timestamp", DateTime.Now.Ticks.ToString());
        PlayerPrefs.SetString(key + "_Chapter", CurrentChapterName());
        PlayerPrefs.SetString(key + "_Quest",   CurrentQuestName());
        
        // GameState 생성
        GameState state = new GameState {
            sceneName         = SceneManager.GetActiveScene().name,
            /*playerPosition    = Player.Instance.transform.position,
            playerRotation    = Player.Instance.transform.rotation,*/
            currentChapter    = CurrentChapterName(),
            currentQuest      = CurrentQuestName(),
            /*completedQuestIds = 퀘스트 ID를 받아올 위치*/
        };
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(key + "_Data", json);
        
        PlayerPrefs.Save();
        Debug.Log($"SaveGame: 슬롯{slot} 저장 완료 → {json}");
    }


    // 불러오기
    public static void LoadGame(int slot)
    {
        string key = (slot == 0) ? "AutoSlot" : $"ManualSlot{slot}";
        var meta = GetSaveMetadata(slot);
        if (meta == null) { Debug.LogWarning($"슬롯{slot}에 데이터 없음"); return; }
        
        if (!PlayerPrefs.HasKey(key + "_Data")) {
            Debug.LogWarning($"슬롯{slot}의 GameState 데이터 없음");
            return;
        }
        
        string json = PlayerPrefs.GetString(key + "_Data");
        GameState state = JsonUtility.FromJson<GameState>(json);

        ApplyGameState(state);
        Debug.Log($"LoadGame: 슬롯{slot} 불러옴 → 씬:{state.sceneName}");
    }
    
    private static void ApplyGameState(GameState state)
    {
        // 씬이 로드됐을 때만 복원
        void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != state.sceneName) return;
            
            // 플레이어 위치/회전
            /*Player.Instance.transform.position = state.playerPosition;
            Player.Instance.transform.rotation = state.playerRotation;*/
            
            // 2) 챕터/퀘스트 복원
            /*DataManager.Instance.SetChapter(state.currentChapter);
            DataManager.Instance.SetQuest(state.currentQuest);*/
            
            // 완료 퀘스트 복원
            
            // 더 복원할 게 있으면 추가
            
            // 등록 해제
            SceneManager.sceneLoaded -= OnLoaded;
        }
        SceneManager.sceneLoaded += OnLoaded;
        SceneManager.LoadScene(state.sceneName);
    }

    static string CurrentChapterName()
    {
        /* 지금 진행중인 챕터 이름 반환 */
        return "";
    }

    static string CurrentQuestName()
    {
        /* 지금 진행중인 퀘스트 이름 반환 */
        return "";
    }
}
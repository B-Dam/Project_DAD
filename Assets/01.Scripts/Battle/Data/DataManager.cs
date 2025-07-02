using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("SO 로드")]
    [SerializeField] CharacterDataSO defaultEnemy;
    
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
        public string[] seenDialogueIDs;
    }
    
    public CharacterDataSO playerData { get; private set; }
    public CharacterDataSO enemyData { get; private set; }
    public CardData[] allCards { get; private set; }   // 모든 카드 SO
    
    public Dictionary<string, QuestData>      questTable;
    public Dictionary<string, CharacterData>  characterTable;
    public Dictionary<string, DialogueData>  dialogueTable;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSkillData();
            LoadAllCsvData();
        }
        else Destroy(gameObject);
    }

    private void LoadAllSkillData()
    {
        // 모든 카드 SO 로드
        allCards = Resources.LoadAll<CardData>("ScriptableObjects/Cards");

        // 모든 캐릭터 SO 로드
        var chars = Resources.LoadAll<CharacterDataSO>("ScriptableObjects/Characters");
        // 플레이어는 항상 Mono로 고정
        playerData = chars.First(c => c.characterId == "Mono");

        // Enemy: Inspector에서 지정된 defaultEnemy가 있으면 사용, 없으면 첫번째 또는 예외 처리
        if (defaultEnemy != null)
            enemyData = defaultEnemy;
        else
            enemyData = chars.First(c => c.characterId != playerData.characterId);
    }
    
    private void LoadAllCsvData()
    {
        questTable     = CsvDatabase.LoadCsvDict("questDB",    f => new QuestData(f));
        characterTable = CsvDatabase.LoadCsvDict("characterDB",f => new CharacterData(f));
        dialogueTable  = CsvDatabase.LoadCsvDict("dialogueDB", f => new DialogueData(f));
        Debug.Log($"퀘스트 {questTable.Count}개, 캐릭터 {characterTable.Count}개, 대화 {dialogueTable.Count}개 로드 완료");
    }
    
    /// <summary>
    /// 런타임에 적을 변경하고 싶을 때 호출
    /// </summary>
    public void SetEnemy(CharacterDataSO newEnemy)
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
        // DataManager.SaveGame(0) 호출로 자동 저장
        string key = (slot == 0) ? "AutoSlot" : $"ManualSlot{slot}";
        
        // 메타 정보
        PlayerPrefs.SetString(key + "_Timestamp", DateTime.Now.Ticks.ToString());
        PlayerPrefs.SetString(key + "_Chapter", CurrentChapterName());
        PlayerPrefs.SetString(key + "_Quest",   CurrentQuestName());
        
        // GameState 생성
        GameState state = new GameState {
            sceneName         = SceneManager.GetActiveScene().name,
            playerPosition    = Player.Instance.transform.position,
            playerRotation    = Player.Instance.transform.rotation,
            currentChapter    = CurrentChapterName(),
            currentQuest      = CurrentQuestName(),
            seenDialogueIDs   = DialogueManager.Instance
                                               .GetAllSeenIDs()  // HashSet<string> → string[]
                                               .ToArray()
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
        if (meta == null) { 
            Debug.LogWarning($"슬롯{slot}에 데이터 없음"); 
            return; 
        }
        if (!PlayerPrefs.HasKey(key + "_Data")) {
            Debug.LogWarning($"슬롯{slot}의 GameState 데이터 없음");
            return;
        }

        string json  = PlayerPrefs.GetString(key + "_Data");
        GameState state = JsonUtility.FromJson<GameState>(json);

        // ApplyGameState 호출을 제거하고, 코루틴만 실행
        Instance.StartCoroutine(Instance.RestoreCoroutine(state));
        Debug.Log($"LoadGame: 슬롯{slot} 불러옴 → 씬:{state.sceneName}");
    }
    
    private IEnumerator RestoreCoroutine(GameState state)
    {
        // 씬 비동기 로드
        var op = SceneManager.LoadSceneAsync(state.sceneName);
        yield return op;       // 씬 로드 완료 대기
        yield return null;     // 한 프레임 더 대기

        // 플레이어 위치·회전 복원
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = state.playerPosition;
            Player.Instance.transform.rotation = state.playerRotation;
        }
        else Debug.LogWarning("Restore: Player.Instance is null");

        // 대화 진행 상태 복원
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.LoadSeenIDs(state.seenDialogueIDs);
        else
            Debug.LogWarning("Restore: DialogueManager.Instance is null");
    }
    
    static string CurrentChapterName()
    {
        // DataManager 인스턴스가 없으면 빈 문자열 반환
        if (Instance == null) return string.Empty;
        
        // DialogueManager가 없으면 빈 문자열 반환
        if (DialogueManager.Instance == null) return string.Empty;

        // CSV로 로드된 모든 퀘스트 정보
        var quests = Instance.questTable.Values;

        // 대화 진행 상태(HasSeen)를 보고, conditionStart를 봤고 conditionComplete는 아직 안 본 퀘스트 찾기
        var active = quests.FirstOrDefault(q =>
            DialogueManager.Instance.HasSeen(q.conditionStart.ToString()) &&
            !DialogueManager.Instance.HasSeen(q.conditionComplete.ToString())
        );

        // 있으면 챕터 이름 반환, 없으면 빈 문자열
        return active != null ? active.chapterName : string.Empty;
    }

    static string CurrentQuestName()
    {
        // DataManager 인스턴스가 없으면 빈 문자열 반환
        if (Instance == null) return string.Empty;
        
        // DialogueManager가 없으면 빈 문자열 반환
        if (DialogueManager.Instance == null) return string.Empty;

        // CSV로 로드된 모든 퀘스트 정보
        var quests = Instance.questTable.Values;

        // 대화 진행 상태(HasSeen)를 보고, conditionStart를 봤고 conditionComplete는 아직 안 본 퀘스트 찾기
        var active = quests.FirstOrDefault(q =>
            DialogueManager.Instance.HasSeen(q.conditionStart.ToString()) &&
            !DialogueManager.Instance.HasSeen(q.conditionComplete.ToString())
        );

        // 있으면 퀘스트 이름 반환, 없으면 빈 문자열
        return active != null ? active.questName : string.Empty;
    }
}
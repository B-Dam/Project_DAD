using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveWrapper
{
    public int          version = 1;
    public SaveEntry[]  entries;
}

[Serializable]
public class SaveEntry
{
    public string id;
    public string json;
}

public class SaveLoadManagerCore : MonoBehaviour
{
    public static SaveLoadManagerCore Instance { get; private set; }

    // 저장파일 이름 패턴
    const string FILE_PATTERN = "slot_{0}.json";

    // UniqueID ISaveable 매핑
    Dictionary<string, ISaveable> saveables = new Dictionary<string, ISaveable>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// 씬의 모든 ISaveable(Active/Inactive 구분 없이)을 한 번에 스캔해서 등록
    /// 저장/로드 전 반드시 호출
    /// </summary>
    public void RegisterSaveables()
    {
        saveables.Clear();
        var all = FindObjectsOfType<MonoBehaviour>(true)
                   .OfType<ISaveable>();
        foreach (var sv in all)
        {
            var id = sv.UniqueID;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[SaveCore] 빈 UniqueID: {sv.GetType()} on {((MonoBehaviour)sv).gameObject.name}");
                continue;
            }
            if (saveables.ContainsKey(id))
            {
                Debug.LogWarning($"[SaveCore] 중복 UniqueID: {id}");
                continue;
            }
            saveables[id] = sv;
        }
        Debug.Log($"[SaveCore] ISaveable 총 {saveables.Count}개 등록됨");
    }

    /// <summary>slotIndex에 저장</summary>
    public void SaveGame(int slotIndex)
    {
        RegisterSaveables();

        // 각 ISaveable의 CaptureState() 결과를 JSON 문자열로 직렬화
        var entries = saveables
            .Select(kv =>
            {
                var stateObj = kv.Value.CaptureState();
                var json     = JsonUtility.ToJson(stateObj);
                return new SaveEntry { id = kv.Key, json = json };
            })
            .ToArray();

        // 래퍼에 담아 한 번 더 직렬화
        var wrapper = new SaveWrapper { version = 1, entries = entries };
        var wrapperJson = JsonUtility.ToJson(wrapper, true);

        // 파일 쓰기
        var path = Path.Combine(Application.persistentDataPath,
                                string.Format(FILE_PATTERN, slotIndex));
        File.WriteAllText(path, wrapperJson);

        Debug.Log($"[SaveCore] 슬롯 {slotIndex} 저장 → {path}");
    }

    /// <summary>slotIndex에서 불러오기</summary>
    public void LoadGame(int slotIndex)
    {
        var path = Path.Combine(Application.persistentDataPath,
                                string.Format(FILE_PATTERN, slotIndex));
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveCore] 파일 없음: {path}");
            return;
        }

        // 래퍼 JSON 읽어 역직렬화
        var wrapperJson = File.ReadAllText(path);
        var wrapper     = JsonUtility.FromJson<SaveWrapper>(wrapperJson);

        // 버전 체크
        if (wrapper.version != 1)
            Debug.LogWarning($"[SaveCore] 버전 불일치: {wrapper.version}");

        // ISaveable 재등록
        RegisterSaveables();

        // 각 엔트리별 RestoreState 호출
        foreach (var entry in wrapper.entries)
        {
            if (saveables.TryGetValue(entry.id, out var sv))
            {
                sv.RestoreState(entry.json);
            }
            else
            {
                Debug.LogWarning($"[SaveCore] 복원 대상 없음 ID={entry.id}");
            }
        }

        Debug.Log($"[SaveCore] 슬롯 {slotIndex} 불러오기 완료 ← {path}");
    }
}

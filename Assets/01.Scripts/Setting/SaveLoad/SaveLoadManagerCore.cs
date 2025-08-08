using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveEntry { public string id; public string json; }

[Serializable]
public class SaveWrapper
{
    public int version = 1;
    public SaveEntry[] entries;
    // tombstone(파괴된 ID) 등 추가하고 싶으면 여기에
}

public class SaveLoadManagerCore : MonoBehaviour
{
    public static SaveLoadManagerCore Instance { get; private set; }

    const string FILE_PATTERN = "slot_{0}.json";

    Dictionary<string, ISaveable> saveables = new();
    readonly Dictionary<string, string> pending = new(); // 아직 씬에 없는 ID

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void RegisterSaveables()
    {
        saveables.Clear();
        var all = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
        int cnt = 0;
        foreach (var sv in all)
        {
            string id = sv.UniqueID;
            if (string.IsNullOrEmpty(id)) { Debug.LogWarning($"[SaveCore] 빈 ID: {((MonoBehaviour)sv).name}"); continue; }
            if (saveables.ContainsKey(id)) { Debug.LogWarning($"[SaveCore] 중복 ID: {id}"); continue; }
            saveables[id] = sv; cnt++;
        }
        Debug.Log($"[SaveCore] ISaveable 총 {cnt}개 등록됨");
    }

    public void SaveGame(int slotIndex)
    {
        RegisterSaveables();

        var entries = saveables.Select(kv => new SaveEntry {
            id   = kv.Key,
            json = JsonUtility.ToJson(kv.Value.CaptureState())
        }).ToArray();

        var wrapper = new SaveWrapper { version = 1, entries = entries };
        var json = JsonUtility.ToJson(wrapper, true);

        var path = Path.Combine(Application.persistentDataPath, string.Format(FILE_PATTERN, slotIndex));
        File.WriteAllText(path, json);
        Debug.Log($"[SaveCore] 슬롯 {slotIndex} 저장 → {path}");
    }

    public void LoadGame(int slotIndex)
    {
        StartCoroutine(LoadRoutine(slotIndex));
    }

    IEnumerator LoadRoutine(int slotIndex)
    {
        var path = Path.Combine(Application.persistentDataPath, string.Format(FILE_PATTERN, slotIndex));
        if (!File.Exists(path)) { Debug.LogWarning($"[SaveCore] 파일 없음: {path}"); yield break; }

        var wrapper = JsonUtility.FromJson<SaveWrapper>(File.ReadAllText(path));

        // 한 프레임 대기: 씬의 Awake/Start/OnEnable 완료 보장
        yield return null;

        RegisterSaveables();
        pending.Clear();

        // 1패스: 환경 우선(Hole/Trigger/Quest/Dialogue 등)
        int applied1 = 0, applied2 = 0;
        foreach (var e in wrapper.entries)
        {
            if (TryGet(e.id, out var sv) && IsEnvironment(sv))
            {
                sv.RestoreState(e.json);
                applied1++;
            }
        }

        // 2패스: 동적(박스/NPC/플레이어/카메라 등)
        foreach (var e in wrapper.entries)
        {
            if (TryGet(e.id, out var sv) && !IsEnvironment(sv))
            {
                sv.RestoreState(e.json);
                applied2++;
            }
            else if (!saveables.ContainsKey(e.id))
            {
                pending[e.id] = e.json; // 나중에 등장하면 적용
            }
        }
        Debug.Log($"[SaveCore] Restore 적용: env={applied1}, dynamic={applied2}, pending={pending.Count}");

        // 다음 프레임에 PostLoad 훅 호출(겹침 정리 등)
        yield return null;
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
            if (mb is IPostLoad post) post.OnPostLoad();

        // 혹시 늦게 생성된 오브젝트가 있다면 한 번 더 시도(선택)
        if (pending.Count > 0)
        {
            RegisterSaveables();
            TryApplyPending();
        }
    }

    bool TryGet(string id, out ISaveable sv) => saveables.TryGetValue(id, out sv);

    void TryApplyPending()
    {
        var keys = pending.Keys.ToList();
        int applied = 0;
        foreach (var k in keys)
        {
            if (saveables.TryGetValue(k, out var sv))
            {
                sv.RestoreState(pending[k]);
                pending.Remove(k);
                applied++;
            }
        }
        if (applied > 0) Debug.Log($"[SaveCore] pending 적용: {applied}, 남음={pending.Count}");
    }
    
    public bool HasSaveFile(int slotIndex)
    {
        var path = System.IO.Path.Combine(
            Application.persistentDataPath,
            string.Format(FILE_PATTERN, slotIndex));
        return System.IO.File.Exists(path);
    }

    // 타입으로 간단 분류: 먼저 복원돼야 안정적인 애들
    bool IsEnvironment(ISaveable sv) =>
        sv is HoleSave || sv is MapTriggerSave || sv is QuestItemSave || sv is DialogueSave;
}

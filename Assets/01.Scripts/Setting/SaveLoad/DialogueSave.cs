using System;
using UnityEngine;

// 저장할 데이터: 지금까지 본 대화 ID 목록
[Serializable]
public struct DialogueSaveData
{
    public string[] seenIDs;
}

public class DialogueSave : MonoBehaviour, ISaveable
{
    UniqueID idComp;

    private void Awake()
    {
        idComp = GetComponent<UniqueID>();
    }

    // ISaveable.UniqueID 구현
    public string UniqueID => idComp.ID;

    public object CaptureState()
    {
        // DialogueManager.Instance.GetAllSeenIDs()가 string[]을 반환
        return new DialogueSaveData {
            seenIDs = DialogueManager.Instance.GetAllSeenIDs()
        };
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<DialogueSaveData>(json);
        DialogueManager.Instance.LoadSeenIDs(data.seenIDs);
    }
}
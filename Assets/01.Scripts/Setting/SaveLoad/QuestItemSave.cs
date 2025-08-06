using System;
using UnityEngine;

// 저장할 데이터
[Serializable]
public struct QuestItemData
{
    public bool isActive;
}

public class QuestItemSave : MonoBehaviour, ISaveable
{
    [SerializeField]
    private string uniqueID;
    public string UniqueID => uniqueID;

    // 저장할 때 호출: 활성화 상태만 담아서 반환
    public object CaptureState()
    {
        return new QuestItemData
        {
            isActive = gameObject.activeSelf
        };
    }

    // 로드할 때 호출: JSON 문자열을 구조체로 파싱 후 복원
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<QuestItemData>(json);
        gameObject.SetActive(data.isActive);
    }
}
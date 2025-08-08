using System;
using UnityEngine;

// 저장할 NPC 상태를 담는 데이터 구조체
[Serializable]
struct NPCData
{
    public bool isActive;
    public Vector3 position;
}

// NPC 세이브
[RequireComponent(typeof(UniqueID))]
public class NPCSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;

    public string UniqueID
    {
        get
        {
            if (idComp == null)
            {
                idComp = GetComponent<UniqueID>() ?? gameObject.AddComponent<UniqueID>();
            }
            return idComp.ID;
        }
    }

    // NPC 상태 저장
    public object CaptureState()
    {
        // NPC가 활성화 상태인지, 위치 값은 어디인지 저장
        return new NPCData
        {
            isActive = gameObject.activeSelf,
            position = transform.position
        };
    }

    // NPC 상태 로드
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        // JSON > NPCData 변환
        var data = JsonUtility.FromJson<NPCData>(json);

        // 상태 복원
        gameObject.SetActive(data.isActive);
        transform.position = data.position;
    }
}
using System;
using UnityEngine;

// 저장할 플레이어 상태 데이터
[Serializable]
public struct PlayerData
{
    public Vector3 position;
    public Quaternion rotation;
}

public class PlayerSave : MonoBehaviour, ISaveable
{
    UniqueID idComp;

    private void Awake()
    {
        idComp = GetComponent<UniqueID>();
    }

    // ISaveable.UniqueID 구현
    public string UniqueID => idComp.ID;


    // 저장 시 호출: 위치·회전 정보를 담아 반환
    public object CaptureState()
    {
        return new PlayerData
        {
            position = transform.position,
            rotation = transform.rotation
        };
    }

    // 로드 시 호출: JSON 문자열로 넘어온 데이터를 파싱해 복원
    public void RestoreState(object state)
    {
        var json = state as string;
        Debug.Log($"[RestoreState] {UniqueID} → json={json}");
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<PlayerData>(json);
        transform.position = data.position;
        transform.rotation = data.rotation;
    }
}
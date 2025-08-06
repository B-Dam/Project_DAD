using System;
using UnityEngine;

[Serializable]
public struct BoxData
{
    public bool    isActive;
    public Vector3 position;
}

public class BoxSave : MonoBehaviour, ISaveable
{
    [SerializeField] private string uniqueID;
    public string UniqueID => uniqueID;

    // 저장 시: 활성화 여부와 위치값을 BoxData로 반환
    public object CaptureState()
    {
        return new BoxData
        {
            isActive = gameObject.activeSelf,
            position = transform.position
        };
    }

    // 로드 시: JSON 문자열을 BoxData로 파싱한 뒤 복원
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<BoxData>(json);
        gameObject.SetActive(data.isActive);
        transform.position = data.position;
    }
}
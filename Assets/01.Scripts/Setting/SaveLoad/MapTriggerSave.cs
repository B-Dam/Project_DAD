using System;
using UnityEngine;

// 저장할 MapTrigger 상태를 담는 데이터 구조체
[Serializable]
struct MapTriggerData
{
    public bool isActive;
}

// 맵 트리거 세이브
public class MapTriggerSave : MonoBehaviour, ISaveable
{
    [SerializeField] private string uniqueID;
    public string UniqueID => uniqueID;

    public object CaptureState()
    {
        // isActvie에 GameObject의 활성화 상태를 저장
        return new MapTriggerData
        {
            isActive = gameObject.activeSelf
        };
    }

    public void RestoreState(object state)
    {
        // state는 JSON 문자열이므로 string으로 캐스팅
        var json = state as string;

        // 다시 MapTriggerData로 파싱
        var data = JsonUtility.FromJson<MapTriggerData>(json);
        gameObject.SetActive(data.isActive);
    }
}
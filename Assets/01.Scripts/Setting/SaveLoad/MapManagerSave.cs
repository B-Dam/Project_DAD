using System;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
public class MapManagerSave : MonoBehaviour, ISaveable
{
    [Serializable]
    struct Data
    {
        public string currentID;
        public string prevID;
    }

    UniqueID uid;
    public string UniqueID => (uid ??= GetComponent<UniqueID>()).ID;

    public object CaptureState()
    {
        var mm = MapManager.Instance;
        return new Data {
            currentID = mm != null ? mm.currentMapID : null,
            prevID    = mm != null ? mm.prevMapID    : null
        };
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<Data>(json);
        if (MapManager.Instance == null) return;
        
        // 직접 값을 세팅하고 코루틴을 돌리는 대신, MapManager에게 모든 작업을 위임
        // 씬이 완전히 로드되고 다른 객체들이 준비될 시간을 벌기 위해 한 프레임 뒤에 호출
        StartCoroutine(Co_Apply(data));
    }

    private System.Collections.IEnumerator Co_Apply(Data data)
    {
        // 다른 ISaveable들이 모두 Restore될 때까지 한 프레임 대기
        yield return null; 

        if (MapManager.Instance != null && !string.IsNullOrEmpty(data.currentID))
        {
            MapManager.Instance.SwitchMapAndApplySettings(data.currentID);
        }
    }
}
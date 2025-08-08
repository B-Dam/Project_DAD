using System;
using UnityEngine;

[Serializable]
public struct TriggerData
{
    public bool isActive;
}

[RequireComponent(typeof(UniqueID))]
public class MapTriggerSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;
    public string UniqueID
    {
        get
        {
            if (idComp == null)
            {
                idComp = GetComponent<UniqueID>();
                if (idComp == null) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    public object CaptureState() => new TriggerData
    {
        isActive = gameObject.activeSelf,
    };

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<TriggerData>(json);

        if (gameObject.activeSelf != data.isActive)
            gameObject.SetActive(data.isActive);
    }
}
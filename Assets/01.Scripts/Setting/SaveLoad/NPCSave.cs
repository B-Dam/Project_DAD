using System;
using UnityEngine;

[Serializable]
public struct NPCData
{
    public bool isActive;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

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
                idComp = GetComponent<UniqueID>();
                if (idComp == null) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    public object CaptureState() => new NPCData
    {
        isActive = gameObject.activeSelf,
        position = transform.position,
        rotation = transform.rotation,
        scale    = transform.localScale
    };

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<NPCData>(json);

        if (gameObject.activeSelf != data.isActive)
            gameObject.SetActive(data.isActive);

        transform.position   = data.position;
        transform.rotation   = data.rotation;
        transform.localScale = data.scale;
    }
}
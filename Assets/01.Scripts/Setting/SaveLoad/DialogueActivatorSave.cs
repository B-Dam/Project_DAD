using System;
using UnityEngine;

[Serializable]
public struct DialogueActivatorState
{
    public bool hasTriggered;
}

[DisallowMultipleComponent]
[RequireComponent(typeof(UniqueID))]
public class DialogueActivatorSave : MonoBehaviour, ISaveable
{
    // 같은 오브젝트에 있으면 비워도 됩니다.
    [SerializeField] private DialogueTriggerCondition target;

    private UniqueID _id;
    private const string PrivateFieldName = "hasTriggered";

    public string UniqueID
    {
        get
        {
            if (!_id) _id = GetComponent<UniqueID>();
            return _id ? _id.ID : null;
        }
    }

    void Awake()
    {
        if (!target) target = GetComponent<DialogueTriggerCondition>();
    }

    public object CaptureState()
    {
        var data = new DialogueActivatorState
        {
            hasTriggered = ReadHasTriggeredSafely()
        };
        // SaveCore는 string(json)으로 저장하므로 Json으로 반환
        return JsonUtility.ToJson(data);
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<DialogueActivatorState>(json);
        WriteHasTriggeredSafely(data.hasTriggered);
    }

    // ----- private helpers -----

    bool ReadHasTriggeredSafely()
    {
        if (!target) return false;
        var f = typeof(DialogueTriggerCondition).GetField(
            PrivateFieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        if (f == null) return false;
        return (bool)f.GetValue(target);
    }

    void WriteHasTriggeredSafely(bool value)
    {
        if (!target) return;
        var f = typeof(DialogueTriggerCondition).GetField(
            PrivateFieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        if (f == null) return;
        f.SetValue(target, value);
    }
}
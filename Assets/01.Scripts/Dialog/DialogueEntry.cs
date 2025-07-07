using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueEntry
{
    public string id;
    public string speaker;

    [TextArea(2, 4)]
    public string text;
    public Transform focusTarget;
    
    public UnityEvent onEndEvents;
    public void OnDialogueEnd()
    {
        onEndEvents?.Invoke();
    }
}
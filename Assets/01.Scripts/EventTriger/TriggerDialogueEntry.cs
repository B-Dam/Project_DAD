using UnityEngine;

[System.Serializable]
public class TriggerDialogueEntry
{
    public string id;
    public string speaker;

    [TextArea(2, 3)]
    public string text;
    public Transform focusTarget;
}

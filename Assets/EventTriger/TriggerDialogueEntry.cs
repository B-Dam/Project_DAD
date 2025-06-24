using UnityEngine;

[System.Serializable]
public class TriggerDialogueEntry
{
    public string id;

    [TextArea(2, 3)]
    public string text;
}

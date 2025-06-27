using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public string id;
    public string speaker;

    [TextArea(2, 4)]
    public string text;
    public Transform focusTarget;
}


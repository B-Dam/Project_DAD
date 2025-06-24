using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public string id;

    [TextArea(2, 4)]
    public string text;
}

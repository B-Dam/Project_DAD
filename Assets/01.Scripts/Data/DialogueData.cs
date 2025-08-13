using System;

[Serializable]
public class DialogueData
{
    public string dialogueId;
    public string speakerId;
    public string dialogueText;

    public DialogueData(string[] f)
    {
        dialogueId      = f[0];
        speakerId       = f[1];
        dialogueText    = f[2];
    }
}
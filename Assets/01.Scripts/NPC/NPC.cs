using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("🗣️ ID + 문장을 하나로 관리")]
    public DialogueEntry[] dialogueEntries;

    public void Interact()
    {
        if (DialogueManager.Instance == null) return;
        if (DialogueManager.Instance.IsDialogueActive) return;
        if (DialogueManager.Instance.IsOnCooldown) return;

        string[] ids = new string[dialogueEntries.Length];
        for (int i = 0; i < dialogueEntries.Length; i++)
        {
            ids[i] = dialogueEntries[i].id;
        }

        DialogueManager.Instance.StartDialogueByIDs(ids);
    }
}

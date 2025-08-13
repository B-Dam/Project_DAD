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
        if (CombatManager.Instance != null && CombatManager.Instance.IsInCombat) return;

        DialogueManager.Instance.StartDialogueWithEntries(dialogueEntries);
    }
}
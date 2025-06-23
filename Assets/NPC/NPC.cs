using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [TextArea(2, 5)]
    public string[] dialogueLines;

    public void Interact()
    {
        if (DialogueManager.Instance == null) return;

        if (DialogueManager.Instance.IsDialogueActive)
        {
            Debug.Log("⚠️ 대화 중입니다. 무시됨.");
            return;
        }

        if (DialogueManager.Instance.IsOnCooldown)
        {
            Debug.Log("⏳ 대화 쿨타임 중입니다. 잠시 기다리세요.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueLines);
    }
}


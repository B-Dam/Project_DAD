using UnityEngine;

public class EventTriggerZone : MonoBehaviour
{
    [Header("🧩 발동 조건")]
    public bool triggerOnce = true;
    public bool requireCondition = false;

    [Header("🆔 트리거 고유 ID")]
public string triggerId;

    [Header("🗣️ 트리거 대사 (ID + 내용)")]
    public TriggerDialogueEntry[] triggerDialogueEntries;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        if (requireCondition && !CheckCondition()) return;

        hasTriggered = true;
        TriggerEvent();
    }

    private void TriggerEvent()
    {
        if (triggerDialogueEntries == null || triggerDialogueEntries.Length == 0) return;

        string[] ids = new string[triggerDialogueEntries.Length];
        for (int i = 0; i < triggerDialogueEntries.Length; i++)
        {
            ids[i] = triggerDialogueEntries[i].id;
        }

        DialogueManager.Instance.StartDialogueByIDs(ids);
    }

    private bool CheckCondition()
    {
        return true;
    }
}

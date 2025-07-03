using System.Collections.Generic;
using UnityEngine;

public class EventTriggerZone : MonoBehaviour
{
    // todo 나중에 세이브 기능 만들면 수정해야함
    private static List<string> _triggeredList = new();


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
        // 이미 재생했던 트리거 리스트에 triggerId가 포함되어있다면,
        // 함수를 바로 종료
        if (_triggeredList.Contains(triggerId))
            return;
        
        // 재생한 트리거 리스트에 triggerId Add
        _triggeredList.Add(triggerId);
        
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
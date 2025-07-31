using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class EventTriggerZone : MonoBehaviour
{
    // todo 나중에 세이브 기능 만들면 수정해야함
    private static List<string> _triggeredList = new();

    private int dialogueIndex = 0;


    [Header("🧩 발동 조건")]
    public bool triggerOnce = true;
    public bool requireCondition = false;

    [Header("🆔 트리거 고유 ID")]
    public string triggerId;

    [Header("🗣️ 트리거 대사 (ID + 내용)")]
    public TriggerDialogueEntry[] triggerDialogueEntries;

    private bool hasTriggered = false;
    
    public static EventTriggerZone Instance { get; private set; }

private void Awake()
{
    Instance = this;
}
public static bool InstanceExists => Instance != null;


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

    dialogueIndex = 0;

    // === [PATCH] DialogueManager에 triggerDialogueEntries 직접 전달 ===
    DialogueManager.Instance.StartDialogueWithEntries(ConvertTriggerEntriesToDialogueEntries(triggerDialogueEntries));

    DialogueManager.Instance.RegisterOnDialogueEndCallback(HandleDialogueEntryEnd);

    // ✅ 최초 대사 시작 이벤트 실행
    HandleDialogueEntryStart();
}

// [PATCH] TriggerDialogueEntry[] → DialogueEntry[] 변환 메서드 추가
private DialogueEntry[] ConvertTriggerEntriesToDialogueEntries(TriggerDialogueEntry[] triggerEntries)
{
    DialogueEntry[] entries = new DialogueEntry[triggerEntries.Length];
    for (int i = 0; i < triggerEntries.Length; i++)
    {
        TriggerDialogueEntry triggerEntry = triggerEntries[i];
        DialogueEntry entry = new DialogueEntry
        {
            id = triggerEntry.id,
            speaker = triggerEntry.speaker,
            text = triggerEntry.text,
            focusTarget = triggerEntry.focusTarget,
            shakeCutscene = triggerEntry.shakeCutscene,
            leftSprite = triggerEntry.leftSprite,
            rightSprite = triggerEntry.rightSprite,
            onStartEvents = triggerEntry.onStartEvents,
            onEndEvents = triggerEntry.onEndEvents
        };
        entries[i] = entry;
    }
    return entries;
}

private void HandleDialogueEntryStart()
{
    if (dialogueIndex >= triggerDialogueEntries.Length) return;

    var entry = triggerDialogueEntries[dialogueIndex];
    entry.OnDialogueStart(); // ✅ 실행
}


   private void HandleDialogueEntryEnd()
{
    // 현재 대사 종료 이벤트 실행
    if (dialogueIndex < triggerDialogueEntries.Length)
    {
        var entry = triggerDialogueEntries[dialogueIndex];
        entry.OnDialogueEnd();
    }

    // 인덱스 증가
    dialogueIndex++;

    // 모든 대사를 다 본 경우
    if (dialogueIndex >= triggerDialogueEntries.Length)
    {
        DialogueManager.Instance.ClearOnDialogueEndCallback(HandleDialogueEntryEnd);

        // 마지막 대화가 끝난 시점 → 플레이어 뒤로 물러나기
        StartCoroutine(MovePlayerBackward(1.5f, 0.5f));
    }
}


/// <summary>
/// 플레이어를 트리거 기준 뒤로 이동
/// </summary>
private IEnumerator MovePlayerBackward(float distance, float duration)
{
    if (PlayerController.Instance == null) yield break;

    PlayerController.Instance.enabled = false;

    Vector3 triggerPos = transform.position;
    Vector3 playerPos = PlayerController.Instance.transform.position;

    // 트리거에서 플레이어 방향으로 계산 → 뒤로 이동
    Vector3 backwardDir = (playerPos - triggerPos).normalized;

    Vector3 startPos = playerPos;
    Vector3 targetPos = startPos + backwardDir * distance;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        PlayerController.Instance.transform.position = Vector3.Lerp(startPos, targetPos, t);
        yield return null;
    }

    PlayerController.Instance.transform.position = targetPos;
    PlayerController.Instance.enabled = true;
}



    private bool CheckCondition()
    {
        return true;
    }
}
이벤트트리거 코드

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
     public string deactivateDialogueID;
    
    public static EventTriggerZone Instance { get; private set; }

private void Awake()
{
    Instance = this;
}
public static bool InstanceExists => Instance != null;

private void Update()
{
    if (!string.IsNullOrEmpty(deactivateDialogueID) &&
        DialogueManager.Instance != null &&
        DialogueManager.Instance.HasSeen(deactivateDialogueID))
    {
        gameObject.SetActive(false);
    }
}



    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어만 감지
        if (!other.CompareTag("Player")) return;

        // 조건 체크 (예: 퀘스트 조건 등)
        if (requireCondition && !CheckCondition()) return;

        // triggerOnce 옵션이 true일 때만 _triggeredList 사용
        if (triggerOnce)
        {
            if (_triggeredList.Contains(triggerId))
                return;

            _triggeredList.Add(triggerId);
        }

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
/// <summary>
/// 플레이어를 트리거의 가장 가까운 면 방향으로 뒤로 이동
/// </summary>
private IEnumerator MovePlayerBackward(float distance, float duration)
{
    if (PlayerController.Instance == null) yield break;

    // 이동 시작 시 플레이어 제어 비활성화
    PlayerController.Instance.enabled = false;

    Animator anim = PlayerController.Instance.GetComponent<Animator>();
    if (anim != null) anim.SetFloat("Run", 1f); // 강제 이동 애니메이션

    Vector3 playerPos = PlayerController.Instance.transform.position;
    Vector3 backwardDir = GetPushDirection(playerPos); // 면 기준 방향 계산

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

    if (anim != null) anim.SetFloat("Run", 0f);

    PlayerController.Instance.enabled = true;
}

/// <summary>
/// 트리거 Collider의 가장 가까운 면 방향으로 밀림 방향 계산
/// </summary>
private Vector3 GetPushDirection(Vector3 playerPos)
{
    // 트리거 Collider2D의 Bounds 가져오기
    Collider2D col = GetComponent<Collider2D>();
    if (col == null) return Vector3.zero;

    Bounds bounds = col.bounds;

    // 각 면과의 거리 계산
    float leftDist = Mathf.Abs(playerPos.x - bounds.min.x);
    float rightDist = Mathf.Abs(playerPos.x - bounds.max.x);
    float bottomDist = Mathf.Abs(playerPos.y - bounds.min.y);
    float topDist = Mathf.Abs(playerPos.y - bounds.max.y);

    // 가장 가까운 면 찾기
    float minDist = Mathf.Min(leftDist, rightDist, bottomDist, topDist);

    if (minDist == leftDist)
        return Vector3.left;
    else if (minDist == rightDist)
        return Vector3.right;
    else if (minDist == bottomDist)
        return Vector3.down;
    else
        return Vector3.up;
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

    
    // 마지막 대사 종료 시 → 뒤로 이동
    if (dialogueIndex >= triggerDialogueEntries.Length)
    {
        // triggerOnce 체크된 경우 뒤로 이동 스킵
        if (!(triggerOnce && _triggeredList.Contains(triggerId)))
        {
            StartCoroutine(MovePlayerBackward(1.3f, 2f));
        }

        DialogueManager.Instance.ClearOnDialogueEndCallback(HandleDialogueEntryEnd);
    }
}


private void OnEnable()
{
    CheckDeactivateCondition();

    // 대화 종료될 때마다 다시 체크
    if (DialogueManager.Instance != null)
        DialogueManager.Instance.RegisterOnDialogueEndCallback(CheckDeactivateCondition);
}

private void OnDisable()
{
    if (DialogueManager.Instance != null)
        DialogueManager.Instance.ClearOnDialogueEndCallback(CheckDeactivateCondition);
}

private void CheckDeactivateCondition()
{
    if (!string.IsNullOrEmpty(deactivateDialogueID) &&
        DialogueManager.Instance != null &&
        DialogueManager.Instance.HasSeen(deactivateDialogueID))
    {
        gameObject.SetActive(false);
    }
}



    private bool CheckCondition()
    {
        return true;
    }
}

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


다이얼로그 매니저 코드


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueDatabase.DialogueLine[] currentDialogueLines;
    private string[] currentDialogueIDs;
    private int dialogueIndex = 1;
    private bool isDialogueActive = false;
    
    // 대화 ID별로 본 여부만 저장
    private HashSet<string> seenIDs = new HashSet<string>();
    
    // NPC 대화 엔트리 배열 저장
    private DialogueEntry[] currentDialogueEntries;

    [Header("UI 컴포넌트")]
    public GameObject dialoguePanel;
    public UnityEngine.UI.Image dialogBoxBackgroundImage;
    public TMPro.TextMeshProUGUI speakerText;
    public TMPro.TextMeshProUGUI dialogueText;

    [Header("👥 좌우 캐릭터 이미지")]
public UnityEngine.UI.Image leftCharacterImage;
public UnityEngine.UI.Image rightCharacterImage;

    [Header("깜빡이는 이미지")]
    public UnityEngine.UI.Image uxBlinkImage;
    private Coroutine blinkCoroutine;

    [Header("쿨타임 관련")]
    private float lastDialogueEndTime = -999f;
    public float dialogueCooldown = 0.2f;
    private float dialogueInputDelay = 0.1f;
    private float dialogueStartTime;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string fullText = "";
    private float typingSpeed = 0.02f; // 글자당 시간 (조절 가능)

    private Action onDialogueEndCallback;

    public UnityEngine.UI.Image cutsceneBackgroundImage; 


    [Header("컷씬 이미지")]
    public UnityEngine.UI.Image cutsceneImage;

    [Header("컷신 대사")]
    public CutsceneDialogue cutsceneDialogue;
    private bool isDisplayingBlackPanelDialogue = false;
    private bool isFading = false;
    private bool isBattle = false;

    public string CurrentDialogueID { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        if (uxBlinkImage != null)
            uxBlinkImage.gameObject.SetActive(false); // 처음엔 꺼두기
              dialogueText.text = "리치 <color=red>텍스트</color> 테스트";
    }

    private void Update()
    {
        if (!isDialogueActive) return;
        if (CutsceneController.Instance != null && (CutsceneController.Instance.IsVideoPlaying || CutsceneController.Instance.IsPreparing)) return;
        if (isFading) return;

        Debug.Log($"[Update] 상태 체크: isDisplayingBlackPanelDialogue = {isDisplayingBlackPanelDialogue}");

        if (isDisplayingBlackPanelDialogue && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space 누를 수 있는 상태1");
            bool shown = ShowBlackPanelDialogue();
            if (!shown)
            {
                isDisplayingBlackPanelDialogue = false;
                StopBlinkUX();
                cutsceneDialogue.Hide();
                StartCoroutine(EndBlackPanelAndContinue());
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - dialogueStartTime > dialogueInputDelay)
        {
            Debug.Log("Space 누를 수 있는 상태2");

            if (isTyping)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                dialogueText.text = fullText;
                isTyping = false;
                StartBlinkUX(); // 즉시 깜빡임 시작
            }
            else
            {
                ShowNextLine();
            }
        }
    }
    
    /// <summary>
    /// NPC에서 대화 엔트리 배열을 직접 넘겨 받을 때 사용
    /// </summary>
    public void StartDialogueWithEntries(DialogueEntry[] entries)
    {
        if (entries == null || entries.Length == 0) return;
        if (Time.time - lastDialogueEndTime < dialogueCooldown) return;

        currentDialogueEntries = entries;
        // ID만 뽑아서 기존 StartDialogueByIDs 호출
        string[] ids = entries.Select(e => e.id).ToArray();
        StartDialogueByIDs(ids);
    }

    public void StartDialogueByIDs(string[] dialogueIDs)
    {
        if (dialogueIDs == null || dialogueIDs.Length == 0) return;

        currentDialogueIDs = dialogueIDs;
        currentDialogueLines = new DialogueDatabase.DialogueLine[dialogueIDs.Length];
        for (int i = 0; i < dialogueIDs.Length; i++)
        {
            currentDialogueLines[i] = DialogueDatabase.Instance.GetLineById(dialogueIDs[i]);
        }

        StartDialogue(currentDialogueLines);
    }

    public void StartDialogue(DialogueDatabase.DialogueLine[] lines)
    {
        if (Time.time - lastDialogueEndTime < dialogueCooldown)
        {
            Debug.Log("⏳ 대화 쿨타임 중입니다.");
            return;
        }

        if (lines == null || lines.Length == 0) return;

        currentDialogueLines = lines;
        dialogueIndex = 0;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        DisplayCurrentLine();
        dialogueStartTime = Time.time;
    }

 private void ShowNextLine()
{
    StopBlinkUX();

    DialogueEntry entry = null;
    if (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
        entry = currentDialogueEntries[dialogueIndex];

    // ✅ 이벤트 먼저 실행 (Shake 등)
    if (entry != null && entry.onEndEvents.GetPersistentEventCount() > 0)
    {
        entry.OnDialogueEnd();
    }

    dialogueIndex++;

    if (dialogueIndex >= currentDialogueLines.Length)
    {
        EndDialogue();
    }
    else
    {
        DisplayCurrentLine();
    }

    //onDialogueEndCallback?.Invoke();
}

    
  public void ResumeDialogue()
{
    if (currentDialogueLines == null || currentDialogueLines.Length == 0)
    {
        EndDialogue(); // 완전 초기화
        return;
    }

    if (dialogueIndex >= currentDialogueLines.Length)
    {
        EndDialogue();
        return;
    }

    isDialogueActive = true;
    dialoguePanel.SetActive(true);

    // === [PATCH] 전투 복귀 시 스프라이트 복원 ===
    if (leftCharacterImage != null)
    {
        leftCharacterImage.sprite = (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
            ? currentDialogueEntries[dialogueIndex].leftSprite
            : (EventTriggerZone.InstanceExists && EventTriggerZone.Instance.triggerDialogueEntries.Length > dialogueIndex
                ? EventTriggerZone.Instance.triggerDialogueEntries[dialogueIndex].leftSprite
                : null);

        leftCharacterImage.gameObject.SetActive(leftCharacterImage.sprite != null);
    }

    if (rightCharacterImage != null)
    {
        rightCharacterImage.sprite = (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
            ? currentDialogueEntries[dialogueIndex].rightSprite
            : (EventTriggerZone.InstanceExists && EventTriggerZone.Instance.triggerDialogueEntries.Length > dialogueIndex
                ? EventTriggerZone.Instance.triggerDialogueEntries[dialogueIndex].rightSprite
                : null);

        rightCharacterImage.gameObject.SetActive(rightCharacterImage.sprite != null);
    }

    DisplayCurrentLine();
    dialogueStartTime = Time.time;
}

    private bool ShowBlackPanelDialogue()
    {
        if (currentDialogueIDs == null || dialogueIndex >= currentDialogueIDs.Length)
            return false;

        var line = currentDialogueLines[dialogueIndex];
        string id = currentDialogueIDs[dialogueIndex];

        bool isBlackPanel = cutsceneDialogue != null && cutsceneDialogue.blackPanelDialogueID.Contains(id);
        Debug.Log($"[ShowBlackPanelDialogue] ID: {id}, isBlackPanel: {isBlackPanel}, Index: {dialogueIndex}");

        if (!isBlackPanel)
            return false;

        cutsceneDialogue.ShowDialogue(id, line.speaker, line.text);
        dialogueIndex++;
        return true;
    }

    private IEnumerator EndBlackPanelAndContinue()
    {
        isFading = true;
        StopBlinkUX();

        int prevIndex = dialogueIndex - 1;
        DialogueEntry currentEntry = (currentDialogueEntries != null && prevIndex >= 0 && prevIndex < currentDialogueEntries.Length) ? currentDialogueEntries[prevIndex] : null;

        if (currentEntry != null && currentEntry.onEndEvents.GetPersistentEventCount() > 0)
        {
            isBattle = true;
            Debug.Log($"[EndBlackPanelAndContinue] 종료 이벤트 실행: {currentDialogueIDs[dialogueIndex]}");
            currentEntry.OnDialogueEnd();
        }

        bool nextIsCutscene = false;
        if (currentDialogueLines != null && dialogueIndex < currentDialogueLines.Length)
        {
            var nextLine = currentDialogueLines[dialogueIndex];
            nextIsCutscene = !string.IsNullOrEmpty(nextLine.spritePath) && nextLine.spritePath.StartsWith("Cutscenes/Video/");
        }
        yield return StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(nextIsCutscene));
        EndVideo();

        isFading = false;
    }

    private void EndVideo()
    {
        Debug.Log($"[EndVideo] 진입 시 dialogueIndex = {dialogueIndex}");

        if (CutsceneController.Instance != null && (CutsceneController.Instance.IsVideoPlaying || CutsceneController.Instance.IsPreparing))
        {
            Debug.Log("[EndVideo] 영상이 아직 재생 중이므로 종료 처리 중단");
            return;
        }

        if (currentDialogueLines == null || dialogueIndex >= currentDialogueLines.Length)
        {
            Debug.Log("[EndVideo] 다음 대사가 없음");
            EndDialogue();
            return;
        }

        var id = currentDialogueIDs[dialogueIndex];

        if (cutsceneDialogue != null && cutsceneDialogue.blackPanelDialogueID.Contains(id))
        {
            Debug.Log($"[EndVideo] 다음 대사 ID = {id}, isBlackPanel = {cutsceneDialogue.blackPanelDialogueID.Contains(id)}");
            isDisplayingBlackPanelDialogue = true;
            bool shown = ShowBlackPanelDialogue();
            if (!shown)
            {
                isDisplayingBlackPanelDialogue = false;
                cutsceneDialogue.Hide();
                StartCoroutine(EndBlackPanelAndContinue());
            }

            if (!CutsceneController.Instance.IsPreparing)
            {
                StartBlinkUX();
            }

            return;
        }
        if (!isBattle)
        {
            DisplayCurrentLine();
        }
    }


    private void DisplayCurrentLine()
{
        Debug.Log($"[DisplayCurrentLine] 호출됨, dialogueIndex = {dialogueIndex}");

        DialogueEntry entry = currentDialogueEntries != null
        ? currentDialogueEntries[dialogueIndex]
        : null;

    if (entry != null && entry.onStartEvents.GetPersistentEventCount() > 0)
        entry.OnDialogueStart();

    if (EventTriggerZone.InstanceExists)
    {
        TriggerDialogueEntry[] triggerEntries = EventTriggerZone.Instance?.triggerDialogueEntries;
        if (triggerEntries != null && dialogueIndex < triggerEntries.Length)
        {
            var triggerEntry = triggerEntries[dialogueIndex];
            if (triggerEntry != null && triggerEntry.onStartEvents.GetPersistentEventCount() > 0)
                triggerEntry.OnDialogueStart();
        }
    }

    var line = currentDialogueLines[dialogueIndex];
    speakerText.text = line.speaker;

    Sprite leftSprite = null;
    Sprite rightSprite = null;

    if (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
    {
        leftSprite = currentDialogueEntries[dialogueIndex].leftSprite;
        rightSprite = currentDialogueEntries[dialogueIndex].rightSprite;
    }
    else if (EventTriggerZone.InstanceExists)
    {
        var triggerEntries = EventTriggerZone.Instance.triggerDialogueEntries;
        if (triggerEntries != null && dialogueIndex < triggerEntries.Length)
        {
            leftSprite = triggerEntries[dialogueIndex].leftSprite;
            rightSprite = triggerEntries[dialogueIndex].rightSprite;
        }
    }
       
        string id = currentDialogueIDs[dialogueIndex];
        if (!string.IsNullOrEmpty(line.spritePath) && line.spritePath.StartsWith("Cutscenes/Video/"))
        {
          Debug.Log($"[DisplayCurrentLine] 컷신 시작: ID = {id}, path = {line.spritePath}");

            StopBlinkUX();
            dialogueIndex++;
            cutsceneBackgroundImage.gameObject.SetActive(false);
            cutsceneImage.gameObject.SetActive(false);

            CutsceneController.Instance.PlayVideo(line.spritePath, EndVideo);
            dialoguePanel.SetActive(false);
            return;
        }
        else dialoguePanel.SetActive(true);



      // === 좌측 캐릭터 스프라이트 처리 ===
if (leftSprite != null)
{
    if (leftCharacterImage.sprite != leftSprite)
    {
        leftCharacterImage.sprite = leftSprite;
        leftCharacterImage.gameObject.SetActive(true);

        // 레이아웃 강제 갱신 후 DropInEffect 실행
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(leftCharacterImage.rectTransform);

        PlayDropInEffect(leftCharacterImage.rectTransform);
    }
}
else
{
    leftCharacterImage.sprite = null;
    leftCharacterImage.gameObject.SetActive(false);
}

// === 우측 캐릭터 스프라이트 처리 ===
if (rightSprite != null)
{
    if (rightCharacterImage.sprite != rightSprite)
    {
        rightCharacterImage.sprite = rightSprite;
        rightCharacterImage.gameObject.SetActive(true);

        // 레이아웃 강제 갱신 후 DropInEffect 실행
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rightCharacterImage.rectTransform);

        PlayDropInEffect(rightCharacterImage.rectTransform);
    }
}
else
{
    rightCharacterImage.sprite = null;
    rightCharacterImage.gameObject.SetActive(false);
}



    // 대화 ID 기록
    if (currentDialogueIDs != null && dialogueIndex < currentDialogueIDs.Length)
        {
            CurrentDialogueID = currentDialogueIDs[dialogueIndex];
            seenIDs.Add(currentDialogueIDs[dialogueIndex]);
        }

    // 컷신 이미지 처리 (기존 로직 유지)
    bool hasCutscene = !string.IsNullOrEmpty(line.spritePath);
    if (hasCutscene)
    {
        Sprite sprite = Resources.Load<Sprite>(line.spritePath);
        if (sprite != null)
        {
            cutsceneImage.sprite = sprite;
            cutsceneImage.gameObject.SetActive(true);

            if (cutsceneBackgroundImage != null)
                cutsceneBackgroundImage.gameObject.SetActive(true);

            bool shouldShake = false;
            if (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
            {
                shouldShake = currentDialogueEntries[dialogueIndex].shakeCutscene;
            }
            else if (EventTriggerZone.InstanceExists)
            {
                var triggerEntries = EventTriggerZone.Instance?.triggerDialogueEntries;
                if (triggerEntries != null && dialogueIndex < triggerEntries.Length)
                    shouldShake = triggerEntries[dialogueIndex].shakeCutscene;
            }

            if (shouldShake)
                ShakeCutsceneImage(0.3f, 20f);
        }
        else
        {
            Debug.LogWarning($"컷신 이미지 로드 실패: {line.spritePath}");
            cutsceneImage.gameObject.SetActive(false);
            if (cutsceneBackgroundImage != null)
                cutsceneBackgroundImage.gameObject.SetActive(false);
        }
    }
    else
    {
        cutsceneImage.gameObject.SetActive(false);
        if (cutsceneBackgroundImage != null)
            cutsceneBackgroundImage.gameObject.SetActive(false);
    }

    // 다이얼로그 박스 투명도
    if (dialogBoxBackgroundImage != null)
    {
        Color color = dialogBoxBackgroundImage.color;
        color.a = hasCutscene ? 0f : 1f;
        dialogBoxBackgroundImage.color = color;
    }

    // 타이핑 효과
    if (typingCoroutine != null)
        StopCoroutine(typingCoroutine);
    typingCoroutine = StartCoroutine(TypeText(line.text));
}


   private IEnumerator TypeText(string text)
{
    isTyping = true;
    fullText = text;
    dialogueText.text = "";
    if (uxBlinkImage != null) uxBlinkImage.gameObject.SetActive(false);

    int i = 0;
    while (i < text.Length)
    {
        // 🔍 리치 텍스트 태그 처리 시작
        if (text[i] == '<')
        {
            int tagEnd = text.IndexOf('>', i);
            if (tagEnd != -1)
            {
                string tag = text.Substring(i, tagEnd - i + 1);
                dialogueText.text += tag;
                i = tagEnd + 1;
                continue;
            }
        }

        // 일반 문자 하나 출력
        dialogueText.text += text[i];
        i++;

        yield return new WaitForSeconds(typingSpeed);
    }

    isTyping = false;
    StartBlinkUX();
}


    private void StartBlinkUX()
    {
        Debug.Log("[StartBlinkUX] 호출됨");
        if (uxBlinkImage == null) return;

        uxBlinkImage.gameObject.SetActive(true);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkUX());
    }

    public void StopBlinkUX()
    {
        Debug.Log("[StopBlinkUX] 호출됨");
        if (uxBlinkImage == null) return;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        uxBlinkImage.gameObject.SetActive(false);
    }

    private IEnumerator BlinkUX()
    {
        while (true)
        {
            uxBlinkImage.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.5f);
            uxBlinkImage.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void EndDialogue(bool clearState = true)
{
    isDialogueActive = false;
    dialoguePanel.SetActive(false);
    lastDialogueEndTime = Time.time;

        StopBlinkUX();

    // 컷신 이미지 비활성화
    if (cutsceneImage != null)
        cutsceneImage.gameObject.SetActive(false);

    if (cutsceneBackgroundImage != null)
        cutsceneBackgroundImage.gameObject.SetActive(false);

    // 스프라이트 초기화
    if (leftCharacterImage != null)
    {
        leftCharacterImage.sprite = null;
        leftCharacterImage.gameObject.SetActive(false);
    }

    if (rightCharacterImage != null)
    {
        rightCharacterImage.sprite = null;
        rightCharacterImage.gameObject.SetActive(false);
    }
        //  현재 종료된 마지막 대사 ID 저장
        if (currentDialogueIDs != null && currentDialogueIDs.Length > 0)
        {
            CurrentDialogueID = currentDialogueIDs[currentDialogueIDs.Length - 1];
        }

        // 콜백 실행
        onDialogueEndCallback?.Invoke();
        onDialogueEndCallback = null;

        // **Combat 전환 시점에서는 clearState = false로 호출하여 데이터 유지**
        if (clearState)
    {
        currentDialogueEntries = null;
        currentDialogueLines = null;
        currentDialogueIDs = null;
        dialogueIndex = 0;
    }
}



    
    /// <summary>
    /// 주어진 ID를 본 적이 있는지 반환
    /// </summary>
    public bool HasSeen(string id)
    {
        return seenIDs.Contains(id);
    }
    
    public string[] GetAllSeenIDs()
    {
        return seenIDs.ToArray();
    }
    
    public void LoadSeenIDs(string[] ids)
    {
        seenIDs.Clear();
        foreach (var id in ids)
            seenIDs.Add(id);
    }

    public void RegisterOnDialogueEndCallback(Action callback)
{
    onDialogueEndCallback += callback;
}

    public void ClearOnDialogueEndCallback(Action callback)
    {
        onDialogueEndCallback -= callback;
    }

private Coroutine cutsceneShakeCoroutine;

/// <summary>
/// cutsceneImage가 보이는 경우 UI 흔들림 실행
/// </summary>
private void ShakeCutsceneImage(float duration = 0.3f, float magnitude = 10f)
{
    if (cutsceneImage == null || !cutsceneImage.gameObject.activeInHierarchy)
        return;

    if (cutsceneShakeCoroutine != null)
        StopCoroutine(cutsceneShakeCoroutine);

    cutsceneShakeCoroutine = StartCoroutine(ShakeCutsceneRoutine(duration, magnitude));
}

private IEnumerator ShakeCutsceneRoutine(float duration, float magnitude)
{
    RectTransform rt = cutsceneImage.rectTransform;
    Vector2 originalPos = rt.anchoredPosition;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        float offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude;
        rt.anchoredPosition = originalPos + new Vector2(0f, offsetY);
        elapsed += Time.deltaTime;
        yield return null;
    }

    rt.anchoredPosition = originalPos;
    cutsceneShakeCoroutine = null;
}

private void PlayDropInEffect(RectTransform target)
{
    if (target == null) return;

    StartCoroutine(DropInAnimation(target));
}

private IEnumerator DropInAnimation(RectTransform target)
{
    // 위치 갱신 강제
    Canvas.ForceUpdateCanvases();
    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(target);

    Vector2 originalPos = target.anchoredPosition;
    Vector2 startPos = originalPos + new Vector2(0f, -120f); 
    Vector2 overshootPos = originalPos + new Vector2(0f, 25f);

    float duration1 = 0.08f;
    float duration2 = 0.05f;
    float elapsed = 0f;

    // 1단계: 위에서 아래로 떨어짐
    target.anchoredPosition = startPos;
    while (elapsed < duration1)
    {
        float t = elapsed / duration1;
        target.anchoredPosition = Vector2.Lerp(startPos, overshootPos, t);
        elapsed += Time.deltaTime;
        yield return null;
    }

    target.anchoredPosition = overshootPos;

    // 2단계: 살짝 위로 되돌아감
    elapsed = 0f;
    while (elapsed < duration2)
    {
        float t = elapsed / duration2;
        target.anchoredPosition = Vector2.Lerp(overshootPos, originalPos, t);
        elapsed += Time.deltaTime;
        yield return null;
    }

    target.anchoredPosition = originalPos;
}

public void HideDialogueSprites()
{
    if (leftCharacterImage != null)
    {
        leftCharacterImage.sprite = null;
        leftCharacterImage.gameObject.SetActive(false);
    }

    if (rightCharacterImage != null)
    {
        rightCharacterImage.sprite = null;
        rightCharacterImage.gameObject.SetActive(false);

    }
}

    public bool IsDialogueActive => isDialogueActive;
    public bool IsOnCooldown => Time.time - lastDialogueEndTime < dialogueCooldown;
}





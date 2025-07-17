using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueDatabase.DialogueLine[] currentDialogueLines;
    private string[] currentDialogueIDs;
    private int dialogueIndex = 0;
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

  

    [Header("깜빡이는 이미지")]
    public UnityEngine.UI.Image uxBlinkImage;
    private Coroutine blinkCoroutine;

    [Header("쿨타임 관련")]
    private float lastDialogueEndTime = -999f;
    public float dialogueCooldown = 0.5f;
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
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - dialogueStartTime > dialogueInputDelay)
        {
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

    onDialogueEndCallback?.Invoke();
}

    
   public void ResumeDialogue()
{
    if (dialogueIndex >= currentDialogueLines.Length)
    {
        Debug.LogWarning("❗ 대사 인덱스 초과로 Resume 실패");
        EndDialogue();
        return;
    }

    isDialogueActive = true;
    dialoguePanel.SetActive(true);
    DisplayCurrentLine();
    dialogueStartTime = Time.time;
}


    private void DisplayCurrentLine()
    {
        // currentDialogueEntries는 StartDialogueWithEntries에서만 사용됨
       DialogueEntry entry = currentDialogueEntries != null
    ? currentDialogueEntries[dialogueIndex]
    : null;

if (entry != null && entry.onStartEvents.GetPersistentEventCount() > 0)
{
    entry.OnDialogueStart(); // ✅ 대사 시작 시 실행
}

 if (EventTriggerZone.InstanceExists)
    {
        TriggerDialogueEntry[] triggerEntries = EventTriggerZone.Instance?.triggerDialogueEntries;
        if (triggerEntries != null && dialogueIndex < triggerEntries.Length)
        {
            var triggerEntry = triggerEntries[dialogueIndex];
            if (triggerEntry != null && triggerEntry.onStartEvents.GetPersistentEventCount() > 0)
            {
                triggerEntry.OnDialogueStart();
            }
        }
    }
        
        var line = currentDialogueLines[dialogueIndex];
        speakerText.text = line.speaker;
        
        // 대화 본 ID만 기록 (StartDialogueByIDs 통해 설정된 ID 사용)
        if (currentDialogueIDs != null && dialogueIndex < currentDialogueIDs.Length)
        {
            seenIDs.Add(currentDialogueIDs[dialogueIndex]);
        }

bool hasCutscene = !string.IsNullOrEmpty(line.spritePath);
if (hasCutscene)
{
    Sprite sprite = Resources.Load<Sprite>(line.spritePath);
    if (sprite != null)
    {
        cutsceneImage.sprite = sprite;
        cutsceneImage.gameObject.SetActive(true);

        // ✅ 검정 배경도 함께 활성화
        if (cutsceneBackgroundImage != null)
            cutsceneBackgroundImage.gameObject.SetActive(true);

        // 흔들림 조건 체크
        bool shouldShake = false;
        if (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
        {
            shouldShake = currentDialogueEntries[dialogueIndex].shakeCutscene;
        }
        else if (EventTriggerZone.InstanceExists)
        {
            var triggerEntries = EventTriggerZone.Instance?.triggerDialogueEntries;
            if (triggerEntries != null && dialogueIndex < triggerEntries.Length)
            {
                shouldShake = triggerEntries[dialogueIndex].shakeCutscene;
            }
        }

        if (shouldShake)
        {
            ShakeCutsceneImage(0.3f, 20f);
        }
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



        // ✅ 다이얼로그 박스 배경만 투명도 조절
        if (dialogBoxBackgroundImage != null)
        {
            Color color = dialogBoxBackgroundImage.color;
            color.a = hasCutscene ? 0f : 1f;
            dialogBoxBackgroundImage.color = color;
        }

        // 대사 출력 (타이핑)
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
        
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        dialogueText.text = "";
        if (uxBlinkImage != null) uxBlinkImage.gameObject.SetActive(false);

        foreach (var ch in text)
        {
            dialogueText.text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        StartBlinkUX();
    }

    private void StartBlinkUX()
    {
        if (uxBlinkImage == null) return;

        uxBlinkImage.gameObject.SetActive(true);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkUX());
    }

    private void StopBlinkUX()
    {
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

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        lastDialogueEndTime = Time.time;

        StopBlinkUX();
        
        // 컷씬 이미지도 끄기
        if (cutsceneImage != null)
            cutsceneImage.gameObject.SetActive(false);
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


    public bool IsDialogueActive => isDialogueActive;
    public bool IsOnCooldown => Time.time - lastDialogueEndTime < dialogueCooldown;
}

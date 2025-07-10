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
        
        // 현재 엔트리 안전하게 얻기
        DialogueEntry entry = null;
        if (currentDialogueEntries != null && dialogueIndex < currentDialogueEntries.Length)
            entry = currentDialogueEntries[dialogueIndex];

        // CombatTriggerEvent가 연결된 엔트리일 경우 전투 트리거 및 대화 일시정지
        if (entry != null && entry.onEndEvents.GetPersistentEventCount() > 0)
        {
            // 전투 트리거
            entry.OnDialogueEnd();

            // 다음 대사로 이어지도록 인덱스 미리 증가
            dialogueIndex++;

            // 대화 일시정지
            isDialogueActive = false;
            dialoguePanel.SetActive(false);
            return;
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
    }
    
    public void ResumeDialogue()
    {
        if (dialogueIndex < currentDialogueLines.Length)
        {
            isDialogueActive = true;
            dialoguePanel.SetActive(true);
            DisplayCurrentLine();
            dialogueStartTime = Time.time;
        }
    }

    private void DisplayCurrentLine()
    {
        // currentDialogueEntries는 StartDialogueWithEntries에서만 사용됨
        DialogueEntry entry = currentDialogueEntries != null
            ? currentDialogueEntries[dialogueIndex]
            : null;
        
        var line = currentDialogueLines[dialogueIndex];
        speakerText.text = line.speaker;
        
        // 대화 본 ID만 기록 (StartDialogueByIDs 통해 설정된 ID 사용)
        if (currentDialogueIDs != null && dialogueIndex < currentDialogueIDs.Length)
        {
            seenIDs.Add(currentDialogueIDs[dialogueIndex]);
        }

        // 컷신 이미지 출력 처리
        bool hasCutscene = !string.IsNullOrEmpty(line.spritePath);
        if (hasCutscene)
        {
            Sprite sprite = Resources.Load<Sprite>(line.spritePath);
            if (sprite != null)
            {
                cutsceneImage.sprite = sprite;
                cutsceneImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"컷신 이미지 로드 실패: {line.spritePath}");
                cutsceneImage.gameObject.SetActive(false);
            }
        }
        else
        {
            cutsceneImage.gameObject.SetActive(false);
        }

        // ✅ 다이얼로그 박스 배경만 투명도 조절
        if (dialogBoxBackgroundImage != null)
        {
            Color color = dialogBoxBackgroundImage.color;
            color.a = hasCutscene ? 0.2f : 1f;
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

    public bool IsDialogueActive => isDialogueActive;
    public bool IsOnCooldown => Time.time - lastDialogueEndTime < dialogueCooldown;
}

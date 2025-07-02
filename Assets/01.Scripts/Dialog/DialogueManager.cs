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

    private void DisplayCurrentLine()
    {
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
            color.a = hasCutscene ? 0f : 1f;
            dialogBoxBackgroundImage.color = color;
        }

        // 대사 출력 (타이핑)
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        dialogueText.text = "";

        if (uxBlinkImage != null)
            uxBlinkImage.gameObject.SetActive(false); // 타이핑 중 숨김

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        StartBlinkUX(); // 타이핑 끝났으므로 UX 시작
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

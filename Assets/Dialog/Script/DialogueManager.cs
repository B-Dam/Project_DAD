using UnityEngine;
using System.Collections;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueDatabase.DialogueLine[] currentDialogueLines;
    private int dialogueIndex = 0;
    private bool isDialogueActive = false;

    public GameObject dialoguePanel;
    public UnityEngine.UI.Image dialogBoxBackgroundImage;
    public TMPro.TextMeshProUGUI speakerText;
    public TMPro.TextMeshProUGUI dialogueText;

    // ✅ 쿨타임 관련
    private float lastDialogueEndTime = -999f;
    public float dialogueCooldown = 0.5f;

    private float dialogueInputDelay = 0.1f;
    private float dialogueStartTime;

    private Coroutine typingCoroutine;
private bool isTyping = false;
private string fullText = "";
private float typingSpeed = 0.02f; // 글자당 시간 (조절 가능)

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
    }

   private void Update()
{
    if (!isDialogueActive) return;

    if (Input.GetKeyDown(KeyCode.Space) && Time.time - dialogueStartTime > dialogueInputDelay)
    {
        if (isTyping)
        {
            // 전체 문장을 즉시 출력
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = fullText;
            isTyping = false;
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

    foreach (char c in text)
    {
        dialogueText.text += c;
        yield return new WaitForSeconds(typingSpeed);
    }

    isTyping = false;
}


    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        lastDialogueEndTime = Time.time;
    }

    

    public bool IsDialogueActive => isDialogueActive;
    public bool IsOnCooldown => Time.time - lastDialogueEndTime < dialogueCooldown;
}

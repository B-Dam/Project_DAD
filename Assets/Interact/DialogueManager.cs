using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private string[] currentDialogueLines;
    private int dialogueIndex = 0;
    private bool isDialogueActive = false;

    public GameObject dialoguePanel;
    public TMPro.TextMeshProUGUI dialogueText;

    // ✅ 쿨타임 관련
    private float lastDialogueEndTime = -999f;
    public float dialogueCooldown = 0.5f;

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
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(string[] lines)
    {
        // ✅ 쿨타임 검사
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
        dialogueText.text = currentDialogueLines[dialogueIndex];
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
            dialogueText.text = currentDialogueLines[dialogueIndex];
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        lastDialogueEndTime = Time.time; // ✅ 종료 시간 기록
    }

    public bool IsDialogueActive => isDialogueActive;

    public bool IsOnCooldown => Time.time - lastDialogueEndTime < dialogueCooldown; // ⏱️ 외부 접근용
}

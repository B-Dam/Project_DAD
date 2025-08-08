using System;
using UnityEngine;
using System.Linq;
using static DialogueDatabase;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueSession session;
    private DialogueEntry[] currentEntries;
    public Action onDialogueEndCallback;
    public static event Action OnDialogueEnded;

    [Header("쿨타임 관련")]
    private float lastDialogueEndTime = -999f;
    public float dialogueCooldown = 0.2f;
    private float dialogueInputDelay = 0.2f;
    private float dialogueStartTime;

    private bool isInputLocked = false;
    private bool isDialogueActive = false;
    private bool isFading = false;

    public string CurrentDialogueID => session?.CurrentID;
    public bool IsDialogueActive => isDialogueActive;
    public bool IsOnCooldown => Time.time - lastDialogueEndTime < dialogueCooldown;
    public DialogueSession Session => session;
    public DialogueDatabase.DialogueLine GetCurrentLine() => session.GetLine(session.CurrentIndex);
    public void UnlockInput() => isInputLocked = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (!isDialogueActive) return;
        if (CutsceneController.Instance != null && (CutsceneController.Instance.IsVideoPlaying || CutsceneController.Instance.IsPreparing)) return;
        if (isFading) return;
        if (isInputLocked) return;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - dialogueStartTime > dialogueInputDelay)
        {
            isInputLocked = true;
            DialogueUIDisplayer.Instance.StopBlinkUX();
            DialogueFlowController.Instance.HandleSpaceInput();
        }
    }

    public void StartDialogueWithEntries(DialogueEntry[] entries)
    {
        if (entries == null || entries.Length == 0 || IsOnCooldown) return;

        currentEntries = entries;
        string[] ids = entries.Select(e => e.id).ToArray();
        StartDialogueByIDs(ids);
    }

    public void StartDialogueByIDs(string[] dialogueIDs)
    {
        if (dialogueIDs == null || dialogueIDs.Length == 0) return;
        
        // 이미 본 대화 ID는 필터링
        var toPlayIds = dialogueIDs
                        .Where(id => !HasSeen(id))   // 또는 DialogueManager.Instance.HasSeen(id)
                        .ToArray();
        
        // 필터 후 남은 게 없으면 아무것도 하지 않음
        if (toPlayIds.Length == 0) return;
        
        // 실제 라인으로 변환해 세션 생성
        var lines = dialogueIDs.Select(id => DialogueDatabase.Instance.GetLineById(id)).ToArray();
        var newSession = new DialogueSession(lines, dialogueIDs);
        
        // 대화 시작
        StartDialogue(newSession);
    }

    public void StartDialogue(DialogueSession newSession)
    {
        if (IsOnCooldown || newSession == null || newSession.IsEmpty) return;

        session = newSession;
        isDialogueActive = true;
        dialogueStartTime = Time.time;

        DialogueUIDisplayer.Instance.ShowPanel();
        DisplayCurrentLine();
    }

    public void DisplayCurrentLine()
    {
        if (session == null || session.IsComplete) return;

        string id = session.CurrentID;
        DialogueLine line = session.GetLine(session.CurrentIndex);

        if (CutsceneDialogueUI.Instance.TryDisplayBlackPanelDialogue(id))
        {
            session.MarkSeen();
            return;
        }

        DialogueEntry entry = (currentEntries != null && session.CurrentIndex < currentEntries.Length) ? currentEntries[session.CurrentIndex] : null;

        entry?.OnDialogueStart();

        var zone = EventTriggerZone.Instance;
        if (zone != null)
        {
            var triggerEntry = EventTriggerZone.Instance?.triggerDialogueEntries?.ElementAtOrDefault(session.CurrentIndex);

            if (triggerEntry != null)
            {
                triggerEntry.OnDialogueStart();
            }
        }

        if (!string.IsNullOrEmpty(line.spritePath) && line.spritePath.StartsWith("Cutscenes/Video/"))
        {
            DialogueUIDisplayer.Instance.StopBlinkUX();
            DialogueUIDisplayer.Instance.HidePanel();
            CutsceneController.Instance.PlayVideo(line.spritePath, OnCutsceneEnded);
            return;
        }

        bool shouldShake = entry?.shakeCutscene ?? false;
        Sprite left = entry?.leftSprite;
        Sprite right = entry?.rightSprite;


        DialogueUIDisplayer.Instance.DisplayLine(line, left, right, shouldShake);
        session.MarkSeen();
        QuestGuideUI.Instance.RefreshQuest();
        UnlockInput();
    }

    public void ShowNextLine()
    {
        if (session == null || session.IsComplete)
        {
            EndDialogue();
            return;
        }

        session.MoveNext();

        if (session.IsComplete)
        {
            EndDialogue();
        }
        else
        {
            DisplayCurrentLine();
            UnlockInput();
        }
    }

    public void ResumeDialogue()
    {
        if (session == null || session.IsEmpty || session.IsComplete)
        {
            EndDialogue();
            return;
        }

        isDialogueActive = true;
        dialogueStartTime = Time.time;

        DialogueUIDisplayer.Instance.ShowPanel();
        QuestGuideUI.Instance.questUI.SetActive(true);

        Sprite left = null, right = null;
        if (currentEntries != null && session.CurrentIndex < currentEntries.Length)
        {
            left = currentEntries[session.CurrentIndex].leftSprite;
            right = currentEntries[session.CurrentIndex].rightSprite;
        }

        DialogueUIDisplayer.Instance.RestoreCharacterSprites(left, right);
        ShowNextLine();
    }

    public void EndDialogue(bool clearState = true)
    {
        isDialogueActive = false;
        lastDialogueEndTime = Time.time;

        DialogueUIDisplayer.Instance.ClearUI();

        onDialogueEndCallback?.Invoke();
        OnDialogueEnded?.Invoke();

        onDialogueEndCallback = null;

        if (clearState)
        {
            session = null;
            currentEntries = null;
        }
    }

    public void OnCutsceneEnded()
    {
        session.MarkSeen();
        session.MoveNext();

        if (session.IsComplete)
        {
            StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(false, () => { EndDialogue(); }, true));
            return;
        }

        string nextID = session.CurrentID;
        var nextLine = session.GetLine(session.CurrentIndex);

        if (CutsceneDialogueUI.Instance.TryDisplayBlackPanelDialogue(nextID)) return;

        if (!string.IsNullOrEmpty(nextLine?.spritePath) && nextLine.spritePath.StartsWith("Cutscenes/Video/"))
        {
            StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(true, () => { DisplayCurrentLine(); }, true));
            return;
        }

        StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(false, () => { DialogueUIDisplayer.Instance.ShowPanel(); DisplayCurrentLine(); }, true));
    }

    public DialogueEntry GetCurrentEntry()
    {
        return (currentEntries != null && session != null && session.CurrentIndex < currentEntries.Length)
            ? currentEntries[session.CurrentIndex]
            : null;
    }

    public void RegisterOnDialogueEndCallback(Action callback)
    {
        onDialogueEndCallback += callback;
    }

    public void ClearOnDialogueEndCallback(Action callback)
    {
        onDialogueEndCallback -= callback;
    }

    public bool HasSeen(string id) => session?.HasSeen(id) ?? false;
    public string[] GetAllSeenIDs() => session?.GetSeenIDs() ?? Array.Empty<string>();
    public void LoadSeenIDs(string[] ids) => session?.LoadSeenIDs(ids);
}
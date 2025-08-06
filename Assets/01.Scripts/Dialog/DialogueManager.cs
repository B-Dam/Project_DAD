using System;
using UnityEngine;
using System.Linq;
using static DialogueDatabase;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueSession session;
    private DialogueEntry[] currentEntries;
    private Action onDialogueEndCallback;

    [Header("쿨타임 관련")]
    private float lastDialogueEndTime = -999f;
    public float dialogueCooldown = 0.2f;
    private float dialogueInputDelay = 2f;
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

        var lines = dialogueIDs.Select(id => DialogueDatabase.Instance.GetLineById(id)).ToArray();
        var newSession = new DialogueSession(lines, dialogueIDs);
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
        if (session == null || session.IsComplete)
        {
            Debug.LogWarning("[DialogueManager] DisplayCurrentLine 호출 - session이 null이거나 완료됨");
            return;
        }

        string id = session.CurrentID;
        DialogueLine line = session.GetLine(session.CurrentIndex);

        if (CutsceneDialogueUI.Instance.TryDisplayBlackPanelDialogue(id))
        {
            Debug.Log($"[DialogueManager] 블랙 패널 대사 출력됨: {id}");
            return;
        }

        Debug.Log($"[DialogueManager] DisplayCurrentLine 호출됨: ID = {id}, speaker = {line.speaker}");

        DialogueEntry entry = (currentEntries != null && session.CurrentIndex < currentEntries.Length) ? currentEntries[session.CurrentIndex] : null;

        if (entry != null) Debug.Log($"[DialogueManager] DialogueEntry 존재: shake = {entry.shakeCutscene}");

        entry?.OnDialogueStart();

        var zone = EventTriggerZone.Instance;
        if (zone != null)
        {
            var triggerEntry = EventTriggerZone.Instance?.triggerDialogueEntries?.ElementAtOrDefault(session.CurrentIndex);

            if (triggerEntry != null)
            {
                Debug.Log("[DialogueManager] TriggerEntry onStartEvents 실행");
                triggerEntry.OnDialogueStart();
            }
        }

        if (!string.IsNullOrEmpty(line.spritePath) && line.spritePath.StartsWith("Cutscenes/Video/"))
        {
            Debug.Log($"[DialogueManager] 컷신 영상 재생: {line.spritePath}");
            DialogueUIDisplayer.Instance.ClearUI();
            DialogueUIDisplayer.Instance.HidePanel();
            CutsceneController.Instance.PlayVideo(line.spritePath, OnCutsceneEnded);
            return;
        }

        bool shouldShake = entry?.shakeCutscene ?? false;
        Sprite left = entry?.leftSprite;
        Sprite right = entry?.rightSprite;
        Debug.Log($"[DialogueManager] 일반 대사 출력: speaker = {line.speaker}, shake = {shouldShake}, text = {line.text}");


        DialogueUIDisplayer.Instance.DisplayLine(line, left, right, shouldShake);
        UnlockInput();
    }

    public void ShowNextLine()
    {
        if (session == null || session.IsComplete)
        {
            EndDialogue();
            return;
        }

        currentEntries?.ElementAtOrDefault(session.CurrentIndex)?.OnDialogueEnd();

        session.MarkSeen();
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

        Sprite left = null, right = null;
        if (currentEntries != null && session.CurrentIndex < currentEntries.Length)
        {
            left = currentEntries[session.CurrentIndex].leftSprite;
            right = currentEntries[session.CurrentIndex].rightSprite;
        }

        DialogueUIDisplayer.Instance.RestoreCharacterSprites(left, right);
        DisplayCurrentLine();
    }

    public void EndDialogue(bool clearState = true)
    {
        isDialogueActive = false;
        lastDialogueEndTime = Time.time;

        DialogueUIDisplayer.Instance.ClearUI();
        DialogueUIDisplayer.Instance.StopBlinkUX();

        onDialogueEndCallback?.Invoke();
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
            EndDialogue();
            return;
        }

        string nextID = session.CurrentID;
        var nextLine = session.GetLine(session.CurrentIndex);

        if (CutsceneDialogueUI.Instance.TryDisplayBlackPanelDialogue(nextID))
        {
            Debug.Log($"[DialogueManager] OnCutsceneFullyEnded - 블랙 패널 대사: {nextID}");
            return;
        }

        if (!string.IsNullOrEmpty(nextLine?.spritePath) && nextLine.spritePath.StartsWith("Cutscenes/Video/"))
        {
            Debug.Log($"[DialogueManager] OnCutsceneFullyEnded - 다음 컷신 대기: {nextID}");
            StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(true, () => {DisplayCurrentLine();}));
            return;
        }

        Debug.Log($"[DialogueManager] OnCutsceneEnded - 일반 대사: {nextID}");
        StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(false, () => {DialogueUIDisplayer.Instance.ShowPanel(); DisplayCurrentLine();}));
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
using UnityEngine;

public class DialogueFlowController : MonoBehaviour
{
    public static DialogueFlowController Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void HandleSpaceInput()
    {
        if (!DialogueManager.Instance.IsDialogueActive)
        {
            DialogueUIDisplayer.Instance.StopBlinkUX();
            return;
        }

        if (DialogueUIDisplayer.Instance.IsTyping)
        {
            DialogueUIDisplayer.Instance.FinishTyping();
            return;
        }

        if (CutsceneDialogueUI.Instance.IsActive)
        {
            CutsceneDialogueUI.Instance.Hide();

            var session = DialogueManager.Instance.Session;
            var currentIndex = session.CurrentIndex + 1;

            DialogueEntry entry = DialogueManager.Instance.GetCurrentEntry();
            if (entry != null && entry.onEndEvents.GetPersistentEventCount() > 0)
            {
                entry.OnDialogueEnd();
                DialogueUIDisplayer.Instance.ClearUI();
                StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(false, () => { CombatTriggerEvent.Instance.TriggerCombat(); }, false));
                return;
            }

            var nextLine = session.HasIndex(currentIndex) ? session.GetLine(currentIndex) : null;
            var nextID = session.HasIndex(currentIndex) ? session.GetID(currentIndex) : null;

            if (!string.IsNullOrEmpty(nextLine?.spritePath) && nextLine.spritePath.StartsWith("Cutscenes/Video/"))
            {
                DialogueManager.Instance.StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(true, () => { DialogueManager.Instance.ShowNextLine(); DialogueManager.Instance.UnlockInput(); }, true));
            }
            else if (CutsceneDialogueUI.Instance.blackPanelDialogueID.Contains(nextID))
            {
                DialogueManager.Instance.ShowNextLine();
                DialogueManager.Instance.UnlockInput();
            }
            else
            {
                DialogueManager.Instance.StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(false, () => { DialogueManager.Instance.ShowNextLine(); DialogueManager.Instance.UnlockInput(); }, true));
            }

            return;
        }

        var line = DialogueManager.Instance.GetCurrentLine();
        if (!string.IsNullOrEmpty(line?.spritePath) && line.spritePath.StartsWith("Cutscenes/Video/"))
            return;
        var normalEntry = DialogueManager.Instance.GetCurrentEntry();
        if (normalEntry != null && normalEntry.onEndEvents.GetPersistentEventCount() > 0)
        {
            normalEntry.OnDialogueEnd();
            DialogueUIDisplayer.Instance.HidePanel();
            DialogueUIDisplayer.Instance.StopBlinkUX();
            QuestGuideUI.Instance.questUI.SetActive(false);
            CutsceneController.Instance.cutsceneVideo.SetActive(false);
        }

        DialogueManager.Instance.ShowNextLine();
        DialogueManager.Instance.UnlockInput();
    }
}

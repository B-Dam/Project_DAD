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
            return;

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
            var nextLine = session.HasIndex(currentIndex) ? session.GetLine(currentIndex) : null;
            var nextID = session.HasIndex(currentIndex) ? session.GetID(currentIndex) : null;

            if (!string.IsNullOrEmpty(nextLine?.spritePath) && nextLine.spritePath.StartsWith("Cutscenes/Video/"))
            {
                Debug.Log("[FlowController] 블랙 패널 이후 → 다음은 컷신 → 페이드인/아웃 후 컷신 진입");

                DialogueManager.Instance.StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(true, () => {DialogueManager.Instance.ShowNextLine(); DialogueManager.Instance.UnlockInput();}));
            }
            else if (CutsceneDialogueUI.Instance.blackPanelDialogueID.Contains(nextID))
            {
                Debug.Log("[FlowController] 블랙 패널 이후 → 다음도 블랙 패널 대사 → 즉시 진행");
                DialogueManager.Instance.ShowNextLine();
                DialogueManager.Instance.UnlockInput();
            }
            else
            {
                Debug.Log("[FlowController] 블랙 패널 이후 → 일반 대사 → 그대로 진행");
                DialogueManager.Instance.StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(false, () => { DialogueManager.Instance.ShowNextLine(); DialogueManager.Instance.UnlockInput();}));
            }

            return;
        }

        var line = DialogueManager.Instance.GetCurrentLine();
        if (!string.IsNullOrEmpty(line?.spritePath) && line.spritePath.StartsWith("Cutscenes/Video/"))
            return;

        DialogueManager.Instance.ShowNextLine();
        DialogueManager.Instance.UnlockInput();
    }
}

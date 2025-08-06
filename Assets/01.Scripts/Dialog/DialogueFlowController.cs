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
                Debug.Log("[FlowController] �� �г� ���� �� ������ �ƽ� �� ���̵���/�ƿ� �� �ƽ� ����");

                DialogueManager.Instance.StartCoroutine(CutsceneController.Instance.EndAfterFadeInOut(true, () => {DialogueManager.Instance.ShowNextLine(); DialogueManager.Instance.UnlockInput();}));
            }
            else if (CutsceneDialogueUI.Instance.blackPanelDialogueID.Contains(nextID))
            {
                Debug.Log("[FlowController] �� �г� ���� �� ������ �� �г� ��� �� ��� ����");
                DialogueManager.Instance.ShowNextLine();
                DialogueManager.Instance.UnlockInput();
            }
            else
            {
                Debug.Log("[FlowController] �� �г� ���� �� �Ϲ� ��� �� �״�� ����");
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

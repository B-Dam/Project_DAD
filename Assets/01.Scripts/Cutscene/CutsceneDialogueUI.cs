using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CutsceneDialogueUI : MonoBehaviour
{
    public static CutsceneDialogueUI Instance { get; private set; }

    [Header("검은 패널용 대사 ID")]
    public List<string> blackPanelDialogueID;

    [Header("검은 패널 UI")]
    public GameObject blackPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    public bool IsActive => blackPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowDialogue(string dialogueID, string speaker, string text)
    {
        if (blackPanelDialogueID.Contains(dialogueID))
        {
            blackPanel.SetActive(true);
            speakerText.text = speaker;
            dialogueText.text = text;
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        blackPanel.SetActive(false);
    }

    public bool TryDisplayBlackPanelDialogue(string id)
    {
        if (!blackPanelDialogueID.Contains(id)) return false;

        var line = DialogueManager.Instance?.GetCurrentLine();
        if (line == null) return false;

        DialogueUIDisplayer.Instance.StartBlinkUX();

        ShowDialogue(id, line.speaker, line.text);
        return true;
    }
}
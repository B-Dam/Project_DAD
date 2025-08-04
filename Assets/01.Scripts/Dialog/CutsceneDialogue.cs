using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CutsceneDialogue : MonoBehaviour
{
    [Header("���� �гο� ��� ID")]
    public List<string> blackPanelDialogueID;

    [Header("���� �г� UI")]
    public GameObject blackPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    public void ShowDialogue(string dialogueID, string speaker, string text)
    {
        foreach (string id in blackPanelDialogueID)
        {
            Debug.Log($"[blackPanelDialogueID] ����Ʈ �׸�: '{id}'");
        }

        Debug.Log($"ShowDialogue ȣ��� - ���� ID: {dialogueID}, isInList: {blackPanelDialogueID.Contains(dialogueID)}");
        if (blackPanelDialogueID.Contains(dialogueID))
        {
            blackPanel.SetActive(true);
            speakerText.text = speaker;
            dialogueText.text = text;
        }
        else
        {
            blackPanel.SetActive(false);
        }
    }

    public void Hide()
    {
        Debug.Log("[CutsceneDialogue] ���� �г� ���� �����");
        blackPanel.SetActive(false);
    }
}

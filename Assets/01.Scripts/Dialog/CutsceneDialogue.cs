using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CutsceneDialogue : MonoBehaviour
{
    [Header("검은 패널용 대사 ID")]
    public List<string> blackPanelDialogueID;

    [Header("검은 패널 UI")]
    public GameObject blackPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    public void ShowDialogue(string dialogueID, string speaker, string text)
    {
        foreach (string id in blackPanelDialogueID)
        {
            Debug.Log($"[blackPanelDialogueID] 리스트 항목: '{id}'");
        }

        Debug.Log($"ShowDialogue 호출됨 - 전달 ID: {dialogueID}, isInList: {blackPanelDialogueID.Contains(dialogueID)}");
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
        Debug.Log("[CutsceneDialogue] 검은 패널 숨김 실행됨");
        blackPanel.SetActive(false);
    }
}

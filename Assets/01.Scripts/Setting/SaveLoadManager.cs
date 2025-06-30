using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SaveLoadManager : MonoBehaviour
{
    public enum Mode { Save, Load }

    [Header("Mode 선택 버튼")]
    [SerializeField] Button saveTabButton;
    [SerializeField] Button loadTabButton;

    [Header("컨텐츠 패널")]
    [SerializeField] GameObject saveContent;  
    [SerializeField] GameObject loadContent;

    [Header("슬롯 버튼 & 라벨")]
    [SerializeField] Button[] saveSlots;     // [0]=Auto, [1..3]=Manual
    [SerializeField] TextMeshPro[] saveSlotLabels;
    [SerializeField] Button[] loadSlots;
    [SerializeField] TextMeshPro[] loadSlotLabels;

    [Header("저장 확인 패널")]
    [SerializeField] GameObject saveConfirmPanel;
    [SerializeField] TextMeshPro saveConfirmText;
    [SerializeField] Button saveYesButton;
    [SerializeField] Button saveNoButton;

    [Header("불러오기 확인 패널")]
    [SerializeField] GameObject loadConfirmPanel;
    [SerializeField] TextMeshPro loadConfirmText;
    [SerializeField] Button loadYesButton;
    [SerializeField] Button loadNoButton;

    private Mode currentMode;
    private int selectedSlotIndex;

    void Start()
    {
        // 
        saveTabButton.onClick.AddListener(() => SwitchMode(Mode.Save));
        loadTabButton.onClick.AddListener(() => SwitchMode(Mode.Load));

        for (int i = 0; i < saveSlots.Length; i++)
            saveSlots[i].onClick.AddListener(() => OnClickSaveSlot(i));
        for (int i = 0; i < loadSlots.Length; i++)
            loadSlots[i].onClick.AddListener(() => OnClickLoadSlot(i));

        saveYesButton.onClick.AddListener(OnConfirmSave);
        saveNoButton.onClick.AddListener(() => saveConfirmPanel.SetActive(false));
        loadYesButton.onClick.AddListener(OnConfirmLoad);
        loadNoButton.onClick.AddListener(() => loadConfirmPanel.SetActive(false));

        SwitchMode(Mode.Save);
    }

    void SwitchMode(Mode mode)
    {
        currentMode = mode;
        saveContent.SetActive(mode == Mode.Save);
        loadContent.SetActive(mode == Mode.Load);
        saveConfirmPanel.SetActive(false);
        loadConfirmPanel.SetActive(false);
        RefreshAllSlots();
    }

    void RefreshAllSlots()
    {
        // 슬롯 레이블 업데이트
        for (int i = 0; i < saveSlots.Length; i++)
        {
            var data = DataManager.GetSaveMetadata(i);
            bool hasData = data != null;
            saveSlotLabels[i].text = hasData
                ? $"{data.timestamp:yyyy.MM.dd HH:mm}\n{data.chapterName} - {data.questName}"
                : "No Data";
            // 자동(Auto) 슬롯(0번)은 Save 모드에서 비활성화
            saveSlots[i].interactable = !(i == 0 && currentMode == Mode.Save);
            
            // Load 모드에선 데이터 없는 슬롯 비활성화
            loadSlots[i].interactable = (currentMode == Mode.Load) ? hasData : false;
            loadSlotLabels[i].text = saveSlotLabels[i].text;
        }
    }

    void OnClickSaveSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        // 0번(Auto)은 수동 저장 불가
        if (currentMode == Mode.Save && slotIndex == 0) return;

        saveConfirmText.text = $"슬롯 {slotIndex}에 저장하시겠습니까?\n슬롯에 저장된 이전의 데이터는 지워집니다!";
        saveConfirmPanel.SetActive(true);
    }

    void OnConfirmSave()
    {
        DataManager.SaveGame(selectedSlotIndex);
        saveConfirmPanel.SetActive(false);
        RefreshAllSlots();
    }

    void OnClickLoadSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        loadConfirmText.text = $"슬롯 {slotIndex}의 데이터를 불러오시겠습니까?\n저장하지 않은 데이터는 지워집니다!";
        loadConfirmPanel.SetActive(true);
    }

    void OnConfirmLoad()
    {
        DataManager.LoadGame(selectedSlotIndex);
        loadConfirmPanel.SetActive(false);
    }
}
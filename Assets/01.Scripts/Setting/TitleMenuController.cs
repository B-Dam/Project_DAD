using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject rootCanvasToHide;   // 시작 누를 때 false로 만들 Canvas (타이틀 UI)
    [SerializeField] Button startButton;
    [SerializeField] GameObject slotsPanel;
    [SerializeField] Button[] slotButtons;          // 0=Auto, 1..N=Manual
    [SerializeField] TMP_Text[] slotLabels;
    [SerializeField] GameObject confirmPanel;
    [SerializeField] TMP_Text confirmText;
    [SerializeField] Button confirmYesButton;
    [SerializeField] Button confirmNoButton;

    [Header("Scene")]
    [SerializeField] string mainSceneName = "MainScene";

    [Header("로드 시 트리거 회피 옵션")]
    [Tooltip("메인 씬 로드 직후, 플레이어 Collider를 잠시 꺼서 발밑 트리거가 즉시 발동하지 않도록 함")]
    [SerializeField] float spawnSafetySeconds = 0.35f; // 0~0.5 사이 권장

    int pendingSlot = -1;
    bool pendingIsLoad = false;

    void Awake()
    {
        if (slotsPanel)   slotsPanel.SetActive(false);
        if (confirmPanel) confirmPanel.SetActive(false);
    }

    void Start()
    {
        if (startButton) startButton.onClick.AddListener(OnClickStart);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;
            slotButtons[i].onClick.AddListener(() => OnClickSlot(idx));
        }

        confirmYesButton.onClick.AddListener(OnConfirmYes);
        confirmNoButton.onClick.AddListener(() => confirmPanel.SetActive(false));

        RefreshSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 우선순위: 확인창 → 닫기
            if (confirmPanel && confirmPanel.activeSelf)
            {
                confirmPanel.SetActive(false);
                return;
            }

            // 그 다음: 슬롯패널 → 닫고 타이틀 Canvas 복원
            if (slotsPanel && slotsPanel.activeSelf)
            {
                slotsPanel.SetActive(false);
                if (rootCanvasToHide) rootCanvasToHide.SetActive(true);
                return;
            }
        }
    }

    void OnClickStart()
    {
        // 타이틀 UI 비활성화
        if (rootCanvasToHide) rootCanvasToHide.SetActive(false);

        // 슬롯 패널 열기 + 갱신
        if (slotsPanel) slotsPanel.SetActive(true);
        RefreshSlots();
    }

    void RefreshSlots()
    {
        if (slotLabels == null) return;

        for (int i = 0; i < slotLabels.Length; i++)
        {
            bool has = SaveLoadManagerCore.Instance != null && SaveLoadManagerCore.Instance.HasSaveFile(i);

            if (i == 0) // Auto slot 표기
            {
                slotLabels[i].text = has ? MakeMetaLabel(i, "(Auto)")
                                         : "자동 저장 슬롯 (저장 없음)";
            }
            else
            {
                slotLabels[i].text = has ? MakeMetaLabel(i, "")
                                         : "빈 슬롯";
            }
        }
    }

    string MakeMetaLabel(int slot, string suffix)
    {
        var meta = DataManager.GetSaveMetadata(slot);
        string time    = meta.timestamp == DateTime.MinValue ? "-" : meta.timestamp.ToString("yyyy.MM.dd HH:mm");
        string chapter = string.IsNullOrEmpty(meta.chapterName) ? "-" : meta.chapterName;
        string quest   = string.IsNullOrEmpty(meta.questName)   ? "-" : meta.questName;
        return $"{time} {suffix}\n{chapter} - {quest}".Trim();
    }

    void OnClickSlot(int slotIndex)
    {
        bool has = SaveLoadManagerCore.Instance != null && SaveLoadManagerCore.Instance.HasSaveFile(slotIndex);

        // 0번(오토슬롯): 저장 없으면 클릭 무시 (새 게임 금지)
        if (slotIndex == 0 && !has)
        {
            // 필요하면 토스트/사운드
            return;
        }

        pendingSlot   = slotIndex;
        pendingIsLoad = has; // 저장 있으면 이어하기, 없으면 새 게임

        if (has)
        {
            confirmText.text = (slotIndex == 0)
                ? "자동 저장을 이어하시겠습니까?"
                : $"슬롯 {slotIndex}의 데이터를 이어하시겠습니까?";
        }
        else
        {
            confirmText.text = $"슬롯 {slotIndex}에 새 게임을 시작하시겠습니까?";
        }

        confirmPanel.SetActive(true);
    }

    void OnConfirmYes()
    {
        confirmPanel.SetActive(false);

        if (pendingIsLoad)
        {
            // ★ 로비에서는 로드하지 않는다. '예약'만 하고 씬 전환
            SaveLoadManagerCore.RequestLoadOnNextScene(pendingSlot);
        }
        // 새 게임은 예약 없이 바로 메인으로

        StartCoroutine(GoMainScene());
    }

    IEnumerator GoMainScene()
    {
        // UI 정리
        if (slotsPanel) slotsPanel.SetActive(false);

        // 원하는 Canvas 숨기기
        if (rootCanvasToHide) rootCanvasToHide.SetActive(false);

        // 메인 씬 로드
        var op = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        pendingSlot = -1;
        pendingIsLoad = false;
    }

    // 플레이어 콜라이더 켜고/끄기 (2D/3D 모두 대응)
    (Collider2D[] cols2D, Collider[] cols3D) TogglePlayerColliders(GameObject player, bool enabled)
    {
        Collider2D[] cols2D = Array.Empty<Collider2D>();
        Collider[]   cols3D = Array.Empty<Collider>();
        if (player != null)
        {
            cols2D = player.GetComponentsInChildren<Collider2D>(true);
            cols3D = player.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols2D) c.enabled = enabled;
            foreach (var c in cols3D) c.enabled = enabled;
        }
        return (cols2D, cols3D);
    }
    void ToggleBack(Collider2D[] cols2D, Collider[] cols3D, bool enabled)
    {
        foreach (var c in cols2D) if (c) c.enabled = enabled;
        foreach (var c in cols3D) if (c) c.enabled = enabled;
    }
}
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class HintMode : MonoBehaviour
{
    public Volume hintVolume;
    private ColorAdjustments colorAdjustments;

    private bool isGrayscale = false;

    private const int maxHintCount = 3;// 힌트 최대 사용 가능 횟수 (변경되지 않으므로 const로 선언)

    // 맵 ID별 힌트 남은 횟수 저장 (각 맵마다 별도 관리)
    private Dictionary<string, int> hintCountsByMap = new Dictionary<string, int>()
    {
        { "002", maxHintCount },
        { "008", maxHintCount }
    };

    private float hintDuration = 10f; // 힌트 지속 시간 
    private float hintDurationTimer = 0f;//지속시간 체크

    private float hintCooldown = 30f; // 힌트 재사용 대기 시간
    private float hintCooldownTimer = 0f; // 힌트 쿨타임 경과 시간

    private bool isInCooldown = false;

    public TextMeshProUGUI hintDurationTxt;
    public TextMeshProUGUI hintCooldownTxt;

    public Image[] hintIcons; // Inspector에서 3개 등록

    public Image hintDurationTimeBar;
    public Image hintCooldownTimeBar;

    public Button HintBtn;

    public HintLineConnector[] hintLineRenderer;

    private string lastMapID = "";
    private void Start()
    {
        if (hintVolume == null)
            hintVolume = Object.FindFirstObjectByType<Volume>();

        //Volume에서 Coloradjustments 컴포넌트를 가져옴
        hintVolume.profile.TryGet(out colorAdjustments);
        //ResetHintCount();
        //  모든 맵 힌트 카운트 초기화
        ResetAllHintCounts();

        // 버튼이 할당되어 있으면 클릭 이벤트 연결
        if (HintBtn != null)
        {
            HintBtn.onClick.AddListener(TryUseHintButton);
        }
    }
    private void Update()
    {
        string currentID = MapManager.Instance.currentMapID;
        // 맵 ID 변경 감지
        if (currentID != lastMapID)
        {
            lastMapID = currentID;

            // 힌트 사용 가능한 맵일 경우 UI 강제 갱신
            if (currentID == "002" || currentID == "008")
            {
                UpdateHintIcon();
                UpdateHintUI();
                UpdateHintTimeBar();
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && (currentID == "002" || currentID == "008"))
        {
            TryUseHint();
        }

        UpdateHintDuration();
        UpdateCooldown();
        UpdateHintUI();
        UpdateHintTimeBar();

        if (!(currentID == "002" || currentID == "008"))
        {
            ForceResetHintState();
        }
        else if (isGrayscale)
        {
            foreach (var connector in hintLineRenderer)
            {
                //connector.UpdateLineRenderer();
            }
        }
    }
    void TryUseHintButton()
    {
        TryUseHint();
    }

    void TryUseHint()
    {
        string currentID = MapManager.Instance.currentMapID;

        if (!isGrayscale)
        {
            //if (hintCount > 0 && !isInCooldown)
            if (hintCountsByMap.ContainsKey(currentID) && hintCountsByMap[currentID] > 0 && !isInCooldown)
            {
                //ActivateHint();
                ActivateHint(currentID); //  currentID 전달
            }
            else if (isInCooldown)
            {
                Debug.Log(" 힌트는 아직 쿨타임 중입니다!");
            }
            else
            {
                Debug.Log(" 힌트는 더 이상 사용할 수 없습니다!");
            }
        }
        else
        {
            DeactivateHint();

        }
    }




    void UpdateHintTimeBar()
    {
        //힌트 지속 시간 바 업데이트
        if (hintDurationTimeBar != null)
        {
            if (isGrayscale)
            {
                float ratio = Mathf.Clamp01((hintDuration - hintDurationTimer) / hintDuration);
                hintDurationTimeBar.fillAmount = ratio; // 지속 시간 바 업데이트

            }
            else
            {
                hintDurationTimeBar.fillAmount = 0f; // 그레이스케일이 아닐 때는 0으로 설정
            }
        }
        if (hintCooldownTimeBar != null)
        {
            //힌트 쿨타임 바 업데이트
            if (isInCooldown)
            {
                //float ratio = Mathf.Clamp01((hintCooldown - hintCooldownTimer)/ hintCooldown);//바 점점 줄어듬
                float ratio = Mathf.Clamp01(hintCooldownTimer / hintCooldown); // 바가 점점 늘어남
                hintCooldownTimeBar.fillAmount = ratio; // 쿨타임 바 업데이트
            }
            else
            {
                hintCooldownTimeBar.fillAmount = 1f; // 쿨타임이 아닐 때는 1으로 설정
            }
        }
    }

    //void ActivateHint()
    void ActivateHint(string mapID) //  맵 ID 기반으로 처리
    {
        isGrayscale = true;
        hintDurationTimer = 0f;
        hintCooldownTimer = 0f;
        isInCooldown = true;

        colorAdjustments.saturation.value = -100f; // 그레이스케일 효과 적용


        //  힌트 활성화가 성공했을 때만 선 켜기
        foreach (var connector in hintLineRenderer)
        {
            connector.SetActive(true);
        }

        //hintCount--; // 힌트 사용
        hintCountsByMap[mapID]--; //  해당 맵 힌트 감소
        Debug.Log($"힌트 사용! [{mapID}] 남은 힌트: {hintCountsByMap[mapID]}");
        //Debug.Log($"힌트 사용! 남은 힌트: {hintCount}");
        UpdateHintIcon();
    }

    void UpdateHintDuration()
    {
        if (!isGrayscale) return;
        hintDurationTimer += Time.deltaTime;
        if (hintDurationTimer >= hintDuration)
        {
            DeactivateHint();
            Debug.Log("힌트 지속 시간 종료!");
        }
    }

    void UpdateCooldown()
    {
        if (!isInCooldown) return;
        hintCooldownTimer += Time.deltaTime;
        if (hintCooldownTimer >= hintCooldown)
        {
            isInCooldown = false;
            Debug.Log("힌트 다시 사용 가능!");
        }
    }

    void DeactivateHint()
    {
        isGrayscale = false;
        colorAdjustments.saturation.value = 0f; // 그레이스케일 효과 해제

        // 힌트 비활성화 시 선도 꺼주기
        foreach (var connector in hintLineRenderer)
        {
            connector.SetActive(false);
        }
    }

    void UpdateHintUI()
    {
        if (hintDurationTxt != null)
        {
            if (isGrayscale)
            {
                hintDurationTxt.text = $"Hint Dur: {Mathf.Max(0, hintDuration - hintDurationTimer):F1}";

            }
            else
            {
                hintDurationTxt.text = "--";
            }
        }
        if (hintCooldownTxt != null)
        {
            if (isInCooldown)
            {
                hintCooldownTxt.text = $"Hint Cool: {Mathf.Max(0, hintCooldown - hintCooldownTimer):F1}";
            }
            else
            {
                hintCooldownTxt.text = "Hint";
            }
        }
    }

    void UpdateHintIcon()
    {
        string currentID = MapManager.Instance.currentMapID;
        int currentCount = hintCountsByMap.ContainsKey(currentID) ? hintCountsByMap[currentID] : 0;
        for (int i = 0; i < hintIcons.Length; i++)
        {
            if (hintIcons[i] != null)
            {
                hintIcons[i].gameObject.SetActive(i < currentCount); // 남은 힌트 개수에 따라 아이콘 활성화
            }
        }
    }

    //void ResetHintCount()
    //{
    //    hintCount = maxHintCount; // 힌트 카운트 초기화
    //}

    void ResetAllHintCounts() //  맵별 힌트 전부 초기화
    {
        hintCountsByMap["002"] = maxHintCount;
        hintCountsByMap["008"] = maxHintCount;
    }

    //public void ForceResetHintState()
    //{
    //    // 힌트 상태 초기화 (이펙트, 선, 타이머, UI 등)
    //    isGrayscale = false;
    //    isInCooldown = false;
    //    hintDurationTimer = 0f;
    //    hintCooldownTimer = 0f;

    //    if (colorAdjustments != null)
    //        colorAdjustments.saturation.value = 0f;

    //    foreach (var connector in hintLineRenderer)
    //        connector.SetActive(false);

    //    // 카운트 초기화
    //    ResetHintCount();

    //    // UI 초기화
    //    UpdateHintIcon();
    //    UpdateHintUI();
    //    UpdateHintTimeBar();
    //}
    public void ForceResetHintState()
    {
        isGrayscale = false;
        isInCooldown = false;
        hintDurationTimer = 0f;
        hintCooldownTimer = 0f;

        if (colorAdjustments != null)
            colorAdjustments.saturation.value = 0f;

        //foreach (var connector in hintLineRenderer)
        //    connector.SetActive(false);

        //ResetAllHintCounts(); //  힌트 전체 초기화

        UpdateHintIcon();
        UpdateHintUI();
        UpdateHintTimeBar();
    }

}

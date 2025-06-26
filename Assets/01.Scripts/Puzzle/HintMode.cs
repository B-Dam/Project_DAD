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
    private int hintCount = 3;
    private const int maxHintCount = 3;//값이 바뀌지 않으므로 const로 선언

    private float hintDuration = 10f; // 힌트 지속 시간 
    private float hintDurationTimer = 0f;//지속시간 체크
    private float hintCooldown = 30f; // 힌트 재사용 대기 시간
    private float hintCooldownTimer = 0f; // 재사용 대기 시간 체크

    private bool isInCooldown = false;

    public TextMeshProUGUI hintDurationTxt;
    public TextMeshProUGUI hintCooldownTxt;

    public Image[] hintIcons; // Inspector에서 3개 등록

    public Image hintDurationTimeBar;
    public Image hintCooldownTimeBar;

    public Button resetBtn;

    public HintLineConnector[] hintLineRenderer;

    private void Start()
    {
        if (hintVolume == null)
            hintVolume = Object.FindFirstObjectByType<Volume>();

        //Volume에서 Coloradjustments 컴포넌트를 가져옴
        hintVolume.profile.TryGet(out colorAdjustments);
        ResetHintCount();

        // 버튼이 할당되어 있으면 클릭 이벤트 연결
        if (resetBtn != null)
        {
            resetBtn.onClick.AddListener(TryUseHintButton);
        }
    }
    private void Update()
    {
        //E키로 힌트 사용
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryUseHint();
        }

        UpdateHintDuration();
        UpdateCooldown();
        UpdateHintUI();
        UpdateHintTimeBar();
    }

    void TryUseHintButton()
    {
        TryUseHint();
    }

    void TryUseHint()
    {
        if (!isGrayscale)
        {
            if (hintCount > 0 && !isInCooldown)
            {
                ActivateHint();

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

    void ActivateHint()
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

        hintCount--; // 힌트 사용
        Debug.Log($"힌트 사용! 남은 힌트: {hintCount}");
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
        for (int i = 0; i < hintIcons.Length; i++)
        {
            if (hintIcons[i] != null)
            {
                hintIcons[i].gameObject.SetActive(i < hintCount); // 남은 힌트 개수에 따라 아이콘 활성화
            }
        }
    }

    public void ResetHintCount()
    {
        hintCount = maxHintCount; // 힌트 카운트 초기화
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleHintUIController : MonoBehaviour
{
    [Header("UI 연결")]
    public Image[] hintIcons; // 힌트 개수 아이콘
    public Image durationBar; // 지속시간 바
    public Image cooldownBar; // 쿨다운 바
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI cooldownText;

    /// <summary>
    /// 힌트 아이콘 업데이트
    /// </summary>
    public void UpdateHintIcons(int count)
    {
        for (int i = 0; i < hintIcons.Length; i++)
        {
            hintIcons[i].gameObject.SetActive(i < count);
        }
    }

    /// <summary>
    /// 바 & 텍스트 상태 동시 갱신
    /// </summary>
    public void UpdateBars(
        bool isHintActive,
        float durationTimer,
        float hintDuration,
        bool isCooldown,
        float cooldownTimer,
        float hintCooldown)
    {
        if (durationBar != null)
            durationBar.fillAmount = isHintActive ? Mathf.Clamp01((hintDuration - durationTimer) / hintDuration) : 0f;

        if (cooldownBar != null)
            cooldownBar.fillAmount = isCooldown ? Mathf.Clamp01(cooldownTimer / hintCooldown) : 1f;

        if (durationText != null)
            durationText.text = isHintActive ? $"Dur: {hintDuration - durationTimer:F1}" : "--";

        if (cooldownText != null)
            cooldownText.text = isCooldown ? $"CD: {hintCooldown - cooldownTimer:F1}" : "Hint";
    }
}

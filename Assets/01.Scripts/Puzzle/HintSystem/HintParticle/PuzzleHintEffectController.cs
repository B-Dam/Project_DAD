using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PuzzleHintEffectController : MonoBehaviour
{
    [Header("후처리 Volume")]
    public Volume grayscaleVolume;

    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (grayscaleVolume == null)
        {
            //Debug.LogWarning("Volume이 연결되지 않았습니다.");
            return;
        }

        if (!grayscaleVolume.profile.TryGet(out colorAdjustments))
        {
            //Debug.LogWarning("ColorAdjustments가 Volume에 없습니다.");
        }
    }

    /// <summary>
    /// 화면을 흑백 또는 컬러로 전환
    /// </summary>
    public void SetGrayscale(bool enable)
    {
        if (colorAdjustments == null) return;

        colorAdjustments.saturation.value = enable ? -100f : 0f;
        //Debug.Log($"화면 색상 변경됨: {(enable ? "흑백" : "컬러")}");
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    private Image fadePanel;

    public Coroutine fadeCoroutine;

    private void Awake()
    {
        fadePanel = GameObject.Find("SettingCanvas")?.transform.Find("FadePanel")?.GetComponent<Image>();
        if (fadePanel == null)
        {
            Debug.LogError("페이드 패널 없음");
            return;
        }
        else
        {
            Color currentColor = fadePanel.color;
            currentColor.a = 0f;
            fadePanel.color = currentColor;
            fadePanel.gameObject.SetActive(false);
            return;
        }
    }

    private void Start()
    {
        fadePanel.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut(float duration)
    {
        fadePanel.gameObject.SetActive(true);
        Debug.Log($"페이드 아웃이 시작 되었고 페이드 패널의 상태가 {fadePanel.gameObject.activeSelf} 입니다");
        if (fadePanel.gameObject.activeSelf == true && fadeCoroutine == null)
        {
            float timer = 0f;
            Color startColor = new Color(0f, 0f, 0f, 0f);
            fadePanel.color = startColor;
            Color targetColor = new Color (startColor.r, startColor.g, startColor.b, 1f);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                fadePanel.color = Color.Lerp(startColor, targetColor, timer / duration);
                yield return null;
            }
            fadePanel.color = targetColor;
        }
        else
        {
            Debug.LogError($"페이드 패널: {fadePanel.gameObject.activeSelf}, 코루틴: {fadeCoroutine}");
        }
    }

    public IEnumerator FadeIn(float duration)
    {
        if (fadePanel.gameObject.activeSelf == true)
        {
            float timer = 0f;
            Color startColor = new Color(0f, 0f, 0f, 1f);
            fadePanel.color = startColor;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                fadePanel.color = Color.Lerp(startColor, targetColor, timer / duration);
                yield return null;
            }
            fadePanel.color = targetColor;
            fadePanel.gameObject.SetActive(false);
            fadeCoroutine = null;

        }
        else
        {
            Debug.LogError($"페이드 패널: {fadePanel.gameObject.activeSelf}, 코루틴: {fadeCoroutine}");
        }
    }

    private void OtherCoroutineStop()
    {

    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DialogueDatabase;

public class DialogueUIDisplayer : MonoBehaviour
{
    public static DialogueUIDisplayer Instance;

    [Header("UI 컴포넌트")]
    public GameObject dialoguePanel;
    public Image dialogBoxBackgroundImage;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    [Header("좌우 캐릭터 이미지")]
    public Image leftCharacterImage;
    public Image rightCharacterImage;

    [Header("깜빡이는 UX 이미지")]
    public Image uxBlinkImage;

    [Header("컷신 이미지")]
    public Image cutsceneImage;
    private Coroutine cutsceneShakeCoroutine;


    private Coroutine typingCoroutine;
    private Coroutine blinkCoroutine;
    private float typingSpeed = 0.02f;
    private bool isTyping = false;
    private string fullText = "";
    public bool IsTyping => isTyping;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowPanel()
    {
        dialoguePanel.SetActive(true);
    }

    public void HidePanel()
    {
        dialoguePanel.SetActive(false);
    }

    public void DisplayLine(DialogueLine line, Sprite left, Sprite right, bool shouldShake)
    {
        ShowPanel();
        speakerText.text = line.speaker;
        RestoreCharacterSprites(left, right);
        HandleCutsceneImage(line.spritePath, shouldShake);
        HandleTypingEffect(line.text);
    }

    public void ClearUI()
    {
        HidePanel();
        StopBlinkUX();
        HideDialogueSprites();
        if (cutsceneImage != null) cutsceneImage.gameObject.SetActive(false);
    }

    public void FinishTyping()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = fullText;
            isTyping = false;
            StartBlinkUX();
        }
    }

    public void RestoreCharacterSprites(Sprite left, Sprite right)
    {
        SetCharacterImage(leftCharacterImage, left);
        SetCharacterImage(rightCharacterImage, right);
    }

    public void HideDialogueSprites()
    {
        leftCharacterImage.sprite = null;
        leftCharacterImage.gameObject.SetActive(false);

        rightCharacterImage.sprite = null;
        rightCharacterImage.gameObject.SetActive(false);
    }

    public void StartBlinkUX()
    {
        if (uxBlinkImage == null) return;
        uxBlinkImage.gameObject.SetActive(true);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void StopBlinkUX()
    {
        if (uxBlinkImage == null) return;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        uxBlinkImage.gameObject.SetActive(false);
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            uxBlinkImage.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.5f);
            uxBlinkImage.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void HandleTypingEffect(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        uxBlinkImage?.gameObject.SetActive(false);

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                int tagEnd = text.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    string tag = text.Substring(i, tagEnd - i + 1);
                    dialogueText.text += tag;
                    i = tagEnd + 1;
                    continue;
                }
            }

            dialogueText.text += text[i];
            i++;
            yield return new WaitForSeconds(typingSpeed);
        }

        var session = DialogueManager.Instance?.Session;
        if (session != null && session.HasIndex(session.CurrentIndex))
        {
            StartBlinkUX();
        }
    }

    private void HandleCutsceneImage(string path, bool shake)
    {
        bool isSpriteCutscene = !string.IsNullOrEmpty(path) && !path.StartsWith("Cutscenes/Video/");
        if (!isSpriteCutscene)
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                cutsceneImage.sprite = sprite;
                cutsceneImage.gameObject.SetActive(true);
                if (shake) ShakeCutsceneImage(0.3f, 20f);
            }
            else
            {
                cutsceneImage.gameObject.SetActive(false);
            }
        }
        else
        {
            cutsceneImage.gameObject.SetActive(false);
        }

        if (dialogBoxBackgroundImage != null)
        {
            Color color = dialogBoxBackgroundImage.color;
            color.a = (!isSpriteCutscene) ? 1f : 1f;
            dialogBoxBackgroundImage.color = color;
        }
    }

    private void ShakeCutsceneImage(float duration, float magnitude)
    {
        if (cutsceneImage == null || !cutsceneImage.gameObject.activeInHierarchy) return;

        if (cutsceneShakeCoroutine != null)
            StopCoroutine(cutsceneShakeCoroutine);

        cutsceneShakeCoroutine = StartCoroutine(ShakeCutsceneRoutine(duration, magnitude));
    }

    private IEnumerator ShakeCutsceneRoutine(float duration, float magnitude)
    {
        RectTransform rt = cutsceneImage.rectTransform;
        Vector2 originalPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            rt.anchoredPosition = originalPos + new Vector2(0f, offsetY);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = originalPos;
        cutsceneShakeCoroutine = null;
    }

    private void SetCharacterImage(Image image, Sprite sprite)
    {
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
            PlayDropInEffect(image.rectTransform);
        }
        else
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
        }
    }

    private void PlayDropInEffect(RectTransform target)
    {
        if (target == null) return;
        StartCoroutine(DropInAnimation(target));
    }

    private IEnumerator DropInAnimation(RectTransform target)
    {
        Vector2 originalPos = target.anchoredPosition;
        Vector2 startPos = originalPos + new Vector2(0f, -120f);
        Vector2 overshootPos = originalPos + new Vector2(0f, 25f);

        float duration1 = 0.08f;
        float duration2 = 0.05f;
        float elapsed = 0f;

        target.anchoredPosition = startPos;
        while (elapsed < duration1)
        {
            float t = elapsed / duration1;
            target.anchoredPosition = Vector2.Lerp(startPos, overshootPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = overshootPos;
        elapsed = 0f;
        while (elapsed < duration2)
        {
            float t = elapsed / duration2;
            target.anchoredPosition = Vector2.Lerp(overshootPos, originalPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }
}
using System;
using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance;

    public VideoPlayer videoPlayer;
    private Action onEndCallback;

    public GameObject cutsceneVideo;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    public bool IsVideoPlaying => videoPlayer.isPlaying;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        fadeCanvas.alpha = 0f;
    }

    public void PlayVideo(string path, Action onEnd)
    {
        VideoClip clip = Resources.Load<VideoClip>(path);
        if (clip == null)
        {
            onEnd?.Invoke();
            return;
        }

        videoPlayer.clip = clip;
        onEndCallback = onEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        StartCoroutine(PlayAfterFadeInOut());
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= OnVideoEnd;

        StartCoroutine(EndAfterFadeInOut());
    }

    private IEnumerator PlayAfterFadeInOut()
    {
        yield return StartCoroutine(FadeIn());
        cutsceneVideo.gameObject.SetActive(true);
        yield return StartCoroutine(FadeOut());

        videoPlayer.Play();
    }

    private IEnumerator EndAfterFadeInOut()
    {
        yield return StartCoroutine(FadeIn());
        cutsceneVideo.gameObject.SetActive(false);
        yield return StartCoroutine(FadeOut());

        onEndCallback?.Invoke();
    }

    private IEnumerator FadeIn()
    {
        fadeCanvas.gameObject.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        fadeCanvas.gameObject.SetActive(false);
    }
}

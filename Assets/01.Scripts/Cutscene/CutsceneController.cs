using System;
using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance;

    public Camera mainCamera;
    private int originalCullingMask;

    public GameObject questUI;

    public VideoPlayer videoPlayer;
    private Action onEndCallback;

    public GameObject cutsceneVideo;
    public RenderTexture renderTexture;

    public CanvasGroup fadeCanvas;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 2f;

    public bool IsVideoPlaying => videoPlayer.isPlaying;
    public bool IsWaitingForInput => waitingForInput;
    private bool waitingForInput = false;
    public bool IsPreparing => isVideoPreparing;
    private bool isVideoPreparing = false;

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
        originalCullingMask = mainCamera.cullingMask;
        mainCamera.cullingMask = 0;

        questUI.SetActive(false);

        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.alpha = 1f;
    }

    public void PlayVideo(string path, Action onEnd)
    {
        waitingForInput = false;
        isVideoPreparing = true;

        Debug.Log($"[CutsceneController] 영상 재생 시도: {path}");

        VideoClip clip = Resources.Load<VideoClip>(path);
        if (clip == null)
        {
            Debug.LogWarning($"[CutsceneController] 영상 로드 실패: {path}");
            onEnd?.Invoke();
            return;
        }

        videoPlayer.clip = clip;
        StartCoroutine(PrepareVideo());
        onEndCallback = onEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        StartCoroutine(PlayAfterFadeInOut());
    }

    private IEnumerator PrepareVideo()
    {
        if (renderTexture != null)
        {
            videoPlayer.targetTexture = null;
            renderTexture.Release();
            renderTexture.Create();
            videoPlayer.targetTexture = renderTexture;
        }

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.frame = 0;
        videoPlayer.StepForward();
        videoPlayer.Pause();

        yield break;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("영상 종료됨. Space 입력 대기 시작.");
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.Pause();

        waitingForInput = true;
    }

    private IEnumerator PlayAfterFadeInOut()
    { 
        if (fadeCanvas.alpha == 1f)
        {
            cutsceneVideo.SetActive(true);
            yield return StartCoroutine(FadeOut(fadeOutDuration));
            mainCamera.cullingMask = originalCullingMask;
            questUI.SetActive(true);
        }
        else
        {
            yield return StartCoroutine(FadeIn(fadeInDuration));
            cutsceneVideo.SetActive(true);
            yield return StartCoroutine(FadeOut(fadeOutDuration));
        }

        isVideoPreparing = false;

        videoPlayer.Play();
    }

    public IEnumerator EndAfterFadeInOut(bool isBlackPanelDialogue)
    {
        videoPlayer.Stop();

        if (isBlackPanelDialogue)
        {
            onEndCallback?.Invoke();
        }
        else
        {
            yield return StartCoroutine(FadeIn(fadeInDuration));
            cutsceneVideo.SetActive(false);
            yield return StartCoroutine(FadeOut(fadeOutDuration));

            onEndCallback?.Invoke();
        }
    }

    private IEnumerator FadeIn(float duration)
    {
        fadeCanvas.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOut(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }
        fadeCanvas.gameObject.SetActive(false);
    }
}

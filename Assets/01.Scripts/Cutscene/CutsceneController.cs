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

    private bool isVideoPreparing = false;
    public bool IsVideoPlaying => videoPlayer.isPlaying;
    public bool IsPreparing => isVideoPreparing;

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
        DialogueUIDisplayer.Instance.SetPreventBlink(true);
        isVideoPreparing = true;

        Debug.Log($"컷신 재생 요청: path = {path}");

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
        Debug.Log("[Cutscene] OnVideoEnd 호출됨");

        videoPlayer.loopPointReached -= OnVideoEnd;
        onEndCallback = null;
        DialogueUIDisplayer.Instance.SetPreventBlink(false);

        if (DialogueManager.Instance != null)
        {
            Debug.Log("[Cutscene] DialogueManager.Instance 확인됨 → OnCutsceneEnded() 호출");
            DialogueManager.Instance.OnCutsceneEnded();
        }
        else
        {
            Debug.LogError("[Cutscene] DialogueManager.Instance가 null임");
        }
    }

    private IEnumerator PlayAfterFadeInOut()
    {
        Debug.Log("[Cutscene] PlayAfterFadeInOut 시작");
        if (fadeCanvas.alpha == 1f)
        {
            Debug.Log("[Cutscene] 화면이 이미 어두움 → 바로 컷신 활성화 후 FadeOut");
            cutsceneVideo.SetActive(true);
            yield return StartCoroutine(FadeOut(fadeOutDuration));
        }
        else
        {
            Debug.Log("[Cutscene] 화면이 밝음 → FadeIn 후 컷신 활성화");
            yield return StartCoroutine(FadeIn(fadeInDuration));
            cutsceneVideo.SetActive(true);
            yield return StartCoroutine(FadeOut(fadeOutDuration));
        }

        isVideoPreparing = false;

        Debug.Log("[Cutscene] videoPlayer.Play() 실행");
        videoPlayer.Play();
    }

    public IEnumerator EndAfterFadeInOut(bool nextIsCutscene, Action onEnd)
    {
        Debug.Log("[Cutscene] EndAfterFadeInOut 시작");
        videoPlayer.Stop();
        Debug.Log("[Cutscene] videoPlayer.Stop() 실행됨");
        yield return StartCoroutine(FadeIn(fadeInDuration));
        Debug.Log("[Cutscene] 화면 FadeIn 완료");

        mainCamera.cullingMask = 0;
        questUI.SetActive(false);
        //DialogueUIDisplayer.Instance.StopBlinkUX();
        Debug.Log("[Cutscene] 카메라와 UI 초기화 완료");


        if (!nextIsCutscene)
        {
            Debug.Log("[Cutscene] 다음은 일반 대사 → 화면 복구 후 FadeOut");
            mainCamera.cullingMask = originalCullingMask;
            questUI.SetActive(true);
            cutsceneVideo.SetActive(false);
            yield return StartCoroutine(FadeOut(fadeOutDuration));
            Debug.Log("[Cutscene] FadeOut 완료");
        }
        else
        {
            Debug.Log("[Cutscene] 다음은 컷신 → FadeOut 생략, 바로 이어짐");
        }

        Debug.Log("[Cutscene] onEnd 콜백 호출");
        onEnd?.Invoke();
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
        fadeCanvas.alpha = 1f;
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
        fadeCanvas.alpha = 0f;
        fadeCanvas.gameObject.SetActive(false);
    }
}

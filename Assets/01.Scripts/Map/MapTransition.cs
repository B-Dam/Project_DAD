using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class MapTransition : MonoBehaviour
{
    [Header("🗺️ 맵 이동 설정")]
    public string transitionID;
    public Transform destinationPoint;
    public CanvasGroup fadeCanvas;

    [Header("🔐 퀘스트 출입 조건")]
    public string requiredComplteQuestId;
    public string requiredinprogressQuestId;
    public string lockedMessage = "아직 이 지역에 들어갈 수 없습니다.";

    public bool isPlayerInRange = false;
    public bool isTransitioning = false;
    public bool isBlockedExternally = false;
    private Vector2 originalVelocity;

    private PlayerController playerController;
    public static MapTransition currentInteraction;
    public bool canInteract = true;

    [Header("이동할 맵 ID")]
    public string destinationMapID;

    [Header("시네마신 카메라")]
    public CinemachineCamera virtualCam;

    [Header("플레이어 위치 저장 여부")]
    public bool shouldSavePosition = false;
    private void Awake()
    {
        // 퀘스트 조건이 모두 비어 있다면 이동 허용
        if (string.IsNullOrEmpty(requiredComplteQuestId) && string.IsNullOrEmpty(requiredinprogressQuestId))
        {
            isBlockedExternally = false;
            return;
        }
    }

    private void Start()
    {
        if (fadeCanvas != null) fadeCanvas.alpha = 0f;
        fadeCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentInteraction != this || !isPlayerInRange || isTransitioning || !canInteract)
            return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return; // ✅ 대사 출력 중일 땐 입력 무시

        if (string.IsNullOrEmpty(requiredComplteQuestId) && string.IsNullOrEmpty(requiredinprogressQuestId))
        {
            isBlockedExternally = false;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (!other.CompareTag("Player")) return;
     if (!canInteract) return; 

    // 조건 검사
        if (!isBlockedExternally)
        {
            // 즉시 이동 시작
            StartCoroutine(Transition());
        }
        else
        {
            string failedQuestId = !string.IsNullOrEmpty(requiredComplteQuestId)
                ? requiredComplteQuestId
                : requiredinprogressQuestId;

            Debug.Log($"❌ 이동 차단됨: 퀘스트 '{failedQuestId}' 조건 불충족");
            StartInteractionCooldown(); // 쿨타임 적용
        }
}


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentInteraction == this)
                currentInteraction = null;

            isPlayerInRange = false;
        }
    }

    protected IEnumerator Transition()
    {
        isTransitioning = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                originalVelocity = playerController.GetVelocity();
                playerController.SetVelocity(Vector2.zero);
                playerController.enabled = false;
            }

        }
      

      
        // ✅ 1. 페이드 아웃
        yield return Fade(1f, true);

        //맵 변경
        if (!string.IsNullOrEmpty(destinationMapID))
        {
            MapManager.Instance.UpdateMapData(destinationMapID);
        }
        //  퍼즐 UI 직접 호출
        PuzzleUIController puzzleUI = FindAnyObjectByType<PuzzleUIController>();

        if (puzzleUI != null)
        {
            puzzleUI.HandleFadeComplete();
        }
        else
        {
            Debug.LogWarning(" PuzzleUIController를 찾지 못했습니다.");
        }
        // ✅ 2. 카메라 줌 연출
        Camera cam = Camera.main;
        float originalSize = cam.orthographicSize;
        yield return StartCoroutine(ZoomCamera(originalSize, originalSize * 0.5f, 0.5f)); // 줌 인

        // ✅ 3. 플레이어 위치 이동 + 카메라도 바로 이동
        if (destinationPoint != null && player != null)
        {
            player.transform.position = destinationPoint.position;
            cam.transform.position = new Vector3(destinationPoint.position.x, destinationPoint.position.y, cam.transform.position.z);
            Debug.Log($"➡ {player.name} 이동 완료: {destinationPoint.position}");

            // 체크된 경우에만 위치 저장
            if (shouldSavePosition)
            {
                PuzzlePlayerResettable resettable = player.GetComponent<PuzzlePlayerResettable>();
                if (resettable != null)
                {
                    resettable.SaveResetPoint();
                    Debug.Log(" 플레이어 위치 저장됨");
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        // ✅ 4. 줌 아웃
        yield return StartCoroutine(ZoomCamera(cam.orthographicSize, originalSize, 0.5f));

        // ✅ 5. 페이드 인
        yield return Fade(1f, false);

       

        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.SetVelocity(originalVelocity);
        }

        yield return new WaitForSeconds(0.5f);

        isTransitioning = false;
        StartInteractionCooldown();

       
    }

    private IEnumerator ZoomCamera(float fromSize, float toSize, float duration)
    {
        //Camera cam = Camera.main;
        //float elapsed = 0f;

        //while (elapsed < duration)
        //{
        //    elapsed += Time.deltaTime;
        //    cam.orthographicSize = Mathf.Lerp(fromSize, toSize, elapsed / duration);
        //    yield return null;
        //}

        //cam.orthographicSize = toSize;
        float elapsed = 0f;

        if (virtualCam == null)
            yield break;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newSize = Mathf.Lerp(fromSize, toSize, elapsed / duration);
            virtualCam.Lens.OrthographicSize = newSize;
            yield return null;
        }

        virtualCam.Lens.OrthographicSize = toSize;
    }

  private IEnumerator Fade(float duration, bool fadeIn)
{
    if (fadeCanvas == null) yield break;

    // 페이드 시작 시 무조건 활성화
    fadeCanvas.gameObject.SetActive(true);

    float startAlpha = fadeCanvas.alpha;
    float targetAlpha = fadeIn ? 1f : 0f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
        yield return null;
    }

    fadeCanvas.alpha = targetAlpha;

    // 알파가 0이 되었으면 완전히 비활성화
    if (targetAlpha == 0f)
    {
        fadeCanvas.gameObject.SetActive(false);
    }
}


    // ✅ 외부 제어용 메서드는 유지
    public void BlockInteractionExternally(bool value)
    {
        isBlockedExternally = value;
    }

    public void StartInteractionCooldown()
    {
        if (canInteract)
        {
            StartCoroutine(DisableInteractionForSeconds(3f)); // 필요 시 시간 조절
        }
    }

    private IEnumerator DisableInteractionForSeconds(float duration)
    {
        canInteract = false;
        Debug.Log($"🕒 MapTransition 쿨타임 시작 ({duration}초)");
        yield return new WaitForSeconds(duration);
        canInteract = true;
        Debug.Log("✅ MapTransition 쿨타임 해제됨");
    }
}

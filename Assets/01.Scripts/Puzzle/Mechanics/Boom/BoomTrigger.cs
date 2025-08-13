using UnityEngine;
using UnityEngine.UI;

public class BoomTrigger : MonoBehaviour
{
    [Header("폭발 시 UI")]
    public Canvas canvas;               // 폭발 UI가 있는 캔버스
    public GameObject boomEffectUI;     // 폭발 UI 루트(비활성 상태로 둠)
    public Button resetButton;          // 폭발 UI 안의 리셋 버튼
    public KeyCode resetKey = KeyCode.R;

    private PuzzleResetManager resetManager;
    private bool isUIActive = false;
    private void Start()
    {
        // 리셋 매니저 참조 (인스펙터에서 할당하지 않았다면 씬에서 검색)
        resetManager = FindFirstObjectByType<PuzzleResetManager>();

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetTriggered);
        else
            //Debug.LogWarning("[BoomTrigger] resetButton이 비어있습니다. 버튼으로 리셋을 못합니다.");

        if (boomEffectUI != null)
            boomEffectUI.SetActive(false);
    }

    private void Update()
    {
        // UI가 떠있는 동안 R키로도 리셋 가능
        if (isUIActive && Input.GetKeyDown(resetKey))
            OnResetTriggered();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Box")) return;

        //Debug.Log("Boom!");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Puzzle/TouchTrap");

        if (canvas == null)
        {
            //Debug.LogError("[BoomTrigger] Canvas가 인스펙터에 할당되어 있지 않습니다!");
            return;
        }
        if (boomEffectUI == null)
        {
            //Debug.LogError("[BoomTrigger] boomEffectUI가 인스펙터에 할당되어 있지 않습니다!");
            return;
        }

        // 게임 일시정지 + UI 표시
        Time.timeScale = 0f;
        boomEffectUI.SetActive(true);
        resetButton.gameObject.SetActive(true);
        isUIActive = true;
    }

    // 버튼 클릭 또는 R키로 호출
    private void OnResetTriggered()
    {
        // ResetPuzzle이 timeScale==0이면 바로 리턴하므로, 먼저 복구
        Time.timeScale = 1f;

        // 퍼즐 리셋
        if (resetManager != null)
            resetManager.ResetPuzzle();
        //else
            //Debug.LogWarning("[BoomTrigger] PuzzleResetManager를 찾지 못해 ResetPuzzle을 호출하지 못했습니다.");

        // UI 닫기
        if (boomEffectUI != null)
            boomEffectUI.SetActive(false);

        if(resetButton != null)
            resetButton.gameObject.SetActive(false);

        isUIActive = false;
    }
}

//// 폭발 이미지 띄우기
//if (boomEffectPrefab != null)
//{
//    Camera cam = Camera.main;
//    if (cam == null)
//    {
//        Debug.LogWarning("[BoomTrigger] 카메라 없음");
//        return;
//    }

//    // 1. 폭탄 위치를 스크린 좌표로 변환
//    Vector2 screenPos = cam.WorldToScreenPoint(transform.position);

//    // 2. 프리팹을 캔버스 하위에 생성
//    GameObject effect = Instantiate(boomEffectPrefab, canvas.transform);

//    // 3. UI 위치 설정
//    effect.GetComponent<RectTransform>().position = screenPos;
//}
//}
//    }
//}

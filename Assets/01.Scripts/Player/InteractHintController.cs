using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InteractHintController : MonoBehaviour
{
    public static InteractHintController Instance { get; private set; }

    public float detectRange = 1.5f;
    public LayerMask interactLayer;
    public GameObject hintUIPrefab;

    private GameObject currentHintUI;
    private Transform currentTarget;
    private bool hintDisabled = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 필요시 인스턴스 제거 (씬 분리될 경우)
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideHint(); // ✅ 씬 전환 시 UI 자동 제거
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive || hintDisabled)
        {
            HideHint();
            return;
        }

        DetectInteractable();
    }

    private void DetectInteractable()
    {
        Vector2 origin = transform.position;

        Collider2D hit = Physics2D.OverlapCircle(origin, detectRange, interactLayer);
        if (hit != null && (hit.CompareTag("NPC") || hit.CompareTag("Door") || hit.CompareTag("Item") || hit.CompareTag("Interact")))
        {
            if (currentTarget != hit.transform)
            {
                currentTarget = hit.transform;
                ShowHint(currentTarget);
            }
        }
        else
        {
            HideHint();
        }
    }

    private void ShowHint(Transform target)
    {
        if (currentHintUI == null)
        {
            GameObject prefab = Resources.Load<GameObject>("PressSpace/PressSpaceUI");
            if (prefab == null)
            {
                Debug.LogError("❌ Resources/PressSpaceUI 프리팹이 없습니다.");
                return;
            }
            currentHintUI = Instantiate(prefab);
        }

        currentHintUI.SetActive(true);
        currentHintUI.transform.position = target.position + Vector3.up * 1.2f;
    }

    public void DisableHint()
    {
        hintDisabled = true;
        HideHint();
    }

    public void EnableHint()
    {
        hintDisabled = false;
    }

    private void HideHint()
    {
        if (currentHintUI != null)
        {
            Destroy(currentHintUI);
            currentHintUI = null;
            currentTarget = null;
        }
    }
}

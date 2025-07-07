using UnityEngine;
using TMPro;

public class InteractHintController : MonoBehaviour
{
    public float detectRange = 1f;
    public LayerMask interactLayer;
    public GameObject hintUIPrefab;

    private GameObject currentHintUI;
    private Transform currentTarget;

    private bool hintDisabled = false; //  힌트 감지 비활성화 여부

    private void Update()
    {
        // 대화 중이면 강제로 힌트 숨기기
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive || hintDisabled)
        {
            HideHint();
            return;
        }
        //외부에서 힌트 비활성화 요청이 들어오면
        if (hintDisabled)
        {
            HideHint();
            return;
        }
        DetectInteractable();
    }


    private void DetectInteractable()
    {
        Vector2 origin = transform.position;
        Vector2 direction = Vector2.zero;

        Collider2D hit = Physics2D.OverlapCircle(origin, detectRange, interactLayer);
        if (hit != null && (hit.CompareTag("NPC") || hit.CompareTag("Item") || hit.CompareTag("Interact")))
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
        currentHintUI.transform.position = target.position + Vector3.up * 1.2f; // 오브젝트 위
    }
    //  외부에서 힌트 비활성화
    public void DisableHint()
    {
        hintDisabled = true;
        HideHint();
    }

    //  다시 힌트 활성화
    public void EnableHint()
    {
        hintDisabled = false;
    }
    void HideHint()
    {
        if (currentHintUI != null)
        {
            Destroy(currentHintUI);
            currentHintUI = null;
            currentTarget = null;
        }
    }
}

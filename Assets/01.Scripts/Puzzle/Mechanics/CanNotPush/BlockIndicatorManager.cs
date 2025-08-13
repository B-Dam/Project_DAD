using UnityEngine;

public class BlockIndicatorManager : MonoBehaviour
{
    public static BlockIndicatorManager Instance;

    public GameObject blockIndicatorPrefab;
    private Canvas canvas;

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
    public void ShowIndicator(Vector3 worldPos)
    {
        if (canvas == null)
        {
            canvas = GameObject.Find("PuzzleUICanvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                //Debug.LogWarning("Puzzle UI Canvas 찾을 수 없음 (BlockIndicatorManager-ShowIndicator 호출 시점)");
                return;
            }
        }

        if (blockIndicatorPrefab == null)
        {
            //Debug.LogWarning(" blockIndicatorPrefab is not assigned");
            return;
        }

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        GameObject icon = Instantiate(blockIndicatorPrefab, canvas.transform);
        //Debug.Log($"icon instantiated at {icon.GetComponent<RectTransform>().position}");

        icon.GetComponent<RectTransform>().position = screenPos;
        Destroy(icon, 0.2f);
    }
}

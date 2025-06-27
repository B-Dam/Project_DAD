using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 offset = new Vector2(10, -10);

    private RectTransform panelRT;
    private Canvas parentCanvas;

    void Awake()
    {
        // 싱글턴
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panelRT     = panel.GetComponent<RectTransform>();
        parentCanvas= GetComponentInParent<Canvas>();
        
        // 레이캐스트 차단
        var cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable   = false;
        
        panel.SetActive(false);
    }
    
    void Update()
    {
        if (!panel.activeSelf) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 screenPos = mouseScreenPos + offset;
        
        // 마우스 좌표를 Canvas 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            parentCanvas.worldCamera,
            out localPoint);

        panelRT.anchoredPosition = localPoint;
    }
    
    // 툴팁 보이기
    public void Show(string text)
    {
        tooltipText.text = text;
        panel.SetActive(true);
    }
    
    // 툴팁 숨기기
    public void Hide()
    {
        panel.SetActive(false);
    }
}
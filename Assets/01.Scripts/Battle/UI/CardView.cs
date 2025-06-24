using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI 참조")]
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] TextMeshProUGUI costText;

    [Header("드롭 영역")]
    [Tooltip("적에게 드래그해서 놓을 때 감지할 RectTransform")]
    [SerializeField] RectTransform enemyDropZone;

    public CardData data { get; private set; }
    HandManager handManager;

    RectTransform rect;
    Canvas canvas;
    CanvasGroup canvasGroup;
    Vector2 startPosition;

    public void Initialize(CardData cardData, HandManager manager)
    {
        data = cardData;
        handManager = manager;

        rect        = GetComponent<RectTransform>();
        canvas      = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 초기 위치 저장
        startPosition = rect.anchoredPosition;

        // UI에 데이터 바인딩
        iconImage.sprite = data.icon;
        nameText.text    = data.displayName;
        costText.text    = data.costAP.ToString();

        // TextFormatter로 키 값을 value로 치환
        descText.text    = TextFormatter.Format(data.effectText, new System.Collections.Generic.Dictionary<string,string> {
            { "damage", (CombatManager.Instance.PlayerBaseAtk + data.effectAttackValue + CombatManager.Instance.playerAtkMod).ToString() },
            { "turns", data.effectTurnValue.ToString() },
            { "shield", data.effectShieldValue.ToString() },
            { "debuff", data.effectAttackDebuffValue.ToString() },
            { "buff", data.effectAttackIncreaseValue.ToString() }
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha           = 0.6f;
        canvasGroup.blocksRaycasts  = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스/터치 이동량에 맞춰 카드 이동
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;

        // 드롭 위치가 적 영역(RectTransform)에 들어왔는지 검사
        if (RectTransformUtility.RectangleContainsScreenPoint(
                enemyDropZone, eventData.position, canvas.worldCamera))
        {
            // 사용 시도
            if (handManager.UseCard(this))
                return;   // 성공적으로 사용되면 이 오브젝트는 파괴됨
        }

        // 아니면 원위치
        rect.anchoredPosition = startPosition;
    }
}
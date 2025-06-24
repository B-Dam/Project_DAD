using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 참조")]
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] TextMeshProUGUI costText;
    
    [HideInInspector] public RectTransform enemyDropZone;

    public RectTransform Rect { get; private set; }
    public CardData data { get; private set; }
    
    public int index;
    
    Vector2 dragOffset;
    HandManager handManager;
    Canvas canvas;
    CanvasGroup canvasGroup;
    
    void Awake()
    {
        // 컴포넌트 캐시
        Rect         = GetComponent<RectTransform>();
        canvas       = GetComponentInParent<Canvas>();
        canvasGroup  = GetComponent<CanvasGroup>() 
                       ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(CardData cardData, HandManager manager, RectTransform dropZone)
    {
        data           = cardData;
        handManager    = manager;
        enemyDropZone  = dropZone;

        // UI 바인딩
        iconImage.sprite = data.icon;
        nameText.text    = data.displayName;
        costText.text    = data.costAP.ToString();
        descText.text    = TextFormatter.Format(
            data.effectText,
            new System.Collections.Generic.Dictionary<string,string> {
                { "damage", (CombatManager.Instance.PlayerBaseAtk + data.effectAttackValue + CombatManager.Instance.playerAtkMod).ToString() },
                { "turns", data.effectTurnValue.ToString() },
                { "shield", data.effectShieldValue.ToString() },
                { "debuff", data.effectAttackDebuffValue.ToString() },
                { "buff", data.effectAttackIncreaseValue.ToString() }
            }
        );
        
        // HandManager에 자신 등록 & 첫 레이아웃 호출
        manager.AddCard(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Tween 취소
        Rect.DOKill();

        // 맨 앞 배치 & 반투명
        Rect.SetAsLastSibling();
        canvasGroup.alpha          = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // 드래그 플래그 ON
        handManager.isDraggingCard = true;

        // 클릭 시 카드와 포인터 간 오프셋 계산
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handManager.handContainer, eventData.position, canvas.worldCamera, out localMouse);
        dragOffset = Rect.anchoredPosition - localMouse;
        
        // 앞쪽으로 올려주고 반투명
        Rect.SetAsLastSibling();
        canvasGroup.alpha          = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 포인터 로컬 좌표 계산
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handManager.handContainer, eventData.position, canvas.worldCamera, out localMouse);

        // 오프셋 보정하여 바로 붙여줌
        Rect.anchoredPosition = localMouse + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 투명도 복구 & Raycast 복원
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 드롭 위치가 적 드롭 존 안인지 체크
        bool droppedOnEnemy = RectTransformUtility
            .RectangleContainsScreenPoint(enemyDropZone, eventData.position, canvas.worldCamera);

        // 방어 카드거나, (공격/약화 카드면서) 적 위에 드롭됐으면 UseCard 시도
        if ((data.typePrimary == CardTypePrimary.방어 || droppedOnEnemy)
            && handManager.UseCard(this))
        {
            // 블록 레이캐스트를 꺼서 사용된 카드가 더 이상 드래그/클릭되지 않도록 함
            canvasGroup.blocksRaycasts = false;
            return;
        }

        // UseCard 실패(코스트 부족 또는 드롭 실패) 시
        // 드래그 플래그 끄고 원위치로 레이아웃 복귀
        handManager.isDraggingCard = false;
        handManager.LayoutHand();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        handManager.OnCardHover(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        handManager.OnCardExit();
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    [Header("핸드 설정")]
    public RectTransform handContainer; // 카드들이 배치될 부모 RectTransform
    public GameObject cardPrefab; // CardView 프리팹
    public int maxHandSize = 5; // 최대 핸드 크기
    public RectTransform enemyDropZone; // 에디터에서 EnemyPanel 할당

    [Header("합성 설정")]
    [Tooltip("합성 시 소모할 AP")]
    public int combineAPCost = 1;

    [Header("게임 참조")]
    public int currentAP { get; private set; }

    [Header("레이아웃 설정")] 
    public float cardWidth = 200f; // 카드 너비 (px, RectTransform width)
    public float spacing = 40f; // 카드 간 간격
    public float animDuration = 0.25f;

    [Header("포커스 설정")] 
    public float focusScale = 1.2f;
    public float focusShift = 50f; // 양옆으로 밀어내는 거리
    public float focusLift  = 30f;  // 위로 띄울 거리

    [HideInInspector] public bool isDraggingCard;

    // 내부 덱 / 디스카드 / 핸드 뷰
    List<CardData> deck = new List<CardData>();
    List<CardData> discard = new List<CardData>();
    List<CardView> handViews = new List<CardView>();

    CardView currentlyDiscarding;

    public static HandManager Instance { get; private set; }

    void Awake()
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

    void Start()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnStart += StartPlayerTurn;
        else
            Debug.LogError("HandManager: TurnManager.Instance is NULL");
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnStart -= StartPlayerTurn;
    }

    // 플레이어 턴이 시작될 때 호출
    void StartPlayerTurn()
    {
        // 덱 초기화: 1성 카드 3종을 5복제 > 15장
        var baseCards = DataManager.Instance.GetPlayerCards(); // 3종 반환
        deck = baseCards
               .SelectMany(cd => Enumerable.Repeat(cd, 5)) // 각 카드 5장씩 복제
               .ToList();

        Shuffle(deck);

        // 이전 턴 사용 기록 비우기
        discard.Clear();
        ClearHand();
        currentAP = 3;

        // 카드 드로우
        for (int i = 0; i < maxHandSize; i++)
            DrawCard();
        
        // ▶ 즉시가 아니라 다음 프레임에 레이아웃 걸기
        DOVirtual.DelayedCall(0f, () => {
            DOTween.Kill("Layout");
            LayoutHand();
        });
    }

    // 덱 섞기
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void DrawCard()
    {
        // 덱이 비어 있으면 재셔플
        if (deck.Count == 0)
            RefillAndShuffleDeck();

        if (deck.Count == 0 || handViews.Count >= maxHandSize)
            return;

        var data = deck[0];
        deck.RemoveAt(0);

        // CardView 생성 및 초기화
        var go = Instantiate(cardPrefab, handContainer);
        var cv = go.GetComponent<CardView>();

        cv.Initialize(data, this, enemyDropZone);
    }

    // 새로 생성된 CardView를 핸드에 등록하고, 레이아웃을 갱신
    public void AddCard(CardView cv)
    {
        // 내부 리스트에 등록
        handViews.Add(cv);

        // 모든 CardView.index 재설정
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].index = i;
    }

    // discard를 모두 덱으로 되돌리고 셔플
    void RefillAndShuffleDeck()
    {
        deck = discard.ToList();
        discard.Clear();
        Shuffle(deck);
    }

    // 카드 사용 시 호출
    public bool UseCard(CardView cv)
    {
        // AP 체크
        if (currentAP < cv.data.costAP)
        {
            return false;
        }

        // AP 차감, 효과 적용
        currentAP -= cv.data.costAP;
        CombatManager.Instance.ApplySkill(cv.data, isPlayer: true);

        // discard에 이동
        discard.Add(cv.data);

        // 현재 카드만 LayoutHand 스킵하기 위한 플래그
        currentlyDiscarding = cv;

        // Tween 시작 전에 남아있는 트윈 모두 제거
        var rt = cv.Rect;
        rt.DOKill();

        // handContainer 아래에 있으면 레이아웃/마스크에 묶이므로, 캔버스 최상위로 이동
        rt.SetParent(handContainer.parent, true);

        // 최전면 배치
        rt.SetAsLastSibling();

        isDraggingCard = false;

        // 애니메이션 (로컬 anchoredPosition 기준)
        rt.DOAnchorPos(rt.anchoredPosition + new Vector2(0, 800f), 0.5f)
          .SetEase(Ease.InBack)
          .OnComplete(() =>
          {
              handViews.Remove(cv);

              // 버려진 뒤에도 index 재설정은 유지
              for (int i = 0; i < handViews.Count; i++)
                  handViews[i].index = i;

              Destroy(cv.gameObject);
              currentlyDiscarding = null;
              LayoutHand();
          });

        return true;
    }

    /// <summary>
    /// 같은 Rank1 카드 두 장을 합성해 Rank2 카드로 만드는 함수
    /// </summary>
    public bool TryCombine(CardData baseCardData)
    {
        // AP 체크
        if (currentAP < combineAPCost) return false;

        // 같은 이름·같은 rank 카드 두 장 찾기
        var candidates = handViews
                         .Where(cv =>
                             cv.data.displayName == baseCardData.displayName &&
                             cv.data.rank == baseCardData.rank)
                         .ToList();
        if (candidates.Count < 2) return false;

        // AP 차감
        currentAP -= combineAPCost;
        CombatUI.instance.UpdateUI();
        
        // 두 장 제거 (트윈 끊지 않도록 바로 DOKill + Destroy)
        foreach (var cv in candidates.Take(2))
        {
            handViews.Remove(cv);
            Destroy(cv.gameObject);
        }
        
        int nextRank = baseCardData.rank + 1;
        var newData = DataManager.Instance.GetCard(baseCardData.displayName, nextRank);
        if (newData == null)
        {
            // 레이아웃 복귀
            DOVirtual.DelayedCall(0f, LayoutHand);
            return false;
        }
        
        // 신규 카드 생성 및 초기화 (여기서 AddCard()만, 레이아웃은 따로)
        var go   = Instantiate(cardPrefab, handContainer);
        var cvNew = go.GetComponent<CardView>();
        cvNew.Initialize(newData, this, enemyDropZone);
        
        // 인덱스 재설정
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].index = i;

        // LayoutHand 한 번 호출해서 새 카드 포함 모든 카드에 애니메이션 걸기
        DOTween.Kill("Layout");  // 기존 레이아웃 트윈만 초기화
        
        isDraggingCard = false;
        LayoutHand();

        // 새 카드만 별도 스케일 애니메이션
        cvNew.Rect.localScale = Vector3.zero;
        cvNew.Rect
             .DOScale(1f, animDuration)
             .SetId("Combine")
             .SetEase(Ease.OutBack);

        return true;
    }

    // 핸드 전체 제거 (초기화용)
    void ClearHand()
    {
        foreach (var cv in handViews)
            Destroy(cv.gameObject);
        handViews.Clear();
    }

    // 카드 배치
    public void LayoutHand()
    {
        if (isDraggingCard) return;

        int count = handViews.Count;
        if (count == 0) return;

        DOTween.Kill("Layout");
        
        float totalW = count * cardWidth + (count - 1) * spacing;
        float startX = -totalW / 2f + cardWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            var cv = handViews[i];

            // 버려지는 애니메이션 중인 카드는 완전히 건너뛰기
            if (cv == currentlyDiscarding) continue;

            var rt = cv.Rect;

            float x = startX + i * (cardWidth + spacing);
            rt.DOAnchorPos(new Vector2(x, 0), animDuration).SetId("Layout").SetEase(Ease.OutQuad);
            rt.DOScale(1f, animDuration).SetId("Layout").SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// 카드 위에 포인터 enter 될 때 호출
    /// </summary>
    public void OnCardHover(int idx)
    {
        if (isDraggingCard) return;

        int count = handViews.Count;
        if (count == 0) return;
        
        DOTween.Kill("Layout");

        float totalW = count * cardWidth + (count - 1) * spacing;
        float startX = -totalW / 2f + cardWidth / 2f;
        
        for (int i = 0; i < count; i++)
        {
            var cv = handViews[i];

            // 버려지는 중인 카드는 보호
            if (cv == currentlyDiscarding) continue;

            var rt = cv.Rect;
            float baseX = startX + i * (cardWidth + spacing);
            Vector2 pos = new Vector2(baseX, 0);
            float scale = 1f;
            Ease easePos = Ease.OutQuad, easeScale = Ease.OutQuad;

            if (i == idx)
            {
                // 스케일 키우기
                scale = focusScale;
                // 위로 살짝 띄우기
                pos.y += focusLift;
                rt.SetAsLastSibling();
                easeScale = Ease.OutBack;
            }
            else if (i == idx - 1) pos += Vector2.left * focusShift;
            else if (i == idx + 1) pos += Vector2.right * focusShift;

            rt.DOAnchorPos(pos, animDuration).SetId("Layout").SetEase(easePos);
            rt.DOScale(scale, animDuration).SetId("Layout").SetEase(easeScale);
        }
    }

    /// <summary>
    /// 카드를 떼면 기본 배치로 복귀
    /// </summary>
    public void OnCardExit()
    {
        if (isDraggingCard) return;
        
        LayoutHand();
    }
    
    /// <summary>
    /// 턴 종료 버튼과 연결
    /// </summary>
    public void OnEndTurnButton()
    {
        // 핸드에 남은 카드 전부 Discard로 이동
        foreach (var cv in handViews)
            discard.Add(cv.data);
        
        ClearHand();
        TurnManager.Instance.EndPlayerTurn();
    }
}
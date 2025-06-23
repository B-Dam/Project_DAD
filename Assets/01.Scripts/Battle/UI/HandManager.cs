using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class HandManager : MonoBehaviour
{
    [Header("핸드 설정")]
    public Transform handContainer;     // 카드들이 배치될 부모 RectTransform
    public GameObject cardPrefab;       // CardView 프리팹
    public int maxHandSize = 5;         // 최대 핸드 크기

    [Header("게임 참조")]
    public int currentAP { get; private set; }

    List<CardData> deck = new List<CardData>();
    List<CardData> discard = new List<CardData>();
    List<CardView> handViews = new List<CardView>();

    void Start()
    {
        Debug.Log("[4] HandManager.OnEnable()");
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnStart += StartPlayerTurn;
        else
            Debug.LogError("[4!] HandManager: TurnManager.Instance is NULL");
    }

    void OnDestroy()
    {
        Debug.Log("[5] HandManager.OnDisable()");
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnStart -= StartPlayerTurn;
    }

    /// <summary>
    /// 플레이어 턴이 시작될 때 호출
    /// </summary>
    void StartPlayerTurn()
    {
        // 1) 덱 초기화: 1성 카드 3종을 5복제 → 15장
        var baseCards = DataManager.Instance.GetPlayerCards();  // 3종 반환
        deck = baseCards
               .SelectMany(cd => Enumerable.Repeat(cd, 5))  // 각 카드 5장씩 복제
               .ToList();

        Shuffle(deck);
        discard.Clear();        // 이전 턴 사용 기록 비우기
        ClearHand();
        currentAP = 3;

        // 3) 카드 드로우
        for (int i = 0; i < maxHandSize; i++)
            DrawCard();
    }

    /// <summary>
    /// 덱 섞기 (Fisher–Yates)
    /// </summary>
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    public void DrawCard()
    {
        // 덱이 비어 있으면 재셔플
        if (deck.Count == 0)
        {
            RefillAndShuffleDeck();
        }

        if (deck.Count == 0 || handViews.Count >= maxHandSize)
            return;

        var data = deck[0];
        deck.RemoveAt(0);
        
        // 사용된 카드는 discard에 보관
        discard.Add(data);

        var go = Instantiate(cardPrefab, handContainer);
        var cv = go.GetComponent<CardView>();
        cv.Initialize(data, this);
        handViews.Add(cv);
        LayoutHand();
    }
    
    void RefillAndShuffleDeck()
    {
        // discard를 모두 덱으로 되돌리고 셔플
        deck = discard.ToList();
        discard.Clear();
        Shuffle(deck);
    }

    /// <summary>
    /// 카드 사용 시 호출
    /// </summary>
    public bool UseCard(CardView cv)
    {
        var data = cv.data;
        if (currentAP < data.costAP) return false;

        // AP 차감
        currentAP -= data.costAP;
        // 전투 매니저에 효과 적용
        CombatManager.Instance.ApplySkill(data, isPlayer: true);

        // 카드 제거
        handViews.Remove(cv);
        Destroy(cv.gameObject);
        LayoutHand();
        return true;
    }

    /// <summary>
    /// 핸드 전체 제거(초기화용)
    /// </summary>
    void ClearHand()
    {
        foreach (var cv in handViews)
            Destroy(cv.gameObject);
        handViews.Clear();
    }

    /// <summary>
    /// 핸드 카드들 위치 재배치
    /// </summary>
    void LayoutHand()
    {
        float spacing = 160f;
        int count = handViews.Count;
        for (int i = 0; i < count; i++)
        {
            var rt = handViews[i].GetComponent<RectTransform>();
            float x = (i - (count - 1) * 0.5f) * spacing;
            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
        }
    }

    /// <summary>
    /// 턴 종료 버튼과 연결
    /// </summary>
    public void OnEndTurnButton()
    {
        // 핸드에 남은 카드 전부 Discard로 이동
        foreach (var cv in handViews)
        {
            discard.Add(cv.data);
        }
        ClearHand();
        
        TurnManager.Instance.EndPlayerTurn();
    }
}
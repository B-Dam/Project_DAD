using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Canvas RectTransform")]
    public RectTransform canvasRect; 
    
    [Header("턴 종료 버튼")]
    public Button endTurnButton;
    
    [Header("튜토리얼 스텝 설정")]
    public TutorialStep[] steps;
    
    [Header("메시지 박스")]
    public RectTransform messageBox;   // MessageBox RectTransform
    public TextMeshProUGUI messageText; // Text 컴포넌트
    public CanvasGroup messageBoxGroup;
    
    [Header("홀 마스크용 패널 (Input 차단)")]
    public Image topPanel;
    public Image bottomPanel;
    public Image leftPanel;
    public Image rightPanel;
    
    [Header("타이핑 효과 설정")]
    [Tooltip("한 글자 출력 간격 (초)")]
    public float typingSpeed = 0.05f;
    
    private int currentStep = -1;
    [SerializeField] bool typingComplete = false;
    
    private Color topOrig, bottomOrig, leftOrig, rightOrig;
    
    // AttachClickOverlay에서 할당할 필드
    private GameObject currentOverlay;
    
    public static TutorialManager Instance { get; private set; }

    void Awake() 
    {
        Instance = this;

        // 초기 색상 저장
        topOrig = topPanel.color;
        bottomOrig = bottomPanel.color;
        leftOrig = leftPanel.color;
        rightOrig = rightPanel.color;

        // 메세지 박스 초기 숨김
        messageBoxGroup.alpha = 0f;
        messageBoxGroup.interactable = false;
        messageBox.gameObject.SetActive(false);

        // 홀 패널도 모두 비활성화
        topPanel.gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        leftPanel.gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!typingComplete) return;

        var step = steps[currentStep];
        
        // 입력 스킵 허용 단계인지
        if (!step.allowSkipWithInput) return;
        
        // 스페이스바 또는 마우스 좌클릭
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            // onStepComplete 이벤트 호출 (여기에 ShowStep(n+1) 리스너가 붙어 있다면 자동 진행)
            step.onStepComplete.Invoke();
            
            // 만약 onStepComplete 리스너가 없다면 직접 다음 스텝 호출
            if (step.onStepComplete.GetPersistentEventCount() == 0)
                ShowStep(currentStep + 1);
        }
    }

    public void ShowStep(int i) 
    {
        if (i == currentStep) 
            return;
        
        if (i >= steps.Length)
        {
            EndTutorial();
            return;
        }
        
        // 이전 단계의 남은 트윈 모두 정리
        topPanel.DOKill();
        bottomPanel.DOKill();
        leftPanel.DOKill();
        rightPanel.DOKill();

        // 색 초기화
        ResetPanelColor(topPanel, topOrig);
        ResetPanelColor(bottomPanel, bottomOrig);
        ResetPanelColor(leftPanel, leftOrig);
        ResetPanelColor(rightPanel, rightOrig);
        
        currentStep = i;
        typingComplete = false;
        var step       = steps[i];
        bool hasHole   = step.highlightTarget != null;

        // 1) 메시지 박스 페이드인
        messageBox.anchoredPosition   = step.messageBoxAnchoredPos;
        messageBox.gameObject.SetActive(true);
        messageBoxGroup.interactable = true;
        messageBoxGroup.alpha        = 0f;
        messageBoxGroup.DOFade(1f, 0.3f);

        // 텍스트 타이핑 시작
        StopAllCoroutines();
        StartCoroutine(TypeText(step.instruction));

        // 홀 패널 초기화 모드 분기
        // 먼저 모든 패널 Anchors 초기화
        PrepPanel(topPanel.rectTransform);
        PrepPanel(bottomPanel.rectTransform);
        PrepPanel(leftPanel.rectTransform);
        PrepPanel(rightPanel.rectTransform);
        
        // 모두 비활성화
        topPanel  .gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        leftPanel .gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);

        if (hasHole)
        {
            // 부분 가림 모드
            Highlight(step.highlightTarget);
        }
        else
        {
            // 전체 가림 모드 → topPanel 하나만 풀스크린
            float cw = canvasRect.rect.width;
            float ch = canvasRect.rect.height;

            topPanel.rectTransform
                    .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, ch);
            topPanel.rectTransform
                    .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, cw);

            topPanel.raycastTarget = true;
            topPanel.gameObject.SetActive(true);
            topPanel.transform.SetAsLastSibling();
        }

        // 4) 스텝 시작 이벤트
        step.onStepStart.Invoke();

        // 5) 완료 시 연결
        step.onStepComplete.RemoveAllListeners();
        step.onStepComplete.AddListener(() => ShowStep(i + 1));
    }
    
    IEnumerator TypeText(string fullText)
    {
        messageText.text = string.Empty;
        messageBoxGroup.alpha = 1f;

        for (int c = 0; c < fullText.Length; c++)
        {
            messageText.text += fullText[c];
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingComplete = true;
    }

    void Highlight(RectTransform target)
    {
        if (canvasRect == null) return;
        
        // 1) 타겟 월드→캔버스 로컬 좌표
        Vector3[] wc = new Vector3[4];
        target.GetWorldCorners(wc);
        Vector2 bl = canvasRect.InverseTransformPoint(wc[0]);
        Vector2 tr = canvasRect.InverseTransformPoint(wc[2]);

        float cw = canvasRect.rect.width;
        float ch = canvasRect.rect.height;
        float px = canvasRect.pivot.x * cw;
        float py = canvasRect.pivot.y * ch;
        int   pad = 8;  // 여유 픽셀

        int x1 = Mathf.RoundToInt(bl.x + px) - pad;
        int y1 = Mathf.RoundToInt(bl.y + py) - pad;
        int x2 = Mathf.RoundToInt(tr.x + px) + pad;
        int y2 = Mathf.RoundToInt(tr.y + py) + pad;

        float holeX     = x1;
        float holeY     = y1;
        float holeW     = x2 - x1;
        float holeH     = y2 - y1;
        float holeTop   = ch - y2;
        float holeRight = cw - x2;

        // 2) 네 패널 4분할 세팅
        // Top
        topPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, holeTop);
        topPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, cw);
        topPanel.raycastTarget = true;
        topPanel.gameObject.SetActive(true);

        // Bottom
        bottomPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, holeY);
        bottomPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, cw);
        bottomPanel.raycastTarget = true;
        bottomPanel.gameObject.SetActive(true);

        // Left
        leftPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, holeX);
        leftPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, holeY, holeH);
        leftPanel.raycastTarget = true;
        leftPanel.gameObject.SetActive(true);

        // Right
        rightPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, holeRight);
        rightPanel.rectTransform
            .SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, holeY, holeH);
        rightPanel.raycastTarget = true;
        rightPanel.gameObject.SetActive(true);

        // 3) 순서 보장
        topPanel  .transform.SetAsLastSibling();
        bottomPanel.transform.SetAsLastSibling();
        leftPanel .transform.SetAsLastSibling();
        rightPanel.transform.SetAsLastSibling();
        messageBox.transform.SetAsLastSibling();
    }
    
    void PrepPanel(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot     = new Vector2(0.5f, 0.5f);
    }

    void EndTutorial()
    {
        // 메시지 박스 숨기기
        messageBoxGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            messageBoxGroup.interactable = false;
            messageBox.gameObject.SetActive(false);
        });

        // 모든 패널 끄기
        topPanel  .gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        leftPanel .gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);
    }
    
    // 카드 드래그를 막음
    public void DisableCardDrag() 
    {
        CardView.DisableCardDragging = true;
    }

    // 카드 드래그를 다시 허용
    public void EnableCardDrag() 
    {
        CardView.DisableCardDragging = false;
    }
    
    /// <summary>
    /// 하이라이트 패널들을 빠르게 페이드아웃
    /// </summary>
    public void FadeOutHighlight(float t = 0.1f)
    {
        topPanel.DOFade(0, t).OnComplete(() => topPanel.gameObject.SetActive(false));
        bottomPanel.DOFade(0, t).OnComplete(() => bottomPanel.gameObject.SetActive(false));
        leftPanel.DOFade(0, t).OnComplete(() => leftPanel.gameObject.SetActive(false));
        rightPanel.DOFade(0, t).OnComplete(() => rightPanel.gameObject.SetActive(false));
    }
    
    // 카드 사용 확인 스텝
    public void StepCardUse()
    {
        // 손에 있는 카드 전부 없에기
        HandManager.Instance.DiscardHandInstant();

        // 카드 드로우 (미리 배치된 카드 드로우)
        HandManager.Instance.DrawCard();

        // 카드 드래그 활성화
        EnableCardDrag();
        
        // 카드 사용 이벤트 구독
        CombatManager.Instance.OnPlayerSkillUsed += OnTutorialCardUsed;

        // 드래그 감지 페이드 이벤트 구독
        CardView.OnCardBeginDrag += OnTutorialCardDragStart;

        // 하이라이트된 카드 클릭 시 페이드 아웃 트리거 연결
        AttachClickOverlay(steps[currentStep].highlightTarget);
        
        // 턴 종료 버튼 비활성화
        endTurnButton.interactable = false;
    }
    
    private void OnTutorialCardDragStart(CardView cv)
    {
        // 구독 해제해서 한 번만 실행
        CardView.OnCardBeginDrag -= OnTutorialCardDragStart;

        // 페이드 아웃
        FadeOutHighlight();
        // 오버레이가 남아 있으면 제거
        if (currentOverlay != null) Destroy(currentOverlay);
    }
    
    void AttachClickOverlay(RectTransform target)
    {
        // Overlay 생성
        var go = new GameObject("TutorialClickOverlay",
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(EventTrigger));
        go.transform.SetParent(canvasRect, false);

        // 다른 UI들보다 위에 놓기
        go.transform.SetAsLastSibling();

        // 크기·위치 동기화
        var rt = go.GetComponent<RectTransform>();
        Vector3[] wc = new Vector3[4];
        target.GetWorldCorners(wc);
        Vector2 bl = canvasRect.InverseTransformPoint(wc[0]);
        Vector2 tr = canvasRect.InverseTransformPoint(wc[2]);
        rt.pivot            = Vector2.zero;
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.zero;
        rt.anchoredPosition = bl;
        rt.sizeDelta        = tr - bl;

        // 투명 이미지 + RaycastTarget
        var img = go.GetComponent<Image>();
        img.color         = new Color(1,1,1,0f);
        img.raycastTarget = true;
        
        currentOverlay = go;

        // EventTrigger 세팅
        var trig = go.GetComponent<EventTrigger>();
        trig.triggers.Clear();

        // 클릭 시 스킵
        var clickEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener(_ => {
            FadeOutHighlight();
            steps[currentStep].onStepComplete.Invoke();
            Destroy(go);
        });
        trig.triggers.Add(clickEntry);
    }
    
    /// <summary>
    /// 카드 사용(스킬 발동)이 확인됐을 때 호출
    /// </summary>
    private void OnTutorialCardUsed(CardData data)
    {
        // 중복 구독 해제
        CombatManager.Instance.OnPlayerSkillUsed -= OnTutorialCardUsed;
        
        if (currentOverlay != null)
            Destroy(currentOverlay);
        
        endTurnButton.interactable = true;

        steps[currentStep].onStepComplete.Invoke();
    }
    
    void ResetPanelColor(Image img, Color orig)
    {
        img.color = orig;
    }
}
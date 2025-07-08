using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// DialogueManager.HasSeen 과 IsDialogueActive를 이용해서
/// “대화가 끝난 시점”에 조건을 평가하고 이벤트를 실행
/// </summary>
public class DialogueTriggerCondition : MonoBehaviour
{
    [Header("감지할 대화 ID")]
    public string startId;  // 이 ID를 본 적이 있어야
    public string endId;    // 이 ID를 아직 보지 않아야

    [Header("조건 만족 시 실행할 이벤트")]
    public UnityEvent onConditionMet;

    private bool hasTriggered = false;
    private bool prevDialogueActive = false;

    private void Update()
    {
        var dm = DialogueManager.Instance;
        if (dm == null || hasTriggered) return;

        // 매 프레임 대화 활성 상태 변화를 체크
        bool nowActive = dm.IsDialogueActive;
        // “대화 중” → “대화 아님” 으로 변할 때(=대화 종료 시점)
        if (prevDialogueActive && !nowActive)
        {
            EvaluateCondition(dm);
        }
        prevDialogueActive = nowActive;
    }

    private void EvaluateCondition(DialogueManager dm)
    {
        bool sawStart = !string.IsNullOrEmpty(startId) && dm.HasSeen(startId);
        bool sawEnd   = !string.IsNullOrEmpty(endId)   && dm.HasSeen(endId);
        if (sawStart && !sawEnd)
        {
            onConditionMet?.Invoke();
            hasTriggered = true;
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 대화 진행 상태(DialogueManager.HasSeen)를 기준으로 조건이 만족되면 이벤트를 실행
/// </summary>
public class DialogueTriggerCondition : MonoBehaviour
{
    [Header("감지할 대화 ID")]
    [Tooltip("이 대화 ID를 본 적이 있어야 실행됨")]
    public string startId;
    [Tooltip("이 대화 ID를 본 적이 없어야 실행됨.")]
    public string endId;

    [Header("조건 만족 시 실행할 이벤트")]
    public UnityEvent onConditionMet;

    private bool hasTriggered = false;

    private void Start()
    {
        var dm = DialogueManager.Instance;
        if (dm == null || hasTriggered) return;

        // startId를 봤는지
        bool sawStart = !string.IsNullOrEmpty(startId) && dm.HasSeen(startId);
        // endId를 봤는지
        bool sawEnd   = !string.IsNullOrEmpty(endId)   && dm.HasSeen(endId);
        
        // startId를 봤고 endId는 아직 안 봤다면 이벤트 실행
        if (sawStart && !sawEnd)
        {
            onConditionMet?.Invoke();
            hasTriggered = true;
        }
    }
}
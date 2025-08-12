using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DialogueTriggerCondition : MonoBehaviour
{
    [Header("감지할 대화 ID")]
    public string startId;  // 이 ID를 본 적이 있어야
    public string endId;    // 이 ID를 아직 보지 않아야

    [Header("조건 만족 시 실행할 이벤트")]
    public UnityEvent onConditionMet;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        DialogueManager.OnDialogueEnded += EvaluateCondition;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueEnded -= EvaluateCondition;
    }

    private void EvaluateCondition()
    {
        if (hasTriggered) return;

        var dm = DialogueManager.Instance;
        if (dm == null) return;

        bool sawStart = !string.IsNullOrEmpty(startId) && dm.HasSeen(startId);
        bool sawEnd = !string.IsNullOrEmpty(endId) && dm.HasSeen(endId);

        //Debug.Log($"[DialogueTriggerCondition] EvaluateCondition 호출됨 - startId: {startId}={sawStart}, endId: {endId}={sawEnd}");

        if (sawStart && !sawEnd)
        {
            //Debug.Log("[DialogueTriggerCondition] 조건 만족 → 이벤트 실행");
            onConditionMet?.Invoke();
            hasTriggered = true;
        }
    }
}
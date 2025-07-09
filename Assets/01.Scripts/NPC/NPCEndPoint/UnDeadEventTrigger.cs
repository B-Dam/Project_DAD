using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnDeadEventTrigger : MonoBehaviour
{
    public OffscreenWatcher offscreenWatcher;
    public InteractHintController hintController;
    [Header("감지할 대사 ID 목록")]
    public List<string> triggerIDs;

    [Header("이동 대상 오브젝트들")]
    public List<Transform> targetObjects;

    [Header("도착지 오브젝트")]
    public Transform destinationTarget;

    [Header("이동 거리")]
    public float moveDuration = 10f;

    private bool hasTriggered = false;

    void Update()
    {
        if (!hasTriggered &&
            DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive)
        {
            string[] seenIDs = DialogueManager.Instance.GetAllSeenIDs();

            if (seenIDs.Length > 0)
            {
                string lastSeenID = seenIDs[seenIDs.Length - 1];

                if (triggerIDs.Contains(lastSeenID))
                {
                    if (destinationTarget == null)
                    {
                        Debug.LogWarning(" destinationTarget이 설정되지 않았습니다!");
                        return;
                    }

                    // 플레이어 비활성화
                    if (PlayerController.Instance != null)
                        PlayerController.Instance.enabled = false;

                    foreach (Transform obj in targetObjects)
                    {
                        if (obj == null) continue;

                        Debug.Log($" 대상 '{obj.name}' → {destinationTarget.position} 으로 이동 시작");
                        StartCoroutine(MoveToPosition(obj, destinationTarget.position, moveDuration));

                        if (offscreenWatcher != null)
                            offscreenWatcher.Register(obj); // 감시자에 등록
                    }

                    hasTriggered = true;
                    hintController.DisableHint();

                }
            }
        }
    }

    private IEnumerator MoveToPosition(Transform obj, Vector3 targetPos, float duration)
    {
        NPCMovementController npc = obj.GetComponent<NPCMovementController>();
        if (npc == null)
        {
            Debug.LogWarning($"'{obj.name}'에 NPCMovementController가 없습니다.");
            yield break;
        }

        Vector2 direction = (targetPos - obj.position).normalized;
        npc.SetMoveDirection(direction);

        float elapsed = 0f;
        float threshold = 0.05f;

        while (elapsed < duration)
        {
            if (obj == null) yield break;

            float distance = Vector2.Distance(obj.position, targetPos);
            if (distance <= threshold)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        npc.Stop(); // 멈춤 처리
        Debug.Log($" {obj.name} 이동 완료: {targetPos}");

        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = true;
        hintController.EnableHint();
    }
}

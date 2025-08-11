using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueMoveOnly : MonoBehaviour
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
    
    public bool IsTriggered => hasTriggered;
    public void SetTriggered(bool v) => hasTriggered = v;
   
    private void Update()
    {
        if (hasTriggered || DialogueManager.Instance == null)
        {
            return;
        }

        if (DialogueManager.Instance.IsDialogueActive)
        {
            string currentID = DialogueManager.Instance.CurrentDialogueID;
            if (triggerIDs.Contains(currentID))//리스트안에 currentID가 있는지 Contains(확인)
            {
                hasTriggered = true;
                DialogueManager.Instance.RegisterOnDialogueEndCallback(OnDialogueEnded);// 대화가 끝날 때 호출될 콜백 등록
                Debug.Log($"대사 ID '{currentID}' 감지됨 → 이동 시작");
            }
        }
    }

    void OnDialogueEnded()
    {
        DialogueManager.Instance.ClearOnDialogueEndCallback(OnDialogueEnded); // 콜백 제거

        hintController?.DisableHint();// 대사걸기 말풍선 비활성화

        if (destinationTarget == null)
        {
            Debug.LogWarning(" 도착지가 설정되지 않았습니다!");
            return;
        }
        // 플레이어 비활성화
        if (PlayerController.Instance != null)
        {
             PlayerController.Instance.enabled = false;
        }

        foreach (Transform obj in targetObjects)
        {
            if (obj == null) continue;

            Collider2D col = obj.GetComponent<Collider2D>();

            if (col != null)
            {
                col.enabled = false; // 충돌 비활성화
            }
            Debug.Log($"대상 '{obj.name}' → {destinationTarget.position} 으로 이동 시작");
            StartCoroutine(MoveToPosition(obj, destinationTarget.position, moveDuration));
        }
    }

    IEnumerator MoveToPosition(Transform obj, Vector3 targetPos, float duration)
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
        float threshold = 0.05f; // 도착 허용 오차

        while (elapsed < duration)
        {
            if (obj == null) yield break; // 오브젝트가 파괴되면 중지
            float distance = Vector2.Distance(obj.position, targetPos);
            if (distance <= threshold) // 도착 허용 오차 이내면 종료
                break;
            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        npc.Stop(); // NPC 멈춤 처리
        Debug.Log($"{obj.name} 이동 완료: {targetPos}");

        PlayerController.Instance.enabled = true; // 플레이어 이동 활성화
        hintController?.EnableHint(); // 대사걸기 말풍선 활성화
    }

    //void Update()
    //{
    //    if (!hasTriggered &&
    //        DialogueManager.Instance != null &&
    //        !DialogueManager.Instance.IsDialogueActive)
    //    {
    //        string[] seenIDs = DialogueManager.Instance.GetAllSeenIDs();

    //        if (seenIDs.Length > 0)
    //        {
    //            string lastSeenID = seenIDs[seenIDs.Length - 1];

    //            if (triggerIDs.Contains(lastSeenID))
    //            {
    //                if (destinationTarget == null)
    //                {
    //                    Debug.LogWarning(" destinationTarget이 설정되지 않았습니다!");
    //                    return;
    //                }

    //                // 플레이어 비활성화
    //                if (PlayerController.Instance != null)
    //                    PlayerController.Instance.enabled = false;

    //                foreach (Transform obj in targetObjects)
    //                {
    //                    if (obj == null) continue;

    //                    Debug.Log($" 대상 '{obj.name}' → {destinationTarget.position} 으로 이동 시작");
    //                    StartCoroutine(MoveToPosition(obj, destinationTarget.position, moveDuration));
    //                }

    //                hasTriggered = true;
    //                hintController.DisableHint();
    //            }
    //        }
    //    }
    //}

    //private IEnumerator MoveToPosition(Transform obj, Vector3 targetPos, float duration)
    //{
    //    NPCMovementController npc = obj.GetComponent<NPCMovementController>();
    //    if (npc == null)
    //    {
    //        Debug.LogWarning($"'{obj.name}'에 NPCMovementController가 없습니다.");
    //        yield break;
    //    }

    //    Vector2 direction = (targetPos - obj.position).normalized;
    //    npc.SetMoveDirection(direction);

    //    float elapsed = 0f;
    //    float threshold = 0.05f;

    //    while (elapsed < duration)
    //    {
    //        if (obj == null) yield break;

    //        float distance = Vector2.Distance(obj.position, targetPos);
    //        if (distance <= threshold)
    //            break;

    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }

    //    npc.Stop(); // 멈춤 처리
    //    Debug.Log($" {obj.name} 이동 완료: {targetPos}");

    //    if (PlayerController.Instance != null)
    //        PlayerController.Instance.enabled = true;
    //    hintController.EnableHint();
    //}
}

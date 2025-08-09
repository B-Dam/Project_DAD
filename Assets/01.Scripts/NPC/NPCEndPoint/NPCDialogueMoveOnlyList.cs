using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueMoveOnlyList : MonoBehaviour
{
    public OffscreenWatcher offscreenWatcher;
    public InteractHintController hintController;
    [Header("감지할 대사 ID 목록")]
    public List<string> triggerIDs;

    [Header("이동 대상 오브젝트들")]
    public List<Transform> targetObjects;

    [Header("이동 경로 (순차적)")]
    public List<Transform> waypoints;

    [Header("이동 거리")]
    public float moveDuration = 10f;

    private bool hasTriggered = false;

    [Header("이동 완료 시 오브젝트 비활성화여부")]
    public bool moveFinishSetActive = false; // Inspector에서 On/Off 가능
    
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

        if (waypoints == null)
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
            Debug.Log($"대상 '{obj.name}' → {waypoints} 으로 이동 시작");
            StartCoroutine(MoveAlongPath(obj, waypoints, moveDuration));
        }
    }

    IEnumerator MoveAlongPath(Transform obj, List<Transform> path, float duration)
    {
        NPCMovementController npc = obj.GetComponent<NPCMovementController>();
        if (npc == null)
        {
            Debug.LogWarning($"'{obj.name}'에 NPCMovementController가 없습니다.");
            yield break;
        }

        //Vector2 direction = (targetPos - obj.position).normalized;
        //npc.SetMoveDirection(direction);

        float threshold = 0.05f; // 도착 허용 오차

        foreach(var waypoint in path)
        {
            if (waypoint == null) continue;
            Vector3 targetPos = waypoint.position;
            Vector2 direction = (targetPos - obj.position).normalized;
            npc.SetMoveDirection(direction);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (obj == null) yield break; // 오브젝트가 파괴되면 중지
                float distance = Vector2.Distance(obj.position, targetPos);
                if (distance <= threshold) // 도착 허용 오차 이내면 다음 웨이포인트로 이동
                    break;
                elapsed += Time.deltaTime;
                yield return null; // 다음 프레임까지 대기
            }
            
        }
       

        npc.Stop(); // NPC 멈춤 처리

        //  이동 완료 후 비활성화 옵션 적용
        if (moveFinishSetActive && obj != null)
        {
            obj.gameObject.SetActive(false);
            Debug.Log($"{obj.name} → 이동 완료 후 비활성화됨");
        }

        Debug.Log($"{obj.name} 이동 완료: {path}");

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

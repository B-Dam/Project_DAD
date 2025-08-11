using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC가 대사(Dialogue)를 마친 후,
/// 지정된 경로로 이동(Move)한 뒤,
/// 자동으로 사라짐(Disappear).
/// </summary>
public class NPCDialogueMoveAndDisappear : MonoBehaviour
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


    /*
    DialogueManager는 대사를 출력할 때마다 그 ID를 seenIDs에 추가함
    DialogueEventTrigger는 Update()에서 seenIDs의 마지막 ID(lastSeenID)가
    triggerIDs 안에 있는지 확인해서
    해당 대사가 끝났으면 이동 트리거 발동(콜백 등록)
    */
    void Update()
    {
        if (SaveLoadManagerCore.IsRestoring) return;
        if (hasTriggered || DialogueManager.Instance == null) return;

        if (DialogueManager.Instance.IsDialogueActive)
        {
            string currentID = DialogueManager.Instance.CurrentDialogueID;
            if (triggerIDs.Contains(currentID))
            {
                hasTriggered = true;
                DialogueManager.Instance.RegisterOnDialogueEndCallback(OnDialogueEnded);
                Debug.Log($"▶ 대사 '{currentID}' 종료 콜백 등록");
            }
        }

    }
    private void OnDialogueEnded()
    {
        //  콜백 해제 (중복 실행 방지)
        DialogueManager.Instance.ClearOnDialogueEndCallback(OnDialogueEnded);

        // npc에게 말거는 말풍선 힌트 비활성화
        hintController.DisableHint();

        if (destinationTarget == null)
        {
            Debug.LogWarning(" destinationTarget이 설정되지 않았습니다!");
            return;
        }

        // 플레이어 비활성화
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }

        //등록한 오브젝트 이동시키기
        foreach (Transform obj in targetObjects)
        {
            if (obj == null) continue;

            //오브젝트에 BoxCollider2D, CircleCollider2D, PolygonCollider2D 같은 게 붙어 있다면 여기서 받아와 꺼버려서
            //벽이나 플레이어,npc 등과 충돌없이 이동 가능하게 함
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false; // 충돌 비활성화
            }

            Debug.Log($" 대상 '{obj.name}' → {destinationTarget.position} 으로 이동 시작");
            StartCoroutine(MoveToPosition(obj, destinationTarget.position, moveDuration));


            if (offscreenWatcher != null)
            {
                // 오프스크린 감시 등록
                offscreenWatcher.Register(obj);
            }

        }
        if (offscreenWatcher != null)
        {
            offscreenWatcher.WatchUntilAllRemoved(() =>
            {
                // 모든 대상이 제거되면 플레이어 다시 활성화
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.enabled = true;
                }
                Debug.Log(" 모든 대상 제거됨 → 플레이어 활성화");

                // 힌트 컨트롤러 활성화
                hintController.EnableHint();
            });
        }

    }

    //*NPC를 지정된 위치까지 부드럽게 이동시키는 코루틴(Coroutine)
    //지정된 시간 동안 오브젝트가 목표 지점까지 이동
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
            if (obj == null) yield break;

            float distance = Vector2.Distance(obj.position, targetPos);
            if (distance <= threshold) break;

            elapsed += Time.deltaTime;
            yield return null;
        }
        //npc.Destroy(); // 멈춤 처리
        npc.gameObject.SetActive(false);
        Debug.Log($" {obj.name} 이동 완료: {targetPos}");

    }
}
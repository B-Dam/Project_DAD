using UnityEngine;

public class Switch_PressureTrigger : MonoBehaviour
{
    [Header("누르면 SetActive(false)")]
    public GameObject targetA;

    [Header("누르면 SetActive(true)")]
    [SerializeField] private GameObject targetB;

    [Header("스위치")]
    public Collider2D switchCollider;

    private bool isActivated = false; // 열린 상태 확인용
    private GameObject instantiatedOpenDoor; // 생성된 열린 문 오브젝트 저장

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isActivated) return;
        // 완전히 스위치 영역 안에 들어왔는지 확인
        if ((other.CompareTag("Player") || other.CompareTag("Box")) &&
            IsFullyInside(switchCollider, other)) // 순서: 스위치가 대상 안에 있는가?
        {
            Debug.Log(" 스위치가 대상 안에 완전히 들어옴 → 문 열기");
            AudioManager.Instance.PlaySFX("PushSwitch");

            OpenDoor();

            isActivated = true;
        }
    }

    //  스위치 Collider가 상대 Collider 안에 완전히 포함됐는지 확인
    bool IsFullyInside(Collider2D inner, Collider2D outer)
    {
        //지정된 좌표가 outer 콜라이더의 경계 안에 포함되어 있는지 확인
        return outer.bounds.Contains(inner.bounds.min) &&//inner 콜라이더의 좌하단 꼭짓점 좌표
               outer.bounds.Contains(inner.bounds.max);//inner 콜라이더의 우상단 꼭짓점 좌표
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box"))
        {
            CloseDoor();

            isActivated = false;
        }
    }
    private void OpenDoor()
    {
        if (targetA != null)
            targetA.SetActive(false);

        AudioManager.Instance.PlaySFX("DoorOpen");

        if (targetB != null && instantiatedOpenDoor == null)
        {
            instantiatedOpenDoor = Instantiate(targetB, targetA.transform.position, targetB.transform.rotation);//Quaternion.identity회전없는상태
        }
    }
    private void CloseDoor()
    {
        if (targetA != null)
            targetA.SetActive(true);

        if (instantiatedOpenDoor != null)
        {
            Destroy(instantiatedOpenDoor);
            instantiatedOpenDoor = null;
        }
    }
}

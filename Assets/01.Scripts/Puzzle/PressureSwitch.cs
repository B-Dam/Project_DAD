using UnityEngine;

public class PressureSwitch : MonoBehaviour
{
    public GameObject targetDoor;
    public Collider2D switchCollider;

    private void OnTriggerStay2D(Collider2D other)
    {
        // 완전히 스위치 영역 안에 들어왔는지 확인
        if ((other.CompareTag("Player") || other.CompareTag("Box")) &&
            IsFullyInside(switchCollider, other)) // 순서: 스위치가 대상 안에 있는가?
        {
            Debug.Log(" 스위치가 대상 안에 완전히 들어옴 → 문 열기");
            if (targetDoor != null)
                targetDoor.SetActive(false);
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
            if (targetDoor != null)
                targetDoor.SetActive(true);
        }
    }
}

using UnityEngine;

public class HoleTrigger : MonoBehaviour
{
    private bool isFalled = false;
    [SerializeField] private GameObject holeCoverPrefab; // 구멍이 채워졌을 때 표시할 커버
    [SerializeField] private GameObject playerBlock;
    private Collider2D holeCollider;

    private void Start()
    {
        holeCollider = GetComponent<Collider2D>();
        holeCollider.isTrigger = true; // 구멍은 트리거로 설정
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isFalled) return; // 이미 구멍에 빠졌다면 무시
        if (other.CompareTag("Box") )//&& IsFullyInside(other, holeCollider)
        {
            BoxPush box = other.GetComponent<BoxPush>();
            if (box != null)
            {
                FillHole(box); // 박스가 구멍에 들어오면 구멍을 채움
            }
        }
    }
    //bool IsFullyInside(Collider2D outer, Collider2D inner)
    //{
    //    //지정된 좌표가 outer 콜라이더의 경계 안에 포함되어 있는지 확인
    //    return outer.bounds.Contains(inner.bounds.min) &&//inner 콜라이더의 좌하단 꼭짓점 좌표
    //           outer.bounds.Contains(inner.bounds.max);//inner 콜라이더의 우상단 꼭짓점 좌표
    //}
    private void FillHole(BoxPush box)
    {
        isFalled = true; // 구멍에 빠졌음을 표시
        //holeCollider.isTrigger = false; // 구멍의 트리거를 비활성화
        holeCollider.enabled = false; // 구멍의 충돌을 비활성화
        if (holeCoverPrefab != null)
        {
            Instantiate(holeCoverPrefab, transform.position, Quaternion.identity);
        }
        playerBlock.SetActive(false); // 플레이어 블록 활성화
        box.gameObject.SetActive(false); // 박스를 비활성화
    }
}

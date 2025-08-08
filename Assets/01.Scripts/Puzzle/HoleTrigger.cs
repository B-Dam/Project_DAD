using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleTrigger : MonoBehaviour
{
    private bool isFalled = false;
    [SerializeField] private GameObject holeCoverPrefab; // 구멍이 채워졌을 때 표시할 커버
    [SerializeField] private GameObject playerBlock;
    private Collider2D holeCollider;
    private static readonly HashSet<int> ClaimedBoxes = new(); // 박스 선점 테이블
    private void Start()
    {
        holeCollider = GetComponent<Collider2D>();
        holeCollider.isTrigger = true; // 구멍은 트리거로 설정

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isFalled) return; // 이미 구멍에 빠졌다면 무시
        if (!other.CompareTag("Box")) return;

        BoxPush box = other.GetComponent<BoxPush>();
        if (box == null) return;
        int boxId = other.GetInstanceID();

        //박스 선점: 이미 다른 구멍이 처리 중이면 무시
        if (!ClaimedBoxes.Add(boxId)) return;


        StartCoroutine(FillHoleDelayed(box)); // 박스가 구멍에 들어오면 구멍을 채움
        AudioManager.Instance.PlaySFX("Puzzle/BoxDrop");


    }
    //bool IsFullyInside(Collider2D outer, Collider2D inner)
    //{
    //    //지정된 좌표가 outer 콜라이더의 경계 안에 포함되어 있는지 확인
    //    return outer.bounds.Contains(inner.bounds.min) &&//inner 콜라이더의 좌하단 꼭짓점 좌표
    //           outer.bounds.Contains(inner.bounds.max);//inner 콜라이더의 우상단 꼭짓점 좌표
    //}
    private IEnumerator FillHoleDelayed(BoxPush box)
    {
        yield return null; // 한 프레임 대기
        FillHole(box);
    }

    private void FillHole(BoxPush box)
    {
        isFalled = true; // 구멍에 빠졌음을 표시
        //holeCollider.isTrigger = false; // 구멍의 트리거를 비활성화
        holeCollider.enabled = false; // 구멍의 충돌을 비활성화



        Transform parent = transform.parent;


        if (holeCoverPrefab != null)
            Instantiate(holeCoverPrefab, transform.position, Quaternion.identity, parent ? parent : null);

        if (playerBlock != null)
            playerBlock.SetActive(false);

        if (box != null)
            box.gameObject.SetActive(false);
        //if (holeCoverPrefab != null && parent != null)
        //{
        //    Instantiate(holeCoverPrefab, transform.position, Quaternion.identity, parent);
        //}
        //else
        //{
        //    // 예외 처리: 부모 없으면 그냥 생성
        //    Instantiate(holeCoverPrefab, transform.position, Quaternion.identity);
        //}
        //playerBlock.SetActive(false); // 플레이어 블록 활성화
        //box.gameObject.SetActive(false); // 박스를 비활성화
    }
}

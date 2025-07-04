using System.Collections.Generic;
using UnityEngine;

public class Switch_MultiTrigger : MonoBehaviour
{
    [Header("안 보여질 것")]
    public GameObject targetA;
    [Header("보여질 것")]
    public GameObject targetB;
    [Header("스위치")]
    public List<Collider2D> switchColliders;

    private bool isActivated = false; // 열린 상태 확인용

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isActivated) return;
        // 완전히 스위치 영역 안에 들어왔는지 확인
        if ((other.CompareTag("Player") || other.CompareTag("Box")) &&
            AllSwitchInside(other)) // 순서: 스위치가 대상 안에 있는가?
        {
            Debug.Log(" 스위치가 대상 안에 완전히 들어옴 → 문 열기");

            OpenTagetB();
            isActivated = true;
        }
    }

    bool AllSwitchInside(Collider2D outer)
    {
        foreach(Collider2D switchCol in switchColliders)
        {
            if(switchCol == null) continue; // 스위치 콜라이더가 비어있으면 건너뛰기
            if (!IsFullyInside(switchCol, outer))
            {
                return false;
            }
        }
        return true;
    }
    
    //  스위치 Collider가 상대 Collider 안에 완전히 포함됐는지 확인
    bool IsFullyInside(Collider2D inner, Collider2D outer)
    {
        //지정된 좌표가 outer 콜라이더의 경계 안에 포함되어 있는지 확인
        return outer.bounds.Contains(inner.bounds.min) &&//inner 콜라이더의 좌하단 꼭짓점 좌표
               outer.bounds.Contains(inner.bounds.max);//inner 콜라이더의 우상단 꼭짓점 좌표
    }

  
    private void OpenTagetB()
    {
        if (targetA != null)
            targetA.SetActive(false);

        if (targetB != null)
        {
            targetB.SetActive(true);
        }
    }
    //private void CloseTagetB()
    //{
    //    if (targetA != null)
    //        targetA.SetActive(false);

    //    if (targetB != null)
    //    {
    //        targetB.SetActive(true);
    //    }
    //}
}

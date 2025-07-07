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

    private void Update()
    {
        if (AllSwitchesAreOccupied())
        {
            if (!isActivated)
            {
                Debug.Log(" 모든 스위치가 누름 상태 → 작동!");
                OpenTargetB();
                isActivated = true;
            }
        }
        else
        {
            if (isActivated)
            {
                Debug.Log(" 하나 이상 비었음 → 꺼짐");
                CloseTargetB();
                isActivated = false;
            }
        }
    }

    bool AllSwitchesAreOccupied()
    {
        foreach(Collider2D switchCol in switchColliders)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(switchCol.bounds.center, switchCol.bounds.size, 0f);
            bool occupied = false;
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player") || hit.CompareTag("Box"))
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied)
                return false; // 이 스위치는 비어 있음
        }
        return true;
    }
    
 

  
    private void OpenTargetB()
    {
        if (targetA != null)
            targetA.SetActive(false);

        if (targetB != null)
        {
            targetB.SetActive(true);
        }
    }
    private void CloseTargetB()
    {
        if (targetA != null)
            targetA.SetActive(true);

        if (targetB != null)
        {
            targetB.SetActive(false);
        }
    }
}

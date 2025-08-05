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

    private Dictionary<Collider2D, bool> switchStates;

    private void Awake()
    {
        switchStates = new Dictionary<Collider2D, bool>();
        foreach (Collider2D switchCol in switchColliders)
        {
            switchStates[switchCol] = false;
        }
    }

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
        bool allOccupied = true;

        foreach (Collider2D switchCol in switchColliders)
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

            if (occupied && !switchStates[switchCol])
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("Puzzle/Puzzle_Switch_button");
                    Debug.Log($"스위치 '{switchCol.name}' 활성화 효과음 재생! (상태 변경 감지)");
                }
                else
                {
                    Debug.LogWarning($"스위치 '{switchCol.name}': AudioManager.Instance가 null이라 효과음 재생 실패.");
                }
            }
            switchStates[switchCol] = occupied;

            if (!occupied)
                allOccupied = false; // 이 스위치는 비어 있음
        }
        return allOccupied;
    }




    private void OpenTargetB()
    {
      

        if (targetB != null)
        {
            
           

            //  카메라 연출 추가
            CameraEventPlayer.Instance.PlayCameraSequence(
                targetA.transform,   // 문을 중심으로 카메라 이동
                1f,                  // 이동 시간
                1f ,                  // 멈춰 있는 시간
                ()=>{
                    if (targetA != null)
                        targetA.SetActive(false);

                    if (targetB != null)
                        targetB.SetActive(true);

                    AudioManager.Instance.PlaySFX("Puzzle/Door-metal");
                }   // 특별한 콜백 작업 필요 없으면 생략 가능
            );
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

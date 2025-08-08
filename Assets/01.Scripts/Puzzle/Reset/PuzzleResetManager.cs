using UnityEngine;
using UnityEngine.UI;

public class PuzzleResetManager : MonoBehaviour
{
    public Button resetButton;

    void Start()
    {
        //hintMode = Object.FindFirstObjectByType<HintMode>();
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetPuzzle);
        }
        else
        {
            Debug.LogWarning("Reset 버튼이 연결되지 않았습니다.");
        }
    }
    void Update()
    {
        string currentMapID = MapManager.Instance.currentMapID;
        if (Input.GetKeyDown(KeyCode.R) && PuzzleManager.Instance.IsPuzzleMap(currentMapID))
        {
            //박스들의 이동을 강제 중지
            BoxPush[] allBoxes = Object.FindObjectsByType<BoxPush>(FindObjectsSortMode.None);
            foreach (var box in allBoxes)
            {
                box.ForceStop();
            }

            ResetPuzzle();
        }
    }
    public void ResetPuzzle()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var controller = player.GetComponent<PlayerController>();
        if (controller == null || Time.timeScale == 0f || !PlayerController.Instance.CanMove())
        {
            return; // 플레이어가 비활성화 상태면 리셋 취소
        }
        string currentMapID = MapManager.Instance.currentMapID;
        Time.timeScale = 1f; // 시간 스케일을 원래대로 복원 
        var resettableObjects = FindObjectsByType<PuzzleResettable>(FindObjectsSortMode.None);
        foreach (var obj in resettableObjects)
        {
            //obj.ResetPuzzleState(currentMapID);
            if (obj.mapID == "Map" + currentMapID) // 현재 맵에 속한 오브젝트만
            {
                obj.ResetPuzzleState(currentMapID);
            }
        }

        var playerResettable = Object.FindFirstObjectByType<PuzzlePlayerResettable>();
        playerResettable.ResetPlayerState(currentMapID);
        PuzzleHintManager.Instance?.DeactivateHint(); // 힌트 비활성화
        Debug.Log("퍼즐 상태 리셋 완료");
    }

}

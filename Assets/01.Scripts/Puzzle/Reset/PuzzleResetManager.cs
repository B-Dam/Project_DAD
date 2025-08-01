using UnityEngine;
using UnityEngine.UI;

public class PuzzleResetManager : MonoBehaviour
{
    private HintMode hintMode;
    public Button resetButton;

    void Start()
    {
        hintMode = Object.FindFirstObjectByType<HintMode>();
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
        Time.timeScale = 1f; // 시간 스케일을 원래대로 복원 
        var resettableObjects = Object.FindObjectsByType<PuzzleResettable>(FindObjectsSortMode.None);
        foreach (var obj in resettableObjects)
        {
            obj.ResetState();
        }

        var playerResettable = Object.FindFirstObjectByType<PuzzlePlayerResettable>();
        playerResettable.ResetState();
        hintMode.ForceResetHintState();
        Debug.Log("퍼즐 상태 리셋 완료");
    }

}

using UnityEngine;

public class PuzzleUIController : MonoBehaviour
{
    public GameObject puzzleUICanvas;

    private void Awake()
    {
        if (puzzleUICanvas != null)
            puzzleUICanvas.SetActive(false);
    }

    // ⬇ 외부에서 직접 호출되는 메서드
    public void HandleFadeComplete()
    {
        string mapID = MapManager.Instance.currentMapID;
        bool shouldShow = mapID == "005" || mapID == "008" || mapID == "002";

        Debug.Log(" 퍼즐 UI 표시 여부: " + shouldShow);
        puzzleUICanvas.SetActive(shouldShow);
    }
}

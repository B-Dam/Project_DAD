using UnityEngine;

public class PuzzleUIController : MonoBehaviour
{
    public GameObject puzzleUICanvas;

    private void Awake()
    {
        if (puzzleUICanvas != null)
            puzzleUICanvas.SetActive(false);
    }


    private void OnEnable()
    {
        MapTransition.OnMapTransitionComplete += OnMapTransitionComplete;
    }

    private void OnDisable()
    {
        MapTransition.OnMapTransitionComplete -= OnMapTransitionComplete;
    }
    private void OnMapTransitionComplete()
    {
        string currentMapID = MapManager.Instance.currentMapID;
        bool isPuzzle = PuzzleManager.Instance.IsPuzzleMap(currentMapID);

        // UI On/Off는 자동 처리하고 싶으면 여기서 SetActive도 가능
        puzzleUICanvas.SetActive(isPuzzle);
    }
}

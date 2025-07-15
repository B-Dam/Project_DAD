using UnityEngine;

public class PuzzleUIController : MonoBehaviour
{
    public GameObject specialUICanvas;

    void Update()
    {
        if (MapManager.Instance != null && specialUICanvas != null)
        {
            // currentMapID가 "105" 또는 "108"일 때만 UI 활성화
            bool shouldShow = MapManager.Instance.currentMapID == "105" ||
                              MapManager.Instance.currentMapID == "108";
            if (specialUICanvas.activeSelf != shouldShow)
            {
                specialUICanvas.SetActive(shouldShow);
            }
        }
    }
}

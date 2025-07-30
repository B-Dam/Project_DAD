using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ResetGame : MonoBehaviour
{
    public Button resetButton;

    public GameObject mapPrefab005;
    public Transform map005ParentInScene;

    public GameObject mapPrefab008;
    public Transform map008ParentInScene;

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetCurrentMap);
        }
        else
        {
            Debug.LogWarning("Reset 버튼이 연결되지 않았습니다.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCurrentMap();
            Debug.Log("R 키로 맵 리셋 시도됨");
        }
    }

    public void ResetCurrentMap()
    {
        string currentID = MapManager.Instance.currentMapID;
        Debug.Log($"[Reset] currentMapID = {currentID}");

        Time.timeScale = 1f;

        if (currentID == "005")
        {
            ResetMap(map005ParentInScene, mapPrefab005, "005", "005");
        }
        else if (currentID == "008")
        {
            ResetMap(map008ParentInScene, mapPrefab008, "008", "008");
        }
    }
    private void ResetMap(Transform mapParent, GameObject mapPrefab, string newMapName, string mapID)
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (mapParent != null && mapParent.gameObject.scene.IsValid())
        {
            spawnPos = mapParent.position;
            spawnRot = mapParent.rotation;

            Transform puzzle = null;
            foreach (Transform child in mapParent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == MapManager.Instance.currentMapID)
                {
                    puzzle = child;
                    Destroy(puzzle.gameObject);
                    Debug.Log("Puzzle 오브젝트 제거됨");
                    break;
                }
            }

            if (puzzle == null)
            {
                Debug.LogWarning("Puzzle 오브젝트가 존재하지 않습니다.");
            }
        }

        GameObject newMap = Instantiate(mapPrefab, spawnPos, spawnRot, mapParent);
        newMap.name = newMapName;

        // DP 위치 찾기
        string previousID = MapManager.Instance.prevMapID;
        string expectedParentName = $"Map{mapID}ToMap{previousID}";
        Transform dpParent = newMap.transform.Find(expectedParentName);

        if (dpParent != null)
        {
            Transform dp = dpParent.Find("DP");
            if (dp != null)
            {
                PlayerController.Instance.transform.position = dp.position;
                Debug.Log($"플레이어 위치를 DP로 이동: {dp.position}");
            }
            else
            {
                Debug.LogWarning("DP 오브젝트를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning($"{expectedParentName} 오브젝트를 찾을 수 없습니다!");
        }

        HintMode hintMode = Object.FindFirstObjectByType<HintMode>();
        if (hintMode != null)
        {
            hintMode.ForceResetHintState();
            Debug.Log("힌트 종료 완료");
        }
        else
        {
            Debug.LogWarning("HintMode 오브젝트를 찾을 수 없습니다!");
        }

        Debug.Log($"맵 {newMapName} 리셋 완료");
    }
}

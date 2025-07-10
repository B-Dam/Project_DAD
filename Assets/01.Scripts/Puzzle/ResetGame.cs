using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetGame : MonoBehaviour
{
    public Button resetButton; // Inspector에 할당할 버튼

    public GameObject mapPrefab105;             // 105번 맵 프리팹
    public Transform map105ParentInScene;       // 현재 하이라키에 존재하는 105번 맵 오브젝트;

    public GameObject mapPrefab108;             // 108번 맵 프리팹
    public Transform map108ParentInScene;       // 현재 하이라키에 존재하는 108번 맵 오브젝트;

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
        // 키보드 R 키로도 리셋
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

        // 혹시 멈춰 있던 게임이라면 시간 재개
        Time.timeScale = 1f;

        // 현재 맵이 105번일 때만 리셋
        if (currentID == "105")
        {
            if (map105ParentInScene != null)
            {
                Vector3 spawn105Pos = Vector3.zero;
                Quaternion spawn105Rot = Quaternion.identity;
                // 씬 안의 객체일 때만 Destroy
                if (map105ParentInScene.gameObject.scene.IsValid())
                {
                    spawn105Pos = map105ParentInScene.position;
                    spawn105Rot = map105ParentInScene.rotation;
                    Destroy(map105ParentInScene.gameObject);
                }
                else
                {
                    Debug.LogWarning("[Reset] map105ParentInScene이 씬 오브젝트가 아닙니다. Destroy 생략");
                }
            // 프리팹을 새로 생성 (Hierarchy 상에서 똑같은 위치에 넣고 싶다면 위치 저장 후 복원 가능)
            GameObject newMap = Instantiate(mapPrefab105, spawn105Pos, spawn105Rot);
            newMap.name = "105";  // 이름 다시 지정해서 접근 쉽게
            Debug.Log("맵 105 리셋 완료");

            // 만약 리셋 후 새 오브젝트 참조도 업데이트하고 싶다면:
            map105ParentInScene = newMap.transform;

            // 플레이어 위치도 초기화
            PlayerController.Instance.transform.position = MapManager.Instance.mapData.player_position_down;

                HintMode hintMode = Object.FindFirstObjectByType<HintMode>();
                if (hintMode != null)
                {
                    hintMode.ForceResetHintState();
                    Debug.Log("힌트 사용 횟수 초기화 완료");
                }
                else
                {
                    Debug.LogWarning("HintMode 오브젝트를 찾을 수 없습니다!");
                }

                Debug.Log("맵 105 리셋 완료");
            
        }
        

    }  
        // 맵 108번 (ID: "108") 리셋
        else if (currentID == "108")
        {
            Vector3 spawn108Pos = Vector3.zero;
            Quaternion spawn108Rot = Quaternion.identity;
            if (map108ParentInScene != null && map108ParentInScene.gameObject.scene.IsValid())
            {
                spawn108Pos = map108ParentInScene.position;
                spawn108Rot = map108ParentInScene.rotation;
                Destroy(map108ParentInScene.gameObject);
            }

            GameObject newMap = Instantiate(mapPrefab108, spawn108Pos, spawn108Rot);
            newMap.name = "108";
            map108ParentInScene = newMap.transform;

            PlayerController.Instance.transform.position = MapManager.Instance.mapData.player_position_left;
            Debug.Log("맵 108 리셋 완료");
            map108ParentInScene = newMap.transform;

            HintMode hintMode = Object.FindFirstObjectByType<HintMode>();
            if (hintMode != null)
            {
                hintMode.ForceResetHintState();
                Debug.Log("힌트 사용 횟수 초기화 완료");
            }
            else
            {
                Debug.LogWarning("HintMode 오브젝트를 찾을 수 없습니다!");
            }

            Debug.Log("맵 108 리셋 완료");
        }
    }
}
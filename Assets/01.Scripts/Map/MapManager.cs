using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<MapManager>();

            if (_instance == null)
                _instance = new GameObject("MapManager").AddComponent<MapManager>();

            return _instance;
        }
    }
    public Transform currentMapTransform { get; private set; }
    public void SetCurrentMapTransform(Transform mapTransform)
    {
        currentMapTransform = mapTransform;
    }
    public string prevMapID;
    public string currentMapID = "001";  //d 첫 시작 시 001로 설정


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
  
    private void Start()
    {
        // 씬에 이미 존재하는 맵 오브젝트를 찾아 세팅
        Transform mapTransform = GameObject.Find($"{currentMapID}")?.transform;
        if (mapTransform != null)
        {
            SetCurrentMapTransform(mapTransform);
            UpdateMapData(currentMapID);
        }
        else
        {
            Debug.LogError($"초기 맵 Map_{currentMapID} 을 씬에서 찾을 수 없습니다!");
        }

        // ✅ 기본 BGM 설정 (처음 시작 시 필수)
        AudioManager.Instance.PlayBGM("MapBGM");

    }

    public void UpdateMapData(string newMapID)
    {
        prevMapID = currentMapID;
        currentMapID = newMapID;

        Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");

        // currentMapTransform 설정
        Transform mapTransform = GameObject.Find(newMapID)?.transform;
        Debug.Log(mapTransform);
        if (mapTransform != null)
        {
            SetCurrentMapTransform(mapTransform);
        }
        else
        {
            Debug.LogError($"❌ {newMapID} 이름의 맵 Transform을 찾을 수 없습니다!");
            return;
        }

        //  Confiner 갱신 위임
        var confinerUpdater = FindAnyObjectByType<CameraConfinerUpdater>();
        if (confinerUpdater != null)
        {
            confinerUpdater.UpdateConfinerFor(currentMapTransform);
        }
        else
        {
            Debug.LogWarning("❗ CameraConfinerUpdater를 찾을 수 없습니다.");
        }
        // BGM 설정
        string targetBGM = PuzzleManager.Instance.IsPuzzleMap(currentMapID) ? "PuzzleMapBGM" : "MapBGM";
        if (AudioManager.Instance != null && AudioManager.Instance.currentBGMName != targetBGM)
        {
            AudioManager.Instance.PlayBGM(targetBGM);
        }

       
    }
    //public void UpdateMapData(string newMapID)
    //{
    //   prevMapID = currentMapID;
    //    currentMapID = newMapID;
    //    //mapData = Database.Instance.Map.GetMapData(currentMapID);
    //    Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");
    //    //PuzzleActiveCheck();

    //    var updater = FindAnyObjectByType<CameraConfinerUpdater>();
    //    Debug.Log("현재 currentMapTransform 값: " + currentMapTransform);

    //    string targetBGM;

    //    if (currentMapID == "005" || currentMapID == "008")
    //    {
    //        targetBGM = "Puzzle_Sound";
    //    }
    //    else
    //    {
    //        targetBGM = "LostSouls";
    //    }

    //    if (AudioManager.Instance != null && AudioManager.Instance.currentBGMName != targetBGM)
    //    {
    //        AudioManager.Instance.PlayBGM(targetBGM);
    //    }
    //    Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");
    //    CameraConfinerUpdater updater = FindAnyObjectByType<CameraConfinerUpdater>();
    //    Debug.Log("현재 currentMapTransform 값: " + currentMapTransform);
    //    if (updater != null && currentMapTransform != null)
    //    {
    //        Debug.Log("📷 Confiner 갱신 시도");
    //        updater.SetConfinerToNewMap(currentMapTransform.gameObject);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("❌ Confiner 갱신 실패 - updater 또는 currentMapTransform이 null");
    //    }
    //}

    //private void PuzzleActiveCheck()
    //{
    //    if (mapData == null)
    //    {
    //        Debug.LogError("[MapManager] mapData가 null입니다. currentMapID: " + currentMapID);
    //        return;
    //    }

    //    // 만약 현재 맵 타입이 puzzle이라면
    //    if (mapData.type == MapType.puzzle)
    //    {
    //        GameObject puzzleObj = GameObject.Find(currentMapID)?.transform.parent.Find("Puzzle")?.gameObject;
    //        ResetGame resetGame = FindAnyObjectByType<ResetGame>();

    //        // bool isClear를 검수하고 false라면 퍼즐 오브젝트를 활성화
    //        puzzleClearStatus.TryGetValue(currentMapID, out bool isClear);
    //        if (isClear == false)
    //        {
    //            puzzleObj?.SetActive(true);
    //            //resetGame?.ResetCurrentMap();
    //            return;
    //        }
    //        else
    //        {
    //            puzzleObj?.SetActive(false);
    //            return;
    //        }
    //    }
    //    else
    //    {
    //        return;
    //    }
    //}

    //public bool isPuzzleUIActive()
    //{
    //    if (mapData.type == MapType.puzzle)
    //    {
    //        puzzleClearStatus.TryGetValue(currentMapID, out bool isClear);
    //        if (isClear == false)
    //            return true;
    //        else
    //            return false;
    //    }
    //    else
    //        return false;
    //}
    //// 퍼즐 클리어 했을 때 puzzleClearStatus를 true로 변경하는 함수
    //public void PuzzleClear()
    //{
    //    puzzleClearStatus[currentMapID] = true;
    //}

    //public IEnumerator OnLeftMap()
    //{
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
    //    UpdateMapData(mapData.left_map);
    //    PlayerController.Instance.transform.position = mapData.player_position_right;
    //    yield return new WaitForSeconds(0.5f);
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    //}
    //public IEnumerator OnRightMap()
    //{
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
    //    UpdateMapData(mapData.right_map);
    //    PlayerController.Instance.transform.position = mapData.player_position_left;
    //    yield return new WaitForSeconds(0.5f);
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    //}
    //public IEnumerator OnUpMap()
    //{
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
    //    UpdateMapData(mapData.up_map);
    //    PlayerController.Instance.transform.position = mapData.player_position_down;
    //    yield return new WaitForSeconds(0.5f);
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    //}
    //public IEnumerator OnDownMap()
    //{
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
    //    UpdateMapData(mapData.down_map);
    //    PlayerController.Instance.transform.position = mapData.player_position_up;
    //    yield return new WaitForSeconds(0.5f);
    //    yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    //}
}

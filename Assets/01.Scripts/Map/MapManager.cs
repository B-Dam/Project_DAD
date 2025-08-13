using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public string currentMapID = "001"; // 첫 시작 시 001로 설정


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
            //Debug.LogError($"초기 맵 Map_{currentMapID} 을 씬에서 찾을 수 없습니다!");
        }

        // ✅ 기본 BGM 설정 (처음 시작 시 필수)
        AudioManager.Instance.PlayBGM("MapBGM");
    }

    public void UpdateMapData(string newMapID)
    {
        prevMapID = currentMapID;
        currentMapID = newMapID;

        //Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");

        // currentMapTransform 설정
        Transform mapTransform = GameObject.Find(newMapID)?.transform;
        //Debug.Log(mapTransform);
        if (mapTransform != null)
        {
            SetCurrentMapTransform(mapTransform);
        }
        else
        {
            //Debug.LogError($"❌ {newMapID} 이름의 맵 Transform을 찾을 수 없습니다!");
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
            //Debug.LogWarning("❗ CameraConfinerUpdater를 찾을 수 없습니다.");
        }

        // BGM 설정
        string targetBGM = PuzzleManager.Instance.IsPuzzleMap(currentMapID) ? "PuzzleMapBGM" : "MapBGM";
        if (AudioManager.Instance != null && AudioManager.Instance.currentBGMName != targetBGM)
        {
            AudioManager.Instance.PlayBGM(targetBGM);
        }
    }

    public void ReapplyForCurrentMap()
    {
        //Debug.Log($"[Map] Reapply current={currentMapID}");

        // 1) Confine_{ID} 오브젝트의 Collider2D 찾기
        var area = GameObject.Find($"Confine_{currentMapID}");
        var poly = area ? area.GetComponent<Collider2D>() : null;
        if (poly == null)
        {
            //Debug.LogWarning($"[Map] Confine_{currentMapID} Collider2D 없음");
            return;
        }

        // 2) Cinemachine Confiner(2D/구버전) 중 살아있는 것을 찾아 바인딩
        var cmAsm = AppDomain.CurrentDomain.GetAssemblies()
                             .FirstOrDefault(a =>
                                 a.GetName().Name.StartsWith("Cinemachine", StringComparison.OrdinalIgnoreCase));
        if (cmAsm == null)
        {
            //Debug.LogWarning("[Map] Cinemachine 어셈블리 없음");
            return;
        }

        var tConf2D = cmAsm.GetType("Cinemachine.CinemachineConfiner2D");
        var tConf = cmAsm.GetType("Cinemachine.CinemachineConfiner");

        Component conf = null;
        if (tConf2D != null) conf = FindObjectOfType(tConf2D) as Component;
        if (conf == null && tConf != null) conf = FindObjectOfType(tConf) as Component;

        if (conf == null)
        {
            //Debug.LogWarning("[Map] Confiner(2D) 컴포넌트 없음");
            return;
        }

        // m_BoundingShape2D 필드 세팅
        var fld = conf.GetType().GetField("m_BoundingShape2D",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        fld?.SetValue(conf, poly);

        // 캐시 무효화 (버전별 메서드 이름 대응)
        var inv = conf.GetType().GetMethod("InvalidateCache",
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  ?? conf.GetType().GetMethod("InvalidatePathCache",
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        inv?.Invoke(conf, null);

        // BGM 재적용
        string targetBGM = PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleMap(currentMapID)
            ? "PuzzleMapBGM"
            : "MapBGM";
        if (AudioManager.Instance != null && AudioManager.Instance.currentBGMName != targetBGM)
            AudioManager.Instance.PlayBGM(targetBGM);
    }
    
    public void SwitchMapAndApplySettings(string newMapID)
    {
        // 맵 ID 갱신
        prevMapID = currentMapID;
        currentMapID = newMapID;
    
        // 새로운 맵 찾아서 활성화하고 Transform 갱신
        Transform newMapObject = GameObject.Find(newMapID)?.transform;
        if (newMapObject != null)
        {
            newMapObject.gameObject.SetActive(true);
            SetCurrentMapTransform(newMapObject);
        }
        else
        {
            return; // 맵을 못찾으면 더 진행하지 않음
        }
    
        // 모든 설정 재적용 (기존 ReapplyForCurrentMap 호출)
        ReapplyForCurrentMap();
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
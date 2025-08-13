using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Reflection;

public class CameraConfinerUpdater : MonoBehaviour
{
    [Header("참조")] 
    public CinemachineConfiner2D confiner; // VCam에 붙은 Confiner2D

    [Header("워프 판정/복구 세팅")] 
    public float warpDistance = 5f; // 이보다 멀면 워프 취급
    public float enableDistance = 1.5f; // 카메라-타깃 거리 이하면 Confiner 재활성
    public int maxWaitFrames = 30; // 안전장치
    
    Coroutine _refreshRoutine;

    public void UpdateConfinerFor(Transform mapTransform)
    {
        if (!mapTransform) return;
        RefreshById(mapTransform.name);
    }

    public void RefreshById(string mapId)
    {
        var go = GameObject.Find($"Cameras/MapCollider/{mapId}_Collider");
        var col = go ? go.GetComponent<Collider2D>() : null;
        if (!col) return;

        // 기존 실행 중단 후 새로 시작 (중복 방지)
        if (_refreshRoutine != null) StopCoroutine(_refreshRoutine);
        _refreshRoutine = StartCoroutine(CoRefreshSmart(col));
    }

    private IEnumerator CoRefreshSmart(Collider2D col)
    {
        if (!confiner) yield break;

        // 내 VCam과 현재 활성 VCam 파악
        var myVcam = GetVCamComponent(confiner.gameObject); // confiner가 붙은 그 VCam
        var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        var follow = GetFollowTarget(myVcam);
        var myTf = confiner.transform;

        // 모든 VCam의 우선순위 백업 → 내 것 최상위로 올림 (브레인은 끄지 않음)
        var prios = BumpPriorityOnlyMine(myVcam, 10000);

        // 내가 직접 껐는지 여부만 기억 (중첩 호출 대비)
        bool turnedOffByMe = confiner.enabled; // 켜져 있으면 내가 끄는 것
        confiner.enabled = false;

        confiner.BoundingShape2D = col;
        TryInvalidate(confiner);
        TrySetPrevStateInvalid(myVcam);
        yield return null;
        
        // 우선순위 올려둔 상태에서 "강제 스냅"을 먼저 실행
        if (myTf && follow)
        {
            var pos = myTf.position;
            var pos2 = new Vector2(pos.x, pos.y);
            bool outside = !col.OverlapPoint(pos2);
            float dist = Vector2.Distance(pos2, (Vector2)follow.position);

            if (outside || dist > warpDistance)
            {
                // 가능한 경우 ForceCameraPosition 사용 (없으면 트랜스폼 직접 이동)
                if (!TryForceCameraPosition(myVcam, new Vector3(follow.position.x, follow.position.y, pos.z),
                        myTf.rotation))
                    myTf.position = new Vector3(follow.position.x, follow.position.y, pos.z);

                TryOnTargetWarped(myVcam, follow, follow.position - pos);
                TrySetPrevStateInvalid(myVcam);
            }
        }

        // 타깃 근처까지 붙을 때까지 잠깐 대기(최대 프레임)
        int frames = 0;
        while (myTf && follow &&
               Vector3.Distance(myTf.position, follow.position) > enableDistance &&
               frames++ < maxWaitFrames)
        {
            TrySetPrevStateInvalid(myVcam);
            yield return null;
        }

        // 내가 껐던 경우에만 다시 킴 (다른 곳에서 원래 꺼놨다면 존중)
        if (turnedOffByMe) confiner.enabled = true;
        TryInvalidate(confiner);
        
        yield return null;
        
        TrySetPrevStateInvalid(myVcam);

        // 우선순위 복원
        RestorePriorities(prios);
        _refreshRoutine = null;
    }

    // === 유틸(리플렉션으로 CM2/CM3 모두 커버) ===
    System.Collections.Generic.List<Component> GetAllVCams()
    {
        var list = new System.Collections.Generic.List<Component>();
        foreach (var cam in Object.FindObjectsOfType<Component>(true))
        {
            if (cam == null) continue;
            var n = cam.GetType().Name;
            if (n == "CinemachineVirtualCamera" || n == "CinemachineCamera")
                list.Add(cam);
        }

        return list;
    }

    System.Collections.Generic.Dictionary<Component, int> BumpPriorityOnlyMine(Component mine, int high)
    {
        var d = new System.Collections.Generic.Dictionary<Component, int>();
        foreach (var v in GetAllVCams())
        {
            int p = GetPriority(v);
            d[v] = p;
            SetPriority(v, v == mine ? high : Mathf.Min(p, 0)); // 내 것만 매우 높게, 나머진 낮춤
        }

        return d;
    }

    void RestorePriorities(System.Collections.Generic.Dictionary<Component, int> map)
    {
        foreach (var kv in map) SetPriority(kv.Key, kv.Value);
    }

    int GetPriority(Component vcam)
    {
        if (!vcam) return 0;
        var p = vcam.GetType().GetProperty("Priority");
        return p != null ? (int)p.GetValue(vcam) : 0;
    }

    void SetPriority(Component vcam, int value)
    {
        if (!vcam) return;
        var p = vcam.GetType().GetProperty("Priority");
        if (p != null && p.CanWrite) p.SetValue(vcam, value);
    }

    Component GetVCamComponent(GameObject go)
    {
        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null) continue;
            var n = c.GetType().Name;
            if (n == "CinemachineVirtualCamera" || n == "CinemachineCamera") return c;
        }

        return null;
    }

    Component GetActiveVCam(CinemachineBrain brain)
    {
        if (!brain) return null;
        var p = brain.GetType().GetProperty("ActiveVirtualCamera");
        var icam = p?.GetValue(brain); // ICinemachineCamera
        var goP = icam?.GetType().GetProperty("VirtualCameraGameObject");
        var go = goP != null ? (GameObject)goP.GetValue(icam) : null;
        return go ? GetVCamComponent(go) : null;
    }

    string GetActiveVCamName(CinemachineBrain brain)
    {
        var a = GetActiveVCam(brain);
        return a ? a.name : "None";
    }

    Transform GetFollowTarget(Component vcam)
    {
        if (!vcam) return null;
        var p = vcam.GetType().GetProperty("Follow");
        return p != null ? (Transform)p.GetValue(vcam) : null;
    }

    bool TryForceCameraPosition(Component vcam, Vector3 pos, Quaternion rot)
    {
        if (!vcam) return false;
        var m = vcam.GetType().GetMethod("ForceCameraPosition", new[] { typeof(Vector3), typeof(Quaternion) });
        if (m == null) return false;
        m.Invoke(vcam, new object[] { pos, rot });
        return true;
    }

    void TryOnTargetWarped(Component vcam, Transform target, Vector3 delta)
    {
        if (!vcam || !target) return;
        var m = vcam.GetType().GetMethod("OnTargetObjectWarped", new[] { typeof(Transform), typeof(Vector3) });
        m?.Invoke(vcam, new object[] { target, delta });
    }

    void TrySetPrevStateInvalid(Component vcam)
    {
        if (!vcam) return;
        var p = vcam.GetType().GetProperty("PreviousStateIsValid");
        if (p != null && p.CanWrite) p.SetValue(vcam, false);
    }

    private static void TryInvalidate(CinemachineConfiner2D conf)
    {
        var t = conf.GetType();
        var m = t.GetMethod("InvalidatePathCache") ?? t.GetMethod("InvalidateCache");
        m?.Invoke(conf, null);
    }
}
// 기존 스크립트
/*public CinemachineConfiner2D confiner; // 메인 카메라의 Confiner2D
public void UpdateConfinerFor(Transform mapTransform)
{
    string mapID = mapTransform.name;
    string path = $"Cameras/MapCollider/{mapID}_Collider";

    var colliderObj = GameObject.Find(path);
    if (colliderObj == null)
    {
        Debug.LogWarning($" Collider 오브젝트를 찾을 수 없습니다: {path}");
        return;
    }

    var poly = colliderObj.GetComponent<PolygonCollider2D>();
    if (poly == null)
    {
        Debug.LogWarning(" PolygonCollider2D를 찾을 수 없습니다. 대상: " + mapTransform.name);
        return;
    }

    confiner.BoundingShape2D = null;
    confiner.InvalidateBoundingShapeCache();

    confiner.BoundingShape2D = poly;
    confiner.InvalidateBoundingShapeCache();

    Debug.Log(" Confiner 경계 업데이트 완료");
}*/


//public Transform player; // 플레이어 (혹은 맵 트리거 기준)

//// 외부에서 호출 시 StartCoroutine으로 감싸기
//public void SetConfinerToNewMap(GameObject newMap)
//{
//    string mapName = MapManager.Instance.currentMapID;
//    //GameObject mapObj = GameObject.Find("Cameras/MapCollider/"+mapName + "_Collider");
//    string path = "Cameras/MapCollider/" + mapName + "_Collider";
//    GameObject mapObj = GameObject.Find(path);
//    Debug.Log("찾는 경로: " + path + " / 결과: " + (mapObj == null ? "못 찾음" : mapObj.name));

//    if (mapObj == null)
//    {
//        Debug.LogError($" 해당 이름의 맵 오브젝트를 찾을 수 없습니다: {mapName}_Collider");
//        return;
//    }

//  PolygonCollider2D poly = mapObj.GetComponentInChildren<PolygonCollider2D>();
//    if (poly == null)
//    {
//        Debug.LogError(" PolygonCollider2D를 찾을 수 없습니다.");
//        return;
//    }

//    // Rigidbody2D 없이 등록!
//    confiner.BoundingShape2D = null;
//    confiner.InvalidateBoundingShapeCache();

//    confiner.BoundingShape2D = poly;
//    confiner.InvalidateBoundingShapeCache();

//    Debug.Log(" PolygonCollider2D 등록 완료 (Rigidbody 없이 시도함)");
//}
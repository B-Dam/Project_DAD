using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;

/// 저장 포맷
[Serializable]
public struct CameraData
{
    public Vector3    position;
    public Quaternion rotation;
    public float      size;               // ortho면 orthographicSize, 아니면 fieldOfView
    public string[]   watchedTargetIDs;   // OffscreenWatcher 대상 UniqueID들
    public string     confineMapId;       // 저장 당시 카메라가 묶여 있던 맵 이름(예: "Map001")
}

/// Maps 루트에서 플레이어가 어느 맵 안에 있는지 판별하거나,
/// 맵 ID로 PolygonCollider2D(없으면 자동 생성)를 찾아주는 헬퍼
static class MapLookup
{
    public static bool TryFindMapByPosition(Transform mapsRoot, Vector2 pos,
                                            out string mapId, out PolygonCollider2D poly)
    {
        mapId = null; poly = null;
        if (!mapsRoot) return false;

        foreach (Transform child in mapsRoot)
        {
            var p = child.GetComponentInChildren<PolygonCollider2D>(true);
            if (!p) p = GetOrBuildBoundsCollider(child); // 폴리곤 없으면 자동 생성
            if (!p) continue;

            if (p.OverlapPoint(pos))
            {
                mapId = child.name;  // "Map001" 가정
                poly  = p;
                return true;
            }
        }
        return false;
    }

    public static PolygonCollider2D FindMapPolygonById(Transform mapsRoot, string mapId)
    {
        if (!mapsRoot || string.IsNullOrEmpty(mapId)) return null;
        var t = mapsRoot.Find(mapId);
        if (!t) return null;

        var p = t.GetComponentInChildren<PolygonCollider2D>(true);
        if (!p) p = GetOrBuildBoundsCollider(t); // 없으면 자동 생성
        return p;
    }

    /// 맵 하위에 렌더러 바운드로 사각형 폴리곤을 자동 생성/캐시
    public static PolygonCollider2D GetOrBuildBoundsCollider(Transform map, string childName="__AutoBounds")
    {
        if (!map) return null;

        var child = map.Find(childName);
        if (!child)
        {
            var go = new GameObject(childName);
            go.hideFlags = HideFlags.DontSave;
            go.layer = LayerMask.NameToLayer("Ignore Raycast"); // 충돌 방지용 권장
            go.transform.SetParent(map, false);
            child = go.transform;
        }

        var poly = child.GetComponent<PolygonCollider2D>();
        if (!poly) poly = child.gameObject.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;

        // 렌더러 바운드 합산 → 로컬 좌표 네 모서리
        var b = ComputeRenderersBoundsWorld(map);
        var min = map.InverseTransformPoint(b.min);
        var max = map.InverseTransformPoint(b.max);

        var pts = new Vector2[4]
        {
            new Vector2(min.x, min.y),
            new Vector2(min.x, max.y),
            new Vector2(max.x, max.y),
            new Vector2(max.x, min.y)
        };
        poly.pathCount = 1;
        poly.SetPath(0, pts);
        return poly;
    }

    static Bounds ComputeRenderersBoundsWorld(Transform root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        Bounds b = new Bounds(root.position, Vector3.zero);
        bool has = false;
        foreach (var r in renderers)
        {
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!has) b = new Bounds(root.position, Vector3.one);
        return b;
    }
}

[RequireComponent(typeof(OffscreenWatcher))]
[RequireComponent(typeof(UniqueID))]
[DefaultExecutionOrder(1100)] // 대부분 초기화가 끝난 뒤 적용되도록 약간 늦게
public class CameraSave : MonoBehaviour, ISaveable
{
    [Header("Maps 루트(미지정 시 'Maps'로 찾음)")]
    [SerializeField] private Transform mapsRoot;

    [Header("플레이어(미지정 시 Tag=Player 탐색)")]
    [SerializeField] private Transform player;

    private UniqueID                 idComp;
    private OffscreenWatcher         watcher;
    private Camera                   cam;
    private CinemachineConfiner2D    confiner;
    private bool                     isOrtho;

    private CameraData pending;
    private bool       hasPending;

    public string UniqueID
    {
        get
        {
            if (!idComp)
            {
                idComp = GetComponent<UniqueID>();
                if (!idComp) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    void Awake()
    {
        idComp  = GetComponent<UniqueID>();
        watcher = GetComponent<OffscreenWatcher>();
        cam     = GetComponent<Camera>();
        isOrtho = cam && cam.orthographic;

        // Confiner는 보통 VCam에 붙어 있으니 현재 오브젝트/자식에서 모두 시도
        confiner = GetComponent<CinemachineConfiner2D>();
        if (!confiner) confiner = GetComponentInChildren<CinemachineConfiner2D>(true);

        if (!mapsRoot)
        {
            var go = GameObject.Find("Maps");
            if (go) mapsRoot = go.transform;
        }
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void OnEnable()  => MapsReadyBroadcaster.OnMapsReady += TryApplyPending;
    void OnDisable() => MapsReadyBroadcaster.OnMapsReady -= TryApplyPending;

    public object CaptureState()
    {
        // 플레이어 위치로 현재 맵 판별 → confineMapId 저장
        string mapId = null;
        if (player && mapsRoot)
            MapLookup.TryFindMapByPosition(mapsRoot, (Vector2)player.position, out mapId, out _);

        // 보강: 현재 confiner가 물고 있는 폴리곤 루트 이름
        if (string.IsNullOrEmpty(mapId) && confiner && confiner.BoundingShape2D)
            mapId = confiner.BoundingShape2D.transform.root.name;

        float sizeVal = 0f;
        if (cam) sizeVal = isOrtho ? cam.orthographicSize : cam.fieldOfView;

        return new CameraData
        {
            position         = transform.position,
            rotation         = transform.rotation,
            size             = sizeVal,
            watchedTargetIDs = watcher ? watcher.GetWatchedIDs() : null,
            confineMapId     = mapId
        };
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        pending    = JsonUtility.FromJson<CameraData>(json);
        hasPending = true;

        // 맵이 이미 준비됐을 수도 있으니 즉시 시도
        TryApplyPending();
    }

    private void TryApplyPending()
    {
        if (!hasPending) return;
        // 경계/다른 컴포넌트 Start/Confiner invalidate 이후에 적용되도록 한 프레임 미룸
        StartCoroutine(ApplyNextFrame());
    }

    private IEnumerator ApplyNextFrame()
    {
        yield return null; // 필요하면 2~3프레임로 늘려도 OK
        ApplyNow();
    }

    private void ApplyNow()
    {
        // 1) 경계(클램프) 먼저 세팅
        if (confiner && !string.IsNullOrEmpty(pending.confineMapId))
        {
            var poly = MapLookup.FindMapPolygonById(mapsRoot, pending.confineMapId);
            if (poly)
            {
                confiner.BoundingShape2D = poly;
                confiner.InvalidateBoundingShapeCache();
            }
        }

        // 2) 카메라 트랜스폼/사이즈 적용
        transform.position = pending.position;
        transform.rotation = pending.rotation;

        if (cam)
        {
            if (isOrtho) cam.orthographicSize = pending.size;
            else         cam.fieldOfView      = pending.size;
        }

        // 3) OffscreenWatcher 대상 복원 (Core 레지스트리 우선)
        if (watcher)
        {
            watcher.ClearAllTargets();
            var ids = pending.watchedTargetIDs;
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    Transform tr = null;

                    // 최적: SaveLoadManagerCore 레지스트리
                    var core = SaveLoadManagerCore.Instance;
                    if (core != null)
                    {
                        // TryGetSaveable 제공한다면 사용
                        var tryGet = core.GetType().GetMethod("TryGet",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (tryGet != null)
                        {
                            object[] args = new object[] { id, null };
                            bool ok = (bool)tryGet.Invoke(core, args);
                            if (ok && args[1] is ISaveable sv && sv is MonoBehaviour mb1)
                                tr = mb1.transform;
                        }
                    }

                    // Fallback: 느리지만 안전한 전역 탐색(로드 단발성이므로 OK)
                    if (!tr)
                    {
                        var mb = GameObject.FindObjectsOfType<MonoBehaviour>(true)
                                 .OfType<ISaveable>()
                                 .FirstOrDefault(s => s.UniqueID == id) as MonoBehaviour;
                        if (mb) tr = mb.transform;
                    }

                    if (tr) watcher.Register(tr);
                }
            }
        }

        hasPending = false;
        Debug.Log($"[CameraSave] Restored. confineMapId={pending.confineMapId}");
    }
}

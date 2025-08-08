using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct CameraData
{
    public Vector3 position;
    public Quaternion rotation;
    public float size;                 // 추가 (ortho면 orthographicSize, 아니면 FOV)
    public string[] watchedTargetIDs;  // OffscreenWatcher 대상들
}

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(OffscreenWatcher))]
[RequireComponent(typeof(UniqueID))]
public class CameraSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;
    private OffscreenWatcher watcher;
    private Camera cam;

    // Core가 JSON 문자열(state)을 넘기므로, 여기선 DTO로 한번 더 파싱
    private CameraData pending;
    private bool hasPending;

    // ISaveable
    public string UniqueID
    {
        get
        {
            if (idComp == null)
                idComp = GetComponent<UniqueID>() ?? gameObject.AddComponent<UniqueID>();
            return idComp.ID;
        }
    }

    private void Awake()
    {
        idComp  = GetComponent<UniqueID>();
        watcher = GetComponent<OffscreenWatcher>();
        cam     = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        MapsReadyBroadcaster.OnMapsReady += TryApplyPending;
    }

    private void OnDisable()
    {
        MapsReadyBroadcaster.OnMapsReady -= TryApplyPending;
    }

    // 저장
    public object CaptureState()
    {
        return new CameraData
        {
            position = transform.position,
            rotation = transform.rotation,
            size     = cam != null && cam.orthographic ? cam.orthographicSize
                                                       : (cam != null ? cam.fieldOfView : 0f),
            watchedTargetIDs = watcher != null ? watcher.GetWatchedIDs() : null
        };
    }

    // 불러오기(즉시 적용 하지 않고 일단 보관)
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        pending    = JsonUtility.FromJson<CameraData>(json);
        hasPending = true;

        // 맵이 이미 준비된 상태라면 바로 시도
        TryApplyPending();
    }

    // 맵/경계 준비 이후에 한 번만 정확히 적용
    private void TryApplyPending()
    {
        if (!hasPending) return;

        // 한 프레임 정도 더 미뤄 안정화 (경계 세팅/타 컴포넌트 Start 이후)
        StartCoroutine(ApplyNextFrame());
    }

    private IEnumerator ApplyNextFrame()
    {
        yield return null; // 필요하면 2~3프레임 더 늘리기
        ApplyNow();
    }

    private void ApplyNow()
    {
        hasPending = false;

        // 위치·회전·사이즈
        transform.position = pending.position;
        transform.rotation = pending.rotation;
        if (cam != null)
        {
            if (cam.orthographic) cam.orthographicSize = pending.size;
            else                   cam.fieldOfView      = pending.size;
        }

        // OffscreenWatcher 대상 복원
        if (watcher != null)
        {
            watcher.ClearAllTargets();

            var ids = pending.watchedTargetIDs;
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (string.IsNullOrEmpty(id)) continue;

                    // 안전하게 탐색
                    var mb = FindObjectsOfType<MonoBehaviour>(true)
                             .OfType<ISaveable>()
                             .FirstOrDefault(s => s.UniqueID == id) as MonoBehaviour;

                    if (mb != null) watcher.Register(mb.transform);
                }
            }
        }

        Debug.Log("[CameraSave] restored after MapsReady.");
    }
}
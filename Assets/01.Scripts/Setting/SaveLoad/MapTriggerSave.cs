using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct TriggerData
{
    public bool isActive;
    public bool triggered;
}

[RequireComponent(typeof(UniqueID))]
public class MapTriggerSave : MonoBehaviour, ISaveable, IPostLoad
{
    private UniqueID idComp;
    
    [SerializeField] EventTriggerZone zone;     // 같은 오브젝트에 붙은 존 참조
    
    private TriggerData _pendingData;
    private bool _hasPending;
    
    public string UniqueID
    {
        get
        {
            if (idComp == null)
            {
                idComp = GetComponent<UniqueID>();
                //if (idComp == null) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    public object CaptureState() => new TriggerData
    {
        isActive = gameObject.activeSelf,
        triggered = zone != null && zone.HasTriggered(), // 추가 저장
    };

    public void RestoreState(object state)
    {
        var json = state as string; if (string.IsNullOrEmpty(json)) return;
        _pendingData = JsonUtility.FromJson<TriggerData>(json);
        _hasPending = true;
    }

    public void OnPostLoad()
    {
        if (!_hasPending) return;
        
        // 비활성 오브젝트에서 코루틴 시작 금지
        var core = SaveLoadManagerCore.Instance;
        if (core != null && core.isActiveAndEnabled)
        {
            core.StartCoroutine(CoApplyAfterSettle_Global(this));
        }
        else
        {
            // 극히 예외적인 경우(코어 없음)엔 즉시 적용으로 폴백
            ApplyPendingNow();
        }
    }

    // 코어가 돌리는 글로벌 코루틴 (self가 비활성이어도 안전)
    private static IEnumerator CoApplyAfterSettle_Global(MapTriggerSave self)
    {
        // 로드 정착이 끝날 때까지 대기
        while (SaveLoadManagerCore.IsRestoring)
            yield return null;

        // 맵 스위치/컨파이너 등 후처리 1프레임 더 대기
        yield return null;

        if (self == null) yield break;
        self.ApplyPendingNow();
    }

    private void ApplyPendingNow()
    {
        if (!_hasPending) return;

        if (!zone) zone = GetComponent<EventTriggerZone>();

        // ※ 멱등 적용: 순서는 트리거 플래그 → 활성 상태
        if (zone != null)
            zone.SetTriggered(_pendingData.triggered);

        if (gameObject.activeSelf != _pendingData.isActive)
            gameObject.SetActive(_pendingData.isActive);

        _hasPending = false;
    }
}
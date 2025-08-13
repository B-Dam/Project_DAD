using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(PuzzlePlayerResettable))]
public class PlayerResetSave : MonoBehaviour, ISaveable
{
    [Serializable]
    private class SaveWrapper
    {
        public int ver = 1;
        public List<PuzzlePlayerResettable.ResetPoint> points;
    }

    private UniqueID idComp;
    private PuzzlePlayerResettable resettable;

    public string UniqueID => (idComp ??= GetComponent<UniqueID>()).ID + ":PlayerReset";

    private void Awake()
    {
        idComp = GetComponent<UniqueID>();
        resettable = GetComponent<PuzzlePlayerResettable>();
    }

    // === 저장 ===
    public object CaptureState()
    {
        //// 최신 리셋 포인트가 비어있지 않도록 현재 맵 포인트를 한번 갱신해두면 안전합니다(선택).
        //if (MapManager.Instance != null)
        //    resettable.SaveResetPlayerPoint(MapManager.Instance.currentMapID);

        return new SaveWrapper
        {
            ver = 1,
            points = resettable.ExportPoints()
        };
    }

    // === 복원 ===
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<SaveWrapper>(json);
        resettable.ImportPoints(data?.points);
    }
}

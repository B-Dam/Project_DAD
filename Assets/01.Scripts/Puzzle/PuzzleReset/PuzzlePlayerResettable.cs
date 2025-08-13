using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePlayerResettable : MonoBehaviour
{

    // 맵 ID별로 초기 위치 저장
    private Dictionary<string, (Vector3 pos, Quaternion rot)> resetPoints = new();

    [Serializable]
    public struct ResetPoint
    {
        public string mapID;
        public Vector3 pos;
        public Quaternion rot;
    }
    /// <summary>
    /// 현재 위치를 특정 맵 ID 기준으로 저장
    /// </summary>
    public void SaveResetPlayerPoint(string mapID)
    {
        resetPoints[mapID] = (transform.position, transform.rotation);
        //Debug.Log($"[PlayerReset] {mapID} 위치 저장됨: {transform.position}");
    }

    /// <summary>
    /// 특정 맵 ID 기준으로 위치 리셋
    /// </summary>
    public void ResetPlayerState(string mapID)
    {
        if (resetPoints.TryGetValue(mapID, out var data))
        {
            transform.position = data.pos;
            transform.rotation = data.rot;
            //Debug.Log($"[PlayerReset] {mapID} 위치로 리셋됨: {data.pos}");
        }
        else
        {
            //Debug.LogWarning($"[PlayerReset] {mapID} 위치 정보 없음");
        }
    }
    // ✅ 추가 1) 세이브용: Dictionary → List 변환
    public List<ResetPoint> ExportPoints()
    {
        var list = new List<ResetPoint>(resetPoints.Count);
        foreach (var kv in resetPoints)
            list.Add(new ResetPoint { mapID = kv.Key, pos = kv.Value.pos, rot = kv.Value.rot });
        return list;
    }

    // ✅ 추가 2) 로드용: List → Dictionary 복원
    public void ImportPoints(List<ResetPoint> points)
    {
        resetPoints.Clear();
        if (points == null) return;
        foreach (var p in points)
            if (!string.IsNullOrEmpty(p.mapID))
                resetPoints[p.mapID] = (p.pos, p.rot);
    }
}
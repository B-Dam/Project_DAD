using System.Collections.Generic;
using UnityEngine;

public class PuzzlePlayerResettable : MonoBehaviour
{

    // 맵 ID별로 초기 위치 저장
    private Dictionary<string, (Vector3 pos, Quaternion rot)> resetPoints = new();

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
}
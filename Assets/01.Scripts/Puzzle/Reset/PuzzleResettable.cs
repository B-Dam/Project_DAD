using System.Collections.Generic;
using UnityEngine;

public class PuzzleResettable : MonoBehaviour
{
    //private Vector3 initialPosition;
    //private Quaternion initialRotation;
    private Dictionary<string, (Vector3 pos, Quaternion rot)> resetPoints = new();
    private HashSet<string> savedMapIDs = new();
    private void OnEnable()
    {
        MapTransition.OnMapTransitionComplete += SaveResetPuzzlePoint;
    }
    private void OnDisable()
    {
        MapTransition.OnMapTransitionComplete -= SaveResetPuzzlePoint;
    }
    public string mapID
    {
        get
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name.StartsWith("Map")) // Map001, Map002...
                {
                    return current.name;
                }
                current = current.parent;
            }
            return null;
        }
    }
    public void SaveResetPuzzlePoint()
    {
        string mapID = MapManager.Instance.currentMapID;
        if (!savedMapIDs.Contains(mapID))
        {
            resetPoints[mapID] = (transform.position, transform.rotation);
            savedMapIDs.Add(mapID);
            //Debug.Log($"[{name}] {mapID} 최초 위치 저장됨: {transform.position}");
        }
    }

    public void ResetPuzzleState(string mapID)
    {
        if (resetPoints.TryGetValue(mapID, out var data))
        {
            transform.position = data.pos;
            transform.rotation = data.rot;
        }
        else
        {
            //Debug.LogWarning($"[{name}] {mapID} 위치가 저장되지 않았습니다.");
        }
    }
    //void Awake()
    //{
    //    // 처음 상태 저장
    //    initialPosition = transform.position;
    //    initialRotation = transform.rotation;
    //}

    //public void ResetState()
    //{
    //    // 상태 복원
    //    transform.position = initialPosition;
    //    transform.rotation = initialRotation;
    //}
}

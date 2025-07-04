using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class V2MapBase : MonoBehaviour
{
    /// <summary>
    /// 새로 만들 스크립트 요약 (V2)
    /// 1. 맵 별로 좌표 영역 설정 V
    /// 2. 지정한 좌표 안에 있으면 currentMapID를 해당 맵으로 변경 V
    /// 2-1. 처음 시작했을 땐 001로 설정 V
    /// 2-2. 콜라이더로 이동시키면 currentMapID를 변경 V
    /// 2-3. 변경 방법 : 현재 맵(mapData)의 dir_map 변수에 따르기 V
    /// 3. 맵 방향에 맞는 콜라이더 오브젝트에 맵 이동 타일 그리기 (001, 002 V)
    /// 4. 스토리 버전과 퍼즐 버전이 있는 맵은 bool 값으로 처리해서 따로 띄우기 -
    /// 5. 카메라 이동하기, 타일맵 영역 내에서만 움직이기 - 
    /// </summary>

    // 타일맵 설정
    public List<Tilemap> tilemaps;
    private Dictionary<string, Bounds> mapBounds = new Dictionary<string, Bounds>();  // 맵아이디, 맵좌표영역 mapBounds

    // 카메라 변수
    private Vector3 camVelocity = Vector3.zero;
    private float smoothTime = 0.2f;
    public Camera cam;

    private void Awake()
    {
        MapManager.Instance.MapBase = this;
    }

    private void Start()
    {
        // 카메라 초기화
        cam = Camera.main;        

        // 타일맵들 좌표 영역 설정
        foreach (Tilemap tilemap in tilemaps)
        {
            string mapName = tilemap.gameObject.name;
            Bounds localBounds = tilemap.localBounds;
            Vector3 worldCenter = tilemap.transform.TransformPoint(localBounds.center);
            Bounds worldBounds = new Bounds(worldCenter, localBounds.size);
            mapBounds[mapName] = worldBounds;  // 각 맵의 좌표 영역 정보를 딕셔너리에 저장
        }

        foreach (var kvp in mapBounds)
        {
            Debug.Log($"맵 ID: {kvp.Key}, 좌표 영역: {kvp.Value}");
        }
    }

    private void LateUpdate()
    {
        if (!mapBounds.ContainsKey(MapManager.Instance.currentMapID)) return;

        Bounds bounds = mapBounds[MapManager.Instance.currentMapID];

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        Vector2 targetPos = PlayerController.Instance.transform.position;
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        Vector3 desiredPos = new Vector3(clampedX, clampedY, cam.transform.position.z);
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, desiredPos, ref camVelocity, smoothTime);

        Debug.Log($"[카메라 클램프] 맵ID: {MapManager.Instance.currentMapID}, bounds: {bounds.min.x} ~ {bounds.max.x}");
        Debug.Log($"halfWidth: {halfWidth}, targetX: {targetPos.x}, clampedX: {clampedX}");

    }

    public void CameraUpdate()
    {
        if (!mapBounds.ContainsKey(MapManager.Instance.currentMapID)) return;

        Bounds bounds = mapBounds[MapManager.Instance.currentMapID];

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        Vector2 targetPos = PlayerController.Instance.transform.position;
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        cam.transform.position = new Vector3(clampedX, clampedY, cam.transform.position.z);
    }

    // 맵 아이디를 키로 검색하면 해당 맵의 좌표값 반환
    public Bounds GetBounds(string mapID)
    {
        return mapBounds.ContainsKey(mapID) ? mapBounds[mapID] : new Bounds();
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class V2MapBase : MonoBehaviour
{
    /// <summary>
    /// 새로 만들 기능 정리
    /// </summary>

    // 타일맵 설정
    public List<Tilemap> tilemaps;
    private Dictionary<string, Bounds> mapBounds = new Dictionary<string, Bounds>();  // 맵아이디, 맵좌표영역 mapBounds

    // 카메라 변수
    private Vector3 camVelocity = Vector3.zero;
    private float smoothTime = 0.05f;
    //public Camera cam;

    private void Awake()
    {
        MapManager.Instance.MapBase = this;
    }

    private void Start()
    {
        // 카메라 초기화
        //cam = Camera.main;

        TileMapSetting();
    }

    public void TileMapSetting()
    {
        // MapBase에 타일맵 영역 찾아서 등록
        foreach (Tilemap tilemap in tilemaps)
        {
            //if (tilemap == null)
            //{

            //}

            tilemap.CompressBounds();  // 실제 타일이 있는 부분만 계산하는 기능

            string mapName = tilemap.gameObject.name;

            BoundsInt cellBounds = tilemap.cellBounds;
            Vector3 worldMin = tilemap.CellToWorld(cellBounds.min);
            Vector3 worldMax = tilemap.CellToWorld(cellBounds.max);
            Vector3 worldCenter = (worldMin + worldMax) / 2f;
            Vector3 worldSize = worldMax - worldMin;

            Bounds worldBounds = new Bounds(worldCenter, worldSize);
            mapBounds[mapName] = worldBounds;
        }
    }

    //private void LateUpdate()
    //{
    //    // currentMapID가 null이면 바로 리턴
    //    if (string.IsNullOrEmpty(MapManager.Instance.currentMapID))
    //        return;

    //    if (!mapBounds.ContainsKey(MapManager.Instance.currentMapID)) return;

    //    Bounds bounds = mapBounds[MapManager.Instance.currentMapID];
    //    float halfHeight = cam.orthographicSize;
    //    float halfWidth = halfHeight * cam.aspect;

    //    float minX = bounds.min.x + halfWidth;
    //    float maxX = bounds.max.x - halfWidth;
    //    float minY = bounds.min.y + halfHeight;
    //    float maxY = bounds.max.y - halfHeight;

    //    Vector2 targetPos = PlayerController.Instance.transform.position;
    //    float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
    //    float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

    //    Vector3 desiredPos = new Vector3(clampedX, clampedY, cam.transform.position.z);
    //    cam.transform.position = Vector3.SmoothDamp(cam.transform.position, desiredPos, ref camVelocity, smoothTime);
    //}

    // 맵 아이디를 키로 검색하면 해당 맵의 좌표값 반환
    public Bounds GetBounds(string mapID)
    {
        return mapBounds.ContainsKey(mapID) ? mapBounds[mapID] : new Bounds();
    }
}
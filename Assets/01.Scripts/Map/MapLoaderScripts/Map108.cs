using UnityEngine;

public class Map108 : MapBase
{


    protected override void OnLoadMap()
    {
        // 맵을 들어갈 때 작동하는 로직
        Vector3 currentSpawnPoint;
        if (MapManager.Instance.prevMapID != null && spawnPoint.TryGetValue(MapManager.Instance.prevMapID, out currentSpawnPoint))
        {
            PlayerController.Instance.playerTransform.position = currentSpawnPoint;
            PlayerController.Instance.playerTransform.localScale = MapManager.Instance.lastPlayerScale;
        }
    }

    public override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직

        MapManager.Instance.prevMapID = MapManager.Instance.currentMapID;
        Debug.Log($"Map002맵에서 나갔습니다 (이전 맵: {MapManager.Instance.prevMapID})");
    }


}

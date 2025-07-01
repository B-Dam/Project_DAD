using UnityEngine;

public class Map102 : MapBase
{


    protected override void OnLoadMap()
    {
        // 맵을 들어갈 때 작동하는 로직
        Vector3 currentSpawnPoint;
        if (prevMapID != null && spawnPoint.TryGetValue(prevMapID, out currentSpawnPoint))
        {
            PlayerController.Instance.playerTransform.position = currentSpawnPoint;
            PlayerController.Instance.playerTransform.localScale = MapManager.Instance.lastPlayerScale;
        }
    }

    public override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직

        prevMapID = MapManager.Instance.currentMapID;
        Debug.Log($"Map002맵에서 나갔습니다 (이전 맵: {prevMapID})");
    }


}

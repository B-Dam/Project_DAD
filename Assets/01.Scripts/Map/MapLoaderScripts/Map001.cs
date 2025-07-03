using UnityEngine;

public class Map001 : MapBase
{
    private void Start()
    {
        OnLoadMap();
    }

    protected override void OnLoadMap()
	{
        Vector3 currentSpawnPoint;
        if (string.IsNullOrEmpty(MapManager.Instance.prevMapID))
        {
            PlayerController.Instance.playerTransform.position = mapData.player_position_left;
        }
        else if (!string.IsNullOrEmpty(MapManager.Instance.prevMapID) && spawnPoint.TryGetValue(MapManager.Instance.prevMapID, out currentSpawnPoint))
        {
            PlayerController.Instance.playerTransform.position = currentSpawnPoint;
            PlayerController.Instance.playerTransform.localScale = MapManager.Instance.lastPlayerScale;
        }
    }

    public override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직
        base.OnReleaseMap();
    }
}
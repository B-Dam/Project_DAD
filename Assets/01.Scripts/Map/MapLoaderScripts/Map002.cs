using UnityEngine;

public class Map002 : MapBase
{
    protected override void Awake()
    {
        base.Awake();
        OnLoadMap();
    }

    protected override void OnLoadMap()
    {
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
        base.OnReleaseMap();
    }
}

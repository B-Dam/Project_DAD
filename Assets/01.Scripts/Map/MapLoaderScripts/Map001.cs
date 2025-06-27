using UnityEngine;

public class Map001 : MapBase
{
    protected override void Awake()
    {
        base.Awake();
        OnLoadMap();
    }

    protected override void OnLoadMap()
	{
		// 맵을 들어갈 때 작동하는 로직
        if (prevMapID == null)
        {
            _playerPosition = mapData.player_position_left;
        }
        else
        {
           _playerPosition = mapData.player_position_right;
        }
    }

    public override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직
        
        prevMapID = MapManager.Instance.currentMapID;
        Debug.Log("Map001맵에서 나갔습니다");
    }
}

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
        if (prevMapID == null)  // 맵이 변경돼서 스폰 포지션이 바뀌었다면 map.csv를 수정하면 됨
        {
            _playerPosition = mapData.player_position_left;
        }
        else
        {
           _playerPosition = mapData.player_position_right;
        }
    }

    protected override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직
        
        prevMapID = MapManager.Instance.currentMapName;
        Debug.Log("Map001맵에서 나갔습니다");
    }
}

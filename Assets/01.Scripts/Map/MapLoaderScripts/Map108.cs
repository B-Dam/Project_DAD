using UnityEngine;

public class Map108 : MapBase
{


    protected override void OnLoadMap()
    {
        // 맵을 들어갈 때 작동하는 로직
        if (prevMapID == "001")
        {
            _playerPosition = mapData.player_position_left;
        }
        else if (prevMapID == "101")
        {
            _playerPosition = mapData.player_position_right;
        }
    }

    public override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직

        prevMapID = MapManager.Instance.currentMapID;
        Debug.Log($"Map002맵에서 나갔습니다 (이전 맵: {prevMapID})");
    }


}

using UnityEngine;

public class Map108 : MapBase
{


    protected override void OnLoadMap()
    {
        // ���� �� �� �۵��ϴ� ����
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
        // ���� ���� �� �۵��ϴ� ����

        prevMapID = MapManager.Instance.currentMapID;
        Debug.Log($"Map002�ʿ��� �������ϴ� (���� ��: {prevMapID})");
    }


}

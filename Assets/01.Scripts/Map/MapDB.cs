using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class MapData
{
    public string map_id;
    public string name;
    public MapType type;
    public string left_map;
    public string right_map;
    public string up_map;
    public string down_map;
    public string bgm;

    public Vector2 player_position_left;
    public Vector2 player_position_right;
    public Vector2 player_position_up;
    public Vector2 player_position_down;
}

public enum MapType
{
    story,
    puzzle,
}

public class MapDB
{
    private const string CSV_FILE_NAME = "map.csv";

    private Dictionary<string, MapData> _mapDataList;

    // 생성자
    public MapDB()
    {
        string[] csvLines = ParseCsv();
        _mapDataList = ParseMapData(csvLines);
    }

    private string[] ParseCsv()  // csv 파일을 전부 읽어오는 함수
    {
        string fullPath = $"{Application.dataPath}/Resources/CSV/map.csv";

        // 경로에 해당하는 파일이 없으면 예외 발생
        if (File.Exists(fullPath) == false)
            throw new($"Can not Read File {fullPath}");

        string[] csvLines = File.ReadAllLines(fullPath);

        return csvLines;
    }

    private Dictionary<string, MapData> ParseMapData(string[] csvLines)  // 읽어온 csv 파일을 줄 별, comma 별로 나누는 함수
    {
        Dictionary<string, MapData> dictionary = new();

        for (var i = 1; i < csvLines.Length; i++)
        {
            string line = csvLines[i];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] fields = line.Split(',');

            MapData map = new MapData();

            // map.map_id = int.Parse(fields[0])

            map.map_id = fields[0];
            map.name = fields[1];
            map.type = (MapType)Enum.Parse(typeof(MapType), fields[2]);
            map.left_map = fields[3];
            map.right_map = fields[4];
            map.up_map = fields[5];
            map.down_map = fields[6];
            map.player_position_left = ParseToVector2(fields[7]);
            map.player_position_right = ParseToVector2(fields[8]);
            map.player_position_up = ParseToVector2(fields[9]);
            map.player_position_down = ParseToVector2(fields[10]);
            map.bgm = fields[11];


            dictionary.Add(map.map_id, map);
        }

        return dictionary;
    }

    private Vector2 ParseToVector2(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return Vector2.zero;

        string[] coords = str.Split('|');
        if (coords.Length == 2)
        {
            float x, y;
            if (float.TryParse(coords[0], out x) && float.TryParse(coords[1], out y))
            {
                return new Vector2(x, y);
            }
            else
            {
                throw new FormatException($"벡터로 초기화 할 수 없음 ({str})");
            }
        }
        else
        {
            Debug.Log($"좌표 값이 x|y 형태가 아닙니다 {str}");
            return Vector2.zero;
        }
    }


    public MapData GetMapData(string mapId)
    {
        MapData data = _mapDataList[mapId];

        return data;
    }
}
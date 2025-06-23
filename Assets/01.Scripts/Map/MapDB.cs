using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Device;

[Serializable]
public class MapData
{
    public string map_id;
    public string name;
    public MapType type;
    public string prev_map_id;
    public string next_map_id;
    public string contains_npcs;
    public string bgm;
    public string cutscene_resource_path;
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

    private string[] ParseCsv()
    {
        string fullPath = $"{Application.dataPath}/CSV/{CSV_FILE_NAME}";

        // 경로에 해당하는 파일이 없으면 예외 발생
        if (File.Exists(fullPath) == false)
            throw new($"Can not Read File {fullPath}");

        string[] csvLines = File.ReadAllLines(fullPath);

        return csvLines;
    }

    private Dictionary<string, MapData> ParseMapData(string[] csvLines)
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
            map.prev_map_id = string.IsNullOrWhiteSpace(fields[3]) ? null : fields[3];
            map.next_map_id = string.IsNullOrWhiteSpace(fields[4]) ? null : fields[4];
            map.contains_npcs = string.IsNullOrWhiteSpace(fields[5]) ? null : fields[5];
            map.bgm = string.IsNullOrWhiteSpace(fields[6]) ? null : fields[6];
            map.cutscene_resource_path = string.IsNullOrWhiteSpace(fields[7]) ? null : fields[7];

            dictionary.Add(map.map_id, map);
        }

        return dictionary;
    }

    public MapData GetMapData(string mapId)
    {
        MapData data = _mapDataList[mapId];

        return data;
    }
}
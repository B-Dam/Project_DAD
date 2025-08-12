using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    private HashSet<string> puzzleMapIDs = new HashSet<string>();

    [Header("맵 CSV 파일 (TextAsset)")]
    public TextAsset mapDatabaseTextAsset;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPuzzleMapIDs();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void LoadPuzzleMapIDs()
    {
        if (mapDatabaseTextAsset == null)
        {
            //Debug.LogWarning("맵 데이터가 연결되지 않았습니다.");
            return;
        }
        puzzleMapIDs.Clear();
        string[] lines =
            mapDatabaseTextAsset.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length <2)
            {
                continue;
            }

            string mapID = cols[0].Trim();
            string mapType = cols[1].Trim();
            if (mapType == "puzzle")
            {
                puzzleMapIDs.Add(mapID);
            }
        }
    }
    public bool IsPuzzleMap(string mapID)
    {
        return puzzleMapIDs.Contains(mapID);
    }

    public IEnumerable<string> GetAllPuzzleMapIDs()
    {
        return puzzleMapIDs;
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase Instance;

    private Dictionary<string, string> dialogueDict = new();

    [Header("🔤 다이얼로그 CSV (Resources 폴더 내)")]
    public string csvFileName = "dialogue_ko"; // 예: dialogue_ko.csv

   private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);  
        LoadCSV(csvFileName);
    }
    else
    {
        Destroy(gameObject);
    }
}

    public void LoadCSV(string fileName)
    {
        dialogueDict.Clear();
        TextAsset csvData = Resources.Load<TextAsset>("dialog/" + fileName);
        if (csvData == null)
        {
            Debug.LogError($"CSV 파일 {fileName}을(를) 찾을 수 없습니다.");
            return;
        }

        using (StringReader reader = new(csvData.text))
        {
            bool isFirstLine = true;
            while (reader.Peek() > -1)
            {
                var line = reader.ReadLine();
                if (isFirstLine) { isFirstLine = false; continue; }

                var values = line.Split(',');
                if (values.Length >= 2)
                {
                    string id = values[0].Trim();
                    string text = values[1].Trim();
                    dialogueDict[id] = text;
                }
            }
        }
    }

    public string GetTextById(string id)
    {
        if (dialogueDict.TryGetValue(id, out var line))
            return line;
        return $"<missing:{id}>";
    }
}

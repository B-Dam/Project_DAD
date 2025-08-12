using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase Instance;

    public class DialogueLine
    {
        public string speaker;
        public string text;
          
        public string spritePath;
        
        public DialogueLine(string speaker, string text, string spritePath = "")
    {
        this.speaker = speaker;
        this.text = text;
        this.spritePath = spritePath;
    }
    }

    private Dictionary<string, DialogueLine> dialogueDict = new();

    [Header("🔤 다이얼로그 CSV (Resources 폴더 내)")]
    public string csvFileName = "dialogueDB"; // 예: dialogue_ko.csv

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
        TextAsset csvData = Resources.Load<TextAsset>("CSV/" + fileName);
        if (csvData == null)
        {
            //Debug.LogError($"❌ CSV 파일 'CSV/{fileName}'을(를) 찾을 수 없습니다.");
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
                if (values.Length >= 3)
{
    string id = values[0].Trim();
    string speaker = values[1].Trim();
    string text = values[2].Trim();
    string spritePath = values.Length >= 4 ? values[3].Trim() : "";
    dialogueDict[id] = new DialogueLine(speaker, text, spritePath);
}

            }
        }
    }

    public DialogueLine GetLineById(string id)
    {
        if (dialogueDict.TryGetValue(id, out var line))
            return line;

        return new DialogueLine("???", $"<missing:{id}>");
    }
}

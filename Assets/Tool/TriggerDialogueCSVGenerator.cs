using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class TriggerDialogueCSVGenerator : EditorWindow
{
    private EventTriggerZone selectedTrigger;
    private string triggerPrefix = "event001";
    private string fileName = "dialogue_ko";

    [MenuItem("Tools/Trigger CSV 자동 생성기")]
    public static void ShowWindow()
    {
        GetWindow<TriggerDialogueCSVGenerator>("Trigger CSV 생성기");
    }

    private void OnGUI()
    {
        GUILayout.Label("🎯 트리거 대사 CSV 자동 생성기", EditorStyles.boldLabel);

        selectedTrigger = (EventTriggerZone)EditorGUILayout.ObjectField("🎬 트리거 선택", selectedTrigger, typeof(EventTriggerZone), true);
        triggerPrefix = EditorGUILayout.TextField("🆔 ID 접두사", triggerPrefix);
        fileName = EditorGUILayout.TextField("📁 CSV 파일명", fileName);

        if (selectedTrigger == null)
        {
            EditorGUILayout.HelpBox("EventTriggerZone을 선택해야 합니다.", MessageType.Warning);
            return;
        }

        if (selectedTrigger.triggerDialogueEntries == null || selectedTrigger.triggerDialogueEntries.Length == 0)
        {
            EditorGUILayout.HelpBox("triggerDialogueEntries가 비어 있습니다.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("📄 CSV에 추가 및 ID 자동 할당"))
        {
            GenerateTriggerCSV();
        }
    }

    private void GenerateTriggerCSV()
    {
        string folderPath = Path.Combine(Application.dataPath, "Resources/dialog");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, fileName + ".csv");

        HashSet<string> existingIDs = new();
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var split = line.Split(',');
                if (split.Length > 0) existingIDs.Add(split[0]);
            }
        }

        StringBuilder sb = new StringBuilder();
        if (!File.Exists(filePath)) sb.AppendLine("id,speaker,text"); // ✅ 헤더 포함

        int lineCount = 1;
        for (int i = 0; i < selectedTrigger.triggerDialogueEntries.Length; i++)
        {
            var entry = selectedTrigger.triggerDialogueEntries[i];

            if (string.IsNullOrEmpty(entry.text)) continue;

            if (string.IsNullOrEmpty(entry.id))
            {
                string newId;
                do
                {
                    newId = $"{triggerPrefix}_{lineCount++}";
                } while (existingIDs.Contains(newId));

                entry.id = newId;
                selectedTrigger.triggerDialogueEntries[i] = entry;
            }

            string safeSpeaker = string.IsNullOrEmpty(entry.speaker) ? "???" : entry.speaker.Replace(",", "，");
            string safeText = entry.text.Replace(",", "，").Replace("\n", "\\n");

            sb.AppendLine($"{entry.id},{safeSpeaker},{safeText}");
        }

        File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(selectedTrigger);
        PrefabUtility.RecordPrefabInstancePropertyModifications(selectedTrigger);

        Debug.Log($" 트리거 대사 {selectedTrigger.triggerDialogueEntries.Length}개가 '{filePath}'에 추가되었습니다.");
    }
}

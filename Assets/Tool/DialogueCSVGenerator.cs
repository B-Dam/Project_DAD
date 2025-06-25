using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class DialogueCSVGenerator : EditorWindow
{
    private NPC selectedNPC;
    private string npcIDPrefix = "npc001";
    private string fileName = "dialogue_ko";

    [MenuItem("Tools/CSV 자동 생성기 📝")]
    public static void ShowWindow()
    {
        GetWindow<DialogueCSVGenerator>("CSV 생성기");
    }

    private void OnGUI()
    {
        GUILayout.Label("🗣️ NPC 대사 CSV에 추가 (덮어쓰기 아님)", EditorStyles.boldLabel);

        selectedNPC = (NPC)EditorGUILayout.ObjectField("NPC 선택", selectedNPC, typeof(NPC), true);
        npcIDPrefix = EditorGUILayout.TextField("ID 접두사", npcIDPrefix);
        fileName = EditorGUILayout.TextField("CSV 파일명", fileName);

       if (selectedNPC == null || selectedNPC.dialogueEntries == null || selectedNPC.dialogueEntries.Length == 0)

        {
            EditorGUILayout.HelpBox("NPC와 dialogueLines가 필요합니다.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("📥 CSV에 대사 추가 및 ID 자동 할당"))
        {
            AppendToCSV(selectedNPC, npcIDPrefix, fileName);
        }
    }

   private void AppendToCSV(NPC npc, string idPrefix, string fileName)
{
    string folderPath = Path.Combine(Application.dataPath, "Resources/dialog");
    Directory.CreateDirectory(folderPath);
    string filePath = Path.Combine(folderPath, fileName + ".csv");

    HashSet<string> existingIDs = new();
    if (File.Exists(filePath))
    {
        var existingLines = File.ReadAllLines(filePath);
        foreach (var line in existingLines)
        {
            var split = line.Split(',');
            if (split.Length > 0)
                existingIDs.Add(split[0]);
        }
    }

    StringBuilder sb = new StringBuilder();
    if (!File.Exists(filePath)) sb.AppendLine("id,speaker,text"); // ✅ 헤더 수정

    int lineCount = 1;
    for (int i = 0; i < npc.dialogueEntries.Length; i++)
    {
        var entry = npc.dialogueEntries[i];

        if (string.IsNullOrEmpty(entry.id))
        {
            string newId;
            do
            {
                newId = $"{idPrefix}_{lineCount++}";
            } while (existingIDs.Contains(newId));

            entry.id = newId; // ID 자동 지정
            npc.dialogueEntries[i] = entry;
        }

        string safeSpeaker = string.IsNullOrEmpty(entry.speaker) ? "???" : entry.speaker.Replace(",", "，");
        string safeText = entry.text.Replace(",", "，").Replace("\n", "\\n");

        sb.AppendLine($"{entry.id},{safeSpeaker},{safeText}");
    }

    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
    AssetDatabase.Refresh();
    EditorUtility.SetDirty(npc);
    PrefabUtility.RecordPrefabInstancePropertyModifications(npc);

    Debug.Log($"✅ {npc.dialogueEntries.Length}개의 대사가 '{filePath}'에 저장되었습니다. (speaker 포함)");
}


}

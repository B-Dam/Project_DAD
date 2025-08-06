using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueSession
{
    private DialogueDatabase.DialogueLine[] lines;
    private string[] ids;
    private HashSet<string> seenIDs = new HashSet<string>();

    public int CurrentIndex { get; private set; } = 0;

    public DialogueSession(DialogueDatabase.DialogueLine[] lines, string[] ids)
    {
        this.lines = lines;
        this.ids = ids;
    }

    public bool IsEmpty => lines == null || lines.Length == 0;

    public bool IsComplete => CurrentIndex >= lines.Length;

    public string CurrentID => ids != null && CurrentIndex < ids.Length ? ids[CurrentIndex] : null;

    // 다음 라인 이동
    public void MoveNext() => CurrentIndex++;

    public DialogueDatabase.DialogueLine GetLine(int index)
    {
        return HasIndex(index) ? lines[index] : null;
    }

    // 안전한 인덱스 접근
    public bool HasIndex(int index)
    {
        return lines != null && index >= 0 && index < lines.Length;
    }

    // 다음 ID 가져오기
    public string GetID(int index)
    {
        return ids != null && index >= 0 && index < ids.Length ? ids[index] : null;
    }

    // 본 대사 기록
    public void MarkSeen()
    {
        if (!string.IsNullOrEmpty(CurrentID))
            seenIDs.Add(CurrentID);
    }

    // 특정 ID가 이미 본 대사인지 확인
    public bool HasSeen(string id)
    {
        return seenIDs.Contains(id);
    }

    // 지금까지 본 모든 ID 목록 조회 + 저장/로드 지원
    public string[] GetSeenIDs()
    {
        return seenIDs.ToArray();
    }
    //저장/로드 지원
    public void LoadSeenIDs(string[] ids)
    {
        seenIDs.Clear();
        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(id))
                seenIDs.Add(id);
        }
    }

}
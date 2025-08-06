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

    // ���� ���� �̵�
    public void MoveNext() => CurrentIndex++;

    public DialogueDatabase.DialogueLine GetLine(int index)
    {
        return HasIndex(index) ? lines[index] : null;
    }

    // ������ �ε��� ����
    public bool HasIndex(int index)
    {
        return lines != null && index >= 0 && index < lines.Length;
    }

    // ���� ID ��������
    public string GetID(int index)
    {
        return ids != null && index >= 0 && index < ids.Length ? ids[index] : null;
    }

    // �� ��� ���
    public void MarkSeen()
    {
        if (!string.IsNullOrEmpty(CurrentID))
            seenIDs.Add(CurrentID);
    }

    // Ư�� ID�� �̹� �� ������� Ȯ��
    public bool HasSeen(string id)
    {
        return seenIDs.Contains(id);
    }

    // ���ݱ��� �� ��� ID ��� ��ȸ + ����/�ε� ����
    public string[] GetSeenIDs()
    {
        return seenIDs.ToArray();
    }
    //����/�ε� ����
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
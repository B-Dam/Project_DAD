using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 저장 포맷
[Serializable]
public struct DialogueSaveData
{
    public bool   isActive;       // 대화창이 켜진 상태였는지
    public int    currentIndex;   // 현재 인덱스
    public string[] ids;          // 진행 중이던 ID 시퀀스 전체
    public string[] seenIds;      // 세션 내 '이미 본' ID들
}

[RequireComponent(typeof(UniqueID))]
public class DialogueSave : MonoBehaviour, ISaveable
{
    private UniqueID idComp;

    public string UniqueID
    {
        get
        {
            if (!idComp)
            {
                idComp = GetComponent<UniqueID>();
                if (!idComp) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    void Awake()
    {
        idComp = GetComponent<UniqueID>();
    }

    // === 저장 ===
    public object CaptureState()
    {
        var dm = DialogueManager.Instance;
        var data = new DialogueSaveData
        {
            isActive     = dm != null && dm.IsDialogueActive,
            currentIndex = 0,
            ids          = Array.Empty<string>(),
            seenIds      = Array.Empty<string>()
        };

        if (dm != null && dm.Session != null)
        {
            // 세션 전체 ID 목록 수집
            var ids = new List<string>();
            int i = 0;
            while (dm.Session.HasIndex(i)) // 세션에 인덱스가 존재하는 동안
            {
                ids.Add(dm.Session.GetID(i));
                i++;
            }

            data.ids          = ids.ToArray();
            data.currentIndex = dm.Session.CurrentIndex;
            data.seenIds      = dm.GetAllSeenIDs(); // 세션 내 '본' 목록
        }
        return data;
    }

    // === 불러오기 ===
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<DialogueSaveData>(json);
        StartCoroutine(ApplyNextFrame(data)); // UI/싱글톤 준비 이후에 적용
    }

    private IEnumerator ApplyNextFrame(DialogueSaveData data)
    {
        // 한 프레임 미루기: DialogueUIDisplayer/DB/컷신 컨트롤러 등 초기화 완료 보장
        yield return null;

        var dm = DialogueManager.Instance;
        var db = DialogueDatabase.Instance;

        if (dm == null || db == null)
        {
            Debug.LogWarning("[DialogueSave] DialogueManager/Database 준비 안 됨. 적용 보류.");
            yield break;
        }

        // 저장 당시 대화가 '꺼짐' 상태였다면, 지금 실행 중이면 꺼주고 끝
        if (!data.isActive || data.ids == null || data.ids.Length == 0)
        {
            if (dm.IsDialogueActive) dm.EndDialogue(true);
            yield break;
        }

        // 1) 저장해둔 ID 시퀀스로 라인 재구성
        var lines = new DialogueDatabase.DialogueLine[data.ids.Length];
        for (int i = 0; i < data.ids.Length; i++)
            lines[i] = db.GetLineById(data.ids[i]);

        // 2) 새로운 세션 생성 후 시작 (index=0에서 표시는 되지만, 곧 점프함)
        var newSession = new DialogueSession(lines, data.ids);
        dm.StartDialogue(newSession);                                                       // 세션 시작/패널 표시/현재 라인 표시 :contentReference[oaicite:0]{index=0}

        // 3) '이미 본' 목록 복원(선택)
        if (data.seenIds != null && data.seenIds.Length > 0)
            dm.LoadSeenIDs(data.seenIds);                                                  // 세션에 seenIDs 주입 :contentReference[oaicite:1]{index=1}

        // 4) 저장해 둔 인덱스로 점프 (이벤트/컷신 발동 없이 세션 인덱스만 이동)
        int cur = dm.Session.CurrentIndex;
        while (cur < data.currentIndex && dm.Session.HasIndex(cur + 1))
        {
            // Display/ShowNextLine을 호출하지 않고, 세션 인덱스만 올린다
            dm.Session.MoveNext();                                                         // 인덱스 증가 :contentReference[oaicite:2]{index=2}
            cur++;
        }

        // 5) 점프한 위치의 라인을 다시 그리기
        dm.DisplayCurrentLine();                                                           // 해당 인덱스 라인 표시 :contentReference[oaicite:3]{index=3}
        dm.UnlockInput();
    }
}

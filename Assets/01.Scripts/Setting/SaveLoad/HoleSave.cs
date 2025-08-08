using System;
using UnityEngine;

[Serializable]
public struct HoleData
{
    public bool hasCover;       // 커버(분다)가 있는지
    public bool blockerActive;  // PlayerBlocker의 활성화 여부
}

public class HoleSave : MonoBehaviour, ISaveable
{
    UniqueID idComp;

    private void Awake()
    {
        idComp = GetComponent<UniqueID>();
    }

    // ISaveable.UniqueID 구현
    public string UniqueID => idComp.ID;

    [Header("프리팹 & 자식 이름 설정")]
    [Tooltip("Instantiate할 HoleCover 프리팹")]
    [SerializeField] private GameObject holeCoverPrefab;
    [Tooltip("Hierarchy 상 복사된 HoleCover 오브젝트 이름")]
    [SerializeField] private string holeCoverName = "HoleCover";
    [Tooltip("PlayerBlocker 오브젝트 (자식)를 드래그해서 할당")]
    [SerializeField] private GameObject playerBlocker;

    // 저장할 데이터를 구조체로 반환
    public object CaptureState()
    {
        bool hasCover = transform.Find(holeCoverName) != null;
        bool blockerActive = playerBlocker != null && playerBlocker.activeSelf;

        return new HoleData
        {
            hasCover      = hasCover,
            blockerActive = blockerActive
        };
    }

    // 로드시 JSON 문자열을 파싱해 상태 복원
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<HoleData>(json);

        // 1) HoleCover 처리
        var existingCover = transform.Find(holeCoverName);
        if (data.hasCover)
        {
            // 커버가 있어야 하는데 없으면 Instantiate
            if (existingCover == null && holeCoverPrefab != null)
            {
                var cover = Instantiate(holeCoverPrefab, transform);
                cover.name = holeCoverName;
            }
        }
        else
        {
            // 커버가 없어야 하는데 있으면 파괴
            if (existingCover != null)
                Destroy(existingCover.gameObject);
        }

        // 2) PlayerBlocker 처리
        if (playerBlocker != null)
            playerBlocker.SetActive(data.blockerActive);
    }
}
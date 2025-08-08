using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HoleTrigger : MonoBehaviour
{
    [Header("플레이어 차단 오브젝트(선택)")]
    [SerializeField] private GameObject playerBlock;

    [Header("트리거 콜라이더 (비워두면 자동탐색)")]
    [SerializeField] private Collider2D holeCollider;

    // 런타임 상태
    private bool isFalled;
    private int? lastConsumedBoxId;

    // 부모 HoleSave (상태 갱신용)
    private HoleSave holeSave;

    private void Awake()
    {
        if (!holeCollider)
            holeCollider = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>(true);

        holeSave = GetComponentInParent<HoleSave>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!holeCollider)
            holeCollider = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>(true);
    }
#endif

    private void OnEnable()  => EnsureTriggerEnabled();
    private void Start()     => EnsureTriggerEnabled();

    private void EnsureTriggerEnabled()
    {
        if (!holeCollider) return;
        holeCollider.isTrigger = true;
        if (!isFalled) holeCollider.enabled = true; // 빈 상태면 켬
    }

    private void OnTriggerEnter2D(Collider2D other) => TryHandle(other);
    private void OnTriggerStay2D(Collider2D other)  => TryHandle(other);

    public void TryHandleExternal(Collider2D other) => TryHandle(other); // 필요 시 자식 리레이용

    private void TryHandle(Collider2D other)
    {
        if (isFalled) return;

        var boxPush = other.GetComponentInParent<BoxPush>();
        if (!boxPush) return;

        var rb = other.attachedRigidbody;
        if (!rb) return;

        // 같은 박스가 여러 콜라이더로 들어와도 1회만 처리
        int boxId = rb.GetInstanceID();
        if (lastConsumedBoxId.HasValue && lastConsumedBoxId.Value == boxId) return;
        lastConsumedBoxId = boxId;

        StartCoroutine(FillHoleDelayed(boxPush));
        AudioManager.Instance?.PlaySFX("Puzzle/BoxDrop");
    }

    /// <summary>세이브 로드 직후, HoleSave에서 호출</summary>
    public void ResetRuntime(bool hasCover)
    {
        isFalled = hasCover;

        if (!holeCollider)
            holeCollider = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>(true);

        if (holeCollider)
        {
            holeCollider.isTrigger = true;
            holeCollider.enabled   = !hasCover;
        }

        if (playerBlock) playerBlock.SetActive(!hasCover);

        lastConsumedBoxId = null;
    }

    private IEnumerator FillHoleDelayed(BoxPush box)
    {
        yield return null;
        FillHole(box);
    }

    private void FillHole(BoxPush box)
    {
        isFalled = true;

        if (holeCollider) holeCollider.enabled = false;
        if (playerBlock)  playerBlock.SetActive(false);

        // 커버 생성/삭제 책임은 HoleSave가 담당
        holeSave?.SetHasCover(true);

        // 박스 비활성
        if (box) box.gameObject.SetActive(false);
    }
}

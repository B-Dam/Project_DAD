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
    private int? lastConsumedBoxId;//동일 박스의 다중 콜라이더 진입 방지
    private int? claimedBoxID; //전역 레지스트리에 내가 점유한 박스ID

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

    private void OnEnable() => EnsureTriggerEnabled();
    private void Start() => EnsureTriggerEnabled();

    private void EnsureTriggerEnabled()
    {
        if (!holeCollider) return;
        holeCollider.isTrigger = true;
        if (!isFalled) holeCollider.enabled = true; // 빈 상태면 켬
    }
    private void OnDisable()

    {
        //내 구멍이 점유 중인 박스가 있으면 안전 해제(씬 전환/비활성 대비)
        if (claimedBoxID.HasValue)
        {
            HoleClaimRegistry.Release(claimedBoxID.Value, this);
            claimedBoxID = null;
        }
    }
    private void OnTriggerEnter2D(Collider2D other) => TryHandle(other);
    private void OnTriggerStay2D(Collider2D other) => TryHandle(other);

   

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

        // 전역 점유: 먼저 잡는 구멍만 유효
        if (!HoleClaimRegistry.TryClaim(boxId, this)) return;

        // 여기까지 왔으면 내가 선점 성공
        lastConsumedBoxId = boxId;
        claimedBoxID = boxId;

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
            holeCollider.enabled = !hasCover;
        }

        if (playerBlock) playerBlock.SetActive(!hasCover);

        lastConsumedBoxId = null;

        // 혹시 남아있을지 모를 점유 해제 (안전)
        if (claimedBoxID.HasValue)
        {
            // 점유 해제
            HoleClaimRegistry.Release(claimedBoxID.Value, this);
            claimedBoxID = null;
        }
    }

    private IEnumerator FillHoleDelayed(BoxPush box)
    {
        // 한 프레임 미뤄 충돌/물리 상태 안정화
        yield return null;
        FillHole(box);
    }

    private void FillHole(BoxPush box)
    {
        isFalled = true;

        if (holeCollider) holeCollider.enabled = false;
        if (playerBlock) playerBlock.SetActive(false);

        // 커버 생성/삭제 책임은 HoleSave가 담당
        holeSave?.SetHasCover(true);

        // 박스 비활성
        if (box) box.gameObject.SetActive(false);

        // 처리 끝났으니 전역 점유 해제
        if (claimedBoxID.HasValue)
        {
            HoleClaimRegistry.Release(claimedBoxID.Value, this);
            claimedBoxID = null;
        }
    }
}

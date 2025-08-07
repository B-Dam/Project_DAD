using UnityEngine;
using System.Linq;

public class PuzzleHintTargetFinder : MonoBehaviour
{
    [Header("기준점")]
    public Transform player;

    [Header("탐색 레이어 설정")]
    public LayerMask targetLayer;  // 예: 힌트 대상 (상자 등)
    public LayerMask answerLayer;  // 예: 정답 대상 (스위치 등)

    [Header("탐색 범위")]
    public float detectionRadius = 200f;

    /// <summary>
    /// 플레이어 기준 가장 가까운 힌트 대상 찾기
    /// </summary>
    public Transform FindClosestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, detectionRadius, targetLayer);

        if (hits.Length == 0) return null;

        Transform closest = hits
            .OrderBy(hit => Vector2.Distance(player.position, hit.transform.position))
            .First()
            .transform;

        return closest;
    }

    /// <summary>
    /// 대상 기준 가장 가까운 정답 오브젝트 찾기
    /// </summary>
    public Transform FindAnswerForTarget(Transform target)
    {
        if (target == null) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(target.position, detectionRadius, answerLayer);

        if (hits.Length == 0) return null;

        Transform closest = hits
            .Where(hit => hit.transform != target) // 자기 자신 제외
            .OrderBy(hit => Vector2.Distance(target.position, hit.transform.position))
            .First()
            .transform;

        return closest;
    }
}

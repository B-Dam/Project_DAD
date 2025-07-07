using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSortingOrder : MonoBehaviour
{
    [Header("y축 기준 정렬 범위 설정")]
    public float minY = -10f; // 위
    public float maxY = 10f;  // 아래
    [Header("오더인레이어 정렬 범위 설정")]
    public int minOrder = 0;
    public int maxOrder = 20;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float y = transform.localPosition.y;
        float t = Mathf.InverseLerp(minY, maxY, y); // 0~1 사이로 정규화
        int order = Mathf.RoundToInt(Mathf.Lerp(minOrder, maxOrder, 1 - t)); // 아래쪽일수록 높은 값

        sr.sortingOrder = order;
    }
}

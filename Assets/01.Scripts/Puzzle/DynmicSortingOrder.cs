using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSortingOrder : MonoBehaviour
{
    public float minY = -10f; // 위
    public float maxY = 10f;  // 아래
    public int minOrder = 0;
    public int maxOrder = 20;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float x = transform.position.y;
        float t = Mathf.InverseLerp(minY, maxY, x); // 0~1 사이로 정규화
        int order = Mathf.RoundToInt(Mathf.Lerp(minOrder, maxOrder, 1 - t)); // 아래쪽일수록 높은 값

        sr.sortingOrder = order;
    }
}

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSortingOrder : MonoBehaviour
{
    [Header("y축 기준 정렬 범위 설정")]
    public float minY = -100f; // 작을수록 오더 크게
    public float maxY = 100f;  // 클 수록 오더 작게
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
        float y = transform.position.y;

        //minY에 가까우면 t = 0
        //maxY에 가까우면 t = 1
        //정규화 0~1로 변환 예) Mathf.InverseLerp(0, 10, 3); // 결과는 0.3f
        float t = Mathf.InverseLerp(minY, maxY, y); 

        //t가 0이면 1 - t = 1 → maxOrder
        //t가 1이면 1 - t = 0 → minOrder
        //선형보간 예)Lerp(0, 10, 0.3f); // 결과는 3.0
        int order = Mathf.RoundToInt(Mathf.Lerp(minOrder, maxOrder, 1 - t)); // 아래쪽일수록 높은 값

        sr.sortingOrder = order;
    }
}

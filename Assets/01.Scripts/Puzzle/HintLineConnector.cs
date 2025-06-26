using UnityEngine;

public class HintLineConnector : MonoBehaviour
{
    public Transform targetA; // 예: 스위치
    public Transform targetB; // 예: 상자
    private LineRenderer lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false; // 처음엔 꺼둔다
    }

    void Update()
    {
        if (targetA != null && targetB != null)
        {
            lineRenderer.SetPosition(0, targetA.position);
            lineRenderer.SetPosition(1, targetB.position);
        }
    }
    public void SetActive(bool active)
    {
        lineRenderer.enabled = active;
        Debug.Log($"[Line] SetActive({active}) 호출됨! 결과: {lineRenderer.enabled}");
    }
   
}

using UnityEngine;

public class HintLineConnector : MonoBehaviour
{
    [Header("예: 상자")]
    public Transform targetA; 
    [Header("예: 스위치")]
    public Transform targetB; 
    private LineRenderer lineRenderer;

    [Header("힌트 파티클 이펙트")]
    public ParticleSystem lineParticlePrefab; // 프리팹 연결

    private ParticleSystem lineParticlesInstance;

    void Start()

    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false; // 처음엔 꺼둔다

        if (lineParticlePrefab != null)
        {
            lineParticlesInstance = Instantiate(lineParticlePrefab, transform);
            lineParticlesInstance.Stop();
            lineParticlesInstance.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (targetA != null && targetB != null)
        {
            // 선 위치 갱신
            lineRenderer.SetPosition(0, targetA.position);
            lineRenderer.SetPosition(1, targetB.position);

            if (lineParticlesInstance != null && lineParticlesInstance.gameObject.activeSelf)
            {
                Vector3 dir = targetB.position - targetA.position;
                float dist = dir.magnitude;
                Vector3 mid = (targetA.position + targetB.position) * 0.5f;

                lineParticlesInstance.transform.position = mid;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                lineParticlesInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

                var shape = lineParticlesInstance.shape;
                shape.scale = new Vector3(dist, shape.scale.y, shape.scale.z);
            }
            UpdateParticleAlphaByDistance(); 
        }
    }
    public void SetActive(bool active)
    {
        lineRenderer.enabled = active;

        if (lineParticlesInstance != null)
        {
            lineParticlesInstance.gameObject.SetActive(active);

            if (active) lineParticlesInstance.Play();
            else lineParticlesInstance.Stop();
        }
        Debug.Log($"[Line] SetActive({active}) 호출됨! 결과: {lineRenderer.enabled}");
    }
    void UpdateParticleAlphaByDistance()
    {
        if (targetA == null || targetB == null || lineParticlesInstance == null) return;

        float distance = Vector2.Distance(targetA.position, targetB.position);

        // 거리 기반 투명도 계산 (예: 가까우면 1, 멀면 0)
        float minDistance = 1f;
        float maxDistance = 10f;
        float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);

        // 파티클의 startColor 알파 조절
        var main = lineParticlesInstance.main;
        Color color = main.startColor.color;
        color.a = alpha;
        main.startColor = new ParticleSystem.MinMaxGradient(color);
    }
}

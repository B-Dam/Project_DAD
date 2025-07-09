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
        lineRenderer.enabled = false;

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
                Vector3 direction = dir.normalized;

                // B에서 생성
                lineParticlesInstance.transform.position = targetB.position;

                // A를 향하도록 회전
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                lineParticlesInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

                // 진행 방향을 Z+ → X+ (Y축 기준 -90도) 회전
                var shape = lineParticlesInstance.shape;
                shape.rotation = new Vector3(0f, -90f, 0f); // 입자 진행 방향 회전
                shape.scale = new Vector3(dist, shape.scale.y, shape.scale.z);

                // 입자 자체의 회전도 적용 (입자 이미지 방향 회전)
                var main = lineParticlesInstance.main;
                main.startRotation3D = true;
                main.startRotationX = 0f;
                main.startRotationY = Mathf.Deg2Rad * -90f; // Y축 기준 회전
                main.startRotationZ = 0f;
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

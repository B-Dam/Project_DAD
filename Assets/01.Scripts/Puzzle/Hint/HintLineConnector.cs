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

    [SerializeField] private float hintActivateDistance = 1f; // A와 B 사이의 거리가 이 값 이상일 때 파티클 활성화
    private bool isHintActive = false; // 현재 상태 저장

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
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogError("[HintLineConnector] LineRenderer가 없습니다! 오브젝트 이름: " + gameObject.name);
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

                // 파티클 길이 조정
                var shape = lineParticlesInstance.shape;
                shape.scale = new Vector3(shape.scale.x, shape.scale.y, dist);

                // A를 향하도록 회전
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                lineParticlesInstance.transform.rotation = Quaternion.Euler(angle, -90f, 0);

            }

            UpdateParticleAlphaByDistance();
            UpdateHintByDistance(); // 거리 기반 힌트 처리
        }
    }

    public void SetActiveParticle(bool active)
    {
        if (lineParticlesInstance != null)
        {
            if (active)
            {
                // 👉 먼저 거리 계산 및 라이프타임 조정
                UpdateHintByDistance();

                // 👉 그 다음 활성화
                lineParticlesInstance.gameObject.SetActive(true);
                lineParticlesInstance.Play();
            }
            else
            {
                lineParticlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                lineParticlesInstance.gameObject.SetActive(false);
            }
        }

        lineRenderer.enabled = active;

        Debug.Log($"[Line] SetActive({active}) 호출됨! 결과: {lineRenderer.enabled}");
    }
    void UpdateParticleAlphaByDistance()
    {
        if (targetA == null || targetB == null || lineParticlesInstance == null) return;

        float distance = Vector2.Distance(targetA.position, targetB.position);
        float minDistance = 1f;
        float maxDistance = 10f;
        float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);

        var main = lineParticlesInstance.main;
        ParticleSystem.MinMaxGradient originalGradient = main.startColor;

        if (originalGradient.mode == ParticleSystemGradientMode.Gradient)
        {
            Gradient oldGradient = originalGradient.gradient;
            Gradient newGradient = new Gradient();

            // 기존 색상 키는 그대로
            newGradient.SetKeys(
                oldGradient.colorKeys,
                new GradientAlphaKey[] {
                new GradientAlphaKey(alpha, 0f),
                new GradientAlphaKey(alpha, 1f)
                }
            );

            main.startColor = new ParticleSystem.MinMaxGradient(newGradient);
        }
        else
        {
            // 만약 단일 색이면 그냥 알파만 곱해서 설정
            Color c = originalGradient.color;
            c.a = alpha;
            main.startColor = new ParticleSystem.MinMaxGradient(c);
        }
        //if (targetA == null || targetB == null || lineParticlesInstance == null) return;

        //float distance = Vector2.Distance(targetA.position, targetB.position);
        //// 거리 기반 투명도 계산 (예: 가까우면 1, 멀면 0)
        //float minDistance = 1f;
        //float maxDistance = 10f;
        //float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);



        //// 파티클의 startColor 알파 조절
        //var main = lineParticlesInstance.main;
        //Color color = main.startColor.color;
        //color.a = alpha;
        //main.startColor = new ParticleSystem.MinMaxGradient(color);
    }

    void UpdateHintByDistance()
    {
        if (targetA == null || targetB == null || lineParticlesInstance == null) return;

        float distance = Vector3.Distance(targetA.position, targetB.position);
        bool shouldActivate = distance >= hintActivateDistance; // 🔧 반대로: "거리가 더 멀면" 힌트 ON

        if (shouldActivate != isHintActive)
        {
            isHintActive = shouldActivate;

            lineRenderer.enabled = isHintActive;
            lineParticlesInstance.gameObject.SetActive(isHintActive);

            if (isHintActive)
            {
                lineParticlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                lineParticlesInstance.Play();
            }
            else
            {
                lineParticlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            Debug.Log($"[Hint] 거리 {distance:F2} → 힌트 {(isHintActive ? "활성화" : "비활성화")}");
        }
        // 🔧 힌트가 켜진 상태일 땐 매 프레임 라이프타임 갱신
        if (isHintActive)
        {
            var main = lineParticlesInstance.main;
            main.startLifetime = distance;;
        }
    }
}

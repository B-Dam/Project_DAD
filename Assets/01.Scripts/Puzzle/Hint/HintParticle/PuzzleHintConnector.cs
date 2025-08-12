using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PuzzleHintConnector : MonoBehaviour
{
    [Header("연결 대상")]
    public Transform targetA; // 시작 지점
    public Transform targetB; // 도착 지점

    [Header("파티클 프리팹")]
    public ParticleSystem lineParticlePrefab;// 연결선에 사용할 파티클 프리팹

    private LineRenderer lineRenderer;// 선을 그리기 위한 컴포넌트
    private ParticleSystem particleInstance;// 인스턴스화된 파티클

    private float lastDistance = -1f; // 최초엔 -1로 초기화
    void Awake()
    {
        // LineRenderer 초기화
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;// 점 두 개로 선 구성
            lineRenderer.enabled = false;// 기본 비활성화
        }
        // 파티클 프리팹이 있다면 인스턴스 생성 후 비활성화
        if (lineParticlePrefab != null)
        {
            particleInstance = Instantiate(lineParticlePrefab, transform);
            particleInstance.Stop();
            particleInstance.gameObject.SetActive(false);
        }

    }

    void Update()
    {
        if (targetA == null || targetB == null) return;

        // LineRenderer의 위치를 대상 A와 B로 설정
        lineRenderer.SetPosition(0, targetA.position);
        lineRenderer.SetPosition(1, targetB.position);

        // 파티클이 재생 중이라면 위치와 회전 갱신
        if (particleInstance != null && particleInstance.isPlaying)
        {
            UpdateParticlePositionAndRotation();

        }
    }

    // 외부에서 대상 A와 B를 설정할 수 있는 함수
    public void SetTargets(Transform a, Transform b, float distance)
    {
        targetA = a;
        targetB = b;

        UpdateLine();// 즉시 선 갱신
        UpdateParticlePositionAndRotation();

        // 거리 변화 감지 (0.1 이상 차이날 때만 재시뮬레이션)
        //if (Mathf.Abs(distance - lastDistance) > 0.1f)
        //{
        //    lastDistance = distance;

        //    float normalized = Mathf.Clamp01(distance / 1f);
        //    float lifetime = Mathf.Max(1f, particleInstance.main.startLifetime.constant);

        //    particleInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        //    particleInstance.Simulate(normalized * lifetime, true, true);
        //    particleInstance.Play();

        //    Debug.Log($"[HintConnector] 파티클 갱신됨 - 거리: {distance:F2}");
        //}
    }

    // 힌트를 켜고 끄는 함수 (선 + 파티클)
    public void SetHintActive(bool active)
    {
        if (lineRenderer != null)
            lineRenderer.enabled = active;// 선 활성화 여부 설정

        if (particleInstance != null)
        {
            if (active)
            {
               
                particleInstance.gameObject.SetActive(true);// 파티클 오브젝트 활성화
                particleInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);// 이전 파티클 정리
                // 코루틴으로 파티클 사전 시뮬레이션 후 재생
                StartCoroutine(PrewarmAndPlay(targetA.position, targetB.position, 1f));
            }
            else
            {
                particleInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);// 파티클 정지
                particleInstance.gameObject.SetActive(false);// 비활성화
            }
        }
    }
    // 파티클의 수명을 거리 기반으로 사전 시뮬레이션한 뒤 재생
    private IEnumerator PrewarmAndPlay(Vector3 startPos, Vector3 endPos, float maxDistance)
    {
        float distance = Vector3.Distance(startPos, endPos);// 두 지점 거리
        float normalized = 1f;// Mathf.Clamp01(distance / maxDistance);// 최대 거리 기준 정규화

        // 파티클 기본 수명 값 가져오기
        float lifetime = particleInstance.main.startLifetime.constant;
        float simulateTime = lifetime * normalized;// 거리 비례 시뮬레이션 시간 계산

        //Debug.Log("simulateTime" + simulateTime);
        // 위치, 회전, 크기 설정
        UpdateParticlePositionAndRotation();

        // 시뮬레이션 후 정지 상태로 대기
        particleInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleInstance.Simulate(simulateTime, true, true);
        particleInstance.Pause();

        yield return null; // 렌더링 한 프레임 기다리기
        //Debug.Log("[Connector] 파티클 재생 시작 (Play 호출)");
        particleInstance.Play();
    }

    // 선(LineRenderer)을 대상 좌표에 맞게 갱신
    private void UpdateLine()
    {
        if (lineRenderer == null || targetA == null || targetB == null) return;

        lineRenderer.SetPosition(0, targetA.position);
        lineRenderer.SetPosition(1, targetB.position);
    }

    // 파티클 위치, 방향, 길이, 수명을 동적으로 갱신
    private void UpdateParticlePositionAndRotation()
    {
        if (targetA == null || targetB == null || particleInstance == null) return;

        Vector3 dir = targetB.position - targetA.position;
        float dist = dir.magnitude;
        Vector3 direction = dir.normalized;

        // 파티클 위치: 도착 지점 (B)
        particleInstance.transform.position = targetB.position;

        // 파티클 shape(모양) 길이 설정 (Z축 기준)
        var shape = particleInstance.shape;
        shape.scale = new Vector3(shape.scale.x, shape.scale.y, dist);

        // 방향을 기준으로 회전 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        particleInstance.transform.rotation = Quaternion.Euler(angle, -90f, 0);

        // 파티클 수명도 거리 기반으로 설정
        var main = particleInstance.main;
        main.startLifetime = dist;
    }
}

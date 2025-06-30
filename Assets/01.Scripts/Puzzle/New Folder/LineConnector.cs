//using UnityEngine;

//public class LineConnector : MonoBehaviour
//{
//    public Transform targetA;
//    public Transform targetB;
//    private LineRenderer lr;

//    void Start()
//    {
//        lr = GetComponent<LineRenderer>();
//        lr.positionCount = 2;
//    }

//    void Update()
//    {
//        if (targetA && targetB)
//        {
//            lr.SetPosition(0, targetA.position);
//            lr.SetPosition(1, targetB.position);
//        }
//    }
//}

using UnityEngine;

public class LineConnector : MonoBehaviour
{
    public Transform targetA;
    public Transform targetB;
    private LineRenderer lr;

    [Header("✨ 연결 이펙트")]
    public ParticleSystem lineParticlePrefab;
    private ParticleSystem lineParticlesInstance;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;

        // 프리팹을 인스턴스화
        if (lineParticlePrefab != null)
        {
            lineParticlesInstance = Instantiate(lineParticlePrefab, transform);
            lineParticlesInstance.Stop();
            lineParticlesInstance.gameObject.SetActive(false);
        }
    }

    void Update()
    {
       
        if (targetA && targetB)
        {


            // 1. 선 위치 업데이트
            lr.SetPosition(0, targetA.position);
            lr.SetPosition(1, targetB.position);

            // 2. 파티클 시스템 위치와 크기 조절
            if (lineParticlesInstance != null)
            {
                Vector3 dir = (targetB.position - targetA.position);
                float dist = dir.magnitude;
                Vector3 mid = (targetA.position + targetB.position) * 0.5f;

                // 위치: 중간 지점
                lineParticlesInstance.transform.position = mid;

                // 회전: 선 방향으로 정렬 (2D 기준 Z축 회전만 사용)
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                lineParticlesInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

                // Shape 크기 조절 (길이만)
                var shape = lineParticlesInstance.shape;
                shape.scale = new Vector3(dist, shape.scale.y, shape.scale.z); // X축 방향으로 늘림

                if (!lineParticlesInstance.gameObject.activeSelf)//파티클 오브젝트가 비활성화 상태인지 확인
                {
                    lineParticlesInstance.gameObject.SetActive(true);//파티클 오브젝트를 활성화시킴
                    lineParticlesInstance.Play();//파티클 시스템을 재생
                }
                ////  입자 수 거리 기반으로 조절!
                //var emission = lineParticles.emission;
                //emission.rateOverTime = dist * 50f; // 조절 가능
            }
        }
    }
}

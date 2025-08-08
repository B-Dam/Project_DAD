using System;
using UnityEngine;

[Serializable]
public struct BoxData
{
    public bool isActive;
    public Vector3 position;
    public Quaternion rotation;
}

[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(Rigidbody2D))]
public class BoxSave : MonoBehaviour, ISaveable
{
    UniqueID idComp;
    private Rigidbody2D rb;

    private void Awake()
    {
        idComp = GetComponent<UniqueID>();
        rb = GetComponent<Rigidbody2D>();
    }

    // ISaveable.UniqueID 구현
    public string UniqueID => idComp.ID;


    // 저장 시: 활성화 여부와 위치값을 BoxData로 반환
    public object CaptureState()
    {
        return new BoxData
        {
            isActive = gameObject.activeSelf,
            position = transform.position,
            rotation = transform.rotation
        };
    }

    // 로드 시: JSON 문자열을 BoxData로 파싱한 뒤 복원
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<BoxData>(json);

        // 저장 당시 비활성이었다면 그대로 끄고 끝
        if (!data.isActive)
        {
            if (rb)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.Sleep();
            }

            gameObject.SetActive(false);
            return;
        }

        if (rb)
        {
            // 1프레임 동안 보간으로 끌려가는 걸 차단
            var prevInterp = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation2D.None;

            // 물리 상태 초기화
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Rigidbody2D 좌표/각도로 “진짜 순간이동”
            rb.position = (Vector2)data.position;
            rb.rotation = data.rotation.eulerAngles.z;

            Physics2D.SyncTransforms(); // 트랜스폼 동기화

            // 원래 보간 복구
            rb.interpolation = prevInterp;
            rb.WakeUp();
        }
        else
        {
            // 혹시 Rigidbody2D가 없다면 트랜스폼으로만
            transform.SetPositionAndRotation(data.position, data.rotation);
        }
    }
}
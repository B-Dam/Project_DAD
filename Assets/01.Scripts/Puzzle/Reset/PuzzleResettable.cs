using UnityEngine;

public class PuzzleResettable : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        // 처음 상태 저장
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetState()
    {
        // 상태 복원
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}

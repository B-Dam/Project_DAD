using UnityEngine;

public class PuzzlePlayerResettable : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool hasSaved = false;

    public void SaveResetPoint()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        hasSaved = true;
    }

    public void ResetState()
    {
        if (!hasSaved) return;

        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}

using UnityEngine;

public class TriggerTutorial : MonoBehaviour
{
    public MovementKeyTutorial tutorialManager;
    public bool isOnTrigger; // true면 켜기, false면 끄기

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($" Trigger 작동됨! ({gameObject.name})");

        if (tutorialManager == null)
        {
            Debug.LogError(" tutorialManager가 비어 있음!");
            return;
        }

        if (isOnTrigger)
            tutorialManager.ShowTutorial();
        else
            tutorialManager.HideTutorial();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MovementKeyTutorial : MonoBehaviour
{
    public GameObject tutorialPanel; // 전체 키 튜토리얼 UI
    public Image wKey, aKey, sKey, dKey, spaceKey; // 각 키 이미지
    public Color defaultColor = Color.white;
    public Color pressedColor = Color.red;

    public string targetMapID = "001"; // 특정 맵 ID
    private bool isTutorialActive = false;

    void Start()
    {
        tutorialPanel.SetActive(false); // 기본은 비활성화
        CheckTutorialCondition();
    }

    void Update()
    {
        CheckTutorialCondition();

        if (!isTutorialActive) return;

        UpdateKeyVisual(wKey, KeyCode.W, KeyCode.UpArrow);
        UpdateKeyVisual(aKey, KeyCode.A, KeyCode.LeftArrow);
        UpdateKeyVisual(sKey, KeyCode.S, KeyCode.DownArrow);
        UpdateKeyVisual(dKey, KeyCode.D, KeyCode.RightArrow);
        UpdateKeySpaceVisual(spaceKey, KeyCode.Space);
    }

    void UpdateKeyVisual(Image img, KeyCode key1, KeyCode key2)
    {
        if (Input.GetKey(key1) || Input.GetKey(key2))
            img.color = pressedColor;
        else
            img.color = defaultColor;
    }
    void UpdateKeySpaceVisual(Image img, KeyCode key1)
    {
        if (Input.GetKey(key1))
            img.color = pressedColor;
        else
            img.color = defaultColor;
    }

    void CheckTutorialCondition()
    {
        if (MapManager.Instance == null) return;

        string currentMap = MapManager.Instance.currentMapID;

        // 조건 만족 → 켜기
        if (currentMap == targetMapID && !isTutorialActive)
        {
            tutorialPanel.SetActive(true);
            isTutorialActive = true;
        }
        // 조건 불만족 → 끄기
        else if (currentMap != targetMapID && isTutorialActive)
        {
            tutorialPanel.SetActive(false);
            isTutorialActive = false;
        }
    }
}

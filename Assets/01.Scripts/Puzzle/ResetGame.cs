using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetGame : MonoBehaviour
{
    public Button resetButton; // Inspector에 할당할 버튼

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(RestartScene);
        }
        else
        {
            Debug.LogWarning("Reset 버튼이 연결되지 않았습니다.");
        }
    }
    void Update()
    {
        // 키보드 R 키로도 리셋
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }
    public void RestartScene()
    {
        Time.timeScale = 1f; // 혹시 멈춘 상태였으면 다시 정상 속도로
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
   

}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;
    //public GameObject eventSystem;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    public void GameStart()
    {
        SceneManager.LoadScene(1);
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            GameManager.Instance.GameStart();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

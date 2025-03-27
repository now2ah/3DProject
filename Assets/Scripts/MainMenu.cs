using UnityEngine;
using UnityEngine.Rendering.Universal;
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
        GameManager.Instance.OnManagersLoaded += _OnManagersLoaded;
    }

    private void _OnManagersLoaded(object sender, System.EventArgs e)
    {
        AudioManager.Instance.PlayBgm(AudioManager.eBgm.BGM_MAIN);
    }

    public void GameStart()
    {
        AudioManager.Instance.PlaySfx(AudioManager.ESfx.UI_CLICK);
        SceneManager.LoadScene(1);
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            GameManager.Instance.GameStart();
        }
        else if (scene.name == "MainMenuScene")
        {
            AudioManager.Instance.PlayBgm(AudioManager.eBgm.BGM_MAIN);
        }
    }

    //화이팅^^ 하하~
    public void ExitGame()
    {
        AudioManager.Instance.PlaySfx(AudioManager.ESfx.UI_CLICK);
        Application.Quit();
    }
}

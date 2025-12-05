using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string GameSceneName;

    public void Play()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }

}

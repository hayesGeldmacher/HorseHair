using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenControls : MonoBehaviour
{
    [SerializeField] private string StartingLevel;
    public void StartGame()
    {
        SceneManager.LoadScene(StartingLevel);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenControls : MonoBehaviour
{
    [SerializeField] private string StartingLevel;
    public void StartGame()
    {
        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("Goal");
        PlayerPrefs.DeleteKey("Thoughts");
        PlayerPrefs.SetInt("TaskNum", 0);
        PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Morning);
        PlayerPrefs.Save();
        SceneManager.LoadScene(StartingLevel);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("Scene"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

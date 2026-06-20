using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] private string StartScreen;
    public void ReturnToStart()
    {
        SceneManager.LoadScene(StartScreen);
    }
}

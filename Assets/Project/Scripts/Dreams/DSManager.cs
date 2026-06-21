using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DSManager : MonoBehaviour
{

    [Header("Scene Transitions")]
    [SerializeField] private float sceneTransitionTime = 3.0f;
    [SerializeField] private string nextSceneName;
    private bool triggeredSceneEnd = false;

    [Header("Audio")]
    [SerializeField] private bool fadeBackgroundAudio = false;
    [SerializeField] private AudioGroupFade audioFade;

    [Header("Animation")]
    [SerializeField] private EyelidsFG eyelids;

    #region Singleton

    public static DSManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of playercontroller present in scene");
            return;
        }

        instance = this;
    }

    #endregion

    public void CallEndScene()
    {
        if (!triggeredSceneEnd)
        {
            triggeredSceneEnd = true;
            StartCoroutine(EndScene());
        }
    }

    private IEnumerator EndScene()
    {
        audioFade.SetBackgroundFadeOut();
        yield return new WaitForSeconds(sceneTransitionTime);
        eyelids.TriggerEyesDownAnimation();
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(nextSceneName);
    }
}

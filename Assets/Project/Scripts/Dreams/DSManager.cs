using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DSManager : MonoBehaviour
{

    [Header("Scene Transitions")]
    [SerializeField] private float sceneTransitionTime = 3.0f;
    [SerializeField] private string nextSceneName;
    private bool triggeredSceneEnd = false;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

        yield return new WaitForSeconds(sceneTransitionTime);
        SceneManager.LoadScene(nextSceneName);
    }
}

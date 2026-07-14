using UnityEngine;

public class ScareManager : MonoBehaviour
{
    #region Singleton

    public static ScareManager instance;

    void Awake()
    {

        onScaresEnabled += ScaresEnabled;

        if (instance != null)
        {
            Debug.LogWarning("More than one instance of scare manager present in scene");
            return;
        }

        instance = this;
    }

    #endregion


    /// <summary>
    /// This script manages the scares, background noises, and animations that occur 
    /// behind the player's back during the fighting game segments
    /// </summary>

    public delegate void OnScaresEnabled();
    public OnScaresEnabled onScaresEnabled;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ScaresEnabled()
    {
        Debug.Log("Scares are enabled!");
    }


    public void CallScares()
    {
        onScaresEnabled?.Invoke();
    }
}

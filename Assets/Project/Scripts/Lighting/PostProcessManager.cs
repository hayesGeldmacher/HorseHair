using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : MonoBehaviour
{

    #region Singleton

    public static PostProcessManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of playercontroller present!! NOT GOOD!");
            return;
        }

        instance = this;
    }

    #endregion

    [Header("References")]
    [SerializeField] private Volume volume;

    [Header("Vignette")]
    [SerializeField] private float vignetteChangeSpeed = 1.0f;
    private Vignette vignette = null;
    private float vignetteStartingIntensity = 0.0f;
    private float currentVignetteIntensity;
    private float targetVignetteIntensity;
    private float previousVignetteIntensity;
    private bool hasVignetteTarget = false;
    private float vignetteLerpTime = 0.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volume.profile.TryGet(out vignette);
        vignetteStartingIntensity = vignette.intensity.value;
        currentVignetteIntensity = vignetteStartingIntensity;
        previousVignetteIntensity = vignetteStartingIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasVignetteTarget) { VignetteUpdate(); }
    }
    
    private void VignetteUpdate()
    {
        currentVignetteIntensity = Mathf.Lerp(previousVignetteIntensity, targetVignetteIntensity, vignetteLerpTime);
        vignetteLerpTime += vignetteChangeSpeed * Time.deltaTime * 0.1f;
        if(vignetteLerpTime > 1.0f)
        {
            hasVignetteTarget = false;
            vignetteLerpTime = 1.0f;
        }

        vignette.intensity.value = currentVignetteIntensity;
    }

    public void SetVignetteIntensity(float intensity)
    {
        hasVignetteTarget = true;
        previousVignetteIntensity = currentVignetteIntensity;
        targetVignetteIntensity = intensity;
        vignetteLerpTime = 0.0f;
    }

    private void OnApplicationQuit()
    {
        vignette.intensity.value = vignetteStartingIntensity;
    }
}

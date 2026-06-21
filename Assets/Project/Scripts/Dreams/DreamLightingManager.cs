using UnityEngine;

public class DreamLightingManager : MonoBehaviour
{

    /// <summary>
    /// Controls the directional and ambient lighting intensity, inteded for use during the dream sequences
    /// Smoothly lerps current lighting settings to target lighting settings based on speed variables in inspector
    /// Triggered by a script called DreamLightingTrigger
    /// Apply DreamLightingTrigger to object with collision trigger to change lighting in scene - HG
    /// </summary>

    #region Singleton

    public static DreamLightingManager instance;

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

    [Header("Directional Light")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private float directionalChangeSpeed = 1.0f;

    private float currentDirectionalIntensity;
    private float targetDirectionalIntensity;
    private bool hasDirectionalTarget = false;
    private float previousDirectionalIntensity;
    private float directionalLerpTime;

    [Header("Ambient Light")]
    [SerializeField] private float ambientChangeSpeed = 1.0f;

    private float currentAmbientIntensity;
    private float targetAmbientIntensity;
    private bool hasAmbientTarget = false;
    private float previousAmbientIntensity;
    private float ambientLerpTime;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmbientIntensity = RenderSettings.ambientIntensity;
        previousAmbientIntensity = currentAmbientIntensity;

        currentDirectionalIntensity = directionalLight.intensity;
        previousDirectionalIntensity = currentDirectionalIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAmbientTarget) { AmbientUpdate(); }
        if (hasDirectionalTarget) { DirectionalUpdate(); }
    }

    private void AmbientUpdate()
    {
       
        currentAmbientIntensity = Mathf.Lerp(previousAmbientIntensity, targetAmbientIntensity, ambientLerpTime);
        ambientLerpTime += ambientChangeSpeed * Time.deltaTime * 0.1f;
        if(ambientLerpTime >= 1.0f)
        {
            hasAmbientTarget = false;
            ambientLerpTime = 1.0f;
        }

        RenderSettings.ambientIntensity = currentAmbientIntensity;
        RenderSettings.reflectionIntensity = currentAmbientIntensity;
    }

    private void DirectionalUpdate()
    {
        currentDirectionalIntensity = Mathf.Lerp(previousDirectionalIntensity, targetDirectionalIntensity, directionalLerpTime);
        directionalLerpTime += directionalChangeSpeed * Time.deltaTime * 0.1f;
        if(directionalLerpTime >= 1.0f)
        {
            hasDirectionalTarget = false;
            directionalLerpTime = 1.0f;
        }

        directionalLight.intensity = currentDirectionalIntensity;
    }

    public void SetAmbientTarget(float intensity)
    {
        targetAmbientIntensity = intensity;
        hasAmbientTarget = true;
        previousAmbientIntensity = currentAmbientIntensity;
        ambientLerpTime = 0;
    }

    public void SetDirectionalTarget(float intensity) 
    { 
        targetDirectionalIntensity = intensity;
        hasDirectionalTarget = true;
        previousDirectionalIntensity = currentDirectionalIntensity;
        directionalLerpTime = 0;

    }
    
}

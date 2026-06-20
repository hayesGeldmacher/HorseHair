using UnityEngine;

public class DreamLightingManager : MonoBehaviour
{

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
    [SerializeField] private float currentDirectionalIntensity;
    [SerializeField] private float targetDirectionalIntensity;
    [SerializeField] private bool hasDirectionalTarget = false;

    [Header("Ambient Light")]
    private float startAmbientIntensity;
    [SerializeField] private float ambientChangeSpeed = 1.0f;
    [SerializeField] private float currentAmbientIntensity;
    [SerializeField] private float targetAmbientIntensity;
    [SerializeField] private bool hasAmbientTarget = false;
    [SerializeField] private float previousAmbientIntensity;
    private float ambientLerpTime;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startAmbientIntensity = RenderSettings.ambientIntensity;
        currentAmbientIntensity = startAmbientIntensity;
        previousAmbientIntensity = startAmbientIntensity;
       // RenderSettings.ambientIntensity = 1.0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (hasAmbientTarget) { AmbientUpdate(); }
        if (hasDirectionalTarget) { DirectionalUpdate(); }
    }

    private void AmbientUpdate()
    {
       
        if(ambientLerpTime >= 1.0f)
        {

        }
        float newAmbience = Mathf.Lerp(previousAmbientIntensity, targetAmbientIntensity, ambientLerpTime);

        ambientLerpTime += ambientChangeSpeed * Time.deltaTime * 0.1f;

    }

    private void DirectionalUpdate()
    {

    }

    public void SetAmbientTarget(float intensity)
    {
        targetAmbientIntensity = intensity;
        hasAmbientTarget = true;
        previousAmbientIntensity = currentAmbientIntensity;
    }

    public void SetDirectionalTarget(float intensity) 
    { 
        targetDirectionalIntensity = intensity;
        hasDirectionalTarget = true;


    }
    
}

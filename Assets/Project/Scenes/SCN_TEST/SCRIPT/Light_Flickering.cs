using UnityEngine;


[System.Serializable]
public class LightFlickerProfile 
{
    public bool isActive = true;
    public bool isFlickering = false;
    public float minIntensity = 0.5f;
    public float maxIntensity = 5.0f;
    public float flickerSpeed = 0.1f;
}

[System.Serializable]
public struct FlickerDayProfile
{
    public LightFlickerProfile morningProfile;
    public LightFlickerProfile afternoonProfile;
}

public class Light_Flickering : MonoBehaviour
{
    [SerializeField] private FlickerDayProfile day1;
    [SerializeField] private FlickerDayProfile day2;
    [SerializeField] private FlickerDayProfile day3;
    [SerializeField] private FlickerDayProfile day4;

    [SerializeField] private Material lightMat;

    [Header("Mesh Renderer Reference")]
    [SerializeField] private MeshRenderer renderer;

    [Header("Material Brightness")]
    [SerializeField] private float minMatIntensity;
    [SerializeField] private float maxMatIntensity;

    private Light light;
    private Color baseColor;
    private Color emissiveColor;
    private float currentMinIntensity = .5f;
    private float currentMaxIntensity = 5.0f;
    private float currentFlickerSpeed = 0.1f;

    [Header("Testing Fields")]
    [SerializeField] private float matIntensity;
    
    private void Start()
    {
        light = GetComponent<Light>();
        LightMapController.instance.OnChangeLights += SetLighting;
    }

    private void Flicker()
    {
        float randomIntensity = Random.Range(currentMinIntensity, currentMaxIntensity);
        light.intensity = randomIntensity;

        matIntensity = Remap(randomIntensity, currentMinIntensity, currentMaxIntensity, minMatIntensity, maxMatIntensity);
        
        Color newEmissive = emissiveColor * matIntensity;
        Color newBase = baseColor * matIntensity;
        lightMat.SetColor("_EmissionColor", newEmissive);
        lightMat.SetColor("_BaseColor", newBase);
    }


    public float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
           float percentage = (value - fromMin) / (fromMax - fromMin);
           return Mathf.Lerp(toMin, toMax, percentage);
    }

    //we really want to set the lamp material to flickering as well - we can do this by editing its emission intensity

    private void SetLighting(int day, bool isMorning)
    {
       
        FlickerDayProfile profile = GetLightProfile(day);
        LightFlickerProfile lightProf = (isMorning) ? profile.morningProfile : profile.afternoonProfile;

        if (!lightProf.isActive)
        {
            light.enabled = false;
            CancelInvoke("Flicker");
            return;
        }

        if (!lightProf.isFlickering)
        {
            CancelInvoke("Flicker");
            return; 
        }

        currentFlickerSpeed = lightProf.flickerSpeed;
        currentMaxIntensity = lightProf.maxIntensity;
        currentMinIntensity = lightProf.minIntensity;

        lightMat = renderer.material;
        lightMat.EnableKeyword("_EMISSION");
        emissiveColor = lightMat.GetColor("_EmissionColor");
        baseColor = lightMat.GetColor("_BaseColor");
        renderer.material = lightMat;
        InvokeRepeating("Flicker", 0f, currentFlickerSpeed);
    }

    private FlickerDayProfile GetLightProfile(int index)
    {
        switch (index)
        {
            case 0:
                return day1;
                break;
            case 1:
                return day2;
                break;
            case 2:
                return day3;
                break;
            case 3:
                return day4;
                break;
        }

        return day1;
    }

}

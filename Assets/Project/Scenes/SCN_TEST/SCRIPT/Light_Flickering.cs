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
    [SerializeField] private float matIntensity;

    [SerializeField] private float minLightIntensity;
    [SerializeField] private float maxLightIntensity;

    private Light light;
    private Color color; 
    public float currentMinIntensity = .5f;
    public float currentMaxIntensity = 5.0f;
    public float currentFlickerSpeed = 0.1f;

    
    private void Start()
    {
        light = GetComponent<Light>();
        LightMapController.instance.OnChangeLights += SetLighting;
    }

    private void Flicker()
    {
        float randomIntensity = Random.Range(currentMinIntensity, currentMaxIntensity);
        light.intensity = randomIntensity;

        float matIntensity = Remap(randomIntensity, currentMinIntensity, minLightIntensity, currentMaxIntensity, maxLightIntensity);
        lightMat.SetColor("_EmissionColor", Color.red);
    }


    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
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

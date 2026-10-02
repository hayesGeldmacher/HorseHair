using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class LensBreathing : MonoBehaviour
{


    [Header("References")]
    [SerializeField] private Volume volume;
    [SerializeField] private UnityEngine.Rendering.Universal.LensDistortion distortion;

    [Header("Values")]
    [SerializeField] private float breathSpeed;
    [SerializeField] private bool goingUp = true;
    [SerializeField] private bool breathing = true;
    [SerializeField] private float breathMin;
    [SerializeField] private float breathMax;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(volume.profile.TryGet<UnityEngine.Rendering.Universal.LensDistortion>(out UnityEngine.Rendering.Universal.LensDistortion lensDistortion))
        {
            distortion = lensDistortion;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (breathing)
        {
            float newBreathSpeed = distortion.intensity.value;
            float finalSpeed = Time.deltaTime * breathSpeed;

            if (goingUp)
            {
                newBreathSpeed += finalSpeed;
                if(newBreathSpeed > breathMax)
                {
                    newBreathSpeed = breathMax;
                    goingUp = false;
                }
            }
            else
            {
                newBreathSpeed -= finalSpeed;
                if(newBreathSpeed < breathMin)
                {
                    newBreathSpeed = breathMin;
                    goingUp = true;
                }

            }

            distortion.intensity.value = newBreathSpeed;

        }
    }
}

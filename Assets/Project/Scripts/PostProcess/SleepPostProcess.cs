using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SleepPostProcess : MonoBehaviour
{

    [SerializeField] Volume volume;
    [SerializeField] private DepthOfField depth;

    //changing focal length from about 50 to 150
    [Header("Volume Settings")]
    [SerializeField] private float minFocal = 50.0f;
    [SerializeField] private float maxFocal = 150.0f;
    [SerializeField] private float focalChangeSpeed = 20.0f;
    [SerializeField] private bool active = false;
    [SerializeField] private bool movingUp = false;
    [SerializeField] private float currentFocal;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volume = transform.GetComponent<Volume>();
        DepthOfField tempD;

        if (volume.profile.TryGet<DepthOfField>(out tempD))
        {
            depth = tempD;
        }

     //   tempD.focalLength.value = 200.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) { return; }
        float focal = depth.focalLength.value;
        if (movingUp)
        {
            if(focal < maxFocal)
            {
                focal += focalChangeSpeed * Time.deltaTime;   
            }
            else
            {
                focal = maxFocal;
                movingUp = false;
            }
        }
        else 
        {
            if (focal > minFocal) 
            {
                focal -= focalChangeSpeed * Time.deltaTime;
            }
            else
            {
                focal = minFocal;
                movingUp = true;
            }
        }

        depth.focalLength.value = focal;
        currentFocal = focal;
    }

    public void TriggerFocalChange( bool setActive)
    {
        active = setActive;
    }
}

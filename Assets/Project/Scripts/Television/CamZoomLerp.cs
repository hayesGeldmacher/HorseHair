using UnityEngine;
using Unity.Cinemachine;
using System;

public class CamZoomLerp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private bool isLerping = false;
    [SerializeField] private float targetFOV;
    [SerializeField] private float oldFOV;
    [SerializeField] private float t;
    [SerializeField] private float currentFOV;

    [SerializeField] private float lerpSpeed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentFOV = cam.Lens.FieldOfView;
    }

    // Update is called once per frame
    void Update()
    {

        if (isLerping)
        {
            t += lerpSpeed * Time.deltaTime;
            if(t > 1.0f)
            {
                t = 1.0f;
                isLerping = false;
            }
            currentFOV = Mathf.Lerp(oldFOV, targetFOV, t);
            cam.Lens.FieldOfView = currentFOV;

        }
    }

    public void SetCameraLerping(float newFOV)
    {
        isLerping = true;
        targetFOV = newFOV;
        oldFOV = currentFOV;
    }
}

using UnityEngine;
using System.Collections;

public class EyelidsFG : MonoBehaviour
{

    [Header("Animation Settings")]
    [SerializeField] private float blinkAnimationSpeed = 0.5f;
    [SerializeField] private Animator blinkAnimator;
    private bool hasBlinked = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!hasBlinked)
        {
            hasBlinked = true;
            StartCoroutine(TriggerBlinkAnimation());
        }
    }

    //function for unique first blinking animation
    private IEnumerator TriggerBlinkAnimation()
    {
 
        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        yield return new WaitForSeconds(0.4f);
        blinkAnimator.SetTrigger("EyesStart");

    }

    public void TriggerEyesDownAnimation()
    {
        blinkAnimator.SetTrigger("EyesDown");
    }
}

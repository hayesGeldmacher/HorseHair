using UnityEngine;
using System.Collections;

public class TelevisionFinalScare : MonoBehaviour
{

    /// <summary>
    /// This script is called by an animation during the final scare sequence in TV night 4
    /// </summary>

    [Header("References")]
    [SerializeField] private Animator blackCanvas;
    [SerializeField] private AudioSource screamAudio;
    [SerializeField] private AudioSource violinSting;
    [SerializeField] private AudioSource canYouSeeSource;

    [Header("Wait Times")]
    [SerializeField] private float wait1;
    [SerializeField] private float wait2;
    [SerializeField] private float wait3;
     private bool triggeredScareSequence = false;

    public void CallScareSequence()
    {
        if (!triggeredScareSequence)
        {
            StartCoroutine(ScareSequence());
        }
    }

    private IEnumerator ScareSequence()
    {

      //  violinSting.Play();
        yield return new WaitForSeconds(wait1);
        canYouSeeSource.Play();
        yield return new WaitForSeconds(wait2);
        blackCanvas.SetTrigger("appear");
        yield return new WaitForSeconds(wait3);
        screamAudio.Play();

    }
}

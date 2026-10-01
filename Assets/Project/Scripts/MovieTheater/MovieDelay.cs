using UnityEngine;
using System.Collections;
using UnityEngine.Video;

public class MovieDelay : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private VideoPlayer vidPlayer;
    [SerializeField] private Animator fadeAnim;

    [Header("Delay")]
    [SerializeField] private float playDelay;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayVideoDelay());
    }

    private IEnumerator PlayVideoDelay()
    {
        yield return new WaitForSeconds(playDelay);
        fadeAnim.SetTrigger("fade");
        vidPlayer.Play();

    }
}

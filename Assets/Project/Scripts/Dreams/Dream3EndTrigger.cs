using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Dream3EndTrigger : MonoBehaviour
{
    private bool triggered = false;

    [Header("References")]
    [SerializeField] private Animator lightBulbAnim;
    [SerializeField] private float bulbAnimDelay = 3.0f;
    [SerializeField] private DSCam cam;
    [SerializeField] private float sceneEndDelay;
    [SerializeField] private AudioVolumeFade fade;


    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            StartCoroutine(PlayBulbAnimation());
        }
    }

    private IEnumerator PlayBulbAnimation()
    {
        cam.StartZooming();
        yield return new WaitForSeconds(bulbAnimDelay);
        fade.StartFadeIn(false, false);
        lightBulbAnim.SetTrigger("play");
        yield return new WaitForSeconds(sceneEndDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }
}

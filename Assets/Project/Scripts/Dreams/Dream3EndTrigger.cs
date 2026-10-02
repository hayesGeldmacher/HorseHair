using UnityEngine;
using System.Collections;

public class Dream3EndTrigger : MonoBehaviour
{
    private bool triggered = false;

    [Header("References")]
    [SerializeField] private Animator lightBulbAnim;
    [SerializeField] private float bulbAnimDelay = 3.0f;


    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            
        }
    }

    private IEnumerator PlayBulbAnimation()
    {
        yield return new WaitForSeconds(bulbAnimDelay);
        lightBulbAnim.SetTrigger("play");

    }
}

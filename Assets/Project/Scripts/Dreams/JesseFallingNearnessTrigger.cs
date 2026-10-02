using UnityEngine;
using System.Collections;

public class JesseFallingNearnessTrigger : PlayerNearTrigger
{ 
    [SerializeField] private Animator spriteAnim;
    [SerializeField] private AudioVolumeFade fade;
    [SerializeField] private float bagScareDelay = 0.25f;
    [SerializeField] private AudioSource bagScareSource;
    
 
    // Update is called once per frame
   override protected void Update()
    {
        base.Update();
    }

    protected override void CallNearTrigger()
    {
        base.CallNearTrigger();
        StartCoroutine(SpritePlayDelay());
    }

    private IEnumerator SpritePlayDelay()
    {
        fade.StartFadeIn(false, false);
        yield return new WaitForSeconds(1.25f);
        spriteAnim.SetTrigger("play");
        yield return new WaitForSeconds(bagScareDelay);
        bagScareSource.Play();
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.ProBuilder;

public class JesseScareTrigger_Nearness : PlayerNearTrigger
{

    [SerializeField] private Animator jesseAnim;
    [SerializeField] private float scareDelay;

    [SerializeField] private AudioSource scareSource;


    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    protected override void CallNearTrigger()
    {
        base.CallNearTrigger();
        StartCoroutine(ScarePlayDelay());
    }

    private IEnumerator ScarePlayDelay()
    {
        yield return new WaitForSeconds(scareDelay);
        jesseAnim.SetTrigger("down");
        scareSource.Play();
    }
}

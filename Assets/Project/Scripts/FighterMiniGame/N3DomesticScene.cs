using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class N3DomesticScene : MonoBehaviour
{
    /// <summary>
    /// General summary of the scene:
    /// 
    /// - Door opens, light spills onto us
    /// - Dad says 'Jesse, get over here, I need to talk to ya' 
    /// - Jesse says 'yeah alright dad, coming'
    /// - puts down controller footsteps, and door closes to a sliver
    /// - player keeps fighting until scene 
    /// </summary>

    [Header("References")]
    [SerializeField] private Animator controllerAnim;
    [SerializeField] private Animator lightAnim;

    [Header("Audio")]
    [SerializeField] private AudioSource fightAudio;

    [Header("Dialogue")]
    [SerializeField] private FGDialogueManager fgDialogueManager;

    [SerializeField] private DialogueTrigger dadTrigger;

 //   [Header("Dialogue")]

    //trigger now just for testing
    private void Start()
    {
        TriggerFightScene();
    }

    public void TriggerFightScene()
    {
        StartCoroutine(CommenceFightScene());
    }

    private IEnumerator CommenceFightScene()
    {

        yield return new WaitForSeconds(5.0f);

        //First: door opens, light spills onto the floor
        lightAnim.SetTrigger("open");

        yield return new WaitForSeconds(2.0f);
        fgDialogueManager.TriggerDadDialogue(dadTrigger);
        //wait a second, dad gives dialogue here

        //wait another second, jesse responds, puts down controller
        yield return new WaitForSeconds(5.5f);


        controllerAnim.SetTrigger("gone");

        yield return new WaitForSeconds(5);

        //wait another second, light anim closes
        lightAnim.SetTrigger("close");

        //wait another few seconds, and the fight starts
        yield return new WaitForSeconds(4);

        fightAudio.Play();
        
    }
}

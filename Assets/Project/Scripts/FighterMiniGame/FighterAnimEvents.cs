using UnityEngine;

public class FighterAnimEvents : MonoBehaviour
{

    /// <summary>
    /// Communicates to fighter from animations
    /// attached to same fighter body as the fighter animator
    /// tells the fighter when character has started, finished attacking, et cet.
    /// </summary>

    [Header("References")]
    [SerializeField] private FightCharacter fightCharacter;


    public void StartAttack()
    {
        //call to fightCharacter function - HG
        //fightCharacter should not be able to move or spam more attack when in attack anim phase - HG
        Debug.Log(fightCharacter.gameObject.name + " started attack animation!");
    } 

    public void EndAttack()
    {
        Debug.Log(fightCharacter.gameObject.name + " finished attack animation!");
    }
}

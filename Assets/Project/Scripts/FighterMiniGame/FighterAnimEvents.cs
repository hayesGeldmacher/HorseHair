using UnityEngine;

public class FighterAnimEvents : MonoBehaviour
{
    /// <summary>
    /// Communicates to fighter from animations
    /// attached to same fighter body as the fighter animator
    /// tells the fighter when character has finished attacking, hit, grabbed, et cet.
    /// </summary>

    [Header("References")]
    [SerializeField] private FightCharacter fightCharacter;

    private void Reset()
    {
        fightCharacter = GetComponentInParent<FightCharacter>();
    }

    private void Awake()
    {
        if (fightCharacter == null)
            fightCharacter = GetComponentInParent<FightCharacter>();
    }

    public void PerformAttackHit()
    {
        if (fightCharacter == null)
            return;

        fightCharacter.PerformAttackHit(); //applies punch kick or special hit at the animation contact frame - AB
    }

    public void PerformGrabHit()
    {
        if (fightCharacter == null)
            return;

        fightCharacter.PerformGrabHit(); //applies grab at the animation contact frame - AB
    }

    public void EndAttack()
    {
        if (fightCharacter == null)
            return;

        fightCharacter.EndAttackAnimation(); //unlocks attack input once attack animation finishes - AB

        Debug.Log(fightCharacter.gameObject.name + " finished attack animation!");
    }
}
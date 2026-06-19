using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FighterAnimation : MonoBehaviour
{

    /// <summary>
    /// controls the visuals for fighter animation based on fighter state and actions performed
    /// Used for both player and AI fighters
    /// </summary>
    
    public enum HeightState
    {
        High,
        Low,
        Ground
    }

    public enum ActionState
    {
        Idle, 
        Moving, 
        Attacking,
        Blocking, 
        Hurt
    }

    [Header("References")]
    private Animator anim;
    [SerializeField] private FightCharacter character;

    [Tooltip("Should character shuffle or walk?")]
    public bool walkNormal = false; //
    private bool isMoving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = transform.GetComponent<Animator>();
        anim.SetBool("walkNormal", walkNormal);
    }

    private void Update()
    {
        isMoving = character.isMoving;
        anim.SetBool("moving", isMoving);

    }
}

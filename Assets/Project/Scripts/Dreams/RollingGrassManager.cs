using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RollingGrassManager : MonoBehaviour
{
    [SerializeField] private FightCharacter fightCharacter; 
    [SerializeField] private DecalProjector projector;
    [SerializeField] private float currentTexOffset = 0;
    [SerializeField] private float offsetSpeed;
    [SerializeField] private bool charMoving;
    [SerializeField] private bool canMove = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        charMoving = fightCharacter.movingForward;
        if (!charMoving || !canMove) { return; }

        currentTexOffset += offsetSpeed * Time.deltaTime;
        if(currentTexOffset >= 1) { currentTexOffset = 0; }
        projector.uvBias = new Vector2(currentTexOffset, 0);
    }

    public void EnableMove(bool enable)
    {
        canMove = enable;
    }
}

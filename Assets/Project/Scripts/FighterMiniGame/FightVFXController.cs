using UnityEngine;

public class FighterVFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FightCharacter fightCharacter;
    [SerializeField] private Transform opponentTransform;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject blockVFX;

    [Header("VFX Scale")]
    [SerializeField] private float VFXScale = 2f;

    [Header("VFX Cleanup")]
    [SerializeField] private float VFXDestroyDelay = 2f;

    [Header("World Position Offset")]
    [SerializeField] private Vector3 worldPositionOffset = new Vector3(0f, 0f, -5f);

    [Header("Hit Position Settings")]
    [SerializeField] private float highHitHeight = 2.5f;
    [SerializeField] private float midHitHeight = 1.2f;
    [SerializeField] private float lowHitHeight = 0.3f;

    [Tooltip("How far in front of the defender the hit VFX appears.")]
    [SerializeField] private float hitForwardOffset = 0f;

    [Header("Block Position Settings")]
    [SerializeField] private float highBlockHeight = 2.5f;
    [SerializeField] private float midBlockHeight = 1.2f;
    [SerializeField] private float lowBlockHeight = 0.3f;

    [Tooltip("How far in front of the defender the block VFX appears.")]
    [SerializeField] private float blockForwardOffset = 0f;

    private void Reset()
    {
        fightCharacter = GetComponent<FightCharacter>();
    }

    private void OnEnable()
    {
        if (fightCharacter != null)
        {
            fightCharacter.MovePerformed += OnMovePerformed;
        }
    }

    private void OnDisable()
    {
        if (fightCharacter != null)
        {
            fightCharacter.MovePerformed -= OnMovePerformed;
        }
    }

    private void OnMovePerformed(FightCharacter attacker, FighterMoveType moveType, FighterMoveResult result)
    {
        if (result == FighterMoveResult.Hit)
        {
            Vector3 spawnPosition = GetHitSpawnPosition(moveType);
            SpawnVFX(hitVFX, spawnPosition);
        }
        else if (result == FighterMoveResult.Blocked)
        {
            Vector3 spawnPosition = GetBlockSpawnPosition(moveType);
            SpawnVFX(blockVFX, spawnPosition);
        }
    }

    private Vector3 GetHitSpawnPosition(FighterMoveType moveType)
    {
        Transform defender = GetDefenderTransform();

        float height = GetHitHeight(moveType);
        Vector3 directionToAttacker = GetDirectionFromDefenderToAttacker(defender);

        return defender.position + Vector3.up * height + directionToAttacker * hitForwardOffset;
    }

    private Vector3 GetBlockSpawnPosition(FighterMoveType moveType)
    {
        Transform defender = GetDefenderTransform();

        float height = GetBlockHeight(moveType);
        Vector3 directionToAttacker = GetDirectionFromDefenderToAttacker(defender);

        return defender.position + Vector3.up * height + directionToAttacker * blockForwardOffset;
    }

    private float GetHitHeight(FighterMoveType moveType)
    {
        switch (moveType)
        {
            case FighterMoveType.CrouchingPunch:
            case FighterMoveType.CrouchingKick:
                return lowHitHeight;

            case FighterMoveType.StandingKick:
                return midHitHeight;

            case FighterMoveType.StandingPunch:
            case FighterMoveType.JumpingPunch:
            case FighterMoveType.JumpingKick:
            default:
                return highHitHeight;
        }
    }

    private float GetBlockHeight(FighterMoveType moveType)
    {
        switch (moveType)
        {
            case FighterMoveType.CrouchingPunch:
            case FighterMoveType.CrouchingKick:
                return lowBlockHeight;

            case FighterMoveType.StandingKick:
                return midBlockHeight;

            case FighterMoveType.StandingPunch:
            case FighterMoveType.JumpingPunch:
            case FighterMoveType.JumpingKick:
            default:
                return highBlockHeight;
        }
    }

    private Transform GetDefenderTransform()
    {
        if (opponentTransform != null)
        {
            return opponentTransform;
        }

        return transform;
    }

    private Vector3 GetDirectionFromDefenderToAttacker(Transform defender)
    {
        Vector3 direction = transform.position - defender.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
        }

        return direction.normalized;
    }

    private void SpawnVFX(GameObject prefab, Vector3 spawnPosition)
    {
        if (prefab == null)
        {
            return;
        }

        spawnPosition += worldPositionOffset;

        Quaternion spawnRotation = Quaternion.identity;

        GameObject vfxInstance = Instantiate(prefab, spawnPosition, spawnRotation);

        vfxInstance.transform.localScale *= VFXScale;

        Destroy(vfxInstance, VFXDestroyDelay);
    }
}
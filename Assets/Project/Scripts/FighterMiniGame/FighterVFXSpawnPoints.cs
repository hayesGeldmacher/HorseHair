using UnityEngine;

/// <summary>
/// Holds shared VFX spawn points for this fighter.
/// Hit and block VFX can use the same high, mid, and low points.
/// </summary>
public class FighterVFXSpawnPoints : MonoBehaviour
{
    [Header("Shared Hit / Block Points")]
    [Tooltip("Used for high hits and high blocks")]
    [SerializeField] private Transform highVFXPoint;

    [Tooltip("Used for mid hits and mid blocks")]
    [SerializeField] private Transform midVFXPoint;

    [Tooltip("Used for low hits and low blocks")]
    [SerializeField] private Transform lowVFXPoint;

    [Header("Special Case Points")]
    [SerializeField] private Transform grabVFXPoint;
    [SerializeField] private Transform specialVFXPoint;

    public Transform GetHitPoint(FighterMoveType moveType)
    {
        return GetPointForMove(moveType);
    }

    public Transform GetBlockPoint(FighterMoveType moveType)
    {
        return GetPointForMove(moveType);
    }

    private Transform GetPointForMove(FighterMoveType moveType)
    {
        switch (moveType)
        {
            case FighterMoveType.CrouchingPunch:
            case FighterMoveType.CrouchingKick:
                return lowVFXPoint != null ? lowVFXPoint : transform;

            case FighterMoveType.StandingKick:
                return midVFXPoint != null ? midVFXPoint : transform;

            case FighterMoveType.Grab:
                return grabVFXPoint != null ? grabVFXPoint : midVFXPoint != null ? midVFXPoint : transform;

            case FighterMoveType.Special:
                return specialVFXPoint != null ? specialVFXPoint : highVFXPoint != null ? highVFXPoint : transform;

            case FighterMoveType.StandingPunch:
            case FighterMoveType.JumpingPunch:
            case FighterMoveType.JumpingKick:
            default:
                return highVFXPoint != null ? highVFXPoint : transform;
        }
    }
}
using UnityEngine;

public class FighterVFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FightCharacter fightCharacter;
    [SerializeField] private Transform opponentTransform;
    [SerializeField] private FighterVFXSpawnPoints opponentVFXPoints;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject blockVFX;
    [SerializeField] private GameObject grabVFX;
    [SerializeField] private GameObject specialVFX;

    [Header("VFX Scale")]
    [SerializeField] private float VFXScale = 2f;

    [Header("VFX Cleanup")]
    [SerializeField] private float VFXDestroyDelay = 2f;

    [Header("World Offset")]
    [Tooltip("Offset to apply to the spawn position of the VFX in world space.")]
    [SerializeField] private Vector3 worldPositionOffset = Vector3.zero;

    private void Reset()
    {
        fightCharacter = GetComponent<FightCharacter>();
    }

    private void Awake()
    {
        AssignMissingReferences();
    }

    private void OnEnable()
    {
        AssignMissingReferences();

        if (fightCharacter != null)
            fightCharacter.MovePerformed += OnMovePerformed;
    }

    private void OnDisable()
    {
        if (fightCharacter != null)
            fightCharacter.MovePerformed -= OnMovePerformed;
    }

    private void AssignMissingReferences()
    {
        if (fightCharacter == null)
            fightCharacter = GetComponent<FightCharacter>();

        if (opponentVFXPoints == null && opponentTransform != null)
        {
            opponentVFXPoints = opponentTransform.GetComponent<FighterVFXSpawnPoints>();

            if (opponentVFXPoints == null)
                opponentVFXPoints = opponentTransform.GetComponentInChildren<FighterVFXSpawnPoints>();
        }
    }

    private void OnMovePerformed(FightCharacter attacker, FighterMoveType moveType, FighterMoveResult result)
    {
        AssignMissingReferences();

        if (result == FighterMoveResult.Hit)
        {
            Vector3 spawnPosition = GetHitSpawnPosition(moveType);
            GameObject prefab = GetHitPrefab(moveType);
            SpawnVFX(prefab, spawnPosition);
        }
        else if (result == FighterMoveResult.Blocked)
        {
            Vector3 spawnPosition = GetBlockSpawnPosition(moveType);
            SpawnVFX(blockVFX, spawnPosition);
        }
    }

    private Vector3 GetHitSpawnPosition(FighterMoveType moveType)
    {
        if (opponentVFXPoints != null)
        {
            Transform point = opponentVFXPoints.GetHitPoint(moveType);
            return point.position;
        }

        return GetFallbackDefenderPosition();
    }

    private Vector3 GetBlockSpawnPosition(FighterMoveType moveType)
    {
        if (opponentVFXPoints != null)
        {
            Transform point = opponentVFXPoints.GetBlockPoint(moveType);
            return point.position;
        }

        return GetFallbackDefenderPosition();
    }

    private GameObject GetHitPrefab(FighterMoveType moveType)
    {
        if (moveType == FighterMoveType.Grab && grabVFX != null)
            return grabVFX;

        if (moveType == FighterMoveType.Special && specialVFX != null)
            return specialVFX;

        return hitVFX;
    }

    private Vector3 GetFallbackDefenderPosition()
    {
        if (opponentTransform != null)
            return opponentTransform.position;

        return transform.position;
    }

    private void SpawnVFX(GameObject prefab, Vector3 spawnPosition)
    {
        if (prefab == null)
            return;

        spawnPosition += worldPositionOffset;

        Quaternion spawnRotation = Quaternion.identity;

        GameObject vfxInstance = Instantiate(prefab, spawnPosition, spawnRotation);

        vfxInstance.transform.localScale *= VFXScale;

        Destroy(vfxInstance, VFXDestroyDelay);
    }
}
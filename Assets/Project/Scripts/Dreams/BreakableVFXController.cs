using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// This class is responsible for spawning VFX objects when a breakable is hit
/// stripped-down version of FightVFXController, written by Angelina
/// </summary>

public class BreakableVFXController : MonoBehaviour
{


    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitVFX;

    [Header("Normal VFX Scale")]
    [Tooltip("Scale multiplier for normal hit, block, and grab VFX.")]
    [SerializeField] private float normalVFXScale = 2f;

    [Header("Special VFX Scale")]
    [Tooltip("Scale multiplier for special attack VFX.")]
    [SerializeField] private float specialVFXScale = 2f;

    [Header("VFX Cleanup")]
    [SerializeField] private float VFXDestroyDelay = 2f;

    [SerializeField] private Transform highVFXPoint;
    [SerializeField] private Transform farmerHandTransform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SpawnNormalVFX(Vector3 spawnPosition)
    {

        Quaternion spawnRotation = Quaternion.identity;

        Vector3 closestPoint = highVFXPoint.position;
        closestPoint.y = farmerHandTransform.position.y;

        GameObject vfxInstance = Instantiate(hitVFX, closestPoint, spawnRotation);
        vfxInstance.transform.localScale *= normalVFXScale;

        Destroy(vfxInstance, VFXDestroyDelay);
    }
}

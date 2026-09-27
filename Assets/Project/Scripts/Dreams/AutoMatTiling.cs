using UnityEngine;

public class AutoMatTiling : MonoBehaviour{

    [Header("References")]
    [SerializeField] private Material tilingMat;

    [Header("Tiling Controls")]
    [SerializeField] private float tilingSpeedX;
    [SerializeField] private float tilingSpeedY;
    [SerializeField] private bool isTiling = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilingMat.mainTextureOffset = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isTiling)
        {
            float xTiling = tilingMat.mainTextureOffset.x;
            float yTiling = tilingMat.mainTextureOffset.y;

            xTiling += tilingSpeedX * Time.deltaTime;
            yTiling += tilingSpeedY * Time.deltaTime;

            if(xTiling >= 1)
            {
                xTiling = 0;
            }

            if(yTiling >= 1)
            {
                yTiling = 0;
            }
            tilingMat.mainTextureOffset = new Vector2(xTiling, yTiling);
        }
    }
}

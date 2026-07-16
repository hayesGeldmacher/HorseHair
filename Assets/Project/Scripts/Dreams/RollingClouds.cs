using UnityEngine;

public class RollingClouds : MonoBehaviour
{

    [SerializeField] private Material mat;
    [SerializeField] private float currentTexOffsetX = 0;
    [SerializeField] private float currentTexOffsetY = 0;
    [SerializeField] private float offsetSpeedX;
    [SerializeField] private float offsetSpeedY;
    [SerializeField] private bool canMove = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (!canMove) { return; }

        if (Mathf.Abs(offsetSpeedX) > 0) { currentTexOffsetX += offsetSpeedX * Time.deltaTime; }
        if (Mathf.Abs(offsetSpeedY) > 0) { currentTexOffsetY += offsetSpeedY * Time.deltaTime; }
        

        if (Mathf.Abs(currentTexOffsetX) >= 1) { currentTexOffsetX = 0; }
        if(Mathf.Abs(currentTexOffsetY) >= 1) {currentTexOffsetY = 0; }

        mat.SetTextureOffset("_MainTex",  new Vector2(currentTexOffsetX, currentTexOffsetY));
        mat.mainTextureOffset = new Vector2(currentTexOffsetX, currentTexOffsetY);
    }

    public void EnableMove(bool enable)
    {
        canMove = enable;
    }
}

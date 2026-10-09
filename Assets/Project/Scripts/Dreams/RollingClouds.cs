using Autodesk.Fbx;
using UnityEngine;

public class RollingClouds : MonoBehaviour
{

    [SerializeField] private Material mat;
    [SerializeField] private float currentTexOffsetX = 0;
    [SerializeField] private float currentTexOffsetY = 0;
    [SerializeField] private float offsetSpeedX;
    [SerializeField] private float offsetSpeedY;
    [SerializeField] private bool canMove = true;

    [Header("Stretching")]
    [SerializeField] private bool stretchClouds = false;
    [SerializeField] private float stretchSpeedX;
    [SerializeField] private float stretchSpeedY;
    [SerializeField] private float currentStretchX = 1.0f;
    [SerializeField] private float currentStretchY = 1.0f;
    private bool stretchingUpX = false;
    private bool stretchingUpY = false;
    [SerializeField] private float maxStretchX = 1.2f;
    [SerializeField] private float minStretchX = 0.8f;
    [SerializeField] private float maxStretchY = 1.2f;
    [SerializeField] private float minStretchY = 0.8f;

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

        if (stretchClouds) { StretchUpdate(); }
    }

    private void StretchUpdate()
    {

        //stretch on X axis
        if (stretchingUpX)
        {
            currentStretchX += stretchSpeedX * Time.deltaTime;
            if(currentStretchX >= maxStretchX)
            {
                currentStretchX = maxStretchX;
                stretchingUpX = false;
            }
        }
        else
        {
            currentStretchX -= stretchSpeedX * Time.deltaTime;
            if(currentStretchX <= minStretchX)
            {
                currentStretchX = minStretchX;
                stretchingUpX = true;
            }
        }

        //stretch on y axis
        if (stretchingUpY)
        {
            currentStretchY += stretchSpeedY * Time.deltaTime;
            if (currentStretchY >= maxStretchY)
            {
                currentStretchY = maxStretchY;
                stretchingUpY = false;
            }
        }
        else
        {
            currentStretchY -= stretchSpeedY * Time.deltaTime;
            if (currentStretchY <= minStretchY)
            {
                currentStretchY = minStretchY;
                stretchingUpY = true;
            }
        }

        mat.SetTextureScale("_MainText", new Vector2(currentStretchX, currentStretchY));
        mat.mainTextureScale = new Vector2(currentStretchX, currentStretchY);
    }

    public void EnableMove(bool enable)
    {
        canMove = enable;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
//This script allows for the angled health bar fill 
/// </summary>
[AddComponentMenu("Angled Health Bar Fill")]
public class AngledHealthBarFill : MaskableGraphic
{
    [Header("Fill")]
    [Range(0f, 1f)]
    [SerializeField] private float fillAmount = 1f;

    [Tooltip("Enable this for the health bar on the right side.")]
    [SerializeField] private bool fillFromRight;

    [Header("Inner Bar Shape")]
    [Tooltip("Bottom left corner of the colored interior.")]
    [SerializeField]
    private Vector2 bottomLeft =
        new Vector2(0.025f, 0.20f);

    [Tooltip("Top left corner of the colored interior.")]
    [SerializeField]
    private Vector2 topLeft =
        new Vector2(0.09f, 0.80f);

    [Tooltip("Top right corner of the colored interior.")]
    [SerializeField]
    private Vector2 topRight =
        new Vector2(0.97f, 0.80f);

    [Tooltip("Bottom right corner of the colored interior.")]
    [SerializeField]
    private Vector2 bottomRight =
        new Vector2(0.90f, 0.20f);

    public float FillAmount
    {
        get { return fillAmount; }
    }

    public void SetFill(float normalizedHealth)
    {
        float newAmount = Mathf.Clamp01(normalizedHealth);

        if (Mathf.Approximately(fillAmount, newAmount))
            return;

        fillAmount = newAmount;
        SetVerticesDirty();
    }

    protected override void GenerateMesh(
        VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        if (fillAmount <= 0f)
            return;

        Rect rect = rectTransform.rect;

        List<Vector2> polygon = new List<Vector2>
    {
        ToLocalPosition(bottomLeft, rect),
        ToLocalPosition(topLeft, rect),
        ToLocalPosition(topRight, rect),
        ToLocalPosition(bottomRight, rect)
    };

        float minimumX = polygon[0].x;
        float maximumX = polygon[0].x;

        for (int i = 1; i < polygon.Count; i++)
        {
            minimumX = Mathf.Min(minimumX, polygon[i].x);
            maximumX = Mathf.Max(maximumX, polygon[i].x);
        }


        float clipX = fillFromRight
            ? Mathf.Lerp(maximumX, minimumX, fillAmount)
            : Mathf.Lerp(minimumX, maximumX, fillAmount);

        polygon = ClipHorizontally(
            polygon,
            clipX,
            fillFromRight
        );

        if (polygon.Count < 3)
            return;

        for (int i = 0; i < polygon.Count; i++)
        {
            vertexHelper.AddVert(
                polygon[i],
                color,
                Vector2.zero
            );
        }

        for (int i = 1; i < polygon.Count - 1; i++)
        {
            vertexHelper.AddTriangle(0, i, i + 1);
        }
    }

    private Vector2 ToLocalPosition(
        Vector2 normalizedPosition,
        Rect rect)
    {
        return new Vector2(
            Mathf.Lerp(
                rect.xMin,
                rect.xMax,
                normalizedPosition.x
            ),
            Mathf.Lerp(
                rect.yMin,
                rect.yMax,
                normalizedPosition.y
            )
        );
    }

    private List<Vector2> ClipHorizontally(
        List<Vector2> input,
        float clipX,
        bool keepRightSide)
    {
        List<Vector2> output = new List<Vector2>();

        if (input.Count == 0)
            return output;

        Vector2 previous = input[input.Count - 1];
        bool previousInside = IsInside(
            previous,
            clipX,
            keepRightSide
        );

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            bool currentInside = IsInside(
                current,
                clipX,
                keepRightSide
            );

            if (currentInside)
            {
                if (!previousInside)
                {
                    output.Add(GetIntersection(
                        previous,
                        current,
                        clipX
                    ));
                }

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(GetIntersection(
                    previous,
                    current,
                    clipX
                ));
            }

            previous = current;
            previousInside = currentInside;
        }

        return output;
    }

    private bool IsInside(
        Vector2 point,
        float clipX,
        bool keepRightSide)
    {
        return keepRightSide
            ? point.x >= clipX
            : point.x <= clipX;
    }

    private Vector2 GetIntersection(
        Vector2 start,
        Vector2 end,
        float clipX)
    {
        float differenceX = end.x - start.x;

        if (Mathf.Approximately(differenceX, 0f))
            return new Vector2(clipX, start.y);

        float interpolation =
            (clipX - start.x) / differenceX;

        return Vector2.Lerp(start, end, interpolation);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        fillAmount = Mathf.Clamp01(fillAmount);
        SetVerticesDirty();
    }
#endif
}
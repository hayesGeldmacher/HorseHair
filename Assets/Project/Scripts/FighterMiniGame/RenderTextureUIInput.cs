using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(MeshCollider))]
public sealed class RenderTextureUIInput :
    MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerMoveHandler,
    IPointerExitHandler
{
    [SerializeField] private Canvas sourceCanvas;
    [SerializeField] private Camera sourceUICamera;
    [SerializeField] private RenderTexture sourceTexture;

    [SerializeField] private MeshCollider screenCollider;
    [SerializeField] private MeshRenderer screenRenderer;

    [SerializeField] private string textureProperty = "_BaseMap";
    [SerializeField] private bool applyMaterialTilingAndOffset = true;
    [SerializeField] private bool flipX;
    [SerializeField] private bool flipY;

    private GraphicRaycaster sourceRaycaster;
    private PointerEventData screenPointer;
    private PointerEventData hoverEvent;
    private EventSystem hoverSystem;
    private GameObject hoverTarget;
    private GameObject nextHoverTarget;

    private readonly List<RaycastResult> uiResults =
        new List<RaycastResult>();

    private void Awake()
    {
        if (screenCollider == null)
        {
            screenCollider = GetComponent<MeshCollider>();
        }

        if (screenRenderer == null)
        {
            screenRenderer = GetComponent<MeshRenderer>();
        }

        if (sourceUICamera == null && sourceCanvas != null)
        {
            sourceUICamera = sourceCanvas.worldCamera;
        }

        if (sourceTexture == null && sourceUICamera != null)
        {
            sourceTexture = sourceUICamera.targetTexture;
        }

        if (sourceCanvas != null)
        {
            sourceRaycaster =
                sourceCanvas.GetComponent<GraphicRaycaster>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        screenPointer = eventData;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        screenPointer = eventData;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        screenPointer = null;
        SetHoverTarget(null);
    }

    private void OnDisable()
    {
        screenPointer = null;
        SetHoverTarget(null);
    }

    private void LateUpdate()
    {
        if (screenPointer == null)
        {
            return;
        }

        nextHoverTarget = null;
        ProcessPointer(screenPointer, true);
        SetHoverTarget(nextHoverTarget);
    }

    private void SetHoverTarget(GameObject target)
    {
        if (hoverTarget == target)
        {
            return;
        }

        if (hoverEvent == null)
        {
            hoverTarget = target;
            return;
        }

        Transform common =
            hoverTarget != null ? hoverTarget.transform : null;

        while (common != null &&
               (target == null || !target.transform.IsChildOf(common)))
        {
            common = common.parent;
        }

        Transform current =
            hoverTarget != null ? hoverTarget.transform : null;

        while (current != null && current != common)
        {
            Transform parent = current.parent;

            ExecuteEvents.Execute(
                current.gameObject,
                hoverEvent,
                ExecuteEvents.pointerExitHandler
            );

            hoverEvent.hovered.Remove(current.gameObject);
            current = parent;
        }

        hoverTarget = target;
        hoverEvent.pointerEnter = target;

        current = target != null ? target.transform : null;

        while (current != null && current != common)
        {
            Transform parent = current.parent;

            hoverEvent.hovered.Add(current.gameObject);

            ExecuteEvents.Execute(
                current.gameObject,
                hoverEvent,
                ExecuteEvents.pointerEnterHandler
            );

            current = parent;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ProcessPointer(eventData, false);
    }

    private void ProcessPointer(
        PointerEventData eventData,
        bool hoverOnly)
    {
        if (!hoverOnly &&
            eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (EventSystem.current == null ||
            screenCollider == null ||
            sourceRaycaster == null ||
            sourceTexture == null ||
            !screenCollider.enabled ||
            !sourceRaycaster.isActiveAndEnabled)
        {
            return;
        }

        Camera viewingCamera =
            !hoverOnly && eventData.pressEventCamera != null
                ? eventData.pressEventCamera
                : eventData.enterEventCamera;

        if (viewingCamera == null)
        {
            viewingCamera = Camera.main;
        }

        if (viewingCamera == null)
        {
            return;
        }

        Ray ray =
            viewingCamera.ScreenPointToRay(eventData.position);

        if (!screenCollider.Raycast(
                ray,
                out RaycastHit screenHit,
                viewingCamera.farClipPlane))
        {
            return;
        }

        Vector2 uv = screenHit.textureCoord;

        ApplyMaterialUVTransform(ref uv);

        if (flipX)
        {
            uv.x = 1f - uv.x;
        }

        if (flipY)
        {
            uv.y = 1f - uv.y;
        }

        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        Vector2 renderTexturePosition = new Vector2(
            uv.x * sourceTexture.width,
            uv.y * sourceTexture.height
        );

        PointerEventData mappedEvent =
            new PointerEventData(EventSystem.current)
            {
                position = renderTexturePosition,
                button = eventData.button,
                pointerId = eventData.pointerId,
                clickCount = eventData.clickCount
            };

        uiResults.Clear();
        sourceRaycaster.Raycast(mappedEvent, uiResults);

        if (uiResults.Count == 0)
        {
            return;
        }

        RaycastResult topResult = uiResults[0];

        if (hoverOnly)
        {
            if (hoverEvent == null ||
                hoverSystem != EventSystem.current)
            {
                SetHoverTarget(null);

                hoverSystem = EventSystem.current;
                hoverEvent = new PointerEventData(hoverSystem);
            }

            hoverEvent.delta =
                mappedEvent.position - hoverEvent.position;

            hoverEvent.position = mappedEvent.position;
            hoverEvent.pointerId = mappedEvent.pointerId;
            hoverEvent.pointerCurrentRaycast = topResult;

            nextHoverTarget = topResult.gameObject;
            return;
        }

        GameObject clickHandler =
            ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                topResult.gameObject
            );

        if (clickHandler == null)
        {
            return;
        }

        mappedEvent.pointerCurrentRaycast = topResult;

        ExecuteEvents.Execute(
            clickHandler,
            mappedEvent,
            ExecuteEvents.pointerClickHandler
        );
    }

    private void ApplyMaterialUVTransform(ref Vector2 uv)
    {
        if (!applyMaterialTilingAndOffset ||
            screenRenderer == null)
        {
            return;
        }

        Material material = screenRenderer.sharedMaterial;

        if (material == null ||
            !material.HasProperty(textureProperty))
        {
            return;
        }

        Vector2 scale =
            material.GetTextureScale(textureProperty);

        Vector2 offset =
            material.GetTextureOffset(textureProperty);

        uv = Vector2.Scale(uv, scale) + offset;
    }
}
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter, IPoolable
{
    [FormerlySerializedAs("image")] // hunt it down later
    public Image ImageUI;
    public TowerComponent Component { get; private set; }
    public Vector2Int[] Shape { get => Component?.Shape ?? System.Array.Empty<Vector2Int>(); set => Component.Shape = value; } // I lost some guarantees here
    public int ItemWidth => Component?.Size.x ?? 0;
    public int ItemHeight => Component?.Size.y ?? 0;

    [HideInInspector] public GameObject Object => gameObject;
    [HideInInspector] public IObjectPool<IPoolable> Pool { get; set; }  

    [HideInInspector] public Vector2Int CurrentGridPos => Component.position;
    [HideInInspector] public Vector2Int originalGridPos;
    [HideInInspector] public RectTransform rectTransform;
    private Vector2 originalPivot;

    private CanvasGroup canvasGroup;
    private RectTransform sellArea;
    private TextMeshProUGUI sellText;

    private bool isDragged = false;

    private float animMutliplier = 1f;
    private Coroutine animationRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Setting Component

    public void SpawnAction(Vector3 position)
    {
        // set position
        rectTransform.anchoredPosition = (Vector2)position;

        // reset color
        ImageUI.color = Color.white;

        // reset originalGridPos
        originalGridPos = CurrentGridPos;
    }

    public void SetComponent(TowerComponent component)
    {
        this.Component = component;

        // Set ImageUI
        ImageUI.sprite = component.Image;

        // Ensure the visual size of the RectTransform matches its cell dimensions
        float totalWidth = ItemWidth * InventoryManager.CellSize + (ItemWidth - 1) * InventoryManager.Spacing;
        float totalHeight = ItemHeight * InventoryManager.CellSize + (ItemHeight - 1) * InventoryManager.Spacing;
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    // Dragging

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;
        // Save original position in case need to revert an invalid drop
        originalGridPos = CurrentGridPos;

        // Clear space in the logic grid
        InventoryManager.ClearItemSpace(this);

        // Move to DragCanvas so it renders on top of everything
        transform.SetParent(InventoryManager.GetDragCanvas(), false);

        originalPivot = rectTransform.pivot;

        // Shift the pivot to exactly where the mouse clicked
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos))
        {
            Vector2 newPivot = new Vector2(
                (localMousePos.x / rectTransform.rect.width) + rectTransform.pivot.x,
                (localMousePos.y / rectTransform.rect.height) + rectTransform.pivot.y
            );
            SetPivotWithoutSnapping(newPivot);
        }
        // -----------------------------

        //canvasGroup.alpha = 0.8f;

        sellArea = InventoryManager.GetSellArea();
        sellText = sellArea.GetComponentInChildren<TextMeshProUGUI>();

        // Start animatino
        animationRoutine = StartCoroutine(Animation());
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / InventoryManager.GetMainCanvas().scaleFactor;

        bool isMouseInSellArea = RectTransformUtility.RectangleContainsScreenPoint(
            sellArea,
            eventData.position,
            eventData.pressEventCamera
        );

        // Color item based on where it is
        if (isMouseInSellArea)
        {
            ImageUI.color = new Color(1f, .7f, .7f, 1f);
            animMutliplier = 3f;
            sellText.text = $"{Component.Price * BuildingManager.instance.SaleMultiplier}€";
        }
        else
        {
            ImageUI.color = Color.white;
            animMutliplier = 1f;
            sellText.text = "€";
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragged = false;
        SetPivotWithoutSnapping(originalPivot);
        // --------------------------------

        // Re-enable raycasts
        canvasGroup.alpha = 1f;
        animMutliplier = 1f;
        sellText.text = "€";

        // Check where the mouse is
        bool isMouseInSellArea = RectTransformUtility.RectangleContainsScreenPoint(
            sellArea,
            eventData.position,
            eventData.pressEventCamera
        );

        if (isMouseInSellArea)
        {
            // Sell component, increase money
            BuildingManager.instance.SellForResources(Component.Price);
            InventoryManager.ReleaseInventoryItem(this);
            InventoryManager.ClearItemSpace(this); // just to be extra safe
        }
        else
        {
            // Ask the manager to handle the snapping and logic
            InventoryManager.HandleItemDrop(this);
        }

        StopCoroutine(animationRoutine);
    }

    private void Update()
    {
        if (isDragged && (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame))
        {
            InventoryManager.HandleRotation(this);
        }
    }
    private void SetPivotWithoutSnapping(Vector2 newPivot)
    {
        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - newPivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);

        // Account for UI scaling
        deltaPosition.x *= rectTransform.localScale.x;
        deltaPosition.y *= rectTransform.localScale.y;

        // CRITICAL: Rotate the position shift by the object's current rotation.
        // If the user rotated the item while dragging, restoring the pivot at OnEndDrag 
        // will cause a jump unless we apply that rotation to the delta.
        deltaPosition = rectTransform.localRotation * deltaPosition;

        rectTransform.pivot = newPivot;
        rectTransform.localPosition -= deltaPosition;
    }

    public bool IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
    {
        // Transform screen point to local rect point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPos,
            eventCamera,
            out Vector2 localMousePos
        );

        float totalCellSize = InventoryManager.CellSize + InventoryManager.Spacing;

        // Calculate local cell coordinates
        int clickX = Mathf.FloorToInt(localMousePos.x / totalCellSize);
        int clickY = Mathf.FloorToInt(Mathf.Abs(localMousePos.y) / totalCellSize);

        // Check if clicked on filled cell
        foreach (Vector2Int coord in Shape)
        {
            if (coord.x == clickX && coord.y == clickY)
                return true;
        }
        
        return false;
    }

    public void Return2Pool()
    {
        Pool.Release(this);
    }

    private IEnumerator Animation()
    {
        float Map(float v, float imin, float imax, float omin, float omax)
        {
            return omin + (v - imin) / (imax - imin) * (omax - omin);
        }

        const float animTime = 1.5f;

        float t = 0f;

        while (true)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Map(Mathf.Cos(2f * Mathf.PI * t / animTime * animMutliplier), -1f, 1f, .7f, .9f);
            canvasGroup.alpha = alpha;

            yield return new WaitForEndOfFrame();
        }
    }


}

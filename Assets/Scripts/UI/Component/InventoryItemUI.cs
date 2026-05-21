using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter, IPoolable
{
    [SerializeField] private Image image;

    public TowerComponent Component { get; private set; }
    public Vector2Int[] Shape => Component?.Shape ?? System.Array.Empty<Vector2Int>();
    public int ItemWidth => Component?.Size?.x ?? 0;
    public int ItemHeight => Component?.Size?.y ?? 0;

    [HideInInspector] public GameObject Object => gameObject;
    [HideInInspector] public IObjectPool<IPoolable> Pool { get; set; }  

    [HideInInspector] public Vector2Int CurrentGridPos => Component.position;
    [HideInInspector] public Vector2Int originalGridPos;
    [HideInInspector] public RectTransform rectTransform;

    private CanvasGroup canvasGroup;

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

        // reset originalGridPos
        originalGridPos = CurrentGridPos;
    }

    public void SetComponent(TowerComponent component)
    {
        this.Component = component;

        // Set image
        image.sprite = component.Image;

        // Ensure the visual size of the RectTransform matches its cell dimensions
        float totalWidth = ItemWidth * InventoryManager.CellSize + (ItemWidth - 1) * InventoryManager.Spacing;
        float totalHeight = ItemHeight * InventoryManager.CellSize + (ItemHeight - 1) * InventoryManager.Spacing;
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    // Dragging

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save original position in case need to revert an invalid drop
        originalGridPos = CurrentGridPos;

        // Clear space in the logic grid
        InventoryManager.ClearItemSpace(this);

        // Move to DragCanvas so it renders on top of everything
        transform.SetParent(InventoryManager.GetDragCanvas(), false);

        // Disable raycasts so the mouse can "see" through the item to the grid below when dropping
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the item with the mouse, scaling appropriately for the Canvas
        rectTransform.anchoredPosition += eventData.delta / InventoryManager.GetMainCanvas().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable raycasts
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Ask the manager to handle the snapping and logic
        InventoryManager.HandleItemDrop(this);
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
}

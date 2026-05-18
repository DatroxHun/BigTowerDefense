using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
    [Header("Item Data")]
    [Tooltip("Define the shape relative to the top-left cell (0,0)")]
    public Vector2Int[] shape;
    public int ItemWidth { get; private set; }
    public int ItemHeight { get; private set; }

    [HideInInspector] public Vector2Int currentGridPos;
    [HideInInspector] public Vector2Int originalGridPos;
    [HideInInspector] public RectTransform rectTransform;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        ItemWidth = shape.Max(c => c.x) + 1;
        ItemHeight = shape.Max(c => c.y) + 1;
    }

    void Start()
    {
        // Ensure the visual size of the RectTransform matches its cell dimensions
        float totalWidth = ItemWidth * InventoryManager.CellSize + (ItemWidth - 1) * InventoryManager.Spacing;
        float totalHeight = ItemHeight * InventoryManager.CellSize + (ItemHeight - 1) * InventoryManager.Spacing;
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

        originalGridPos = currentGridPos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save original position in case need to revert an invalid drop
        originalGridPos = currentGridPos;

        // Clear space in the logic grid
        InventoryManager.ClearItemSpace(this);

        // Move to DragCanvas so it renders on top of everything
        transform.SetParent(InventoryManager.GetDragCanvas());

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
        InventoryManager.HandleItemDrop(this, eventData);
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
        foreach (Vector2Int coord in shape)
        {
            if (coord.x == clickX && coord.y == clickY)
                return true;
        }
        
        return false;
    }
}

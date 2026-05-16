using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Data")]
    public int itemWidth = 2;  // how many cells wide this item is
    public int itemHeight = 3; // how many cells tall this item is

    [HideInInspector] public Vector2Int currentGridPos;
    [HideInInspector] public Vector2Int originalGridPos;
    [HideInInspector] public RectTransform rectTransform;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // Ensure the visual size of the RectTransform matches its cell dimensions
        float totalWidth = itemWidth * InventoryManager.Instance.CellSize + (itemWidth - 1) * InventoryManager.Instance.Spacing;
        float totalHeight = itemHeight * InventoryManager.Instance.CellSize + (itemHeight - 1) * InventoryManager.Instance.Spacing;
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

        originalGridPos = currentGridPos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save original position in case need to revert an invalid drop
        originalGridPos = currentGridPos;

        // Clear space in the logic grid
        InventoryManager.Instance.ClearItemSpace(this);

        // Move to DragCanvas so it renders on top of everything
        transform.SetParent(InventoryManager.Instance.DragCanvas);

        // Disable raycasts so the mouse can "see" through the item to the grid below when dropping
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the item with the mouse, scaling appropriately for the Canvas
        rectTransform.anchoredPosition += eventData.delta / InventoryManager.Instance.MainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable raycasts
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Ask the manager to handle the snapping and logic
        InventoryManager.Instance.HandleItemDrop(this, eventData);
    }
}

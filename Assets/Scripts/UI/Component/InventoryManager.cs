#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Splines.SplineInstantiate;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Grid Settings")]
    [field: SerializeField] public int GridWidth { get; private set; } = 11;
    public int GridHeight { get => NumberOfCells / GridWidth; }
    [field: SerializeField] public int NumberOfCells { get; private set; } = 55;
    public float CellSize { get => gridLayout.cellSize.x; }
    public float Spacing { get => gridLayout.spacing.x; }
    [SerializeField] private GridLayoutGroup gridLayout = null!;
    [SerializeField] private GameObject cellPrefab = null!;

    [Header("References")]
    [field: SerializeField] public RectTransform ItemContainer { get; private set; } = null!;
    [field: SerializeField] public Transform DragCanvas { get; private set; } = null!;
    [field: SerializeField] public Canvas MainCanvas { get; private set; } = null!;

    private InventoryItemUI?[,] grid = null!;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);


        grid = new InventoryItemUI[GridWidth, GridHeight];

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GridWidth;
        for (int i = 0; i < NumberOfCells; i++)
        {
            Instantiate(cellPrefab, gridLayout.transform);
        }
    }

    public void HandleItemDrop(InventoryItemUI item, PointerEventData eventData)
    {
        // Get the item's Top-Left corner in the ItemContainer's local space
        Vector3 localItemPos = ItemContainer.InverseTransformPoint(item.transform.position);

        // Adjust for GridLayoutGroup padding
        float adjustedX = localItemPos.x - gridLayout.padding.left;
        float adjustedY = Mathf.Abs(localItemPos.y) - gridLayout.padding.top;

        // Convert local position to Grid X and Y
        float totalCellSize = CellSize + Spacing;
        int gridX = Mathf.RoundToInt(adjustedX / totalCellSize);
        int gridY = Mathf.RoundToInt(adjustedY / totalCellSize);

        Debug.Log($"gX: {gridX}; gY: {gridY}");

        // Check if the drop is valid
        if (CanPlaceItem(item, gridX, gridY))
        {
            // Valid drop: Place it in the logical grid and snap visually
            PlaceItem(item, gridX, gridY);
            Debug.Log("placable");
        }
        else
        {
            // Invalid drop: Put it back where it came from
            PlaceItem(item, item.originalGridPos.x, item.originalGridPos.y);
            Debug.Log("unplacable");
        }
    }

    private bool CanPlaceItem(InventoryItemUI item, int startX, int startY)
    {
        // Bounds check
        if (startX < 0 || startY < 0 || startX + item.itemWidth > GridWidth || startY + item.itemHeight > GridHeight)
            return false;

        // Check for overlapping items
        for (int dx = 0; dx < item.itemWidth; dx++)
        {
            for (int dy = 0; dy < item.itemHeight; dy++)
            {
                if (grid[startX + dx, startY + dy] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void PlaceItem(InventoryItemUI item, int x, int y)
    {
        // Update array
        for (int i = 0; i < item.itemWidth; i++)
        {
            for (int j = 0; j < item.itemHeight; j++)
            {
                grid[x + i, y + j] = item;
            }
        }

        // Update item's internal data
        item.currentGridPos.Set(x, y);

        // Snap visual position
        item.transform.SetParent(ItemContainer);
        float totalCellSize = CellSize + Spacing;

        Vector2 snappedPosition = new Vector2(
            (x * totalCellSize) + gridLayout.padding.left,
            -((y * totalCellSize) + gridLayout.padding.top) // negative because Y goes down in UI
        );

        item.rectTransform.anchoredPosition = snappedPosition;
    }

    public void ClearItemSpace(InventoryItemUI item)
    {
        // Remove item from logic array so it doesn't collide with itself while dragging
        for (int dx = 0; dx < item.itemWidth; dx++)
        {
            for (int dy = 0; dy < item.itemHeight; dy++)
            {
                grid[item.currentGridPos.x + dx, item.currentGridPos.y + dy] = null;
            }
        }
    }
}

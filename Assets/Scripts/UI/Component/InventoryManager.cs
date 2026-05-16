#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        // 1. Find mouse position relative to the ItemContainer's top-left corner
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ItemContainer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos
        );

        // 2. Convert local position to Grid X and Y
        float totalCellSize = CellSize + Spacing;
        int gridX = Mathf.FloorToInt(localMousePos.x / totalCellSize);
        int gridY = Mathf.FloorToInt(Mathf.Abs(localMousePos.y) / totalCellSize);

        // 3. Check if the drop is valid
        if (CanPlaceItem(item, gridX, gridY))
        {
            // Valid drop: Place it in the logical grid and snap visually
            PlaceItem(item, gridX, gridY);
        }
        else
        {
            // Invalid drop: Put it back where it came from
            PlaceItem(item, item.originalGridPos.x, item.originalGridPos.y);
        }
    }

    private bool CanPlaceItem(InventoryItemUI item, int startX, int startY)
    {
        // Check out of bounds
        if (startX < 0 || startY < 0 || startX + item.itemWidth > GridWidth || startY + item.itemHeight > GridHeight)
        {
            return false;
        }

        // Check for overlapping items
        for (int x = 0; x < item.itemWidth; x++)
        {
            for (int y = 0; y < item.itemHeight; y++)
            {
                if (grid[startX + x, startY + y] != null)
                {
                    return false; // Space is occupied
                }
            }
        }
        return true;
    }

    public void PlaceItem(InventoryItemUI item, int x, int y)
    {
        // 1. Update logic array
        for (int i = 0; i < item.itemWidth; i++)
        {
            for (int j = 0; j < item.itemHeight; j++)
            {
                grid[x + i, y + j] = item;
            }
        }

        // 2. Update item's internal data
        item.currentGridPos.Set(x, y);

        // 3. Snap visual position
        item.transform.SetParent(ItemContainer);
        float totalCellSize = CellSize + Spacing;

        Vector2 snappedPosition = new Vector2(
            x * totalCellSize,
            -(y * totalCellSize) // Negative because Y goes down in UI
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

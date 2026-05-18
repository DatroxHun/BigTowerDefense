#nullable enable

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Splines.SplineInstantiate;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;

    [Header("Grid Settings")]
    [field: SerializeField] public int GridWidth { get; private set; } = 11;
    public int GridHeight { get => NumberOfCells / GridWidth; }
    [field: SerializeField] public int NumberOfCells { get; private set; } = 55;
    public static float CellSize { get => instance.gridLayout.cellSize.x; }
    public static float Spacing { get => instance.gridLayout.spacing.x; }
    [SerializeField] private GridLayoutGroup gridLayout = null!;
    [SerializeField] private GameObject cellPrefab = null!;

    [Header("References")]
    [field: SerializeField] public RectTransform ItemContainer { get; private set; } = null!;
    [field: SerializeField] public Transform DragCanvas { get; private set; } = null!;
    [field: SerializeField] public Canvas MainCanvas { get; private set; } = null!;

    // logical grid
    private static InventoryItemUI?[,] grid = null!;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);


        // Initialize logical grid
        grid = new InventoryItemUI[GridWidth, GridHeight];

        // Initialize grid layout
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GridWidth;
        for (int i = 0; i < NumberOfCells; i++)
        {
            Instantiate(cellPrefab, gridLayout.transform);
        }
    }

    public static Transform GetDragCanvas() => instance.DragCanvas;
    public static Canvas GetMainCanvas() => instance.MainCanvas;

    public static void HandleItemDrop(InventoryItemUI item, PointerEventData eventData)
    {
        // Get the item's Top-Left corner in the ItemContainer's local space
        Vector3 localItemPos = instance.ItemContainer.InverseTransformPoint(item.transform.position);

        // Get the dynamic starting point of the grid
        Vector2 gridOrigin = instance.GetGridOrigin();

        // Adjust for GridLayoutGroup padding
        float adjustedX = localItemPos.x - gridOrigin.x;
        float adjustedY = gridOrigin.y - localItemPos.y;

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

    private static bool CanPlaceItem(InventoryItemUI item, int startX, int startY)
    {
        foreach (Vector2Int coord in item.shape)
        {
            int checkX = startX + coord.x;
            int checkY = startY + coord.y;

            // Bound check
            if (checkX < 0 || checkY < 0 || checkX >= instance.GridWidth || checkY >= instance.GridHeight)
                return false;

            // Overlapping item check
            if (grid[checkX, checkY] != null)
                return false;
        }

        return true;
    }

    public static void PlaceItem(InventoryItemUI item, int x, int y)
    {
        // Update array
        foreach (Vector2Int coord in item.shape)
        {
            grid[x + coord.x, y + coord.y] = item;
        }

        // Update item's internal data
        item.currentGridPos.Set(x, y);

        // Snap visual position
        item.transform.SetParent(instance.ItemContainer);
        float totalCellSize = CellSize + Spacing;
        Vector2 gridOrigin = instance.GetGridOrigin();

        Vector2 snappedPosition = new Vector2(
            gridOrigin.x + x * totalCellSize,
            gridOrigin.y - y * totalCellSize // negative because Y goes down in UI
        );

        item.rectTransform.anchoredPosition = snappedPosition;
    }

    public static void ClearItemSpace(InventoryItemUI item)
    {
        // Remove item from logic array
        foreach (Vector2Int coord in item.shape)
        {
            int clearX = item.currentGridPos.x + coord.x;
            int clearY = item.currentGridPos.y + coord.y;

            // Safety check to avoid index out of bounds if something went wrong
            if (clearX >= 0 && clearX < instance.GridWidth && clearY >= 0 && clearY < instance.GridHeight)
            {
                grid[clearX, clearY] = null;
            }
        }
    }

    public static void SetInventoryGrid(InventoryItemUI[,] grid) => InventoryManager.grid = grid;

    private Vector2 GetGridOrigin()
    {
        // Calculate the total physical size of the grid (all rows and columns)
        float totalGridWidth = (GridWidth * CellSize) + ((GridWidth - 1) * Spacing);
        float totalGridHeight = (GridHeight * CellSize) + ((GridHeight - 1) * Spacing);

        // Find out how much available space exists inside the container (minus defined padding)
        float availableWidth = ItemContainer.rect.width - gridLayout.padding.left - gridLayout.padding.right;
        float availableHeight = ItemContainer.rect.height - gridLayout.padding.top - gridLayout.padding.bottom;

        // Calculate the leftover blank space
        float blankSpaceX = availableWidth - totalGridWidth;
        float blankSpaceY = availableHeight - totalGridHeight;

        // Center alignment splits the blank space evenly on both sides
        float dynamicLeftMargin = gridLayout.padding.left + (blankSpaceX / 2f);
        float dynamicTopMargin = gridLayout.padding.top + (blankSpaceY / 2f);

        // Return the exact X and Y starting coordinates for cell (0,0)
        // (Y is negative because UI coordinates go down)
        return new Vector2(dynamicLeftMargin, -dynamicTopMargin);
    }
}

#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;

    private static ComponentModule module = null!;
    [SerializeField] private ScrollBuy ScrollBuy;
    private List<InventoryItemUI> inventoryItems = new();

    [Header("Grid Settings")]
    public static int GridWidth => module.Size.x;
    public static int GridHeight => module.Size.y;
    public static int NumberOfCells => module.Size.x * module.Size.y;
    public static float CellSize { get => instance.gridLayout.cellSize.x; }
    public static float Spacing { get => instance.gridLayout.spacing.x; }
    private List<GameObject> gridCells = new List<GameObject>();
    [SerializeField] private GridLayoutGroup gridLayout = null!;
    [SerializeField] private GameObject cellPrefab = null!;

    [Header("References")]
    [field: SerializeField] public RectTransform ItemContainer { get; private set; } = null!;
    [field: SerializeField] public Transform DragCanvas { get; private set; } = null!;
    [field: SerializeField] public Canvas MainCanvas { get; private set; } = null!;
    [field: SerializeField] public RectTransform SellArea { get; private set; } = null!;
    [field: SerializeField] public Material BorderMaterial { get; private set; } = null!;

    // pool
    [SerializeField] private GameObject inventoryItemPrefab;
    private static ObjectPool<IPoolable> itemPool = null!;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        // Initialize Background Grid
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 11;
        for (int i = 0; i < 55; i++)
        {
            GameObject go = Instantiate(cellPrefab, gridLayout.transform);
            go.SetActive(false);
            gridCells.Add(go);
        }

        // Initialize pool
        ObjectPool<IPoolable> newPool = null!;

        newPool = new ObjectPool<IPoolable>
        (
            createFunc: () =>
            {
                GameObject obj = Instantiate(inventoryItemPrefab);
                obj.transform.SetParent(ItemContainer, false);
                obj.SetActive(false);

                if (!obj.TryGetComponent<IPoolable>(out IPoolable poolable))
                    throw new MissingComponentException("Spawner: IPoolable component is missing from InventoryItem.");

                poolable.Pool = newPool;

                return poolable;
            },

            actionOnGet: (obj) =>
            {
                obj.Object.SetActive(true);
            },

            actionOnRelease: (obj) =>
            {
                obj.Object.SetActive(false);
            },

            actionOnDestroy: (obj) => Destroy(obj.Object),

            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );

        itemPool = newPool;
    }

    private void Update()
    {
        // InventoryItemUI shader
        BorderMaterial.SetFloat("_UnscaledTime", Time.unscaledTime);
    }

    public static Transform GetDragCanvas() => instance.DragCanvas;
    public static Canvas GetMainCanvas() => instance.MainCanvas;
    public static RectTransform GetSellArea() => instance.SellArea;

    // Moving Items

    public static void HandleItemDrop(InventoryItemUI item)
    {
        // Get the item's Top-Left corner in the ItemContainer's local space
        Vector3 localItemPos = instance.ItemContainer.InverseTransformPoint(item.transform.position);
        Vector2Int[] localShape = item.Shape;
        var localImage = item.ImageUI;

       // Get the dynamic starting point of the grid
       Vector2 gridOrigin = instance.GetGridOrigin();

        // Adjust for GridLayoutGroup padding
        float adjustedX = localItemPos.x - gridOrigin.x;
        float adjustedY = gridOrigin.y - localItemPos.y;

        // Convert local position to Grid X and Y
        float totalCellSize = CellSize + Spacing;
        int gridX = Mathf.RoundToInt(adjustedX / totalCellSize);
        int gridY = Mathf.RoundToInt(adjustedY / totalCellSize);

        //Debug.Log($"gX: {gridX}; gY: {gridY}");

        // Check if the drop is valid
        if (CanPlaceItem(item, gridX, gridY))
        {
            // Valid drop: Place it in the logical grid and snap visually
            PlaceItem(item, gridX, gridY);
            Debug.Log("placable");

            AudioManager.PlaySFX(Clip.Place);
        }
        else
        {
            // Invalid drop: Put it back where it came from
            item.ImageUI = localImage;
            item.Shape = localShape;
            PlaceItem(item, item.originalGridPos.x, item.originalGridPos.y);
            Debug.Log("unplacable");

            AudioManager.PlaySFX(Clip.Warning);
        }
    }

    public static bool AddComponent(TowerComponent component)
    {
        bool result = module.AddComponent(component);
        ResetComponentModule(module);
        
        return result;
    }

    public static void HandleRotation /* Clockwise */ (InventoryItemUI item)
    {
        var maxX = item.Component.Shape.Max(s => s.x);
        item.Component.Shape = item.Component.Shape.Select(coord => new Vector2Int(coord.y, -coord.x + maxX)).ToArray();
        foreach (var coord in item.Component.Shape)
        {
            Debug.Log($"X: {coord.x} Y: {coord.y}");
        }
        Vector3 currentRotation = item.ImageUI.rectTransform.localEulerAngles;

        item.ImageUI.rectTransform.localEulerAngles = new Vector3(
            currentRotation.x,
            currentRotation.y,
            currentRotation.z - 90f
        );
    }

    private static bool CanPlaceItem(InventoryItemUI item, int startX, int startY)
    {
        return module.Placeable(item.Component, startX, startY);
    }

    public static void PlaceItem(InventoryItemUI item, int x, int y)
    {
        module.AddComponent(item.Component, x, y);

        // Snap visual position
        item.transform.SetParent(instance.ItemContainer, false);
        item.rectTransform.anchoredPosition = instance.GetSnappedPosition(x, y);
    }

    public static bool ClearItemSpace(InventoryItemUI item)
    {
        return module.RemoveComponent(item.Component);
    }

    public static void ResetComponentModule(ComponentModule module)
    {
        InventoryManager.module = module;

        // Refresh Background Grid
        instance.gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        instance.gridLayout.constraintCount = GridWidth;

        while (instance.gridCells.Count < Mathf.Min(NumberOfCells, 250))
        {
            GameObject go = Instantiate(instance.cellPrefab, instance.gridLayout.transform);
            go.SetActive(false);
            instance.gridCells.Add(go);
        }

        for (int i = 0; i < instance.gridCells.Count; i++)
        {
            instance.gridCells[i].SetActive(i < NumberOfCells);
        }

        // Initialize UI Components
        foreach (InventoryItemUI item in instance.inventoryItems)
        {
            item.Return2Pool();
        }

        instance.inventoryItems.Clear();

        foreach (TowerComponent component in module.Components)
        {
            IPoolable poolable = itemPool.Get();
            InitializePoolable(poolable, component);
        }
    }

    private static void InitializePoolable(IPoolable poolable, TowerComponent component)
    {
        if (poolable.Object.TryGetComponent<InventoryItemUI>(out InventoryItemUI item))
        {
            item.transform.SetParent(instance.ItemContainer, false);

            item.SetComponent(component);
            poolable.SpawnAction(instance.GetSnappedPosition(component.position));

            instance.inventoryItems.Add(item);
        }
        else
        {
            throw new MissingComponentException("InventoryManager: InventoryItemUI component is missing from pooled object.");
        }
    }

    public static void ReleaseInventoryItem(InventoryItemUI item)
    {
        instance.inventoryItems.Remove(item);
        item.Return2Pool();
    }

    public static void RefreshBar(List<ComponentType> allowedTypes)
    {
        instance.ScrollBuy.Refresh(allowedTypes);
    }

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

    private Vector2 GetSnappedPosition(Vector2Int at) => GetSnappedPosition(at.x, at.y);

    private Vector2 GetSnappedPosition(int x, int y)
    {
        float totalCellSize = CellSize + Spacing;
        Vector2 gridOrigin = GetGridOrigin();

        return new Vector2(
            gridOrigin.x + x * totalCellSize,
            gridOrigin.y - y * totalCellSize // negative because Y goes down in UI
        );
    }
}

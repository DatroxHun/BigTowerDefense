#nullable enable

using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter, IPoolable, IPointerClickHandler
{
    [FormerlySerializedAs("image")]
    public Image ImageUI = null!;
    public TowerComponent Component { get; private set; } = null!;
    public Vector2Int[] Shape { get => Component?.Shape ?? System.Array.Empty<Vector2Int>(); set => Component.Shape = value; }

    // Note: Ensure your Component.Size dynamically updates based on the current Shape!
    public int ItemWidth => (Shape != null && Shape.Length > 0) ? Shape.Max(c => c.x) + 1 : 0;
    public int ItemHeight => (Shape != null && Shape.Length > 0) ? Shape.Max(c => c.y) + 1 : 0;

    [HideInInspector] public GameObject Object => gameObject;
    [HideInInspector] public IObjectPool<IPoolable> Pool { get; set; } = null!;

    [HideInInspector] public Vector2Int CurrentGridPos => Component.position;
    [HideInInspector] public Vector2Int originalGridPos;
    [HideInInspector] public RectTransform rectTransform = null!;

    private CanvasGroup canvasGroup = null!;
    private RectTransform sellArea = null!;
    private TextMeshProUGUI sellText = null!;

    private float animMutliplier = 1f;
    private Coroutine? animationRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SpawnAction(Vector3 position)
    {
        rectTransform.anchoredPosition = (Vector2)position;
        ImageUI.color = Color.white;
        originalGridPos = CurrentGridPos;
    }

    public void SetComponent(TowerComponent component)
    {
        this.Component = component;
        ImageUI.sprite = component.Image;

        ImageUI.rectTransform.localRotation = Quaternion.identity;

        float baseWidth = ItemWidth * InventoryManager.CellSize + (ItemWidth - 1) * InventoryManager.Spacing;
        float baseHeight = ItemHeight * InventoryManager.CellSize + (ItemHeight - 1) * InventoryManager.Spacing;
        ImageUI.rectTransform.sizeDelta = new Vector2(baseWidth, baseHeight);

        RefreshBounds();
    }


    public void ApplyVisualRotation(float angle)
    {
        ImageUI.rectTransform.Rotate(0, 0, angle);
        Component.TimesRotated++;
        RefreshBounds();
    }

    private void RefreshBounds()
    {
        float totalWidth = ItemWidth * InventoryManager.CellSize + (ItemWidth - 1) * InventoryManager.Spacing;
        float totalHeight = ItemHeight * InventoryManager.CellSize + (ItemHeight - 1) * InventoryManager.Spacing;

        // Resize the un-rotated logical Root container
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

        ImageUI.rectTransform.anchoredPosition = Vector2.zero;
    }

    // --------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalGridPos = CurrentGridPos;
        InventoryManager.ClearItemSpace(this);
        transform.SetParent(InventoryManager.GetDragCanvas(), false);

        canvasGroup.blocksRaycasts = false;
        sellArea = InventoryManager.GetSellArea();
        sellText = sellArea.GetComponentInChildren<TextMeshProUGUI>();

        animationRoutine = StartCoroutine(Animation());
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / InventoryManager.GetMainCanvas().scaleFactor;

        bool isMouseInSellArea = RectTransformUtility.RectangleContainsScreenPoint(
            sellArea, eventData.position, eventData.pressEventCamera);

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
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        animMutliplier = 1f;
        sellText.text = "€";

        bool isMouseInSellArea = RectTransformUtility.RectangleContainsScreenPoint(
            sellArea, eventData.position, eventData.pressEventCamera);

        if (isMouseInSellArea)
        {
            BuildingManager.instance.SellForResources(Component.Price);
            InventoryManager.ReleaseInventoryItem(this);
            InventoryManager.ClearItemSpace(this);
        }
        else
        {
            InventoryManager.HandleItemDrop(this);
        }

        if (animationRoutine != null) StopCoroutine(animationRoutine);
    }

    public bool IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPos, eventCamera, out Vector2 localMousePos);

        float totalCellSize = InventoryManager.CellSize + InventoryManager.Spacing;

        int clickX = Mathf.FloorToInt(localMousePos.x / totalCellSize);
        int clickY = Mathf.FloorToInt(Mathf.Abs(localMousePos.y) / totalCellSize);

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
        float Map(float v, float imin, float imax, float omin, float omax) =>
            omin + (v - imin) / (imax - imin) * (omax - omin);

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventoryManager.HandleRotation(this);
        }
    }
}
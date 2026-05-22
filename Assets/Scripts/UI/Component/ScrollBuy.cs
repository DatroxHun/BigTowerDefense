using UnityEngine;
using UnityEngine.UI;

public class ScrollBuy : MonoBehaviour
{
    [SerializeField] private Transform verticalLayout;
    [SerializeField] private GameObject shopItemPrefab;

    private static GameObject shopItem = null!;

    private void Awake()
    {
        shopItem = new GameObject
        (
            "ShopItem",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
    }

    void Start()
    {
        foreach ((string name, TowerComponent component) in ComponentLibrary.SampleAll())
        {
            Debug.Log($"name: {name}");
            CreateNakedImageItem(component.Image);
        }
    }

    public void CreateNakedImageItem(Sprite itemSprite)
    {
        // Create a blank GameObject
        GameObject newObj = Instantiate(shopItem);

        // Set the parent to your UI container
        newObj.transform.SetParent(verticalLayout, false);

        // Configure the RectTransform
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = Vector2.zero;      // Position at top-left of container
        rectTransform.sizeDelta = new Vector2(320, 175);    // Set your desired pixel size

        // Configure the Image component
        Image image = newObj.GetComponent<Image>();
        image.sprite = itemSprite;
        image.preserveAspect = true;

        // Configure the Button
        Button button = newObj.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint; // Enables default UI color transitions

        button.onClick.AddListener(() => Debug.Log("Clicked shop item..."));
    }
}

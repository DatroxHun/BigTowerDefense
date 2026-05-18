using UnityEngine;

public class TowerSettings : MonoBehaviour
{
    private static TowerSettings instance;

    [SerializeField] private CanvasGroup canvasGroup;

    private bool isOpen = false;

    private const float animTime = .35f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void ToggleVisibility() => SetVisibility(!isOpen);

    public static void SetVisibility(bool isOpen)
    {
        instance.isOpen = isOpen;

        LeanTween.alphaCanvas(instance.canvasGroup, isOpen ? 1f : 0f, animTime)
            .setEase(LeanTweenType.easeInOutSine)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                instance.canvasGroup.interactable = isOpen;
                instance.canvasGroup.blocksRaycasts = isOpen;
            });
    }

    public static void SetInventory(InventoryItemUI[,] inventory) => InventoryManager.SetInventoryGrid(inventory);
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shop : MonoBehaviour
{
    private bool openShop = false;
    private bool animatingShop = false;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Button Press Callbacks
    public void CloseButtonPressed()
    {
        ToggleShop();
    }

    public void ToggleShop()
    {
        SetShopVisiblity(!openShop);
    }

    public void SetShopVisiblity(bool visible)
    {      
        if (!animatingShop && visible != openShop)
        {
            animatingShop = true;
            openShop = visible;

            float targetX = openShop ? -rectTransform.rect.width : 0f;

            LeanTween.value(gameObject, rectTransform.anchoredPosition.x, targetX, 0.5f)
                .setOnUpdate((float x) =>
                {
                    rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
                })
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() => animatingShop = false)
                .setIgnoreTimeScale(true);
        }
    }

    public void BuyButtonPressed(int id)
    {
        Debug.Log($"buy button id: {id}");

        SetShopVisiblity(false);
    }
}

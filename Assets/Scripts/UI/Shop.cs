using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shop : MonoBehaviour
{
    private bool openShop = false;
    private bool animatingShop = false;

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

            LeanTween.moveLocalX(gameObject, 1920 / 2 + 400 * (openShop ? 0 : 1), .5f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() => animatingShop = false);
        }
    }

    public void BuyButtonPressed(int id)
    {
        Debug.Log($"buy button id: {id}");

        SetShopVisiblity(false);
    }
}

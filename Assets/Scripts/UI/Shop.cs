using UnityEngine;
using UnityEngine.SceneManagement;

public class Shop : MonoBehaviour
{
    private bool openShop = false;
    private bool animatingShop = false;

    // Button Press Callbacks
    public void CloseButtonPressed()
    {
        if (!animatingShop)
        {
            animatingShop = true;
            openShop = !openShop;

            LeanTween.moveLocalX(gameObject, 1920 / 2 + 400 * (openShop ? 0 : 1), .5f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() => animatingShop = false);
        }
    }

    public void BuyButtonPressed(int id)
    {
        Debug.Log($"buy button id: {id}");
    }
}

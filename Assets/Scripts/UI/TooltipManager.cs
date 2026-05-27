using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TooltipManager : MonoBehaviour
{
    private static TooltipManager instance;

    [SerializeField] private RectTransform tooltipWindow;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform parentCanvasRect;
    [SerializeField] private Camera currentCamera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            canvasGroup.alpha = 0f;
            tooltipWindow.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (tooltipWindow.gameObject.activeSelf && Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvasRect,
                mousePosition,
                currentCamera,
                out Vector2 localPoint);

            tooltipWindow.localPosition = localPoint;
        }
    }

    public static void ShowTooltip(string message)
    {
        instance.tooltipText.text = message;
        instance.tooltipWindow.gameObject.SetActive(true);

        Animate(true);
    }

    public static void HideTooltip()
    {
        Animate(false, () => instance.tooltipWindow.gameObject.SetActive(false));
    }

    private static void Animate(bool visible, Action callback = null)
    {
        LeanTween.cancel(instance.canvasGroup.gameObject);

        LeanTween.alphaCanvas(instance.canvasGroup, visible ? 1f : 0f, 0.2f)
                 .setEaseInOutSine()
                 .setIgnoreTimeScale(true)
                 .setOnComplete(() =>
                 {
                     callback?.Invoke();
                 });
    }
}

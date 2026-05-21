#nullable enable

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    private static List<Interaction> interactions = new();

    [SerializeField] private float aboveY = 95f;
    [SerializeField] private float belowY = -95f;

    [SerializeField] private CanvasGroup canvasGroup = null!;
    List<Transform> buttons = new();

    [Header("Tower")]
    [SerializeField] private Tower? tower;

    [SerializeField] private Image? visibilityImage;
    [SerializeField] private Sprite? visibleSprite;
    [SerializeField] private Sprite? invisibleSprite;

    [Header("Obstacle")]
    [SerializeField] private Obstacle? obstacle;

    public bool Visible { get; private set; } = false;

    private const float animTime = 0.35f;
    private RectTransform rectTransform = null!;

    private void Awake()
    {
        if (!interactions.Contains(this))
            interactions.Add(this);
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        foreach (Transform child in canvasGroup.transform)
        {
            buttons.Add(child);
        }
    }

    public void PressedButton(int idx)
    {
        if (idx == 0 && visibilityImage != null && tower != null && 
            invisibleSprite != null && visibleSprite != null) // tower hiding
        {
            tower.ToggleHide(() =>
            {
                // swap image
                visibilityImage.sprite = tower.Hiding ? invisibleSprite : visibleSprite;
            });            
        }
        else if (idx == 1 && tower != null) // tower settings
        {
            tower.LoadInventory();
            TowerSettings.SetVisibility(true);
        }
        else if (idx == 2 && obstacle != null) // obstacle removal
        {
            obstacle.OnRemove();
        }
    }

    public static void CloseAll(Interaction? except = null)
    {
        foreach (Interaction interaction in interactions)
        {
            if (interaction != except && interaction != null)
                interaction.SetVisibility(false);
        }
    }

    public void SetVisibility(bool visible)
    {
        if (!this.Visible) FitOnScreen();

        if (this.Visible == visible) return;
        this.Visible = visible;

        if (visible)
            CloseAll(except: this);

        Animate(visible);
    }

    private void Animate(bool visible)
    {
        LeanTween.scale(gameObject, visible ? Vector3.one : Vector3.one * .25f, animTime)
            .setEase(visible ? LeanTweenType.easeOutBack : LeanTweenType.easeOutExpo);

        foreach (Transform t in buttons)
        {
            LeanTween.scale(t.gameObject, visible ? Vector3.one : Vector3.one * .5f, animTime)
                .setEase(visible ? LeanTweenType.easeOutBack : LeanTweenType.easeOutExpo);
        }

        LeanTween.alphaCanvas(canvasGroup, visible ? 1f : 0f, animTime)
            .setEase(visible ? LeanTweenType.easeInSine : LeanTweenType.easeOutExpo)
            .setOnComplete(() =>
        {
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        });
    }

    private void FitOnScreen()
    {
        const float bottomMargin = .15f;

        Vector3? parentViewportPos = null;
        
        if (tower != null)
            parentViewportPos = Camera.main.WorldToViewportPoint(tower.transform.position);
        else if (obstacle != null)
            parentViewportPos = Camera.main.WorldToViewportPoint(obstacle.transform.position);

        if (parentViewportPos != null && rectTransform != null)
        {
            if (parentViewportPos.Value.y < bottomMargin)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, aboveY);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, belowY);
            }
        }
    }
}

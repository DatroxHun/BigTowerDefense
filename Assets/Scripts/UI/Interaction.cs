using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    private static List<Interaction> interactions = new();

    [SerializeField] private Tower tower;

    [SerializeField] private Image visibilityImage;
    [SerializeField] private Sprite visibleSprite;
    [SerializeField] private Sprite invisibleSprite;

    [SerializeField] private CanvasGroup canvasGroup;
    List<Transform> buttons = new();

    public bool Visible { get; private set; } = false;

    private const float animTime = 0.35f;

    private void Awake()
    {
        if (!interactions.Contains(this))
            interactions.Add(this);
    }

    private void Start()
    {
        foreach (Transform child in canvasGroup.transform)
        {
            buttons.Add(child);
        }
    }

    public void PressedButton(int idx)
    {
        if (idx == 0) // visibility
        {
            tower.ToggleHide(() =>
            {
                // swap image
                visibilityImage.sprite = tower.Hiding ? invisibleSprite : visibleSprite;
            });            
        }
        else if (idx == 1) // settings
        {

        }
    }

    public static void CloseAll(Interaction except = null)
    {
        foreach (Interaction interaction in interactions)
        {
            if (interaction != except)
                interaction.SetVisibility(false);
        }
    }

    public void SetVisibility(bool visible)
    {
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
}

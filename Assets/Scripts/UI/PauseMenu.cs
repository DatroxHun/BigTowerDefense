using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float animTime = .1f;

    public static bool IsPaused { get => Time.timeScale < 1e-2f; }
    private static PauseMenu instance;



    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();
        }
    }

    public static void TogglePauseGame()
    {
        SetPauseGame(!IsPaused);
    }

    public static void SetPauseGame(bool isPaused)
    {
        if (isPaused) Time.timeScale = 0f;
        else          Time.timeScale = 1f;
    }

    public void TogglePauseMenu() => SetPauseMenu(!IsPaused);

    public void SetPauseMenu(bool isPaused)
    {
        SetPauseGame(isPaused);

        LeanTween.alphaCanvas(canvasGroup, isPaused ? 1f : 0f, animTime)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                canvasGroup.blocksRaycasts = isPaused;
                canvasGroup.interactable = isPaused;
            })
            .setIgnoreTimeScale(true);
    }
}

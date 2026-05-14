using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float animTime = .1f;

    [SerializeField] private TextMeshProUGUI masterVolumeValueTXT;
    [SerializeField] private TextMeshProUGUI musicVolumeValueTXT;
    [SerializeField] private TextMeshProUGUI sfxVolumeValueTXT;

    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedSliderValueTXT;

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
        Time.timeScale = isPaused ? 0f : GetTimeScaleFromSliderValue(instance.speedSlider.value);
        AudioManager.SetBGMPitch(isPaused ? 0.9f : 1f);
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

    public void MasterVolumeSliderValueChanged(float value)
    {
        masterVolumeValueTXT.text = $"{Mathf.Round(100f * value)}%";
        AudioManager.SetMasterVolume(value);
    }

    public void MusicVolumeSliderValueChanged(float value)
    {
        musicVolumeValueTXT.text = $"{Mathf.Round(100f * value)}%";
        AudioManager.SetMusicVolume(value);
    }

    public void SFXVolumeSliderValueChanged(float value)
    {
        sfxVolumeValueTXT.text = $"{Mathf.Round(100f * value)}%";
        AudioManager.SetSFXVolume(value);
    }

    // ---

    private static float GetTimeScaleFromSliderValue(float value) => Mathf.Pow(2, value);

    public void SpeedSliderValueChanged(float value)
    {
        float speed = GetTimeScaleFromSliderValue(value);
        speedSliderValueTXT.text = $"{Mathf.Round(10f * speed) / 10f}×";
        Time.timeScale = speed;
    }
}

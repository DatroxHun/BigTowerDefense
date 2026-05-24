#nullable enable

using TMPro;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu? Instance { get; private set; } = null;

    [SerializeField] private Image TransitionPanel = null!;
    [SerializeField] private RectTransform ButtonContainer = null!;
    [SerializeField] private Image TransitionInImage = null!;

    [SerializeField] private TextMeshProUGUI masterVolumeValueTXT = null!;
    [SerializeField] private TextMeshProUGUI musicVolumeValueTXT = null!;
    [SerializeField] private TextMeshProUGUI sfxVolumeValueTXT = null!;

    [SerializeField] private Slider masterSlider = null!;
    [SerializeField] private Slider musicSlider = null!;
    [SerializeField] private Slider sfxSlider = null!;

    private void Awake()
    {
        // Singleton stuff
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        if (Time.time > 1f)
        {
            TransitionInImage.gameObject.SetActive(true);

            LeanTween.value(TransitionInImage.gameObject, (float val) =>
            {
                TransitionInImage.transform.localScale = Vector3.one * (1f - val);
                TransitionInImage.transform.eulerAngles = new Vector3(0, 0, -val * 270f);
            }, 0f, 1f, 1f)
            .setEase(LeanTweenType.easeInOutSine).setIgnoreTimeScale(true);
        }

        // Set sliders
        if (AudioManager.GetMasterVolume(out float mv))
            masterSlider.value = mv;

        if (AudioManager.GetMusicVolume(out float muv))
            musicSlider.value = muv;

        if (AudioManager.GetSFXVolume(out float sfxv))
            sfxSlider.value = sfxv;
    }

    public void LoadScene(int sceneIdx)
    {
        // Check paramter
        if (sceneIdx == 0)
        {
            Debug.LogWarning("Why would you want to load this menu?");
        }
        else if (sceneIdx < 0)
        {
            Debug.LogError("Negative sceneIdx is not allowed!");
            return;
        }

        // Transition
        TransitionPanel.gameObject.SetActive(true);
        LeanTween.value(TransitionPanel.gameObject, (float val) =>
        {
            Color c = TransitionPanel.color;
            c.a = val;
            TransitionPanel.color = c;
        }, 0f, 1f, .5f)
        .setEase(LeanTweenType.easeInSine)
        .setOnComplete(() => SceneManager.LoadSceneAsync(sceneIdx));
    }

    // Button Press Callbacks

    public void PlayButtonPressed()
    {
        ToggleLevelSelector(0, -1);
    }

    public void OptionButtonPressed()
    {
        ToggleLevelSelector(0, 1);
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
    }

    public void BackButtonPressed(int from)
    {
        ToggleLevelSelector(from, 0);
    }

    // Sliders

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

    // Utils

    private void ToggleLevelSelector(int from, int to)
    {
        LeanTween.value(TransitionPanel.gameObject, (float val) =>
        {
            ButtonContainer.anchoredPosition = new Vector2(val, ButtonContainer.anchoredPosition.y);
        }, -3000f * from, -3000f * to, .5f)
        .setEase(LeanTweenType.easeOutSine);
    }
}

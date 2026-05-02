#nullable enable

using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu? Instance { get; private set; } = null;

    [SerializeField] private Image TransitionPanel = null!;
    [SerializeField] private RectTransform LevelSelectorHolder = null!;
    [SerializeField] private RectTransform ButtonHolder = null!;
    [SerializeField] private Image TransitionInImage = null!;

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
        TransitionInImage.gameObject.SetActive(true);

        LeanTween.value(TransitionInImage.gameObject, (float val) =>
        {
            TransitionInImage.transform.localScale = Vector3.one * (1f - val);
            TransitionInImage.transform.eulerAngles = new Vector3(0, 0, -val * 270f);
        }, 0f, 1f, 1f)
        .setEase(LeanTweenType.easeOutSine);
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
        ToggleLevelSelector(true);
    }

    public void OptionButtonPressed()
    {
        Debug.LogWarning("Not implemented!");
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
    }

    public void BackButtonPressed()
    {
        ToggleLevelSelector(false);
    }

    // Utils

    private void ToggleLevelSelector(bool visible)
    {
        LeanTween.value(TransitionPanel.gameObject, (float val) =>
        {
            LevelSelectorHolder.anchoredPosition = new Vector2(val, LevelSelectorHolder.anchoredPosition.y);
            ButtonHolder.anchoredPosition = new Vector2(val + 3000f, ButtonHolder.anchoredPosition.y);

        }, visible ? -3000f : 0f, visible ? 0f : -3000f, .5f)
        .setEase(LeanTweenType.easeOutSine);
    }
}

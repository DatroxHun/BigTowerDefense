using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }

    [SerializeField] CanvasGroup group;
    [SerializeField] RectTransform transitionImageTransform;

    private bool shown = false;
    private bool animating = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public void Toggle()
    {
        PauseMenu.SetPauseGame(true);

        if (!animating)
        {
            animating = true;
            shown = !shown;

            LeanTween.alphaCanvas(group, shown ? 1f : 0f, 2f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() =>
                {
                    group.blocksRaycasts = shown;
                    group.interactable = shown;

                    animating = false;
                })
                .setIgnoreTimeScale(true);
        }
    }

    // Button Press Callbacks
    public void MenuButtonPressed()
    {
        LeanTween.value(transitionImageTransform.gameObject, (float val) =>
        {
            transitionImageTransform.localScale = Vector3.one * val * 1.25f;
            transitionImageTransform.eulerAngles = new Vector3(0, 0, val * 270f);
        }, 0f, 1f, 1f)
        .setEase(LeanTweenType.easeInSine)
        .setOnComplete(() =>
        {
            PauseMenu.SetPauseGame(false);
            AudioManager.PlayBGM(Clip.CalmBGM);
            SceneManager.LoadSceneAsync(0);
        })
        .setIgnoreTimeScale(true);
    }
}

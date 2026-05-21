using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform transitionImageTransform;

    [SerializeField] private TextMeshProUGUI gameOverText;

    private bool shown = false;
    private bool animating = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public void Toggle(bool won)
    {
        PauseMenu.SetPauseGame(true);

        gameOverText.text = won ? "You Won!" : "Game Over!";
        gameOverText.color = won ? Color.green : Color.red;

        if (!animating)
        {
            animating = true;
            shown = true;

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

            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync(0);
        })
        .setIgnoreTimeScale(true);
    }
}

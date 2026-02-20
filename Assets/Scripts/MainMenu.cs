using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Image TransitionPanel;

    void Start()
    {
        
    }

    // Button Press Callbacks

    public void PlayButtonPressed()
    {
        // Transition
        TransitionPanel.gameObject.SetActive(true);
        LeanTween.value(TransitionPanel.gameObject, (float val) =>
        {
            Color c = TransitionPanel.color;
            c.a = val;
            TransitionPanel.color = c;
        }, 0f, 1f, .5f)
        .setEase(LeanTweenType.easeInSine)
        .setOnComplete(() => SceneManager.LoadSceneAsync(1)); // 1: TestLevel;
    }
}

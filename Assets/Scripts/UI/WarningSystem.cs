using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningSystem : MonoBehaviour
{
    private static WarningSystem instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;

    private bool isProcessing = false;
    private Queue<(string, float, float)> messages = new Queue<(string, float, float)>(4);

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            StartCoroutine(ProcessCycle());
        }
        else
            Destroy(this);
    }

    public static void DisplayWarningMessage(string msg, float duration)
    {
        instance.messages.Enqueue((msg, duration, Time.time));
    }

    private IEnumerator ProcessCycle()
    {
        while (true)
        {
            yield return new WaitWhile(() => isProcessing || messages.Count == 0);

            AudioManager.PlaySFX(Clip.Warning);

            (string msg, float duration, float time) = messages.Dequeue();
            if (Time.time - time < 5f)
            {
                isProcessing = true;
                text.text = msg;

                LeanTween.alphaCanvas(canvasGroup, 1f, .3f)
                    .setIgnoreTimeScale(true)
                    .setEaseOutQuad()
                    .setOnComplete(() =>
                    {
                        LeanTween.delayedCall(duration, () =>
                        {
                            LeanTween.alphaCanvas(canvasGroup, 0f, .5f)
                            .setIgnoreTimeScale(true)
                            .setEaseInOutSine()
                            .setOnComplete(() =>
                            {
                                isProcessing = false;
                            });
                        });
                    });
            }
        }
    }
}

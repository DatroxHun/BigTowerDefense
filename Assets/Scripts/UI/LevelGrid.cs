using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelGrid : MonoBehaviour
{
    [SerializeField] private int levelCount;
    [SerializeField] private Gradient backgroundGradient;
    [SerializeField] private GameObject levelSelector;

    void Start()
    {
        //for (int i = 1; i <= levelCount; i++)
        for (int i = 3; i <= levelCount; i++)
        {
            int localIdx = i;
            GameObject selector = Instantiate(levelSelector);

            selector.name = $"Level Selector #{i}";
            selector.transform.SetParent(transform, false);

            Button btn = selector.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => MainMenu.Instance.LoadScene(localIdx));

            Image img = selector.GetComponent<Image>();
            img.color = backgroundGradient.Evaluate((Mathf.PI * i) % 1f);

            TextMeshProUGUI text = selector.GetComponentInChildren<TextMeshProUGUI>();
            //text.text = $"{i}";
            text.text = "T";
            text.color = backgroundGradient.Evaluate((Mathf.PI * i) % 1f) * Color.grey * Color.lightGray;
        }
    }
}
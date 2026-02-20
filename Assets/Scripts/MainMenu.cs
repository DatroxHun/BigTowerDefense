using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        
    }

    // Button Press Callbacks

    public void PlayButtonPressed()
    {
        SceneManager.LoadSceneAsync(1);
    }
}

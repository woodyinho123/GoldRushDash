using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float splashDuration = 3f;  // Duration for splash screen to show

    void Start()
    {
        Invoke("LoadMainGameScene", splashDuration);  // Calls LoadMainGameScene after a delay
    }

    void LoadMainGameScene()
    {
        SceneManager.LoadScene("MainGameScene");  // Replace with the actual name of your main game scene
    }
}

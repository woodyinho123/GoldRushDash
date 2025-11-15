using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float splashDuration = 3f;  // Duration for the splash screen to show

    void Start()
    {
        Invoke("LoadMainMenu", splashDuration);  // Calls LoadMainMenu after 3 seconds
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainGameScene");  // Change this to your main scene's name
    }
}

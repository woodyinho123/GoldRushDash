using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float splashDuration = 3f;  // duration for splash screen to show

    void Start()
    {
        Invoke("LoadMainGameScene", splashDuration);  //game scene loaded after delay
    }

    // SplashScreen.cs 
    void LoadMainGameScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        // If splash is last for some reason, loop back to 0
        if (next >= SceneManager.sceneCountInBuildSettings)
            next = 0;

        SceneManager.LoadScene(next);
    }

}

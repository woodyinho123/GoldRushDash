using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float splashDuration = 3f;  // duration for splash screen to show

    void Start()
    {
        Time.timeScale = 1f;

        Invoke("LoadMainGameScene", splashDuration);  //game scene loaded after delay
    }

    // SplashScreen.cs 
    

    void LoadMainGameScene()

    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }


}

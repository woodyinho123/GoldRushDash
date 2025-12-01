using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float splashDuration = 3f;  // duration for splash screen to show

    void Start()
    {
        Invoke("LoadMainGameScene", splashDuration);  //game scene loaded after delay
    }

    void LoadMainGameScene()
    {
        SceneManager.LoadScene("MainGameScene");  
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsScreenUI : MonoBehaviour
{
    // tell this screen what scene to load next
    private const string KEY_NEXT_SCENE = "GRD_NextScene";
    private const string KEY_BACK_SCENE = "GRD_BackScene";

    [Header("Fallback (only used if PlayerPrefs keys are missing)")]
    [SerializeField] private string fallbackNextScene = "Level1_Mine";
    [SerializeField] private string fallbackBackScene = "MainMenuScene";

    private void Awake()
    {
        // keep things consistent after splash
        Time.timeScale = 1f;
    }

    public void Continue()
    {
        string next = PlayerPrefs.GetString(KEY_NEXT_SCENE, fallbackNextScene);
        SceneManager.LoadScene(next);
    }

    public void BackToMenu()
    {
        string back = PlayerPrefs.GetString(KEY_BACK_SCENE, fallbackBackScene);
        SceneManager.LoadScene(back);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuUITK : MonoBehaviour
{
    [SerializeField] private UIDocument doc;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private VisualElement _root;
    private bool _paused;

    private void Awake()
    {
        if (doc == null) doc = GetComponent<UIDocument>();
        _root = doc.rootVisualElement;

        _root.style.display = DisplayStyle.None;

        var resumeBtn = _root.Q<Button>("ResumeButton");
        var quitBtn = _root.Q<Button>("QuitButton");
        var volumeSlider = _root.Q<Slider>("VolumeSlider");

        if (resumeBtn != null) resumeBtn.clicked += Resume;
        if (quitBtn != null) quitBtn.clicked += QuitToMenu;

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.RegisterValueChangedCallback(e => AudioListener.volume = e.newValue);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_paused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        _paused = true;
        Time.timeScale = 0f;
        _root.style.display = DisplayStyle.Flex;
                 UnityEngine.Cursor.lockState = CursorLockMode.None;
                 UnityEngine.Cursor.visible = true;

    }

    private void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        _root.style.display = DisplayStyle.None;
                 UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                 UnityEngine.Cursor.visible = false;

    }

    private void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

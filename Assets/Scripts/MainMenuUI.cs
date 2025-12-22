using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string controlsSceneName = "ControlsScene";
    [SerializeField] private string firstLevelSceneName = "Level1_Mine";
    [SerializeField] private string scoreboardSceneName = "ScoreboardScene";


    [Header("Optional name input")]
    [SerializeField] private TMP_InputField nameInput;

    [Header("Menu Panel (optional)")]
    [SerializeField] private CanvasGroup menuCanvasGroup; 

    private void Awake()
    {
        // this prevents UI/timeline weirdness***
        Time.timeScale = 1f;
        //SplashScreen bug where screen stays black for 4 seconds***
        //  menu visible + clickable 
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        // load saved name into input field
        if (nameInput != null)
        {
            string saved = RunScoreManager.GetPlayerName();
            if (!string.IsNullOrWhiteSpace(saved))
                nameInput.text = saved;

            // save whenever they finish typing
            nameInput.onEndEdit.AddListener(OnNameEdited);
        }
    }

    private void OnDestroy()
    {
        if (nameInput != null)
            nameInput.onEndEdit.RemoveListener(OnNameEdited);
    }

    private void OnNameEdited(string value)
    {
        RunScoreManager.SetPlayerName(value);
    }

    // start button 
    public void StartGoldRush()
    {
        // final name
        string chosenName = (nameInput != null) ? nameInput.text : "";
        if (string.IsNullOrWhiteSpace(chosenName))
            chosenName = "Player";

        RunScoreManager.SetPlayerName(chosenName);
        RunScoreManager.ResetRun();
        MenuMusicController.Instance?.StopAndDestroy();

        // what to load next + where to return 
        PlayerPrefs.SetString("GRD_NextScene", firstLevelSceneName);
        PlayerPrefs.SetString("GRD_BackScene", SceneManager.GetActiveScene().name);

        SceneManager.LoadScene(controlsSceneName);

    }

    // scoreboard button
    public void OpenScoreboard()
    {
        SceneManager.LoadScene(scoreboardSceneName);
    }
}

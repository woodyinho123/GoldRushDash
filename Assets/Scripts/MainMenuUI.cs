using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string firstLevelSceneName = "Level1_Mine";
    [SerializeField] private string scoreboardSceneName = "ScoreboardScene";

    [Header("Optional name input")]
    [SerializeField] private TMP_InputField nameInput;

    [Header("Menu Panel (optional)")]
    [SerializeField] private CanvasGroup menuCanvasGroup; // drag your MenuPanel CanvasGroup here

    private void Awake()
    {
        // IMPORTANT: if you paused in a previous scene, this prevents UI/timeline weirdness
        Time.timeScale = 1f;

        // Force menu visible & clickable (prevents "still faded" after splash)
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        // Load saved name into input field
        if (nameInput != null)
        {
            string saved = RunScoreManager.GetPlayerName();
            if (!string.IsNullOrWhiteSpace(saved))
                nameInput.text = saved;

            // Save whenever they finish editing
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

    // Hook this to your Start button OnClick()
    public void StartGoldRush()
    {
        // Decide final name
        string chosenName = (nameInput != null) ? nameInput.text : "";
        if (string.IsNullOrWhiteSpace(chosenName))
            chosenName = "Player";

        RunScoreManager.SetPlayerName(chosenName);
        RunScoreManager.ResetRun();

        SceneManager.LoadScene(firstLevelSceneName);
    }

    // Hook this to your Scoreboard button OnClick()
    public void OpenScoreboard()
    {
        SceneManager.LoadScene(scoreboardSceneName);
    }
}

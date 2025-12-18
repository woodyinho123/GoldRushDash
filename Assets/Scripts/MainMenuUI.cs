using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string firstLevelSceneName = "Level1_Mine";
    [SerializeField] private string scoreboardSceneName = "ScoreboardScene";

    [Header("Optional name input")]
    [SerializeField] private TMP_InputField nameInput;

    public void StartGoldRush()
    {
        // Reset run total for a fresh run
        RunScoreManager.ResetRun();

        // Store player name (optional)
        string name = nameInput != null ? nameInput.text : "";
        if (string.IsNullOrWhiteSpace(name))
            name = LeaderboardService.GetNextDefaultPlayerName();

        RunScoreManager.SetPlayerName(name);

        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void OpenScoreboard()
    {
        SceneManager.LoadScene(scoreboardSceneName);
    }
}

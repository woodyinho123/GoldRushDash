using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreboardUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private void Start()
    {
        LeaderboardService.AddEntry("Test Player", 123);

        Refresh();
    }

    public void Refresh()
    {
        // clear rows
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        var entries = LeaderboardService.GetEntries();

        foreach (var e in entries)
        {
            var row = Instantiate(rowPrefab, content);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>(true);

            
            if (texts.Length >= 2)
            {
                texts[0].text = e.playerName;
                texts[1].text = e.score.ToString();
            }
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

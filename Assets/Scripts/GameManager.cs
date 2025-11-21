using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;  // UI shown when all gold is collected

    private int totalOre;
    private int collectedOre;

    private void Awake()
    {
        // Simple singleton so my player can talk to this
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // thisll count how many ore pieces are in the scene at the start
        totalOre = GameObject.FindGameObjectsWithTag("GoldOre").Length;
        collectedOre = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void OreCollected()
    {
        collectedOre++;   //ore wasnt incrementing when picked up at first, now fixed*

        if (collectedOre >= totalOre)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        // Pause the game
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // need to hook this to a button to restart
    public void RestartLevel()
    {
        Debug.Log("RestartLevel was called");  // restart button wasnt working, added this for Temp testing

        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}

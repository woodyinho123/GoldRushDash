using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;      // for Slider
using TMPro;               // for TextMeshProUGUI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI oreCounterText;      // new
    public TextMeshProUGUI gameOverText;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public Slider energyBar;

    [Header("Timer Settings")]
    public float maxTime = 120f;     // seconds
    public Slider timerBar;
    public TextMeshProUGUI timerLabel;

    private int totalOre;
    private int collectedOre;

    private float currentEnergy;
    private float currentTime;
    private bool isGameOver = false;

    private void Awake()
    {
        // Simple singleton
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
        // Ensure time is running
        Time.timeScale = 1f;

        // Count ore at start
        totalOre = GameObject.FindGameObjectsWithTag("GoldOre").Length;
        collectedOre = 0;

        UpdateOreUI();

        // Init energy & timer
        currentEnergy = maxEnergy;
        currentTime = maxTime;

        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = maxEnergy;
            energyBar.value = maxEnergy;
        }

        if (timerBar != null)
        {
            timerBar.minValue = 0f;
            timerBar.maxValue = maxTime;
            timerBar.value = maxTime;
        }

        if (timerLabel != null)
        {
            timerLabel.text = Mathf.CeilToInt(currentTime).ToString() + "s";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

    

        // --------- TIMER COUNTDOWN ----------
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        if (timerBar != null)
        {
            timerBar.value = currentTime;
        }

        if (timerLabel != null)
        {
            timerLabel.text = Mathf.CeilToInt(currentTime).ToString() + "s";
        }

        if (currentTime <= 0f)
        {
            LoseGame("You ran out of time! The mine collapsed.");
        }
    }

    public void OreCollected()
    {
        collectedOre++;
        UpdateOreUI();

        if (collectedOre >= totalOre && !isGameOver)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        string msg = $"You collected all the gold!\n({collectedOre}/{totalOre})";
        GameOver(msg);
    }

    private void UpdateOreUI()
    {
        if (oreCounterText != null)
        {
            oreCounterText.text = $"Ore: {collectedOre}/{totalOre}";
        }
    }


    private void LoseGame(string reason)
    {
        GameOver(reason);
    }

    private void GameOver(string message)
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = message;
    }

    // Hooked to your Restart button
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void SpendEnergy(float amount)
    {
        if (isGameOver) return;

        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        if (energyBar != null)
        {
            energyBar.value = currentEnergy;
        }

        if (currentEnergy <= 0f)
        {
            LoseGame("You ran out of energy! The mine collapsed.");
        }
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Music")]
    public AudioSource backgroundMusicSource;
    [Range(0f, 1f)] public float musicVolume = 0.15f;
    public float musicFadeDuration = 2f;

    [Header("UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI oreCounterText;
    public TextMeshProUGUI gameOverText;

    [Header("HEALTH SYSTEM")]
    public float maxHealth = 100f;
    public Slider healthBar;
    private float currentHealth;

    [Header("ENERGY SYSTEM")]
    public float maxEnergy = 100f;
    public Slider energyBar;
    private float currentEnergy;

    [Header("Timer settings")]
    public float maxTime = 120f;
    public Slider timerBar;
    public TextMeshProUGUI timerLabel;

    private int totalOre;
    private int collectedOre;

    private float currentTime;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Count ore at start
        totalOre = GameObject.FindGameObjectsWithTag("GoldOre").Length;
        collectedOre = 0;
        UpdateOreUI();

        // --- HEALTH ---
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // --- ENERGY ---
        currentEnergy = maxEnergy;
        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = maxEnergy;
            energyBar.value = currentEnergy;
        }

        // --- TIMER ---
        currentTime = maxTime;
        if (timerBar != null)
        {
            timerBar.minValue = 0f;
            timerBar.maxValue = maxTime;
            timerBar.value = currentTime;
        }
        if (timerLabel != null)
            timerLabel.text = Mathf.CeilToInt(currentTime).ToString() + "s";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // MUSIC
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = musicVolume;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // Timer countdown
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        if (timerBar != null)
            timerBar.value = currentTime;

        if (timerLabel != null)
            timerLabel.text = Mathf.CeilToInt(currentTime).ToString() + "s";

        if (currentTime <= 0f)
            LoseGame("You ran out of time! The mine collapsed.");
    }

    // ----------------------
    // GOLD COLLECTION
    // ----------------------
    public void OreCollected()
    {
        collectedOre++;
        UpdateOreUI();

        if (collectedOre >= totalOre && !isGameOver)
            WinGame();
    }

    private void UpdateOreUI()
    {
        if (oreCounterText != null)
            oreCounterText.text = $"Ore: {collectedOre}/{totalOre}";
    }

    private void WinGame()
    {
        string msg = $"You collected all the ore!\n({collectedOre}/{totalOre})";
        GameOver(msg);
    }

    // ----------------------
    // ENERGY (for mining only)
    // ----------------------
    public void SpendEnergy(float amount)
    {
        if (isGameOver) return;

        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        if (energyBar != null)
            energyBar.value = currentEnergy;

        // GAME OVER when energy is empty
        if (currentEnergy <= 0f)
        {
            LoseGame("You ran out of energy!");
        }
    }


    // ----------------------
    // HEALTH (for damage only)
    // ----------------------
    public void TakeDamage(float amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (currentHealth <= 0f)
            LoseGame("You were crushed by falling rocks!");
    }

    // ----------------------
    // GAME OVER
    // ----------------------
    private void LoseGame(string reason)
    {
        GameOver(reason);
    }

    private void GameOver(string message)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = message;

        if (backgroundMusicSource != null)
            StartCoroutine(FadeOutMusic());

        Time.timeScale = 0f;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = backgroundMusicSource.volume;
        float t = 0f;

        while (t < musicFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
            yield return null;
        }

        backgroundMusicSource.Stop();
        backgroundMusicSource.volume = startVolume;
    }

    // ----------------------
    // RESTART
    // ----------------------
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}

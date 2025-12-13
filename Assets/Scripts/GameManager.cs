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
    [SerializeField] private float currentHealth;

    [Header("ENERGY SYSTEM")]
    public float maxEnergy = 100f;
    public Slider energyBar;
    [SerializeField] private float currentEnergy;
    private float lastEnergyUseTime = -999f; // NEW: tracks when we last spent energy

    [Header("Timer settings")]
    public float maxTime = 120f;
    public Slider timerBar;
    public TextMeshProUGUI timerLabel;

    [Header("HUD messages / energy")]
    [SerializeField] private TMP_Text hudMessageText;      // drag TMP text here in Inspector
    [SerializeField] private float energyRechargeDelay = 3f;   // wait before regen starts
    [SerializeField] private float energyRechargeRate = 15f;   // energy/second while regening

    private int totalOre;
    private int collectedOre;

    private float currentTime;
    private bool isGameOver = false;

    
    private Coroutine rechargeDelayCoroutine;
    private Coroutine rechargeCoroutine;


    // energy / HUD helpers
    private bool isRechargingEnergy = false;
    private Coroutine hudMessageCoroutine;

    // For PlayerController: true if we have any usable energy
    private const float ENERGY_EMPTY_EPS = 0.01f;   // NEW: treat <= this as empty

    public bool HasEnergy => currentEnergy > ENERGY_EMPTY_EPS;


    // NEW: if you hit 0 energy, sprint stays locked until fully recharged
    private bool sprintLocked = false;
    public bool CanSprint => !sprintLocked && HasEnergy;  // HasEnergy just avoids weird edge cases

    [Header("Score")]
    public int score = 0;
    [SerializeField] private TMP_Text scoreText;   // drag a TMP text here


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
            timerLabel.text = Mathf.CeilToInt(currentTime) + " SECONDS UNTIL COLLAPSE!";

        // SCORE
        score = 0;
        UpdateScoreUI();

        // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Hide HUD message at start
        if (hudMessageText != null)
            hudMessageText.gameObject.SetActive(false);

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
            timerLabel.text = Mathf.CeilToInt(currentTime) + " SECONDS UNTIL COLLAPSE!";

        if (currentTime <= 0f)
            LoseGame("You ran out of time! The mine collapsed.");

    

        // DEBUG: test health damage with the H key
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }
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


    public void AddScore(int amount)
    {
        score += amount;
        if (score < 0) score = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }


    private void WinGame()
    {
        string msg = $"You collected all the ore!\n({collectedOre}/{totalOre})";
        GameOver(msg);
    }

    // ----------------------
    // ENERGY (stamina style)
    // ----------------------
    public void SpendEnergy(float amount)
    {
        if (isGameOver) return;
        if (amount <= 0f) return;

        // Mark the last time energy was used (unscaled so pausing doesn't break logic)
        lastEnergyUseTime = Time.unscaledTime;

        // Stop any recharge currently happening or waiting
        StopEnergyRechargeCoroutines();

        // Spend energy
        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        UpdateEnergyUI();

        // If we hit "empty", lock sprint until full
        if (currentEnergy <= ENERGY_EMPTY_EPS)
        {
            currentEnergy = 0f;
            sprintLocked = true;
            UpdateEnergyUI();

            ShowHudMessage("YOU RAN OUT OF ENERGY!");
        }

        // Start countdown to recharge AFTER delay (even if not empty)
        rechargeDelayCoroutine = StartCoroutine(EnergyRechargeAfterDelay());
    }

    private void StopEnergyRechargeCoroutines()
    {
        isRechargingEnergy = false;

        if (rechargeDelayCoroutine != null)
        {
            StopCoroutine(rechargeDelayCoroutine);
            rechargeDelayCoroutine = null;
        }

        if (rechargeCoroutine != null)
        {
            StopCoroutine(rechargeCoroutine);
            rechargeCoroutine = null;
        }
    }
    private IEnumerator EnergyRechargeAfterDelay()
    {
        // Wait until we've gone energyRechargeDelay seconds with NO energy use
        while (!isGameOver && (Time.unscaledTime - lastEnergyUseTime) < energyRechargeDelay)
            yield return null;

        // Only start recharging if we actually need it
        if (!isGameOver && currentEnergy < maxEnergy)
        {
            rechargeCoroutine = StartCoroutine(EnergyRechargeRoutine());
        }

        rechargeDelayCoroutine = null;
    }

    private IEnumerator EnergyRechargeRoutine()
    {
        if (isRechargingEnergy) yield break;
        isRechargingEnergy = true;

        while (!isGameOver && isRechargingEnergy && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRechargeRate * Time.unscaledDeltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            UpdateEnergyUI();
            yield return null;
        }

        // If full, unlock sprint and show message once
        if (!isGameOver && currentEnergy >= maxEnergy - 0.01f)
        {
            currentEnergy = maxEnergy;
            UpdateEnergyUI();

            if (sprintLocked)
            {
                sprintLocked = false;
                ShowHudMessage("SPRINT READY!");
            }
        }

        isRechargingEnergy = false;
        rechargeCoroutine = null;
    }


    private void UpdateEnergyUI()
    {
        if (energyBar != null)
            energyBar.value = currentEnergy;
    }

  

    private void ShowHudMessage(string message)
    {
        if (hudMessageText == null) return;

        hudMessageText.text = message;
        hudMessageText.gameObject.SetActive(true);

        if (hudMessageCoroutine != null)
            StopCoroutine(hudMessageCoroutine);

        hudMessageCoroutine = StartCoroutine(HideHudMessageAfterDelay(2f));
    }

    private IEnumerator HideHudMessageAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);


        if (hudMessageText != null)
            hudMessageText.gameObject.SetActive(false);

        hudMessageCoroutine = null;
    }

    // ----------------------
    // HEALTH
    // ----------------------
    public void TakeDamage(float amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log($"[Health] damage={amount}, currentHealth={currentHealth}, sliderValue={(healthBar != null ? healthBar.value : -1)}");

        if (currentHealth <= 0f)
            LoseGame("You were crushed by falling rocks!");
    }

    // Called by lava trigger
    public void LavaDeath()
    {
        LoseGame("You fell into lava and died!");
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

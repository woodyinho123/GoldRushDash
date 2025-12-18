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

    // INSERT after line 19:
    [Header("Fade / Level Exit")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;  // full-screen black image with CanvasGroup
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeHoldDuration = 0.15f;

    [Header("Elevator SFX")]
    public AudioSource elevatorSfxSource;
    public AudioClip elevatorSfxClip;
    [Range(0f, 1f)] public float elevatorSfxVolume = 1f;


    [Header("Collapse FX")]
    public GameObject collapseRockfallOverlay;   // assign a full-screen UI overlay (disabled by default)


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
    
    [SerializeField] private float collapseWarningSeconds = 20f;
    [SerializeField] private float collapseWarningMessageDuration = 3f;
    private bool collapseWarningShown = false;

    private const string COLLAPSE_TIMEOUT_MESSAGE = "You ran out of time! The mine collapsed.";


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


    public bool IsGameOver => isGameOver;
    public bool IsCollapsed => currentTime <= 0f;
    public float TimeRemaining => currentTime;




    // NEW: if you hit 0 energy, sprint stays locked until fully recharged
    private bool sprintLocked = false;
    public bool CanSprint => !sprintLocked && HasEnergy;  // HasEnergy just avoids weird edge cases

    [Header("Score")]
    public int score = 0;
    [SerializeField] private TMP_Text scoreText;   // drag a TMP text here

    [Header("CHECKPOINTS")]
    [SerializeField] private bool checkpointsEnabled = false;     // TURN ON in Level 3 only
    [SerializeField] private float respawnHealth = 100f;          // health after respawn (set to maxHealth in Start)
    [SerializeField] private float deathRespawnDamage = 15f;      // damage applied after respawn (rocks)
    [SerializeField] private float lavaRespawnDamage = 25f;       // damage applied after respawn (lava)

    private Vector3 _checkpointPos;
    private Quaternion _checkpointRot;
    private bool _hasCheckpoint = false;


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
        // CHECKPOINT INIT
        respawnHealth = maxHealth;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _checkpointPos = playerObj.transform.position;
            _checkpointRot = playerObj.transform.rotation;
            _hasCheckpoint = true;
        }

       


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

        collapseWarningShown = false;

        if (collapseRockfallOverlay != null)
            collapseRockfallOverlay.SetActive(false);


        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;

            // Keep it OFF until we need it
            fadeCanvasGroup.gameObject.SetActive(false);
        }




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

        // NEW:
        if (currentTime <= 0f)
            LoseGame(COLLAPSE_TIMEOUT_MESSAGE);



        if (!collapseWarningShown && currentTime <= collapseWarningSeconds && currentTime > 0f)
        {
            ShowHudMessage("THE MINE IS COLLAPSING, GET TO THE ELEVATOR!", collapseWarningMessageDuration);
            collapseWarningShown = true;
        }


        //// DEBUG: test health damage with the H key
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    TakeDamage(10f);
        //}
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



    private void ShowHudMessage(string message, float durationSeconds = 2f)
    {
        if (hudMessageText == null) return;

        hudMessageText.text = message;
        hudMessageText.gameObject.SetActive(true);

        if (hudMessageCoroutine != null)
            StopCoroutine(hudMessageCoroutine);

        hudMessageCoroutine = StartCoroutine(HideHudMessageAfterDelay(durationSeconds));
    }



    private IEnumerator HideHudMessageAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);


        if (hudMessageText != null)
            hudMessageText.gameObject.SetActive(false);

        hudMessageCoroutine = null;
    }

    public void SetCheckpoint(Transform checkpointTransform, string message = "CHECKPOINT REACHED!")
    {
        if (!checkpointsEnabled || checkpointTransform == null) return;

        _checkpointPos = checkpointTransform.position;
        _checkpointRot = checkpointTransform.rotation;
        _hasCheckpoint = true;

        ShowHudMessage(message, 2f);
    }

    public void RespawnToCheckpoint(string message = "RESPAWNING...")
    {
        if (!checkpointsEnabled || !_hasCheckpoint) return;
        if (isGameOver) return;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // Teleport player safely
        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = _checkpointPos;
            rb.rotation = _checkpointRot;
        }
        else
        {
            playerObj.transform.SetPositionAndRotation(_checkpointPos, _checkpointRot);
        }

        // Optional: small damage + brief invulnerability so you don't instantly die again
        if (lavaRespawnDamage > 0f)
            TakeDamage(lavaRespawnDamage);

    


        ShowHudMessage(message, 1.5f);
    }

    

    private void RespawnInternal(string message, float postRespawnDamage)
    {
        if (!_hasCheckpoint) return;
        if (isGameOver) return;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // Restore health first
        currentHealth = Mathf.Clamp(respawnHealth, 1f, maxHealth);
        if (healthBar != null) healthBar.value = currentHealth;

        // Teleport player
        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = _checkpointPos;
            rb.rotation = _checkpointRot;
        }
        else
        {
            playerObj.transform.SetPositionAndRotation(_checkpointPos, _checkpointRot);
        }

        // Apply penalty damage AFTER moving (without triggering death loop)
        if (postRespawnDamage > 0f)
        {
            currentHealth -= postRespawnDamage;
            currentHealth = Mathf.Clamp(currentHealth, 1f, maxHealth);
            if (healthBar != null) healthBar.value = currentHealth;
        }

        ShowHudMessage(message, 2f);
    }

    public void RespawnFromRocks()
    {
        if (!checkpointsEnabled) return;
        RespawnInternal("YOU WERE CRUSHED! (RESPAWN)", deathRespawnDamage);
    }

    public void RespawnFromLava()
    {
        if (!checkpointsEnabled) return;
        RespawnInternal("YOU FELL IN LAVA! (RESPAWN)", lavaRespawnDamage);
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
        {
            if (checkpointsEnabled)
                RespawnFromRocks();
            else
                LoseGame("You were crushed by falling rocks!");
        }

    }

    // Called by lava trigger
    public void LavaDeath()
    {
        if (checkpointsEnabled)
            RespawnFromLava();
        else
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

        // INSERT after line 502:
        if (collapseRockfallOverlay != null)
        {
            bool showCollapseFx = (message == COLLAPSE_TIMEOUT_MESSAGE);
            collapseRockfallOverlay.SetActive(showCollapseFx);

            if (showCollapseFx)
            {
                // If you're using an Animator on the overlay (recommended)
                var anim = collapseRockfallOverlay.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.updateMode = AnimatorUpdateMode.UnscaledTime; // keeps playing even when Time.timeScale = 0
                                                                       // Reset CanvasGroup alpha before playing (prevents stuck white overlay)
                    var cg = collapseRockfallOverlay.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 0f;

                    anim.Play(0, 0, 0f);
                }

                // If you're using ParticleSystems instead (optional)
                foreach (var ps in collapseRockfallOverlay.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var main = ps.main;
                    main.useUnscaledTime = true;
                    ps.Play(true);
                }
            }
        }


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

    // INSERT after line 524:
    public void ElevatorExitToNextScene()
    {
        if (isGameOver) return;
        StartCoroutine(ElevatorExitRoutine());
    }

    private IEnumerator ElevatorExitRoutine()
    {
        // Lock player input
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        var pc = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;
        if (pc == null && playerObj != null) pc = playerObj.GetComponentInParent<PlayerController>();
        if (pc != null) pc.SetInputEnabled(false);

        // Stop any leftover movement
        var rb = playerObj != null ? playerObj.GetComponent<Rigidbody>() : null;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Play elevator SFX
        if (elevatorSfxClip != null)
        {
            if (elevatorSfxSource != null)
                elevatorSfxSource.PlayOneShot(elevatorSfxClip, elevatorSfxVolume);
            else if (playerObj != null)
                AudioSource.PlayClipAtPoint(elevatorSfxClip, playerObj.transform.position, elevatorSfxVolume);
        }

        // Fade to black
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.gameObject.SetActive(true);
            yield return FadeCanvasGroup(fadeCanvasGroup, 1f, fadeOutDuration);
        }
        else
        {
            yield return new WaitForSecondsRealtime(fadeOutDuration);
        }

        if (fadeHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(fadeHoldDuration);

        // Load next scene in Build Settings
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings)
            next = 0;

        SceneManager.LoadScene(next);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
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

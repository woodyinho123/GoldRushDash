using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;      // for slider
using TMPro;               // for textmeshPro
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("music")]
    public AudioSource backgroundMusicSource;
    [Range(0f, 1f)] public float musicVolume = 0.15f;
    public float musicFadeDuration = 2f;   // fade inseconds


    [Header("UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI oreCounterText;      // new*
    public TextMeshProUGUI gameOverText;

    [Header("Energy settings")]
    public float maxEnergy = 100f;
    public Slider energyBar;

    [Header("Timer settings")]
    public float maxTime = 120f;     // seconds again
    public Slider timerBar;
    public TextMeshProUGUI timerLabel;

    private int totalOre;
    private int collectedOre;

    private float currentEnergy;
    private float currentTime;
    private bool isGameOver = false;

    private void Awake()
    {
       
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
        // ensure time is running
        Time.timeScale = 1f;

        // count ore at thestart*
        totalOre = GameObject.FindGameObjectsWithTag("GoldOre").Length;
        collectedOre = 0;

        UpdateOreUI();

        // initialise energy and the timer
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

        // start western music
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = 0.15f;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();

        }
    }

    private void Update()
    {
        if (isGameOver) return;

    

        // timer countdown
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
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = message;

        // fade out the music
        if (backgroundMusicSource != null)
        {
            Debug.Log("GameOver: starting music fade");
            StartCoroutine(FadeOutMusic());
        }

        // and finally pause gameplay
        Time.timeScale = 0f;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = backgroundMusicSource.volume;
        float t = 0f;

        while (t < musicFadeDuration)
        {
            t += Time.unscaledDeltaTime; // ignore timescale here
            float k = Mathf.Clamp01(t / musicFadeDuration);
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }

        backgroundMusicSource.Stop();
        backgroundMusicSource.volume = startVolume;
    }


    // hooking to  restart button
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

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDListener : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text oreCounterText;

    [Header("Bars")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Slider timerBar;

    [Header("Timer Label")]
    [SerializeField] private TMP_Text timerLabel;

    [Header("HUD Message")]
    [SerializeField] private TMP_Text hudMessageText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    private Coroutine hudMessageRoutine;
    private int lastTimerSeconds = int.MinValue;

    private void OnEnable()
    {
        GameEvents.ScoreChanged += OnScoreChanged;
        GameEvents.OreChanged += OnOreChanged;
        GameEvents.HealthChanged += OnHealthChanged;
        GameEvents.EnergyChanged += OnEnergyChanged;
        GameEvents.TimerChanged += OnTimerChanged;
        GameEvents.HudMessage += OnHudMessage;
        GameEvents.GameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.ScoreChanged -= OnScoreChanged;
        GameEvents.OreChanged -= OnOreChanged;
        GameEvents.HealthChanged -= OnHealthChanged;
        GameEvents.EnergyChanged -= OnEnergyChanged;
        GameEvents.TimerChanged -= OnTimerChanged;
        GameEvents.HudMessage -= OnHudMessage;
        GameEvents.GameOver -= OnGameOver;
    }

    private void Start()
    {
        // basic defaults so it looks right even before the first event arrives
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (hudMessageText != null) hudMessageText.gameObject.SetActive(false);

        // pull initial ranges from GameManager (safe + handy)
        var gm = GameManager.Instance;
        if (gm != null)
        {
            if (healthBar != null)
            {
                healthBar.minValue = 0f;
                healthBar.maxValue = gm.maxHealth;
                healthBar.value = gm.CurrentHealth;
            }

            if (energyBar != null)
            {
                energyBar.minValue = 0f;
                energyBar.maxValue = gm.maxEnergy;
                energyBar.value = gm.CurrentEnergy;
            }

            if (timerBar != null)
            {
                timerBar.minValue = 0f;
                timerBar.maxValue = gm.maxTime;
                timerBar.value = gm.TimeRemaining;
            }

            // do one manual sync so HUD looks correct instantly
            OnScoreChanged(gm.score);
            OnOreChanged(gm.CollectedOre, gm.TotalOre);
            OnTimerChanged(gm.TimeRemaining, Mathf.CeilToInt(gm.TimeRemaining));
        }
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void OnOreChanged(int collected, int total)
    {
        if (oreCounterText != null) oreCounterText.text = $"Ore: {collected}/{total}";
    }

    private void OnHealthChanged(float health)
    {
        if (healthBar != null) healthBar.value = health;
    }

    private void OnEnergyChanged(float energy)
    {
        if (energyBar != null) energyBar.value = energy;
    }

    private void OnTimerChanged(float timeRemaining, int secondsInt)
    {
        if (timerBar != null) timerBar.value = timeRemaining;

        if (timerLabel != null && secondsInt != lastTimerSeconds)
        {
            lastTimerSeconds = secondsInt;
            timerLabel.text = secondsInt + " SECONDS UNTIL COLLAPSE!";
        }
    }

    private void OnHudMessage(string message, float duration)
    {
        if (hudMessageText == null) return;

        hudMessageText.text = message;
        hudMessageText.gameObject.SetActive(true);

        if (hudMessageRoutine != null) StopCoroutine(hudMessageRoutine);
        hudMessageRoutine = StartCoroutine(HideHudMessageAfterDelay(duration));
    }

    private IEnumerator HideHudMessageAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (hudMessageText != null) hudMessageText.gameObject.SetActive(false);
        hudMessageRoutine = null;
    }

    private void OnGameOver(string message)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null) gameOverText.text = message;
    }
}
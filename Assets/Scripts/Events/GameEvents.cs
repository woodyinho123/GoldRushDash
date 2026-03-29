using System;
using UnityEngine;

/// summary
/// simple event hub for decoupling gameplay state changes from UI/FX listeners

public static class GameEvents
{
    // core HUD values
    public static event Action<int> ScoreChanged;
    public static event Action<float> HealthChanged;
    public static event Action<float> EnergyChanged;
    public static event Action<int, int> OreChanged;              // collected, total
    public static event Action<float, int> TimerChanged;          // timeRemaining, secondsInt

    // feedback + game state
    public static event Action<Vector3, Quaternion> CheckpointSet;
    public static event Action<string, float> HudMessage;         // message, duration
    public static event Action<string> GameOver;                  // reason/message

    // raise helpers 
    public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    public static void RaiseHealthChanged(float health) => HealthChanged?.Invoke(health);
    public static void RaiseEnergyChanged(float energy) => EnergyChanged?.Invoke(energy);
    public static void RaiseOreChanged(int collected, int total) => OreChanged?.Invoke(collected, total);
    public static void RaiseTimerChanged(float timeRemaining, int secondsInt) => TimerChanged?.Invoke(timeRemaining, secondsInt);

    public static void RaiseCheckpointSet(Vector3 pos, Quaternion rot) => CheckpointSet?.Invoke(pos, rot);
    public static void RaiseHudMessage(string message, float duration) => HudMessage?.Invoke(message, duration);
    public static void RaiseGameOver(string message) => GameOver?.Invoke(message);
}
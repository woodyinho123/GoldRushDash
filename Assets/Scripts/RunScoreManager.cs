using UnityEngine;

public static class RunScoreManager
{
    public const string RunScoreKey = "GRD_RunScore";
    public const string PlayerNameKey = "GRD_PlayerName";

    public static void ResetRun()
    {
        PlayerPrefs.SetInt(RunScoreKey, 0);
        PlayerPrefs.Save();
    }

    public static int GetRunScore()
    {
        return PlayerPrefs.GetInt(RunScoreKey, 0);
    }

    public static void AddPoints(int amount)
    {
        if (amount <= 0) return;
        int total = GetRunScore() + amount;
        PlayerPrefs.SetInt(RunScoreKey, total);
        PlayerPrefs.Save();
    }

    public static void SetPlayerName(string name)
    {
        PlayerPrefs.SetString(PlayerNameKey, name ?? "");
        PlayerPrefs.Save();
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, "");
    }
}

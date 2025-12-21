using UnityEngine;

public static class RunScoreManager
{
    private const string ScoreKey = "RunScore";
    private const string NameKey = "PlayerName";

    public static void ResetRun()
    {
        PlayerPrefs.SetInt(ScoreKey, 0);
        PlayerPrefs.Save();
    }

    public static int GetRunScore()
    {
        return PlayerPrefs.GetInt(ScoreKey, 0);
    }

    public static void AddPoints(int amount)
    {
        int newScore = Mathf.Max(0, GetRunScore() + amount);
        PlayerPrefs.SetInt(ScoreKey, newScore);
        PlayerPrefs.Save();
    }

    public static void SetPlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        PlayerPrefs.SetString(NameKey, playerName.Trim());
        PlayerPrefs.Save();
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(NameKey, "");
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public static class LeaderboardService
{
    private const string Key = "GRD_LeaderboardJson";
    private const int KeepTop = 10;

    [Serializable]
    public class Entry
    {
        public string playerName;
        public int score;
    }

    [Serializable]
    private class Wrapper
    {
        public List<Entry> entries = new List<Entry>();
    }

    public static List<Entry> GetEntries()
    {
        var w = Load();
        // highest first
        w.entries.Sort((a, b) => b.score.CompareTo(a.score));
        return w.entries;
    }

    public static string GetNextDefaultPlayerName()
    {
        // player 1 player 2 etc 
        int next = GetEntries().Count + 1;
        return $"Player {next}";
    }

    public static void AddEntry(string name, int score)
    {
        if (score <= 0) return;

        var w = Load();

        w.entries.Add(new Entry
        {
            playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name,
            score = score
        });

        w.entries.Sort((a, b) => b.score.CompareTo(a.score));

        if (w.entries.Count > KeepTop)
            w.entries.RemoveRange(KeepTop, w.entries.Count - KeepTop);

        Save(w);
    }

    private static Wrapper Load()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrWhiteSpace(json))
            return new Wrapper();

        try { return JsonUtility.FromJson<Wrapper>(json) ?? new Wrapper(); }
        catch { return new Wrapper(); }
    }

    private static void Save(Wrapper w)
    {
        string json = JsonUtility.ToJson(w);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }
}

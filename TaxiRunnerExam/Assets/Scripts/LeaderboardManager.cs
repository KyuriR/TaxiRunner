using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
// LeaderboardManager.cs  (updated)
//
// HOW TO SET UP
// ─────────────────────────────────────────────────────────────────────────────
// 1. Create a LeaderboardRow prefab (see LeaderboardRow.cs for instructions).
//    Duplicate it 10 times inside a Vertical Layout Group in your canvas.
//
// 2. Assign all 10 row GameObjects to the rows[] array here.
//    You no longer need separate nameTexts[], timeTexts[] etc. arrays —
//    delete those from your scene if they exist.
//
// 3. Set your row colours in the Inspector:
//      evenRowColour  — e.g. a light grey  (0.9, 0.9, 0.9, 1)
//      oddRowColour   — e.g. a slightly darker grey (0.8, 0.8, 0.8, 1)
//      topEntryColour — e.g. a gold tint   (1.0, 0.85, 0.2, 1)
// ─────────────────────────────────────────────────────────────────────────────

public class LeaderboardManager : MonoBehaviour
{
    [Header("Row References")]
    [Tooltip("Assign all 10 LeaderboardRow GameObjects here in order.")]
    public LeaderboardRow[] rows;

    [Header("Row Colours")]
    public Color evenRowColour = new Color(0.92f, 0.92f, 0.92f, 1f);
    public Color oddRowColour = new Color(0.80f, 0.80f, 0.80f, 1f);
    public Color topEntryColour = new Color(1.00f, 0.85f, 0.20f, 1f);

    private const string LeaderboardKey = "TaxiLeaderboard";
    private const int MaxEntries = 10;

    void Start()
    {
        DisplayLeaderboard();
    }

    public void DisplayLeaderboard()
    {
        List<LeaderboardEntry> entries = LoadLeaderboard();
        Debug.Log("Leaderboard entry count: " + entries.Count);

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;

            Color bg = (i == 0)
                ? topEntryColour
                : (i % 2 == 0 ? evenRowColour : oddRowColour);

            if (i < entries.Count)
                rows[i].Populate(i + 1, entries[i], bg, i == 0);
            else
                rows[i].Clear(i % 2 == 0 ? evenRowColour : oddRowColour);
        }
    }

    // ── Static methods called from GameManager ────────────────────────────────

    public static void AddEntry(string playerName, int timeSeconds,
                                int cash, int passengers, string areaName)
    {
        List<LeaderboardEntry> entries = LoadLeaderboard();

        entries.Add(new LeaderboardEntry(playerName, timeSeconds,
                                         cash, passengers, areaName));

        entries.Sort((a, b) =>
        {
            int cashCompare = b.cash.CompareTo(a.cash);
            if (cashCompare != 0) return cashCompare;

            int passengerCompare = b.passengers.CompareTo(a.passengers);
            if (passengerCompare != 0) return passengerCompare;

            return b.timeSeconds.CompareTo(a.timeSeconds);
        });

        if (entries.Count > MaxEntries)
            entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);

        SaveLeaderboard(entries);
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    static void SaveLeaderboard(List<LeaderboardEntry> entries)
    {
        LeaderboardEntryList wrapper = new LeaderboardEntryList();
        wrapper.entries = entries;
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }

    static List<LeaderboardEntry> LoadLeaderboard()
    {
        if (!PlayerPrefs.HasKey(LeaderboardKey))
            return new List<LeaderboardEntry>();

        string json = PlayerPrefs.GetString(LeaderboardKey);
        LeaderboardEntryList wrapper = JsonUtility.FromJson<LeaderboardEntryList>(json);

        if (wrapper == null || wrapper.entries == null)
            return new List<LeaderboardEntry>();

        return wrapper.entries;
    }

    public static void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey(LeaderboardKey);
        PlayerPrefs.Save();
    }
}
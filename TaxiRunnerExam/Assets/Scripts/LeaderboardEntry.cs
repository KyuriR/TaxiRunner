using System;

// ─────────────────────────────────────────────────────────────────────────────
// LeaderboardEntry.cs  (updated)
// Added: areaName field so each score shows which area it was earned in.
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int timeSeconds;
    public int cash;
    public int passengers;
    public string areaName;

    public LeaderboardEntry(string playerName, int timeSeconds, int cash,
                            int passengers, string areaName)
    {
        this.playerName = playerName;
        this.timeSeconds = timeSeconds;
        this.cash = cash;
        this.passengers = passengers;
        this.areaName = areaName;
    }
}
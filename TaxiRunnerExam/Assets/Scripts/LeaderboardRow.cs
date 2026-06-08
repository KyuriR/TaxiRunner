using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
// LeaderboardRow.cs
//
// Attach this to each row GameObject in your leaderboard.
// Each row holds its own text references so LeaderboardManager only needs
// to assign 10 row GameObjects instead of 50 individual text fields.
//
// HOW TO SET UP
// ─────────────────────────────────────────────────────────────────────────────
// 1. Create a row GameObject in your leaderboard layout with these children:
//      RankText       — TextMeshProUGUI  (shows "1", "2", etc. or "👑" for rank 1)
//      NameText       — TextMeshProUGUI
//      AreaText       — TextMeshProUGUI
//      TimeText       — TextMeshProUGUI
//      CashText       — TextMeshProUGUI
//      PassengerText  — TextMeshProUGUI
//      Background     — Image component on the row root (for alternating colours)
//
// 2. Attach this script to the row root GameObject.
//    Wire up all fields in the Inspector.
//
// 3. Make it a prefab, then duplicate it 10 times in your leaderboard layout.
//    Assign all 10 to LeaderboardManager.rows[].
// ─────────────────────────────────────────────────────────────────────────────

public class LeaderboardRow : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI areaText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI cashText;
    public TextMeshProUGUI passengerText;

    [Header("Row Background")]
    public Image rowBackground;

    // Fills this row with entry data
    public void Populate(int rank, LeaderboardEntry entry,
                         Color bgColour, bool isTopEntry)
    {
        // Rank — crown emoji for first place
        if (rankText != null)
            rankText.text = isTopEntry ? "👑" : rank.ToString();

        if (nameText != null)
            nameText.text = entry.playerName;

        if (areaText != null)
            areaText.text = string.IsNullOrEmpty(entry.areaName) ? "-" : entry.areaName;

        if (timeText != null)
            timeText.text = FormatTime(entry.timeSeconds);

        if (cashText != null)
            cashText.text = "R" + entry.cash;

        if (passengerText != null)
            passengerText.text = entry.passengers.ToString();

        if (rowBackground != null)
            rowBackground.color = bgColour;
    }

    // Clears the row when there is no entry for this slot
    public void Clear(Color bgColour)
    {
        if (rankText != null) rankText.text = "-";
        if (nameText != null) nameText.text = "-";
        if (areaText != null) areaText.text = "-";
        if (timeText != null) timeText.text = "-";
        if (cashText != null) cashText.text = "-";
        if (passengerText != null) passengerText.text = "-";

        if (rowBackground != null)
            rowBackground.color = bgColour;
    }

    string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
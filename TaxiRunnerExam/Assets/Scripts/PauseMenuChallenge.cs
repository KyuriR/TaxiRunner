using UnityEngine;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
// PauseMenuChallenge.cs
//
// Attach to your pause panel. Shows the daily challenge description and
// current status (IN PROGRESS / COMPLETE! / CLAIMED) when the panel opens.
//
// HOW TO SET UP
// ─────────────────────────────────────────────────────────────────────────────
// 1. Inside your existing pause panel, add a small section with:
//      - A header label: "DAILY CHALLENGE" (static text, not a script field)
//      - challengeText  — TMP label for the challenge description
//      - rewardText     — TMP label for the reward amount
//      - statusText     — TMP label for IN PROGRESS / COMPLETE! / CLAIMED
//
// 2. Attach this script to the pause panel root GameObject.
//    Wire up the three text fields in the Inspector.
//
// OnEnable fires whenever the pause panel becomes active, so the
// challenge info always refreshes when the player opens the pause menu.
// ─────────────────────────────────────────────────────────────────────────────

public class PauseMenuChallenge : MonoBehaviour
{
    [Header("Challenge Text Fields")]
    public TextMeshProUGUI challengeText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI statusText;

    [Header("Status Colours")]
    public Color inProgressColour = new Color(1f, 0.8f, 0.2f);
    public Color completedColour  = new Color(0.2f, 0.9f, 0.2f);
    public Color claimedColour    = new Color(0.6f, 0.6f, 0.6f);

    void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        if (DailyChallenge.Instance == null) return;

        if (challengeText != null)
            challengeText.text = DailyChallenge.Instance.GetChallengeDescription();

        if (rewardText != null)
            rewardText.text = DailyChallenge.Instance.GetRewardDescription();

        if (statusText == null) return;

        bool completed = DailyChallenge.Instance.IsCompleted;
        bool claimed   = DailyChallenge.Instance.RewardClaimed;

        if (claimed)
        {
            statusText.text  = "CLAIMED";
            statusText.color = claimedColour;
        }
        else if (completed)
        {
            statusText.text  = "COMPLETE! Claim on leaderboard.";
            statusText.color = completedColour;
        }
        else
        {
            statusText.text  = "IN PROGRESS";
            statusText.color = inProgressColour;
        }
    }
}

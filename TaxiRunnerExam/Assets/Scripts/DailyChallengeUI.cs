using UnityEngine;
using TMPro;


public class DailyChallengeUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI challengeText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI statusText;

    [Header("Colours")]
    public Color inProgressColour = new Color(1f, 0.8f, 0.2f);
    public Color completedColour = new Color(0.2f, 0.9f, 0.2f);
    public Color claimedColour = new Color(0.6f, 0.6f, 0.6f);

    void OnEnable()
    {
        Refresh();
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (DailyChallenge.Instance == null) return;

        DailyChallenge.Instance.RefreshChallenge();

        if (challengeText != null)
            challengeText.text = DailyChallenge.Instance.GetChallengeDescription();

        if (rewardText != null)
            rewardText.text = DailyChallenge.Instance.GetRewardDescription();

        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (statusText == null || DailyChallenge.Instance == null) return;

        bool completed = DailyChallenge.Instance.IsCompleted;
        bool claimed = DailyChallenge.Instance.RewardClaimed;

        if (claimed)
        {
            statusText.text = "CLAIMED";
            statusText.color = claimedColour;
        }
        else if (completed)
        {
            statusText.text = "COMPLETE! Claim on leaderboard.";
            statusText.color = completedColour;
        }
        else
        {
            statusText.text = "IN PROGRESS";
            statusText.color = inProgressColour;
        }
    }
}
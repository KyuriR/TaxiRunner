using UnityEngine;
using TMPro;

public class DailyChallengeClaim : MonoBehaviour
{
    [Header("Popup Panel")]
    public GameObject claimPopup;

    [Header("Text Fields")]
    public TextMeshProUGUI challengeText;
    public TextMeshProUGUI rewardText;

    void Start()
    {
        if (claimPopup != null)
            claimPopup.SetActive(false);

        // Auto-open if challenge is complete and reward not yet claimed
        if (DailyChallenge.Instance == null) return;

        DailyChallenge.Instance.RefreshChallenge();

        if (DailyChallenge.Instance.IsCompleted
            && !DailyChallenge.Instance.RewardClaimed)
        {
            OpenPopup();
        }
    }

    void OpenPopup()
    {
        if (challengeText != null)
            challengeText.text = DailyChallenge.Instance.GetChallengeDescription();

        if (rewardText != null)
            rewardText.text = DailyChallenge.Instance.GetRewardDescription();

        if (claimPopup != null)
            claimPopup.SetActive(true);
    }// Called by the X / Close button — dismisses without claiming
    public void ClosePopup()
    {
        if (claimPopup != null)
            claimPopup.SetActive(false);
    }
    // Called by ClaimButton OnClick
    public void ClaimAndClose()
    {
        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.ClaimReward();

        if (claimPopup != null)
            claimPopup.SetActive(false);
    }
}

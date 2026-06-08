using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartMenuManager : MonoBehaviour
{
    [Header("Main Start Screen")]
    public GameObject titleText;
    public GameObject startButton;

    [Header("Panels")]
    public GameObject instructionsPanel;
    public GameObject namePanel;
    public GameObject dailyChallengePanel;

    [Header("Name Input")]
    public TMP_InputField nameInput;

    void Start()
    {
        instructionsPanel.SetActive(false);
        namePanel.SetActive(false);

        if (dailyChallengePanel != null)
            dailyChallengePanel.SetActive(false);
    }

    public void PressStart()
    {
        titleText.SetActive(false);
        startButton.SetActive(false);
        instructionsPanel.SetActive(true);
        namePanel.SetActive(true);
    }

    public void PressCancel()
    {
        instructionsPanel.SetActive(false);
        namePanel.SetActive(false);
        titleText.SetActive(true);
        startButton.SetActive(true);
    }

    public void PressNext()
    {
        string enteredName = "player";
        if (nameInput != null && nameInput.text.Trim() != "")
            enteredName = nameInput.text.Trim().ToLower();

        PlayerPrefs.SetString("PlayerName", enteredName);
        PlayerPrefs.SetString("CurrentProfile", enteredName);
        PlayerPrefs.Save();

        // Refresh challenge now that profile key is set
        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.RefreshChallenge();

        // Hide name and instructions, show challenge panel
        instructionsPanel.SetActive(false);
        namePanel.SetActive(false);

        if (dailyChallengePanel != null)
            dailyChallengePanel.SetActive(true);
    }

    public void PressChallengeContinue()
    {
        string existingName = PlayerPrefs.GetString("PlayerName", "");
        if (existingName != "")
        {
            PlayerPrefs.SetString("CurrentProfile", existingName);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("VehicleSelectScene");
    }

    public void CheatMoney()
    {
        PlayerProfile.TotalMoney += 2000;
        Debug.Log("Cheat: R2000 added. Total: R" + PlayerProfile.TotalMoney);
    }
    public void SecretReset()
    {
        LeaderboardManager.ClearLeaderboard();
        PlayerProfile.TotalMoney = 0;
        PlayerPrefs.DeleteKey(ProfileManager.GetKey("VEHICLE_1"));
        PlayerPrefs.DeleteKey(ProfileManager.GetKey("VEHICLE_2"));
        PlayerPrefs.DeleteKey(ProfileManager.GetKey("AREA_1"));
        PlayerPrefs.DeleteKey(ProfileManager.GetKey("AREA_2"));
        PlayerPrefs.DeleteKey("DC_Date");
        PlayerPrefs.DeleteKey("DC_Completed");
        PlayerPrefs.DeleteKey("DC_Claimed");
        PlayerPrefs.Save();
        Debug.Log("Full reset complete.");
    }
}
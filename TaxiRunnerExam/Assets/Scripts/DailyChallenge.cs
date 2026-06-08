using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public enum ChallengeType { EarnMoney, PassengersNoPothole, SurviveInArea }

[System.Serializable]
public class ChallengeData
{
    public ChallengeType type;
    public int targetValue;       // money target, passenger count, or seconds
    public int areaIndex;         // only used for SurviveInArea
    public string areaName;       // only used for SurviveInArea
    public int rewardAmount;      // bonus currency on completion
    public string dateKey;        // "yyyy-MM-dd" — used to detect day change
}

public class DailyChallenge : MonoBehaviour
{
    public static DailyChallenge Instance;

    [Header("Area Config Reference")]
    [Tooltip("Drag in your AreaConfig GameObject from the scene.")]
    public AreaConfig areaConfig;

    [Header("Reward Settings")]
    public int baseReward = 150;

    // ── Runtime state ─────────────────────────────────────────────────────────
    public ChallengeData Today { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool RewardClaimed { get; private set; }

    // Progress tracked during a run
    private int runMoneyEarned = 0;
    private int runPassengersNoPothole = 0;
    private bool potHoleHitThisRun = false;
    private float runSurvivalTime = 0f;

    // PlayerPrefs keys
    private const string KEY_DATE        = "DC_Date";
    private const string KEY_TYPE        = "DC_Type";
    private const string KEY_TARGET      = "DC_Target";
    private const string KEY_AREA        = "DC_Area";
    private const string KEY_AREA_NAME   = "DC_AreaName";
    private const string KEY_REWARD      = "DC_Reward";
    private const string KEY_COMPLETED   = "DC_Completed";
    private const string KEY_CLAIMED     = "DC_Claimed";

    private const string AREA_KEY_PREFIX = "AREA_";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        RefreshChallenge();
    }


    public void PressChallengeContinue()
    {
        // If returning from a previous run, PlayerName is already set
        // If not set, default to last used name
        string existingName = PlayerPrefs.GetString("PlayerName", "");
        if (existingName != "")
        {
            PlayerPrefs.SetString("CurrentProfile", existingName);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("VehicleSelectScene");
    }
    public void RefreshChallenge()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string saved = PlayerPrefs.GetString(KEY_DATE, "");

        if (saved == today)
        {
            // Load existing challenge for today
            Today = LoadSaved();
            IsCompleted  = PlayerPrefs.GetInt(KEY_COMPLETED, 0) == 1;
            RewardClaimed = PlayerPrefs.GetInt(KEY_CLAIMED, 0) == 1;
        }
        else
        {
            // New day — generate fresh challenge
            Today = GenerateChallenge(today);
            SaveChallenge(Today);
            IsCompleted   = false;
            RewardClaimed = false;
            PlayerPrefs.SetInt(KEY_COMPLETED, 0);
            PlayerPrefs.SetInt(KEY_CLAIMED, 0);
            PlayerPrefs.Save();
        }
    }

    ChallengeData GenerateChallenge(string dateKey)
    {
        // Seed from date so targets vary per day
        int dayOfYear = DateTime.Now.DayOfYear;
        int year      = DateTime.Now.Year;
        int seed      = dayOfYear + year * 366;

        ChallengeType type = (ChallengeType)(dayOfYear % 3);

        ChallengeData data = new ChallengeData();
        data.dateKey      = dateKey;
        data.type         = type;
        data.rewardAmount = baseReward;

        switch (type)
        {
            case ChallengeType.EarnMoney:
                // Target between R150 and R400, steps of 50
                int[] moneyTargets = { 150, 200, 250, 300, 350, 400 };
                data.targetValue = moneyTargets[seed % moneyTargets.Length];
                break;

            case ChallengeType.PassengersNoPothole:
                // Target between 3 and 8 passengers
                int[] passengerTargets = { 3, 4, 5, 6, 7, 8 };
                data.targetValue = passengerTargets[seed % passengerTargets.Length];
                break;

            case ChallengeType.SurviveInArea:
                // Target between 45 and 120 seconds
                int[] timeTargets = { 45, 60, 75, 90, 105, 120 };
                data.targetValue = timeTargets[seed % timeTargets.Length];

                // Pick an area the player has unlocked
                int areaIdx = GetUnlockedAreaForChallenge(seed);
                data.areaIndex = areaIdx;
                data.areaName  = GetAreaName(areaIdx);
                break;
        }

        return data;
    }

    // Returns an unlocked area index for a survival challenge.
    // Only assigns Sandton or CBD if the player has actually unlocked them.
    // Falls back to Soweto (index 0) if nothing else is available.
    int GetUnlockedAreaForChallenge(int seed)
    {
        // Collect all unlocked area indices
        var unlocked = new System.Collections.Generic.List<int>();
        unlocked.Add(0); // Soweto always unlocked

        int areaCount = areaConfig != null && areaConfig.areas != null
            ? areaConfig.areas.Length
            : 3;

        for (int i = 1; i < areaCount; i++)
        {
            string key = ProfileManager.GetKey(AREA_KEY_PREFIX + i);
            if (PlayerPrefs.GetInt(key, 0) == 1)
                unlocked.Add(i);
        }

        return unlocked[seed % unlocked.Count];
    }

    string GetAreaName(int index)
    {
        if (areaConfig != null && areaConfig.areas != null
            && index < areaConfig.areas.Length)
            return areaConfig.areas[index].areaName;

        switch (index)
        {
            case 1: return "Sandton";
            case 2: return "CBD";
            default: return "Soweto";
        }
    }

    // ── Run Tracking (called from GameManager) ────────────────────────────────

    // Call this at the start of each run to reset tracking
    public void OnRunStart()
    {
        runMoneyEarned         = 0;
        runPassengersNoPothole = 0;
        potHoleHitThisRun      = false;
        runSurvivalTime        = 0f;
    }

    // Call every frame during the run
    public void OnRunUpdate(float deltaTime)
    {
        if (Today == null || IsCompleted) return;
        runSurvivalTime += deltaTime;
    }

    // Call when a passenger is picked up
    public void OnPassengerPickedUp()
    {
        if (Today == null || IsCompleted) return;
        if (!potHoleHitThisRun)
            runPassengersNoPothole++;
    }

    // Call when a pothole is hit
    public void OnPotholeHit()
    {
        if (Today == null || IsCompleted) return;
        potHoleHitThisRun      = true;
        runPassengersNoPothole = 0; // reset — must go the whole run without one
    }

    // Call at end of run with final money and area
    public void OnRunEnd(int moneyEarned, int selectedAreaIndex)
    {
        if (Today == null || IsCompleted || RewardClaimed) return;

        runMoneyEarned = moneyEarned;

        bool met = false;

        switch (Today.type)
        {
            case ChallengeType.EarnMoney:
                met = runMoneyEarned >= Today.targetValue;
                break;

            case ChallengeType.PassengersNoPothole:
                met = runPassengersNoPothole >= Today.targetValue;
                break;

            case ChallengeType.SurviveInArea:
                bool correctArea = selectedAreaIndex == Today.areaIndex;
                bool survivedLong = Mathf.FloorToInt(runSurvivalTime) >= Today.targetValue;
                met = correctArea && survivedLong;
                break;
        }

        if (met)
        {
            IsCompleted = true;
            PlayerPrefs.SetInt(KEY_COMPLETED, 1);
            PlayerPrefs.Save();
        }
    }

    // Call when the player presses Claim on the UI
    public void ClaimReward()
    {
        if (!IsCompleted || RewardClaimed) return;

        RewardClaimed = true;
        PlayerPrefs.SetInt(KEY_CLAIMED, 1);
        PlayerPrefs.Save();

        PlayerProfile.TotalMoney += Today.rewardAmount;
    }

    // ── Returns a human-readable description of today's challenge ─────────────
    public string GetChallengeDescription()
    {
        if (Today == null) return "";

        switch (Today.type)
        {
            case ChallengeType.EarnMoney:
                return "Earn R" + Today.targetValue + " in a single run";

            case ChallengeType.PassengersNoPothole:
                return "Pick up " + Today.targetValue
                       + " passengers without hitting a pothole";

            case ChallengeType.SurviveInArea:
                return "Survive " + Today.targetValue
                       + " seconds in " + Today.areaName;

            default: return "";
        }
    }

    public string GetRewardDescription()
    {
        if (Today == null) return "";
        return "+ R" + Today.rewardAmount + " bonus";
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    void SaveChallenge(ChallengeData data)
    {
        PlayerPrefs.SetString(KEY_DATE,      data.dateKey);
        PlayerPrefs.SetInt(KEY_TYPE,         (int)data.type);
        PlayerPrefs.SetInt(KEY_TARGET,       data.targetValue);
        PlayerPrefs.SetInt(KEY_AREA,         data.areaIndex);
        PlayerPrefs.SetString(KEY_AREA_NAME, data.areaName ?? "");
        PlayerPrefs.SetInt(KEY_REWARD,       data.rewardAmount);
        PlayerPrefs.Save();
    }

    ChallengeData LoadSaved()
    {
        ChallengeData data = new ChallengeData();
        data.dateKey      = PlayerPrefs.GetString(KEY_DATE, "");
        data.type         = (ChallengeType)PlayerPrefs.GetInt(KEY_TYPE, 0);
        data.targetValue  = PlayerPrefs.GetInt(KEY_TARGET, 100);
        data.areaIndex    = PlayerPrefs.GetInt(KEY_AREA, 0);
        data.areaName     = PlayerPrefs.GetString(KEY_AREA_NAME, "Soweto");
        data.rewardAmount = PlayerPrefs.GetInt(KEY_REWARD, baseReward);
        return data;
    }
}

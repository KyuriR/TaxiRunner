using UnityEngine;

public static class PlayerProfile
{
    private const string MONEY_KEY = "TOTAL_MONEY";

    public static int TotalMoney
    {
        get
        {
            return PlayerPrefs.GetInt(ProfileManager.GetKey(MONEY_KEY), 0);
        }
        set
        {
            PlayerPrefs.SetInt(ProfileManager.GetKey(MONEY_KEY), value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsVehicleUnlocked(int index)
    {
        if (index == 0) return true;

        return PlayerPrefs.GetInt(ProfileManager.GetKey("VEHICLE_" + index), 0) == 1;
    }

    public static void UnlockVehicle(int index)
    {
        PlayerPrefs.SetInt(ProfileManager.GetKey("VEHICLE_" + index), 1);
        PlayerPrefs.Save();
    }
}
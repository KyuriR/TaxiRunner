using UnityEngine;

public static class VehicleUnlock
{
    public static bool IsUnlocked(int index)
    {
        if (index == 0) return true; // Taxi always unlocked

        return PlayerPrefs.GetInt("VEHICLE_" + index, 0) == 1;
    }

    public static void Unlock(int index)
    {
        PlayerPrefs.SetInt("VEHICLE_" + index, 1);
        PlayerPrefs.Save();
    }
}
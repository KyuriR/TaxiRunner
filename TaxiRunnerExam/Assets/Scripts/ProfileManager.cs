using UnityEngine;

public static class ProfileManager
{
    public static string CurrentProfile
    {
        get
        {
            return PlayerPrefs.GetString("CurrentProfile", "player");
        }
    }

    public static string GetKey(string baseKey)
    {
        return CurrentProfile + "_" + baseKey;
    }
}
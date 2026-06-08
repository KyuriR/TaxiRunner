using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VehicleSelectManager : MonoBehaviour
{
    public VehicleData[] vehicles;
    public TextMeshProUGUI[] priceTexts;
    public TextMeshProUGUI totalMoneyText;

    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;

    void OnEnable()
    {
        // Force PlayerPrefs to sync from disk before we read anything.
        // This prevents reading stale/zero values when coming from another scene.
        PlayerPrefs.Save();
        UpdateUI();
    }

    void Start()
    {
        // Also update here as a fallback
        UpdateUI();
    }

    public void SelectVehicle(int index)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        if (index < 0 || index >= vehicles.Length) return;

        if (PlayerProfile.IsVehicleUnlocked(index))
        {
            RunData.selectedVehicleIndex = index;
            SceneManager.LoadScene("AreaSelectScene");
        }
        else
        {
            TryBuyVehicle(index);
        }
    }

    void TryBuyVehicle(int index)
    {
        int cost = vehicles[index].cost;

        if (PlayerProfile.TotalMoney >= cost)
        {
            PlayerProfile.TotalMoney -= cost;
            PlayerProfile.UnlockVehicle(index);

            RunData.selectedVehicleIndex = index;

            UpdateUI();
        }
        else
        {
            Debug.Log("Not enough money to buy: " + vehicles[index].vehicleName
                      + " (Have R" + PlayerProfile.TotalMoney + ", need R" + cost + ")");
        }
    }

    void UpdateUI()
    {
        int total = PlayerProfile.TotalMoney;

        Debug.Log("UpdateUI — TotalMoney: R" + total); // remove this once working

        if (totalMoneyText != null)
            totalMoneyText.text = "R " + total;
        else
            Debug.LogWarning("totalMoneyText is not assigned in VehicleSelectManager!");

        for (int i = 0; i < vehicles.Length; i++)
        {
            if (i >= priceTexts.Length) continue;
            if (priceTexts[i] == null) continue;

            if (PlayerProfile.IsVehicleUnlocked(i))
            {
                priceTexts[i].text = "SELECT";
                priceTexts[i].color = unlockedColor;
            }
            else
            {
                priceTexts[i].text = "R " + vehicles[i].cost;
                priceTexts[i].color = lockedColor;
            }
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("StartScene");
    }
}
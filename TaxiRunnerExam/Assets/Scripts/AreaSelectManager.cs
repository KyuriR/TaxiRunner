using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class AreaSelectManager : MonoBehaviour
{
    [Header("Area Data")]
    public AreaData[] areas;

    [Header("UI — Price Labels")]
    [Tooltip("One TMP label per area button showing cost or SELECT.")]
    public TextMeshProUGUI[] priceTexts;

    [Tooltip("TMP label showing the player's total saved money.")]
    public TextMeshProUGUI totalMoneyText;

    [Header("UI — Info Panels")]
    [Tooltip("Three info panel GameObjects, one per area. Set all inactive by default.")]
    public GameObject[] infoPanels;

    [Header("Colours")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;

    private const string AreaKeyPrefix = "AREA_";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        PlayerPrefs.Save();
        CloseAllInfoPanels();
        UpdateUI();
    }

    void Start()
    {
        CloseAllInfoPanels();
        UpdateUI();
    }

    public void SelectArea(int index)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        if (index < 0 || index >= areas.Length) return;

        if (IsAreaUnlocked(index))
        {
            RunData.selectedAreaIndex = index;
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            TryBuyArea(index);
        }
    }

    void TryBuyArea(int index)
    {
        int cost = areas[index].cost;

        if (PlayerProfile.TotalMoney >= cost)
        {
            PlayerProfile.TotalMoney -= cost;
            UnlockArea(index);
            RunData.selectedAreaIndex = index;
            UpdateUI();
        }
        else
        {
            Debug.Log("Not enough money for: " + areas[index].areaName
                      + " — need R" + cost
                      + ", have R" + PlayerProfile.TotalMoney);
        }
    }

 
    public void OpenInfoPanel(int index)
    {
        CloseAllInfoPanels();

        if (index >= 0 && index < infoPanels.Length && infoPanels[index] != null)
            infoPanels[index].SetActive(true);
    }

    public void CloseAllInfoPanels()
    {
        if (infoPanels == null) return;

        foreach (GameObject panel in infoPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    void UpdateUI()
    {
        if (totalMoneyText != null)
            totalMoneyText.text = "R " + PlayerProfile.TotalMoney;

        for (int i = 0; i < areas.Length; i++)
        {
            if (i >= priceTexts.Length || priceTexts[i] == null) continue;

            if (IsAreaUnlocked(i))
            {

                priceTexts[i].text = "SELECT";
                priceTexts[i].color = unlockedColor;
            }
            else
            {
                
                priceTexts[i].text = "LOCKED R " + areas[i].cost;
                priceTexts[i].color = lockedColor;
            }
        }
    }


    bool IsAreaUnlocked(int index)
    {
        if (index == 0) return true; // Soweto always free
        string key = ProfileManager.GetKey(AreaKeyPrefix + index);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    void UnlockArea(int index)
    {
        string key = ProfileManager.GetKey(AreaKeyPrefix + index);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }


    public void GoBack()
    {
        SceneManager.LoadScene("VehicleSelectScene");
    }
}
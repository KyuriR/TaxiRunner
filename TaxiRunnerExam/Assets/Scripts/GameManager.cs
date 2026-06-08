using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Vehicle Selection")]
    public GameObject[] vehiclePrefabs;
    public VehicleData[] vehicles;
    public Transform spawnPoint;

    private GameObject currentVehicleObject;
    private TaxiController taxiController;

    [Header("Speed")]
    public float baseSpeed = 9f;
    public float speedIncreaseAmount = 0.9f;
    public float timeBetweenIncreases = 45f;
    private float timeSinceLastIncrease = 0f;

    [Header("Spawn Intervals")]
    public float carBaseInterval = 1.8f;
    public float carIntervalDecreasePerMinute = 0.15f;
    public float minCarInterval = 0.9f;
    public float potholeBaseInterval = 2.2f;
    public float potholeIntervalDecreasePerMinute = 0.15f;
    public float minPotholeInterval = 1.1f;
    public float powerUpBaseInterval = 10f;
    public float powerUpIntervalDecreasePerMinute = 0.25f;
    public float minPowerUpInterval = 6f;

    [Header("State")]
    public bool gameOver = false;
    public bool pickupPause = false;
    public bool crashChoiceActive = false;
    public bool pauseActive = false;

    private bool moneySaved = false;

    [Header("Money")]
    public int money = 0;
    public TextMeshProUGUI walletText;
    public int carPenalty = 15;

    [Header("Passengers")]
    public int maxPassengers = 8;
    public int currentPassengers = 0;
    public int totalPassengersPickedUp = 0;
    public int basePassengerFare = 10;
    public TextMeshProUGUI passengerText;

    [Header("Timer")]
    public float survivalTime = 0f;
    public TextMeshProUGUI timerText;

    [Header("Lives")]
    public int wheels = 4;

    [Header("Drop-Off Timing")]
    public float minRandomDropOffDelay = 4f;
    public float maxRandomDropOffDelay = 9f;
    public float dropOffCountdownDuration = 10f;

    [Header("Drop-Off Position")]
    public Transform taxiTransform;
    public float leftDropOffX = -4.3f;
    public float rightDropOffX = 4.3f;
    public float dropOffTolerance = 0.3f;

    [Header("Drop-Off UI")]
    public GameObject dropOffPanel;
    public TextMeshProUGUI dropOffCountdownText;
    public GameObject dropOffButtonPrompt;

    [Header("Money Popup")]
    public MoneyPopup popupPrefab;
    public Canvas mainCanvas;

    [Header("Shield")]
    public bool shieldActive = false;

    [Header("Binoculars")]
    public bool hasBinoculars = false;
    public bool binocularsActive = false;
    public float binocularsDuration = 3f;
    public float binocularsZoomOutSize = 7f;
    public Camera gameplayCamera;
    public GameObject binocularIcon;

    private float binocularsTimer = 0f;
    private float normalCameraSize = 5f;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject crashPanel;
    public GameObject gameOverPanel;

    [Header("Wheel Game Over Panel")]
    public GameObject wheelGameOverPanel;
    public GameObject wheelGameOverPanel2;

    [Header("Buy Wheel")]
    public int wheelCost = 50;
    public TMPro.TextMeshProUGUI buyWheelCostText;
    private bool wheelChoiceActive = false;
    private bool wheelAlreadyBought = false;

    // POLICE SYSTEM

    [Header("Police System")]
    public GameObject policePrefab;

    [Tooltip("How long the taxi must stay on the boundary before the warning + police triggers.")]
    public float boundaryTimeToTrigger = 10f;

    [Tooltip("How long the police car stays on screen once spawned.")]
    public float policeDuration = 7f;

    [Tooltip("Fine if the taxi crashes into a car while police are present.")]
    public int policeCrashFine = 20;

    [Tooltip("Fine if the taxi stays on the boundary for the entire police visit.")]
    public int policeLoiterFine = 30;

    [Tooltip("How close to the boundary edge counts as hugging it.")]
    public float boundaryHugTolerance = 0.5f;

    // Internal police state
    public bool policeActive = false;
    private float boundaryTimer = 0f;   // how long taxi has been hugging boundary THIS cycle
    private float policeTimer = 0f;     // counts up while police are on screen
    private bool policeLoitering = false;
    private bool warningFlashStarted = false;
    private bool policeSpawned = false;
    private GameObject activePoliceObject = null;

    [Header("Police Warning")]
    [Tooltip("Full-screen red Image in the Canvas used for the warning flash. Set alpha in Inspector.")]
    public Image redFlashImage;

    [Header("Fine System")]
    public GameObject finePanel;
    public TextMeshProUGUI fineText;

    public bool dropOffWarningActive = false;

    private readonly List<PassengerRideData> activePassengers = new List<PassengerRideData>();

    // INIT


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        SpawnSelectedVehicle();
        ApplySelectedVehicleStats();
        ApplySelectedAreaStats();
        if (wheelGameOverPanel != null) wheelGameOverPanel.SetActive(false);

        if (gameplayCamera == null && Camera.main != null)
            gameplayCamera = Camera.main;

        if (gameplayCamera != null)
            normalCameraSize = gameplayCamera.orthographicSize;

        if (dropOffPanel != null) dropOffPanel.SetActive(false);
        if (dropOffButtonPrompt != null) dropOffButtonPrompt.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (crashPanel != null) crashPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (finePanel != null) finePanel.SetActive(false);
        if (redFlashImage != null) redFlashImage.gameObject.SetActive(false);

        UpdateAllUI();

        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.OnRunStart();
    }

    void Update()
    {
        if (gameOver || crashChoiceActive || pauseActive || wheelChoiceActive) return;

        survivalTime += Time.deltaTime;
        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.OnRunUpdate(Time.deltaTime);
        UpdateTimerUI();

        timeSinceLastIncrease += Time.deltaTime;
        if (timeSinceLastIncrease >= timeBetweenIncreases)
        {
            baseSpeed += speedIncreaseAmount;
            timeSinceLastIncrease = 0f;
        }

        UpdatePassengerTimers(Time.deltaTime);
        CheckForDropOffWarning();
        UpdateBinoculars(Time.deltaTime);
        UpdateDropOffUI();
        UpdatePoliceSystem(Time.deltaTime);
    }


    void ApplySelectedAreaStats()
    {
        AreaConfig config = FindObjectOfType<AreaConfig>();
        if (config == null || config.areas == null) return;

        int index = Mathf.Clamp(RunData.selectedAreaIndex, 0, config.areas.Length - 1);
        AreaData area = config.areas[index];

      
        baseSpeed *= area.speedMultiplier;
        basePassengerFare = Mathf.RoundToInt(basePassengerFare * area.fareMultiplier);

        carBaseInterval = Mathf.Max(minCarInterval, carBaseInterval / area.speedMultiplier);
        potholeBaseInterval = Mathf.Max(minPotholeInterval, potholeBaseInterval / area.speedMultiplier);

       
        PassengerSpawner ps = FindObjectOfType<PassengerSpawner>();
        if (ps != null)
        {
            switch (RunData.selectedAreaIndex)
            {
                case 0: ps.interval = 3f; break; // Soweto — balanced
                case 1: ps.interval = 5f; break; // Sandton — patient, high value
                case 2: ps.interval = 1.5f; break; // CBD — frequent, frantic
            }
        }
    }


    // POLICE SYSTEM


    bool IsTaxiOnBoundary()
    {
        if (taxiTransform == null) return false;
        float x = taxiTransform.position.x;
        return x <= leftDropOffX + boundaryHugTolerance ||
               x >= rightDropOffX - boundaryHugTolerance;
    }

    void UpdatePoliceSystem(float deltaTime)
    {
        if (policeActive)
        {
            // Count up police visit timer
            policeTimer += deltaTime;

            // If taxi leaves the boundary during the visit, they're no longer loitering
            if (!IsTaxiOnBoundary())
                policeLoitering = false;

            // Check if the boundary loiter fine threshold is reached (stayed entire 7s)
            if (policeLoitering && policeTimer >= policeDuration)
            {
                // Apply loiter fine and dismiss - no game over
                ShowFinePanel(policeLoiterFine, "LOITER FINE -R" + policeLoiterFine);
                EndPoliceEvent();
                return;
            }

            // Police visit ended normally (no loitering)
            if (policeTimer >= policeDuration)
            {
                EndPoliceEvent();
            }
        }
        else
        {
            // Not active — track boundary time
            if (IsTaxiOnBoundary())
            {
                boundaryTimer += deltaTime;

                // At 9 seconds: flash warning (1 second before police spawn at 10s)
                if (!warningFlashStarted && boundaryTimer >= boundaryTimeToTrigger - 1f)
                {
                    warningFlashStarted = true;
                    StartCoroutine(FlashWarningThenSpawn());
                }
            }
            else
            {
                // Reset everything if they leave before triggering
                boundaryTimer = 0f;
                warningFlashStarted = false;
                policeSpawned = false;
            }
        }
    }


    IEnumerator FlashWarningThenSpawn()
    {
        // Play siren as soon as flashing starts
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPolice();

        for (int i = 0; i < 3; i++)
        {
            if (redFlashImage != null) redFlashImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.15f);
            if (redFlashImage != null) redFlashImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.1f);

        if (!policeSpawned)
        {
            policeSpawned = true;
            SpawnPolice();
        }
    }

    void SpawnPolice()
    {
        if (policePrefab == null || taxiTransform == null) return;

        policeActive = true;
        policeTimer = 0f;
        policeLoitering = IsTaxiOnBoundary(); // only loitering if still on boundary at spawn

        // Spawn well above the top of the screen so the car drives naturally onto screen
        float screenTop = Camera.main != null
            ? Camera.main.transform.position.y + Camera.main.orthographicSize + 4f
            : taxiTransform.position.y + 10f;

        Vector3 spawnPos = new Vector3(taxiTransform.position.x, screenTop, 0f);

        activePoliceObject = Instantiate(policePrefab, spawnPos, Quaternion.identity);

        PoliceChase pc = activePoliceObject.GetComponent<PoliceChase>();
        if (pc != null)
            pc.target = taxiTransform;
    }

    public void EndPoliceEvent()
    {
        policeActive = false;
        policeLoitering = false;
        boundaryTimer = 0f;
        policeTimer = 0f;
        warningFlashStarted = false;
        policeSpawned = false;

        if (activePoliceObject != null)
        {
            Destroy(activePoliceObject);
            activePoliceObject = null;
        }
    }

    // FINE PANEL
   
    void ShowFinePanel(int amount, string label)
    {
        RemoveMoney(amount);

        if (finePanel != null)
        {
            finePanel.SetActive(true);
            if (fineText != null)
                fineText.text = label;
        }

        Time.timeScale = 0f;
    }

    // Called by the "Continue" button on the fine panel
    public void DismissFinePanel()
    {
        if (finePanel != null)
            finePanel.SetActive(false);

        Time.timeScale = 1f;
        GameOver();
    }

    // VEHICLE SPAWNING


    void SpawnSelectedVehicle()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            Debug.LogError("GameManager: No vehicle prefabs assigned.");
            return;
        }

        int index = Mathf.Clamp(RunData.selectedVehicleIndex, 0, vehiclePrefabs.Length - 1);
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0f, -4.33f, 0f);

        currentVehicleObject = Instantiate(vehiclePrefabs[index], spawnPos, Quaternion.identity);
        taxiController = currentVehicleObject.GetComponent<TaxiController>();
        taxiTransform = currentVehicleObject.transform;

        if (taxiController == null)
            Debug.LogError("Selected vehicle prefab is missing TaxiController.");

        CameraFollow cam = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cam != null)
            cam.target = currentVehicleObject.transform;
    }

    void ApplySelectedVehicleStats()
    {
        if (vehicles == null || vehicles.Length == 0) return;

        int index = Mathf.Clamp(RunData.selectedVehicleIndex, 0, vehicles.Length - 1);

        maxPassengers = vehicles[index].capacity;
        baseSpeed = vehicles[index].speed;
        currentPassengers = 0;
    }


    // SPEED / DIFFICULTY
  
    public float RoadSpeed
    {
        get
        {
            if (gameOver || pickupPause || crashChoiceActive || pauseActive) return 0f;
            return baseSpeed;
        }
    }

    public float CarSpeed
    {
        get
        {
            if (gameOver || crashChoiceActive || pauseActive) return 0f;
            return baseSpeed;
        }
    }

    public int GetDifficultyStage()
    {
        return Mathf.FloorToInt(survivalTime / timeBetweenIncreases);
    }

    public float GetCarSpawnInterval()
    {
        float value = carBaseInterval - (GetDifficultyStage() * carIntervalDecreasePerMinute);
        return Mathf.Max(minCarInterval, value);
    }

    public float GetPotholeSpawnInterval()
    {
        float value = potholeBaseInterval - (GetDifficultyStage() * potholeIntervalDecreasePerMinute);
        return Mathf.Max(minPotholeInterval, value);
    }

    public float GetPowerUpSpawnInterval()
    {
        float value = powerUpBaseInterval - (GetDifficultyStage() * powerUpIntervalDecreasePerMinute);
        return Mathf.Max(minPowerUpInterval, value);
    }


    // PASSENGERS & DROP-OFF
    public bool CanPickUpPassenger()
    {
        return currentPassengers < maxPassengers;
    }

    public void PickUpPassenger(int fare)
    {
        if (!CanPickUpPassenger()) return;
        

        PassengerRideData ride = new PassengerRideData();
        ride.baseFare = fare;
        ride.timeSincePickup = 0f;
        ride.randomDropOffDelay = Random.Range(minRandomDropOffDelay, maxRandomDropOffDelay);
        ride.warningStarted = false;
        ride.countdownRemaining = 0f;

        activePassengers.Add(ride);

        currentPassengers++;
        totalPassengersPickedUp++;


        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.OnPassengerPickedUp();

        AddMoney(fare);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCash();
        SpawnMoneyPopup(fare);
        UpdatePassengerUI();
    }

    void UpdatePassengerTimers(float deltaTime)
    {
        for (int i = 0; i < activePassengers.Count; i++)
        {
            activePassengers[i].timeSincePickup += deltaTime;

            if (activePassengers[i].warningStarted)
            {
                activePassengers[i].countdownRemaining -= deltaTime;
                if (activePassengers[i].countdownRemaining < 0f)
                    activePassengers[i].countdownRemaining = 0f;
            }
        }
    }

    void CheckForDropOffWarning()
    {
        if (dropOffWarningActive) return;

        for (int i = 0; i < activePassengers.Count; i++)
        {
            if (!activePassengers[i].warningStarted &&
                activePassengers[i].timeSincePickup >= activePassengers[i].randomDropOffDelay)
            {
                activePassengers[i].warningStarted = true;
                activePassengers[i].countdownRemaining = dropOffCountdownDuration;
                dropOffWarningActive = true;
                break;
            }
        }
    }

    bool HasPassengersToDropOff()
    {
        return activePassengers.Count > 0;
    }

    bool CanShowDropOffPrompt()
    {
        if (!dropOffWarningActive) return false;
        if (taxiTransform == null) return false;

        float x = taxiTransform.position.x;
        bool nearLeft = Mathf.Abs(x - leftDropOffX) <= dropOffTolerance;
        bool nearRight = Mathf.Abs(x - rightDropOffX) <= dropOffTolerance;
        return nearLeft || nearRight;
    }

    public void TryDropOff()
    {
        if (!dropOffWarningActive) return;
        if (!CanShowDropOffPrompt()) return;
       

        int tip = 0;
        for (int i = 0; i < activePassengers.Count; i++)
        {
            if (activePassengers[i].warningStarted)
            {
                float remaining = activePassengers[i].countdownRemaining;

                if (remaining >= 1f && remaining <= 3f)
                    tip += 10;
                else if (remaining > 3f && remaining <= 5f)
                    tip += 5;
            }
        }

        // Minibus (index 2) earns 1.5x tips — bulk delivery bonus
        if (RunData.selectedVehicleIndex == 2 && tip > 0)
            tip = Mathf.FloorToInt(tip * 1.5f);

        activePassengers.RemoveAll(r => r.warningStarted);
        currentPassengers = activePassengers.Count;
        dropOffWarningActive = false;

        if (tip > 0)
        {
            AddMoney(tip);
            SpawnTipPopup(tip);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayTip();
        }

        UpdatePassengerUI();
        UpdateDropOffUI();
    }
    float GetCurrentCountdownRemaining()
    {
        float min = float.MaxValue;
        for (int i = 0; i < activePassengers.Count; i++)
        {
            if (activePassengers[i].warningStarted && activePassengers[i].countdownRemaining < min)
                min = activePassengers[i].countdownRemaining;
        }
        return min == float.MaxValue ? 0f : min;
    }

    void UpdateDropOffUI()
    {
        bool showPanel = dropOffWarningActive && HasPassengersToDropOff();
        bool showPrompt = CanShowDropOffPrompt();

        if (dropOffPanel != null) dropOffPanel.SetActive(showPanel);

        if (dropOffCountdownText != null)
        {
            dropOffCountdownText.text = showPanel
                ? "DROP OFF IN " + Mathf.CeilToInt(GetCurrentCountdownRemaining()) + "s"
                : "";
        }

        if (dropOffButtonPrompt != null) dropOffButtonPrompt.SetActive(showPrompt);
    }


    // MONEY POPUPS
    void SpawnMoneyPopup(int amount)
    {
        if (popupPrefab == null || mainCanvas == null) return;
        if (Camera.main == null || taxiTransform == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(taxiTransform.position + Vector3.up * 1.2f);
        MoneyPopup popup = Instantiate(popupPrefab, mainCanvas.transform);
        popup.GetComponent<RectTransform>().position = screenPos;
        popup.SetAmount(amount);
    }

    void SpawnTipPopup(int amount)
    {
        if (popupPrefab == null || mainCanvas == null) return;
        if (Camera.main == null || taxiTransform == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(taxiTransform.position + Vector3.up * 1.6f);
        MoneyPopup popup = Instantiate(popupPrefab, mainCanvas.transform);
        popup.GetComponent<RectTransform>().position = screenPos;
        popup.SetCustomText("TIP +R" + amount);
    }


    // MONEY
    public void AddMoney(int amount)
    {
        money += amount;
        UpdateWalletUI();
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
        if (money < 0) money = 0;
        UpdateWalletUI();
    }



    // UI
    void UpdateAllUI()
    {
        UpdateWalletUI();
        UpdatePassengerUI();
        UpdateTimerUI();
        UpdateDropOffUI();
    }

    void UpdateWalletUI()
    {
        if (walletText != null)
            walletText.text = "<b>R" + money + "</b>";
    }

    void UpdatePassengerUI()
    {
        if (passengerText != null)
        {
            passengerText.text = currentPassengers + "/" + maxPassengers;
            passengerText.color = Color.black;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(survivalTime / 60f);
        int seconds = Mathf.FloorToInt(survivalTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    // POWER-UPS
    public void BuyWheel()
    {
        if (money >= wheelCost)
        {
            RemoveMoney(wheelCost);
            wheels = 1;
            wheelAlreadyBought = true;
            wheelChoiceActive = false;
            Time.timeScale = 1f;

            if (wheelGameOverPanel != null)
                wheelGameOverPanel.SetActive(false);
        }
        else
        {
            if (buyWheelCostText != null)
                buyWheelCostText.text = "Insufficient funds!";
        }
    }
    IEnumerator InsufficientFundsDelay()
    {
        // Already showing "Insufficient funds!" from ShowWheelGameOverPanel
        // Wait 2 seconds at timeScale 0 using realtime
        yield return new WaitForSecondsRealtime(2f);

        wheelChoiceActive = false;

        if (wheelGameOverPanel != null)
            wheelGameOverPanel.SetActive(false);

        Time.timeScale = 1f;
        GameOver();
    }

    public void ActivateShield()
    {
        shieldActive = true;
        if (taxiController != null)
            taxiController.SetShieldVisual(true);
    }

    public void ConsumeShield()
    {
        shieldActive = false;
        if (taxiController != null)
            taxiController.SetShieldVisual(false);
    }

    public void GiveBinoculars()
    {
        hasBinoculars = true;
        if (binocularIcon != null)
            binocularIcon.SetActive(true);
    }

    public void TryUseBinoculars()
    {
        if (gameOver || crashChoiceActive || pauseActive) return;
        if (!hasBinoculars || binocularsActive) return;

        hasBinoculars = false;
        binocularsActive = true;
        binocularsTimer = binocularsDuration;

        if (binocularIcon != null)
            binocularIcon.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.orthographicSize = binocularsZoomOutSize;
    }

    void UpdateBinoculars(float deltaTime)
    {
        if (!binocularsActive) return;

        binocularsTimer -= deltaTime;
        if (binocularsTimer <= 0f)
        {
            binocularsActive = false;
            if (gameplayCamera != null)
                gameplayCamera.orthographicSize = normalCameraSize;
        }
    }


    // HAZARDS

    public void TakePotholeHit()
    {
        if (gameOver) return;

        int damage = (RunData.selectedVehicleIndex == 1) ? 2 : 1;
        wheels -= damage;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPothole();
        if (wheels < 0) wheels = 0;

        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.OnPotholeHit();

        if (wheels <= 0)
        {
            if (!wheelAlreadyBought)
                ShowWheelGameOverPanel();
            else
                ShowWheelGameOverPanel2();
        }
    }

    void ShowWheelGameOverPanel()
    {
        wheelChoiceActive = true;
        Time.timeScale = 0f;

        if (buyWheelCostText != null)
        {
            if (money >= wheelCost)
                buyWheelCostText.text = "Buy a wheel for R" + wheelCost + "?";
            else
                buyWheelCostText.text = "Insufficient funds!";
        }

        if (wheelGameOverPanel != null)
            wheelGameOverPanel.SetActive(true);
    }

    void ShowWheelGameOverPanel2()
    {
        wheelChoiceActive = true;
        Time.timeScale = 0f;

        if (wheelGameOverPanel2 != null)
            wheelGameOverPanel2.SetActive(true);
    }


    public void CarCrash()
    {
        Debug.Log("CAR CRASH CALLED");
        Debug.Log("policeActive = " + policeActive);
        Debug.Log("crashPanel assigned to: " + (crashPanel != null ? crashPanel.name : "NULL"));
        Debug.Log("finePanel assigned to: " + (finePanel != null ? finePanel.name : "NULL"));

        if (gameOver || crashChoiceActive) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCrash();

        if (policeActive)
        {
            Debug.Log("POLICE ACTIVE: opening fine panel");
            EndPoliceEvent();
            ShowFinePanel(policeCrashFine, "POLICE FINE -R" + policeCrashFine);
            return;
        }

        Debug.Log("NO POLICE: opening crash panel");

        crashChoiceActive = true;
        Time.timeScale = 0f;

        if (crashPanel != null)
            crashPanel.SetActive(true);
    }

    public void PayPenalty()
    {
        if (money >= carPenalty)
        {
            RemoveMoney(carPenalty);
            crashChoiceActive = false;
            Time.timeScale = 1f;

            if (crashPanel != null)
                crashPanel.SetActive(false);
        }
        else
        {
            crashChoiceActive = false;
            Time.timeScale = 1f;

            if (crashPanel != null)
                crashPanel.SetActive(false);

            GameOver();
        }
    }

    // PAUSE / PICKUP

    public void PauseForPickup(float seconds)
    {
        if (pickupPause || crashChoiceActive || gameOver || pauseActive) return;
        StartCoroutine(PickupPauseRoutine(seconds));
    }

    IEnumerator PickupPauseRoutine(float seconds)
    {
        pickupPause = true;
        yield return new WaitForSeconds(seconds);
        pickupPause = false;
    }

    public void TogglePause()
    {
        if (gameOver || crashChoiceActive) return;

        pauseActive = !pauseActive;
        Time.timeScale = pauseActive ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(pauseActive);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
    }

    public void ResumeFromPause()
    {
        pauseActive = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

    }

    public void EndRunFromWheels()
    {
        wheelChoiceActive = false;
        Time.timeScale = 1f;

        if (wheelGameOverPanel != null)
            wheelGameOverPanel.SetActive(false);

        GameOver();
    }


    // SCENE NAVIGATION 
    public void RestartGame()
    {
        SaveMoney();
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void GoToLeaderboard()
    {
        // Make sure game over logic runs before leaving the scene
        if (!gameOver)
            GameOver();

        Time.timeScale = 1f;
        SceneManager.LoadScene("LeaderboardScene");
    }
    public void GoToStartMenu()
    {
        SaveMoney();
        Time.timeScale = 1f;

        // Resume music when returning to start
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic();

        SceneManager.LoadScene("StartScene");
    }


    // GAME OVER


    string GetCurrentAreaName()
    {
        AreaConfig config = FindObjectOfType<AreaConfig>();
        if (config == null || config.areas == null) return "Unknown";

        int index = Mathf.Clamp(RunData.selectedAreaIndex, 0, config.areas.Length - 1);
        return config.areas[index].areaName;
    }

    public void GameOver()
    {
        Debug.Log("GameOver called. gameOver was: " + gameOver
                  + " | timeScale: " + Time.timeScale
                  + " | gameOverPanel null: " + (gameOverPanel == null));
        if (gameOver) return;

        gameOver = true;
        Time.timeScale = 0f;

        CloseAllPanels();

      
        int finalMoney = money;
        int finalTime = Mathf.FloorToInt(survivalTime);
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        string areaName = GetCurrentAreaName();
        Debug.Log("GAMEOVER DEBUG — money: " + money
        + " | finalMoney: " + finalMoney
        + " | time: " + finalTime
        + " | passengers: " + totalPassengersPickedUp
        + " | player: " + playerName
        + " | area: " + areaName);
        // Save to leaderboard BEFORE saving money
        LeaderboardManager.AddEntry(playerName, finalTime, finalMoney, totalPassengersPickedUp, areaName);
        if (DailyChallenge.Instance != null)
            DailyChallenge.Instance.OnRunEnd(finalMoney, RunData.selectedAreaIndex);

        // NOW save money
        SaveMoney();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayGameOver();
        }
    }

    void CloseAllPanels()
    {
        if (crashPanel != null) crashPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (finePanel != null) finePanel.SetActive(false);
        if (dropOffPanel != null) dropOffPanel.SetActive(false);
        if (wheelGameOverPanel != null) wheelGameOverPanel.SetActive(false);
        if (wheelGameOverPanel2 != null) wheelGameOverPanel2.SetActive(false);
        if (redFlashImage != null) redFlashImage.gameObject.SetActive(false);

        crashChoiceActive = false;
        pauseActive = false;
        dropOffWarningActive = false;
        wheelChoiceActive = false;
        wheelAlreadyBought = false;
    }

    void SaveMoney()
    {
        if (moneySaved) return;
        moneySaved = true;

        if (money > 0)
        {
            PlayerProfile.TotalMoney += money;
            money = 0;
            UpdateWalletUI();
        }
    }
}

[System.Serializable]
public class PassengerRideData
{
    public int baseFare;
    public float timeSincePickup;
    public float randomDropOffDelay;
    public bool warningStarted;
    public float countdownRemaining;
}
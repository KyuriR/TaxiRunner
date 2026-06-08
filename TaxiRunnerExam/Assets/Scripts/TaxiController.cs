using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TaxiController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float horizontalLimit = 5f;

    [Header("Money Popup")]
    public MoneyPopup popupPrefab;
    public Canvas mainCanvas;

    private float moveInput;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (GameManager.Instance.pickupPause || GameManager.Instance.crashChoiceActive || GameManager.Instance.pauseActive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, 0f);

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, -horizontalLimit, horizontalLimit);
        rb.position = pos;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<float>();
    }

    public void OnDropOff(InputValue value)
    {
        if (GameManager.Instance == null) return;
        if (!value.isPressed) return;

        GameManager.Instance.TryDropOff();
    }

    public void OnPause(InputValue value)
    {
        if (GameManager.Instance == null) return;
        if (!value.isPressed) return;

        GameManager.Instance.TogglePause();
    }

    public void OnBinoculars(InputValue value)
    {
        if (GameManager.Instance == null) return;
        if (!value.isPressed) return;

        GameManager.Instance.TryUseBinoculars();
    }

    public void SetShieldVisual(bool active)
    {
        if (sr == null) return;

        sr.color = active ? Color.yellow : originalColor;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null) return;

        if (collision.gameObject.CompareTag("Car"))
        {
            if (GameManager.Instance.shieldActive)
            {
                GameManager.Instance.ConsumeShield();
                Destroy(collision.gameObject);
                return;
            }

            GameManager.Instance.CarCrash();
            Destroy(collision.gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Pothole"))
        {
            if (GameManager.Instance.shieldActive)
            {
                GameManager.Instance.ConsumeShield();
                Destroy(collision.gameObject);
                return;
            }

            GameManager.Instance.TakePotholeHit();
            Destroy(collision.gameObject);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null) return;

        if (other.CompareTag("Passenger"))
        {
            if (!GameManager.Instance.CanPickUpPassenger())
                return;

            Passenger passenger = other.GetComponent<Passenger>();
            int amount = passenger != null ? passenger.moneyValue : GameManager.Instance.basePassengerFare;

            GameManager.Instance.PickUpPassenger(amount);

            GameManager.Instance.PauseForPickup(3f);

          

            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("PowerUp"))
        {
            PowerUp powerUp = other.GetComponent<PowerUp>();

            if (powerUp != null)
            {
                if (powerUp.type == PowerUpType.Shield)
                    GameManager.Instance.ActivateShield();

                if (powerUp.type == PowerUpType.Binoculars)
                    GameManager.Instance.GiveBinoculars();
            }

            Destroy(other.gameObject);
            return;
        }
    }

    void SpawnMoneyPopup(int amount, Vector3 worldPos)
    {
        if (popupPrefab == null || mainCanvas == null) return;
        if (Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        MoneyPopup popup = Instantiate(popupPrefab, mainCanvas.transform);

        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.position = screenPos;

        popup.SetAmount(amount);
    }
}
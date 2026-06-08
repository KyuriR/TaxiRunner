using UnityEngine;

public class PoliceChase : MonoBehaviour
{
    public Transform target;

    [Tooltip("Speed at which the police car slides onto the screen initially.")]
    public float entrySpeed = 14f;

    [Tooltip("Speed at which it tracks the taxi sideways once it has entered.")]
    public float followSpeed = 6f;

    [Tooltip("How far above the taxi the police car hovers during the chase.")]
    public float verticalOffset = 2f;

    [Tooltip("Speed at which the police car drives off the top of the screen when dismissed.")]
    public float exitSpeed = 18f;

    // How close the police car needs to be (Y) before switching from entry to follow mode
    private const float EntryThreshold = 0.15f;

    private enum Phase { Entering, Following, Exiting }
    private Phase phase = Phase.Entering;

    private float targetY;

    void Start()
    {
        if (target != null)
            targetY = target.position.y + verticalOffset;

        // Disable physics so it moves purely by script
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;

        // Keep target Y up to date in case the camera/taxi moves
        if (target != null)
            targetY = target.position.y + verticalOffset;

        switch (phase)
        {
            case Phase.Entering:
                DoEnter();
                break;

            case Phase.Following:
                DoFollow();
                break;

            case Phase.Exiting:
                DoExit();
                break;
        }
    }

    // Slide downward from off-screen until we reach the hover position
    void DoEnter()
    {
        Vector3 pos = transform.position;

        // Move down toward the target Y
        pos.y = Mathf.MoveTowards(pos.y, targetY, entrySpeed * Time.deltaTime);

        // Also snap X toward the taxi so entry looks natural
        if (target != null)
            pos.x = Mathf.Lerp(pos.x, target.position.x, followSpeed * Time.deltaTime);

        transform.position = pos;

        // Switch to follow once we're close enough
        if (Mathf.Abs(pos.y - targetY) <= EntryThreshold)
            phase = Phase.Following;
    }

    // Hover above the taxi, tracking side to side
    void DoFollow()
    {
        Vector3 pos = transform.position;

        if (target != null)
            pos.x = Mathf.Lerp(pos.x, target.position.x, followSpeed * Time.deltaTime);

        pos.y = targetY;

        transform.position = pos;
    }

    // Drive off the top of the screen and self-destruct
    void DoExit()
    {
        transform.Translate(Vector3.up * exitSpeed * Time.deltaTime, Space.World);

        float screenTop = Camera.main != null
            ? Camera.main.transform.position.y + Camera.main.orthographicSize + 3f
            : transform.position.y + 20f;

        if (transform.position.y > screenTop)
            Destroy(gameObject);
    }

    // Called by GameManager.EndPoliceEvent() to send the car away
    public void StartExit()
    {
        phase = Phase.Exiting;
    }
}
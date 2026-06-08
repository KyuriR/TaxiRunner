using UnityEngine;

public class PoliceCar : MonoBehaviour
{
    public float speed = 10f;

    Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;

        // follow player X
        Vector3 target = new Vector3(player.position.x, transform.position.y, 0);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        // move down screen
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CarCrash();
        }
    }
}
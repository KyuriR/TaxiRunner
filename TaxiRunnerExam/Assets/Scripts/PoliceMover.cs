using UnityEngine;

public class PoliceMover : MonoBehaviour
{
    public float speed = 12f;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        float bottom = Camera.main.transform.position.y - Camera.main.orthographicSize - 2f;

        if (transform.position.y < bottom)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CarCrash();
            Destroy(gameObject);
        }
    }
}
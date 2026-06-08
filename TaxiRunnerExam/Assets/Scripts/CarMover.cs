using UnityEngine;

public class CarMover : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        if (Camera.main == null) return;

        float speed = GameManager.Instance.CarSpeed;

        transform.Translate(Vector3.down * speed * Time.deltaTime);

        float bottom = Camera.main.transform.position.y
                     - Camera.main.orthographicSize
                     - 1.5f; // small buffer

        if (transform.position.y < bottom)
        {
            Destroy(gameObject);
        }
    }
}
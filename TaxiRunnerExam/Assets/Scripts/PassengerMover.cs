using UnityEngine;

public class PassengerMover : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        if (Camera.main == null) return;

        transform.Translate(Vector3.down * GameManager.Instance.RoadSpeed * Time.deltaTime);

        float bottom = Camera.main.transform.position.y
                     - Camera.main.orthographicSize
                     - 1.5f;

        if (transform.position.y < bottom)
        {
            Destroy(gameObject);
        }
    }
}
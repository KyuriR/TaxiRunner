using UnityEngine;

public class PotholeMover : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        if (Camera.main == null) return;

        float speed = GameManager.Instance.RoadSpeed;

        transform.Translate(Vector3.down * speed * Time.deltaTime);

        float bottom = Camera.main.transform.position.y
                     - Camera.main.orthographicSize
                     - 1.5f;

        if (transform.position.y < bottom)
        {
            Destroy(gameObject);
        }
    }
}
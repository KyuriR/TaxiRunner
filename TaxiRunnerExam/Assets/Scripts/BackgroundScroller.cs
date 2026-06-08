using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Transform[] roadPieces;

    private float pieceHeight;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (roadPieces == null || roadPieces.Length == 0)
        {
            Debug.LogError("No road pieces assigned!");
            return;
        }

        SpriteRenderer sr = roadPieces[0].GetComponent<SpriteRenderer>();
        pieceHeight = sr.bounds.size.y;

        // Stack pieces vertically
        for (int i = 0; i < roadPieces.Length; i++)
        {
            roadPieces[i].position = new Vector3(
                roadPieces[i].position.x,
                i * pieceHeight,
                0f
            );
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float speed = GameManager.Instance.RoadSpeed;

        for (int i = 0; i < roadPieces.Length; i++)
        {
            roadPieces[i].Translate(Vector3.down * speed * Time.deltaTime, Space.World);

            // ?? THIS IS THE FIX
            float cameraBottom = cam.transform.position.y - cam.orthographicSize;

            if (roadPieces[i].position.y + pieceHeight < cameraBottom)
            {
                Transform highest = GetHighestPiece();

                roadPieces[i].position = new Vector3(
                    roadPieces[i].position.x,
                    highest.position.y + pieceHeight,
                    0f
                );
            }
        }
    }

    Transform GetHighestPiece()
    {
        Transform highest = roadPieces[0];

        for (int i = 1; i < roadPieces.Length; i++)
        {
            if (roadPieces[i].position.y > highest.position.y)
                highest = roadPieces[i];
        }

        return highest;
    }
}
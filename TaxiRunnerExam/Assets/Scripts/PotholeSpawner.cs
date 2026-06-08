using UnityEngine;

public class PotholeSpawner : MonoBehaviour
{
    public GameObject[] potholePrefabs;
    public float extraSpawnPaddingY = 2f;

    [Header("Lane Setup")]
    public float[] lanes = { -2.98f, -1f, 1.07f, 3.04f };

    [Header("Spawn Control")]
    public float minSpawnGapY = 2.5f;
    public float rowGapY = 1.2f;

    private float timer;

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        if (GameManager.Instance.crashChoiceActive) return;
        if (Camera.main == null) return;

        timer += Time.deltaTime;

        if (timer >= GameManager.Instance.GetPotholeSpawnInterval())
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        if (potholePrefabs == null || potholePrefabs.Length == 0) return;
        if (lanes == null || lanes.Length == 0) return;

        float spawnY = Camera.main.transform.position.y
                     + Camera.main.orthographicSize
                     + extraSpawnPaddingY;

        string[] checkTags = { "Car", "Pothole", "PowerUp" };

        if (SpawnHelper.IsRowTooCrowded(spawnY, rowGapY, checkTags))
            return;

        for (int tries = 0; tries < 10; tries++)
        {
            float spawnX = lanes[Random.Range(0, lanes.Length)];

            if (SpawnHelper.IsLaneBlocked(spawnX, spawnY, minSpawnGapY, checkTags))
                continue;

            GameObject prefab = potholePrefabs[Random.Range(0, potholePrefabs.Length)];

            Instantiate(prefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
            return;
        }
    }
}
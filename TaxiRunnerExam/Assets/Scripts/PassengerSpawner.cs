using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    public WeightedPrefab[] passengerPrefabs;
    public float interval = 3f;
    public float extraSpawnPaddingY = 2f;

    private readonly float[] spawnXs = { -4.95f, 5.23f };
    private float timer;

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        if (GameManager.Instance.crashChoiceActive) return;
        if (GameManager.Instance.pauseActive) return;
        if (Camera.main == null) return;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        GameObject chosen = PickWeighted(passengerPrefabs);
        if (chosen == null) return;

        float spawnY = Camera.main.orthographicSize + extraSpawnPaddingY;
        float x = spawnXs[Random.Range(0, spawnXs.Length)];

        Instantiate(chosen, new Vector3(x, spawnY, 0f), Quaternion.identity);
    }

    GameObject PickWeighted(WeightedPrefab[] list)
    {
        if (list == null || list.Length == 0) return null;

        float total = 0f;

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].prefab != null && list[i].weight > 0f)
                total += list[i].weight;
        }

        if (total <= 0f) return null;

        float roll = Random.value * total;
        float running = 0f;

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].prefab == null || list[i].weight <= 0f) continue;

            running += list[i].weight;

            if (roll <= running)
                return list[i].prefab;
        }

        return list[list.Length - 1].prefab;
    }
}
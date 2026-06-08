using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// AreaBackground.cs
//
// Spawns the correct road prefab for the selected area and hands its road
// piece Transforms to BackgroundScroller so scrolling works as normal.
//
// HOW TO SET UP IN UNITY
// ─────────────────────────────────────────────────────────────────────────────
// 1. Create an empty GameObject in GameScene called "AreaBackground".
//    Attach this script to it.
//
// 2. Create your 3 road prefabs. Each prefab should contain the same
//    number of road piece child GameObjects (with SpriteRenderers) that
//    your current BackgroundScroller uses — just with different sprites.
//    For example if BackgroundScroller currently has 3 road pieces,
//    each prefab should have 3 child GameObjects.
//
//    Name the prefabs something like:
//      RoadPrefab_Soweto
//      RoadPrefab_Sandton
//      RoadPrefab_CBD
//
// 3. In the Inspector, assign:
//      roadPrefabs[0] → RoadPrefab_Soweto
//      roadPrefabs[1] → RoadPrefab_Sandton
//      roadPrefabs[2] → RoadPrefab_CBD
//      backgroundScroller → drag in the BackgroundScroller GameObject
//
// 4. Remove the road piece assignments from BackgroundScroller in the Inspector
//    (leave the roadPieces array empty) — this script fills it at runtime.
//
// HOW IT WORKS
// ─────────────────────────────────────────────────────────────────────────────
// On Start(), it instantiates the correct prefab, collects all child
// Transforms, and assigns them to BackgroundScroller.roadPieces.
// BackgroundScroller then runs exactly as it always has.
// ─────────────────────────────────────────────────────────────────────────────

public class AreaBackground : MonoBehaviour
{
    [Tooltip("One road prefab per area — index matches RunData.selectedAreaIndex.")]
    public GameObject[] roadPrefabs;

    [Tooltip("The BackgroundScroller in this scene.")]
    public BackgroundScroller backgroundScroller;

    void Awake()
    {
        // Run in Awake so road pieces exist before BackgroundScroller.Start() fires
        SpawnRoad();
    }

    void SpawnRoad()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0)
        {
            Debug.LogError("AreaBackground: no road prefabs assigned.");
            return;
        }

        if (backgroundScroller == null)
        {
            Debug.LogError("AreaBackground: BackgroundScroller not assigned.");
            return;
        }

        int index = Mathf.Clamp(RunData.selectedAreaIndex, 0, roadPrefabs.Length - 1);
        GameObject prefab = roadPrefabs[index];

        if (prefab == null)
        {
            Debug.LogError("AreaBackground: road prefab at index " + index + " is null.");
            return;
        }

        // Spawn the road prefab at its area-specific position
        Vector3 spawnPos = GetSpawnPositionForArea(index);
        GameObject roadInstance = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Collect all direct children as road pieces
        int childCount = roadInstance.transform.childCount;

        if (childCount == 0)
        {
            Debug.LogError("AreaBackground: road prefab has no children. " +
                           "Each road piece should be a child GameObject.");
            return;
        }

        Transform[] pieces = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
            pieces[i] = roadInstance.transform.GetChild(i);

        // Hand the pieces to BackgroundScroller
        backgroundScroller.roadPieces = pieces;
    }

    Vector3 GetSpawnPositionForArea(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(-0.83f, 0f, 0f); // Soweto
            case 1: return new Vector3(-1.22f, 0f, 0f); // Sandton
            case 2: return new Vector3(-1.03f, 0f, 0f); // CBD
            default: return Vector3.zero;
        }
    }
}
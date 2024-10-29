using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSpawner : MonoBehaviour
{
    public GameObject[] shapePrefabs;     // shape prefabs to spawn
    public int shapeCount = 5;            // Number of shapes to spawn
    public float spawnPadding = 0.5f;     // space between shapes to prevent overlap
    public Transform spawnSpace;  //area where to spawn
    public Vector2 spawnArea;         // W and h if spawn space

    private List<GameObject> spawnedShapes = new List<GameObject>();

    private void Start()
    {
        SpawnShapes();
    }

    // Spawn the shapes in random positions 
    private void SpawnShapes()
    {
        int spawned = 0;
        while (spawned < shapeCount)
        {
            Vector3 spawnPosition = GetRandomPositionInYellowBox();

            // Check if this position is clear
            if (IsPositionClear(spawnPosition))
            {
                // Instantiate the shape prefab
                GameObject shape = Instantiate(shapePrefabs[Random.Range(0, shapePrefabs.Length)], spawnPosition, Quaternion.identity);
                shape.transform.parent = spawnSpace;
                spawnedShapes.Add(shape);
                spawned++;
            }
        }
    }


    // Get a random position in spawn space
    private Vector3 GetRandomPositionInYellowBox()
    {
        float x = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
        float y = Random.Range(-spawnArea.y / 2, spawnArea.y / 2);
        return spawnSpace.position + new Vector3(x, y, 0);
    }

    // Check if a position is clear of other shapes
    private bool IsPositionClear(Vector3 position)
    {
        foreach (var shape in spawnedShapes)
        {
            if (Vector3.Distance(shape.transform.position, position) < spawnPadding)
            {
                return false; // Position is too close to another shape
            }
        }
        return true; // Position is clear
    }
}
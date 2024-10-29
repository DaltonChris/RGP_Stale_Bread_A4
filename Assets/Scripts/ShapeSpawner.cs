using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSpawner : MonoBehaviour
{
    public GameObject[] shapePrefabs;     // Shape prefabs to spawn
    public int shapeCount = 5;            // Number of shapes to spawn
    public float spawnPadding = 0.5f;     // Space between shapes to prevent overlap
    public Transform spawnSpace;          // Area where to spawn
    public Vector2 spawnArea;             // w and h of spawn space
    private BoxCollider2D spawnAreaCollider; 

    private List<GameObject> spawnedShapes = new List<GameObject>();

    private void Start()
    {
        spawnAreaCollider = spawnSpace.GetComponent<BoxCollider2D>();

        SpawnShapes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Check if the spawn area is empty before respawning shapes
            if (IsSpawnAreaEmpty() && spawnedShapes.Count <= 0)
            {
                SpawnShapes();
            }
            else
            {
                Debug.Log("Spawn not empty");
            }
        }
    }

    // Spawn shapes in random positions without overlap
    private void SpawnShapes()
    {
        int spawned = 0;
        int attempts = 0;

        while (spawned < shapeCount && attempts < 100)
        {
            Vector3 spawnPosition = GetRandomPositionInSpawnSpace();
            attempts++;

            // Check if this position is clear of other shapes
            if (IsPositionClear(spawnPosition))
            {
                // Instantiate a shape prefab at the position
                GameObject shape = Instantiate(shapePrefabs[Random.Range(0, shapePrefabs.Length)], spawnPosition, Quaternion.identity);
                shape.transform.parent = spawnSpace;
                spawnedShapes.Add(shape);
                spawned++;
            }
        }

        if (spawned < shapeCount)
        {
            Debug.LogWarning("Not all shapes could be spawned without overlapping.");
        }
    }

    // Get a random position within the spawn area
    private Vector3 GetRandomPositionInSpawnSpace()
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
            if (shape != null && Vector3.Distance(shape.transform.position, position) < spawnPadding)
            {
                return false;
            }
        }
        return true;
    }

    // Check if the spawn area is empty
    private bool IsSpawnAreaEmpty()
    {
        // Use the BoxCollider2D bounds to detect any shapes in the spawn area
        Collider2D[] colliders = Physics2D.OverlapBoxAll(spawnAreaCollider.bounds.center, spawnAreaCollider.bounds.size, 0);

        // Loop through the colliders detected in the spawn area
        foreach (Collider2D col in colliders)
        {
            // If any collider belongs to a spawned shape, the area is not empty
            if (col.gameObject != spawnSpace.gameObject && col.gameObject.CompareTag("Shape"))
            {
                return false;
            }
        }
        return true;  // No shapes found, the area is empty
    }
}

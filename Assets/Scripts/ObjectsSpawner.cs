using System.Collections;
using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    public int objectsToSpawn = 1;

    [Header("Bounds")]
    [SerializeField] private float minX, maxX, minZ, maxZ;

    [Header("Time Modifiers")]
    [SerializeField] private float obstacleSpawnTime = 3f;
    [SerializeField] private float minObstacleSpawnTime = 0.5f;

    [Header("Spawn Settings")]
    [SerializeField] private float defaultY = 12f;
    [SerializeField] private int initialObstacleCount = 3;
    [SerializeField] private float minDistanceFromTrain = 20f;

    [Header("Prefabs")]
    [SerializeField] private GameObject passengersPrefab, obstaclePrefab, enemyPrefab;

    [Header("Enemy Spawning")]
    [SerializeField] private int initialEnemyCount = 1;
    [SerializeField] private float enemySpawnY = 0f;

    private TrainController _trainController;

    // Spawn a few obstacles at the start before starting the coroutine
    private void Start()
    {
        SpawnObject(passengersPrefab);

        _trainController = FindFirstObjectByType<TrainController>();

        if (_trainController == null)
        {
            Debug.LogWarning("TrainController not found in scene!");
        }

        for (int i = 0; i < initialObstacleCount; i++)
        {
            SpawnObject(obstaclePrefab);
        }

        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }

        StartCoroutine(SpawnObstacle());
    }

    public GameObject SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        return Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 spawnPosition;
        float distanceFromTrain;

        // Ensure obstacles don't spawn too close to the train (would be unfair)
        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            spawnPosition = new Vector3(randomX, defaultY, randomZ);

            if (_trainController != null)
            {
                distanceFromTrain = Vector3.Distance(spawnPosition, _trainController.transform.position);
            }
            else
            {
                distanceFromTrain = minDistanceFromTrain + 1f; // If not close to the train, allow spawning
            }
        } while (_trainController != null && distanceFromTrain < minDistanceFromTrain);

        return spawnPosition;
    }

    public void DecreaseSpawnTime(float amount)
    {
        obstacleSpawnTime = Mathf.Max(minObstacleSpawnTime, obstacleSpawnTime - amount);
    }

    private IEnumerator SpawnObstacle()
    {
        while (true)
        {
            for (int i = 0; i < objectsToSpawn; i++)
            {
                SpawnObject(obstaclePrefab);
            }
            yield return new WaitForSeconds(obstacleSpawnTime);
        }
    }

    public GameObject SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not assigned in ObjectsSpawner!");
            return null;
        }

        // Pick a random edge: 0 = left, 1 = right, 2 = bottom, 3 = top
        int edge = Random.Range(0, 4);
        float x = 0f, z = 0f;

        switch (edge)
        {
            case 0: // left
                x = minX;
                z = Random.Range(minZ, maxZ);
                break;
            case 1: // right
                x = maxX;
                z = Random.Range(minZ, maxZ);
                break;
            case 2: // bottom
                z = minZ;
                x = Random.Range(minX, maxX);
                break;
            case 3: // top
                z = maxZ;
                x = Random.Range(minX, maxX);
                break;
        }

        Vector3 spawnPos = new Vector3(x, enemySpawnY, z);
        return Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}

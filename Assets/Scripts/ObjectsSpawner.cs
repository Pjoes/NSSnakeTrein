using System.Collections;
using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    public int objectsToSpawn = 1;

    [Header("Bounds")]
    [SerializeField] private float minX = -80;
    [SerializeField] private float maxX = 80;
    [SerializeField] private float minZ = -41;
    [SerializeField] private float maxZ = 41;

    [Header("Time Modifiers")]
    [SerializeField] private float obstacleSpawnTime = 3f;
    [SerializeField] private float minObstacleSpawnTime = 0.5f;

    [Header("Spawn Settings")]
    [SerializeField] private float defaultY = 12f;
    [SerializeField] private int initialObstacleCount = 3;
    [SerializeField] private float minDistanceFromTrain = 20f;

    [Header("Prefabs")]
    [SerializeField] private GameObject passengersPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Powerups")]
    [SerializeField] private GameObject[] powerupPrefabs = new GameObject[4];
    [SerializeField] private float powerupInterval = 15f;
    [SerializeField] private float powerupSpawnY = 13f;
    private float powerupTimer = 0f;

    [Header("Enemy Spawning")]
    [SerializeField] private int initialEnemyCount = 1;
    [SerializeField] private float enemySpawnY = 0f;

    [Header("Script References")]
    [SerializeField] private TrainController _trainController;

    private const string PowerupTag = "Powerup";

    private void Start()
    {
        InitializeSpawner();
        SpawnInitialObjects();
        StartCoroutine(SpawnObstacle());
    }

    private void Update()
    {
        UpdatePowerupTimer();
    }

    private void InitializeSpawner()
    {
        _trainController = FindFirstObjectByType<TrainController>();

        if (_trainController == null)
        {
            Debug.LogWarning("TrainController not found in scene!");
        }
    }

    private void SpawnInitialObjects()
    {
        SpawnObject(passengersPrefab);

        for (int i = 0; i < initialObstacleCount; i++)
        {
            SpawnObject(obstaclePrefab);
        }

        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void UpdatePowerupTimer()
    {
        powerupTimer += Time.deltaTime;
        if (powerupTimer >= powerupInterval)
        {
            powerupTimer = 0f;
            SpawnRandomPowerup();
        }
    }

    public GameObject SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        Vector3 spawnPosition = GetRandomPosition(defaultY, SpawnMode.AvoidTrain);
        return Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    public GameObject SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not assigned in ObjectsSpawner!");
            return null;
        }

        Vector3 spawnPos = GetRandomPosition(enemySpawnY, SpawnMode.EdgesOnly);
        return Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    public void DecreaseSpawnTime(float amount)
    {
        obstacleSpawnTime = Mathf.Max(minObstacleSpawnTime, obstacleSpawnTime - amount);
    }

    public void CleanupPowerups()
    {
        GameObject[] powerups = GameObject.FindGameObjectsWithTag(PowerupTag);
        foreach (var powerup in powerups)
        {
            Destroy(powerup);
        }
    }

    private void SpawnRandomPowerup()
    {
        CleanupPowerups();

        GameObject powerupToSpawn = GetRandomPowerupPrefab();
        if (powerupToSpawn == null)
        {
            Debug.LogWarning("Powerup prefab is null");
            return;
        }

        Vector3 spawnPos = GetRandomPosition(powerupSpawnY, SpawnMode.Anywhere);
        Instantiate(powerupToSpawn, spawnPos, Quaternion.identity);
    }

    private GameObject GetRandomPowerupPrefab()
    {
        return powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];
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

    private Vector3 GetRandomPosition(float y, SpawnMode mode)
    {
        switch (mode)
        {
            case SpawnMode.AvoidTrain:
                return GetPositionAvoidingTrain(y);
            case SpawnMode.EdgesOnly:
                return GetEdgePosition(y);
            case SpawnMode.Anywhere:
            default:
                return GetAnywherePosition(y);
        }
    }

    private Vector3 GetPositionAvoidingTrain(float y)
    {
        Vector3 spawnPosition;
        float distanceFromTrain;

        do
        {
            spawnPosition = GetAnywherePosition(y);

            if (_trainController != null)
            {
                distanceFromTrain = Vector3.Distance(spawnPosition, _trainController.transform.position);
            }
            else
            {
                distanceFromTrain = minDistanceFromTrain + 1f;
            }
        } while (_trainController != null && distanceFromTrain < minDistanceFromTrain);

        return spawnPosition;
    }

    private Vector3 GetAnywherePosition(float y)
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        return new Vector3(randomX, y, randomZ);
    }

    private Vector3 GetEdgePosition(float y)
    {
        int edge = Random.Range(0, 4);
        float x, z;

        switch (edge)
        {
            case 0: // Left
                x = minX;
                z = Random.Range(minZ, maxZ);
                break;
            case 1: // Right
                x = maxX;
                z = Random.Range(minZ, maxZ);
                break;
            case 2: // Bottom
                x = Random.Range(minX, maxX);
                z = minZ;
                break;
            case 3: // Top
                x = Random.Range(minX, maxX);
                z = maxZ;
                break;
            default:
                x = minX;
                z = minZ;
                break;
        }

        return new Vector3(x, y, z);
    }

    private enum SpawnMode
    {
        Anywhere,
        EdgesOnly,
        AvoidTrain
    }
}
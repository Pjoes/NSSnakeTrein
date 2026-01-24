using System.Collections;
using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    public int objectsToSpawn = 1;

    [SerializeField] private float minX, maxX, minZ, maxZ;
    [SerializeField] private float obstacleSpawnTime = 3f;
    [SerializeField] private float minObstacleSpawnTime = 0.5f;
    [SerializeField] private float defaultY = 12f;
    [SerializeField] private int initialObstacleCount = 3;
    [SerializeField] private GameObject passengersPrefab, obstaclePrefab;
    [SerializeField] private float minDistanceFromTrain = 20f;

    private TrainController trainController;

    // Spawn a few obstacles at the start before starting the coroutine
    private void Start()
    {
        SpawnObject(passengersPrefab);

        trainController = FindFirstObjectByType<TrainController>();
        if (trainController == null)
        {
            Debug.LogWarning("TrainController not found in scene!");
        }

        for (int i = 0; i < initialObstacleCount; i++)
        {
            SpawnObject(obstaclePrefab);
        }
        StartCoroutine(SpawnObstacle());
    }

    public GameObject SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("SpawnObject called with null prefab.");
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

            if (trainController != null)
            {
                distanceFromTrain = Vector3.Distance(spawnPosition, trainController.transform.position);
                Debug.Log(distanceFromTrain);
                Debug.Log("Spawning object at: " + spawnPosition.ToString());
            }
            else
            {
                distanceFromTrain = minDistanceFromTrain + 1f; // If not close to the train, allow spawning
            }
        } while (trainController != null && distanceFromTrain < minDistanceFromTrain);

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    [SerializeField] private float minX, maxX, minZ, maxZ;
    [SerializeField] private float defaultY = 12f;
    [SerializeField] private GameObject obstaclePrefab;

    private void Start()
    {
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
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        return new Vector3(randomX, defaultY, randomZ);
    }

    private IEnumerator SpawnObstacle()
    {
        while (true)
        {
            SpawnObject(obstaclePrefab);
            yield return new WaitForSeconds(5f);
        }
    }
}

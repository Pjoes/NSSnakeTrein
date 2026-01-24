using UnityEngine;
using System.Collections;

public class ObstacleDespawner : MonoBehaviour
{
    private float maxDespawnTime = 5f, minDespawnTime = 3f;

    void Start()
    {
        StartCoroutine(DespawnObject());
    }

    private IEnumerator DespawnObject()
    {
        float despawnTime = Random.Range(minDespawnTime, maxDespawnTime);
        yield return new WaitForSeconds(despawnTime);
        Destroy(gameObject);
    }
}

using UnityEngine;
using System.Collections;

public class ObstacleDespawner : MonoBehaviour
{
    [SerializeField] private float despawnTime = 5f;

    void Start()
    {
        StartCoroutine(DespawnObject());
    }

    private IEnumerator DespawnObject()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(gameObject);
    }
}

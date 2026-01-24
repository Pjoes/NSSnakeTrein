using System.Collections;
using UnityEngine;

public class PowerupsCleaner : MonoBehaviour
{
    [SerializeField] private float cleanupDelay = 1f;
    [SerializeField] private string powerupTag = "Powerup";

    public IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(cleanupDelay);
        CleanupPowerups();
    }

    // Remove all existing powerups from the scene (maybe bad for performance on a larger scale?)
    private void CleanupPowerups()
    {
        GameObject[] powerups = GameObject.FindGameObjectsWithTag(powerupTag);
        foreach (var p in powerups)
        {
            Destroy(p);
        }
    }
}

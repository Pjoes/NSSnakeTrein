using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private int speedIncreaseThreshold = 75;
    [SerializeField] private int spawnRateIncreaseThreshold = 50;
    [SerializeField] private int spawnAmountIncreaseThreshold = 25;
    [SerializeField] private int speedIncreaseAmount = 5;
    [SerializeField] private float spawnTimeDecreaseAmount = 0.3f;

    public void IncreaseDifficulty(int score)
    {
        if (score % speedIncreaseThreshold == 0)
        {
            // Increase train speed
            TrainController trainController = FindFirstObjectByType<TrainController>();
            if (trainController != null)
            {
                trainController.IncreaseSpeed(speedIncreaseAmount);
            }
        }

        if (score % spawnRateIncreaseThreshold == 0)
        {
            // Increase obstacle spawn rate
            ObjectsSpawner objectsSpawner = FindFirstObjectByType<ObjectsSpawner>();
            if (objectsSpawner != null)
            {
                objectsSpawner.DecreaseSpawnTime(spawnTimeDecreaseAmount);
            }
        }

        if (score % spawnAmountIncreaseThreshold == 0)
        {
            ObjectsSpawner objectsSpawner = FindFirstObjectByType<ObjectsSpawner>();
            if (objectsSpawner != null)
            {
                objectsSpawner.objectsToSpawn++;
            }
        }
    }
}

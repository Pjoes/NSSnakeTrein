using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Increase Thresholds")]
    [SerializeField] private int speedIncreaseThreshold = 75;
    [SerializeField] private int spawnRateIncreaseThreshold = 50;
    [SerializeField] private int spawnAmountIncreaseThreshold = 25;
    [SerializeField] private int enemySpawnThreshold = 150;

    [Header("Difficulty Increase Values")]
    [SerializeField] private int speedIncreaseAmount = 5;
    [SerializeField] private float spawnTimeDecreaseAmount = 0.3f;

    private TrainController _trainController;
    private ObjectsSpawner _objectsSpawner;

    private void Awake()
    {
        _trainController = FindFirstObjectByType<TrainController>();
        _objectsSpawner = FindFirstObjectByType<ObjectsSpawner>();
    }

    public void IncreaseDifficulty(int score)
    {
        if (ShouldIncreaseSpeed(score))
        {
            IncreaseTrainSpeed();
        }

        if (ShouldIncreaseSpawnRate(score))
        {
            IncreaseObstacleSpawnRate();
        }

        if (ShouldIncreaseSpawnAmount(score))
        {
            IncreaseObstacleSpawnAmount();
        }

        if (ShouldSpawnEnemy(score))
        {
            SpawnNewEnemy();
        }
    }

    private bool ShouldIncreaseSpeed(int score)
    {
        return score % speedIncreaseThreshold == 0;
    }

    private bool ShouldIncreaseSpawnRate(int score)
    {
        return score % spawnRateIncreaseThreshold == 0;
    }

    private bool ShouldIncreaseSpawnAmount(int score)
    {
        return score % spawnAmountIncreaseThreshold == 0;
    }

    private bool ShouldSpawnEnemy(int score)
    {
        return score % enemySpawnThreshold == 0;
    }

    private void IncreaseTrainSpeed()
    {
        if (_trainController != null)
        {
            _trainController.IncreaseSpeed(speedIncreaseAmount);
        }
    }

    private void IncreaseObstacleSpawnRate()
    {
        if (_objectsSpawner != null)
        {
            _objectsSpawner.DecreaseSpawnTime(spawnTimeDecreaseAmount);
        }
    }

    private void IncreaseObstacleSpawnAmount()
    {
        if (_objectsSpawner != null)
        {
            _objectsSpawner.objectsToSpawn++;
        }
    }

    private void SpawnNewEnemy()
    {
        if (_objectsSpawner != null)
        {
            _objectsSpawner.SpawnEnemy();
        }
    }
}
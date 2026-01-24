using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    [Header("Train Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float steerSpeed = 180f;

    [Header("Car Spacing")]
    [SerializeField] private float gap = 75f;
    [SerializeField] private float gapDecreaseAmount = 7.5f;
    [SerializeField] private float maximumGapSize = 75f;
    [SerializeField] private float minimumGapSize = 30f;
    [SerializeField] private int firstCarGap = 20;
    [SerializeField] private int initialCars = 3;

    [Header("Prefabs")]
    [SerializeField] private GameObject carPrefab, passengersPrefab, gameOverScreen;

    [Header("Tags")]
    [SerializeField] private string passengersTag = "Passengers", obstacleTag = "Obstacle", carTag = "Car";

    private ScoreManager _scoreManager;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private int scorePerPassenger = 25;

    private float lastFoodTime = -1f;
    private bool isGameOver = false;

    // Expose current move speed for other systems (e.g., enemy prediction)
    public float CurrentSpeed => moveSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _scoreManager = FindFirstObjectByType<ScoreManager>();

        // Simulate adding an initial car to fix the gap between the locomotive and the first car
        int requiredHistoryLength = firstCarGap + Mathf.RoundToInt(initialCars * gap);
        for (int i = 0; i < requiredHistoryLength; i++)
        {
            positionsHistory.Add(transform.position);
            rotationHistory.Add(transform.rotation);
        }

        for (int i = 0; i < initialCars; i++)
        {
            GrowTrain();
        }
    }

    private void Update()
    {
        // Ensure cars stop moving when the game is over
        if (isGameOver)
            return;

        // Move forward
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Steer
        float steerDirection = Input.GetAxis("Horizontal"); // Returns value -1, 0, or 1
        transform.Rotate(Vector3.up * steerDirection * steerSpeed * Time.deltaTime);

        // Store position history
        positionsHistory.Insert(0, transform.position);
        rotationHistory.Insert(0, transform.rotation);

        // Move cars
        for (int index = 0; index < cars.Count; index++)
        {
            GameObject car = cars[index];
            int historyIndex = CalculateHistoryIndex(index);

            Vector3 point = positionsHistory[historyIndex];
            car.transform.position = point;

            // Rotate car using the head's recorded rotation at the same path index
            car.transform.rotation = rotationHistory[historyIndex];
        }
    }

    // Adjust the gap for the first car
    private int CalculateHistoryIndex(int carIndex)
    {
        int rawIndex = (carIndex == 0)
            ? firstCarGap
            : firstCarGap + Mathf.RoundToInt(carIndex * gap);

        return Mathf.Clamp(rawIndex, 0, positionsHistory.Count - 1);
    }

    // Increase train size by adding a new car to the end
    private void GrowTrain()
    {
        int newCarIndex = cars.Count;
        int historyIndex = CalculateHistoryIndex(newCarIndex);

        // Get spawn position and rotation from history
        Vector3 spawnPos = positionsHistory[historyIndex];
        Quaternion spawnRot = rotationHistory[historyIndex];

        GameObject car = Instantiate(carPrefab, spawnPos, spawnRot);
        cars.Add(car);
    }

    public void IncreaseSpeed(int amount)
    {
        moveSpeed += amount;
        gap -= gapDecreaseAmount;
        Mathf.Clamp(gap, minimumGapSize, maximumGapSize);
    }

    // Display game over screen and pause the game
    private void GameOver()
    {
        isGameOver = true;

        _scoreManager.ManageFinalScore();
        gameOverScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    // Check for collisions with passengers and obstacles
    private void OnTriggerEnter(Collider other)
    {
        // Increase train length and score when picking up passengers
        if (other.gameObject.CompareTag(passengersTag) && Time.time - lastFoodTime > 0.1f)
        {
            GrowTrain();
            _scoreManager.AddScore(scorePerPassenger);
            Destroy(other.gameObject);

            ObjectsSpawner spawner = FindFirstObjectByType<ObjectsSpawner>();

            if (spawner != null)
            {
                spawner.SpawnObject(passengersPrefab);
            }
            lastFoodTime = Time.time;
        }
        if (other.gameObject.CompareTag(obstacleTag) || other.gameObject.CompareTag(carTag))
        {
            Debug.Log("Hit Obstacle! Game Over.");
            GameOver();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float steerSpeed = 180f;
    [SerializeField] private int gap = 10;
    [SerializeField] private int firstCarGap = 20;
    [SerializeField] private int initialCars = 3;
    [SerializeField] private GameObject carPrefab, passengersPrefab, gameOverScreen;

    private ScoreManager _scoreManager;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private int scorePerPassenger = 25;

    private float lastFoodTime = -1f;
    private bool isGameOver = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _scoreManager = FindFirstObjectByType<ScoreManager>();

        // Simulate adding an initial car to fix the gap between the locomotive and the first car
        for (int i = 0; i < firstCarGap + (initialCars * gap); i++)
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
        int index = 0;
        foreach (var car in cars)
        {
            // First car has a larger gap, rest use normal gap
            int historyIndex;
            if (index == 0)
            {
                historyIndex = Mathf.Clamp(firstCarGap, 0, positionsHistory.Count - 1);
            }
            else
            {
                historyIndex = Mathf.Clamp(firstCarGap + (index * gap), 0, positionsHistory.Count - 1);
            }

            Vector3 point = positionsHistory[historyIndex];

            // Place car exactly at the recorded path point for tight spacing
            Vector3 moveDirection = point - car.transform.position;
            car.transform.position = point;

            // Rotate car using the head's recorded rotation at the same path index
            if (rotationHistory.Count > historyIndex)
            {
                car.transform.rotation = rotationHistory[historyIndex];
            }

            index++;
        }
    }

    private void GrowTrain()
    {
        // Calculate where this new car should spawn in the history -> should be at the end of the current cars
        int newCarIndex = cars.Count;
        int historyIndex = Mathf.Clamp(firstCarGap + (newCarIndex * gap), 0, positionsHistory.Count - 1);

        // Get spawn position and rotation from history
        Vector3 spawnPos = positionsHistory.Count > historyIndex ? positionsHistory[historyIndex] : transform.position;
        Quaternion spawnRot = rotationHistory.Count > historyIndex ? rotationHistory[historyIndex] : transform.rotation;

        GameObject car = Instantiate(carPrefab, spawnPos, spawnRot);
        cars.Add(car);
    }

    private void UpdateScore(int scoreToAdd)
    {
        _scoreManager.score += scoreToAdd;
    }

    private void GameOver()
    {
        isGameOver = true;

        _scoreManager.DisplayFinalScore();
        gameOverScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f; // Pause the game
    }

    private void OnTriggerEnter(Collider other)
    {
        // Increase train length and score when picking up passengers
        if (other.gameObject.CompareTag("Passengers") && Time.time - lastFoodTime > 0.1f)
        {
            GrowTrain();
            UpdateScore(scorePerPassenger);
            Destroy(other.gameObject);

            ObjectsSpawner spawner = FindFirstObjectByType<ObjectsSpawner>();

            if (spawner != null)
            {
                spawner.SpawnObject(passengersPrefab);
            }
            lastFoodTime = Time.time;
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Hit Obstacle! Game Over.");
            GameOver();
        }
    }
}
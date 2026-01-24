using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    [Header("Train Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float steerSpeed = 180f;
    [SerializeField] private float secondsToActivateDamageHitbox = 2f;

    [Header("Car Spacing")]
    [SerializeField] private float gap = 75f;
    [SerializeField] private float gapDecreaseAmount = 7.5f;
    [SerializeField] private float maximumGapSize = 75f;
    [SerializeField] private float minimumGapSize = 30f;
    [SerializeField] private int firstCarGap = 20;
    [SerializeField] private int initialCars = 3;

    [Header("Prefabs")]
    [SerializeField] private GameObject carPrefab, passengersPrefab, gameOverScreen;

    [Header("Hitboxes")]
    [SerializeField] private GameObject damageHitbox;

    [Header("Powerups")]
    public GameObject armourVisual;
    public int health = 1;
    public int maxHealth = 2;

    private ScoreManager _scoreManager;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private int scorePerPassenger = 25;

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

        ToggleArmourVisual();

        // Activate damage hitbox after a delay to prevent player immediately dying upon level start
        StartCoroutine(ActivateDamageHitbox());
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

    // Updates health and checks for game over
    public void UpdateHealth(int amount)
    {
        health += amount;
        Debug.Log(health);
        health = Mathf.Clamp(health, 0, maxHealth);

        ToggleArmourVisual();
        GameOver();
    }

    private void ToggleArmourVisual()
    {

        if (health > 1)
        {
            armourVisual.SetActive(true);
        }
        else
        {
            armourVisual.SetActive(false);
        }
    }

    // Display game over screen and pause the game
    private void GameOver()
    {
        if (health <= 0)
        {
            isGameOver = true;

            _scoreManager.ManageFinalScore();
            gameOverScreen.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }

    // Called by PickupHitbox when hit by passengers
    public void PickupPassenger(GameObject passengerObject)
    {
        GrowTrain();
        _scoreManager.AddScore(scorePerPassenger);
        Destroy(passengerObject);

        ObjectsSpawner _spawner = FindFirstObjectByType<ObjectsSpawner>();

        if (_spawner != null)
        {
            _spawner.SpawnObject(passengersPrefab);
        }
    }

    private IEnumerator ActivateDamageHitbox()
    {
        yield return new WaitForSeconds(secondsToActivateDamageHitbox);
        damageHitbox.SetActive(true);
    }
}
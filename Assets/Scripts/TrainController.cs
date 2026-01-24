using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    [Header("Train Movement")]
    public float moveSpeed = 20f;
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
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject passengersPrefab;

    [Header("Hitboxes")]
    [SerializeField] private GameObject damageHitbox;
    [SerializeField] private GameObject pickupHitbox;
    [SerializeField] private GameObject enlargedHitboxVisual;

    [Header("Powerups")]
    public GameObject armourVisual;
    public int health = 1;
    public int maxHealth = 2;

    [Header("UI")]
    [SerializeField] private GameObject gameOverScreen;

    private ScoreManager _scoreManager;
    private Vector3 _pickupHitboxBaseSize;
    private bool _pickupHitboxBaseSizeInitialized = false;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private int scorePerPassenger = 25;
    private float hitboxEnlargedDuration = 10f;
    private bool isGameOver = false;

    // Expose current move speed for other systems
    public float CurrentSpeed => moveSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _scoreManager = FindFirstObjectByType<ScoreManager>();

        // Cache the base size of the pickup hitbox for clamped scaling
        if (pickupHitbox != null)
        {
            BoxCollider col = pickupHitbox.GetComponent<BoxCollider>();
            if (col != null)
            {
                _pickupHitboxBaseSize = col.size;
                _pickupHitboxBaseSizeInitialized = true;
            }
        }

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

    public void ApplyBrake(float brakeSpeed, float recoveryDuration)
    {
        StartCoroutine(BrakeAndRecover(brakeSpeed, recoveryDuration));
    }

    public void RemoveLastCars(int count)
    {
        for (int i = 0; i < count && cars.Count > 0; i++)
        {
            GameObject carToRemove = cars[cars.Count - 1];
            cars.RemoveAt(cars.Count - 1);
            Destroy(carToRemove);
        }
    }

    public void EnlargePickupHitbox(float multiplier)
    {
        if (!_pickupHitboxBaseSizeInitialized || pickupHitbox == null)
            return;

        BoxCollider collider = pickupHitbox.GetComponent<BoxCollider>();
        if (collider == null)
            return;

        if (enlargeHitboxCoroutine != null)
        {
            StopCoroutine(enlargeHitboxCoroutine);
        }

        enlargeHitboxCoroutine = StartCoroutine(EnlargeAndRestoreHitbox(collider, multiplier));
    }

    private Coroutine enlargeHitboxCoroutine = null;

    // Store original hitbox size and revert after powerup duration runs out
    private IEnumerator EnlargeAndRestoreHitbox(BoxCollider collider, float multiplier)
    {
        // Clamp multiplier to max 3x and min 1x
        float clampedMultiplier = Mathf.Clamp(multiplier, 1f, 3f);

        // Apply immediately
        collider.size = _pickupHitboxBaseSize * clampedMultiplier;

        if (enlargedHitboxVisual != null)
        {
            enlargedHitboxVisual.SetActive(true);
        }

        yield return new WaitForSeconds(hitboxEnlargedDuration);

        // Restore
        collider.size = _pickupHitboxBaseSize;
        if (enlargedHitboxVisual != null)
        {
            enlargedHitboxVisual.SetActive(false);
        }

        enlargeHitboxCoroutine = null;
    }

    // Recover original speed after hitting brake powerup (placed here because the powerup gets destroyed on pickup)
    private IEnumerator BrakeAndRecover(float brakeSpeed, float recoveryDuration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = brakeSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < recoveryDuration)
        {
            elapsedTime += Time.deltaTime;
            float recoveryProgress = elapsedTime / recoveryDuration;
            moveSpeed = Mathf.Lerp(brakeSpeed, originalSpeed, recoveryProgress);
            yield return null;
        }

        moveSpeed = originalSpeed;
    }
}
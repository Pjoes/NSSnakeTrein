using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    [Header("Train Movement")]
    public float moveSpeed = 20f;
    [SerializeField] private float steerSpeed = 180f;
    [SerializeField] private float secondsToActivateDamageHitbox = 2f;

    /* DEFAULT VALUES FOR TRAIN CARS AND SPAWNING
    Gap = 30f
    Maximum Gap Size = 30f
    Minimum Gap Size = 20f
    Gap Decrease Amount = 5f
    First Car Gap = 15f
    */

    [Header("Car Spacing")]
    [SerializeField] private float gap = 30f;
    [SerializeField] private float gapDecreaseAmount = 5f;
    [SerializeField] private float maximumGapSize = 30f;
    [SerializeField] private float minimumGapSize = 20f;
    [SerializeField] private float firstCarGap = 15f;
    [SerializeField] private int initialCars = 3;
    private float trainCarY = 9f;

    [Header("Bounds")]
    [SerializeField] private float minX = -85;
    [SerializeField] private float maxX = 85;
    [SerializeField] private float minZ = -50;
    [SerializeField] private float maxZ = 50;

    [Header("Prefabs")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject passengersPrefab;

    [Header("Hitboxes")]
    [SerializeField] private GameObject damageHitbox;
    [SerializeField] private GameObject pickupHitbox;

    [Header("Powerup Visuals")]
    [SerializeField] private List<PowerupVisual> powerupVisuals = new List<PowerupVisual>();
    [SerializeField] private float uncoupleVisualDuration = 1.5f;
    private bool shouldBeActive = true;

    [Header("Health")]
    public int health = 1;
    public int maxHealth = 2;
    [SerializeField] private float damageInvulnerableDuration = 1f;

    [Header("UI")]
    [SerializeField] private GameObject gameOverScreen;

    [Header("Sounds")]
    [SerializeField] private float trainMoveSoundDuration = 0.975f;
    private float trainMoveSoundTimer = 0f;

    private ScoreManager _scoreManager;
    private Vector3 _pickupHitboxBaseSize;
    private bool _pickupHitboxBaseSizeInitialized = false;
    private bool _isDamageInvulnerable = false;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private readonly Dictionary<PowerupVisualType, GameObject> _powerupVisualLookup = new Dictionary<PowerupVisualType, GameObject>();
    private readonly Dictionary<PowerupVisualType, Coroutine> _powerupVisualTimers = new Dictionary<PowerupVisualType, Coroutine>();

    private int scorePerPassenger = 25;
    private float hitboxEnlargedDuration = 10f;
    private Coroutine enlargeHitboxCoroutine = null;

    private bool isGameOver = false;

    // Expose current move speed for other systems
    public float CurrentSpeed => moveSpeed;

    private void Awake()
    {
        CachePowerupVisuals();
    }

    private void Start()
    {
        // Lock frame rate to 60 FPS for consistent behavior in editor and build
        Application.targetFrameRate = 60;

        Cursor.lockState = CursorLockMode.Locked;

        InitializePowerupVisualStates();

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
        float requiredHistoryLength = firstCarGap + Mathf.RoundToInt(initialCars * gap);
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

        // Play train move sound at intervals
        trainMoveSoundTimer -= Time.deltaTime;
        if (trainMoveSoundTimer <= 0f)
        {
            SoundManager.PlaySound(SoundType.TRAINMOVE, 1f);
            trainMoveSoundTimer = trainMoveSoundDuration;
        }

        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        float steerDirection = Input.GetAxis("Horizontal"); // Returns value -1, 0, or 1
        transform.Rotate(Vector3.up * steerDirection * steerSpeed * Time.deltaTime);

        PreventOutOfBounds();

        positionsHistory.Insert(0, transform.position);
        rotationHistory.Insert(0, transform.rotation);

        // Trim history to prevent infinite growth
        float maxHistoryLength = firstCarGap + Mathf.RoundToInt((cars.Count + 10) * gap);
        if (positionsHistory.Count > maxHistoryLength)
        {
            positionsHistory.RemoveRange((int)maxHistoryLength, positionsHistory.Count - (int)maxHistoryLength);
            rotationHistory.RemoveRange((int)maxHistoryLength, rotationHistory.Count - (int)maxHistoryLength);
        }

        // Move cars
        for (int index = 0; index < cars.Count; index++)
        {
            GameObject car = cars[index];
            int historyIndex = CalculateHistoryIndex(index);

            Vector3 point = positionsHistory[historyIndex];
            point.y = trainCarY;
            car.transform.position = point;
            car.transform.rotation = rotationHistory[historyIndex];
        }
    }

    // Adjust the gap for the first car
    private int CalculateHistoryIndex(int carIndex)
    {
        float rawIndex = (carIndex == 0)
            ? firstCarGap
            : firstCarGap + Mathf.RoundToInt(carIndex * gap);

        return Mathf.Clamp((int)rawIndex, 0, positionsHistory.Count - 1);
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
        gap = Mathf.Clamp(gap, minimumGapSize, maximumGapSize);
    }

    // Updates health and checks for game over
    public void UpdateHealth(int amount)
    {
        Debug.Log($"[TrainController] UpdateHealth called: amount={amount}, current health={health}, invulnerable={_isDamageInvulnerable}");

        // Ignore consecutive hits during brief invulnerability window
        if (amount < 0 && _isDamageInvulnerable)
            return;

        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);

        Debug.Log($"[TrainController] Health after change: {health}");

        if (amount < 0 && !_isDamageInvulnerable)
        {
            StartCoroutine(DamageInvulnerability());
        }

        ToggleArmourVisual();

        if (health <= 0)
        {
            Debug.Log("[TrainController] Health <= 0, calling GameOver");
            GameOver();
        }
    }

    private void ToggleArmourVisual()
    {
        SetPowerupVisual(PowerupVisualType.Armour, health > 1);
    }

    // Display game over screen and pause the game
    private void GameOver()
    {
        Debug.Log($"[TrainController] GameOver() called. isGameOver={isGameOver}, health={health}");

        if (health <= 0)
        {
            isGameOver = true;

            _scoreManager.ManageFinalScore();
            gameOverScreen.SetActive(true);

            SoundManager.PlaySound(SoundType.GAMEOVER, 1f);

            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;

            Debug.Log("[TrainController] Game Over complete");
        }
    }

    // Called by PickupHitbox when hit by passengers
    public void PickupPassenger(GameObject passengerObject)
    {
        GrowTrain();
        SoundManager.PlaySound(SoundType.PASSENGERPICKUP, 0.5f);
        _scoreManager.AddScore(scorePerPassenger);
        Destroy(passengerObject);

        ObjectsSpawner _spawner = FindFirstObjectByType<ObjectsSpawner>();

        if (_spawner != null)
        {
            _spawner.SpawnObject(passengersPrefab);
        }
    }

    // Activate damage hitbox after a delay to prevent immediate death on level start
    private IEnumerator ActivateDamageHitbox()
    {
        yield return new WaitForSeconds(secondsToActivateDamageHitbox);
        damageHitbox.SetActive(true);
    }

    // Apply brake effect and recover original speed over time
    public void ApplyBrake(float brakeSpeed, float recoveryDuration)
    {
        StartCoroutine(BrakeAndRecover(brakeSpeed, recoveryDuration));
    }

    // Uncouple last N cars from the train
    public void RemoveLastCars(int count)
    {
        bool removedAnyCars = false;
        for (int i = 0; i < count && cars.Count > 0; i++)
        {
            GameObject carToRemove = cars[cars.Count - 1];
            cars.RemoveAt(cars.Count - 1);
            StartCoroutine(UncoupleCarDeletion(carToRemove));
            removedAnyCars = true;
        }

        if (removedAnyCars)
        {
            SetPowerupVisual(PowerupVisualType.Uncouple, shouldBeActive, uncoupleVisualDuration);
        }
    }

    // Enlarge the pickup hitbox for a short duration
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

    // Ensure player cannot leave the playable area with extra health
    private void PreventOutOfBounds()
    {
        bool isOutOfBounds = false;

        if (transform.position.x < minX || transform.position.x > maxX)
        {
            isOutOfBounds = true;
            Debug.LogWarning($"[TrainController] OUT OF BOUNDS - X: {transform.position.x} (min={minX}, max={maxX})");
        }

        if (transform.position.z < minZ || transform.position.z > maxZ)
        {
            isOutOfBounds = true;
            Debug.LogWarning($"[TrainController] OUT OF BOUNDS - Z: {transform.position.z} (min={minZ}, max={maxZ})");
        }

        if (isOutOfBounds)
        {
            Debug.LogError($"[TrainController] Train went out of bounds! Position: {transform.position}. Setting health to 0.");
            health = 0;
            GameOver();
        }
    }

    // Store original hitbox size and revert after powerup duration runs out
    private IEnumerator EnlargeAndRestoreHitbox(BoxCollider collider, float multiplier)
    {
        // Clamp multiplier to max 3x and min 1x
        float clampedMultiplier = Mathf.Clamp(multiplier, 1f, 3f);

        // Apply immediately
        collider.size = _pickupHitboxBaseSize * clampedMultiplier;
        SetPowerupVisual(PowerupVisualType.Magnet, shouldBeActive, hitboxEnlargedDuration);

        yield return new WaitForSeconds(hitboxEnlargedDuration);

        // Restore
        collider.size = _pickupHitboxBaseSize;

        enlargeHitboxCoroutine = null;
    }

    // Recover original speed after hitting brake powerup (placed here because the powerup gets destroyed on pickup)
    private IEnumerator BrakeAndRecover(float brakeSpeed, float recoveryDuration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = brakeSpeed;

        SetPowerupVisual(PowerupVisualType.Brake, shouldBeActive, recoveryDuration);

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

    // Brief invulnerability after taking damage to prevent multiple hits in quick succession
    private IEnumerator DamageInvulnerability()
    {
        _isDamageInvulnerable = true;
        yield return new WaitForSeconds(damageInvulnerableDuration);
        _isDamageInvulnerable = false;
    }

    // Destroy uncoupled cars after 1 second
    private IEnumerator UncoupleCarDeletion(GameObject car)
    {
        if (car == null)
            yield break;

        yield return new WaitForSeconds(1f);

        if (car != null)
        {
            Destroy(car);
        }
    }

    private void CachePowerupVisuals()
    {
        _powerupVisualLookup.Clear();
        foreach (PowerupVisual entry in powerupVisuals)
        {
            if (entry.visual == null)
                continue;

            // Ensure visual follows train transform
            if (entry.visual.transform.parent != transform)
            {
                entry.visual.transform.SetParent(transform, true);
            }

            _powerupVisualLookup[entry.type] = entry.visual;
        }
    }

    private void InitializePowerupVisualStates()
    {
        foreach (KeyValuePair<PowerupVisualType, GameObject> kvp in _powerupVisualLookup)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(false);
            }
        }

        _powerupVisualTimers.Clear();
    }

    private void SetPowerupVisual(PowerupVisualType type, bool isActive, float autoDisableAfterSeconds = -1f)
    {
        if (!_powerupVisualLookup.TryGetValue(type, out GameObject visual) || visual == null)
            return;

        if (_powerupVisualTimers.TryGetValue(type, out Coroutine timer) && timer != null)
        {
            StopCoroutine(timer);
        }

        visual.SetActive(isActive);
        _powerupVisualTimers[type] = null;

        if (isActive && autoDisableAfterSeconds > 0f)
        {
            _powerupVisualTimers[type] = StartCoroutine(DisableVisualAfterDelay(type, autoDisableAfterSeconds));
        }
    }

    private IEnumerator DisableVisualAfterDelay(PowerupVisualType type, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPowerupVisual(type, false);
    }



    // Enum for powerup visual types
    private enum PowerupVisualType
    {
        Armour,
        Magnet,
        Uncouple,
        Brake
    }

    // Struct to link powerups with their visuals
    [System.Serializable]
    private struct PowerupVisual
    {
        public PowerupVisualType type;
        public GameObject visual;
    }
}
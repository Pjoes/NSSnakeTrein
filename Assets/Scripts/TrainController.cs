using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float steerSpeed = 180f;
    [SerializeField] private float carFollowSpeed = 15f;
    [SerializeField] private int gap = 10;
    [SerializeField] private int firstCarGap = 20;
    [SerializeField] private int initialCars = 3;
    [SerializeField] private GameObject carPrefab;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();

    private int score = 0;
    private float lastFoodTime = -1f;

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // Pre-populate position history so GrowTrain() can access it
        for (int i = 0; i < firstCarGap + (initialCars * gap); i++)
        {
            positionsHistory.Add(transform.position);
        }

        for (int i = 0; i < initialCars; i++)
        {
            GrowTrain();
        }
    }

    private void Update()
    {
        // Move forward
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Steer
        float steerDirection = Input.GetAxis("Horizontal"); // Returns value -1, 0, or 1
        transform.Rotate(Vector3.up * steerDirection * steerSpeed * Time.deltaTime);

        // Store position history
        positionsHistory.Insert(0, transform.position);

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

            // Move car directly to the point with minimal interpolation for tighter following
            Vector3 moveDirection = point - car.transform.position;
            float distance = moveDirection.magnitude;

            // Use higher speed when further away to catch up faster
            float dynamicSpeed = carFollowSpeed * (1f + distance * 0.5f);
            car.transform.position += moveDirection.normalized * Mathf.Min(dynamicSpeed * Time.deltaTime, distance);

            // Rotate car towards the point along the train's path
            if (moveDirection.magnitude > 0.01f)
            {
                car.transform.LookAt(point);
            }

            index++;
        }
    }

    private void GrowTrain()
    {
        // Calculate where this new car should spawn in the history
        int newCarIndex = cars.Count;
        int historyIndex = Mathf.Clamp(firstCarGap + (newCarIndex * gap), 0, positionsHistory.Count - 1);

        // Get spawn position and rotation from history
        Vector3 spawnPos = positionsHistory.Count > historyIndex ? positionsHistory[historyIndex] : transform.position;
        Quaternion spawnRot = Quaternion.identity;

        GameObject car = Instantiate(carPrefab, spawnPos, spawnRot);
        cars.Add(car);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food") && Time.time - lastFoodTime > 0.1f)
        {
            Debug.Log("Food!");
            GrowTrain();
            score += 10;
            Debug.Log(score);
            Destroy(other.gameObject);
            lastFoodTime = Time.time;
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Hit Obstacle! Game Over.");
            // Implement game over logic here
        }
    }
}
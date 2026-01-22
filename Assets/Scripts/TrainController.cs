using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    // Settings
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float steerSpeed = 180f;
    [SerializeField] private float carFollowSpeed = 15f;
    [SerializeField] private int gap = 10;
    [SerializeField] private int firstCarGap = 20;
    [SerializeField] private float trainY = 10.18f;

    // References
    [SerializeField] private GameObject carPrefab;

    // Lists
    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, trainY, transform.position.z);

        GrowTrain();
        GrowTrain();
        GrowTrain();
    }

    private void Update()
    {
        // Move forward
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Keep y at fixed height
        Vector3 pos = transform.position;
        pos.y = trainY;
        transform.position = pos;

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
        GameObject car = Instantiate(carPrefab);
        cars.Add(car);
    }
}

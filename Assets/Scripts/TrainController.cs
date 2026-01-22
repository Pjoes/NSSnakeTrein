using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    public Vector3 direction = Vector3.forward;

    [SerializeField] private GameObject carPrefab;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float trainY = 10.18f;
    [SerializeField] private float carSpacing = 1f;
    [SerializeField] private float turnCooldown = 0.2f;

    private Vector3 nextDirection;
    private Quaternion targetRotation;
    private float lastTurnTime;

    private List<GameObject> cars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationsHistory = new List<Quaternion>();

    private readonly Quaternion forwardRotation = Quaternion.identity;
    private readonly Quaternion backRotation = Quaternion.Euler(0, 180, 0);
    private readonly Quaternion leftRotation = Quaternion.Euler(0, 270, 0);
    private readonly Quaternion rightRotation = Quaternion.Euler(0, 90, 0);

    private void Start()
    {
        nextDirection = direction;
        targetRotation = forwardRotation;
        lastTurnTime = -turnCooldown;
        transform.position = new Vector3(transform.position.x, trainY, transform.position.z);

        GrowTrain();
        GrowTrain();
        GrowTrain();
    }

    private void Update()
    {
        if (Time.time < lastTurnTime + turnCooldown)
            return;

        // Only allow turning left/right while moving forward/backward
        if (direction == Vector3.forward || direction == Vector3.back)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                nextDirection = Vector3.left;
                lastTurnTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                nextDirection = Vector3.right;
                lastTurnTime = Time.time;
            }
        }
        // Only allow turning forward/backward while moving left/right
        else if (direction == Vector3.left || direction == Vector3.right)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                nextDirection = Vector3.forward;
                lastTurnTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                nextDirection = Vector3.back;
                lastTurnTime = Time.time;
            }
        }
    }

    private void FixedUpdate()
    {
        // Update direction and set target rotation if it changed
        if (nextDirection != direction)
        {
            direction = nextDirection;
            RotateToDirection(direction);
        }

        // Smoothly rotate towards target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Move smoothly based on time
        float moveDistance = (speed * speedMultiplier) * Time.fixedDeltaTime;
        transform.position += direction * moveDistance;

        // Keep y at 10.18
        Vector3 pos = transform.position;
        pos.y = 10.18f;
        transform.position = pos;

        // Store current position and rotation
        positionsHistory.Insert(0, transform.position);
        rotationsHistory.Insert(0, transform.rotation);

        // Update each car to follow at its respective distance
        for (int i = 0; i < cars.Count; i++)
        {
            int historyIndex = Mathf.Min(i * 30 + 5, positionsHistory.Count - 1); // First car is 5 frames back, then 30 apart
            Vector3 carPos = positionsHistory[historyIndex];
            carPos.y = trainY;
            cars[i].transform.position = carPos;
            cars[i].transform.rotation = rotationsHistory[historyIndex];
        }

        // Clean up old history to avoid memory issues
        if (positionsHistory.Count > 1000)
        {
            positionsHistory.RemoveAt(positionsHistory.Count - 1);
            rotationsHistory.RemoveAt(rotationsHistory.Count - 1);
        }
    }

    private void RotateToDirection(Vector3 dir)
    {
        if (dir == Vector3.forward)
            targetRotation = forwardRotation;
        else if (dir == Vector3.back)
            targetRotation = backRotation;
        else if (dir == Vector3.left)
            targetRotation = leftRotation;
        else // Vector3.right
            targetRotation = rightRotation;
    }

    private void GrowTrain()
    {
        GameObject car = Instantiate(carPrefab);
        cars.Add(car);
    }
}

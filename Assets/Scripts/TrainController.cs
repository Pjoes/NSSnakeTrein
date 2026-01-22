using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainController : MonoBehaviour
{
    public Vector3 direction = Vector3.forward;

    [SerializeField] private float speed = 20f;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float trainY = 10.18f;
    [SerializeField] private float turnCooldown = 0.2f;

    private Vector3 nextDirection;
    private Quaternion targetRotation;
    private float lastTurnTime;

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
}

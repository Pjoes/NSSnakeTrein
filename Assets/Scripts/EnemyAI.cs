using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private TrainController train;
    [SerializeField] private float predictionTime = 2f;

    [Header("Movement")]
    [SerializeField] private float lungeSpeed = 60f;
    [SerializeField] private float lungeDuration = 1.5f;
    [SerializeField] private float lungeCooldown = 3f;
    [SerializeField] private float spawnY = 0f;

    [Header("Play Area Bounds")]
    [SerializeField] private float minX = -80f;
    [SerializeField] private float maxX = 80f;
    [SerializeField] private float minZ = -41f;
    [SerializeField] private float maxZ = 41f;

    private Vector3 lungeDirection;
    private float lungeTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isLunging = false;

    private void Start()
    {
        if (train == null)
        {
            train = FindFirstObjectByType<TrainController>();
        }

        SpawnOnEdge();
        cooldownTimer = lungeCooldown; // wait briefly before first lunge
    }

    private void Update()
    {
        if (train == null)
            return;

        if (isLunging)
        {
            // Advance along the precomputed lunge vector
            transform.position += lungeDirection * lungeSpeed * Time.deltaTime;
            lungeTimer -= Time.deltaTime;
            if (lungeTimer <= 0f)
            {
                isLunging = false;
                cooldownTimer = lungeCooldown;
            }
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            PrepareLunge();
        }
    }

    private void PrepareLunge()
    {
        // Predict where the train head will be after predictionTime seconds (simple linear projection)
        float speed = train.CurrentSpeed;
        Vector3 predictedPosition = train.transform.position + train.transform.forward * speed * predictionTime;

        lungeDirection = (predictedPosition - transform.position).normalized;
        lungeTimer = lungeDuration;
        isLunging = true;
    }

    private void SpawnOnEdge()
    {
        // Pick a random edge: 0=left,1=right,2=bottom,3=top
        int edge = Random.Range(0, 4);
        float x = 0f, z = 0f;

        switch (edge)
        {
            case 0: // left
                x = minX;
                z = Random.Range(minZ, maxZ);
                break;
            case 1: // right
                x = maxX;
                z = Random.Range(minZ, maxZ);
                break;
            case 2: // bottom
                z = minZ;
                x = Random.Range(minX, maxX);
                break;
            case 3: // top
                z = maxZ;
                x = Random.Range(minX, maxX);
                break;
        }

        transform.position = new Vector3(x, spawnY, z);
    }
}

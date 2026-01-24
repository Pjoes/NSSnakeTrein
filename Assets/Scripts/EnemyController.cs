using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private TrainController train;
    [SerializeField] private float predictionTime = 2f;

    [Header("Movement")]
    [SerializeField] private float lungeSpeed = 60f;
    [SerializeField] private float lungeCooldown = 3f;
    [SerializeField] private float spawnY = 0f;
    [SerializeField] private float overshoot = 5f;

    [Header("Play Area Bounds")]
    [SerializeField] private float minX = -80f;
    [SerializeField] private float maxX = 80f;
    [SerializeField] private float minZ = -41f;
    [SerializeField] private float maxZ = 41f;

    private Vector3 lungeDirection;
    private Vector3 lungeEndPoint;
    private float cooldownTimer = 0f;
    private bool isLunging = false;

    private void Start()
    {
        if (train == null)
        {
            train = FindFirstObjectByType<TrainController>();
        }

        cooldownTimer = lungeCooldown; // wait briefly before first lunge
    }

    private void Update()
    {
        if (train == null)
            return;

        if (isLunging)
        {
            // Move towards precomputed exit point beyond bounds
            float step = lungeSpeed * Time.deltaTime;
            Vector3 toEnd = lungeEndPoint - transform.position;
            if (toEnd.sqrMagnitude <= step * step)
            {
                transform.position = lungeEndPoint;
                isLunging = false;
                cooldownTimer = lungeCooldown;
            }
            else
            {
                // Only move horizontally
                Vector3 delta = new Vector3(lungeDirection.x, 0f, lungeDirection.z) * step;
                transform.position += delta;

                // Ensure enemy stops lunging if it goes too far
                if (IsOutsideExpandedBounds(transform.position))
                {
                    isLunging = false;
                    cooldownTimer = lungeCooldown;
                }
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

        Vector3 rawDir = predictedPosition - transform.position;
        rawDir.y = 0f;
        if (rawDir.sqrMagnitude < 1e-4f)
        {
            // Fallback: use train forward projected to XZ
            Vector3 f = train.transform.forward; f.y = 0f; rawDir = f.sqrMagnitude > 1e-4f ? f : Vector3.forward;
        }
        lungeDirection = rawDir.normalized;
        lungeEndPoint = ComputeExitPoint(transform.position, lungeDirection);
        isLunging = true;
    }

    private Vector3 ComputeExitPoint(Vector3 start, Vector3 dir)
    {
        // Compute intersection with expanded bounds (overshoot beyond edges)
        float exMinX = minX - overshoot;
        float exMaxX = maxX + overshoot;
        float exMinZ = minZ - overshoot;
        float exMaxZ = maxZ + overshoot;

        Vector3 bestPoint = start;
        float bestT = float.PositiveInfinity;

        bool InRange(float v, float a, float b) => v >= Mathf.Min(a, b) && v <= Mathf.Max(a, b);

        // X planes
        if (Mathf.Abs(dir.x) > 1e-4f)
        {
            float txMax = (exMaxX - start.x) / dir.x;
            if (txMax > 0f)
            {
                Vector3 p = start + dir * txMax;
                if (InRange(p.z, exMinZ, exMaxZ) && txMax < bestT)
                {
                    bestT = txMax; bestPoint = p;
                }
            }
            float txMin = (exMinX - start.x) / dir.x;
            if (txMin > 0f)
            {
                Vector3 p = start + dir * txMin;
                if (InRange(p.z, exMinZ, exMaxZ) && txMin < bestT)
                {
                    bestT = txMin; bestPoint = p;
                }
            }
        }

        // Z planes
        if (Mathf.Abs(dir.z) > 1e-4f)
        {
            float tzMax = (exMaxZ - start.z) / dir.z;
            if (tzMax > 0f)
            {
                Vector3 p = start + dir * tzMax;
                if (InRange(p.x, exMinX, exMaxX) && tzMax < bestT)
                {
                    bestT = tzMax; bestPoint = p;
                }
            }
            float tzMin = (exMinZ - start.z) / dir.z;
            if (tzMin > 0f)
            {
                Vector3 p = start + dir * tzMin;
                if (InRange(p.x, exMinX, exMaxX) && tzMin < bestT)
                {
                    bestT = tzMin; bestPoint = p;
                }
            }
        }

        // Fallback: if no intersection found (shouldn't happen), go far along direction
        if (!float.IsFinite(bestT))
        {
            bestPoint = start + dir * 1000f;
        }

        return new Vector3(bestPoint.x, spawnY, bestPoint.z);
    }

    private bool IsOutsideExpandedBounds(Vector3 p)
    {
        float exMinX = minX - overshoot;
        float exMaxX = maxX + overshoot;
        float exMinZ = minZ - overshoot;
        float exMaxZ = maxZ + overshoot;
        return p.x < exMinX || p.x > exMaxX || p.z < exMinZ || p.z > exMaxZ;
    }
}

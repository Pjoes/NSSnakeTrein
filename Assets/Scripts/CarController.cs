using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("Scoring")]
    [SerializeField] private int scoreToDeduct = 25;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(obstacleTag))
        {
            // Ensure only one car gets destroyed
            Destroy(other.gameObject);

            Destroy(gameObject);

            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(-scoreToDeduct);
            }
        }
    }
}

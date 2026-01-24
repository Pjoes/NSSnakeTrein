using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private string obstacleTag = "Obstacle";
    [SerializeField] private int scoreToDeduct = 25;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(obstacleTag))
        {
            Debug.Log("Car has collided with an obstacle!");
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

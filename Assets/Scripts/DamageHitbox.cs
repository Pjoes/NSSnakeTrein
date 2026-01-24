using UnityEngine;

public class DamageHitbox : MonoBehaviour
{
    [SerializeField] private string obstacleTag = "Obstacle";
    [SerializeField] private string carTag = "Car";

    private int damage = 1;
    private TrainController _trainController;

    private void Start()
    {
        _trainController = GetComponentInParent<TrainController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(obstacleTag) || other.gameObject.CompareTag(carTag))
        {
            if (_trainController != null)
            {
                _trainController.UpdateHealth(-damage);
            }
        }
    }
}

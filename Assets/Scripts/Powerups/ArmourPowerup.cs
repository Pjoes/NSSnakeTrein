using UnityEngine;

public class ArmourPowerup : MonoBehaviour
{
    private int armourValue = 1;

    private void ApplyArmour(Collider other)
    {
        TrainController _trainController = other.GetComponent<TrainController>();
        if (_trainController != null)
        {
            _trainController.UpdateHealth(armourValue);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyArmour(other);
            Destroy(gameObject);
        }
    }
}

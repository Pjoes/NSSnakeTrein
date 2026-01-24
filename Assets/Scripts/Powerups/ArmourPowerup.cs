using UnityEngine;

public class ArmourPowerup : Powerup
{
    private int armourValue = 1;
    private string pickupHitboxTag = "PickupHitbox";

    protected override void DoMainAction()
    {
        TrainController _trainController = FindFirstObjectByType<TrainController>();
        Debug.Log(_trainController);
        if (_trainController != null)
        {
            _trainController.UpdateHealth(armourValue);
            Debug.Log("Applied armour! Current Health: " + _trainController.health);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(pickupHitboxTag))
        {
            DoMainAction();
            Debug.Log("Hit train!");
            Destroy(gameObject);
        }
    }
}
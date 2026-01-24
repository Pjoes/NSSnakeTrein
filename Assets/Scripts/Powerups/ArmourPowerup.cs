using UnityEngine;

public class ArmourPowerup : Powerup
{
    private int armourValue = 1;
    private string pickupHitboxTag = "PickupHitbox";

    protected override void DoMainAction()
    {
        FindTrainController();
        if (_trainController != null)
        {
            _trainController.UpdateHealth(armourValue);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(pickupHitboxTag))
        {
            DoMainAction();
            Destroy(gameObject);
        }
    }
}
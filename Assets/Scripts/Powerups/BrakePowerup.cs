using UnityEngine;

public class BrakePowerup : Powerup
{
    [SerializeField] private float brakeSpeed = 10f;
    [SerializeField] private float recoveryDuration = 5f;
    [SerializeField] private string pickupHitboxTag = "PickupHitbox";

    protected override void DoMainAction()
    {
        FindTrainController();
        if (_trainController != null)
        {
            _trainController.ApplyBrake(brakeSpeed, recoveryDuration);
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

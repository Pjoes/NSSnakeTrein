using UnityEngine;

public class UncouplePowerup : Powerup
{
    [SerializeField] private int carsToUncouple = 2;
    [SerializeField] private string pickupHitboxTag = "PickupHitbox";

    protected override void DoMainAction()
    {
        FindTrainController();
        if (_trainController != null)
        {
            _trainController.RemoveLastCars(carsToUncouple);
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

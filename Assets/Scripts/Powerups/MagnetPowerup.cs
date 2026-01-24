using UnityEngine;

public class MagnetPowerup : Powerup
{
    [SerializeField] private float sizeMultiplier = 3f;
    [SerializeField] private string pickupHitboxTag = "PickupHitbox";

    protected override void DoMainAction()
    {
        FindTrainController();
        if (_trainController != null)
        {
            _trainController.EnlargePickupHitbox(sizeMultiplier);
            Debug.Log("Increasing hitbox size by " + sizeMultiplier + "x");
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

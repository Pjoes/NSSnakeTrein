using UnityEngine;

public class PickupHitbox : MonoBehaviour
{
    [SerializeField] private string passengersTag = "Passengers", powerupTag = "Powerup";

    private TrainController trainController;
    private float lastPickupTime = -1f;
    private float pickupCooldown = 0.1f;

    private void Start()
    {
        trainController = GetComponentInParent<TrainController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(passengersTag) && Time.time - lastPickupTime > pickupCooldown)
        {
            if (trainController != null)
            {
                trainController.PickupPassenger(other.gameObject);
            }
            lastPickupTime = Time.time;
        }
        else if (other.gameObject.CompareTag(powerupTag))
        {
            if (trainController != null)
            {
                SoundManager.PlaySound(SoundType.ITEMPICKUP, 0.7f);
            }
        }
    }
}

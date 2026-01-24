using UnityEngine;

public abstract class Powerup : MonoBehaviour
{
    protected TrainController _trainController;
    protected abstract void OnTriggerEnter(Collider other);

    // Put DoMainAction() into a separate function in case any of these functions ever need to be called later
    // Set to public in that case
    protected abstract void DoMainAction();

    protected void FindTrainController()
    {
        _trainController = FindFirstObjectByType<TrainController>();
    }
}

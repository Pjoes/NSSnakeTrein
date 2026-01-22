using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float turnSpeed = 360f; // turn slowly instead of snapping
    [SerializeField] private int gap = 75;
    [SerializeField] private int initialSpawnedCars = 2;

    private Vector3 _moveDir = Vector3.forward;
    private Quaternion _rotationOffset;
    private Quaternion _targetRot;

    public GameObject trainCar;
    private List<GameObject> trainCars = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();

    private void Awake()
    {
        // model is messed up, so calculate an offset to keep it upright
        _rotationOffset = Quaternion.Inverse(Quaternion.LookRotation(Vector3.forward, Vector3.up)) * transform.rotation;
        _targetRot = transform.rotation;
    }

    private void Start()
    {
        int neededHistory = initialSpawnedCars * gap + 1;
        for (int i = 0; i < neededHistory; i++)
        {
            positionsHistory.Add(transform.position);
        }

        for (int i = 0; i < initialSpawnedCars; i++)
        {
            GrowTrain();
        }
    }


    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // only allow 90 degree turns
        if (Mathf.Abs(_moveDir.z) > 0.001f)
        {
            // if moving up/down, allow left/right turn
            if (Mathf.Abs(h) > 0.1f)
            {
                _moveDir = new Vector3(Mathf.Sign(h), 0f, 0f);
                _targetRot = Quaternion.LookRotation(_moveDir, Vector3.up) * _rotationOffset;
            }
        }
        else if (Mathf.Abs(_moveDir.x) > 0.001f)
        {
            // if moving left/right, allow up/down turn
            if (Mathf.Abs(v) > 0.1f)
            {
                _moveDir = new Vector3(0f, 0f, Mathf.Sign(v));
                _targetRot = Quaternion.LookRotation(_moveDir, Vector3.up) * _rotationOffset;
            }
        }

        transform.position += _moveDir * moveSpeed * Time.deltaTime;

        // smoothly rotate like a real train
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRot, turnSpeed * Time.deltaTime);

        // store locomotive position history
        positionsHistory.Insert(0, transform.position);

        // move train cars
        int index = 0;
        foreach (var car in trainCars)
        {
            Vector3 point = positionsHistory[Mathf.Min(index * gap, positionsHistory.Count - 1)];
            Vector3 moveDirection = point - car.transform.position;
            car.transform.position += moveDirection * moveSpeed * Time.deltaTime;

            car.transform.LookAt(point);
            Vector3 euler = car.transform.eulerAngles;
            car.transform.rotation = Quaternion.Euler(-90f, euler.y, euler.z);

            index++;
        }
    }

    private void GrowTrain()
    {
        GameObject car = Instantiate(trainCar);
        trainCars.Add(car);
        Debug.Log("Growing train!");
    }
}

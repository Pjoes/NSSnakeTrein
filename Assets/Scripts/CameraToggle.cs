using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    [Header("Camera Positions")]
    [SerializeField] private Transform topDownPosition;
    [SerializeField] private Transform firstPersonPosition;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    [SerializeField] private float transitionSpeed = 5f;

    private bool isTopDown = true;

    private void Start()
    {
        if (topDownPosition == null || firstPersonPosition == null)
        {
            Debug.LogError("CameraToggle: Camera positions not assigned!");
            enabled = false;
            return;
        }

        // Start in top-down view
        transform.position = topDownPosition.position;
        transform.rotation = topDownPosition.rotation;
    }

    private void Update()
    {
        // Toggle between views
        if (Input.GetKeyDown(toggleKey))
        {
            isTopDown = !isTopDown;
        }
    }

    private void LateUpdate()
    {
        // Update camera position after all other updates
        if (isTopDown)
        {
            transform.position = Vector3.Lerp(transform.position, topDownPosition.position, transitionSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, topDownPosition.rotation, transitionSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, firstPersonPosition.position, transitionSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, firstPersonPosition.rotation, transitionSpeed * Time.deltaTime);
        }
    }
}

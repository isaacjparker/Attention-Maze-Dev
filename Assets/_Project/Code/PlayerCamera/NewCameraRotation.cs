using UnityEngine;

public class NewCameraRotation : MonoBehaviour
{
    public MazeSettings settings;
    public Transform player;  // Reference to the player object
    public Vector3 positionOffset = Vector3.zero; // Offset from the player position
    public float rotationSpeed = 100f;  // Speed at which the camera rotates
    public float returnSpeed = 2f;
    public float maxRotationAngle = 45f;  // Maximum rotation in degrees

    private float currentRotation = 0f;  // Tracks the camera's current local rotation relative to the player

    void Start()
    {
        // Initialize the offset
        positionOffset = transform.position - player.position;
    }

    void Update()
    {
        RotateCamera();
    }

    void LateUpdate()
    {
        // Update the camera's position based on the player's position and offset
        transform.position = player.position + Quaternion.Euler(0f, player.eulerAngles.y, 0f) * positionOffset;
    }

    /*
    void RotateCamera()
    {

        float input = 0f;

        // Only allow input if manually controlling view
        if (settings.ControlView == true)
        {
            // Get input from the A and D keys
            input = Input.GetAxis("Horizontal"); // -1 for A, 1 for D
        }

        float rotationAmount = input * rotationSpeed * Time.deltaTime;

        // Adjust the current rotation based on user input
        currentRotation += rotationAmount;

        // Clamp the current rotation
        currentRotation = Mathf.Clamp(currentRotation, -maxRotationAngle, maxRotationAngle);

        // Smoothly adjust the currentRotation back towards zero if no input
        if (Mathf.Approximately(input, 0f))
        {
            currentRotation = Mathf.MoveTowards(currentRotation, 0f, returnSpeed * Time.deltaTime);
        }

        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.Euler(0f, player.eulerAngles.y + currentRotation, 0f);

        // Apply the rotation to the camera
        transform.rotation = targetRotation;
    }
    */

    void RotateCamera()
    {
        float input = 0f;

        if (settings.ControlView)
        {
            // Get input from A and D keys
            input = Input.GetAxis("Horizontal"); // -1 for A, 1 for D
        }

        // Only adjust the head rotation with input

        if (!Mathf.Approximately(input, 0f))
        {
            float rotationAmount = input * rotationSpeed * Time.deltaTime;
            currentRotation += rotationAmount;
            currentRotation = Mathf.Clamp(currentRotation, -maxRotationAngle, maxRotationAngle);
        }
        // OPTIONAL: If you want to auto-center the head, use this block.
        // Otherwise, comment it out if you want the head to stay wherever the player last looked.
        else
        {
            //currentRotation = Mathf.MoveTowards(currentRotation, 0f, returnSpeed * Time.deltaTime);
        }

        // Always rotate the camera to the body's heading + head yaw offset
        Quaternion targetRotation = Quaternion.Euler(0f, player.eulerAngles.y + currentRotation, 0f);
        transform.rotation = targetRotation;
    }
}

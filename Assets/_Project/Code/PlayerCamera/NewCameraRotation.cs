using UnityEngine;

public class NewCameraRotation : MonoBehaviour
{
    //public MazeSettings settings;
    public Transform player;  // Reference to the player object
    public float returnSpeed = 2f;
    public float maxRotationAngle = 45f;  // Maximum rotation in degrees

    private Vector3 positionOffset = Vector3.zero; // Offset from the player position
    private float currentRotation = 0f;  // Tracks the camera's current local rotation relative to the player

    private bool manualLookEnabled = true;
    private float rotationSpeed = 100f;

    // Runtime gate from Director (don’t read input until Running)
    private bool isRunning = false;

    private void OnEnable()
    {
        // Pull config now or subscribe if not yet ready
        if (Director.Instance != null && Director.Instance.IsSetupReady)
        {
            ApplyConfig();
        }
        else if (Director.Instance != null)
        {
            Director.Instance.OnApplicationSetup += ApplyConfig;
        }

        if (Director.Instance != null && Director.Instance.IsRunReady)
        {
            BeginRun();
        }
        else if (Director.Instance != null)
        {
            Director.Instance.OnApplicationRun += BeginRun;
        }
        if (Director.Instance != null)
        {
            Director.Instance.OnApplicationEnd += EndRun;
        }
    }

    private void OnDisable()
    {
        if (Director.Instance != null)
        {
            Director.Instance.OnApplicationSetup -= ApplyConfig;
            Director.Instance.OnApplicationRun -= BeginRun;
            Director.Instance.OnApplicationEnd -= EndRun;
        }
            
    }

    void Start()
    {
        // Initialize the offset
        if (player != null)
            positionOffset = transform.position - player.position;
    }

    void Update()
    {
        if (!isRunning) return;
        RotateCamera();
    }

    void LateUpdate()
    {
        // Keep following the body even outside Running (safe & visually stable)
        if (player == null) return;
        transform.position = player.position + Quaternion.Euler(0f, player.eulerAngles.y, 0f) * positionOffset;
    }

    void RotateCamera()
    {
        float input = 0f;

        if (manualLookEnabled)
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

    private void ApplyConfig()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationSetup -= ApplyConfig;

        if (ConfigService.Instance == null)
            return;

        manualLookEnabled = (ConfigService.Instance != null) ? ConfigService.Instance.ManualLook : manualLookEnabled;
        rotationSpeed = (ConfigService.Instance != null) ? ConfigService.Instance.LookSpeed : rotationSpeed;
    }

    private void BeginRun()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationRun -= BeginRun;

        isRunning = true;
    }

    private void EndRun()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationEnd -= EndRun;

        isRunning = false;
    }
}

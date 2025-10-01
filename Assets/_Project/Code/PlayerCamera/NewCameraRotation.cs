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

    private bool returnToCenter = false;
    private KeyCode keyLookLeft = KeyCode.A;
    private KeyCode keyLookRight = KeyCode.D;

    // Runtime gate from Director (don’t read input until Running)
    private bool isRunning = false;

    private void OnEnable()
    {
        
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

        // Initialize the offset
        if (player != null)
            positionOffset = transform.position - player.position;
    }

    void Update()
    {
        if (!isRunning) return;
        if (!manualLookEnabled) return;
        RotateCamera();
    }

    void LateUpdate()
    {
        // Keep following the body even outside Running (safe & visually stable)
        if (player == null) return;
        transform.position = player.position + Quaternion.Euler(0f, player.eulerAngles.y, 0f) * positionOffset;

        // Guide camera view around corners
        // Rotation: body heading + clamped yaw offset
        transform.rotation = Quaternion.Euler(0f, player.eulerAngles.y + currentRotation, 0f);
    }

    void RotateCamera()
    {
        float input = Input.GetAxis("Horizontal");

        if (Mathf.Abs(input) < 0.0001f)
        {
            if (keyLookLeft != KeyCode.None && Input.GetKey(keyLookLeft)) input -= 1f;
            if (keyLookRight != KeyCode.None && Input.GetKey(keyLookRight)) input += 1f;
        }

        if (Mathf.Abs(input) > 0.0001f)
        {
            // Manual yaw offset in deg/sec
            float delta = input * rotationSpeed * Time.deltaTime;
            currentRotation = Mathf.Clamp(currentRotation + delta, -maxRotationAngle, maxRotationAngle);
        }
        else if (returnToCenter && returnSpeed > 0f)
        {
            // Recenter toward 0 at returnSpeed deg/sec
            currentRotation = Mathf.MoveTowards(currentRotation, 0f, returnSpeed * Time.deltaTime);
        }
    }

    private void ApplyConfig()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationSetup -= ApplyConfig;

        if (ConfigService.Instance == null)
            return;

        manualLookEnabled = ConfigService.Instance.ManualLook;
        rotationSpeed = ConfigService.Instance.LookSpeed;

        maxRotationAngle = ConfigService.Instance.MaxViewAngle;       // clamp to ±max
        returnToCenter = ConfigService.Instance.ReturnViewToCenter; // enable/disable recenter
        returnSpeed = ConfigService.Instance.ReturnViewSpeed;    // deg/sec toward center
        keyLookLeft = ConfigService.Instance.KeyLookLeft;
        keyLookRight = ConfigService.Instance.KeyLookRight;
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

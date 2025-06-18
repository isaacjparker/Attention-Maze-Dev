using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mover : MonoBehaviour
{
    public MazeSettings settings;
    public CheckpointManager checkpointManager;
    public float speed = 5f;           // Movement speed
    public float rotationSpeed = 5f;   // Rotation speed
    public float checkpointRadius = 0.5f;  // Distance to checkpoint before rotating towards the next one
    //public float checkpointTriggerDistance = 0.3f;

    private Checkpoint nextCheckpoint;
    private int nextCheckpointIndex = 0;    // Next checkpoint index
    private float distanceToNextCheckpoint;
    private CharacterController controller;  // Reference to the CharacterController
    private bool canMove = true;      // Flag to control when to stop moving

    private void OnEnable()
    {
        // If Maze is reset at runtime, checkpoint index must also be reset
        nextCheckpointIndex = 0;
        canMove = true;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (checkpointManager == null)
        {
            Debug.LogError("No Checkpoint Manager provided.");
        }

        if (checkpointManager.Checkpoints == null || checkpointManager.Checkpoints.Count == 0)
        {
            Debug.LogError("No checkpoint assigned.");
            canMove = false;  // Disable movement if no checkpoint
        }

    }

    void Update()
    {
        // Get the next checkpoint from index
        nextCheckpoint = checkpointManager.Checkpoints[nextCheckpointIndex];

        CheckDistanceToCheckpoint();

        // Only move is movement is enabled
        if (canMove == false) return;

        // Move if the experiment is set to manual move and participant is pressing W
        if (settings.ControlMovement == true && Input.GetKey(KeyCode.W))
        {
            MoveAlongCheckpoints();
        }

        // Move automatically if manual move is off
        if (settings.ControlMovement == false)
        {
            MoveAlongCheckpoints();
        }
    }

    void CheckDistanceToCheckpoint()
    {
        distanceToNextCheckpoint = DistanceToPosition(transform.position, nextCheckpoint.transform.position);

        // Do nothing until within check distance
        if (distanceToNextCheckpoint >= checkpointRadius) return;

        Debug.Log("Checkpoint within radius");

        // If this is the last checkpoint, stop moving
        if (nextCheckpointIndex == checkpointManager.Checkpoints.Count - 1)
        {
            Debug.Log("Stopping movement");
            canMove = false;  // Stop movement
            return;
        }

        // Otherwise, increment index to begin rotating towards the next checkpoint
        if (nextCheckpointIndex < checkpointManager.Checkpoints.Count - 1)
        {
            checkpointManager.ActivateCheckpoint(nextCheckpointIndex);
            // Move to the next checkpoint
            nextCheckpointIndex++;
            Debug.Log("Checkpoint index incremented");
        }
    }

    void MoveAlongCheckpoints()
    {

        // Calculate the direction to the next checkpoint
        Vector3 direction = (nextCheckpoint.transform.position - transform.position).normalized;

        // Move the character towards the current checkpoint
        controller.Move(direction * speed * Time.deltaTime);

        // Smoothly rotate towards the next checkpoint (anticipating the next one)
        RotateTowardsNextCheckpoint(nextCheckpointIndex);

    }

    private float DistanceToPosition(Vector3 origin, Vector3 target)
    { 
        return Vector3.Distance(origin, target);
    }

    void RotateTowardsNextCheckpoint(int checkpointIndex)
    {
        // Get the next checkpoint if available, or the current one if it's the last checkpoint
        Checkpoint targetCheckpoint = GetNextCheckpoint(checkpointIndex);

        // Calculate the direction towards the next checkpoint
        Vector3 directionToNextCheckpoint = (targetCheckpoint.transform.position - transform.position).normalized;

        // Create the target rotation based on that direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToNextCheckpoint);

        // Smoothly rotate the character towards the next checkpoint
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Checkpoint GetNextCheckpoint(int checkpointIndex)
    { 
        return checkpointManager.Checkpoints[Mathf.Min(checkpointIndex + 1, checkpointManager.Checkpoints.Count - 1)];
    }
}

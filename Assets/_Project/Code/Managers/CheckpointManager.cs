using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public enum CheckpointType
{ 
    START,
    STRAIGHT,
    CORNER,
    END
}

/// <summary>
/// Registers every Checkpoint in the prefab hierarchy and publishes an
/// EventKind when one is activated. Networking/CSV is delegated to
/// TelemtryManager.
/// </summary>
[DisallowMultipleComponent]
public class CheckpointManager : MonoBehaviour
{
    // ------------------------------------------------------------------  
    // Public data — Mover (and other scripts) can read this list.
    // ------------------------------------------------------------------  


    [Tooltip("Populated automatically from child objects in Awake()")]
    public List<Checkpoint> Checkpoints = new List<Checkpoint>(); // List of Checkpoints to populate

    // Optional singleton getter (stay consistent with previous pattern)
    public static CheckpointManager Instance { get; private set; }

    // ------------------------------------------------------------------  
    // Initialisation
    // ------------------------------------------------------------------  

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Gather every Checkpoint component under this manager
        Checkpoints.Clear();
        Checkpoints.AddRange(GetComponentsInChildren<Checkpoint>(true));

        if (Checkpoints.Count == 0)
            Debug.LogError($"CheckpointManager on {name} found no Checkpoint components.");
    }

    // ------------------------------------------------------------------  
    // Called by Mover when the player reaches a checkpoint.
    // ------------------------------------------------------------------  


    public void ActivateCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex < 0 || checkpointIndex >= Checkpoints.Count)
        {
            Debug.LogError($"Checkpoint index {checkpointIndex} out of range.");
            return;
        }

        Checkpoint checkpoint = Checkpoints[checkpointIndex];
        EventKind kind = checkpoint.EventKind;

        // 1) Fire Telemtry
        TelemetryManager.Instance?.PublishCheckpoint(kind);


    }

}

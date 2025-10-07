using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    public static Director Instance { get; private set; }

    public bool IsSetupReady { get; private set; }
    public bool IsRunReady { get; private set; }

    public event Action OnApplicationSetup;
    public event Action OnApplicationStart;
    public event Action OnApplicationRun;
    public event Action OnApplicationEnd;

    private bool _setupEmitted = false;
    private bool _runEmitted = false;
    private bool _endEmitted = false;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsSetupReady = false;
        IsRunReady = false;
    }

    // Temporary signal here. Need to listen for ConfigService.Instance.OnResolved
    // Then wait for correct time && ConfigService.Instance.IsResolved to invoke the
    // Correct state. This should probably act as small state machine.
    private void OnEnable()
    {
        
        
    }

    private void Start()
    {
        // ConfigService has to be separate from other logic
        // So have a setup event waiting for COnfig to be ready
        if (ConfigService.Instance != null && ConfigService.Instance.IsResolved == true)
        {
            SetupApplication();
        }
        else if (ConfigService.Instance != null)
        {
            ConfigService.Instance.OnResolved += SetupApplication;
        }

        // Then when the maze sends the end of maze signal, we invoke
        // OnApplicationEnd for the end dialogue
    }

    private void OnDisable()
    {
        if (ConfigService.Instance != null)
            ConfigService.Instance.OnResolved -= SetupApplication;
    }

    private void SetupApplication()
    {
        if (ConfigService.Instance != null)
            ConfigService.Instance.OnResolved -= SetupApplication;

        IsSetupReady = true;
        _setupEmitted = true;

        OnApplicationSetup?.Invoke();

        // No start dialogue yet; go straight to run for this phase of work.
        //StartApplication();
        RunApplication();
    }

    private void StartApplication()
    {
        if (!_setupEmitted) return;
        OnApplicationStart?.Invoke();
    }

    private void RunApplication()
    {
        if (_runEmitted) return;
        if (!_setupEmitted) return; // Must happen after setup
        
        IsRunReady = true;
        _runEmitted = true;

        OnApplicationRun?.Invoke();
    }

    private void EndApplication()
    {
        if (_endEmitted)
        {
            Debug.Log("End already emitted");
            return;
        }

        if (!_runEmitted)
        {
            Debug.Log("Trying to end but run never emitted.");
            return;
        }

        Debug.Log("Director emitting end");
        _endEmitted = true;
        OnApplicationEnd?.Invoke();
    }

    // ---- Public triggers for other systems (UI/Checkpoints) ----

    /// <summary>
    /// Call this when you want to begin the running phase (e.g., when start UI completes).
    /// Safe to call multiple times; it will emit once.
    /// </summary>
    public void TriggerRun()
    {
        RunApplication();
    }

    /// <summary>
    /// Call this when the maze is complete (e.g., CheckpointManager notifies).
    /// Safe to call multiple times; it will emit once.
    /// </summary>
    public void TriggerEnd()
    {
        EndApplication();
    }
}

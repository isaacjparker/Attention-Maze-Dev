using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects when the user presses the space bar and tells TelemetryManager
/// to publish a Spacebar event, if ManualLogging is enabled.
/// </summary>
[DisallowMultipleComponent]
public class AttentionLogger : MonoBehaviour
{
    // Cache from ConfigService so we don't look it up every frame
    private bool manualLoggingEnabled = true;
    private bool autoLoggingEnabled = false;
    private float autoLoggingInterval = 1f;

    private bool isRunning = false;

    // Coroutine handle so we can stop/restart cleanly
    private Coroutine autoLogRoutine = null;

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
            

        if (autoLogRoutine != null)
        {
            StopCoroutine(autoLogRoutine);
            autoLogRoutine = null;
        }

        isRunning = false;
    }

    private void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        if (!isRunning || !manualLoggingEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Fire the telemtry event.
            TelemetryManager.Instance?.PublishSpaceBar();
        }
    }

    private void ApplyConfig()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationSetup -= ApplyConfig;

        if (ConfigService.Instance == null)
            return;

        manualLoggingEnabled = (ConfigService.Instance != null) ? ConfigService.Instance.ManualLogging : manualLoggingEnabled;
        autoLoggingEnabled = (ConfigService.Instance != null) ? ConfigService.Instance.AutoLogging : autoLoggingEnabled;
        autoLoggingInterval = (ConfigService.Instance != null) ? ConfigService.Instance.AutoLoggingIntervalSec : autoLoggingInterval;

        if (isRunning)
        {
            RestartAutoLoopIfNeeded();
        }

    }

    private void BeginRun()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationRun -= BeginRun;

        isRunning = true;
        RestartAutoLoopIfNeeded();
    }

    private void EndRun()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationEnd -= EndRun;

        isRunning = false;

        if (autoLogRoutine != null)
        { 
            StopCoroutine(autoLogRoutine);
            autoLogRoutine = null;
        }
    }

    // ---- Auto logging ----

    private void RestartAutoLoopIfNeeded()
    {
        // Stop existing loop first
        if (autoLogRoutine != null)
        {
            StopCoroutine(autoLogRoutine);
            autoLogRoutine = null;
        }

        // Start only if we're in Running phase and auto logging is enabled with a positive interval
        if (isRunning && autoLoggingEnabled && autoLoggingInterval > 0f)
        {
            autoLogRoutine = StartCoroutine(AutoLogLoop());
        }
    }

    private IEnumerator AutoLogLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(autoLoggingInterval);

        while (true)
        {
            TelemetryManager.Instance?.PublishSpaceBar();

            yield return wait;
        }
    }

}

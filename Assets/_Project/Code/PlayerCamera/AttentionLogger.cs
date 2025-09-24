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

    // Coroutine handle so we can stop/restart cleanly
    private Coroutine autoLogRoutine = null;

    private void OnEnable()
    {
        if (Director.Instance != null && Director.Instance.IsReady)
        {
            ApplyConfig();
        }
        else if (Director.Instance != null)
        {
            Director.Instance.ApplyConfig += ApplyConfig;
        }
    }

    private void OnDisable()
    {
        if (Director.Instance != null)
            Director.Instance.ApplyConfig -= ApplyConfig;

        if (autoLogRoutine != null)
        {
            StopCoroutine(autoLogRoutine);
            autoLogRoutine = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!manualLoggingEnabled)
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
            Director.Instance.ApplyConfig -= ApplyConfig;

        manualLoggingEnabled = (ConfigService.Instance != null) ? ConfigService.Instance.ManualLogging : manualLoggingEnabled;
        autoLoggingEnabled = (ConfigService.Instance != null) ? ConfigService.Instance.AutoLogging : autoLoggingEnabled;
        autoLoggingInterval = (ConfigService.Instance != null) ? ConfigService.Instance.AutoLoggingIntervalSec : autoLoggingInterval;

        // Manage auto-logging loop
        if (autoLogRoutine != null)
        {
            StopCoroutine(autoLogRoutine);
            autoLogRoutine = null;
        }

        if (autoLoggingEnabled && autoLoggingInterval > 0f)
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

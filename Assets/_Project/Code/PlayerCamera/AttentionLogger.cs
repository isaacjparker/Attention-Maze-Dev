using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects when the user presses the space bar and tells TelemtryManager
/// to publish a Spacebar event. Nothing more.
/// </summary>
[DisallowMultipleComponent]
public class AttentionLogger : MonoBehaviour
{
    // Optional compile-time flag so designers can switch off console spam.
    [SerializeField] private bool _debugLogInEditor = true;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Fire the telemtry event.
            TelemetryManager.Instance?.PublishSpaceBar();

#if UNITY_EDITOR
            //if (_debugLogInEditor)
                //Debug.Log($"[{Time.time:F2}] AttentionLogger - space bar pressed, event queued.");

#endif
        }
    }



}

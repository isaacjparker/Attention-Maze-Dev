using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight provider that tells TelemtryManager where the player is,
/// which way they're facing, and how long the session has been running.
/// </summary>
[DisallowMultipleComponent]
public class UserTracker : MonoBehaviour, IUserStateProvider
{
    [Tooltip("If left blank the script uses its own transform. " +
            "You can point it at the camera rig or another object " +
            "if that better represents the player's viewpoint.")]
    [SerializeField] private Transform _target;

    private float _sessionStartTime;

    // ---------------------------------------------------------------------


    private void Awake()
    {
        // Default to this gameObject if no target assigned.
        if (_target == null)
            _target = transform;

        _sessionStartTime = Time.time;
    }

    public void ResetSessionTime()
    {
        _sessionStartTime = Time.time;
    }

    // ---------------------------------------------------------------------
    // IPlayerStateProvider
    // ---------------------------------------------------------------------


    public UserSnapshot GetUserSnapshot()
    {
        Vector3 pos = _target.position;
        float rotY = _target.rotation.eulerAngles.y;

        return new UserSnapshot
        {
            posX = pos.x,
            posZ = pos.z,
            rotY = rotY,
            time = Time.time - _sessionStartTime
        };
    }
}

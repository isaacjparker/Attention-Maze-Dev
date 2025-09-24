using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    public static Director Instance { get; private set; }

    public bool IsReady = false;
    public event Action ApplyConfig;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Temporary signal here. Need to listen for ConfigService.Instance.OnResolved
    // Then wait for correct time && COnfigService.Instance.IsResolved to invoke ApplyConfig.
    // OR separate ApplyConfig which just sets up Trial variables from something like StartSystems() which triggers coroutines like autoLog etc.
    private void OnEnable()
    {
        IsReady = true;
        ApplyConfig?.Invoke();
    }

    private void Start()
    {
        // Go through app flow here?
        // Do we need StartSystems() and StopSystems()?
    }
}

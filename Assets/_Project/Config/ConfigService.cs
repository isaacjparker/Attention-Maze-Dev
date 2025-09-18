using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

public class ConfigService : MonoBehaviour
{
    public static ConfigService Instance { get; private set; }

    [Header("Inputs")]
    [Tooltip("Fallback defaults (ScriptableObject).")]
    [SerializeField] private MazeSettings defaultsSO;
    [Tooltip("Local CSV preset e.g. TrialA.csv")]
    [SerializeField] private TextAsset csvPreset;

    public bool IsResolved { get; private set; }
    public event Action OnResolved;

    // Public read-only properties
    public string ConfigName => current?.ConfigName ?? defaultsSO.ConfigName;
    public string Version => current?.Version ?? "";
    public bool ManualLocomotion => current?.ManualLocomotion ?? defaultsSO.ManualLocomotion;
    public bool ManualLook => current?.ManualLook ?? defaultsSO.ManualLook;
    public bool ManualLogging => current?.ManualLogging ?? defaultsSO.ManualLogging;
    public bool AutoLogging => current?.AutoLogging ?? defaultsSO.AutoLogging;
    public float AutoLoggingIntervalSec => current?.AutoLoggingIntervalSec ?? defaultsSO.AutoLoggingIntervalSec;

    public string SourceCsvName => current?.SourceCsvName ?? "";
    public IReadOnlyDictionary<string, string> Snapshot => current?.Snapshot ?? fallbackSnapshot;

    private ConfigData current;
    private Dictionary<string, string> fallbackSnapshot;

    private void Awake()
    {
        // Singleton guard
    }
}

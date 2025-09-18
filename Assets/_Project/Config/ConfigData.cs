using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigData
{
    public readonly string ConfigName;
    public readonly string Version;
    public readonly bool ManualLocomotion;
    public readonly bool ManualLook;
    public readonly bool ManualLogging;
    public readonly bool AutoLogging;
    public readonly float AutoLoggingIntervalSec;

    public readonly string SourceCsvName;

    public readonly IReadOnlyDictionary<string, string> Snapshot;

    public ConfigData(
        string configName,
        string version,
        bool manualLocomotion,
        bool manualLook,
        bool manualLogging,
        bool autoLogging,
        float autoLoggingIntervalSec,
        string sourceCsvName,
        IReadOnlyDictionary<string, string> snapshot)
    { 
        ConfigName = configName;
        Version = version;
        ManualLocomotion = manualLocomotion;
        ManualLook = manualLook;
        ManualLogging = manualLogging;
        AutoLogging = autoLogging;
        AutoLoggingIntervalSec = autoLoggingIntervalSec;
        SourceCsvName = sourceCsvName;
        Snapshot = snapshot;
    }
}

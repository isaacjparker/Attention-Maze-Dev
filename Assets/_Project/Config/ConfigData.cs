using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueAlignment
{
    LEFT,
    CENTER,
    RIGHT,
    JUSTIFY
}

public class ConfigData
{
    public readonly string ConfigName;
    public readonly string Version;
    public readonly bool AutoLogging;
    public readonly float AutoLoggingIntervalSec;
    public readonly bool ManualLocomotion;
    public readonly float LocomotionSpeed;
    public readonly bool ManualLook;
    public readonly float LookSpeed;
    public readonly bool ManualLogging;
    public readonly bool ShowStartDialogueScreen;
    public readonly string StartDialogueText;
    public readonly DialogueAlignment StartDialogueAlignment;
    public readonly bool ShowStartButton;
    public readonly float AutoStartTimer;
    public readonly bool ShowEndDialogueScreen;
    public readonly string EndDialogueText;
    public readonly DialogueAlignment EndDialogueAlignment;
    public readonly bool ShowExitButton;
    public readonly float AutoExitTimer;
    public readonly bool ShowLinkButton;
    public readonly string LinkButtonURL;
    
    

    public readonly string SourceCsvName;

    public readonly IReadOnlyDictionary<string, string> Snapshot;

    public ConfigData(
        string configName,
        string version,
        bool autoLogging,
        float autoLoggingIntervalSec,
        bool manualLocomotion,
        float locomotionSpeed,
        bool manualLook,
        float lookSpeed,
        bool manualLogging,
        bool showStartDialogueScreen,
        string startDialogueText,
        DialogueAlignment startDialogueAlignment,
        bool showStartButton,
        float autoStartTimer,
        bool showEndDialogueScreen,
        string endDialogueText,
        DialogueAlignment endDialogueAlignment,
        bool showExitButton,
        float autoExitTimer,
        bool showLinkButton,
        string linkButtonURL,
        string sourceCsvName,
        IReadOnlyDictionary<string, string> snapshot)
    { 
        ConfigName = configName;
        Version = version;
        AutoLogging = autoLogging;
        AutoLoggingIntervalSec = autoLoggingIntervalSec;
        ManualLocomotion = manualLocomotion;
        LocomotionSpeed = locomotionSpeed;
        ManualLook = manualLook;
        LookSpeed = lookSpeed;
        ManualLogging = manualLogging;
        ShowStartDialogueScreen = showStartDialogueScreen;
        StartDialogueText = startDialogueText;
        StartDialogueAlignment = startDialogueAlignment;
        ShowStartButton = showStartButton;
        AutoStartTimer = autoStartTimer;
        ShowEndDialogueScreen = showEndDialogueScreen;
        EndDialogueText = endDialogueText;
        EndDialogueAlignment = endDialogueAlignment;
        ShowExitButton = showExitButton;
        AutoExitTimer = autoExitTimer;
        ShowLinkButton = showLinkButton;
        LinkButtonURL = linkButtonURL;
        SourceCsvName = sourceCsvName;
        Snapshot = snapshot;
    }
}

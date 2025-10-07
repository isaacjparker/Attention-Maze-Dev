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
    // Identity / versioning
    public readonly string ConfigName;
    public readonly string Version;

    // Logging
    public readonly bool AutoLogging;
    public readonly float AutoLoggingIntervalSec;
    public readonly bool ManualLogging;

    // Movement / Look
    public readonly bool ManualLocomotion;
    public readonly float LocomotionSpeed;
    public readonly bool ManualLook;
    public readonly float LookSpeed;
    public readonly float MaxViewAngle;
    public readonly float CornerTurnSpeed;
    public readonly bool ReturnViewToCenter;
    public readonly float ReturnViewSpeed;

    // Keybinds
    public readonly KeyCode KeyForward;
    public readonly KeyCode KeyLookLeft;
    public readonly KeyCode KeyLookRight;
    public readonly KeyCode KeyAttention;

    // POIs
    public readonly IReadOnlyList<string> PoiTextList;

    // Networking
    public readonly string ServerURL;
    public readonly string WarmupURL;

    // Dialogue
    public readonly int DialogueTextSize;

    // Start Dialogue
    public readonly bool ShowStartDialogueScreen;
    public readonly string StartDialogueText;
    public readonly DialogueAlignment StartDialogueAlignment;
    public readonly bool ShowStartButton;
    public readonly float AutoStartTimer;

    // End Dialogue
    public readonly bool ShowEndDialogueScreen;
    public readonly string EndDialogueText;
    public readonly DialogueAlignment EndDialogueAlignment;
    public readonly string ExitButtonText;
    public readonly float AutoExitTimer;
    public readonly string LinkButtonURL;
    
    
    // Provenance
    public readonly string SourceCsvName;
    public readonly IReadOnlyDictionary<string, string> Snapshot;

    public ConfigData(
        // identity
        string configName,
        string version,

        // logging
        bool autoLogging,
        float autoLoggingIntervalSec,
        bool manualLogging,

        // movement/look
        bool manualLocomotion,
        float locomotionSpeed,
        bool manualLook,
        float lookSpeed,
        float maxViewAngle,
        float cornerTurnSpeed,
        bool returnViewToCenter,
        float returnViewSpeed,
        
        // keybinds
        KeyCode keyForward,
        KeyCode keyLookLeft,
        KeyCode keyLookRight,
        KeyCode keyAttention,

        //POIs
        IReadOnlyList<string> poiTextList,

        // networking
        string serverURL,
        string warmupURL,

        // dialogue
        int dialogueTextSize,

        // start dialogue
        bool showStartDialogueScreen,
        string startDialogueText,
        DialogueAlignment startDialogueAlignment,
        bool showStartButton,
        float autoStartTimer,

        // end dialogue
        bool showEndDialogueScreen,
        string endDialogueText,
        DialogueAlignment endDialogueAlignment,
        string exitButtonText,
        float autoExitTimer,
        string linkButtonURL,

        // provenance
        string sourceCsvName,
        IReadOnlyDictionary<string, string> snapshot)
    { 
        // identity
        ConfigName = configName;
        Version = version;

        // logging
        AutoLogging = autoLogging;
        AutoLoggingIntervalSec = autoLoggingIntervalSec;
        ManualLogging = manualLogging;

        // movement / look
        ManualLocomotion = manualLocomotion;
        LocomotionSpeed = locomotionSpeed;
        ManualLook = manualLook;
        LookSpeed = lookSpeed;
        MaxViewAngle = maxViewAngle;
        CornerTurnSpeed = cornerTurnSpeed;
        ReturnViewToCenter = returnViewToCenter;
        ReturnViewSpeed = returnViewSpeed;
        
        // keybinds
        KeyForward = keyForward;
        KeyLookLeft = keyLookLeft;
        KeyLookRight = keyLookRight;
        KeyAttention = keyAttention;

        // POIs
        PoiTextList = poiTextList;

        // networking
        ServerURL = serverURL;
        WarmupURL = warmupURL;

        // dialogue
        DialogueTextSize = dialogueTextSize;

        // start dialogue
        ShowStartDialogueScreen = showStartDialogueScreen;
        StartDialogueText = startDialogueText;
        StartDialogueAlignment = startDialogueAlignment;
        ShowStartButton = showStartButton;
        AutoStartTimer = autoStartTimer;

        // end dialogue
        ShowEndDialogueScreen = showEndDialogueScreen;
        EndDialogueText = endDialogueText;
        EndDialogueAlignment = endDialogueAlignment;
        ExitButtonText = exitButtonText;
        AutoExitTimer = autoExitTimer;
        LinkButtonURL = linkButtonURL;

        // provenance
        SourceCsvName = sourceCsvName;
        Snapshot = snapshot;
    }
}

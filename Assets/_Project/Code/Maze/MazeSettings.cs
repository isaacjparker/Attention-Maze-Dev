using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallTokens
{ 
    NONE,
    SHAPES,
    WORDS
}

[CreateAssetMenu(menuName = "Maze/Settings/New Settings")]
public class MazeSettings : ScriptableObject
{
    // Identity
    public string ConfigName;
    public string Version;

    // Logging
    public bool AutoLogging;
    public float AutoLoggingIntervalSec;
    public bool ManualLogging;

    // Movement / Look
    public bool ManualLocomotion;
    public float LocomotionSpeed;
    public bool ManualLook;
    public float LookSpeed;
    [Tooltip("Angle to either side of center. Total allowed = 2x this value.")]
    public float MaxViewAngle = 45f;
    public float CornerTurnSpeed = 1f;
    public bool ReturnViewToCenter = false;
    public float ReturnViewSpeed = 2f;

    // Keybind defaults (stored as strings; parsed to KeyCode in ConfigService)
    [Header("Keybinds (string names: e.g., 'w', 'a', 'd', 'space')")]
    public string KeyForward = "w";
    public string KeyLookLeft = "a";
    public string KeyLookRight = "d";
    public string KeyAttention = "space";

    // Points of Interest (comma-separated, 15 items, no spaces)
    [TextArea(1, 3)]
    public string PoiTextList = "sap,yard,diner,bowl,grass,key,fence,limb,rod,opera,tile,ant,wick,mat,canoe";

    // Networking
    [Header("Networking")]
    public string ServerURL = "";   // required for posting
    public string WarmupURL = "";   // optional GET health/warmup

    // UI / Start Dialogue
    [Header("Start Dialogue")]
    public bool ShowStartDialogueScreen;
    [TextArea(3, 8)] public string StartDialogueText;
    public DialogueAlignment StartDialogueAlignment = DialogueAlignment.JUSTIFY;
    public bool ShowStartButton = true;
    public float AutoStartTimer = -1f;

    // UI / End Dialogue
    [Header("End Dialogue")]
    public bool ShowEndDialogueScreen;
    [TextArea(3, 8)] public string EndDialogueText;
    public DialogueAlignment EndDialogueAlignment = DialogueAlignment.JUSTIFY;
    public bool ShowExitButton = true;
    public float AutoExitTimer = -1f;
    public bool ShowLinkButton = false;
    public string LinkButtonURL;

    // Existing field you already had
    public WallTokens WallTokens;
}

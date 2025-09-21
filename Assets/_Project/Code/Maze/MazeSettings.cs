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
    public string ConfigName;
    public string Version;
    public bool AutoLogging;
    public float AutoLoggingIntervalSec;
    public bool ManualLocomotion;
    public float LocomotionSpeed;
    public bool ManualLook;
    public float LookSpeed;
    public bool ManualLogging;
    public bool ShowStartDialogueScreen;
    public string StartDialogueText;
    public DialogueAlignment StartDialogueAlignment;
    public bool ShowStartButton;
    public float AutoStartTimer;
    public bool ShowEndDialogueScreen;
    public string EndDialogueText;
    public DialogueAlignment EndDialogueAlignment;
    public bool ShowExitButton;
    public float AutoExitTimer;
    public bool ShowLinkButton;
    public string LinkButtonURL;
    public WallTokens WallTokens;
}

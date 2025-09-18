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
    public bool ManualLocomotion;
    public bool ManualLook;
    public bool ManualLogging;
    public bool AutoLogging;
    public float AutoLoggingIntervalSec;
    public string Version;
    public WallTokens WallTokens;

}

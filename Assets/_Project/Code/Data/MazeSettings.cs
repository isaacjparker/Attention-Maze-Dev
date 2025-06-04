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
    public string TemplateName;
    public bool ControlMovement;
    public bool ControlView;
    public bool SpacebarToSignifyActivity;
    public WallTokens WallTokens;

}

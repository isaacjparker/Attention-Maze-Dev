using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SquareColourType { RED, YELLOW, GREEN}
public class SquareData : MonoBehaviour
{

    public int id;
    public SquareColourType colourType;

    // Called to update visibility registration (from occlusion check)
    public void SetVisible(bool visible)
    {
        if (visible)
        {
            VisibleSquaresManager.Instance?.AddSquare(this);
        }
        else
        { 
            VisibleSquaresManager.Instance?.RemoveSquare(this);
        }
    }
}

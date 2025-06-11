using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum POIColourType { RED, YELLOW, GREEN}
public class POIData : MonoBehaviour
{
    public int id;
    public POIColourType colourType;

    // Called to update visibility registration (from occlusion check)
    public void SetVisible(bool visible)
    {
        if (visible)
        {
            POIManager.Instance?.AddVisiblePOI(this);
        }
        else
        { 
            POIManager.Instance?.RemoveVisiblePOI(this);
        }
    }
}

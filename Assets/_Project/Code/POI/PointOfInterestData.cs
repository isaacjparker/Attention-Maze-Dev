using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointOfInterestColourType { RED, YELLOW, GREEN, TEXT}

/// <summary>
/// Metadata for a single POI. The extra two public fields are updated every
/// frame by PointOfInterestOcclusionChecker, so TelemtryManager can read them
/// without doing any extra maths.
/// </summary>
public class PointOfInterestData : MonoBehaviour
{
    [Header("Read-only ID stamped at start-up")]
    public int id;

    [Header("Colour or label category")]
    public PointOfInterestColourType colourType;

    // ───────────────────────────────────────────────────────────────
    // Telemetry values (updated each frame by OcclusionChecker)
    // ───────────────────────────────────────────────────────────────
    [HideInInspector] public float distance;    // metres to camera
    [HideInInspector] public float dotProduct;  // 1 = dead-centre, 0 = 90° off-axis


    // ----------------------------------------------------------------
    // Called by PointOfInterestOcclusionChecker whenever visibility
    // flips, so the manager’s HashSet stays in sync.
    // ----------------------------------------------------------------
    public void SetVisible(bool visible)
    {
        if (visible)
        {
            PointOfInterestManager.Instance?.AddVisiblePOI(this);
        }
        else
        { 
            PointOfInterestManager.Instance?.RemoveVisiblePOI(this);
        }
    }
}

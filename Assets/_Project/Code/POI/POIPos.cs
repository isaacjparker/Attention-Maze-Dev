using UnityEngine;

/// <summary>
/// Empty “marker” MonoBehaviour. 
/// Attach this to any empty child under POIGroup to mark a position.
/// POIGroup will look for all POIPos components in its direct children,
/// and spawn prefabs at those exact transforms.
/// </summary>
public class POIPos : MonoBehaviour
{
    // Intentionally left blank. POIGroup will handle everything.
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Text;


/// <summary>
/// Single enum covering all event kinds that trigger a data publish
/// </summary>
public enum EventKind
{ 
    Start,
    Straight,
    Corner,
    End,
    Spacebar        // User pressed the space-bar
}

/// <summary>
/// Snapshot of the user at a moment in time (flattened for easy csv).
/// </summary>
[Serializable]
public struct UserSnapshot
{
    public float posX;      // world X
    public float posZ;      // world Z
    public float rotY;      // yaw in degrees
    public float time;      // seconds since session start
}

/// <summary>
/// Snapshot of a single point of interest
/// </summary>
[Serializable]
public struct POISnapshot
{
    public int id;
    public string type;         // "RED", "GREEN", "YELLOW", "TEXT"
    public string label;        // The TMP Text or label as string
    public Vector3 position;    // full 3-D pos for reference
    public float distance;
    public float dotProduct;
}

/// <summary>
/// Raw in-memory event bundle before flattening/serializing.
/// </summary>
[Serializable]
public class DataPacket
{ 
    public EventKind eventKind;
    public UserSnapshot userSnapshot;
    public List<POISnapshot> visiblePOIs = new List<POISnapshot>(4);
}

/// <summary>
/// Any component that knows how to read the user's current state.
/// </summary>
public interface IUserStateProvider
{
    UserSnapshot GetUserSnapshot();
}

/// <summary>
/// Any component that can list the POIs that are visible *right now*.
/// </summary>
public interface IPointOfInterestProvider
{
    IList<POISnapshot> GetVisiblePOIs();
}

/// <summary>
/// Minimal interface the TelemetryManager cares about for sending a row.
/// Your existing MyWebGLPoster can implement this so we avoid hard dependency.
/// </summary>
public interface IRowPoster
{
    void PostRow(string csvRow);
}

/// <summary>
/// Central hub: builds DataPackets and hands them to the network layer.
/// </summary>
public class TelemetryManager : MonoBehaviour
{
    // ---- Singleton plumbing -------------------------------------------------

    public static TelemetryManager Instance { get; private set; }

    [Header("Optional UI read-out")]
    [SerializeField] private TextMeshProUGUI _packetReadout;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Duplicate TelemetryManager on {gameObject.name}. Destroying new instance.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // ---- Dependencies (wire up via inspector or at runtime) -----------------

    [Tooltip("Component that supplies current user position/rotation/time")]
    [SerializeField] private UserTracker _userProviderSource;

    [Tooltip("Component that supplies current list of visible POIs")]
    [SerializeField] private PointOfInterestManager _poiProviderSource;

    [Tooltip("WebGLPoster (or other transport) that receives CSV rows")]
    [SerializeField] private MyWebGLPoster _posterSource;

    private IUserStateProvider _userProvider;
    private IPointOfInterestProvider _poiProvider;
    private IRowPoster _rowPoster;     // see below

    private const int MaxVisiblePOIs = 4;              // flattening cap

    private void OnEnable()
    {
        // Cache interface references once for performance & null-checks.
        _userProvider = _userProviderSource as IUserStateProvider;
        _poiProvider = _poiProviderSource as IPointOfInterestProvider;
        _rowPoster = _posterSource as IRowPoster;

        Debug.Log($"User src = {_userProviderSource}, Poster src = {_posterSource}");

        if (_userProvider == null)
            Debug.LogError("TelemetryManager: User provider missing or does not implement IUserStateProvider");

        if (_poiProvider == null)
            Debug.LogError("TelemetryManager: POI provider missing or does not implement IPointOfInterestProvider");

        if (_rowPoster == null)
            Debug.LogError("TelemetryManager: Poster missing or does not implement IRowPoster");
    }

    // ---- Public API (called by CheckpointManager, AttentionLogger, …) -------

    public void Publish(EventKind kind)
    {
        if (_userProvider == null || _poiProvider == null || _rowPoster == null)
            return; // avoid NRE spam if dependencies are broken

        DataPacket packet = new DataPacket
        {
            eventKind = kind,
            userSnapshot = _userProvider.GetUserSnapshot()
        };

        // copy up to MaxVisiblePOIs into packet
        foreach (POISnapshot poi in _poiProvider.GetVisiblePOIs()
                                        .Take(MaxVisiblePOIs))
        {
            packet.visiblePOIs.Add(poi);
        }

        /* ── DEBUG: list them in the Console when the user presses Space ───── */
#if UNITY_EDITOR      // keep build output clean
        if (kind == EventKind.Spacebar)
        {
            if (packet.visiblePOIs.Count == 0)
            {
                Debug.Log("[Spacebar] No POIs are visible.");
            }
            else
            {
                string list = string.Join(", ",
                               packet.visiblePOIs.Select(p => $"{p.id}:{p.label}"));
                Debug.Log($"[Spacebar] Visible POIs ({packet.visiblePOIs.Count}): {list}");
            }
        }
#endif

        // 2. Flatten to wide-row CSV.
        string csvRow = FlattenToCsv(packet);

        if (kind == EventKind.Spacebar && _packetReadout != null)
            _packetReadout.text = BuildPrettyText(packet);

        // 3. Ship it off to the transport
        //_rowPoster.PostRow(csvRow);
    }

    // Convenience wrappers for callers that prefer semantic names.
    public void PublishCheckpoint(EventKind checkpointType) => Publish(checkpointType);
    public void PublishSpaceBar() => Publish(EventKind.Spacebar);

    // ---- Private helpers ----------------------------------------------------

    /// <summary>
    /// Converts the DataPacket to a single comma-separated row with fixed columns
    /// (max 4 POIs * 6 fields each). Empty POI slots are left blank.
    /// </summary>>
    private static string FlattenToCsv(DataPacket packet)
    { 
        // 1 column for EventKind, 4 for userSnapshot, 7 per VisiblePOI
        List<string> cols = new List<string>(1 + 4 + MaxVisiblePOIs * 7);

        // Event kind & player
        cols.Add(packet.eventKind.ToString());
        cols.Add(packet.userSnapshot.time.ToString("F3"));
        cols.Add(packet.userSnapshot.posX.ToString("F3"));
        cols.Add(packet.userSnapshot.posZ.ToString("F3"));
        cols.Add(packet.userSnapshot.rotY.ToString("F1"));

        // POI slots (id, type, posX, posY, posZ, dist, dot)
        for (int i = 0; i < MaxVisiblePOIs; i++)
        {
            if (i < packet.visiblePOIs.Count)
            {
                POISnapshot pOI = packet.visiblePOIs[i];
                cols.Add(pOI.id.ToString());
                cols.Add(pOI.type);
                cols.Add(pOI.position.x.ToString("F3"));
                cols.Add(pOI.position.y.ToString("F3"));
                cols.Add(pOI.position.z.ToString("F3"));
                cols.Add(pOI.distance.ToString("F3"));
                cols.Add(pOI.dotProduct.ToString("F3"));
            }
            else
            {
                // fill empty columns so the row length is constant
                cols.Add(""); cols.Add(""); cols.Add(""); cols.Add(""); cols.Add(""); cols.Add(""); cols.Add("");
            }
        }

        return string.Join(",", cols);
    }

    private static string BuildPrettyText(DataPacket p)
    {
        var sb = new StringBuilder(256);

        // headline
        sb.AppendLine($"<b>{p.eventKind}</b>   <size=80%>{p.userSnapshot.time:F2}s</size>");
        sb.AppendLine($"Pos  <b>{p.userSnapshot.posX:F1}</b>, <b>{p.userSnapshot.posZ:F1}</b>   " +
                      $"Yaw  <b>{p.userSnapshot.rotY:F0}°</b>");
        sb.AppendLine();        // blank line

        // POIs
        if (p.visiblePOIs.Count == 0)
        {
            sb.AppendLine("<i>No POIs in view</i>");
        }
        else
        {
            sb.AppendLine($"Visible POIs <b>{p.visiblePOIs.Count}</b>:");
            foreach (var poi in p.visiblePOIs)
            {
                sb.AppendLine($"  <b>{poi.id,2}</b> " +                          // id padded to 2
                              $"{poi.label,-10} " +                              // label up to 10 chars
                              $"[{poi.type}]  " +
                              $"d={poi.distance:F1}m  " +
                              $"dot={poi.dotProduct:F2}");
            }
        }

        return sb.ToString();
    }

}

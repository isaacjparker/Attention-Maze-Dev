using System.Linq;              // ← for OrderBy/OrderByDescending
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;  // for TMP_Text

/// <summary>
/// Central registry for every Point-of-Interest in the scene
/// and the single source of visible-POI telemtry data.
/// </summary>
public class PointOfInterestManager : MonoBehaviour, IPointOfInterestProvider
{

    [Header("— CSV Data for TEXT POIs —")]
    [Tooltip("Drop in a CSV file where each cell (or line) is one word/label.")]
    public TextAsset csvFile;

    // --------------------------------------------------------------------- 
    // Singleton plumbing
    // --------------------------------------------------------------------- 

    public static PointOfInterestManager Instance { get; private set; }


    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        ParseCsv();
    }


    /// <summary>
    /// POIs currently inside the camera frsutrum (filled by PointOfInterestOcclusionChecker).
    /// HashSet avoids duplicates and provides fast add/remove.
    /// </summary>
    private HashSet<PointOfInterestData> _visiblePOIs = new();

    /// <summary>
    /// Words extracted from the CSV, used to label TEXT-type POIs
    /// </summary>
    private List<string> _csvWords = new();

    private int _nextId;

    // --------------------------------------------------------------------- 
    // Scene initialisation helpers (ID stamping, CSV labelling) 
    // --------------------------------------------------------------------- 

    private void Start()
    { 
        AssignAllIDs();
        AssignTextFromCsv();
    }

    private void ParseCsv()
    {
        _csvWords.Clear();

        if (csvFile == null || string.IsNullOrWhiteSpace(csvFile.text))
            return;

        var lines = csvFile.text
                           .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
            _csvWords.AddRange(line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim()));
    }

    /// <summary>
    /// Walks every POIGroup in ascending z,
    /// then within each group orders its POIData
    /// by x (asc for odd groups, desc for even),
    /// and stamps incremental IDs.
    /// </summary>
    public void AssignAllIDs()
    {
        _nextId = 0;

        // 1) Gather all POIGroup, sort by z-position
        List<PointOfInterestGroup> groups = FindObjectsOfType<PointOfInterestGroup>().OrderBy(g => g.transform.position.z).ToList();

        
        foreach (PointOfInterestGroup group in groups)
        {
            // 2) COmpute bucket = floor(z/10)
            float z = group.transform.position.z;
            int bucket = Mathf.FloorToInt(z / 10f);

            // 3) Decide odd/even on the bucket, not on the loop index
            bool isEvenBucket = (bucket % 2) == 0;

            // 4) Pull out each POIData under that group's markers
            List<PointOfInterestData> datas = group.markers
                .Select(marker => marker.GetComponentInChildren<PointOfInterestData>())
                .Where(d => d != null)
                .ToList();

            // 4) Order by x ascending (odd) or descending (even)
            if (isEvenBucket)
                datas = datas.OrderBy(d => d.transform.position.x).ToList();
            else
                datas = datas.OrderByDescending(d => d.transform.position.x).ToList();

            // 5) Assign IDs
            foreach (PointOfInterestData data in datas)
            {
                data.id = _nextId;
                _nextId++;
            }
        }

    }

    /// <summary>
    /// Call this after you run AssignAllIDs() (or whenever your IDs/text should refresh).
    /// It will find every TEXT‐typed POIData, in ascending id order,
    /// and assign its TMP_Text from csvWords[ id ].
    /// </summary>
    public void AssignTextFromCsv()
    {
        // find all the TEXT POIs, ordered by their id
        var textPOIs = FindObjectsOfType<PointOfInterestData>()
                      .Where(d => d.colourType == PointOfInterestColourType.TEXT)
                      .OrderBy(d => d.id)
                      .ToList();

        // for safety, only go as far as we have words
        int count = Math.Min(textPOIs.Count, _csvWords.Count);

        for (int i = 0; i < count; i++)
        {
            var poiData = textPOIs[i];
            // find the TMP_Text in its children
            var label = poiData.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = _csvWords[i];
            else
                Debug.LogWarning($"POI id={poiData.id} has no TMP_Text to assign.");
        }
    }

    /// <summary>
    /// Finds every POIGroup whose poiList is non-empty and
    /// whose POIData.colourType == TEXT for all markers,
    /// then calls RepositionTextLabels() on them.
    /// </summary>
    public void RepositionAllTextGroups()
    {
        // grab every POIGroup in the scene
        var groups = FindObjectsOfType<PointOfInterestGroup>();
        foreach (var group in groups)
        {
            // skip empty groups
            if (group.markers == null || group.markers.Count == 0)
                continue;

            // check that *every* POIData under each marker is TEXT
            bool allText = group.markers
                .Select(marker => marker.GetComponentInChildren<PointOfInterestData>())
                .All(data => data != null && data.colourType == PointOfInterestColourType.TEXT);

            if (allText)
                group.RepositionTextLabels();
        }
    }

    // --------------------------------------------------------------------- 
    // External hooks called by PointOfInterestOcclusionChecker
    // --------------------------------------------------------------------- 

    public void AddVisiblePOI(PointOfInterestData poi) => _visiblePOIs.Add(poi);
    public void RemoveVisiblePOI(PointOfInterestData poi) => _visiblePOIs.Remove(poi);

    // --------------------------------------------------------------------- 
    // IPointOfInterestProvider implementation
    // --------------------------------------------------------------------- 

    /// <summary>
    /// Retrun a *fresh* list each call so TelemtryManager can iterate safely.
    /// The list is ordered nearest-first to make any cap (e.g. top-4) meaningful.
    /// </summary>
    public IList<POISnapshot> GetVisiblePOIs()
    { 
        if (_visiblePOIs.Count == 0)
            return Array.Empty<POISnapshot>();

        // Build and sort by distance ascending
        List<POISnapshot> pOISnapshots = new List<POISnapshot>(_visiblePOIs.Count);

        foreach (PointOfInterestData poi in _visiblePOIs)
        {
            // PointOfInterestOccluder keeps these up to date.
            pOISnapshots.Add(new POISnapshot
            {
                id = poi.id,
                type = poi.colourType.ToString(),
                position = poi.transform.position,
                distance = poi.distance,                // pre-calculated in checker
                dotProduct = poi.dotProduct             // pre-calculated in checker
            });
        }

        pOISnapshots.Sort((a, b) => a.distance.CompareTo(b.distance));
        return pOISnapshots;
    }

}

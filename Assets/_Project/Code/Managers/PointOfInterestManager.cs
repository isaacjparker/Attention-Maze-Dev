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

    // --------------------------------------------------------------------- 
    // Singleton plumbing
    // --------------------------------------------------------------------- 

    public static PointOfInterestManager Instance { get; private set; }

    /// <summary>
    /// POIs currently inside the camera frsutrum (filled by PointOfInterestOcclusionChecker).
    /// HashSet avoids duplicates and provides fast add/remove.
    /// </summary>
    private HashSet<PointOfInterestData> _visiblePOIs = new();

    private List<string> _csvWords = new();

    private int _nextId;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }

    private void OnDisable()
    {
        if (Director.Instance != null)
        {
            Director.Instance.OnApplicationSetup -= OnApplicationSetup;
            Director.Instance.OnApplicationRun -= OnApplicationRun;
            Director.Instance.OnApplicationEnd -= OnApplicationEnd;
        }
    }

    private void Start()
    {
        AssignAllIDs();
        //AssignTextFromCsv();

        // Defer text assignment until config is ready
        if (Director.Instance != null && Director.Instance.IsSetupReady)
        {
            OnApplicationSetup();
        }
        else if (Director.Instance != null)
        {
            Director.Instance.OnApplicationSetup += OnApplicationSetup;
        }

        // (Run/End not strictly needed here, but wire for parity if you like)
        if (Director.Instance != null && Director.Instance.IsRunReady)
            OnApplicationRun();
        else if (Director.Instance != null)
            Director.Instance.OnApplicationRun += OnApplicationRun;

        if (Director.Instance != null)
            Director.Instance.OnApplicationEnd += OnApplicationEnd;
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
            {
                string labelString = _csvWords[i];
                label.text = labelString;
                label.ForceMeshUpdate();
                poiData.labelText = labelString;
            }
            else
            {
                Debug.LogWarning($"POI id={poiData.id} has no TMP_Text to assign.");
            }
                
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
                label = poi.labelText,
                position = poi.transform.position,
                distance = poi.distance,                // pre-calculated in checker
                dotProduct = poi.dotProduct             // pre-calculated in checker
            });
        }

        pOISnapshots.Sort((a, b) => a.distance.CompareTo(b.distance));
        return pOISnapshots;
    }

    private void OnApplicationSetup()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationSetup -= OnApplicationSetup;

        _csvWords.Clear();

        // Primary source: config (poi_text_list)
        var cs = ConfigService.Instance;
        if (cs != null && cs.PoiTextList != null && cs.PoiTextList.Count > 0)
        {
            _csvWords.AddRange(cs.PoiTextList);
        }
        else
        {
            Debug.LogWarning("[POI] ConfigService has no poi_text_list; labels will be empty.");
            // (Optionally pad with 15 empties)
            for (int i = 0; i < 15; i++) _csvWords.Add("");
        }

        AssignTextFromCsv();
        // Optional: if your text layout depends on content width
        // RepositionAllTextGroups();
    }

    private void OnApplicationRun()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationRun -= OnApplicationRun;
        // No-op needed here for POIs
    }

    private void OnApplicationEnd()
    {
        if (Director.Instance != null)
            Director.Instance.OnApplicationEnd -= OnApplicationEnd;
        // No-op needed here for POIs
    }


}

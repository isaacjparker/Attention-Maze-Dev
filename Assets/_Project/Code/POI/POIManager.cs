using System.Linq;              // ← for OrderBy/OrderByDescending
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;  // for TMP_Text

public class POIManager : MonoBehaviour
{

    [Header("— CSV Data for TEXT POIs —")]
    [Tooltip("Drop in a CSV file where each cell (or line) is one word/label.")]
    public TextAsset csvFile;


    public static POIManager Instance { get; private set; }

    private HashSet<POIData> visiblePOIs = new HashSet<POIData>();
    private List<string> csvWords;
    private int nextID;

    private void Awake()
    {
        nextID = 0;

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // --- parse the CSV into a flat list of words ---
        csvWords = new List<string>();
        if (csvFile != null)
        {
            // split on newlines, then on commas—flatten into one list
            var lines = csvFile.text
                         .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
                csvWords.AddRange(line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => s.Trim()));
        }
    }

    private void Start()
    {
        // 1) Stamp IDs
        AssignAllIDs();

        // 2) Populate any TEXT POIs from the CSV
        AssignTextFromCsv();

        // 3) Re‐space purely‐TEXT groups
        RepositionAllTextGroups();
    }

    /// <summary>
    /// Walks every POIGroup in ascending z,
    /// then within each group orders its POIData
    /// by x (asc for odd groups, desc for even),
    /// and stamps incremental IDs.
    /// </summary>
    public void AssignAllIDs()
    {
        nextID = 0;

        // 1) Gather all POIGroup, sort by z-position
        List<POIGroup> groups = FindObjectsOfType<POIGroup>().OrderBy(g => g.transform.position.z).ToList();

        
        foreach (POIGroup group in groups)
        {
            // 2) COmpute bucket = floor(z/10)
            float z = group.transform.position.z;
            int bucket = Mathf.FloorToInt(z / 10f);

            // 3) Decide odd/even on the bucket, not on the loop index
            bool isEvenBucket = (bucket % 2) == 0;

            // 4) Pull out each POIData under that group's markers
            List<POIData> datas = group.poiList
                .Select(marker => marker.GetComponentInChildren<POIData>())
                .Where(d => d != null)
                .ToList();

            // 4) Order by x ascending (odd) or descending (even)
            if (isEvenBucket)
                datas = datas.OrderBy(d => d.transform.position.x).ToList();
            else
                datas = datas.OrderByDescending(d => d.transform.position.x).ToList();

            // 5) Assign IDs
            foreach (POIData data in datas)
            {
                data.id = nextID;
                nextID++;
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
        var textPOIs = FindObjectsOfType<POIData>()
                      .Where(d => d.colourType == POIColourType.TEXT)
                      .OrderBy(d => d.id)
                      .ToList();

        // for safety, only go as far as we have words
        int count = Math.Min(textPOIs.Count, csvWords.Count);

        for (int i = 0; i < count; i++)
        {
            var poiData = textPOIs[i];
            // find the TMP_Text in its children
            var label = poiData.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = csvWords[i];
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
        var groups = FindObjectsOfType<POIGroup>();
        foreach (var group in groups)
        {
            // skip empty groups
            if (group.poiList == null || group.poiList.Count == 0)
                continue;

            // check that *every* POIData under each marker is TEXT
            bool allText = group.poiList
                .Select(marker => marker.GetComponentInChildren<POIData>())
                .All(data => data != null && data.colourType == POIColourType.TEXT);

            if (allText)
                group.RepositionTextLabels();
        }
    }

    public void AddVisiblePOI(POIData square)
    { 
        visiblePOIs.Add(square);
    }

    public void RemoveVisiblePOI(POIData square)
    { 
        visiblePOIs.Remove(square);
    }

    public IEnumerable<POIData> GetVisiblePOIs()
    { 
        return visiblePOIs;
    }

}

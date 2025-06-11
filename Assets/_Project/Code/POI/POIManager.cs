using System.Linq;              // ← for OrderBy/OrderByDescending
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIManager : MonoBehaviour
{
    public static POIManager Instance { get; private set; }

    private HashSet<POIData> visiblePOIs = new HashSet<POIData>();
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
    }

    private void Start()
    {
        AssignAllIDs();
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

        // 2) For each group...
        for (int gi = 0; gi < groups.Count; gi++)
        {
            POIGroup group = groups[gi];
            bool isOddGroup = ((gi + 1) % 2) == 1;

            // 3) Pull out each POIData under that group's markers
            List<POIData> datas = group.poiList
                .Select(marker => marker.GetComponentInChildren<POIData>())
                .Where(d => d != null)
                .ToList();

            // 4) Order by x ascending (odd) or descending (even)
            if (isOddGroup)
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

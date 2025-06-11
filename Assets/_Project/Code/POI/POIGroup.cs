using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// POIGroup holds:
///   • a List<GameObject> POIPrefabs  (the pool of prefabs to spawn)  
///   • a bool InstantiateRandom        (tick‐box: random vs. ordered)  
///   • a List<POIPos> poiList         (auto‐populated when InstantiatePrefabs is called)  
/// 
/// NOTHING is spawned automatically in Edit Mode. You must click the button “Instantiate Prefabs”
/// in the Inspector. That call will:
///   1) Find all direct children with POIPos → rebuild poiList  
///   2) Destroy any children under those POIPos markers (so you never double‐spawn)  
///   3) Loop (or random‐pick) through POIPrefabs and place one instance under each POIPos  
/// 
/// If you want exactly the same behavior at runtime, you can simply call InstantiatePrefabs()
/// from Start() (or from another script). But by default this class does **not** do anything until you press the button.
/// </summary>
public class POIGroup : MonoBehaviour
{
    [Header("■ Prefab Pool")]
    [Tooltip("Drop in all the prefab variants you want. \n" +
             "If InstantiateRandom = false, they'll be used in index order (wrapping). \n" +
             "If InstantiateRandom = true, each POIPos gets a random choice.")]
    public List<GameObject> POIPrefabs = new List<GameObject>();

    [Header("■ Instantiate Mode")]
    [Tooltip("Unchecked = cycle through POIPrefabs in order. Checked = pick a random prefab for each POIPos.")]
    public bool InstantiateRandom = false;

    [Space]
    [Tooltip("(Read‐only) This list is rebuilt whenever you click “Instantiate Prefabs.”")]
    public List<POIPos> poiList = new List<POIPos>();

    /// <summary>
    /// Call this from your custom Editor button. 
    /// It will:
    ///   1) Rebuild poiList = all direct children with POIPos
    ///   2) Destroy any old spawned children under those POIPos
    ///   3) Instantiate (in‐order or random) one prefab under each POIPos, wrapping if needed.
    /// </summary>
    public void InstantiatePrefabs()
    {
        // ───────────────────────────────────────────────────
        // 1) Find all direct children with a POIPos → rebuild poiList
        // ───────────────────────────────────────────────────
        poiList.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            POIPos marker = child.GetComponent<POIPos>();
            if (marker != null)
            {
                poiList.Add(marker);
            }
        }

#if UNITY_EDITOR
        // In Edit Mode, mark this object dirty so Unity knows poiList changed.
        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
#endif

        // ───────────────────────────────────────────────────
        // 2) Destroy any old spawned children under each POIPos
        // ───────────────────────────────────────────────────
        foreach (POIPos marker in poiList)
        {
            Transform parentT = marker.transform;
            // Destroy everything under this marker before spawning anew
            for (int k = parentT.childCount - 1; k >= 0; k--)
            {
                GameObject old = parentT.GetChild(k).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(old);
                else
                    Destroy(old);
#else
                Destroy(old);
#endif
            }
        }

        // ───────────────────────────────────────────────────
        // 3) If no prefabs or no POIPos markers, abort
        // ───────────────────────────────────────────────────
        if (POIPrefabs == null || POIPrefabs.Count == 0 || poiList.Count == 0)
            return;

        // ───────────────────────────────────────────────────
        // 4) Instantiate one prefab under each POIPos (in‐order or random)
        // ───────────────────────────────────────────────────
        int prefabCount = POIPrefabs.Count;
        int nextIndex = 0;

        foreach (POIPos marker in poiList)
        {
            GameObject chosen = null;

            if (InstantiateRandom)
            {
                int rnd = Random.Range(0, prefabCount);
                chosen = POIPrefabs[rnd];
            }
            else
            {
                chosen = POIPrefabs[nextIndex % prefabCount];
                nextIndex++;
            }

            if (chosen == null)
                continue; // skip if a slot in POIPrefabs was left empty

            GameObject instance = null;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In the Editor, use PrefabUtility so it stays a “connected” prefab instance
                instance = (GameObject)PrefabUtility.InstantiatePrefab(chosen, marker.transform);
                instance.transform.localPosition = Vector3.zero;
                instance.name = chosen.name;
            }
            else
            {
                // In Play Mode, do a normal Instantiate
                instance = Instantiate(chosen, marker.transform);
                instance.transform.localPosition = Vector3.zero;
                instance.name = chosen.name;
            }
#else
            // In a build, always do a normal Instantiate
            instance = Instantiate(chosen, marker.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.name = chosen.name;
#endif
        }
    }

    /// <summary>
    /// Destroys all spawned prefabs under each POIPos marker
    /// and then clears the poiList.
    /// </summary>
    public void ClearPrefabs()
    {
        // Destroy any old spawned children under each POIPos
        foreach (POIPos marker in poiList)
        { 
            Transform parentT = marker.transform;
            for (int k = parentT.childCount - 1; k >= 0; k--)
            {
                GameObject old = parentT.GetChild(k).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(old);
                else
                    Destroy(old);
#else
                Destroy(old);
#endif
            }
        }

        // Empty out the list of marker
        poiList.Clear();
    }
}

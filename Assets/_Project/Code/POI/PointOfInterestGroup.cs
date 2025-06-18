using System.Linq;          // Sum()
using TMPro;                // TMP_Text
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Spawns one prefab under each direct child that carries a
/// <see cref="PointOfInterestOcclusionChecker"/> component (“marker”).
///
/// • Press **Instantiate Prefabs** in the Inspector (or call it at runtime) to
///      1) rebuild <c>markers</c>,
///      2) clear any previously-spawned children,
///      3) place a prefab from <c>POIPrefabs</c> under every marker
///        (ordered or random).
/// • Press **Clear Prefabs** to delete the spawned children and empty the list.
/// </summary>
public class PointOfInterestGroup : MonoBehaviour
{
    // ───────────────────────────────────────────────── Prefab pool

    [Header("■ Prefab Pool")]
    [Tooltip(
        "Drop in all prefab variants.\n" +
        "If InstantiateRandom = false they’re used in index order (wrapping).\n" +
        "If true, each marker gets a random choice.")]
    public List<GameObject> POIPrefabs = new List<GameObject>();

    // ───────────────────────────────────────────────── Mode toggle

    [Header("■ Instantiate Mode")]
    [Tooltip("Unchecked = cycle through prefabs in order. Checked = random per marker.")]
    public bool InstantiateRandom = false;

    // ───────────────────────────────────────────────── Debug info

    [Space]
    [Tooltip("(Read-only) Rebuilt whenever you press “Instantiate Prefabs.”")]
    public List<PointOfInterestOcclusionChecker> markers = new List<PointOfInterestOcclusionChecker>();

    // ======================================================================
    // PUBLIC API
    // ======================================================================

    public void InstantiatePrefabs()
    {
        // ── 1) rebuild marker list ────────────────────────────────────────
        markers.Clear();

        foreach (Transform childTransform in transform)
        {
            PointOfInterestOcclusionChecker checker =
                childTransform.GetComponent<PointOfInterestOcclusionChecker>();

            if (checker != null)
                markers.Add(checker);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
#endif

        // ── 2) remove previously spawned children ─────────────────────────
        foreach (PointOfInterestOcclusionChecker checker in markers)
            DestroyAllChildren(checker.transform);

        // ── 3) early-out if nothing to spawn ──────────────────────────────
        if (POIPrefabs.Count == 0 || markers.Count == 0)
            return;

        // ── 4) spawn one prefab per marker ────────────────────────────────
        int prefabCount = POIPrefabs.Count;
        int sequentialIndex = 0;

        foreach (PointOfInterestOcclusionChecker checker in markers)
        {
            GameObject selectedPrefab;

            if (InstantiateRandom)
            {
                int randomIndex = Random.Range(0, prefabCount);
                selectedPrefab = POIPrefabs[randomIndex];
            }
            else
            {
                selectedPrefab = POIPrefabs[sequentialIndex % prefabCount];
                sequentialIndex++;
            }

            if (selectedPrefab == null)
                continue;        // skip empty slots in the prefab list

#if UNITY_EDITOR
            GameObject instance;
            if (!Application.isPlaying)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(
                    selectedPrefab, checker.transform);
            }
            else
            {
                instance = Instantiate(selectedPrefab, checker.transform);
            }
#else
            GameObject instance = Instantiate(selectedPrefab, checker.transform);
#endif
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.name = selectedPrefab.name;
        }

        // ── 5) optional text-row layout pass ──────────────────────────────
        bool everyMarkerIsText = markers.All(currentChecker =>
        {
            PointOfInterestData dataComponent =
                currentChecker.GetComponentInChildren<PointOfInterestData>();

            return dataComponent != null &&
                   dataComponent.colourType == PointOfInterestColourType.TEXT;
        });

        if (everyMarkerIsText)
        {
            //RepositionTextLabels();
        }
    }

    public void ClearPrefabs()
    {
        foreach (PointOfInterestOcclusionChecker checker in markers)
            DestroyAllChildren(checker.transform);

        markers.Clear();
    }

    // ======================================================================
    // HELPERS
    // ======================================================================

    private static void DestroyAllChildren(Transform parentTransform)
    {
        for (int childIndex = parentTransform.childCount - 1;
             childIndex >= 0;
             childIndex--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Object.DestroyImmediate(parentTransform.GetChild(childIndex).gameObject);
            else
                Object.Destroy(parentTransform.GetChild(childIndex).gameObject);
#else
            Object.Destroy(parentTransform.GetChild(childIndex).gameObject);
#endif
        }
    }

    /// <summary>
    /// Re-centres each TextMeshPro label so that the visual gap between
    /// adjacent words is constant.
    /// </summary>
    public void RepositionTextLabels()
    {
        int markerCount = markers.Count;
        if (markerCount < 2) return;

        float firstX = markers[0].transform.localPosition.x;
        float lastX = markers[markerCount - 1].transform.localPosition.x;
        float totalSpan = lastX - firstX;

        // 1) gather prefab roots under each marker
        Transform[] prefabRoots = new Transform[markerCount];
        for (int i = 0; i < markerCount; i++)
        {
            if (markers[i].transform.childCount > 0)
                prefabRoots[i] = markers[i].transform.GetChild(0);
        }

        // 2) measure rendered widths
        float[] labelWidths = new float[markerCount];
        for (int i = 0; i < markerCount; i++)
        {
            Transform rootTransform = prefabRoots[i];
            if (rootTransform == null) continue;

            TMP_Text textComponent = rootTransform.GetComponentInChildren<TMP_Text>();
            if (textComponent == null) continue;

            textComponent.ForceMeshUpdate();
            labelWidths[i] = textComponent.GetRenderedValues(false).x;
        }

        if (labelWidths[0] <= 0f) return;  // avoid divide-by-zero

        // 3) compute uniform gap
        float occupiedWidth = labelWidths.Sum();
        float gap = (totalSpan - occupiedWidth) / (markerCount - 1);

        // 4) reposition each label root
        float currentX = firstX + labelWidths[0] / 2f;

        for (int i = 0; i < markerCount; i++)
        {
            Transform rootTransform = prefabRoots[i];
            if (rootTransform == null) continue;

            Vector3 localPosition = rootTransform.localPosition;
            rootTransform.localPosition = new Vector3(
                currentX,
                localPosition.y,
                localPosition.z
            );

            if (i < markerCount - 1)
            {
                currentX += (labelWidths[i] / 2f)
                          + gap
                          + (labelWidths[i + 1] / 2f);
            }
        }
    }
}

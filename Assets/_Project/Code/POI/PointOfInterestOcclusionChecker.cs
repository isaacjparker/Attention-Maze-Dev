using UnityEngine;
using TMPro;

public class PointOfInterestOcclusionChecker : MonoBehaviour
{
    [Tooltip("Draw corner spheres (green/red) and rays (cyan/yellow) in Scene view")]
    public bool showDebugGizmos = true;

    PointOfInterestData data;
    Camera mainCam;
    BoxCollider poiCol;      // for RED / GREEN / YELLOW cubes
    TMP_Text poiText;     // for TEXT labels
    string cachedLabel;

    bool lastVisible;

    // ── cached sample points for gizmos & raycasts ──────────────────────────
    Vector3[] pts = new Vector3[4];   // most-recent world points
    bool[] inView = new bool[4];
    bool[] hitSelf = new bool[4];

    // ── one-time cache of the 4 text corners (world space) ──────────────────
    Vector3[] textWorldCorners;
    bool cornersReady;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        data = GetComponentInChildren<PointOfInterestData>();
        poiCol = data.GetComponent<BoxCollider>();
        poiText = data.GetComponentInChildren<TMP_Text>();
        mainCam = Camera.main;

        Debug.Log($"A {name}: colour={data.colourType} poiText={(poiText != null)} poiCol={(poiCol != null)}");

        if (data.colourType == PointOfInterestColourType.TEXT && poiText != null)
            CacheTextCorners();                    // build once
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (mainCam == null) return;

        // telemetry
        Vector3 camPos = mainCam.transform.position;
        Vector3 toPoi = transform.position - camPos;
        data.distance = toPoi.magnitude;
        data.dotProduct = 1f - Mathf.Clamp01(Vector3.Angle(mainCam.transform.forward, toPoi) / 90f);

        bool visible = TestVisibility();

        if (visible != lastVisible)
        {
            data.SetVisible(visible);
            lastVisible = visible;
            //if (visible) Debug.Log($"POI visible → id={data.id}  label={data.labelText}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    bool TestVisibility()
    {
        Vector3[] corners = GetSamplePoints();   // returns world points
        if (corners == null) return false;

        EnsureArrays(corners.Length);

        bool anyInFrustum = false;
        for (int i = 0; i < corners.Length; ++i)
        {
            pts[i] = corners[i];
            inView[i] = InFrustum(pts[i]);
            hitSelf[i] = false;
            anyInFrustum |= inView[i];
        }
        if (!anyInFrustum) return false;

        Vector3 camPos = mainCam.transform.position;

        for (int i = 0; i < corners.Length; ++i)
        {
            if (!inView[i]) continue;

            // TEXT branch – require clear line of sight only
            if (data.colourType == PointOfInterestColourType.TEXT)
            {
                if (!Physics.Linecast(camPos, pts[i], out _, ~0, QueryTriggerInteraction.Ignore))
                {
                    hitSelf[i] = true;    // cyan gizmo
                    return true;
                }
                continue;
            }

            // Cube branch – must hit own collider
            Vector3 dir = pts[i] - camPos;
            float dist = dir.magnitude;
            if (Physics.Raycast(camPos, dir.normalized, out RaycastHit hit, dist + 0.05f))
            {
                if (hit.collider.transform.IsChildOf(transform))
                {
                    hitSelf[i] = true;
                    return true;
                }
            }
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    Vector3[] GetSamplePoints()
    {
        if (data.colourType == PointOfInterestColourType.TEXT && poiText != null)
        {
            // Re-cache whenever we still have no usable corners
            // OR the label length has changed (easiest dirty-flag).
            if (!cornersReady || poiText.text != cachedLabel)
                CacheTextCorners();

            return textWorldCorners;
        }

        if (poiCol != null)                         // coloured cube
            return FaceCorners(poiCol, true);

        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CacheTextCorners()
    {
        poiText.ForceMeshUpdate();                  // ensure mesh exists this frame
        TMP_TextInfo ti = poiText.textInfo;

        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;

        for (int i = 0; i < ti.characterCount; ++i)
        {
            var ch = ti.characterInfo[i];
            if (!ch.isVisible) continue;

            minY = Mathf.Min(minY, ch.bottomLeft.y);
            maxY = Mathf.Max(maxY, ch.topRight.y);
            minX = Mathf.Min(minX, ch.bottomLeft.x);
            maxX = Mathf.Max(maxX, ch.topRight.x);
        }

        // local-space rectangle tightly around glyphs
        Vector3 tl = new Vector3(minX, maxY, 0);
        Vector3 tr = new Vector3(maxX, maxY, 0);
        Vector3 bl = new Vector3(minX, minY, 0);
        Vector3 br = new Vector3(maxX, minY, 0);

        textWorldCorners = new Vector3[4] {
            poiText.transform.TransformPoint(tl),
            poiText.transform.TransformPoint(tr),
            poiText.transform.TransformPoint(bl),
            poiText.transform.TransformPoint(br) };

        cachedLabel = poiText.text;
        cornersReady = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    static Vector3[] FaceCorners(BoxCollider col, bool facePlusZ)
    {
        Vector3 h = col.size * 0.5f;
        float z = facePlusZ ? h.z : -h.z;

        Vector3[] loc = {
            col.center + new Vector3(-h.x,  h.y,  z),   // TL
            col.center + new Vector3( h.x,  h.y,  z),   // TR
            col.center + new Vector3(-h.x, -h.y,  z),   // BL
            col.center + new Vector3( h.x, -h.y,  z) }; // BR

        for (int i = 0; i < 4; ++i)
            loc[i] = col.transform.TransformPoint(loc[i]);

        return loc;
    }

    bool InFrustum(Vector3 w)
    {
        Vector3 v = mainCam.WorldToViewportPoint(w);
        return v.z > 0 && v.x is >= 0 and <= 1 && v.y is >= 0 and <= 1;
    }

    void EnsureArrays(int n)
    {
        if (pts.Length != n)
        {
            pts = new Vector3[n];
            inView = new bool[n];
            hitSelf = new bool[n];
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || pts == null) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 camPos = mainCam.transform.position;

        for (int i = 0; i < pts.Length; ++i)
        {
            Gizmos.color = (i < inView.Length && inView[i]) ? Color.green : Color.red;
            Gizmos.DrawSphere(pts[i], 0.04f);

            Gizmos.color = (i < hitSelf.Length && hitSelf[i]) ? Color.cyan : Color.yellow;
            Gizmos.DrawLine(camPos, pts[i]);
        }
    }
}

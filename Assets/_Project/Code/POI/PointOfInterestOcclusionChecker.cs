using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1) Determines if its POI is currently visible & unoccluded
/// 2) Updates distance and dotProduct every frame for telemtry.
/// </summary>
public class PointOfInterestOcclusionChecker : MonoBehaviour
{

    private PointOfInterestData data;
    private bool lastVisible = false;
    private Camera mainCam;


    void Start()
    {
        data = GetComponentInChildren<PointOfInterestData>();
        mainCam = Camera.main;
    }


    void Update()
    {
        if (mainCam == null) return;

        // --- Update distance & dotProduct each frame -------------------------
        Vector3 camPos = mainCam.transform.position;
        Vector3 toPoi = transform.position - camPos;
        data.distance = toPoi.magnitude;

        // Dot remapped to 0-1 where 1 is straight ahead, 0 is ≥90 degrees off
        float angle = Vector3.Angle(mainCam.transform.forward, toPoi);
        data.dotProduct = 1f - Mathf.Clamp01(angle / 90f);

        // --- Visibility test --------------------------------------------------
        bool isVisible = CheckUnoccluded();

        if (isVisible != lastVisible)
        { 
            data.SetVisible(isVisible);
            lastVisible = isVisible;
        }

    }

    bool CheckUnoccluded()
    {
        float halfX = 0.5f;
        float halfY = 0.5f;
        float frontZ = 0.5f - 0.01f; // Adjust if -Z is exposed

        Vector3[] localOffsets = new Vector3[]
        {
            new Vector3(-halfX, halfY, frontZ),
            new Vector3(halfX, halfY, frontZ),
            new Vector3(-halfX, -halfY, frontZ),
            new Vector3(halfX, -halfY, frontZ),
        };

        bool anyCornerInView = false;
        Vector3[] worldPoints = new Vector3[localOffsets.Length];

        // Convert all corners to world space and check frustrum
        for (int i = 0; i < localOffsets.Length; i++)
        { 
            Vector3 worldPoint = transform.TransformPoint(localOffsets[i]);
            worldPoints[i] = worldPoint;
            if (IsInCameraView(mainCam, worldPoint))
                anyCornerInView = true;
        }

        if (!anyCornerInView)
            return false; // Not in view, no need to check occlusion

        for (int i = 0; i < localOffsets.Length; i++)
        {
            Vector3 worldPoint = worldPoints[i];
            if (!IsInCameraView(mainCam, worldPoint))
                continue; // Skip occluded/out-of-view corners

            Vector3 camPos = mainCam.transform.position;
            Vector3 dir = worldPoint - camPos;
            float dist = dir.magnitude;
            Ray ray = new Ray(camPos, dir.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, dist + 0.05f))
            {
                if (hit.collider.transform.IsChildOf(transform))
                    return true; // This corner is visible and unoccluded
            }
        }

        // No corners were both in view and unoccluded
        return false;
        
    }

    bool IsInCameraView(Camera cam, Vector3 worldPoint)
    { 
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPoint);
        return viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    // For gizmo drawing
    //private Vector3[] lastSamplePoints;
    //private bool[] lastHits;

    /*
    // Draw the sample points and rays in the editor
    void OnDrawGizmos()
    {
        if (lastSamplePoints == null || lastHits == null) return;
        if (mainCam == null) return;

        Vector3 camPos = mainCam.transform.position;

        for (int i = 0; i < lastSamplePoints.Length; i++)
        {
            Gizmos.color = lastHits[i] ? Color.green : Color.red;
            Gizmos.DrawSphere(lastSamplePoints[i], 0.05f);

            // Draw ray from camera to sample point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(camPos, lastSamplePoints[i]);
        }
    }
    */
}

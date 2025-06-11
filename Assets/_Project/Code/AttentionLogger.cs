using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttentionLogger : MonoBehaviour
{
    public Camera playerCam; // Assign your camera here if not Camera.main

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Attention Indicated");
            LogVisibleSquares();
        }
    }

    void LogVisibleSquares()
    { 
        var visible = POIManager.Instance.GetVisiblePOIs();
        Vector3 camPos = playerCam.transform.position;
        Vector3 camForward = playerCam.transform.forward;
        float timestamp = Time.time;

        foreach (var poi in visible)
        { 
            Vector3 toPoi = poi.transform.position - camPos;
            float distance = toPoi.magnitude;
            //float rawDot = Vector3.Dot(camForward, toSquare.normalized);
            // throw away any "behind" values, so dot now runs 0 to 1 rather than -1 to 1
            //float dot = Mathf.Clamp01(rawDot);

            float angle = Vector3.Angle(camForward, toPoi); // in degrees, 0...180
            // remap 0-90 deg to 1...0
            float dot90 = 1f - Mathf.Clamp01(angle / 90f);

            // Replace with file/data logging as needed
            Debug.Log($"[{timestamp:F2}] Square ID: {poi.id}, Color: {poi.colourType}, Distance: {distance:F2}, Dot90: {dot90:F2}");

        }
    }
}

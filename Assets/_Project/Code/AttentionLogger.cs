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

        foreach (var square in visible)
        { 
            Vector3 toSquare = square.transform.position - camPos;
            float distance = toSquare.magnitude;
            float dot = Vector3.Dot(camForward, toSquare.normalized);

            // Replace with file/data logging as needed
            Debug.Log($"[{timestamp:F2}] Square ID: {square.id}, Color: {square.colourType}, Distance: {distance:F2}, Dot: {dot:F2}");

        }
    }
}

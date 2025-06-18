using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Thin transport layer: receives a pre-formatted CSV line and POSTSs
/// it to the research server. TelemtryManager handles batching,
/// throttling, and formatting, so this class stays small.
/// </summary>
public class MyWebGLPoster : MonoBehaviour, IRowPoster
{
    [Tooltip("Absolute URL of the endpoint that accepts a CSV row payload.")]
    [SerializeField]
    private string _endpoint =
        "https://my-small-research-server.onrender.com/submit-data";

    /// <summary>
    /// Optional callback: UI elements can subscribe to show success/error.
    /// </summary>
    public event Action<string> OnPostResult;

    // ---------------------------------------------------------------------
    // IRowPoster implementation — called by TelemetryManager
    // ---------------------------------------------------------------------
    public void PostRow(string csvRow)
    {
        if (string.IsNullOrWhiteSpace(csvRow))
            return;

        StartCoroutine(PostCsvRow(csvRow));
    }

    // ---------------------------------------------------------------------
    // Coroutine that actually does the POST
    // ---------------------------------------------------------------------
    private IEnumerator PostCsvRow(string row)
    {
        // For simplicity we wrap the CSV line in a JSON object:
        // { "row": "EventKind,Time,PosX,..." }
        string jsonPayload = $"{{\"row\":\"{EscapeForJson(row)}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using UnityWebRequest request =
            new UnityWebRequest(_endpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer()
            };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result is UnityWebRequest.Result.ConnectionError
            or UnityWebRequest.Result.ProtocolError)
        {
            string msg = $"POST error: {request.error}";
            Debug.Log(msg);
            OnPostResult?.Invoke(msg);
        }
        else
        { 
            string msg = $"POST ok: {request.downloadHandler.text}";
            Debug.Log(msg);
            OnPostResult?.Invoke(msg);
        }
            
    }

    // Simple JSON string escape (quotes and backslashes)
    private static string EscapeForJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Thin transport layer: receives a pre-formatted CSV line and POSTSs
/// it to the research server. TelemtryManager handles batching,
/// throttling, and formatting, so this class stays small.
/// </summary>
public class AzureTelemetryPoster : MonoBehaviour, IRowPoster
{
    [Tooltip("Absolute URL of the endpoint that accepts a CSV row payload.")]
    private string _endpoint =
        "https://func-attention-maze-fubda4b0ezbqa5ed.uksouth-01.azurewebsites.net/api/submit";

    //"https://my-small-research-server.onrender.com/submit-data"

    // Session id generated once per app run
    private string _sessionId;

    private readonly Queue<string> _queue = new();
    private bool _sending;
    private const int MaxRetries = 3;


    /// <summary>
    /// Optional callback: UI elements can subscribe to show success/error.
    /// </summary>
    public event Action<string> OnPostResult;

    private void Awake()
    {
        _sessionId = Guid.NewGuid().ToString();
        _endpoint = (_endpoint ?? "").Trim(); // <- important for WebGL & Editor
    }

    public void PostRow(string csvRow)
    {
        if (string.IsNullOrWhiteSpace(csvRow)) return;
        _queue.Enqueue(csvRow);
        if (!_sending) StartCoroutine(SendLoop());
    }

    private IEnumerator SendLoop()
    { 
        _sending = true;

        while (_queue.Count > 0)
        {
            string row = _queue.Peek();     // don't dequeue until success
            int attempt = 0;
            int backoffMs = 1000;

            while (true)
            {
                attempt++;

                // Build JSON with optional meta (seq helps with dedupe later)
                string jsonPayload = $"{{\"session_id\":\"{_sessionId}\",\"row\":\"{EscapeForJson(row)}\"}}";


                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

                using var request = new UnityWebRequest(_endpoint, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(bodyRaw),
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 10
                };
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();

                bool ok = request.result is UnityWebRequest.Result.Success &&
                          request.responseCode >= 200 && request.responseCode < 300;

                if (ok)
                {
                    _queue.Dequeue();      // remove the row we just sent
                    OnPostResult?.Invoke($"ok {request.responseCode}");
                    break;                 // move to the next queue item
                }

                // Failed: either retry or give up on this row (but keep queue moving)
                if (attempt >= MaxRetries)
                {
                    Debug.LogWarning($"POST failed after {attempt} tries: {request.responseCode} {request.error}");
                    OnPostResult?.Invoke($"error {request.responseCode} {request.error}");
                    _queue.Dequeue();  // drop this row after max retries
                    break;
                }

                // Exponential backoff
                yield return new WaitForSeconds(backoffMs / 1000f);
                backoffMs = Math.Min(backoffMs * 2, 8000); // cap at 8s
            }
        }

        _sending = false;
    }


    /*
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
        Debug.Log("Posting Row");
        // For simplicity we wrap the CSV line in a JSON object:
        // { "row": "EventKind,Time,PosX,..." }
        string jsonPayload = $"{{\"session_id\":\"{_sessionId}\",\"row\":\"{EscapeForJson(row)}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using UnityWebRequest request =
            new UnityWebRequest(_endpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer()
            };

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.timeout = 10;

        Debug.Log($"POST → {_endpoint}  bytes:{bodyRaw.Length}");
        yield return request.SendWebRequest();
        Debug.Log($"← {(long)request.responseCode} {request.error}  Body: {request.downloadHandler.text}");


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
    */

    // Simple JSON string escape (quotes and backslashes)
    private static string EscapeForJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"");


    private IEnumerator WarmUpServer(string healthURL)
    {
        using var req = UnityWebRequest.Get(healthURL);
        req.timeout = 5;
        yield return req.SendWebRequest();
        Debug.Log($"Warm-up → {(long)req.responseCode} {(req.error ?? "ok")}");
    }

    public void TriggerWarmUpServer()
    {
        StartCoroutine(WarmUpServer("https://func-attention-maze-fubda4b0ezbqa5ed.uksouth-01.azurewebsites.net/api/health"));
    }
}

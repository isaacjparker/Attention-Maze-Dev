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
    private string _endpoint = "";   // submit URL from config
    private string _warmupUrl = "";  // warmup/health URL from config

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
    }

    public void PostRow(string csvRow)
    {
        if (string.IsNullOrWhiteSpace(csvRow)) return;
        _queue.Enqueue(csvRow);
        if (!_sending) StartCoroutine(SendLoop());
    }

    public void SetEndPoints(string endpoint, string warmupUrl)
    {
        _endpoint = (endpoint ?? "").Trim();
        _warmupUrl = (warmupUrl ?? "").Trim();

        if (string.IsNullOrEmpty(_endpoint))
            Debug.LogWarning("[AzureTelemetryPoster] server_url is empty; will not send until provided.");

        // If we already have a queue and weren't sending, kick the loop now that we’re configured
        if (!string.IsNullOrEmpty(_endpoint) && _queue.Count > 0 && !_sending)
            StartCoroutine(SendLoop());
    }


    private IEnumerator SendLoop()
    { 
        _sending = true;

        // If not configured yet, pause sending but keep the queue (no drops)
        if (string.IsNullOrEmpty(_endpoint))
        {
            _sending = false;
            yield break;
        }

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

    // Simple JSON string escape (quotes and backslashes)
    private static string EscapeForJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"");


    private IEnumerator WarmUpServer(string healthURL)
    {
        using var req = UnityWebRequest.Get(healthURL);
        req.timeout = 12;
        yield return req.SendWebRequest();
        Debug.Log($"Warm-up → {(long)req.responseCode} {(req.error ?? "ok")}");
    }

    public void TriggerWarmUpServer()
    {
        if (!string.IsNullOrEmpty(_warmupUrl))
            StartCoroutine(WarmUpServer(_warmupUrl));
        else
            Debug.Log("[AzureTelemetryPoster] No warmup_url configured; skipping warm-up.");
    }

}

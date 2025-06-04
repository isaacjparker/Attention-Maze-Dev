using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MyWebGLPoster : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public string username;
        public int score;
    }

    public event Action<string> OnPostResult;

    public void SendDataToServer(string username, int score)
    {
        PlayerData pd = new PlayerData { username = username, score = score };
        StartCoroutine(PostData("https://my-small-research-server.onrender.com/submit-data", pd));
    }

    IEnumerator PostData(string url, PlayerData data)
    { 
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMessage = "Error: " + request.error;
            //Debug.LogError(errorMessage);

            // 2) Invoke the event so subscribers receive the error message
            OnPostResult?.Invoke(errorMessage);
        }
        else
        {
            string successMessage = "Success! Response: " + request.downloadHandler.text;
            //Debug.Log(successMessage);

            // 3) Invoke the event so subscribers receive the success message
            OnPostResult?.Invoke(successMessage);
        }
    }
}

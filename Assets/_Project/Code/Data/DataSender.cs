using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;

public class DataSender : MonoBehaviour
{

    private string formURL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSeXEr4-n8p_fAmzz0JBY0hPboGENIcDjbbd_YL4TQVonOca_w/formResponse";

    public void SubmitData(string sessionID, string checkpointNumber, string checkpointType, string checkpointTime)
    {
        StartCoroutine(Post(sessionID, checkpointNumber, checkpointType, checkpointTime));
    }

    private IEnumerator Post(string sessionID, string checkpointNumber, string checkpointType, string checkpointTime)
    { 
        WWWForm form = new WWWForm();

        form.AddField("entry.747167582", sessionID);
        form.AddField("entry.1144242060", checkpointNumber);
        form.AddField("entry.1247283355", checkpointType);
        form.AddField("entry.118700046", checkpointTime);

        using (UnityWebRequest www = UnityWebRequest.Post(formURL, form))
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data submited succesfully");
            }
            else
            {
                Debug.LogError("Error in data submission. " + www.error);
            }
        }
    }
}

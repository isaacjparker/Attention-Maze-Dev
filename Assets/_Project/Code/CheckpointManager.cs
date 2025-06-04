using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public enum CheckpointType
{ 
    START,
    STRAIGHT,
    CORNER,
    END
}

public class CheckpointManager : MonoBehaviour
{
    [Header("Checkpoints")]
    public List<Checkpoint> Checkpoints = new List<Checkpoint>(); // List of Checkpoints to populate

    [Space]
    [Header("Checkpoint UI")]
    [SerializeField] private TextMeshProUGUI checkpointTMPro;
    [SerializeField] private TextMeshProUGUI timeTMPro;
    [SerializeField] private TextMeshProUGUI typeTMPro;
    [SerializeField] private TextMeshProUGUI filePathTMPro;

    [Header("WebGLPoster")]
    [SerializeField] private MyWebGLPoster myWebGLPoster;

    [Header("Data Sender")]
    [SerializeField] private DataSender dataSender;

    private float sessionTimer = 0f;
    private string sessionID;

    private string fileName;
    private string filePathConcat;
    private string filePath;

    void OnEnable()
    {
        if (dataSender == null)
        { 
            GetComponent<DataSender>();
        }

        // Clear the list just in case there are any pre-existing entries
        Checkpoints.Clear();

        // Loop through each child of the current GameObject
        foreach (Transform child in transform)
        {
            // Get the checkpoint component from the child object
            //Checkpoint checkpoint = child.GetComponent<Checkpoint>();
            Checkpoint checkpoint = child.GetComponentInChildren<Checkpoint>();

            // Check this component exists
            if (checkpoint == null)
            {
                // If not, log error
                Debug.LogError("Maze contains objects without Checkpoint script");
                return;
            }

            // Add the child transform to the checkpoints list
            Checkpoints.Add(checkpoint);
        }

        if (myWebGLPoster != null)
        {
            myWebGLPoster.OnPostResult += HandlePOSTResult;
        }
        

    }

    private void Start()
    {
        sessionID = DateTime.Now.ToString("ddMMyyyy_HHmmss");

        // Define the path to the CSV file
        fileName = $"checkpoint_log_{sessionID}.csv";
        filePath = Application.persistentDataPath;
        filePathConcat = Path.Combine(Application.persistentDataPath, fileName);

        myWebGLPoster.SendDataToServer("MagicUser", (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    private void OnDisable()
    {
        //Debug.Log("CSV file is being saved to: " + filePathConcat);

        if (myWebGLPoster != null)
        {
            myWebGLPoster.OnPostResult -= HandlePOSTResult;
        }
    }

    public void ActivateCheckpoint(int checkpointIndex)
    {
        sessionTimer = Time.timeSinceLevelLoad;


        //WriteCheckpointToDataSender(checkpointIndex);

        //UpdateCheckpointUI(checkpointIndex);
    }

    private void WriteCheckpointToDataSender(int checkpointIndex)
    {
        string checkpointType = Checkpoints[checkpointIndex].CheckpointType.ToString();
        string checkpointNumber = checkpointIndex.ToString();
        string checkpointTime = sessionTimer.ToString();
        //dataSender.SubmitData(sessionID, checkpointNumber, checkpointType, checkpointTime);
    }

    private void WriteCheckpointToCSV(int checkpointIndex)
    {
        string checkpointType = Checkpoints[checkpointIndex].CheckpointType.ToString();
        string message = $"SessionID {sessionID} has reached checkpoint {checkpointIndex} of type: {checkpointType} at {sessionTimer} seconds.";
        Debug.Log(message);

        // Prepare CSV line
        string csvLine = $"{sessionID},{checkpointIndex},{checkpointType},{sessionTimer}";

        try
        {
            // Append the CSV line to the file
            File.AppendAllText(filePathConcat, csvLine + Environment.NewLine);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write to CSV file: " + e.Message);
        }
    }

    private void UpdateCheckpointUI(int checkpointIndex)
    {
        string checkpointType = Checkpoints[checkpointIndex].CheckpointType.ToString();

        checkpointTMPro.text = "| " + checkpointIndex.ToString();
        timeTMPro.text = "| " + sessionTimer.ToString();
        typeTMPro.text = "| " + checkpointType.ToString();

        filePathTMPro.text = filePath.ToString();
    }

    private void HandlePOSTResult(string message)
    {
        // For example, display it on a Text or TMP UI object
        Debug.Log("UIResultHandler received: " + message);

        filePathTMPro.text = message;
    }
}

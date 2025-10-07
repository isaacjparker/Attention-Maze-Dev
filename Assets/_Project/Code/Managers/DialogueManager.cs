using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Start Dialogue Components")]
    [SerializeField] private GameObject _startDialogue;
    [Space]
    [Header("End Dialogue Components")]
    [SerializeField] private GameObject _endDialogue;
    [SerializeField] private TextMeshProUGUI _endDialogueTMProUGUI;
    [SerializeField] private Button _exitButton;
    [SerializeField] private TextMeshProUGUI _exitButtonTMProUGUI;

    // Config cache
    private bool _showEndDialogueScreen;
    private string _endDialogueText;
    private DialogueAlignment _endDialogueAlignment;
    private float _dialogueTextSize;
    private string _exitButtonText;


    private void OnDisable()
    {
        if (Director.Instance != null)
        {
            Director.Instance.OnApplicationSetup -= ApplyConfig;
            Director.Instance.OnApplicationRun -= BeginRun;
            Director.Instance.OnApplicationEnd -= EndRun;
        }

        if (_exitButton != null)
            _exitButton.onClick.RemoveListener(OnExitButtonPressed);
    }

    private void Start()
    {
        if (Director.Instance != null && Director.Instance.IsSetupReady)
        {
            ApplyConfig();
        }
        else if (Director.Instance != null)
        {
            Director.Instance.OnApplicationSetup += ApplyConfig;
        }

        if (Director.Instance != null && Director.Instance.IsRunReady)
        {
            BeginRun();
        }
        else if (Director.Instance != null)
        {
            Director.Instance.OnApplicationRun += BeginRun;
        }

        if (Director.Instance != null)
        {
            Director.Instance.OnApplicationEnd += EndRun;
        }

        if (_exitButton != null)
            _exitButton.onClick.AddListener(OnExitButtonPressed);
    }

    private void ApplyConfig()
    {
        // Unsubscribe if we were waiting
        if (Director.Instance != null)
            Director.Instance.OnApplicationSetup -= ApplyConfig;

        if (ConfigService.Instance == null)
            return;

        // Cache variables
        _showEndDialogueScreen = ConfigService.Instance.ShowEndDialogueScreen;
        _endDialogueText = ConfigService.Instance.EndDialogueText;
        _endDialogueAlignment = ConfigService.Instance.EndDialogueAlignment;
        _dialogueTextSize = ConfigService.Instance.DialogueTextSize;
        _exitButtonText = ConfigService.Instance.ExitButtonText;

        _endDialogueTMProUGUI.text = _endDialogueText ?? "";
        _endDialogueTMProUGUI.enableAutoSizing = false;
        _endDialogueTMProUGUI.fontSize = _dialogueTextSize; // int → float ok
        _endDialogueTMProUGUI.ForceMeshUpdate();

        _exitButtonTMProUGUI.text = string.IsNullOrEmpty(_exitButtonText) ? "Continue" : _exitButtonText;

        switch (_endDialogueAlignment)
        {
            case DialogueAlignment.LEFT:
                if (_endDialogueTMProUGUI) _endDialogueTMProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Left;
                break;
            case DialogueAlignment.RIGHT:
                if (_endDialogueTMProUGUI) _endDialogueTMProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Right;
                break;
            case DialogueAlignment.CENTER:
                if (_endDialogueTMProUGUI) _endDialogueTMProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Center;
                break;
            case DialogueAlignment.JUSTIFY:
                if (_endDialogueTMProUGUI) _endDialogueTMProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Justified;
                break;
        }

    }

    private void BeginRun()
    {
        // ApplyConfig(): ensure end panel starts hidden (race-safe)
        if (_endDialogue != null && _endDialogue.activeSelf)
            _endDialogue.SetActive(false);

        // If you also want to be safe:
        if (_startDialogue != null && _startDialogue.activeSelf)
            _startDialogue.SetActive(false);
    }

    private void EndRun()
    {
        Debug.Log("Dialogue Manager Is Ending Run");

        if (!_showEndDialogueScreen)
        {
            Debug.Log("Show End Dialogue == false");
            return;
        }

        if (_endDialogue == null)
        {
            Debug.Log("End Dialogue object is null");
        }

        Debug.Log("Setting End Dialogue to Active");
        
        _endDialogue.SetActive(true);
    }

    private void OnExitButtonPressed()
    {
        TelemetryManager.Instance?.LinkToWebPage();
    }
}

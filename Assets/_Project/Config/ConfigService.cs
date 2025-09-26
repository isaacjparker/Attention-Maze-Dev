using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class ConfigService : MonoBehaviour
{
    public static ConfigService Instance { get; private set; }

    [Header("Inputs")]
    [Tooltip("Fallback defaults (ScriptableObject).")]
    [SerializeField] private MazeSettings defaultsSO;
    [Tooltip("Local CSV preset e.g. TrialA.csv")]
    [SerializeField] private TextAsset csvPreset;

    public bool IsResolved { get; private set; }
    public event Action OnResolved;

    // Public read-only properties
    // Maze Settings
    public string ConfigName => current?.ConfigName ?? defaultsSO.ConfigName;
    public string Version => current?.Version ?? "";
    public bool AutoLogging => current?.AutoLogging ?? defaultsSO.AutoLogging;
    public float AutoLoggingIntervalSec => current?.AutoLoggingIntervalSec ?? defaultsSO.AutoLoggingIntervalSec;
    public bool ManualLocomotion => current?.ManualLocomotion ?? defaultsSO.ManualLocomotion;
    public float LocomotionSpeed => current?.LocomotionSpeed ?? defaultsSO.LocomotionSpeed;
    public bool ManualLook => current?.ManualLook ?? defaultsSO.ManualLook;
    public float LookSpeed => current?.LookSpeed ?? defaultsSO.LookSpeed;
    public bool ManualLogging => current?.ManualLogging ?? defaultsSO.ManualLogging;

    // Start Dialogue Screen Settings
    public bool ShowStartDialogueScreen => current?.ShowStartDialogueScreen ?? defaultsSO.ShowStartDialogueScreen;
    public string StartDialogueText => current?.StartDialogueText ?? defaultsSO.StartDialogueText;
    public DialogueAlignment StartDialogueAlignment => current?.StartDialogueAlignment ?? defaultsSO.StartDialogueAlignment;
    public bool ShowStartButton => current?.ShowStartButton ?? defaultsSO.ShowStartButton;
    public float AutoStartTimer => current?.AutoStartTimer ?? defaultsSO.AutoStartTimer;

    // End Dialogue Screen Settings
    public bool ShowEndDialogueScreen => current?.ShowEndDialogueScreen ?? defaultsSO.ShowEndDialogueScreen;
    public string EndDialogueText => current?.EndDialogueText ?? defaultsSO.EndDialogueText;
    public DialogueAlignment EndDialogueAlignment => current?.EndDialogueAlignment ?? defaultsSO.EndDialogueAlignment;
    public bool ShowExitButton => current?.ShowExitButton ?? defaultsSO.ShowExitButton;
    public float AutoExitTimer => current?.AutoExitTimer ?? defaultsSO.AutoExitTimer;

    // URL Button Settings
    public bool ShowLinkButton => current?.ShowLinkButton ?? defaultsSO.ShowLinkButton;
    public string LinkButtonURL => current?.LinkButtonURL ?? defaultsSO.LinkButtonURL;
    
    public string SourceCsvName => current?.SourceCsvName ?? "";
    public IReadOnlyDictionary<string, string> Snapshot => current?.Snapshot ?? fallbackSnapshot;

    private ConfigData current;
    private Dictionary<string, string> fallbackSnapshot;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build fallback snapshot from defaults
        fallbackSnapshot = new Dictionary<string, string>
        {
            ["config_name"] = NullSafe(defaultsSO.ConfigName),
            ["version"] = NullSafe(defaultsSO.Version),
            ["auto_logging"] = defaultsSO.AutoLogging ? "true" : "false",
            ["auto_logging_interval_sec"] = defaultsSO.AutoLoggingIntervalSec.ToString("0.###", CultureInfo.InvariantCulture),
            ["manual_locomotion"] = defaultsSO.ManualLocomotion ? "true" : "false",
            ["locomotion_speed"] = defaultsSO.LocomotionSpeed.ToString("0.###", CultureInfo.InvariantCulture),
            ["manual_look"] = defaultsSO.ManualLook ? "true" : "false",
            ["look_speed"] = defaultsSO.LookSpeed.ToString("0.###", CultureInfo.InvariantCulture),
            ["manual_logging"] = defaultsSO.ManualLogging ? "true" : "false",
            ["show_start_dialogue_screen"] = defaultsSO.ShowStartDialogueScreen ? "true" : "false",
            ["start_dialogue_text"] = NullSafe(defaultsSO.StartDialogueText),
            ["start_dialogue_alignment"] = defaultsSO.StartDialogueAlignment.ToString().ToUpperInvariant(),
            ["show_start_button"] = defaultsSO.ShowStartButton ? "true" : "false",
            ["auto_start_timer"] = defaultsSO.AutoStartTimer.ToString("0.###", CultureInfo.InvariantCulture),
            ["show_end_dialogue_screen"] = defaultsSO.ShowEndDialogueScreen ? "true" : "false",
            ["end_dialogue_text"] = NullSafe(defaultsSO.EndDialogueText),
            ["end_dialogue_alignment"] = defaultsSO.EndDialogueAlignment.ToString().ToUpperInvariant(),
            ["show_exit_button"] = defaultsSO.ShowExitButton ? "true" : "false",
            ["auto_exit_timer"] = defaultsSO.AutoExitTimer.ToString("0.###", CultureInfo.InvariantCulture),
            ["show_link_button"] = defaultsSO.ShowLinkButton ? "true" : "false",
            ["link_button_url"] = NullSafe(defaultsSO.LinkButtonURL)
        };

        ResolveOnce();
    }

    private void ResolveOnce()
    { 
        // 1) Start merged KeyaValues with defaults
        Dictionary<string, string> merged = new Dictionary<string, string>(fallbackSnapshot);

        // 2) Overlay CSV
        string csvName = "";
        if (csvPreset != null && !string.IsNullOrWhiteSpace(csvPreset.text))
        {
            csvName = csvPreset.name + ".csv";
            try
            {
                foreach ((string k, string v) in ParseCsvKeyValue(csvPreset.text))
                {
                    if (string.IsNullOrWhiteSpace(k)) continue;
                    merged[k] = v;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[ConfigService] CSV parse failed; using defaults. {e.Message} ");
            }
        }

        // 3) Coerce + validate (case-insensitive keys assumed)
        string configName = GetOr(merged, "config_name", fallbackSnapshot["config_name"]);
        string version = GetOr(merged, "version", fallbackSnapshot["version"]);

        bool autoLog = CoerceBool(GetOr(merged, "auto_logging", fallbackSnapshot["auto_logging"]), defaultsSO.AutoLogging);
        float autoInt = CoerceFloat(GetOr(merged, "auto_logging_interval_sec", fallbackSnapshot["auto_logging_interval_sec"]), defaultsSO.AutoLoggingIntervalSec);
        if (autoInt < 0.1f) autoInt = 0.1f;

        bool manualLoc = CoerceBool(GetOr(merged, "manual_locomotion", fallbackSnapshot["manual_locomotion"]), defaultsSO.ManualLocomotion);
        float locomotion = CoerceFloat(GetOr(merged, "locomotion_speed", fallbackSnapshot["locomotion_speed"]), defaultsSO.LocomotionSpeed);

        bool manualLook = CoerceBool(GetOr(merged, "manual_look", fallbackSnapshot["manual_look"]), defaultsSO.ManualLook);
        float lookSpeed = CoerceFloat(GetOr(merged, "look_speed", fallbackSnapshot["look_speed"]), defaultsSO.LookSpeed);

        bool manualLog = CoerceBool(GetOr(merged, "manual_logging", fallbackSnapshot["manual_logging"]), defaultsSO.ManualLogging);

        bool showStart = CoerceBool(GetOr(merged, "show_start_dialogue_screen", fallbackSnapshot["show_start_dialogue_screen"]), defaultsSO.ShowStartDialogueScreen);
        string startText = CoerceString(GetOr(merged, "start_dialogue_text", fallbackSnapshot["start_dialogue_text"]));
        var startAlign = CoerceEnum(GetOr(merged, "start_dialogue_alignment", fallbackSnapshot["start_dialogue_alignment"]), defaultsSO.StartDialogueAlignment);

        bool showStartBtn = CoerceBool(GetOr(merged, "show_start_button", fallbackSnapshot["show_start_button"]), defaultsSO.ShowStartButton);
        float autoStart = CoerceFloat(GetOr(merged, "auto_start_timer", fallbackSnapshot["auto_start_timer"]), defaultsSO.AutoStartTimer);

        bool showEnd = CoerceBool(GetOr(merged, "show_end_dialogue_screen", fallbackSnapshot["show_end_dialogue_screen"]), defaultsSO.ShowEndDialogueScreen);
        string endText = CoerceString(GetOr(merged, "end_dialogue_text", fallbackSnapshot["end_dialogue_text"]));
        var endAlign = CoerceEnum(GetOr(merged, "end_dialogue_alignment", fallbackSnapshot["end_dialogue_alignment"]), defaultsSO.EndDialogueAlignment);

        bool showExitBtn = CoerceBool(GetOr(merged, "show_exit_button", fallbackSnapshot["show_exit_button"]), defaultsSO.ShowExitButton);
        float autoExit = CoerceFloat(GetOr(merged, "auto_exit_timer", fallbackSnapshot["auto_exit_timer"]), defaultsSO.AutoExitTimer);

        bool showLinkBtn = CoerceBool(GetOr(merged, "show_link_button", fallbackSnapshot["show_link_button"]), defaultsSO.ShowLinkButton);
        string linkUrl = CoerceString(GetOr(merged, "link_button_url", fallbackSnapshot["link_button_url"]));

        // 4) Freeze snapshot (stringified for easy logging)
        Dictionary<string, string> snap = new Dictionary<string, string>
        {
            ["config_name"] = configName,
            ["version"] = version,
            ["auto_logging"] = autoLog ? "true" : "false",
            ["auto_logging_interval_sec"] = autoInt.ToString("0.###", CultureInfo.InvariantCulture),
            ["manual_locomotion"] = manualLoc ? "true" : "false",
            ["locomotion_speed"] = locomotion.ToString("0.###", CultureInfo.InvariantCulture),
            ["manual_look"] = manualLook ? "true" : "false",
            ["look_speed"] = lookSpeed.ToString("0.###", CultureInfo.InvariantCulture),
            ["manual_logging"] = manualLog ? "true" : "false",
            ["show_start_dialogue_screen"] = showStart ? "true" : "false",
            ["start_dialogue_text"] = startText,
            ["start_dialogue_alignment"] = startAlign.ToString().ToUpperInvariant(),
            ["show_start_button"] = showStartBtn ? "true" : "false",
            ["auto_start_timer"] = autoStart.ToString("0.###", CultureInfo.InvariantCulture),
            ["show_end_dialogue_screen"] = showEnd ? "true" : "false",
            ["end_dialogue_text"] = endText,
            ["end_dialogue_alignment"] = endAlign.ToString().ToUpperInvariant(),
            ["show_exit_button"] = showExitBtn ? "true" : "false",
            ["auto_exit_timer"] = autoExit.ToString("0.###", CultureInfo.InvariantCulture),
            ["show_link_button"] = showLinkBtn ? "true" : "false",
            ["link_button_url"] = linkUrl
        };

        current = new ConfigData(
            configName,
            version,
            autoLog,
            autoInt,
            manualLoc,
            locomotion,
            manualLook,
            lookSpeed,
            manualLog,
            showStart,
            startText,
            startAlign,
            showStartBtn,
            autoStart,
            showEnd,
            endText,
            endAlign,
            showExitBtn,
            autoExit,
            showLinkBtn,
            linkUrl,
            csvName,
            snap
        );

        Debug.Log("Loco: " + ManualLocomotion + " Look: " + ManualLook + " Log: " + ManualLogging);

        IsResolved = true;
        OnResolved?.Invoke();
    }

    // ----- Helpers -----

    private static string GetOr(Dictionary<string, string> d, string key, string fallback)
        => d.TryGetValue(key, out var v) ? v : fallback;

    private static bool CoerceBool(string s, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(s)) return fallback;
        switch (s.Trim().ToLowerInvariant())
        {
            case "1":
            case "true":
            case "yes":
            case "y":
            case "on":
                return true;
            case "0":
            case "false":
            case "no":
            case "n":
            case "off":
                return false;
            default:
                return fallback;
        }
    }

    private static float CoerceFloat(string s, float fallback)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
            return f;
        return fallback;
    }

    private static string CoerceString(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var t = s.Trim();
        if (string.Equals(t, "null", StringComparison.OrdinalIgnoreCase)) return "";
        return t;
    }

    private static string NullSafe(string s) => s ?? "";

    private static DialogueAlignment CoerceEnum(string s, DialogueAlignment fallback)
    {
        if (string.IsNullOrWhiteSpace(s)) return fallback;
        if (Enum.TryParse<DialogueAlignment>(s.Trim(), true, out var val))
            return val;
        return fallback;
    }

    // Robust CSV reader that supports:
    // - quoted cells
    // - escaped quotes ("")
    // - newlines inside quoted cells
    // Expects a header row with at least "key" and "value".
    private static IEnumerable<(string key, string value)> ParseCsvKeyValue(string csv)
    {
        var rows = new List<List<string>>();
        var cell = new StringBuilder();
        var row = new List<string>();
        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < csv.Length && csv[i + 1] == '\"')
                {
                    cell.Append('\"'); // escaped quote
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                row.Add(cell.ToString());
                cell.Length = 0;
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                // End of row (handle CRLF / LF)
                // If CRLF, skip the LF after CR
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;

                row.Add(cell.ToString());
                cell.Length = 0;

                // Skip completely empty rows
                bool allEmpty = true;
                foreach (var col in row) { if (!string.IsNullOrEmpty(col)) { allEmpty = false; break; } }
                if (!allEmpty) rows.Add(row);

                row = new List<string>();
            }
            else
            {
                cell.Append(c);
            }
        }
        // last cell/row
        row.Add(cell.ToString());
        bool lastEmpty = true;
        foreach (var col in row) { if (!string.IsNullOrEmpty(col)) { lastEmpty = false; break; } }
        if (!lastEmpty) rows.Add(row);

        if (rows.Count == 0) yield break;

        // Map header
        int keyIdx = -1, valIdx = -1;
        for (int i = 0; i < rows[0].Count; i++)
        {
            var h = rows[0][i].Trim().ToLowerInvariant();
            if (h == "key") keyIdx = i;
            if (h == "value") valIdx = i;
        }
        if (keyIdx < 0 || valIdx < 0)
            throw new Exception("CSV must include 'key' and 'value' header columns.");

        for (int r = 1; r < rows.Count; r++)
        {
            var cols = rows[r];
            if (cols.Count <= Math.Max(keyIdx, valIdx)) continue;
            var k = cols[keyIdx].Trim().ToLowerInvariant(); // normalize keys
            var v = cols[valIdx]; // keep raw (we’ll trim/interpret per type)
            yield return (k, v);
        }
    }
}

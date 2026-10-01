using UnityEngine;
using SMMARTS;

/// <summary>
/// Runtime debugging interface for monitoring and controlling microcontroller hardware.
/// Activated with Left Shift + M. Designed for developers who need to troubleshoot
/// SMMARTS Whitebox connectivity and hardware functionality in-situ from the application.
/// </summary>
public class MicrocontrollerBackdoor : MonoBehaviour
{
    public bool showSettings = false;
    public float GUI_ScaleFactorAdjust = 1f;
    public float scaleFactor;

    /// <summary>
    /// Handles keyboard input for toggling the microcontroller debugging interface.
    /// Left Shift + M activates/deactivates the debugging panel.
    /// </summary>
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
            showSettings = !showSettings;
    }

    /// <summary>
    /// Renders the microcontroller debugging GUI using Unity's immediate mode GUI system.
    /// Provides a two-column layout with real-time hardware monitoring on the left
    /// and manual hardware control buttons on the right. Implements DPI scaling
    /// for consistent appearance across different display configurations.
    /// </summary>
    void OnGUI()
    {
        if (!showSettings) return;

        // DPI Scaling System - maintains consistent GUI size across different display densities
        scaleFactor = Screen.dpi / 96f * GUI_ScaleFactorAdjust; // 96 is standard DPI
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));

        // Panel sizing - larger than ultrasound backdoor to accommodate extensive controls
        float panelWidth = (Screen.width * 0.7f) / scaleFactor;
        float panelHeight = (Screen.height * 0.95f) / scaleFactor;

        // Center the panel
        float x = (Screen.width - panelWidth * scaleFactor) / 2 / scaleFactor;
        float y = (Screen.height - panelHeight * scaleFactor) / 2 / scaleFactor;

        // Create a darker, more opaque background style
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.90f)); // Dark with 90% opacity

        // Draw background box with custom style
        GUI.Box(new Rect(x, y, panelWidth, panelHeight), "", boxStyle);

        GUILayout.BeginArea(new Rect(x + 10, y + 10, panelWidth - 20, panelHeight - 20));

        GUILayout.Label("MICROCONTROLLER DEVELOPER ACCESS");

        // Two-column layout: monitoring/configuration on left, hardware control on right
        GUILayout.BeginHorizontal();

        // Left Column - Real-time monitoring and connection configuration
        GUILayout.BeginVertical(GUILayout.Width((panelWidth - 40) * 0.5f));

        // Connection Status Display - shows current connection state and active port
        GUILayout.Label("--- Connection Status ---");
        GUILayout.Label($"Connected: {Microcontroller.ME.Connected}");
        GUILayout.Label($"Connection Status: {Microcontroller.ME.CurrentConnectionStatus}");
        GUILayout.Label($"Current Port: {Microcontroller.ME.CurrentConnectedPort}");

        // Connection Control - runtime connection management
        GUILayout.Label("--- Connection Control ---");
        Microcontroller.ME.DisableMicrocontrollerConnetion = GUILayout.Toggle(Microcontroller.ME.DisableMicrocontrollerConnetion, "Disable Connection");
        GUILayout.Label("Specify COM Port (leave empty for auto):");
        Microcontroller.ME.SpecifyComPortNumber = GUILayout.TextField(Microcontroller.ME.SpecifyComPortNumber);

        // Raw Sensor Data Display - live readings from SMMARTS Whitebox sensors
        GUILayout.Label("--- Raw Data ---");
        GUILayout.Label($"Camera Controller Button: {Microcontroller_Manager.ME.TUIPressed}");
        GUILayout.Label($"US Probe FSR, Ridge Side: {Microcontroller_Manager.ME.ProbeFaceFSR_RidgeSide:F2}");
        GUILayout.Label($"US Probe FSR, Flat Side: {Microcontroller_Manager.ME.ProbeFaceFSR_FlatSide:F2}");
        GUILayout.Label($"Syringe Pressure: {Microcontroller_Manager.ME.SyringePressure:F2}");
        GUILayout.Label("Raw Serial Data:");
        GUILayout.TextArea(Microcontroller.ME.ReplayFormattedMicrocontrollerInputString, GUILayout.Height(60));

        // Debug Output Controls - toggle various levels of debug messaging
        GUILayout.Label("--- Debugging ---");
        Microcontroller.ME.DebuggingActive = GUILayout.Toggle(Microcontroller.ME.DebuggingActive, "Debug Messages");
        Microcontroller.ME.ErrorDebuggingActive = GUILayout.Toggle(Microcontroller.ME.ErrorDebuggingActive, "Error Debug Messages");

        GUILayout.EndVertical();

        // Right Column - Manual hardware control and testing
        GUILayout.BeginVertical(GUILayout.Width((panelWidth - 40) * 0.5f));

        // LED Control - manual control of indicator lights on SMMARTS Whitebox
        GUILayout.Label("--- LED Flashback Lights ---");
        if (GUILayout.Button("Blue Light"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.BlueLight);
        if (GUILayout.Button("Red Light"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.RedLight);
        if (GUILayout.Button("Lights Off"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.LightsOff);

        // LOR Valve Control - Loss of Resistance valve for epidural simulation
        GUILayout.Label("--- Loss Of Resistance (LOR) Valve Actions ---");
        if (GUILayout.Button("Open (LORO)"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.LORO);
        if (GUILayout.Button("Closed (LORC)"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.LORC);

        // Tactile Feedback Control - manual triggering of haptic feedback mechanisms
        GUILayout.Label("--- Tactile Pop Actions ---");
        if (GUILayout.Button("Large Pop (LPop)"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.LPop);
        if (GUILayout.Button("Small Pop (SPop)"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.SPop);

        // Syringe System Control - pressure sensor calibration and reset functions
        GUILayout.Label("--- Syringe Pressure Sensor Control ---");
        if (GUILayout.Button("Zero Syringe Pressure"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.ZeroSyringePressure);
        if (GUILayout.Button("Reset Syringe"))
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.ResetSyringe);

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Creates a solid color texture for GUI styling purposes.
    /// Used to generate the semi-transparent dark background for the debugging panel.
    /// </summary>
    /// <param name="width">Texture width in pixels</param>
    /// <param name="height">Texture height in pixels</param>
    /// <param name="col">Color to fill the texture with</param>
    /// <returns>Texture2D filled with the specified color</returns>
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
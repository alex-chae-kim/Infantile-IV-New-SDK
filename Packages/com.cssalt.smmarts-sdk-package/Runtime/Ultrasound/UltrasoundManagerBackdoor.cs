using UnityEngine;
using SMMARTS;
using System.Collections;

/// <summary>
/// Runtime developer interface for adjusting ultrasound simulation parameters via GUI.
/// Activated with Left Shift + U. Designed for end users and instructors who need
/// parameter control without Unity editor access.
/// </summary>
public class UltrasoundManagerBackdoor : MonoBehaviour
{
    public bool showSettings = false;
    public float GUI_ScaleFactorAdjust = 1f;
    public float scaleFactor;

    // Store default values
    private bool defaultRendering;
    private bool defaultImagedReversed;
    private int defaultDepth_cm;
    private int defaultWidth;
    private int defaultAngle;
    private float defaultBeamThickness_mm;
    private bool defaultBlur;
    private bool defaultDepthAttenuation;
    private float defaultDepthAttenuationFraction;
    private bool defaultAnisotropy;
    private bool defaultDrawAirGaps;
    private float defaultMinimumAirGap_mm;

    void Start()
    {
        StartCoroutine(CaptureDefaultsAfterDelay());
    }

    /// <summary>
    /// Captures ultrasound settings as defaults after a delay to ensure UltrasoundManager
    /// has fully initialized. The 3-second delay prevents race conditions where this
    /// script might run before the main ultrasound system is ready.
    /// </summary>
    private IEnumerator CaptureDefaultsAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        // Capture initial values as defaults
        defaultRendering = UltrasoundManager.ME.Rendering;
        defaultImagedReversed = UltrasoundDisplayController.ME.ImagedReversed;
        defaultDepth_cm = UltrasoundManager.ME.Depth_cm;
        defaultWidth = UltrasoundManager.ME.Width;
        defaultAngle = UltrasoundManager.ME.Angle;
        defaultBeamThickness_mm = UltrasoundManager.ME.BeamThickness_mm;
        defaultBlur = UltrasoundManager.ME.Blur;
        defaultDepthAttenuation = UltrasoundManager.ME.DepthAttenuation;
        defaultDepthAttenuationFraction = UltrasoundManager.ME.DepthAttenuationFraction;
        defaultAnisotropy = UltrasoundManager.ME.Anisotropy;
        defaultDrawAirGaps = UltrasoundManager.ME.DrawAirGaps;
        defaultMinimumAirGap_mm = UltrasoundManager.ME.MinimumAirGap_mm;
        Debug.Log("Default ultrasound settings captured");
    }

    /// <summary>
    /// Handles keyboard input for toggling the backdoor interface.
    /// Left Shift + U activates/deactivates the settings panel.
    /// </summary>
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.U))
            showSettings = !showSettings;
    }

    /// <summary>
    /// Renders the backdoor GUI interface using Unity's immediate mode GUI system.
    /// Implements DPI scaling for consistent appearance across different resolutions.
    /// Provides real-time controls for all major ultrasound simulation parameters.
    /// </summary>
    void OnGUI()
    {
        if (!showSettings) return;

        // DPI Scaling System - maintains consistent GUI size across different display densities
        scaleFactor = Screen.dpi / 96f * GUI_ScaleFactorAdjust; // 96 is standard DPI
        // if (scaleFactor < 1f) scaleFactor = 1f; // Don't scale down
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));

        // Panel sizing and positioning - responsive to screen size
        float panelWidth = (Screen.width * 0.4f) / scaleFactor;
        float panelHeight = (Screen.height * 0.90f) / scaleFactor;

        // Center the panel
        float x = (Screen.width - panelWidth * scaleFactor) / 2 / scaleFactor;
        float y = (Screen.height - panelHeight * scaleFactor) / 2 / scaleFactor;

        // Create a darker, more opaque background style
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.85f)); // Dark with 80% opacity

        // Draw background box with custom style
        GUI.Box(new Rect(x, y, panelWidth, panelHeight), "", boxStyle);

        GUILayout.BeginArea(new Rect(x + 10, y + 10, panelWidth - 20, panelHeight - 20));

        GUILayout.Label("ULTRASOUND DEVELOPER ACCESS");

        // Master rendering toggle and image orientation control side by side
        GUILayout.BeginHorizontal();
        UltrasoundManager.ME.Rendering = GUILayout.Toggle(UltrasoundManager.ME.Rendering, "Rendering On/Off");
        string orientationButtonText = UltrasoundDisplayController.ME.ImagedReversed ? "Orientation: Right" : "Orientation: Left";
        if (GUILayout.Button(orientationButtonText, GUILayout.Width(120)))
        {
            UltrasoundDisplayController.ME.ReverseImage();
        }
        GUILayout.EndHorizontal();

        // Insonation Plane Geometry Controls - depth, width, angle, thickness
        GUILayout.Label("--- Insonation Plane Geometry ---");
        GUILayout.Label($"Depth (cm): {UltrasoundManager.ME.Depth_cm}");
        int newDepth = Mathf.RoundToInt(GUILayout.HorizontalSlider(UltrasoundManager.ME.Depth_cm, 1, 15));
        UltrasoundManager.ME.Depth_cm = newDepth;

        GUILayout.Label($"Width at Probe Face (mm): {UltrasoundManager.ME.Width}");
        int newWidth = Mathf.RoundToInt(GUILayout.HorizontalSlider(UltrasoundManager.ME.Width, 1, 100));
        UltrasoundManager.ME.Width = newWidth;

        GUILayout.Label($"Angle: {UltrasoundManager.ME.Angle}°" + $"   (Spread Angle: {UltrasoundManager.ME.Angle * 2}°)");
        int newSpreadAngle = Mathf.RoundToInt(GUILayout.HorizontalSlider(UltrasoundManager.ME.Angle, 2, 60));
        UltrasoundManager.ME.Angle = newSpreadAngle;

        GUILayout.Label($"Beam Thickness (mm): {UltrasoundManager.ME.BeamThickness_mm:F1}");
        UltrasoundManager.ME.BeamThickness_mm = GUILayout.HorizontalSlider(UltrasoundManager.ME.BeamThickness_mm, 0, 4);

        // Realism Effects - visual enhancements for more realistic ultrasound appearance
        GUILayout.Label("--- Realism Effects ---");
        UltrasoundManager.ME.Blur = GUILayout.Toggle(UltrasoundManager.ME.Blur, "Blur");
        UltrasoundManager.ME.DepthAttenuation = GUILayout.Toggle(UltrasoundManager.ME.DepthAttenuation, "Depth Attenuation");

        float attenuationPercent = UltrasoundManager.ME.DepthAttenuationFraction * 100f;
        GUILayout.Label($"Max Depth Attenuation (%): {attenuationPercent:F0}");
        attenuationPercent = GUILayout.HorizontalSlider(attenuationPercent, 0, 100);
        UltrasoundManager.ME.DepthAttenuationFraction = attenuationPercent / 100f;

        UltrasoundManager.ME.Anisotropy = GUILayout.Toggle(UltrasoundManager.ME.Anisotropy, "Anisotropy");

        // Air Gap Controls - determines how air interfaces affect image visibility
        GUILayout.Label("--- Acceptable Air Gap ---");
        UltrasoundManager.ME.DrawAirGaps = GUILayout.Toggle(UltrasoundManager.ME.DrawAirGaps, "Air Gaps Hide Image");
        GUILayout.Label($"Minimum Tolerable Air Gap (mm): {UltrasoundManager.ME.MinimumAirGap_mm:F1}");
        UltrasoundManager.ME.MinimumAirGap_mm = GUILayout.HorizontalSlider(UltrasoundManager.ME.MinimumAirGap_mm, 0, 10);

        // Reset functionality - restores all parameters to their startup values
        if (GUILayout.Button("Reset to Defaults"))
        {
            UltrasoundManager.ME.Rendering = defaultRendering;
            // Reset image orientation if it's different from default
            if (UltrasoundDisplayController.ME.ImagedReversed != defaultImagedReversed)
            {
                UltrasoundDisplayController.ME.ReverseImage();
            }
            UltrasoundManager.ME.Depth_cm = defaultDepth_cm;
            UltrasoundManager.ME.Width = defaultWidth;
            UltrasoundManager.ME.Angle = defaultAngle;
            UltrasoundManager.ME.BeamThickness_mm = defaultBeamThickness_mm;
            UltrasoundManager.ME.Blur = defaultBlur;
            UltrasoundManager.ME.DepthAttenuation = defaultDepthAttenuation;
            UltrasoundManager.ME.DepthAttenuationFraction = defaultDepthAttenuationFraction;
            UltrasoundManager.ME.Anisotropy = defaultAnisotropy;
            UltrasoundManager.ME.DrawAirGaps = defaultDrawAirGaps;
            UltrasoundManager.ME.MinimumAirGap_mm = defaultMinimumAirGap_mm;
            Debug.Log("Ultrasound settings reset to defaults");
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Creates a solid color texture for GUI styling purposes.
    /// Used to generate the semi-transparent dark background for the settings panel.
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
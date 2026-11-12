using System.Collections.Generic;
using UnityEngine;
using SMMARTS;

/// <summary>
/// Tracks needle insertion into skin, veins and arteries, and records the results
/// through the SMMARTS recording system.
///
/// Events go to EventMarkers.csv (via EventMarkerRecorder) while a session is recording:
///   SkinEntry (attempt number), SkinEntryAngle_deg, VeinEntry, ArteryEntry,
///   VeinExit / ArteryExit ("Withdrawn" or "Backwall"),
///   SkinExit, MaxInsertionLength_mm, MaxSkinDepth_mm (per attempt).
///
/// A per-frame trace goes to NeedleInsertion.csv in the same session folder.
///
/// Uses the SDK's NeedleInteraction (hub-to-tip raycasts) on the same GameObject.
/// Scene units are millimetres.
/// </summary>
[RequireComponent(typeof(NeedleInteraction))]
public class NeedleInsertionMetrics : MonoBehaviour
{
    [Header("Tissue Targets")]
    [Tooltip("Skin mesh(es) with colliders.")]
    public GameObject[] skinObjects;
    [Tooltip("Vein mesh(es) with colliders.")]
    public GameObject[] veinObjects;
    [Tooltip("Artery mesh(es) with colliders.")]
    public GameObject[] arteryObjects;

    [Header("Detection")]
    [Tooltip("Tip must be at least this far past a surface (mm) to count as entered. Filters tracker jitter at the surface.")]
    public float entryThreshold_mm = 0.2f;

    [Header("Recording")]
    [Tooltip("Write a per-frame NeedleInsertion.csv while a session is recording.")]
    public bool writeFrameCsv = true;

    [Header("Live Values (read only)")]
    [SerializeField] private int attempt = 0;
    [SerializeField] private bool inSkin;
    [SerializeField] private bool inVein;
    [SerializeField] private bool inArtery;
    [Tooltip("Tip has gone through the far wall of the vein.")]
    [SerializeField] private bool veinBackwalled;
    [Tooltip("Tip has gone through the far wall of the artery.")]
    [SerializeField] private bool arteryBackwalled;
    [Tooltip("Angle between needle and skin surface at the moment of entry. 0 = parallel, 90 = perpendicular.")]
    [SerializeField] private float entryAngle_deg = float.NaN;
    [Tooltip("Current angle between needle and skin surface.")]
    [SerializeField] private float currentAngle_deg = float.NaN;
    [Tooltip("Length of needle past the skin entry point, along the shaft.")]
    [SerializeField] private float insertionLength_mm;
    [Tooltip("Perpendicular distance of the tip below the skin surface at the entry point.")]
    [SerializeField] private float skinDepth_mm;
    [SerializeField] private float maxInsertionLength_mm;
    [SerializeField] private float maxSkinDepth_mm;

    public int Attempt => attempt;
    public bool InSkin => inSkin;
    public bool InVein => inVein;
    public bool InArtery => inArtery;
    public bool VeinBackwalled => veinBackwalled;
    public bool ArteryBackwalled => arteryBackwalled;
    public float EntryAngleDeg => entryAngle_deg;
    public float InsertionLengthMm => insertionLength_mm;
    public float SkinDepthMm => skinDepth_mm;

    private NeedleInteraction needle;
    private Transform needleHub;
    private Transform needleTip;
    private LayerMask mask;
    private readonly HashSet<GameObject> skinSet = new HashSet<GameObject>();
    private readonly HashSet<GameObject> veinSet = new HashSet<GameObject>();
    private readonly HashSet<GameObject> arterySet = new HashSet<GameObject>();

    private CSVWriter frameCsv;
    private bool wasRecording;

    void Start()
    {
        needle = GetComponent<NeedleInteraction>();
        needleHub = transform.Find("NeedleHubMarker");
        needleTip = transform.Find("NeedleTipMarker");

        int layers = 0;
        AddTargets(skinObjects, skinSet, ref layers);
        AddTargets(veinObjects, veinSet, ref layers);
        AddTargets(arteryObjects, arterySet, ref layers);
        mask = layers;

        if (skinSet.Count == 0)
            Debug.LogWarning($"NeedleInsertionMetrics on {name}: no skin objects assigned.");
    }

    private static void AddTargets(GameObject[] objects, HashSet<GameObject> set, ref int layers)
    {
        if (objects == null) return;
        foreach (var go in objects)
        {
            if (go == null) continue;
            set.Add(go);
            layers |= 1 << go.layer;
        }
    }

    void Update()
    {
        if (needle == null || needleHub == null || needleTip == null) return;

        UpdateRecordingState();

        Vector3 needleDir = (needleTip.position - needleHub.position).normalized;
        NeedleInteractionSummary[] hits = needle.TestPenetration(mask);

        // Closest hit from the hub for each tissue type (hits are sorted by distance from hub).
        NeedleInteractionSummary skinHit = FirstHit(hits, skinSet);
        NeedleInteractionSummary veinHit = FirstHit(hits, veinSet);
        NeedleInteractionSummary arteryHit = FirstHit(hits, arterySet);

        UpdateSkin(skinHit, needleDir);
        inVein = UpdateVessel("Vein", veinHit, inVein, out veinBackwalled);
        inArtery = UpdateVessel("Artery", arteryHit, inArtery, out arteryBackwalled);

        WriteFrame();
    }

    private static NeedleInteractionSummary FirstHit(NeedleInteractionSummary[] hits, HashSet<GameObject> set)
    {
        foreach (var h in hits)
            if (set.Contains(h.gameObject)) return h;
        return null;
    }

    /// <summary>
    /// Angle between the needle shaft and the surface plane at the hit point.
    /// 0 = parallel to skin, 90 = perpendicular.
    /// </summary>
    private static float SurfaceAngle(Vector3 needleDir, Vector3 surfaceNormal)
    {
        return 90f - Vector3.Angle(needleDir, -surfaceNormal);
    }

    private void UpdateSkin(NeedleInteractionSummary skinHit, Vector3 needleDir)
    {
        bool penetrating = skinHit != null && skinHit.isPenetrating;
        float length = penetrating ? skinHit.distanceFromTip : 0f;

        currentAngle_deg = skinHit != null ? SurfaceAngle(needleDir, skinHit.hit.normal) : float.NaN;

        if (!inSkin && penetrating && length >= entryThreshold_mm)
        {
            inSkin = true;
            attempt++;
            entryAngle_deg = currentAngle_deg;
            maxInsertionLength_mm = 0f;
            maxSkinDepth_mm = 0f;

            RecordEvent("SkinEntry", attempt);
            RecordEvent("SkinEntryAngle_deg", entryAngle_deg);
        }
        else if (inSkin && !penetrating)
        {
            inSkin = false;
            RecordEvent("SkinExit", attempt);
            RecordEvent("MaxInsertionLength_mm", maxInsertionLength_mm);
            RecordEvent("MaxSkinDepth_mm", maxSkinDepth_mm);
        }

        if (inSkin)
        {
            insertionLength_mm = length;
            // Perpendicular depth of the tip below the skin surface at the entry point.
            skinDepth_mm = Mathf.Max(0f, Vector3.Dot(skinHit.hit.point - needleTip.position, skinHit.hit.normal));
            maxInsertionLength_mm = Mathf.Max(maxInsertionLength_mm, insertionLength_mm);
            maxSkinDepth_mm = Mathf.Max(maxSkinDepth_mm, skinDepth_mm);
        }
        else
        {
            insertionLength_mm = 0f;
            skinDepth_mm = 0f;
        }
    }

    /// <summary>
    /// Tip is "in" a vessel when it is past the near wall but not through the far wall.
    /// </summary>
    private bool UpdateVessel(string label, NeedleInteractionSummary hit, bool wasIn, out bool backwalled)
    {
        bool pastNearWall = hit != null && hit.isPenetrating;
        backwalled = pastNearWall && hit.isBackwalled;
        bool inside = pastNearWall && !backwalled;

        if (!wasIn && inside && hit.distanceFromTip >= entryThreshold_mm)
        {
            RecordEvent(label + "Entry", attempt);
            return true;
        }
        if (wasIn && !inside)
        {
            RecordEvent(label + "Exit", backwalled ? "Backwall" : "Withdrawn");
            return false;
        }
        return wasIn;
    }

    private static void RecordEvent(string eventName, object value)
    {
        if (EventMarkerRecorder.ME != null)
            EventMarkerRecorder.ME.RecordEvent(eventName, value);
    }

    // ---- Per-frame CSV, tied to the RecordingManager session lifecycle ----

    private void UpdateRecordingState()
    {
        var rm = RecordingManager.ME;
        bool recording = rm != null && rm.IsRecording;

        if (recording && !wasRecording && writeFrameCsv)
        {
            string[] headers =
            {
                "Time", "Attempt", "InSkin", "InVein", "InArtery", "VeinBackwalled", "ArteryBackwalled",
                "InsertionLength_mm", "SkinDepth_mm", "CurrentAngle_deg", "EntryAngle_deg"
            };
            frameCsv = new CSVWriter(rm.CurrentSessionDirectory, "NeedleInsertion.csv", rm.CurrentSessionID, headers);
        }
        else if (!recording && wasRecording)
        {
            CloseCsv();
        }

        wasRecording = recording;
    }

    private void WriteFrame()
    {
        var rm = RecordingManager.ME;
        if (frameCsv == null || rm == null || rm.IsPaused) return;

        frameCsv.WriteRow(
            Time.time - rm.StartTime,
            attempt, inSkin, inVein, inArtery, veinBackwalled, arteryBackwalled,
            insertionLength_mm, skinDepth_mm,
            float.IsNaN(currentAngle_deg) ? "" : (object)currentAngle_deg,
            inSkin ? (object)entryAngle_deg : "");
    }

    private void CloseCsv()
    {
        if (frameCsv != null)
        {
            frameCsv.Close();
            frameCsv = null;
        }
    }

    void OnDisable() => CloseCsv();
    void OnApplicationQuit() => CloseCsv();
}

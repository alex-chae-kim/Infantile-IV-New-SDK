using UnityEngine;
using SMMARTS;

/// <summary>
/// Drives the whitebox lights from the needle's vessel state:
///   needle tip in an artery -> Red Light
///   needle tip in a vein    -> Blue Light
///   neither                 -> Lights Off
/// Artery wins if both are true.
///
/// Microcontroller commands are bit flags queued until the next serial send cycle, and
/// are written in a fixed order (7 Blue, 8 Red, 9 Off) regardless of call order. To avoid
/// an "Off" and an "On" landing in the same cycle and cancelling out, only one light
/// command is queued per state change, and it is re-sent once after resendDelay_s so a
/// rapid flip-flop settles on the correct final state.
/// </summary>
[RequireComponent(typeof(NeedleInsertionMetrics))]
public class NeedleVesselLightFeedback : MonoBehaviour
{
    public enum LightState { Off, Blue, Red }

    [Tooltip("Re-send the current light command once this long after a change, to settle any commands that shared a send cycle.")]
    public float resendDelay_s = 0.25f;

    [Header("Read only")]
    [SerializeField] private LightState currentLight = LightState.Off;

    private NeedleInsertionMetrics metrics;
    private bool hasSentInitial;
    private float resendAt = -1f;

    void Start()
    {
        metrics = GetComponent<NeedleInsertionMetrics>();
    }

    void Update()
    {
        if (metrics == null || Microcontroller_Manager.ME == null) return;

        LightState wanted = metrics.InArtery ? LightState.Red
                          : metrics.InVein ? LightState.Blue
                          : LightState.Off;

        if (!hasSentInitial || wanted != currentLight)
        {
            currentLight = wanted;
            hasSentInitial = true;
            Send(currentLight);
            resendAt = Time.time + resendDelay_s;
        }
        else if (resendAt >= 0f && Time.time >= resendAt)
        {
            Send(currentLight);
            resendAt = -1f;
        }
    }

    private static void Send(LightState state)
    {
        switch (state)
        {
            case LightState.Red:
                Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.RedLight);
                break;
            case LightState.Blue:
                Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.BlueLight);
                break;
            default:
                Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.LightsOff);
                break;
        }
    }

    void OnDisable()
    {
        // Don't leave a light on if this component is turned off mid-insertion.
        if (currentLight != LightState.Off && Microcontroller_Manager.ME != null)
            Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.LightsOff);
        currentLight = LightState.Off;
        hasSentInitial = false;
        resendAt = -1f;
    }
}

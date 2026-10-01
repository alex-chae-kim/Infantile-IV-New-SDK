using UnityEngine;

namespace SMMARTS
{
    public class Microcontroller_Manager : MonoBehaviour
    {
        public static Microcontroller_Manager ME;
        private void Awake()
        {
            if (ME != null)
                Destroy(ME);
            ME = this;
        }
        int microcontrollerDataSetIndexOfTUI = 0;
        int microcontrollerDataSetIndexOfRidgeSideProbeFSR = 1;
        int microcontrollerDataSetIndexOfFlatSideProbeFSR = 2;
        int microcontrollerDataSetIndexOfSyringePressure = 3;
        [Header("------Microcontroller Values (Display Only)------")]
        [SerializeField]
        bool tuiPressed = false;
        public bool TUIPressed
        {
            get
            {
                if (Microcontroller.ME.Connected && Microcontroller.ME.MicrocontrollerData.Length > microcontrollerDataSetIndexOfTUI)
                    return int.Parse(Microcontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfTUI]) > 0;
                else
                    return false;
            }
        }
        [SerializeField]
        float probeFaceFSR_RidgeSide = 0;
        public float ProbeFaceFSR_RidgeSide
        {
            get
            {
                if (Microcontroller.ME.Connected && Microcontroller.ME.MicrocontrollerData.Length > microcontrollerDataSetIndexOfRidgeSideProbeFSR)
                    return float.Parse(Microcontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfRidgeSideProbeFSR]);
                else
                    return 0;
            }
        }
        [SerializeField]
        float probeFaceFSR_FlatSide = 0;
        public float ProbeFaceFSR_FlatSide
        {
            get
            {
                if (Microcontroller.ME.Connected && Microcontroller.ME.MicrocontrollerData.Length > microcontrollerDataSetIndexOfFlatSideProbeFSR)
                    return float.Parse(Microcontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfFlatSideProbeFSR]);
                else
                    return 0;
            }
        }
        [SerializeField]
        float syringePressure = 0;
        public float SyringePressure
        {
            get
            {
                if (Microcontroller.ME.Connected && Microcontroller.ME.MicrocontrollerData.Length > microcontrollerDataSetIndexOfSyringePressure)
                    return float.Parse(Microcontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfSyringePressure]);
                else
                    return 0;
            }
        }
        private void Update()
        {
            tuiPressed = TUIPressed;
            probeFaceFSR_RidgeSide = ProbeFaceFSR_RidgeSide;
            probeFaceFSR_FlatSide = ProbeFaceFSR_FlatSide;
            syringePressure = SyringePressure;
        }
        public enum COMMAND
        {
            ZeroSyringePressure,
            ResetSyringe,
            LightsOff,
            BlueLight,
            RedLight,
            LORC,
            LORO,
            LPop,
            SPop
        }
        public void SendCommand(COMMAND command)
        {
            if (command == COMMAND.ZeroSyringePressure)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_P);
            }
            else if (command == COMMAND.ResetSyringe)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_9);
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_3);
            }
            else if (command == COMMAND.LightsOff)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_9);
            }
            else if (command == COMMAND.BlueLight)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_7);
            }
            else if (command == COMMAND.RedLight)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_8);
            }
            else if (command == COMMAND.LORC)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_4);
            }
            else if (command == COMMAND.LORO)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_3);
            }
            else if (command == COMMAND.SPop)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_2);
            }
            else if (command == COMMAND.LPop)
            {
                Microcontroller.ME.SendCommand(Microcontroller.MicrocontrollerCommand.Microcontroller_Command_1);
            }
        }
    }
}

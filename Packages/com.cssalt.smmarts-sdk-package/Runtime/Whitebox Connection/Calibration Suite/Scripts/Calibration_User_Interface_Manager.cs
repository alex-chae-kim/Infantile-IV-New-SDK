using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 
/// Manages the UI of the CSSALT SMMARTS SDK calibration software.
/// 
/// 
/// Developer:
/// Andre Kazimierz Bigos
/// 01.02.2018
/// DD.MM.YYYY
/// 16:23 EST
/// Gainesville, FL
/// </summary>
namespace SMMARTS
{
    public class Calibration_User_Interface_Manager : MonoBehaviour
    {
        public static Calibration_User_Interface_Manager ME;
        [SerializeField]
        GameObject sensorTextureObject = null, calibrationTextureObject = null;
        //[SerializeField]
        Texture2D sensorFindingTexture = null, calibrationTexture = null;
        [SerializeField]
        GameObject calibrationText = null;
        [SerializeField]
        GameObject calibrationInstructionImage = null;
        [SerializeField]
        GameObject startCalibrationButton = null, calibrationTitle = null;
        private void Awake()
        {
            if (ME != null)
                Destroy(ME);
            ME = this;
        }
        public void Start()
        {
            ZeroCalibrationTextures();
        }
        public void ZeroCalibrationTextures()
        {
            sensorFindingTexture = new Texture2D(100, 30);
            sensorFindingTexture.filterMode = FilterMode.Point;
            calibrationTexture = new Texture2D(300, 30);
            calibrationTexture.filterMode = FilterMode.Point;
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    sensorFindingTexture.SetPixel(x, y, Color.white);
                }
                for (int x = 0; x < 300; x++)
                {
                    calibrationTexture.SetPixel(x, y, Color.white);
                }
            }
            sensorFindingTexture.Apply();
            calibrationTexture.Apply();
            sensorTextureObject.GetComponent<RawImage>().texture = sensorFindingTexture;
            calibrationTextureObject.GetComponent<RawImage>().texture = calibrationTexture;
        }
        public void StartCalibration()
        {
            Calibration.ME.StartCalibration();
            startCalibrationButton.SetActive(false);
            calibrationTitle.SetActive(false);
        }
        public void CalibrationUpdate(float sensorFindingFraction, float calibrationFraction)
        {
            if (sensorFindingFraction == 0)
                ZeroCalibrationTextures();
            if (sensorFindingFraction > 1)
                sensorFindingFraction = 1;
            if (calibrationFraction > 1)
                calibrationFraction = 1;
            if (sensorFindingFraction < 0)
            {
                UpdateCalibrationText("(Waiting to Calibrate)");
                ZeroCalibrationTextures();
            }
            else if (sensorFindingFraction == 0)
            {
                UpdateCalibrationText("Move finger to calibration zone.\nHold in place to calibrate.");
                calibrationInstructionImage.SetActive(true);
            }
            else if (sensorFindingFraction < 1)
                UpdateCalibrationText("Locating sensor");
            else if (calibrationFraction < 1)
                UpdateCalibrationText("Calibrating sensor: " + ((int)(calibrationFraction * 100)) + "%");
            else
            {
                UpdateCalibrationText("Calibration Complete");
                EndCalibration();
            }

            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 100 * sensorFindingFraction; x++)
                {
                    sensorFindingTexture.SetPixel(x, y, Color.blue);
                }
                for (int x = 0; x < 300 * calibrationFraction; x++)
                {
                    calibrationTexture.SetPixel(x, y, Color.green);
                }
            }
            sensorFindingTexture.Apply();
            calibrationTexture.Apply();
            sensorTextureObject.GetComponent<RawImage>().texture = sensorFindingTexture;
            calibrationTextureObject.GetComponent<RawImage>().texture = calibrationTexture;
        }
        public void EndCalibration()
        {
            Calibration.ME.EndCalibration();
            ResetUI();
        }
        public void UpdateCalibrationText(string newString)
        {
            calibrationText.GetComponent<Text>().text = newString;
        }
        void ResetUI()
        {
            calibrationTitle.SetActive(true);
            startCalibrationButton.SetActive(true);
            calibrationInstructionImage.SetActive(false);
            ZeroCalibrationTextures();
        }
    }
}
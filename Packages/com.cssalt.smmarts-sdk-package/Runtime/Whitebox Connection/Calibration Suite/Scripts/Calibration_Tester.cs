using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SMMARTS
{
	public class Calibration_Tester : MonoBehaviour
	{
		[SerializeField]
		Calibration calibration = null;
		[SerializeField]
		bool calibrate = false;
		[SerializeField]
		bool endCalibration = false;
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (calibrate)
			{
				calibrate = false;
				calibration.StartCalibration();
			}
			if (endCalibration)
			{
				calibration.EndCalibration();
				endCalibration = false;
			}
		}
	}
}

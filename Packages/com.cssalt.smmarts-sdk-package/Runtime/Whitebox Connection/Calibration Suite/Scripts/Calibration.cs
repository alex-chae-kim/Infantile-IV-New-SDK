using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Developer
/// Andre Kazimierz Bigos
/// 01.02.2018
/// DD.MM.YYYY
/// 16:23 EST
/// Gainesville, FL
/// </summary>
namespace SMMARTS
{
	public class Calibration : MonoBehaviour
	{
		public static Calibration ME;
		private void Awake()
		{
			if (ME != null)
				Destroy(ME);
			ME = this;
		}
		bool findSensorInField = false;
		bool calibrateSensor = false;
		bool startSensorInFieldTime = false;
		float sensorInFieldStartTime = 0;
		float sensorInFieldTime = 1;/// (seconds) Time between when sensor enters calibration range, to when calibration begins.
									/// Ensures more accurate data as user has time to place sensor in calibration region
									/// and hold sensor in place before averaging position and rotation data.
		[SerializeField]
		float maxAcceptableRange = 30f;
		[SerializeField]
		GameObject calibrationObject = null; /// Object being calibrated. Will be calibrated (fixed) to sensor.
		[SerializeField]
		GameObject calibrationIdealLocation = null; /// Hypothetical Ideal Calibration Location. This is where calibration object should appear in space.
													/// The object being calibrated is aligned to this position, and the sensor is moved to this area.
													/// The sensor is measured relative to this location. The object is then mated to the sensor at the
													/// offset distance.
		[SerializeField]
		GameObject sensor = null; /// The sensor being tracked, to which the calibrationObject is fixed.

		[SerializeField]
		float locatingFraction = 0;
		bool startCalibrationTime = false;
		float calibrationStartTime = 0;
		float calibrationTime = 8; /// (seconds) Total time to calibration. In general, averaging over more time creates more accurated data.
		[SerializeField]
		float calibratingFraction = 0;
		[SerializeField]
		bool calibrated = false;
		public bool Calibrated
		{ get { return calibrated; } }
		[SerializeField]
		List<Vector3> positions = new List<Vector3>();
		//List<Vector3> rotations = new List<Vector3>();
		List<Quaternion> quaternions = new List<Quaternion>();
		void Start()
		{

		}
		void Update()
		{
			if (findSensorInField)
			{
				if (Vector3.Distance(calibrationIdealLocation.transform.position, sensor.transform.position) < maxAcceptableRange)
				{
					if (startSensorInFieldTime)
					{
						sensorInFieldStartTime = Time.time;
						startSensorInFieldTime = false;
					}
					else
					{
						float timeSinceSensorInFieldStart = Time.time - sensorInFieldStartTime;
						locatingFraction = timeSinceSensorInFieldStart / sensorInFieldTime;
						Calibration_User_Interface_Manager.ME.CalibrationUpdate(locatingFraction, calibratingFraction);
						if (timeSinceSensorInFieldStart > sensorInFieldTime)
						{
							calibrateSensor = true;
							startCalibrationTime = true;
							findSensorInField = false;
						}
					}
				}
				else
				{
					StartCalibration();
				}
			}
			if (calibrateSensor)
			{
				if (Vector3.Distance(calibrationIdealLocation.transform.position, sensor.transform.position) < maxAcceptableRange)
				{
					positions.Add(sensor.transform.position);
					//rotations.Add(transform.rotation.eulerAngles);
					quaternions.Add(sensor.transform.rotation);
					if (startCalibrationTime)
					{
						startCalibrationTime = false;
						calibrationStartTime = Time.time;
					}
					else
					{
						float timeSinceCalibrationStart = Time.time - calibrationStartTime;
						if (timeSinceCalibrationStart > calibrationTime)
						{
							PerformCalibration();
							calibrated = true;
							startCalibrationTime = true;
							calibrateSensor = false;
						}
						calibratingFraction = timeSinceCalibrationStart / calibrationTime;
						Calibration_User_Interface_Manager.ME.CalibrationUpdate(locatingFraction, calibratingFraction);
					}
				}
				else
				{
					StartCalibration();
				}
			}
		}
		public void StartCalibration()
		{
			LockFingerToCalibrationLocation();
			ZeroAllValues();
			findSensorInField = true;
			startSensorInFieldTime = true;
			Calibration_User_Interface_Manager.ME.CalibrationUpdate(locatingFraction, calibratingFraction);
		}
		public void EndCalibration()
		{
			ZeroAllValues();
			Calibration_User_Interface_Manager.ME.CalibrationUpdate(-1, -1);
		}
		void ZeroAllValues()
		{
			positions = new List<Vector3>();
			//rotations = new List<Vector3>();
			quaternions = new List<Quaternion>();
			findSensorInField = false;
			calibrateSensor = false;
			startSensorInFieldTime = false;
			sensorInFieldStartTime = 0;
			locatingFraction = 0;
			startCalibrationTime = false;
			calibrationStartTime = 0;
			calibratingFraction = 0;
			calibrated = false;
		}
		void PerformCalibration()
		{
			Vector3 averagePosition = Vector3.zero;
			Vector3 averageRotation = Vector3.zero;
			Quaternion averageQuaternion = new Quaternion(0, 0, 0, 0);
			float averageX = 0;
			float averageY = 0;
			float averageZ = 0;
			float averageW = 0;
			foreach (Vector3 position in positions)
			{
				averagePosition += position;
			}
			averagePosition /= positions.Count;
			//foreach(Vector3 rotation in rotations)
			//{
			//	averageRotation += rotation;
			//}
			foreach (Quaternion quaternion in quaternions)
			{
				averageX += quaternion.x;
				averageY += quaternion.y;
				averageZ += quaternion.z;
				averageW += quaternion.w;
			}
			averageX /= quaternions.Count;
			averageY /= quaternions.Count;
			averageZ /= quaternions.Count;
			averageW /= quaternions.Count;
			averageQuaternion = new Quaternion(averageX, averageY, averageZ, averageW);
			//averageRotation /= rotations.Count;
			Debug.Log(sensor.transform.position.ToString() + " " + averagePosition.ToString() + " " + sensor.transform.rotation.eulerAngles.ToString() + " " + averageRotation.ToString() + " " + sensor.transform.rotation.ToString() + " " + averageQuaternion.ToString());
			sensor.transform.position = averagePosition;
			sensor.transform.rotation = averageQuaternion;
			calibrationObject.transform.parent = sensor.transform;
		}
		void LockFingerToCalibrationLocation()
		{
			calibrationObject.transform.position = calibrationIdealLocation.transform.position;
			calibrationObject.transform.rotation = calibrationIdealLocation.transform.rotation;
			calibrationObject.transform.parent = calibrationIdealLocation.transform;
		}
	}
}

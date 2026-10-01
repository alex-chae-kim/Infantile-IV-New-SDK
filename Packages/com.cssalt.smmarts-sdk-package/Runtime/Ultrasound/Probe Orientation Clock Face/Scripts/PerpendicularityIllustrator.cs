using UnityEngine;
using System.Collections;

// detector for: If the needle is pointing at a layer, what is the degrees offset to normal?
// and uses the needle hub and tip analogy.

namespace SMMARTS
{
	public class PerpendicularityIllustrator : MonoBehaviour
	{
		[Header("----------- Object References -----------------------------------------------------------")]
		public GameObject Hub;
		public GameObject Tip;
		public GameObject IndicatorStick, IndicatorCenterDot;
		public GameObject Skin;
		public GameObject Rotation;
		//public GameObject TiltAxisDot, RockAxisDot;

		//[Header("----------- PARRT Rotation (degrees) ----------------------------------------------------")]
		//public float RotationAngle;
		//public float RotationHours;
		//public int RotationOClock;

		//[Header("----------- PARRT Tilt (degrees) --------------------------------------------------------")]
		//public float TiltAngle;  

		//[Header("----------- PARRT Rock (degrees) --------------------------------------------------------")]
		//public float RockAngle;   

		[Header("----------- Off-Perpendicular (degrees) -------------------------------------------------")]
		public float DegreesOffPerpendicular;

		private LayerMask SkinMask;
		private GameObject Indicator;
		private Vector3[] SkinMeshNormals;
		private int[] SkinMeshTriangles;
		private Vector3 IndicatorStickVector, TiltAxis, RockAxis;

		void Start()
		{
			MeshCollider SkinCollider = Skin.GetComponent<MeshCollider>();
			Mesh SkinMesh = SkinCollider.sharedMesh;
			SkinMeshNormals = SkinMesh.normals;
			SkinMeshTriangles = SkinMesh.triangles;
			SkinMask = 1 << LayerMask.NameToLayer("Skin");

			// IndicatorCenterDot = GameObject

		}

		public void Update()
		{

			RaycastHit hit;

			// forward direction
			Vector3 forward = Tip.transform.position - Hub.transform.position;
			Ray lookingForward = new Ray(Hub.transform.position, forward);
			//float tipDistance = Vector3.Distance (Hub.transform.position, Tip.transform.position);


			float d = forward.magnitude * 1.4F;

			if (Physics.Raycast(lookingForward, out hit, d, SkinMask))
			{

				// Extract local space normals of the triangle we hit.   
				Vector3 n0 = SkinMeshNormals[SkinMeshTriangles[hit.triangleIndex * 3 + 0]];
				Vector3 n1 = SkinMeshNormals[SkinMeshTriangles[hit.triangleIndex * 3 + 1]];
				Vector3 n2 = SkinMeshNormals[SkinMeshTriangles[hit.triangleIndex * 3 + 2]];

				// Extract the barycentric coordinate of the hitpoint 
				Vector3 baryCenter = hit.barycentricCoordinate;

				// Use barycentric coordinate to interpolate between the three normals
				Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;

				// normalize the interpolated normal 
				interpolatedNormal = interpolatedNormal.normalized;

				// Transform local space normals to world space 
				Transform hitTransform = hit.collider.transform;
				interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);

				// We can now move the indicator
				//Indicator.transform.position = hit.point;
				//Indicator.transform.rotation = Quaternion.FromToRotation (Vector3.up, interpolatedNormal);

				// First we move the entire indicator - including the camera, background, everything - to align with the skin perpendicular.
				transform.rotation = Quaternion.FromToRotation(Vector3.back, interpolatedNormal);

				// Second, we move the Indicator Stick to align with the ultrasound probe.
				IndicatorStick.transform.rotation = Tip.transform.rotation;

				// Define specific probe rotations.

				// PARRT Rotation.  On the clockface, 270 = 12 o'clock. 0 = 3 o'clock. 90 = 6 o'clock. 180 = 9 o'clock. 
				float rotationAngle = IndicatorStick.transform.localEulerAngles.z;

				//RotationHours = Mathf.InverseLerp(0, 360F, RotationAngle);
				//RotationHours = Mathf.Lerp(3F, 15F, RotationHours);

				//bool inPlane = (Mathf.Abs(RotationHours - Mathf.RoundToInt(RotationHours)) < 0.2F);

				//if (RotationHours > 12) RotationHours -= 12;

				//RotationOClock = Mathf.RoundToInt(RotationHours);
				//if (RotationOClock == 0) RotationOClock = 12;

				//if (!inPlane) RotationOClock = 0;

				if (Rotation != null)
					Rotation.transform.localEulerAngles = new Vector3(Rotation.transform.localEulerAngles.x, rotationAngle, Rotation.transform.localEulerAngles.z);

				// note the direction of the indicator stick, which is the probe in relation to the skin normal.  
				IndicatorStickVector = IndicatorStick.transform.position - IndicatorCenterDot.transform.position;

				// PARRT Rock angle.
				//TiltAxis = IndicatorStick.transform.position - TiltAxisDot.transform.position;
				//RockAngle = Vector3.Angle(IndicatorStickVector, Vector3.ProjectOnPlane(IndicatorStickVector, TiltAxis));

				// PARRT Tilt angle.  
				//RockAxis = IndicatorStick.transform.position - RockAxisDot.transform.position;
				//TiltAngle = Vector3.Angle(IndicatorStickVector, Vector3.ProjectOnPlane(IndicatorStickVector, RockAxis));

				// Entire difference from perpendicular
				DegreesOffPerpendicular = 180F - Vector3.Angle(forward.normalized, interpolatedNormal);


			}

		}



	}
}
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

using Object = UnityEngine.Object;

namespace SMMARTS
{
    [System.Serializable]
    public class Project_Populator : EditorWindow
    {
        //Use this if we want to update the status of the window UI dynamically.
        //bool sceneDirty = false;

        static bool isIndexPlateActive, newindexPlateStatus;
        static bool isTuiProbeActive, newtuiProbeStatus;
        static bool isNeedleActive;
        static bool isUltrasoundActive, newUltrasoundStatus;

        static bool isBasicSceneSetUp;

        static GameObject communicator;

        static UnityEngine.Object indexPlate;
        static UnityEngine.Object newIndexPlate;
        static UnityEngine.Object tuiProbe;
        static UnityEngine.Object newTuiProbe;
        static UnityEngine.Object ultrasound;
        static UnityEngine.Object newUltrasound;
        static UnityEngine.Object needle;
        static UnityEngine.Object newNeedle;

        static GameObject atcTrackedObj;

        string[] needleOptions = new string[] { "None", "Basic Needle", "CVA Needle", "RA Needle" };//, "Custom" };


        List<int> instanceIDs = new List<int>();
        static int needleIndex = 0;
        static int newNeedleIndex = 0;

        string warningMessage = "";
        // Add menu named "Preferences" to the CSSALT menu
        [MenuItem("SMMARTS/Setup New Project %P")]


        static void Init()
        {
            // Get existing open window or if none, make a new one:
            Project_Populator window = (Project_Populator)GetWindow(typeof(Project_Populator));
            ReadPrefsFromScene();
            window.Show();
        }

        private void OnHierarchyChange()
        {
            //Debug.Log("Hello from Project change.");
            ReadPrefsFromScene();
            Repaint();
        }

        private void Awake()
        {
            //Debug.Log("Hello from Awake.");
            ReadPrefsFromScene();
            Repaint();

            //Check updates last. TBS (To be safe :P)
            UpdateChecker.CheckUpdates();
        }

        void OnGUI()
        {
            if (Application.isPlaying)
                return;

            // GUILayout.Label("Choose Your Tracked Objects", EditorStyles.boldLabel);

            GUILayout.BeginVertical();

            GUI.enabled = !isBasicSceneSetUp;
            if (GUILayout.Button("\n" + "Set Up White Box Communicator" + "\n"))
            {
                if (GameObject.Find("WhiteBoxCommunicator") == null)
                {
                    communicator = Instantiate(Resources.Load<GameObject>("WhiteBoxCommunicator")) as GameObject;
                    communicator.name = communicator.name.Replace("(Clone)", "");
                    isBasicSceneSetUp = true;
                }
            }

            if (isBasicSceneSetUp)
            {
                GUI.enabled = true;
                GUILayout.Label("------Select SMMARTS Components------", EditorStyles.centeredGreyMiniLabel);
                newindexPlateStatus = EditorGUILayout.Toggle("Standard Index Plate", newindexPlateStatus);
                isIndexPlateActive = GameObject.Find("IndexPlate");
                if (isIndexPlateActive && !newindexPlateStatus)
                {
                    //Debug.Log("Destroy Index Plate");
                    //DestroyAllOfType(typeof(IBasePlate));
                    DestroyImmediate(GameObject.Find("IndexPlate"));
                }
                else if (!isIndexPlateActive && newindexPlateStatus)
                {
                    //Debug.Log("Make Index Plate");
                    indexPlate = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("IndexPlate"));
                    instanceIDs.Add(indexPlate.GetInstanceID());
                    communicator.GetComponent<ATC>().BaseObject = (GameObject)indexPlate;
                }
                else 
                { 
                    // Debug.Log("weird logic issue"); 
                }

                // Debug.Log("button: " + newindexPlateStatus + ", indexplate: " + isIndexPlateActive);

                newUltrasoundStatus = EditorGUILayout.Toggle("Ultrasound Probe", isUltrasoundActive);
                if (newUltrasoundStatus != isUltrasoundActive)
                {
                    if (newUltrasoundStatus)
                    {
                        ultrasound = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("Ultrasound 1"));
                        instanceIDs.Add(ultrasound.GetInstanceID());
                        communicator.GetComponent<ATC>().TrackedObject2 = ((GameObject)ultrasound).transform.GetChild(0).gameObject;
                    }
                    else
                    {
                        //Remove all instances of IUltrasound
                        //DestroyAllOfType(typeof(IUltrasound));
                        DestroyImmediate(GameObject.Find("Ultrasound 1"));
                    }
                    isUltrasoundActive = newUltrasoundStatus;
                    EditorSceneManager.MarkAllScenesDirty();
                }

                newtuiProbeStatus = EditorGUILayout.Toggle("TUI Probe", isTuiProbeActive);
                if (newtuiProbeStatus != isTuiProbeActive)
                {
                    if (newtuiProbeStatus)
                    {
                        tuiProbe = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("TUI Probe"));
                        instanceIDs.Add(tuiProbe.GetInstanceID());
                        communicator.GetComponent<ATC>().TrackedObject1 = (GameObject)tuiProbe;
                    }
                    else
                    {
                        //DestroyAllOfType(typeof(ITUIProbe));
                        DestroyImmediate(GameObject.Find("TUI Probe"));
                    }
                    isTuiProbeActive = newtuiProbeStatus;
                    EditorSceneManager.MarkAllScenesDirty();

                }


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Needle");
                newNeedleIndex = EditorGUILayout.Popup(needleIndex, needleOptions, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                if (newNeedleIndex != needleIndex && newNeedleIndex != 4)
                {
                    HandleNeedleSelection(newNeedleIndex);
                    needleIndex = newNeedleIndex;
                }
                if (newNeedleIndex == 4)
                {
                    HandleCustomSelection(typeof(INeedle), ref needle, ref newNeedle, "Needle", 3);
                    needleIndex = newNeedleIndex;
                }
            }


            GUILayout.EndVertical();
        }



        private void DestroyAllOfType(Type type)
        {
            //Yes yes its a nested for loop, but they're always gonna be small.
            // #NoFret

            foreach (Type t in InterfaceHelper.GetTypesInheritedFromInterface(type))
            {
                //Debug.Log("Removing all of type: " + t.FullName);
                foreach (UnityEngine.Object o in FindObjectsOfType(t))
                {
                    MonoBehaviour current = (MonoBehaviour)o;
                    if (instanceIDs.Contains(current.gameObject.GetInstanceID()))
                    {
                        instanceIDs.Remove(current.gameObject.GetInstanceID());
                        DestroyImmediate(current.gameObject);

                    }

                }
            }
        }

        private static bool CheckIfAnyExists(Type type)
        {
            foreach (Type t in InterfaceHelper.GetTypesInheritedFromInterface(type))
            {
                //Debug.Log("Scanning all of type: " + t.FullName);
                if (FindObjectOfType(t) != null)
                {
                    return true;
                }
            }
            return false;
        }


        private static void ReadPrefsFromScene()
        {
            if (communicator == null)
            {
                if (GameObject.Find("WhiteBoxCommunicator") != null)
                {
                    communicator = GameObject.Find("WhiteBoxCommunicator");
                    isBasicSceneSetUp = true;
                }
                else
                    isBasicSceneSetUp = false;

            }

            isIndexPlateActive = CheckIfAnyExists(typeof(IBasePlate));
            Debug.LogWarning("Set Index Plate to " + isIndexPlateActive);

            isTuiProbeActive = CheckIfAnyExists(typeof(ITUIProbe));
            Debug.LogWarning("Set TUI Probe to " + isNeedleActive);

            isUltrasoundActive = CheckIfAnyExists(typeof(IUltrasound));
            Debug.LogWarning("Set ultrasound to " + isUltrasoundActive);

            isNeedleActive = CheckIfAnyExists(typeof(INeedle));
            if (!isNeedleActive)
                needleIndex = 0;

            Debug.LogWarning("Set needle to " + isNeedleActive);

        }

        private void HandleCustomSelection(Type type, ref Object obj, ref Object newObj, String objTypeName, int atcTrackobjNum)
        {
            newObj = EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
            if (newObj != null)
            {
                if (CheckIfAnyExists(type))
                {
                    if (newObj != obj)
                    {
                        obj = newObj;
                        if (((GameObject)newObj).GetComponent(type) != null)
                        {
                            warningMessage = "Scene contains " + objTypeName + "!";
                            DestroyAllOfType(type);
                            try
                            {
                                newObj = PrefabUtility.InstantiatePrefab(newObj);

                            }
                            catch (UnityException e)
                            {
                                Debug.Log(e);
                            }
                            if (newObj == null)
                            {
                                atcTrackedObj = (GameObject)obj;
                            }
                            else
                            {
                                instanceIDs.Add(newObj.GetInstanceID());
                                atcTrackedObj = (GameObject)newObj;
                            }

                        }
                        else if (((GameObject)newObj).GetComponent(type) == null)
                        {
                            warningMessage = "Selected Object is not of type " + type.ToString() + "!";
                        }
                    }
                }
                else
                {
                    if (newObj != obj)
                    {
                        obj = newObj;
                        if (((GameObject)newObj).GetComponent(type) == null)
                        {
                            warningMessage = "Selected Object is not of type " + type.ToString() + "!";
                        }
                        else if (((GameObject)newObj).GetComponent(type) != null)
                        {
                            newObj = PrefabUtility.InstantiatePrefab(newObj);
                            instanceIDs.Add(newObj.GetInstanceID());
                            atcTrackedObj = (GameObject)newObj;
                        }
                    }


                }
                assignTrackObj(atcTrackobjNum);
            }
            if (!String.IsNullOrEmpty(warningMessage))
                EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
        }

        private void assignTrackObj(int num)
        {
            switch (num)
            {
                case 1:
                    communicator.GetComponent<ATC>().TrackedObject1 = atcTrackedObj;
                    break;
                case 2:
                    communicator.GetComponent<ATC>().TrackedObject2 = atcTrackedObj;
                    break;
                case 3:
                    communicator.GetComponent<ATC>().TrackedObject3 = atcTrackedObj;
                    break;
                case 4:
                    communicator.GetComponent<ATC>().TrackedObject4 = atcTrackedObj;
                    break;
                default:
                    break;
            }

        }
        private void HandleNeedleSelection(int index)
        {
            EditorSceneManager.MarkAllScenesDirty();
            switch (index)
            {
                case 0:
                    {
                        DestroyAllOfType(typeof(INeedle));
                        break;
                    }
                case 1:
                    {
                        DestroyAllOfType(typeof(INeedle));
                        needle = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("Basic Needle"));
                        instanceIDs.Add(needle.GetInstanceID());
                        communicator.GetComponent<ATC>().TrackedObject3 = (GameObject)needle;
                        break;
                    }
                case 2:
                    {
                        DestroyAllOfType(typeof(INeedle));
                        needle = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("CVA Needle and Syringe"));
                        instanceIDs.Add(needle.GetInstanceID());
                        communicator.GetComponent<ATC>().TrackedObject3 = (GameObject)needle;
                        break;
                    }
                case 3:
                    {
                        DestroyAllOfType(typeof(INeedle));
                        needle = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("RA Tuohy Needle"));
                        instanceIDs.Add(needle.GetInstanceID());
                        communicator.GetComponent<ATC>().TrackedObject3 = (GameObject)needle;
                        break;
                    }
            }


        }


    }
}
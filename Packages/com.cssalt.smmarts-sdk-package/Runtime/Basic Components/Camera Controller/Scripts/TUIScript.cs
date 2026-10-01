using UnityEngine;
using System.Collections;

// SINGLETON
// 

namespace SMMARTS
{
    public class TUIScript : MonoBehaviour, ITUIProbe
    {
        // SINGLETON
        public static TUIScript ME;

        [Header("----------- Initializations --------------------------------------------------------------")]
        // public bool ShowCrosshairs;
        public Transform DefaultParent;

        [Header("----------- External Follow Object  ------------------------------------------------------")]
        public bool EnableFollowParent;
        public GameObject FollowParent;
        public float StickyDistance;

        // TODO: add crosshairs. only sim to use them to date is original thoracic RA from 2016
        // we used to have a little X appear in the middle of the screen whenevery you held the TUI button down
        // that way an instructor could use it to point things out. 
        // [Header("-- Put your own crosshairs here ------------------------------------------------------")]
        // public GameObject Crosshairs;

        // this is a flag that the system uses to test if the camera is intended to follow another object.
        // for example, it can follow the ultrasound probe if the shutter button is clicked if the camera is near a probe.
        // TODO: make a user interface that makes this explicit.
        // TODO: make a property inspector interface that allows developers to add or remove tools that can be followed.
        private bool FollowParentLatch;

        //to disable camera switching in skill scripts
        public bool actingAsStylusPointer_notCamera { get; set; }

        // Use this for initialization
        void Start()
        {
            actingAsStylusPointer_notCamera = false;
            if (ME != null) GameObject.Destroy(ME);
            else ME = this;
            DontDestroyOnLoad(this);

            ResetFree();

        }

        //public bool ActingAsStylusPointer_notCamera { get; set; } // Commented out by Andre. This boolean was not being used anywhere and was causing great
        // confusion because it's the only thing an outside script can change but it does nothing.
        // I added a get and a set for the other lowercase actingAsStylusPointer_notCamera boolean.
        public void ResetFree()
        {
            Camera.main.transform.SetParent(DefaultParent);
            Camera.main.GetComponent<Camera>().orthographic = false;
            FollowParentLatch = false;
        }

        // Update is called once per frame
        void Update()
        {

            if (actingAsStylusPointer_notCamera == false) // acting as a normal Camera, not a Stylus Pointer
            {

                // bail out early if the microcontroller isn't active - usually in a scene during development
                if (Microcontroller_Manager.ME == null) { return; }

                bool cameraShutter = Microcontroller_Manager.ME.TUIPressed;

                if (cameraShutter)
                {

                    Camera.main.orthographic = false;
                    Camera.main.transform.position = transform.position;
                    Camera.main.transform.rotation = transform.rotation;


                    if (EnableFollowParent & !FollowParentLatch)
                    {
                        float d = Vector3.Distance(transform.position, FollowParent.transform.position);
                        if (d < StickyDistance) FollowParentLatch = true;
                    }

                    if (!FollowParentLatch)
                    {
                        ResetFree();
                    }
                    else // standard camera 
                    {
                        Camera.main.transform.SetParent(FollowParent.transform);
                    }
                }
                else // no camera shutter.
                {
                    FollowParentLatch = false;
                }

                /*
                // Crosshairs
                if (ShowCrosshairs)
                {
                    if (cameraShutter)
                    {
                        if (!Crosshairs.activeSelf)
                            Crosshairs.SetActive(true);

                    }
                    else
                    {
                        if (Crosshairs.activeSelf)
                            Crosshairs.SetActive(false);
                    }
                }
                */

            }



        }

    }
}

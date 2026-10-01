using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SMMARTS
{
    public class UltrasoundDisplayController : MonoBehaviour
    {

        /// This static UltrasoundManager is used to ensure a single instance of the ultrasound manager exists in the scene and 
        /// that the single instance is easily accessible to outside scripts. In awake we destroy all other possible instances.
        /// When we need to create a second ultrasound image rendering simultaneously, like you may find in a biplane probe, then duplicate this entire script and name it UltrasoundManager_2. 
        public static UltrasoundDisplayController ME;

        [Header("-----Appearance at Startup-----", order = 1)]

        /// If true, begins using small ultrasound UI, if false, begins using large ultrasound UI dimensions.
        [SerializeField]
        bool beginInSmallScreen = true;

        /// If true, begins using blur, attenuation, and anisotropy settings.
        [SerializeField]
        bool beginWithRealism = true;

        /// If true, begins using reversed image (cardiac mode)
        [SerializeField]
        bool beginWithReverseImage = false;

        /// Enum for the screen dimensions. Allows further expansion and default dimensions to be added at a different time.
        /// Used to quickly and simply toggle screen between two editable sizes.
        public enum SCREEN_DIMENSIONS
        {
            LARGE = 1,
            SMALL = 2
        }

        [Header("-----User Interface Managment-----", order = 1)]

        /// Hide ultrasoundScreenSize inspector editing functionality in custom inspector. 
        [SerializeField]
        SCREEN_DIMENSIONS ultrasoundScreenSize = SCREEN_DIMENSIONS.SMALL;
        public SCREEN_DIMENSIONS UltrasoundScreenSize
        {
            get
            {
                return ultrasoundScreenSize;
            }
            set
            {
                if (value != ultrasoundScreenSize)
                {
                    ultrasoundScreenSize = value;
                    UpdateDisplayPositionOnScreen();
                }
            }
        }


        /// The GameObject which holds the canvas upon which the ultrasound is rendered.
        RectTransform Ultrasound_Display_Screen;

        /// The GameObject whose canvas holds the large dimensions used for the increasing size of the ultrasound.
        RectTransform Ultrasound_Screen_Large_Dimensions;

        /// The GameObject whose canvas holds the small dimensions use for the decreasing size of the ultrasound.
        RectTransform Ultrasound_Screen_Small_Dimensions;

        RectTransform small_or_large_rect;


        /// <summary>
        /// In Awake() we create the static singleton reference UltrasoundUIManager.ME. It also destroys every unnecessary instance of 
        /// the ultrasound ensuring the single instance.
        /// </summary>
        private void Awake()
        {
            if (ME != null)
                Destroy(ME);
            ME = this;
        }


        // Start is called before the first frame update
        void Start()
        {
            // These are common SDK US parts so we can find them once at start().
            // They are hard-coded because we are reserving this script's inspector for simualtor-specific variables.
            Ultrasound_Display_Screen = GameObject.Find("Ultrasound Display Screen").GetComponent<RectTransform>();
            Ultrasound_Screen_Large_Dimensions = GameObject.Find("Ultrasound Screen Large Dimensions").GetComponent<RectTransform>();
            Ultrasound_Screen_Small_Dimensions = GameObject.Find("Ultrasound Screen Small Dimensions").GetComponent<RectTransform>();

            UpdateDisplayPositionOnScreen();

            if (beginWithRealism) RealismOn(); else RealismOff();
            if (beginWithReverseImage) ReverseImage_On(); else ReverseImage_Off();

        }

        // Update is called once per frame
        void Update()
        {
            UpdateDisplayPositionOnScreen();
        }


        /// <summary>
        /// Sets ultrasound display to one of the two preset sizes. 
        /// Currently there are two default sizes, Large and Small, but we expect those to move around as necessary to meet the needs of a specific simulator.
        /// The transform rectangle adjusts anchor positions, width, and height - not scale or rotation.
        /// It tests first to see if they need to be moved to avoid updating canvas items needlessly.
        /// Intended to be called from Update() because we expect the small and large dimensions to change dynamically.
        /// </summary>
        /// 
        void UpdateDisplayPositionOnScreen()
        {

            if (ultrasoundScreenSize == SCREEN_DIMENSIONS.SMALL)
                small_or_large_rect = Ultrasound_Screen_Small_Dimensions; 
            else
                small_or_large_rect = Ultrasound_Screen_Large_Dimensions;

            // Changing a RectTransform's properties (like position, size, anchored position, etc.)
            // triggers Unity's layout system to recalculate the positions and sizes of all affected UI elements. 
            // So, we'll test first and update only if necessary.
            if (Ultrasound_Display_Screen.anchoredPosition != small_or_large_rect.anchoredPosition)
            {
                Ultrasound_Display_Screen.anchoredPosition = small_or_large_rect.anchoredPosition;
                Ultrasound_Display_Screen.anchoredPosition3D = small_or_large_rect.anchoredPosition3D;
                Ultrasound_Display_Screen.anchorMax = small_or_large_rect.anchorMax;
                Ultrasound_Display_Screen.anchorMin = small_or_large_rect.anchorMin;
                Ultrasound_Display_Screen.sizeDelta = small_or_large_rect.sizeDelta;
                Ultrasound_Display_Screen.offsetMin = small_or_large_rect.offsetMin;
                Ultrasound_Display_Screen.offsetMax = small_or_large_rect.offsetMax;
            }

        }



        // Fullscreen is a toggle that copies the properties of two prototype objects (a small and a large) to the ultrasound display image.
        public void FullscreenToggle()
        {
            if (ultrasoundScreenSize == SCREEN_DIMENSIONS.SMALL)
            {
                FullscreenOn();
            }
            else
            {
                FullscreenOff();
            }
        }

        public void FullscreenOn()
        {
            ultrasoundScreenSize = SCREEN_DIMENSIONS.LARGE;
            UpdateDisplayPositionOnScreen();
        }

        public void FullscreenOff()
        {
            ultrasoundScreenSize = SCREEN_DIMENSIONS.SMALL;
            UpdateDisplayPositionOnScreen();
        }


        // REALISM in SMMARTS simulators is a bundle of Blur, Depth Attenuation, and Anisotropy. 
        // Blur, Depth Attenuation, and Anisotropy can be adjusted separately but they have been bundled together in practice since creation.
        // the default is to have Realism off (no Blur, Depth Attenuation, or Anisotropy).
        public void RealismToggle()
        {
            UltrasoundManager.ME.Blur = !UltrasoundManager.ME.Blur;
            UltrasoundManager.ME.DepthAttenuation = !UltrasoundManager.ME.DepthAttenuation;
            UltrasoundManager.ME.Anisotropy = !UltrasoundManager.ME.Anisotropy;
        }

        public void RealismOn()
        {
            UltrasoundManager.ME.Blur = true;
            UltrasoundManager.ME.DepthAttenuation = true;
            UltrasoundManager.ME.Anisotropy = true;
        }

        public void RealismOff()
        {
            UltrasoundManager.ME.Blur = false;
            UltrasoundManager.ME.DepthAttenuation = false;
            UltrasoundManager.ME.Anisotropy = false;
        }


        // REVERSE IMAGE flips the US image view and moves the dot - without flipping the scale.
        // ultrasound machines label this function Reverse Image, Reverse, R, Rt/Lt depending on the manufacturer.
        // The orientation dot on the US image is on the left by default; cardioloy puts the dot on the right.
        // this is done by changing the sign of the X value of the tiling of the texture
        // it also enables a different sidedness dot. We're using a different sidedness dot becuase the flipped dot may interfere with the scale, 
        // so its safer to just have two for you to move around as you see fit, based on the shape of your insonating plane and the scale.
        public MeshRenderer RenderTexture;
        public GameObject SidednessDot_Normal, SidednessDot_Flipped;
        public bool ImagedReversed = false;
        public void ReverseImage()
        {
            if (ImagedReversed == false)
            {
                ReverseImage_On();  // orientation dot on right side (used by cardiology)
            }
            else
            {
                ReverseImage_Off(); // orientation dot on left side (default)
            }
        }

        public void ReverseImage_On()
        {
            ImagedReversed = true;
            RenderTexture.material.mainTextureScale = new Vector2(-1, 1); // in the inspector, this is listed under Main Maps and labeled "Tiling" 
            SidednessDot_Normal.SetActive(false);
            SidednessDot_Flipped.SetActive(true);
        }

        public void ReverseImage_Off()
        {
            ImagedReversed = false;
            RenderTexture.material.mainTextureScale = new Vector2(1, 1); // in the inspector, this is listed under Main Maps and labeled "Tiling" 
            SidednessDot_Normal.SetActive(true);
            SidednessDot_Flipped.SetActive(false);
        }


        // FREEZING the US image (without turning the device on or off) is done by simply turning the render texture cameras on and off.
        public Camera Ultrasound_Image_Camera;   // looks at the simulated ultrasound image (lower resolution) and has effects applied to it like blur
        public Camera Ultrasound_Display_Camera; // looks at the overlayed elements of the ultrasound display, like cm scale, centerlines, sidedness dot, etc with no image effects like blur

        public void FreezeToggle()
        {
            Ultrasound_Image_Camera.enabled = !Ultrasound_Image_Camera.enabled;
            Ultrasound_Display_Camera.enabled = !Ultrasound_Display_Camera.enabled;
        }

        public void Freeze()
        {
            Ultrasound_Image_Camera.enabled = false;
            Ultrasound_Display_Camera.enabled = false;
        }

        public void UnFreeze()
        {
            Ultrasound_Image_Camera.enabled = true;
            Ultrasound_Display_Camera.enabled = true;
        }

        // Controlling the visiblity of the In-Plane Cognitive Aid
        // this is the view looking down the axis of the probe
        public GameObject CogAid_NeedlePlaneIndicator;
        public void NeedlePlaneCogAidToggle()
        {
            CogAid_NeedlePlaneIndicator.SetActive(!CogAid_NeedlePlaneIndicator.activeInHierarchy);
        }

        public void NeedlePlaneCogAid_On()
        {
            CogAid_NeedlePlaneIndicator.SetActive(true);
        }

        public void NeedlePlaneCogAid_Off()
        {
            CogAid_NeedlePlaneIndicator.SetActive(false);
        }

        // Controlling the visiblity of the insonation angle Cognitive Aids 
        // this is the view looking at one of the sides of the probe
        // there are two cog aids for this; one for in-plane needling and another for out-of-plane needling. they are mutually exclusive.
        // the controller for these two cog aids determines which one to render. 
        // This switch turns on and off the set; if off, neither is shown. if on, either the in-plane needling or out-of-plane needling is shown. 
        public GameObject CogAid_NeedleInsonationAngleIndicator; // this game object holds both in-plane and out-of-plane cognitive aids and their shared controller.
        public void NeedleInsonationAngleCogAidToggle()
        {
            CogAid_NeedleInsonationAngleIndicator.SetActive(!CogAid_NeedleInsonationAngleIndicator.activeInHierarchy);
        }

        public void NeedleInsonationAngleCogAid_On()
        {
            CogAid_NeedleInsonationAngleIndicator.SetActive(true);
        }

        public void NeedleInsonationAngleCogAid_Off()
        {
            CogAid_NeedleInsonationAngleIndicator.SetActive(false);
        }


    }
}

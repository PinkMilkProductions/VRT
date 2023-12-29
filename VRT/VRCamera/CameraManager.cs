using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;
using Valve.VR;

namespace VRMaker
{
    public static class CameraManager
    {
        static CameraManager()
        {
            CurrentCameraMode = VRCameraMode.UI;
            // Fix it for the First stereo camera
            if (VRT.Main.FirstCam != null)
            {
                VRT.Main.FirstCam.nearClipPlane = NearClipPlaneDistance;
                VRT.Main.FirstCam.farClipPlane = FarClipPlaneDistance;
            }
            // Fix it for the second stereo camera
            if (VRT.Main.SecondCam != null)
            {
                VRT.Main.SecondCam.nearClipPlane = NearClipPlaneDistance;
                VRT.Main.SecondCam.farClipPlane = FarClipPlaneDistance;
            }

        }

        public static void ReduceNearClipping()
        {
            // Fix it for the First stereo camera
            if (VRT.Main.FirstCam != null)
            {
                VRT.Main.FirstCam.nearClipPlane = NearClipPlaneDistance;
                VRT.Main.FirstCam.farClipPlane = FarClipPlaneDistance;
            }
            // Fix it for the second stereo camera
            if (VRT.Main.SecondCam != null)
            {
                VRT.Main.SecondCam.nearClipPlane = NearClipPlaneDistance;
                VRT.Main.SecondCam.farClipPlane = FarClipPlaneDistance;
            }
        }

        /*
        public static void TurnOffPostProcessing()
        {
            Camera CurrentCamera = Game.GetCamera();
            UnityEngine.PostProcessing.PostProcessingBehaviour PPbehaviour = CurrentCamera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
            PPbehaviour.enabled = false;
        }
        */

        public static void AddSkyBox()
        {
            //// ADD THE LOADED SKYBOX !!!!
            //var SceneSkybox = GameObject.Instantiate(AssetLoader.Skybox, Vector3.zeroVector, Quaternion.identityQuaternion);
            //SceneSkybox.transform.localScale = new Vector3(999999, 999999, 999999);
            //SceneSkybox.transform.eulerAngles = new Vector3(270, 0, 0);
            //Renderer rend = SceneSkybox.GetComponent<Renderer>();
            //rend.enabled = true;
        }

        public static void SpawnHands()
        {
            //if (!RightHand)
            //{
            //    RightHand = GameObject.Instantiate(AssetLoader.RightHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
            //    RightHand.transform.parent = VROrigin.transform;
            //    Renderer rend = RightHand.GetComponent<Renderer>();
            //    rend.enabled = true;
            //}
            //if (!LeftHand)
            //{
            //    LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
            //    LeftHand.transform.parent = VROrigin.transform;
            //    Renderer rend = LeftHand.GetComponent<Renderer>();
            //    rend.enabled = true;
            //}
        }

        public static void HandleDemeoCamera()
        {
            if ((CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.DemeoLike)
                && RightHand && LeftHand)
            {
                // Add physics to the VROrigin
                if (!VROrigin.GetComponent<Rigidbody>())
                {
                    Rigidbody tempvar = VROrigin.AddComponent<Rigidbody>();
                    tempvar.useGravity = false;
                }    
                Rigidbody VROriginPhys = VROrigin.GetComponent<Rigidbody>();

                // If we are grabbing with both our hands
                if (RightHandGrab && LeftHandGrab)
                {
                  

                    // ROTATING
                    // Setup
                    if (InitialRotation == true)
                    {
                        InitialRotationPoint = Vector3.Lerp(CameraManager.LeftHand.transform.position, CameraManager.RightHand.transform.position, 0.5f);
                        PreviousRotationVector = CameraManager.LeftHand.transform.position - InitialRotationPoint;
                        PreviousRotationVector.y = 0;
                        InitialRotation = false;
                    }
                    Vector3 RotationVector = CameraManager.LeftHand.transform.position - CameraManager.RightHand.transform.position;
                    RotationVector.y = 0;
                    float Angle = Vector3.SignedAngle(PreviousRotationVector, RotationVector, Vector3.up);
                    Angle = Angle / 2;
                    
                    // Do the actual rotating
                    VROrigin.transform.RotateAround(InitialRotationPoint, Vector3.up, Angle);

                    PreviousRotationVector = RotationVector;
                }
                else if (RightHandGrab || LeftHandGrab)
                {
                    // Reset scaling/rotating flags
                    InitialHandDistance = 0f;
                    InitialRotation = true;

                    
                }
                else
                {
                    // Reset flags + stop extra camera movement
                    InitialHandDistance = 0f;
                    InitialRotation = true;
                    VROriginPhys.velocity = Vector3.zero;
                }
            }
        }


        public static void HandleStereoRendering()
        {
            //Camera.main.fieldOfView = SteamVR.instance.fieldOfView;
            VRT.Main.FirstCam.gameObject.SetActive(true);


            VRT.Main.FirstEye.transform.position = Camera.main.transform.position;
            VRT.Main.FirstEye.transform.rotation = Camera.main.transform.rotation;
            VRT.Main.FirstEye.transform.localScale = Camera.main.transform.localScale;
            VRT.Main.FirstCam.enabled = true;
            VRT.Main.FirstCam.stereoTargetEye = StereoTargetEyeMask.Left;
            VRT.Main.FirstCam.projectionMatrix = VRT.Main.FirstCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            VRT.Main.FirstCam.targetTexture = VRT.Main.MyDisplay.GetRenderTextureForRenderPass(0);

            //Camera.main.enabled = true;
            //Camera.main.stereoTargetEye = StereoTargetEyeMask.Left;
            //Camera.main.projectionMatrix = Camera.main.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            //Camera.main.targetTexture = VRT.Main.MyDisplay.GetRenderTextureForRenderPass(0);

            VRT.Main.SecondCam.gameObject.SetActive(true);

            VRT.Main.SecondEye.transform.position = Camera.main.transform.position;
            VRT.Main.SecondEye.transform.rotation = Camera.main.transform.rotation;
            VRT.Main.SecondEye.transform.localScale = Camera.main.transform.localScale;
            VRT.Main.SecondCam.enabled = true;
            VRT.Main.SecondCam.stereoTargetEye = StereoTargetEyeMask.Right;
            VRT.Main.SecondCam.projectionMatrix = VRT.Main.SecondCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            VRT.Main.SecondCam.targetTexture = VRT.Main.MyDisplay.GetRenderTextureForRenderPass(1);
        }



        public enum VRCameraMode
        {
            DemeoLike,
            FirstPerson,
            Cutscene,
            UI
        }

        //Strictly camera stuff
        public static VRCameraMode CurrentCameraMode;
        public static float NearClipPlaneDistance = 0.01f;
        public static float FarClipPlaneDistance = 59999f;
        public static bool DisableParticles = false;

        // VR Origin and body stuff
        public static Transform OriginalCameraParent = null;
        public static GameObject VROrigin = new GameObject();
        public static GameObject LeftHand = null;
        public static GameObject RightHand = null;
        

        // VR Input stuff
        public static bool RightHandGrab = false;
        public static bool LeftHandGrab = false;
        public static Vector2 LeftJoystick = Vector2.zero;
        public static Vector2 RightJoystick = Vector2.zero;

        // Demeo-like camera stuff
        public static float InitialHandDistance = 0f;
        public static bool InitialRotation = true;
        public static Vector3 PreviousRotationVector = Vector3.zero;
        public static Vector3 InitialRotationPoint = Vector3.zero;
        public static Vector3 ZoomOrigin = Vector3.zero;
        public static float SpeedScalingFactor = 1f;

        // FIrst person camera stuff
        public static float Turnrate = 3f;

    }
    
}

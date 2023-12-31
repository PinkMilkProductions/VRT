﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;
using Valve.VR;
using Kingmaker.Designers;

namespace VRMaker
{
    public static class CameraManager
    {
        static CameraManager()
        {
            CurrentCameraMode = VRCameraMode.UI;

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

        public static void SwitchPOV()
        {
            Logs.WriteInfo("Entered SwitchPOV function");

            Camera OriginalCamera = Game.GetCamera();
            // If we are not in firstperson
            if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
            {
                Logs.WriteInfo("Got past cameramode check");
                if (GameHelper.GetPlayerCharacter() != null)
                {
                    Logs.WriteInfo("Got past maincharacter exist check");
                    // switch to first person
                    VROrigin.transform.parent = null;
                    VROrigin.transform.position = GameHelper.GetPlayerCharacter().Position;

                    //VROrigin.transform.LookAt(Game.Instance.Player.MainCharacter.Value.OrientationDirection);

                    if (!OriginalCameraParent)
                    {
                        OriginalCameraParent = OriginalCamera.transform.parent;
                    }

                    OriginalCamera.transform.parent = VROrigin.transform;
                    if (RightHand)
                        RightHand.transform.parent = VROrigin.transform;
                    if (LeftHand)
                        LeftHand.transform.parent = VROrigin.transform;
                    if (VRT.Main.FirstCam)
                        VRT.Main.FirstCam.transform.parent = VROrigin.transform;
                    if (VRT.Main.SecondCam)
                        VRT.Main.SecondCam.transform.parent = VROrigin.transform;
                    CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
                }

            }
            else
            {
                VROrigin.transform.position = OriginalCameraParent.position;
                VROrigin.transform.rotation = OriginalCameraParent.rotation;
                VROrigin.transform.localScale = OriginalCameraParent.localScale;

                VROrigin.transform.parent = OriginalCameraParent;

                CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.DemeoLike;
            }
        }

        public static void SpawnHands()
        {
            if (!RightHand)
            {
                //RightHand = GameObject.Instantiate(AssetLoader.RightHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
                RightHand = new GameObject("RightHand");
                RightHand.transform.parent = VROrigin.transform;
            }
            if (!LeftHand)
            {
                //LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
                LeftHand = new GameObject("LeftHand");
                LeftHand.transform.parent = VROrigin.transform;
            }
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
                    // SCALING
                    // Setup
                    if (InitialHandDistance == 0f)
                    {
                        InitialHandDistance = Vector3.Distance(CameraManager.RightHand.transform.position, CameraManager.LeftHand.transform.position);
                        ZoomOrigin = VROrigin.transform.position;
                    }
                    float HandDistance = Vector3.Distance(CameraManager.RightHand.transform.position, CameraManager.LeftHand.transform.position);
                    float scale = HandDistance / InitialHandDistance;

                    // Do the actual distance scaling
                    VROrigin.transform.position = Vector3.LerpUnclamped(GameHelper.GetPlayerCharacter().Position, ZoomOrigin, scale);

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

                    // MOVING CAMERA
                    SpeedScalingFactor = Mathf.Clamp(Math.Abs(Vector3.Distance(GameHelper.GetPlayerCharacter().Position, VROrigin.transform.position)), 1.0f, FarClipPlaneDistance);
                    if (RightHandGrab)
                    {
                        Vector3 ScaledSpeed = SteamVR_Actions._default.RightHandPose.velocity * SpeedScalingFactor;
                        Vector3 AdjustedSpeed = new Vector3(-ScaledSpeed.x, -ScaledSpeed.y, -ScaledSpeed.z);
                        VROriginPhys.velocity = VROrigin.transform.rotation * AdjustedSpeed;
                    }
                    if (LeftHandGrab)
                    {
                        Vector3 ScaledSpeed = SteamVR_Actions._default.LeftHandPose.velocity * SpeedScalingFactor;
                        Vector3 AdjustedSpeed = new Vector3(-ScaledSpeed.x, -ScaledSpeed.y, -ScaledSpeed.z);
                        VROriginPhys.velocity = VROrigin.transform.rotation * AdjustedSpeed;
                    }
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

        public static void HandleFirstPersonCamera()
        {
            if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                // POSITION
                // Attach our origin to the Main Character's (this function gets called every tick)
                CameraManager.VROrigin.transform.position = GameHelper.GetPlayerCharacter().Position;
                //VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.EyePosition;

                //ROTATION
                Vector3 RotationEulers = new Vector3(0, Turnrate * RightJoystick.x, 0);
                VROrigin.transform.Rotate(RotationEulers);

                // Movement is done via a patch
            }
        }

        public static void HandleStereoRendering()
        {
            HandleFirstPersonCamera();
            HandleDemeoCamera();
            //Camera.main.fieldOfView = SteamVR.instance.fieldOfView;
            VRT.Main.FirstCam.gameObject.SetActive(true);


            VRT.Main.FirstEye.transform.position = Game.GetCamera().transform.position;
            VRT.Main.FirstEye.transform.rotation = Game.GetCamera().transform.rotation;
            VRT.Main.FirstEye.transform.localScale = Game.GetCamera().transform.localScale;
            VRT.Main.FirstCam.enabled = true;
            VRT.Main.FirstCam.stereoTargetEye = StereoTargetEyeMask.Left;
            VRT.Main.FirstCam.projectionMatrix = VRT.Main.FirstCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            VRT.Main.FirstCam.targetTexture = VRT.Main.MyDisplay.GetRenderTextureForRenderPass(0);

            VRT.Main.SecondCam.gameObject.SetActive(true);

            VRT.Main.SecondEye.transform.position = Game.GetCamera().transform.position;
            VRT.Main.SecondEye.transform.rotation = Game.GetCamera().transform.rotation;
            VRT.Main.SecondEye.transform.localScale = Game.GetCamera().transform.localScale;
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
        public static float NearClipPlaneDistance = 0.0001f;
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

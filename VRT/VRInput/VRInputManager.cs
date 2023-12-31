using Kingmaker;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using static VRT.Main;

namespace VRMaker
{
    public class VRInputManager
    {
        static VRInputManager()
        {
            Logs.WriteInfo("calling VRInputManager SetUpListeners...");
            SetUpListeners();
            Logs.WriteInfo("VRInputManager SetUpListeners finished");
        }

        public static void SetUpListeners()
        {
            // BOOLEANS
            SteamVR_Actions._default.grabright.AddOnStateDownListener(GrabRightDown, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.grabright.AddOnStateUpListener(GrabRightUp, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.grableft.AddOnStateDownListener(GrabLeftDown, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.grableft.AddOnStateUpListener(GrabLeftUp, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.switchpov.AddOnStateDownListener(OnSwitchPOVDown, SteamVR_Input_Sources.Any);
            Logs.WriteInfo("switchpov bound");

            //Debug
            SteamVR_Actions._default.pause.AddOnStateDownListener(OnPauseDown, SteamVR_Input_Sources.Any);

            // VECTOR 2Ds
            SteamVR_Actions._default.move.AddOnUpdateListener(OnLeftJoystickUpdate, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.movecamera.AddOnUpdateListener(OnRightJoystickUpdate, SteamVR_Input_Sources.Any);

            // POSES
            SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateRightHand);
            SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);
        }


        // BOOLEANS
        public static void OnSwitchPOVDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Logs.WriteInfo("OnSwitchPOVDown called");
            if (!(VRT.Main.FirstCam && VRT.Main.SecondCam))
            {

                CameraManager.ReduceNearClipping();

                //Without this there is no headtracking
                Camera.main.gameObject.AddComponent<SteamVR_TrackedObject>();

                VRT.Main.FirstEye = new GameObject("FirstEye");
                VRT.Main.FirstCam = VRT.Main.FirstEye.AddComponent<Camera>();
                VRT.Main.FirstCam.gameObject.AddComponent<SteamVR_TrackedObject>();
                VRT.Main.FirstCam.CopyFrom(Camera.main);

                // Without this the right eye gets stuck at a very far point in the map
                VRT.Main.FirstCam.transform.parent = Camera.main.transform.parent;

                VRT.Main.SecondEye = new GameObject("SecondEye");
                VRT.Main.SecondCam = VRT.Main.SecondEye.AddComponent<Camera>();
                VRT.Main.SecondCam.gameObject.AddComponent<SteamVR_TrackedObject>();
                VRT.Main.SecondCam.CopyFrom(Camera.main);

                // Without this the right eye gets stuck at a very far point in the map
                VRT.Main.SecondCam.transform.parent = Camera.main.transform.parent;



                //// Pimax 5K plus causes the fog of war to behave very bad, this is supposed to fix it but doesn't work yet.
                //if (Plugin.HMDModel == "Vive MV")
                //{
                //    Logs.WriteInfo("HMD recognised as VIVE MV, disabling FogOfWar");
                //    Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar.FogOfWarFeature.Instance.DisableFeature();
                //}



                CameraManager.VROrigin = new GameObject("VROrigin");

            }
            SteamVR.settings.lockPhysicsUpdateRateToRenderFrequency = false;
            Time.maximumDeltaTime = Time.fixedDeltaTime;
            CameraManager.SpawnHands();
            CameraManager.SwitchPOV();
            
        }

        public static void OnPauseDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Logs.WriteInfo("pause is Down");
            Game.Instance.ControllerMode = Game.ControllerModeType.Gamepad;
            Logs.WriteInfo("ControllerMode is: " + Game.Instance.ControllerMode);
            Controllers.Init();
            LogBinds();
        }

        public static void GrabRightDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.RightHandGrab = true;
        }

        public static void GrabRightUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.RightHandGrab = false;
        }

        public static void GrabLeftDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.LeftHandGrab = true;
        }

        public static void GrabLeftUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.LeftHandGrab = false;
        }

        // VECTOR 2Ds
        public static void OnLeftJoystickUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            // Doesn't seem to stop joystick drift in it's current state?
            if (axis.magnitude > 0.1f)
                CameraManager.LeftJoystick = axis;
            else
                CameraManager.LeftJoystick = Vector2.zero;
        }

        public static void OnRightJoystickUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            // Doesn't seem to stop joystick drift in it's current state?
            if (axis.magnitude > 0.1f)
                CameraManager.RightJoystick = axis;
            else
                CameraManager.RightJoystick = Vector2.zero;
        }


        // POSES
        public static void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CameraManager.RightHand)
            {
                CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
                CameraManager.RightHand.transform.localRotation = fromAction.localRotation;
            }

        }

        public static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CameraManager.LeftHand)
            {
                CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
                CameraManager.LeftHand.transform.localRotation = fromAction.localRotation;
            }
        }


        // GAMEPAD STUFF

        public static void LogBinds()
        {
            //Controllers.LogAllGameActions(Owlcat.Runtime.UI.ConsoleTools.GamepadInput.GamePad.Instance.Player);
            Controllers.LogAllGameActions(Owlcat.Runtime.UI.ConsoleTools.GamepadInput.GamePad.s_Instance.Player);
        }
    }
}

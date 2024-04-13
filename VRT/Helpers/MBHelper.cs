using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace VRMaker
{
    public class MBHelper : MonoBehaviour
    {
        public static MBHelper Create()
        {
            var instance = new GameObject(nameof(MBHelper)).AddComponent<MBHelper>();

            // Do some unrelated stuff we still want to do on creation time
            instance.OnCreate();

            return instance;
        }

        public void Update()
        {
            //Logs.WriteInfo("MyHelper Update hook called");
            if (VRT.Main.FirstCam && VRT.Main.SecondCam)
            {
                CameraManager.HandleStereoRendering();
            }
            // Used for determining a transform for the loadingscreen when there is no camera.
            if (Kingmaker.Game.GetCamera())
                VRT.Main.MyHelper.transform.position = Kingmaker.Game.GetCamera().transform.position + Kingmaker.Game.GetCamera().transform.forward;
            //Lazy fix for the load save menu clipping at the main menu
            if (Camera.main.nearClipPlane > 0.2f)
                CameraManager.ReduceNearClipping();
            Controllers.Update();
            CameraManager.HandleSkyBox();
        }

        public void OnCreate()
        {
            // Test
            Logs.WriteInfo("Myhelper OnCreate() called");
            //CameraManager.HandleStereoRendering();
        }
    }
}

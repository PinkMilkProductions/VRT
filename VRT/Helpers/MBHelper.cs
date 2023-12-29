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
            Logs.WriteInfo("MyHelper Update hook called");
            if (VRT.Main.SecondCam && Camera.main)
            {
                CameraManager.HandleStereoRendering();
            }
            //Controllers.Update();
        }

        public void OnCreate()
        {
            // Test
            Logs.WriteInfo("Myhelper OnCreate() CameraManager.HandleStereoRendering() called");
            CameraManager.HandleStereoRendering();
        }
    }
}

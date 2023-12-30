using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using VRMaker;
using Kingmaker;
using Rewired;
using Valve.VR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using Unity.XR.OpenVR;

namespace VRT;

#if DEBUG
[EnableReloading]
#endif
static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;

    //BepinEx main vars
    public static string gameExePath = Process.GetCurrentProcess().MainModule.FileName;
    public static string gamePath = Path.GetDirectoryName(gameExePath);
    public static string HMDModel = "";

    public static MBHelper MyHelper;

    public static UnityEngine.XR.Management.XRManagerSettings managerSettings = null;

    public static List<UnityEngine.XR.XRDisplaySubsystemDescriptor> displaysDescs = new List<UnityEngine.XR.XRDisplaySubsystemDescriptor>();
    public static List<UnityEngine.XR.XRDisplaySubsystem> displays = new List<UnityEngine.XR.XRDisplaySubsystem>();
    public static UnityEngine.XR.XRDisplaySubsystem MyDisplay = null;

    public static GameObject FirstEye = null;
    public static Camera FirstCam = null;
    public static GameObject SecondEye = null;
    public static Camera SecondCam = null;



    //Create a class that actually inherits from MonoBehaviour
    public class MyStaticMB : MonoBehaviour
    {
    }

    //Variable reference for the class
    public static MyStaticMB myStaticMB;


    static bool Load(UnityModManager.ModEntry modEntry)
    {
        log = modEntry.Logger;
#if DEBUG
        modEntry.OnUnload = OnUnload;
#endif
        modEntry.OnGUI = OnGUI;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());


        //BepinEx Awake() stuff
        // Plugin startup logic
        Logs.WriteInfo($"Plugin VRT is loaded!");

        new AssetLoader();

        //If the instance not exit the first time we call the static class
        if (myStaticMB == null)
        {
            //Create an empty object called MyStatic
            GameObject gameObject = new GameObject("MyStatic");


            //Add this script to the object
            myStaticMB = gameObject.AddComponent<MyStaticMB>();
        }

        myStaticMB.StartCoroutine(InitVRLoader());

        //Game.s_Instance.ControllerMode = Game.ControllerModeType.Gamepad;

        Logs.WriteInfo("Reached end of VRT.Load()");

        return true;
    }

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        Logs.WriteInfo("Testing 'OnGUI' log");
    }

#if DEBUG
    static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif

    public static System.Collections.IEnumerator InitVRLoader()
    {

        SteamVR_Actions.PreInitialize();

        var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
        managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
        var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();


        var settings = OpenVRSettings.GetSettings();
        settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;


        generalSettings.Manager = managerSettings;

        managerSettings.loaders.Clear();
        managerSettings.loaders.Add(xrLoader);

        managerSettings.InitializeLoaderSync(); ;


        XRGeneralSettings.AttemptInitializeXRSDKOnLoad();
        XRGeneralSettings.AttemptStartXRSDKOnBeforeSplashScreen();

        SteamVR.Initialize(true);


        SubsystemManager.GetInstances(displays);
        MyDisplay = displays[0];
        MyDisplay.Start();

        Logs.WriteInfo("SteamVR hmd modelnumber: " + SteamVR.instance.hmd_ModelNumber);
        HMDModel = SteamVR.instance.hmd_ModelNumber;

        new VRInputManager();

        Logs.WriteInfo("Reached end of InitVRLoader");

        yield return null;

    }
}
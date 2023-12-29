using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace VRMaker
{
    class Logs
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public static void WriteInfo(string data)
        {
            VRT.Main.log.Log(data);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void WriteWarning(string data)
        {
            VRT.Main.log.Warning(data);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void WriteError(string data)
        {
            VRT.Main.log.Error(data);
        }
    }
}

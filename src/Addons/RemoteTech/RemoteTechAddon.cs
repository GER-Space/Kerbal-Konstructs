using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Addons
{
    internal class RemoteTechAddon
    {

        internal static bool isInitialized = false;
        internal static Assembly rtAssembly = null;

        internal static List<Guid> allGroundStations = new List<Guid>();
        private static Dictionary<string, int> bodies;
        private static Dictionary<string,Guid> groundStations;


        /// <summary>
        /// Checks if RemoteTech assembly is loaded
        /// </summary>
        internal static bool isInstalled
        {
            get          
                {
                    Assembly assembly = (from a in AssemblyLoader.loadedAssemblies
                                         where a.name.ToLower().Equals("RemoteTech".ToLower())
                                         select a).FirstOrDefault().assembly;
                    return assembly != null;
                }
        }


        internal static void Init()
        {
            if (!isInstalled)
                return;

            rtAssembly = (from a in AssemblyLoader.loadedAssemblies
                          where a.name.ToLower().Equals("RemoteTech".ToLower())
                          select a).FirstOrDefault().assembly;

            // create lookup table for body to int
            int count = 0;
            foreach (var body in FlightGlobals.Bodies)
            {
                count++;
                bodies.Add(body.name, count);
            }
            isInitialized = true;
        }

        internal static void AddGroundstation(string name, double lat, double lon, float alt, CelestialBody body)
        {
            
            Type type = rtAssembly.GetType("RemoteTech.API.API");

            MethodInfo methodInfo = type.GetMethod("AddGroundStation");
            // public static Guid AddGroundStation(string name, double latitude, double longitude, double height, int body)
            var returnValue = ((List<Guid>)methodInfo.Invoke(null, new object[] { "Test" , 90d , 90d, 100, 1  })).FirstOrDefault();

            allGroundStations.Add(returnValue);
            groundStations.Add("name", returnValue);


        }

        internal static void CloseAllStations()
        {

        }



    }
}

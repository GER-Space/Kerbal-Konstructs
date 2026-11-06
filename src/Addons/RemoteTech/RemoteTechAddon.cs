using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KerbalKonstructs.Addons
{
    internal class RemoteTechAddon
    {

        internal static bool isInitialized = false;
        internal static Assembly rtAssembly = null;
        internal static Type rtType = null;
        private static Dictionary<string, int> bodies = new Dictionary<string, int>();



        /// <summary>
        /// Checks if RemoteTech assembly is loaded
        /// </summary>
        internal static bool isInstalled
        {
            get
            {
                var assembly = (from a in AssemblyLoader.loadedAssemblies
                                where a.name.ToLower().Equals("RemoteTech".ToLower())
                                select a).FirstOrDefault();
                return assembly != null;
            }
        }

        /// <summary>
        /// Initialize the shared resources for the RT reflection calls.
        /// </summary>
        internal static void Init()
        {
            if (!isInstalled)
                return;

            rtAssembly = (from a in AssemblyLoader.loadedAssemblies
                          where a.name.ToLower().Equals("RemoteTech".ToLower())
                          select a).FirstOrDefault().assembly;

            rtType = rtAssembly.GetType("RemoteTech.API.API");
            // create lookup table for body to int

            for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                bodies.Add(FlightGlobals.Bodies[i].name, i);
            }
            isInitialized = true;
        }


        /// <summary>
        /// Check if RemoteTech is enabled in the mods settings
        /// </summary>
        /// <returns></returns>
        internal static bool IsRemoteTechEnabled()
        {
            if (!isInstalled)
                return false;

            if (!isInitialized)
                Init();


            MethodInfo methodInfo = rtType.GetMethod("IsRemoteTechEnabled");

            bool returnValue = (bool)methodInfo.Invoke(null, null);
            return returnValue;
        }


        /// <summary>
        /// Opens a GroundStation at the position
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        internal static Guid AddGroundstation(string name, double lat, double lon, double alt, CelestialBody body)
        {
            if (!isInstalled)
                return Guid.Empty;

            if (!isInitialized)
                Init();

            MethodInfo methodInfo = rtType.GetMethod("AddGroundStation");

            Guid returnValue = (Guid)methodInfo.Invoke(null, new object[] { name, lat, lon, alt, bodies[body.name] });
            return returnValue;
        }


        internal static Guid GetGroundStationGuid(String name)
        {
            if (!isInstalled)
                return Guid.Empty;

            if (!isInitialized)
                Init();

            MethodInfo methodInfo = rtType.GetMethod("GetGroundStationGuid");
            Guid returnValue = (Guid)methodInfo.Invoke(null, new object[] { name });
            return returnValue;

        }

        internal static void RemoveGroundStation(Guid stationId)
        {
            if (!isInstalled)
            {
                return;
            }

            if (!isInitialized)
            {
                Init();
            }

            MethodInfo methodInfo = rtType.GetMethod("RemoveGroundStation");
            methodInfo.Invoke(null, new object[] { stationId });
        }

        /// <summary>
        /// Change the Omni range of a ground station.
        /// Note that this change is temporary. For example it is overridden to the value written in the settings file if the tracking station is upgraded.
        /// </summary>
        /// <param name="stationId">The station ID for which to change the antenna range.</param>
        /// <param name="newRange">The new range in meters.</param>
        /// <returns>true if the ground station antenna range was changed, false otherwise.</returns>
        public static void ChangeGroundStationRange(Guid stationId, float newRange)
        {
            if (!isInstalled)
                return;

            if (!isInitialized)
                Init();

            MethodInfo methodInfo = rtType.GetMethod("ChangeGroundStationRange");
            methodInfo.Invoke(null, new object[] { stationId, newRange });
        }



        internal static void CloseAllStations()
        {
            if (!isInstalled)
            {
                return;
            }

            if (!isInitialized)
            {
                Init();
            }

            Guid stationID = Guid.Empty;
            MethodInfo methodInfo = rtType.GetMethod("GetGroundStations");
            IEnumerable<string> allStations = null;
            allStations = (IEnumerable<string>)methodInfo.Invoke(null, null);

            List<Guid> toRemove = new List<Guid>();
            foreach (string stationName in allStations)
            {
                stationID = GetGroundStationGuid(stationName);
                //don't remove mission control
                if (stationID.ToString("N").Equals("5105f5a9d62841c6ad4b21154e8fc488"))
                {
                    continue;
                }

                toRemove.Add(stationID);
            }

            foreach(Guid id in toRemove)
                RemoveGroundStation(id);
        }

    }
}

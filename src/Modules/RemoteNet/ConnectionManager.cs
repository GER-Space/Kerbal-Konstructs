using System;
using System.Collections.Generic;
using System.Linq;
using KerbalKonstructs.Core;
using UnityEngine;
using CommNet;
using KerbalKonstructs.Addons;
#if!KSP12
using KSP.Localization;
#endif
namespace KerbalKonstructs.Modules
{

    internal class ConnectionManager
    {

        static List<StockStation> stockGroundStations = new List<StockStation>();
        static bool stockWasRemoved = false;

        static internal List<StaticInstance> openCNStations = new List<StaticInstance>();
        static internal Dictionary<StaticInstance, Guid> openRTStations = new Dictionary<StaticInstance, Guid>();


        internal struct StockStation
        {
            internal GameObject gameObject;
            internal string nodeName;
            internal string displaynodeName;
            internal Transform nodeTransform;
            internal bool isKSC;
        }


        /// <summary>
        /// creates the list of stock groundstations. This should never change
        /// </summary>
        internal static void ScanForStockCommNet()
        {
            foreach (var home in UnityEngine.Object.FindObjectsOfType<CommNetHome>())
            {
                // we never touch the KSC
                if (home.isKSC == true)
                {
                    continue;
                }
                StockStation stockStation = new StockStation();
                stockStation.gameObject = home.gameObject;
                stockStation.nodeName = home.nodeName;
#if !KSP12
                stockStation.displaynodeName = Localizer.Format(home.displaynodeName);
#endif
                stockStation.nodeTransform = home.nodeTransform;
                stockStation.isKSC = home.isKSC;

                Log.Normal("Added stock CommNet to list: " + home.nodeName);

                stockGroundStations.Add(stockStation);
            }
        }


        /// <summary>
        /// Removes all custom Stations added by KK
        /// </summary>
        internal static void ResetAll()
        {
            var openRTStationsArray = openRTStations.Values.ToArray();
            var openCommNetStations = openCNStations.ToArray();

            foreach (var rtStation in openRTStationsArray)
            {
                RemoteTechAddon.RemoveGroundStation(rtStation);
            }
            openRTStations.Clear();

            foreach (var commnetStation in openCommNetStations)
            {
                CloseCommNetStation(commnetStation);
            }
            openCNStations.Clear();
        }




        /// <summary>
        /// iterates through all potential open TrackingStations and attaches GroundStations
        /// </summary>
        internal static void LoadGroundStations()
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.LOADING)
            {
                return;
            }


            // only remove the Stock Stations if we should do it.
            if (KerbalKonstructs.instance.dontRemoveStockCommNet || KerbalKonstructs.instance.enableCommNet == false)
            {
                RestoreStockCommNet();
            }
            else
            {
                RemoveStockStations();
            }
            // needed as long the Groundstation list is still not saved in persestence layer.
            if (KerbalKonstructs.instance.enableRT)
            {
                // just remove all groundstations and add them later
                RemoteTechAddon.CloseAllStations();
            }

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {

                if (instance.facilityType != KKFacilityType.GroundStation && instance.facilityType != KKFacilityType.TrackingStation)
                {
                    continue;
                }

                // we check here for the gamemode etc
                if (!instance.myFacilities[0].isOpen)
                {
                    continue;
                }

                if (openRTStations.ContainsKey(instance) || openCNStations.Contains(instance))
                {
                    continue;
                }

                // Don't revreal hidden bases
                if (instance.myFacilities[0].OpenCost == 0f && instance.groupCenter.isHidden)
                {
                    continue;
                }

                AttachGroundStation(instance);
            }
        }

        /// <summary>
        /// Destroys the Stock GroundStations.
        /// </summary>
        internal static void RemoveStockStations()
        {
            if (!CommNet.CommNetScenario.CommNetEnabled)
            {
                KerbalKonstructs.instance.enableCommNet = false;
                return;

            }

            if (!stockWasRemoved && HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations)
            {
                foreach (StockStation station in stockGroundStations)
                {
                    var home = station.gameObject.GetComponentInChildren<CommNetHome>(true);
                    if (home != null)
                    {
                        UnityEngine.Object.Destroy(home);
                        Log.Normal("deleted stock GroundStation: " + station.displaynodeName);
                    }
                    else
                    {
                        Log.Warning("Could not find CommNet to delete: " + station.displaynodeName);
                    }
                }
                stockWasRemoved = true;
                CommNet.CommNetNetwork.Reset();
            }
        }

        /// <summary>
        /// restores the Stock GroundStations to the places where they belong to.
        /// </summary>
        internal static void RestoreStockCommNet()
        {
            if (stockWasRemoved)
            {
                foreach (StockStation station in stockGroundStations)
                {
                    KKCommNetHome commNetHome = station.gameObject.AddComponent<KKCommNetHome>();
                    commNetHome.RestoreStockStation(station);
                    Log.Normal("Restored CommNetHome: " + station.displaynodeName);

                }
                stockWasRemoved = false;
                CommNet.CommNetNetwork.Reset();
            }
        }


        /// <summary>
        /// Attaches A CommNet or RemoteTech Groundstation to a Static Object instance
        /// </summary>
        /// <param name="instance"></param>
        internal static void AttachGroundStation(StaticInstance instance)
        {
            GroundStation myfacility = instance.myFacilities.Where(fac => fac.facType == KKFacilityType.GroundStation).First() as GroundStation;
            // we use a messure of 1000km from the settings.
            if (myfacility.TrackingShort == 0f || instance.Group == "KSCUpgrades")
            {
                return;
            }
            // add CommNet Groundstations
            if (KerbalKonstructs.instance.enableCommNet)
            {
                AddCommNetStation(myfacility);
            }
            // Add RemoteTech Groundstation
            if (KerbalKonstructs.instance.enableRT)
            {
                AddRemoteTechStation(myfacility);
            }
        }


        private static void AddCommNetStation(GroundStation facility)
        {
            StaticInstance instance = facility.staticInstance;
            float antennaPower = facility.TrackingShort * 1000000;
            Log.Normal("Adding Groundstation: " + instance.Group);
            if (!openCNStations.Contains(instance))
            {
                KKCommNetHome commNetGroudStation = instance.gameObject.AddComponent<KKCommNetHome>();
                commNetGroudStation.nodeName = instance.CelestialBody.name + ": " + instance.Group;
#if !KSP12
                commNetGroudStation.nodeName = instance.gameObject.name;

                commNetGroudStation.displaynodeName = instance.CelestialBody.name + ": " + instance.Group;
#endif
                // force them to be enabled all the time
                //commNetGroudStation.isKSC = true;
                commNetGroudStation.comm = new CommNode();
                commNetGroudStation.comm.antennaTransmit.power = antennaPower;
                //commNetGroudStation.enabled = true;
                openCNStations.Add(instance);
                CommNet.CommNetNetwork.Reset();
            }
            else
            {
                Log.UserError("Adding GroundStations should never be here: (instance allready added when open was called)");
            }
        }


        private static void AddRemoteTechStation(GroundStation facility)
        {
            float antennaPower = facility.TrackingShort * 1000000;
            StaticInstance instance = facility.staticInstance;

            Guid stationID = RemoteTechAddon.GetGroundStationGuid((instance.CelestialBody.name) + " " + instance.Group);
            if (stationID == Guid.Empty)
            {
                //double lng, lat, alt;
                //alt = instance.CelestialBody.pqsController.GetSurfaceHeight(instance.RadialPosition) - instance.CelestialBody.pqsController.radius + 15;
                //Vector3 objectPos = instance.CelestialBody.transform.InverseTransformPoint(instance.gameObject.transform.position);
                //lng = KKMath.GetLongitudeInDeg(objectPos);
                //lat = KKMath.GetLatitudeInDeg(objectPos);
                //stationID = RemoteTechAddon.AddGroundstation((instance.CelestialBody.name) + " " + instance.Group, lat, lng, alt, instance.CelestialBody);

                double altitude = instance.CelestialBody.pqsController.GetSurfaceHeight(instance.RadialPosition) - instance.CelestialBody.Radius + 15;
                stationID = RemoteTechAddon.AddGroundstation((instance.CelestialBody.name) + " " + instance.Group, instance.RefLatitude, instance.RefLongitude, altitude, instance.CelestialBody);
                Log.Normal("Got RTStationID: " + stationID);
            }
            openRTStations.Add(instance, stationID);
            RemoteTechAddon.ChangeGroundStationRange(stationID, antennaPower);
        }




        /// <summary>
        /// Removes all GroundStations from an Static Object.
        /// </summary>
        /// <param name="instance"></param>
        internal static void DetachGroundStation(StaticInstance instance)
        {
            if (KerbalKonstructs.instance.enableCommNet)
            {
                CloseCommNetStation(instance);
            }

            if (KerbalKonstructs.instance.enableRT)
            {
                if(openRTStations.TryGetValue(instance, out var station))
                {
                    openRTStations.Remove(instance);
                    RemoteTechAddon.RemoveGroundStation(station);
                }
                else
                {
                    Log.Warning("Trying To close a GroundStation that was never opened by us: " + instance.groupCenterName + ":" + instance.gameObject.name);
                }
            }
        }

        private static void CloseCommNetStation(StaticInstance instance)
        {
            if (openCNStations.Contains(instance))
            {
                Log.Normal("Closing Antenna for " + instance.Group);
                CommNetHome commNetGroundStation = instance.gameObject.GetComponent<CommNetHome>();
                //commNetGroundStation.enabled = false;
                UnityEngine.Object.Destroy(commNetGroundStation);
                openCNStations.Remove(instance);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using UnityEngine;
using CommNet;
using KerbalKonstructs.Addons;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Modules
{

    internal class RemoteNet
    {
        static List<StaticObject> openCNStations = new List<StaticObject>();
        static Dictionary<StaticObject, Guid> openRTStations = new Dictionary<StaticObject, Guid>();

        /// <summary>
        /// iterates through all potential open TrackingStations and attaches GroundStations
        /// </summary>
        internal static void LoadGroundStations()
        {
            // first we close any postential open Groundstations.
            if (KerbalKonstructs.instance.enableCommNet)
            {
                if (!CommNet.CommNetScenario.CommNetEnabled)
                {
                    KerbalKonstructs.instance.enableCommNet = false;
                }
                else
                {
                    CommNetHome[] homes = UnityEngine.Object.FindObjectsOfType<CommNetHome>();
                    for (int i = 0; i < homes.Length; i++)
                    {
                        Log.Normal("Found ComNet: " + homes[i].nodeName + " " + homes[i].isKSC);
                        if (homes[i].isKSC == true)
                            continue;
                        UnityEngine.Object.Destroy(homes[i]);
                    }
                }
            }
            // needed as long the Groundstation list is still not saved in persestence layer.
            if (KerbalKonstructs.instance.enableRT)
            {
                // nothing to do, as RT saves the groundstations an we query them later.

            }

            foreach (StaticObject instance in StaticDatabase.GetAllStatics())
            {

                if ((string)instance.getSetting("FacilityType") != "TrackingStation")
                {
                    continue;
                }

                if ((string)instance.getSetting("OpenCloseState") == "Closed" && (float)instance.getSetting("OpenCost") != 0f)
                {
                    continue;
                }

                if (openRTStations.ContainsKey(instance))
                    continue;
                if (openCNStations.Contains(instance))
                    continue;

                if (CareerUtils.isSandboxGame || CareerUtils.FacilityIsOpen(instance))
                {
                    AttachGroundStation(instance);
                }
            }
        }

        /// <summary>
        /// Attaches A CommNet or RemoteTech Groundstation to a Static Object instance
        /// </summary>
        /// <param name="instance"></param>
        internal static void AttachGroundStation(StaticObject instance)
        {
            // we use a messure of 1000km from the settings.
            float antennaPower = (float)instance.getSetting("TrackingShort") * 1000000;
            if (antennaPower == 0f || (string)instance.getSetting("Group") == "KSCUpgrades")
            {
                return;
            }

            // add CommNet Groundstations
            if (KerbalKonstructs.instance.enableCommNet)
            {

                Log.Normal("Adding Groundstation: " + (string)instance.getSetting("Group"));
                if (openCNStations.Contains(instance) == false)
                {
                    CommNetHome commNetGroudStation = instance.gameObject.AddComponent<CommNetHome>();

                    commNetGroudStation.nodeName = instance.body.name + " " + (string)instance.getSetting("Group");
                    //commNetGroudStation.enabled = true;
                    openCNStations.Add(instance);
                    CommNet.CommNetNetwork.Reset();
                }
                else
                {
                    Log.UserError("Adding GroundStations should never be here: (instance allready added when open was called)");
                }
            }
            // Add RemoteTech Groundstation
            if (KerbalKonstructs.instance.enableRT)
            {

                Guid stationID = RemoteTechAddon.GetGroundStationGuid((instance.body.name) + " " + (string)instance.getSetting("Group"));
                if (stationID == Guid.Empty)
                {
                    double lng, lat, alt;
                    alt = instance.body.pqsController.GetSurfaceHeight(instance.pqsCity.repositionRadial) - instance.body.pqsController.radius + 15;

                    var objectPos = instance.body.transform.InverseTransformPoint(instance.gameObject.transform.position);
                    lng = NavUtils.GetLongitude((Vector3d)objectPos) * KKMath.rad2deg;
                    lat = NavUtils.GetLatitude((Vector3d)objectPos) * KKMath.rad2deg;
                    stationID = RemoteTechAddon.AddGroundstation((instance.body.name) + " " + (string)instance.getSetting("Group"), lat, lng, alt, instance.body);
                    Log.Normal("Got RTStationID: " + stationID);
                }
                openRTStations.Add(instance, stationID);
                RemoteTechAddon.ChangeGroundStationRange(stationID, antennaPower);
            }
        }

        /// <summary>
        /// Removes all GroundStations from an Static Object.
        /// </summary>
        /// <param name="instance"></param>
        internal static void DetachGroundStation(StaticObject instance)
        {
            if (KerbalKonstructs.instance.enableCommNet)
            {
                if (openCNStations.Contains(instance))
                {
                    Log.Normal("Closing Antenna for " + (string)instance.getSetting("Group"));
                    CommNetHome commNetGroundStation = instance.gameObject.GetComponent<CommNetHome>();
                    //commNetGroundStation.enabled = false;
                    UnityEngine.Object.Destroy(commNetGroundStation);
                    openCNStations.Remove(instance);

                }
            }

            if (KerbalKonstructs.instance.enableRT)
            {
                RemoteTechAddon.RemoveGroundStation(openRTStations[instance]);
                openRTStations.Remove(instance);
            }
        }


    }
}

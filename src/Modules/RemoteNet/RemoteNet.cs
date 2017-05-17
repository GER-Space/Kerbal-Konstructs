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
        static List<StaticInstance> openCNStations = new List<StaticInstance>();
        static Dictionary<StaticInstance, Guid> openRTStations = new Dictionary<StaticInstance, Guid>();

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

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {

                if (instance.facilityType !=  KKFacilityType.GroundStation && instance.facilityType != KKFacilityType.TrackingStation)
                {
                    continue;
                }

                

                if (instance.myFacilities[0].OpenCloseState == "Closed" && instance.myFacilities[0].OpenCost != 0f)
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
        internal static void AttachGroundStation(StaticInstance instance)
        {
            GroundStation myfacility = instance.myFacilities[0] as GroundStation;
            // we use a messure of 1000km from the settings.
            float antennaPower = myfacility.TrackingShort * 1000000;
            if (antennaPower == 0f || instance.Group == "KSCUpgrades")
            {
                return;
            }

            // add CommNet Groundstations
            if (KerbalKonstructs.instance.enableCommNet)
            {

                Log.Normal("Adding Groundstation: " + instance.Group);
                if (openCNStations.Contains(instance) == false)
                {
                    KKCommNetHome commNetGroudStation = instance.gameObject.AddComponent<KKCommNetHome>();

                    commNetGroudStation.nodeName = instance.CelestialBody.name + " " + instance.Group;
                    commNetGroudStation.comm = new CommNode();
                    var commNode = commNetGroudStation.comm;
                    commNode.antennaTransmit.power = antennaPower;
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

                Guid stationID = RemoteTechAddon.GetGroundStationGuid((instance.CelestialBody.name) + " " + instance.Group);
                if (stationID == Guid.Empty)
                {
                    double lng, lat, alt;
                    alt = instance.CelestialBody.pqsController.GetSurfaceHeight(instance.pqsCity.repositionRadial) - instance.CelestialBody.pqsController.radius + 15;

                    var objectPos = instance.CelestialBody.transform.InverseTransformPoint(instance.gameObject.transform.position);
                    lng = KKMath.GetLongitudeInDeg(objectPos);
                    lat = KKMath.GetLatitudeInDeg(objectPos);
                    stationID = RemoteTechAddon.AddGroundstation((instance.CelestialBody.name) + " " + instance.Group, lat, lng, alt, instance.CelestialBody);
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
        internal static void DetachGroundStation(StaticInstance instance)
        {
            if (KerbalKonstructs.instance.enableCommNet)
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

            if (KerbalKonstructs.instance.enableRT)
            {
                RemoteTechAddon.RemoveGroundStation(openRTStations[instance]);
                openRTStations.Remove(instance);
            }
        }


    }

    internal class KKCommNetHome : CommNetHome
    {
        internal new CommNode comm;
    }
}

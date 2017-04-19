using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using UnityEngine;
using CommNet;

namespace KerbalKonstructs.Modules
{

    internal class RemoteNet
    {
        static List<StaticObject> openCNStations = new List<StaticObject>();
        static Dictionary<StaticObject,Guid> openRTStations = new Dictionary<StaticObject, Guid>();

        /// <summary>
        /// iterates through all potential open TrackingStations and attaches GroundStations
        /// </summary>
        internal static void LoadGroundStations()
        {
            // first we close any postential open Groundstations.

            CommNetHome[] homes = UnityEngine.Object.FindObjectsOfType<CommNetHome>();
            for (int i = 0; i < homes.Length; i++)
            {
                Log.Normal("Found ComNet: " + homes[i].nodeName +" " + homes[i].isKSC);
                if (homes[i].isKSC == true)
                    continue;
                UnityEngine.Object.Destroy(homes[i]);
            }


            foreach (StaticObject instance in KerbalKonstructs.instance.getStaticDB().getAllStatics())
            {

                if ((string)instance.getSetting("FacilityType") != "TrackingStation")
                {
                    continue;
                }

                if ((string)instance.getSetting("OpenCloseState") == "Closed" && (float)instance.getSetting("OpenCost") != 0f)
                {
                    continue;
                }

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
            float antennaPower = (float)instance.getSetting("TrackingShort") * 10;
            if (antennaPower == 0f || (string)instance.getSetting("Group") == "KSCUpgrades")
            {
                return;
            }

            // add CommNet Groundstations
            if (KerbalKonstructs.instance.enableCommNet)
            {
                Log.Normal("Adding Groundstation: " + (string)instance.getSetting("Group") );
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
                }
            }
            // Add RemoteTech Groundstation
            if (KerbalKonstructs.instance.enableRT)
            {

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

            }
        }


    }
}

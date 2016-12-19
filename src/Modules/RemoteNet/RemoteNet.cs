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
        internal static void LoadAntennas()
        {
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
                    AttachAntennas(instance);
                }
            }
        }


        internal static void AttachAntennas(StaticObject instance)
        {
            float antennaPower = (float)instance.getSetting("TrackingShort") * 1000;
            if (antennaPower == 0f || (string)instance.getSetting("Group") == "KSCUpgrades" )
            {
                return;
            }

            Log.Normal("Adding Antenna: " + (antennaPower/1000).ToString() + "km");
            if (KerbalKonstructs.instance.enableCommNet)
            {
                if (instance.gameObject.GetComponent<CommNetNode>() == null)
                {
                    var commNetAntenna = instance.gameObject.AddComponent<CommNetHome>();
               //     CommNode node = new CommNode();

               //     node.antennaTransmit = new CommNode.AntennaInfo();
               //     node.antennaTransmit.power = (double)antennaPower;

               //     node.isHome = true;

                //    commNetAntenna.Comm = node;
                    commNetAntenna.enabled = true;

                } else
                {
                    var commNetAntenna = instance.gameObject.GetComponent<CommNetNode>();
                    commNetAntenna.enabled = true;
                }
            } 

            if (KerbalKonstructs.instance.enableRT)
            {

            }
        }


        internal static void CloseAntenna(StaticObject instance)
        {
            if (KerbalKonstructs.instance.enableCommNet)
            {
                Log.Normal("Closing Antenna");
                var commNetAntenna = instance.gameObject.GetComponent<CommNetNode>();
                commNetAntenna.enabled = false;

            }

            if (KerbalKonstructs.instance.enableRT)
            {

            }
        }




    }
}

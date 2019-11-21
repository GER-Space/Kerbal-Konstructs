using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Hangar2 : KKFacility
    {

        [CFGSetting]
        public float FacilityMassCapacity;
        [CFGSetting]
        public int FacilityCraftCapacity;

        internal List<Vessel> storedVessels = new List<Vessel>();

        internal Dictionary<Guid, OrbitDriver> storedOrbitDrivers = new Dictionary<Guid, OrbitDriver>();


        internal override void LoadCareerConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);



            foreach (ConfigNode resourceNode in cfgNode.GetNodes("StoredVessel"))
            {
                if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.flightState == null)
                {
                    return;
                }

                Guid vesselID = Guid.Parse(resourceNode.GetValue("VesselID"));

                foreach (ProtoVessel protoVessel  in HighLogic.CurrentGame.flightState.protoVessels)
                {
                    if (protoVessel.vesselID == vesselID)
                    {
                        storedVessels.Add(protoVessel.vesselRef);
                        protoVessel.vesselRef.MakeInactive();
                        if (protoVessel.vesselRef.loaded)
                        {
                            protoVessel.vesselRef.Unload();
                        }
                        // save the OrbitDriver for later, so we don't have to create a new one.
                        if (!storedOrbitDrivers.ContainsKey(protoVessel.vesselID))
                        {
                            storedOrbitDrivers.Add(protoVessel.vesselID, protoVessel.vesselRef.orbitDriver);
                        }
                        protoVessel.vesselRef.orbitDriver = null;
                    }
                }
            }
        }

        internal override void SaveCareerConfig(ConfigNode cfgNode)
        {
            ConfigNode resourceNode = null;

            base.WriteConfig(cfgNode);

            foreach (Vessel vessel in storedVessels)
            {
                resourceNode = new ConfigNode("StoredVessel");
                resourceNode.SetValue("VesselID", vessel.protoVessel.vesselID.ToString(), true);
                cfgNode.AddNode(resourceNode);
            }

        }


    }
}

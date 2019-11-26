using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class Hangar : KKFacility
    {

        [CFGSetting]
        public float FacilityMassCapacity;
        [CFGSetting]
        public int FacilityCraftCapacity;


        internal struct StoredVessel
        {
            internal string vesselName;
            internal Guid uuid;
            internal ConfigNode vesselNode;
        }


        internal List<StoredVessel> storedVessels = new List<StoredVessel>();


        internal override void LoadCareerConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);

            storedVessels = new List<StoredVessel>();

            foreach (ConfigNode resourceNode in cfgNode.GetNodes("StoredVessel"))
            {

                StoredVessel storedVessel = new StoredVessel
                {
                    uuid = Guid.Parse(resourceNode.GetValue("VesselID")),
                    vesselName = resourceNode.GetValue("VesselName"),
                    vesselNode = resourceNode.GetNode("VESSEL")
                };
                storedVessels.Add(storedVessel);
            }
            
        }

        internal override void SaveCareerConfig(ConfigNode facilityNode)
        {
            base.WriteConfig(facilityNode);

            foreach (StoredVessel vessel in storedVessels)
            {
                ConfigNode vesselNode = new ConfigNode("StoredVessel");
                vesselNode.SetValue("VesselID", vessel.uuid.ToString(), true);
                vesselNode.SetValue("VesselName", vessel.vesselName, true);
                vesselNode.AddNode(vessel.vesselNode);

                facilityNode.AddNode(vesselNode);
            }

        }


        // static functions, that make live a lot easier

        /// <summary>
        /// Stores a Vessel into the specified Hangar
        /// </summary>
        internal static void StoreVessel (Vessel vessel, Hangar hangar) 
        {

            StoredVessel storedVessel = new StoredVessel
            {
                uuid = vessel.protoVessel.vesselID,
                vesselName = vessel.GetDisplayName()
            };

            //get the experience and assign the crew to the rooster
            foreach (Part part in vessel.parts)
            {
                int count = part.protoModuleCrew.Count;
                for  (int i = 0; i <count; i++ )
                {
                    part.protoModuleCrew[i].flightLog.AddEntryUnique(FlightLog.EntryType.Recover);
                    part.protoModuleCrew[i].flightLog.AddEntryUnique(FlightLog.EntryType.Land, FlightGlobals.currentMainBody.name);
                    part.protoModuleCrew[i].ArchiveFlightLog();

                    // remove the crew from the ship
                    part.RemoveCrewmember(part.protoModuleCrew[i]);
                }
            }


            // save the ship
            storedVessel.vesselNode = new ConfigNode("VESSEL");

            //create a backup of the current state, then save that state
            ProtoVessel backup = vessel.BackupVessel();
            backup.Save(storedVessel.vesselNode);

            // save the stored information in the hangar
            hangar.storedVessels.Add(storedVessel);

            // remove the stored vessel from the game
            vessel.MakeInactive();
            vessel.Unload();
            //vessel.Die();

            FlightGlobals.RemoveVessel(vessel);
            if (vessel != null)
            {
                vessel.protoVessel.Clean();
            }

            //UnityEngine.Object.Destroy(vessel.gameObject);

            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            HighLogic.LoadScene(GameScenes.SPACECENTER);
        }



        internal static Vessel RollOutVessel(StoredVessel storedVessel, Hangar hangar)
        {
            if (!hangar.storedVessels.Contains(storedVessel))
            {
                Log.Error("no Stored Vessel found:" + storedVessel.vesselName);
                return null;
            }
            hangar.storedVessels.Remove(storedVessel);

            ProtoVessel protoVessel = new ProtoVessel(storedVessel.vesselNode, HighLogic.CurrentGame);


            protoVessel.Load(HighLogic.CurrentGame.flightState);

            Vessel vessel = protoVessel.vesselRef;

            return vessel;

        }



    }
}

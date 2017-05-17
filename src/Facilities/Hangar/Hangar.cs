using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Hangar : KKFacility
    {
        [CareerSetting]
        public string InStorage1 = "";
        [CareerSetting]
        public string InStorage2 = "";
        [CareerSetting]
        public string InStorage3 = "";


        [CFGSetting]
        public float FacilityMassCapacity;
        [CFGSetting]
        public int FacilityCraftCapacity;

        internal static void DoHangaredCraftCheck()
        {
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {


                if (instance.facilityType == KKFacilityType.Hangar)
                {
                    Hangar thisHanar = instance.gameObject.GetComponent<Hangar>();


                    string sInStorage = thisHanar.InStorage1;
                    string sInStorage2 = thisHanar.InStorage2;
                    string sInStorage3 = thisHanar.InStorage3;

                    string bHangarHasStoredCraft1 = "None";
                    string bHangarHasStoredCraft2 = "None";
                    string bHangarHasStoredCraft3 = "None";

                    bool bCraftExists = false;

                    if (sInStorage != "None" && sInStorage != "")
                    {
                        foreach (Vessel vVesselStored in FlightGlobals.Vessels)
                        {
                            if (vVesselStored.id.ToString() == sInStorage)
                            {
                                bCraftExists = true;
                                break;
                            }
                        }

                        if (bCraftExists)
                            bHangarHasStoredCraft1 = "InStorage1";
                        else
                        {
                            // Craft no longer exists. Clear this hangar space.
                            Log.Debug("Craft InStorage no longer exists. Emptying this hangar space.");
                            thisHanar.InStorage1 = "None";
                        }
                    }

                    bCraftExists = false;

                    if (sInStorage2 != "None" && sInStorage2 != "")
                    {
                        foreach (Vessel vVesselStored in FlightGlobals.Vessels)
                        {
                            if (vVesselStored.id.ToString() == sInStorage2)
                            {
                                bCraftExists = true;
                                break;
                            }
                        }

                        if (bCraftExists)
                            bHangarHasStoredCraft2 = "InStorage2";
                        else
                        {
                            // Craft no longer exists. Clear this hangar space.
                            Log.Debug("Craft TargetID no longer exists. Emptying this hangar space.");
                            thisHanar.InStorage2 = "None";
                        }
                    }

                    bCraftExists = false;

                    if (sInStorage3 != "None" && sInStorage3 != "")
                    {
                        foreach (Vessel vVesselStored in FlightGlobals.Vessels)
                        {
                            if (vVesselStored.id.ToString() == sInStorage3)
                            {
                                bCraftExists = true;
                                break;
                            }
                        }

                        if (bCraftExists)
                            bHangarHasStoredCraft3 = "InStorage3";
                        else
                        {
                            // Craft no longer exists. Clear this hangar space.
                            Log.Debug("Craft TargetType no longer exists. Emptying this hangar space.");

                            thisHanar.InStorage3 = "None";
                        }
                    }

                    if (bHangarHasStoredCraft1 == "None" && bHangarHasStoredCraft2 == "None" && bHangarHasStoredCraft3 == "None")
                    {

                    }
                    else
                    {
                        string sHangarSpace = "";

                        foreach (Vessel vVesselStored in FlightGlobals.Vessels)
                        {
                            if (vVesselStored.id.ToString() == sInStorage)
                                sHangarSpace = "InStorage1";

                            if (vVesselStored.id.ToString() == sInStorage2)
                                sHangarSpace = "InStorage2";

                            if (vVesselStored.id.ToString() == sInStorage3)
                                sHangarSpace = "InStorage3";

                            // If a vessel is hangared
                            if (vVesselStored.id.ToString() == sInStorage || vVesselStored.id.ToString() == sInStorage2 || vVesselStored.id.ToString() == sInStorage3)
                            {
                                if (vVesselStored == FlightGlobals.ActiveVessel)
                                {
                                    // Craft has been taken control
                                    // Empty the hangar
                                    Log.Debug("Craft has been been taken control of. Emptying " + sHangarSpace + " hangar space.");
                                    typeof(Hangar).GetField(sHangarSpace).SetValue(thisHanar, "None");
                                    //instance.setSetting(sHangarSpace, "None");
                                }
                                else
                                {
                                    Log.Debug("Hiding vessel " + vVesselStored.vesselName + ". It is in the hangar.");
                                    // Hide the vessel - it is in the hangar

                                    foreach (Part p in vVesselStored.Parts)
                                    {
                                        if (p != null && p.gameObject != null)
                                            p.gameObject.SetActive(false);
                                        else
                                            continue;
                                    }

                                    vVesselStored.MakeInactive();
                                    vVesselStored.enabled = false;
                                    vVesselStored.Unload();
                                }
                            }

                        }
                    }
                }
            }
        }

    }
}

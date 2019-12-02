using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class HangarGUI
    {
        public static Vector2 scrollNearbyCraft;


        public static void HangarInterface(StaticInstance selectedFacility)
        {
            Hangar myHangar = selectedFacility.myFacilities[0] as Hangar;


            float fMaxMass = myHangar.FacilityMassCapacity;
            if (fMaxMass < 1)
            {
                fMaxMass = 25f;
            }

            int fMaxCrafts = myHangar.FacilityCraftCapacity;

            GUILayout.Space(2);
            GUILayout.Label("Where necessary craft are disassembled for storage or re-assembled before being rolled out. Please note that for game purposes, this procedure is instantaneous.", UIMain.LabelInfo);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Craft: " + fMaxCrafts.ToString("#0"), UIMain.LabelInfo);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Max Mass/Craft: " + fMaxMass.ToString("#0") + " T", UIMain.LabelInfo);
            GUILayout.EndHorizontal();


            if (myHangar.storedVessels.Count == 0)
            {
                GUILayout.Label("");
                GUILayout.Label("No craft currently held in this facility.", UIMain.LabelInfo);
                GUILayout.Label("");
            }
            else
            {
                int iNumberCrafts = myHangar.storedVessels.Count;

                GUILayout.Box("Stored Craft (" + iNumberCrafts.ToString() + "/" + fMaxCrafts.ToString() + ")", UI.UIMain.Yellowtext);

                foreach (Hangar.StoredVessel vessel in myHangar.storedVessels)
                {
                    if (GUILayout.Button("" + vessel.vesselName, UIMain.ButtonKK, GUILayout.Height(20)))
                    {
                        // Empty the hangar
                        if (HangarwayIsClear(selectedFacility))
                        {
                            //Vessel newVessel = Hangar.RollOutVessel(vessel, myHangar);
                            //newVessel.Load();
                            //newVessel.MakeActive() ;
                            UI2.HangarKSCGUI.useFromFlight = true;
                            UI2.HangarKSCGUI.selectedFacility = selectedFacility;
                            UI2.HangarKSCGUI.Open();
                        }
                        else
                        {
                            MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10, 3);
                        }
                    }

                }
            }

            GUILayout.Space(5);


            GUILayout.Box("Store Vessel", UIMain.Yellowtext);

            bool bNearbyCraft = false;

            float vDistToCraft = Vector3.Distance(FlightGlobals.ActiveVessel.gameObject.transform.position, selectedFacility.position);
            if (vDistToCraft < KerbalKonstructs.instance.facilityUseRange)
            {
                bNearbyCraft = true;
            }

            GUI.enabled = bNearbyCraft;

            float fMass = FlightGlobals.ActiveVessel.GetTotalMass();
            if (fMass > fMaxMass)
            {
                GUILayout.Label("Vessel too heavy: " + Math.Round(fMass,1).ToString() +"/"+ fMaxMass);
                GUI.enabled = false;
            }
            if (myHangar.storedVessels.Count >= myHangar.FacilityCraftCapacity)
            {
                GUILayout.Label("Hangar is Full!", UIMain.LabelRed);
                GUI.enabled = false;
            }

            if (GUILayout.Button("Store Vessel" + FlightGlobals.ActiveVessel.vesselName , UIMain.ButtonKK, GUILayout.Height(23)))
            {
                Hangar.StoreVessel(FlightGlobals.ActiveVessel, myHangar);

            }
            GUI.enabled = true;
            if (!bNearbyCraft)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("You are ", UIMain.LabelInfo);
                    GUILayout.Label(((int)vDistToCraft - KerbalKonstructs.instance.facilityUseRange).ToString() , UIMain.LabelRed);

                    GUILayout.Label("m", UIMain.LabelWhite);
                    GUILayout.Label(" too far away to store your craft!", UIMain.LabelInfo);
                }
                GUILayout.EndHorizontal();
                
            }


            GUILayout.FlexibleSpace();
        }

        public static Boolean HangarwayIsClear(StaticInstance instance)
        {
            Boolean bIsClear = true;

            foreach (Vessel vVessel in FlightGlobals.Vessels)
            {
                if (vVessel == null) continue;
                if (!vVessel.loaded) continue;
                if (vVessel.vesselType == VesselType.EVA) continue;
                if (vVessel.vesselType == VesselType.Flag) continue;
                if (vVessel.situation != Vessel.Situations.LANDED) continue;

                var vDistToCraft = Vector3.Distance(vVessel.gameObject.transform.position, instance.position);
                if (vDistToCraft > 260)
                {
                    continue;
                }
                else
                {
                    bIsClear = false;
                }
            }

            return bIsClear;
        }

      
    }
}

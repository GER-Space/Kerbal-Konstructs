using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using System;
using System.Collections.Generic;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class HangarGUI
	{
		public static GUIStyle Yellowtext;
		public static GUIStyle KKWindow;
		public static GUIStyle DeadButton;
		public static GUIStyle DeadButtonRed;
		public static GUIStyle BoxNoBorder;
		public static GUIStyle LabelInfo;
		public static GUIStyle ButtonSmallText;

		public static Vector2 scrollNearbyCraft;

		public static string sInStorage = "None";
		public static string sInStorage2 = "None";
		public static string sInStorage3 = "None";

		public static void HangarInterface(StaticInstance selectedFacility)
		{
            Hangar myHangar = selectedFacility.myFacilities[0] as Hangar;

			DeadButton = new GUIStyle(GUI.skin.button);
			DeadButton.normal.background = null;
			DeadButton.hover.background = null;
			DeadButton.active.background = null;
			DeadButton.focused.background = null;
			DeadButton.normal.textColor = Color.white;
			DeadButton.hover.textColor = Color.white;
			DeadButton.active.textColor = Color.white;
			DeadButton.focused.textColor = Color.white;
			DeadButton.fontSize = 14;
			DeadButton.fontStyle = FontStyle.Bold;

			DeadButtonRed = new GUIStyle(GUI.skin.button);
			DeadButtonRed.normal.background = null;
			DeadButtonRed.hover.background = null;
			DeadButtonRed.active.background = null;
			DeadButtonRed.focused.background = null;
			DeadButtonRed.normal.textColor = Color.red;
			DeadButtonRed.hover.textColor = Color.yellow;
			DeadButtonRed.active.textColor = Color.red;
			DeadButtonRed.focused.textColor = Color.red;
			DeadButtonRed.fontSize = 12;
			DeadButtonRed.fontStyle = FontStyle.Bold;

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

			Yellowtext = new GUIStyle(GUI.skin.box);
			Yellowtext.normal.textColor = Color.yellow;
			Yellowtext.normal.background = null;

			LabelInfo = new GUIStyle(GUI.skin.label);
			LabelInfo.normal.background = null;
			LabelInfo.normal.textColor = Color.white;
			LabelInfo.fontSize = 13;
			LabelInfo.fontStyle = FontStyle.Bold;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

			ButtonSmallText = new GUIStyle(GUI.skin.button);
			ButtonSmallText.fontSize = 12;
			ButtonSmallText.fontStyle = FontStyle.Normal;

			sInStorage = myHangar.InStorage1;
			sInStorage2 = myHangar.InStorage2;
			sInStorage3 = myHangar.InStorage3;

			float fMaxMass = (float)selectedFacility.model.DefaultFacilityMassCapacity;
			if (fMaxMass < 1) fMaxMass = 25f;
			int fMaxCrafts = selectedFacility.model.DefaultFacilityCraftCapacity;
			if (fMaxCrafts < 1 || fMaxCrafts > 3) fMaxCrafts = 2;

			GUILayout.Space(2);
			GUILayout.Label("Where necessary craft are disassembled for storage or re-assembled before being rolled out. Please note that for game purposes, this procedure is instantaneous.", LabelInfo);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Max Craft: " + fMaxCrafts.ToString("#0"), LabelInfo);
			GUILayout.FlexibleSpace();
			GUILayout.Label("Max Mass/Craft: " + fMaxMass.ToString("#0") + " T", LabelInfo);
			GUILayout.EndHorizontal();
 
			if (sInStorage == null || sInStorage == "")
			{
				sInStorage = "None";
                myHangar.InStorage1 =  "None";
			}
			if (sInStorage2 == null || sInStorage2 == "")
			{
				sInStorage2 = "None";
                myHangar.InStorage2 =  "None";
			}
			if (sInStorage3 == null || sInStorage3 == "")
			{
				sInStorage3 = "None";
                myHangar.InStorage3 =  "None";
			}

			if (sInStorage == "None" && sInStorage2 == "None" && sInStorage3 == "None")
				GUILayout.Label("No craft currently held in this facility.", LabelInfo);
			else
			{
				int iNumberCrafts = NumberCraftHangared(selectedFacility);

				GUILayout.Box("Stored Craft (" + iNumberCrafts.ToString() + "/" + fMaxCrafts.ToString("#0") + ")", Yellowtext);

				List<Vessel> lVessels = FlightGlobals.Vessels;
						
				foreach (Vessel vVesselStored in lVessels)
				{
					if (vVesselStored.id.ToString() == sInStorage)
					{
						if (GUILayout.Button("" + vVesselStored.vesselName, ButtonSmallText, GUILayout.Height(20)))
						{
							// Empty the hangar
							if (HangarwayIsClear(selectedFacility))
							{
								sInStorage = "None";
								UnhangarCraft(vVesselStored, selectedFacility);
								sInStorage = "None";
							}
							else
							{
								MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10,
									3);
							}
						}
						break;
					}
				}

				foreach (Vessel vVesselStored in lVessels)
				{
					if (vVesselStored.id.ToString() == sInStorage2)
					{
						if (GUILayout.Button("" + vVesselStored.vesselName, ButtonSmallText, GUILayout.Height(20)))
						{
							// Empty the hangar
							if (HangarwayIsClear(selectedFacility))
							{
								sInStorage2 = "None";
								UnhangarCraft(vVesselStored, selectedFacility);
								sInStorage2 = "None";
							}
							else
							{
								MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10,
									3);
							}
						}
						break;
					}
				}
						
				foreach (Vessel vVesselStored in lVessels)
				{
					if (vVesselStored.id.ToString() == sInStorage3)
					{
						if (GUILayout.Button("" + vVesselStored.vesselName, ButtonSmallText, GUILayout.Height(20)))
						{
							// Empty the hangar
							if (HangarwayIsClear(selectedFacility))
							{
								sInStorage3 = "None";
								UnhangarCraft(vVesselStored, selectedFacility);
								sInStorage3 = "None";
							}
							else
							{
								MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10,
									3);
							}
						}
						break;
					}
				}
			}

			GUILayout.Space(5);

			scrollNearbyCraft = GUILayout.BeginScrollView(scrollNearbyCraft);

			GUILayout.Box("Nearby Craft", Yellowtext);

			bool bNearbyCraft = false;

			foreach (Vessel vVessel in FlightGlobals.Vessels)
			{
				if (vVessel == null) continue;
				if (!vVessel.loaded) continue;
				if (vVessel.vesselType == VesselType.SpaceObject) continue;
				if (vVessel.vesselType == VesselType.Debris) continue;
				if (vVessel.vesselType == VesselType.EVA) continue;
				if (vVessel.vesselType == VesselType.Flag) continue;
				if (vVessel.vesselType == VesselType.Unknown) continue;
				if (vVessel == FlightGlobals.ActiveVessel) continue;
				if (vVessel.situation != Vessel.Situations.LANDED) continue;
				if (vVessel.GetCrewCount() > 0) continue;

				var vDistToCraft = Vector3.Distance(vVessel.gameObject.transform.position, selectedFacility.gameObject.transform.position);
				if (vDistToCraft > 250) continue;

				bNearbyCraft = true;

				if (GUILayout.Button(" " + vVessel.vesselName + " ", ButtonSmallText, GUILayout.Height(20)))
				{
					float fMass = vVessel.GetTotalMass();

					if (fMass > fMaxMass)
					{
						MiscUtils.HUDMessage("Craft too heavy for this facility. Max " + fMaxMass.ToString("#0") + "T per craft.", 10,
							3);
					}
					else
					{
						float fMaxCraft = (float)selectedFacility.model.DefaultFacilityCraftCapacity;
						if (fMaxCraft < 1 || fMaxCraft > 3) fMaxCraft = 2;

						int iNumberCraft = NumberCraftHangared(selectedFacility);

						if (iNumberCraft < (int)fMaxCraft)
							HangarCraft(vVessel, selectedFacility, (int)fMaxCraft);
						else
							MiscUtils.HUDMessage("This facility is full. Max craft: " + fMaxCraft.ToString("#0"), 10,
								3);
					}
				}
			}

			if (!bNearbyCraft)
				GUILayout.Label("There are no craft close enough to store in this facility.", LabelInfo);

			GUILayout.EndScrollView();

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

				var vDistToCraft = Vector3.Distance(vVessel.gameObject.transform.position, instance.gameObject.transform.position);
				if (vDistToCraft > 260) continue;
				else
					bIsClear = false;
			}

			return bIsClear;
		}

		public static void CacheHangaredCraft(StaticInstance instance)
		{
            Hangar myHangar = instance.myFacilities[0] as Hangar;
			string sInStorage = myHangar.InStorage1;
			string sInStorage2 = myHangar.InStorage2;
			string sInStorage3 = myHangar.InStorage3;

			foreach (Vessel vVesselStored in FlightGlobals.Vessels)
			{
				if (vVesselStored == null) continue;
				if (!vVesselStored.loaded) continue;
				if (vVesselStored.vesselType == VesselType.SpaceObject) continue;
				if (vVesselStored.vesselType == VesselType.Debris) continue;
				if (vVesselStored.vesselType == VesselType.EVA) continue;
				if (vVesselStored.vesselType == VesselType.Flag) continue;
				if (vVesselStored.vesselType == VesselType.Unknown) continue;

				string sHangarSpace = "None";
				// If a vessel is hangared
				if (vVesselStored.id.ToString() == sInStorage)
					sHangarSpace = "InStorage1";
				if (vVesselStored.id.ToString() == sInStorage2)
					sHangarSpace = "InStorage2";
				if (vVesselStored.id.ToString() == sInStorage3)
					sHangarSpace = "InStorage3";

				if (sHangarSpace != "None")
				{
					if (vVesselStored == FlightGlobals.ActiveVessel)
					{
                        // Craft has been taken control
                        // Empty the hangar
                        typeof(Hangar).GetField(sHangarSpace).SetValue(myHangar, "None");
                    }
					else
					{
						// Hide the vessel - it is in the hangar
						if (vVesselStored != null)
						{
							foreach (Part p in vVesselStored.Parts)
							{
								if (p != null && p.gameObject != null)
									p.gameObject.SetActive(false);
								else
									continue;
							}

							vVesselStored.MakeInactive();
							vVesselStored.enabled = false;

							if (vVesselStored.loaded)
								vVesselStored.Unload();
						}
					}
				}
			}
		}

		public static void HangarCraft(Vessel vVessel, StaticInstance soHangar, int iMax = 2)
		{
			string sSpace = GetHangarSpace(soHangar, iMax);
            Hangar myHangar = soHangar.myFacilities[0] as Hangar;


            if (sSpace == "None")
			{
				MiscUtils.HUDMessage("This facility is full.", 10,
					3);
			}
			else
			{
				string sVesselID = vVessel.id.ToString();
                typeof(Hangar).GetField(sSpace).SetValue(myHangar, sVesselID);

				// Hangar the vessel - hide it
				foreach (Part p in vVessel.Parts)
				{
					if (p != null && p.gameObject != null)
						p.gameObject.SetActive(false);
					else
						continue;
				}

				vVessel.MakeInactive();
				vVessel.enabled = false;
				vVessel.Unload();
			}
		}

		public static string GetHangarSpace(StaticInstance instacne, int iMax = 2)
		{
            Hangar myHangar = instacne.myFacilities[0] as Hangar;

            string sSpace = "None";

			if (myHangar.InStorage1 == "None")
			{
				sSpace = "InStorage1";
				if (iMax < 2) return sSpace;
			}
			else
				if (myHangar.InStorage2 == "None")
				{
					sSpace = "InStorage2";
					if (iMax == 2) return sSpace;
				}
				else
					if (myHangar.InStorage3 == "None")
						sSpace = "InStorage3";

			return sSpace;
		}

		public static int NumberCraftHangared(StaticInstance instance)
		{
            Hangar myHangar = instance.myFacilities[0] as Hangar;
			int iNumber = 0;

			if (myHangar.InStorage1 != "None") iNumber = iNumber + 1;
			if (myHangar.InStorage2 != "None") iNumber = iNumber + 1;
			if (myHangar.InStorage3 != "None") iNumber = iNumber + 1;

			return iNumber;
		}

		public static void RemoveCorrectCraft(Vessel vVessel, StaticInstance instance)
		{
            Hangar myHangar = instance.myFacilities[0] as Hangar;
            string sSpace = "InStorage1";
			string sVesselID = vVessel.id.ToString();

			if (sVesselID == myHangar.InStorage2)
				sSpace = "InStorage2";
			if (sVesselID == myHangar.InStorage3)
				sSpace = "InStorage3";

            typeof(Hangar).GetField(sSpace).SetValue(myHangar, "None");
		}

		public static void UnhangarCraft(Vessel vVesselStored, StaticInstance instance)
		{
			RemoveCorrectCraft(vVesselStored, instance);

			// Convert the stored protovessel to a new protovessel.
			// Use BackupVessel because that seems to work and protovessel does not. `\o/`
			ProtoVessel pVessel = vVesselStored.BackupVessel();

			// Get rid of the original hidden vessel - even though it was unloaded KSP still 'sees' the original craft.
			// I do not care why. :|
			vVesselStored.state = Vessel.State.DEAD;

			foreach (Part p in vVesselStored.Parts)
			{
				if (p != null && p.gameObject != null)
					p.gameObject.DestroyGameObject();
			}

			// Load the new protovessel we made. KSP won't reload (or rather render) a vessel that was unloaded - it will only load a protovessel. :$
			pVessel.Load(FlightDriver.FlightStateCache.flightState);
			// I suspect this is actually a KSP bug since the same crap happens with newly spawned static objects. If you query the active state
			// of the invisible craft, it claims it is active. And no, forcing the renderers doesn't work either.

			// Convert protovessel to vessel
			Vessel vNewVessel = pVessel.vesselRef;

			// Unload then reload the vessel - this seems to be the way to properly re-initialise flightstate etc.
			// Don't do this and you get a craft with a stuck surface velocity reading. It looks like KSP transposes orbital
			// and surface velocity or some other stupid s**t. I don't care.
			// And yes, this time KSP does load an unloaded vessel with no need for protovessel b******t. I don't care why.
			vNewVessel.Unload();
			vNewVessel.Load();
		}
	}
}

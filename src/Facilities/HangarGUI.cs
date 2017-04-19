using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
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

		public static void HangarInterface(StaticObject selectedFacility)
		{
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

			sInStorage = (string)selectedFacility.getSetting("InStorage");
			sInStorage2 = (string)selectedFacility.getSetting("TargetID");
			sInStorage3 = (string)selectedFacility.getSetting("TargetType");

			float fMaxMass = (float)selectedFacility.model.getSetting("DefaultFacilityMassCapacity");
			if (fMaxMass < 1) fMaxMass = 25f;
			float fMaxCrafts = (float)selectedFacility.model.getSetting("DefaultFacilityCraftCapacity");
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
				selectedFacility.setSetting("InStorage", "None");
			}
			if (sInStorage2 == null || sInStorage2 == "")
			{
				sInStorage2 = "None";
				selectedFacility.setSetting("TargetID", "None");
			}
			if (sInStorage3 == null || sInStorage3 == "")
			{
				sInStorage3 = "None";
				selectedFacility.setSetting("TargetType", "None");
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
						float fMaxCraft = (float)selectedFacility.model.getSetting("DefaultFacilityCraftCapacity");
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

		public static Boolean HangarwayIsClear(StaticObject soHangar)
		{
			Boolean bIsClear = true;

			foreach (Vessel vVessel in FlightGlobals.Vessels)
			{
				if (vVessel == null) continue;
				if (!vVessel.loaded) continue;
				if (vVessel.vesselType == VesselType.EVA) continue;
				if (vVessel.vesselType == VesselType.Flag) continue;
				if (vVessel.situation != Vessel.Situations.LANDED) continue;

				var vDistToCraft = Vector3.Distance(vVessel.gameObject.transform.position, soHangar.gameObject.transform.position);
				if (vDistToCraft > 260) continue;
				else
					bIsClear = false;
			}

			return bIsClear;
		}

		public static void CacheHangaredCraft(StaticObject obj)
		{
			string sInStorage = (string)obj.getSetting("InStorage");
			string sInStorage2 = (string)obj.getSetting("TargetID");
			string sInStorage3 = (string)obj.getSetting("TargetType");

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
					sHangarSpace = "InStorage";
				if (vVesselStored.id.ToString() == sInStorage2)
					sHangarSpace = "TargetID";
				if (vVesselStored.id.ToString() == sInStorage3)
					sHangarSpace = "TargetType";

				if (sHangarSpace != "None")
				{
					if (vVesselStored == FlightGlobals.ActiveVessel)
					{
						// Craft has been taken control
						// Empty the hangar
						obj.setSetting(sHangarSpace, "None");
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

		public static void HangarCraft(Vessel vVessel, StaticObject soHangar, int iMax = 2)
		{
			string sSpace = GetHangarSpace(soHangar, iMax);

			if (sSpace == "None")
			{
				MiscUtils.HUDMessage("This facility is full.", 10,
					3);
			}
			else
			{
				string sVesselID = vVessel.id.ToString();
				soHangar.setSetting(sSpace, sVesselID);

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

		public static string GetHangarSpace(StaticObject soHangar, int iMax = 2)
		{
			string sSpace = "None";

			if ((string)soHangar.getSetting("InStorage") == "None")
			{
				sSpace = "InStorage";
				if (iMax < 2) return sSpace;
			}
			else
				if ((string)soHangar.getSetting("TargetID") == "None")
				{
					sSpace = "TargetID";
					if (iMax == 2) return sSpace;
				}
				else
					if ((string)soHangar.getSetting("TargetType") == "None")
						sSpace = "TargetType";

			return sSpace;
		}

		public static int NumberCraftHangared(StaticObject soHangar)
		{
			int iNumber = 0;

			if ((string)soHangar.getSetting("InStorage") != "None") iNumber = iNumber + 1;
			if ((string)soHangar.getSetting("TargetID") != "None") iNumber = iNumber + 1;
			if ((string)soHangar.getSetting("TargetType") != "None") iNumber = iNumber + 1;

			return iNumber;
		}

		public static void RemoveCorrectCraft(Vessel vVessel, StaticObject soHangar)
		{
			string sSpace = "InStorage";
			string sVesselID = vVessel.id.ToString();

			if (sVesselID == (string)soHangar.getSetting("TargetID"))
				sSpace = "TargetID";
			if (sVesselID == (string)soHangar.getSetting("TargetType"))
				sSpace = "TargetType";

			soHangar.setSetting(sSpace, "None");
		}

		public static void UnhangarCraft(Vessel vVesselStored, StaticObject soHangar)
		{
			RemoveCorrectCraft(vVesselStored, soHangar);

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

using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.Utilities
{
	class PersistenceUtils
	{
		public static void saveCommsPersistence(Vessel vVessel, Part pPart, string sTargetType, 
			string sTargetKey, bool isRelay, string sRelayTarget = "None", string sStationKey = "None")
		{
			Boolean bFoundComms = false;

			string sPartID = pPart.flightID.ToString();
			string sVesselID = vVessel.id.ToString();

			string saveConfigPath = string.Format("{0}saves/{1}/KKComms.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			ConfigNode rootNode = new ConfigNode();

			if (!File.Exists(saveConfigPath))
			{
				ConfigNode GameNode = rootNode.AddNode("GAME");
				ConfigNode ScenarioNode = GameNode.AddNode("SCENARIO");
				ScenarioNode.AddValue("Name", "KKComms");
				rootNode.Save(saveConfigPath);
			}

			rootNode = ConfigNode.Load(saveConfigPath);

			ConfigNode rootrootNode = rootNode.GetNode("GAME");
			ConfigNode cnHolder = new ConfigNode();

			foreach (ConfigNode ins in rootrootNode.GetNodes("SCENARIO"))
			{
				cnHolder = ins;
				foreach (ConfigNode insins in ins.GetNodes("KKComm"))
				{
					string sCommsID = insins.GetValue("PartID");
					if (sCommsID == null)
					{
						Debug.Log("KK: Got a comm but it has no key! WTF?????");
						continue;
					}

					if (sCommsID == sPartID)
					{
						if (insins.HasValue("VesselID"))
							insins.RemoveValue("VesselID");
						if (insins.HasValue("TargetType"))
							insins.RemoveValue("TargetType");
						if (insins.HasValue("TargetID"))
							insins.RemoveValue("TargetID");
						if (insins.HasValue("isRelay"))
							insins.RemoveValue("isRelay");
						if (insins.HasValue("RelayTarget"))
							insins.RemoveValue("RelayTarget");
						if (insins.HasValue("StationKey"))
							insins.RemoveValue("StationKey");

						insins.AddValue("VesselID", sVesselID);
						insins.AddValue("TargetType", sTargetType);
						insins.AddValue("TargetID", sTargetKey);
						insins.AddValue("RelayTarget", sRelayTarget);
						insins.AddValue("StationKey", sStationKey);

						if (isRelay)
							insins.AddValue("isRelay", "Yes");
						else
							insins.AddValue("isRelay", "No");

						bFoundComms = true;
						break;
					}
				}
			}

			if (!bFoundComms)
			{
				ConfigNode newComm = new ConfigNode("KKComm");
				newComm.AddValue("PartID", sPartID);
				newComm.AddValue("VesselID", sVesselID);
				newComm.AddValue("TargetType", sTargetType);
				newComm.AddValue("TargetID", sTargetKey);
				newComm.AddValue("RelayTarget", sRelayTarget);
				newComm.AddValue("StationKey", sStationKey);

				if (isRelay)
					newComm.AddValue("isRelay", "Yes");
				else
					newComm.AddValue("isRelay", "No");

				cnHolder.AddNode(newComm);
			}

			rootNode.Save(saveConfigPath);
		}

		public static void saveStaticPersistence(StaticObject obj)
		{
			Boolean bFoundStatic = false;

			// Debug.Log("KK: saveStaticPersistence");
			var FacilityKey = obj.getSetting("RadialPosition");
			// Debug.Log("KK: FacilityKey is " + FacilityKey.ToString());

			string saveConfigPath = string.Format("{0}saves/{1}/KKFacilities.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			ConfigNode rootNode = new ConfigNode();

			if (!File.Exists(saveConfigPath))
			{
				ConfigNode GameNode = rootNode.AddNode("GAME");
				ConfigNode ScenarioNode = GameNode.AddNode("SCENARIO");
				ScenarioNode.AddValue("Name", "KKStatics");
				rootNode.Save(saveConfigPath);
			}

			rootNode = ConfigNode.Load(saveConfigPath);

			ConfigNode rootrootNode = rootNode.GetNode("GAME");
			ConfigNode cnHolder = new ConfigNode();

			foreach (ConfigNode ins in rootrootNode.GetNodes("SCENARIO"))
			{
				cnHolder = ins;
				foreach (ConfigNode insins in ins.GetNodes("KKStatic"))
				{
					// Debug.Log("KK: Found a KKStatic");
					string sRadPos = insins.GetValue("RadialPosition");
					if (sRadPos == null)
					{
						Debug.Log("KK: Got a KKStatic but it has no key! WTF?????");
						continue;
					}
					if (sRadPos == FacilityKey.ToString())
					{
						// Debug.Log("KK: Got a KKStatic key match - editing the node");

						if (insins.HasValue("LqFCurrent"))
							insins.RemoveValue("LqFCurrent");
						if (insins.HasValue("OxFCurrent"))
							insins.RemoveValue("OxFCurrent");
						if (insins.HasValue("MoFCurrent"))
							insins.RemoveValue("MoFCurrent");

						if (insins.HasValue("StaffCurrent"))
							insins.RemoveValue("StaffCurrent");
						if (insins.HasValue("TrackingShort"))
							insins.RemoveValue("TrackingShort");
						if (insins.HasValue("TrackingAngle"))
							insins.RemoveValue("TrackingAngle");
						if (insins.HasValue("FacilityXP"))
							insins.RemoveValue("FacilityXP");
						if (insins.HasValue("TargetType"))
							insins.RemoveValue("TargetType");
						if (insins.HasValue("TargetID"))
							insins.RemoveValue("TargetID");
						if (insins.HasValue("OpenCloseState"))
							insins.RemoveValue("OpenCloseState");

						insins.AddValue("LqFCurrent", obj.getSetting("LqFCurrent").ToString());
						insins.AddValue("OxFCurrent", obj.getSetting("OxFCurrent").ToString());
						insins.AddValue("MoFCurrent", obj.getSetting("MoFCurrent").ToString());

						insins.AddValue("StaffCurrent", obj.getSetting("StaffCurrent").ToString());
						insins.AddValue("TrackingShort", obj.getSetting("TrackingShort").ToString());
						insins.AddValue("TrackingAngle", obj.getSetting("TrackingAngle").ToString());
						insins.AddValue("FacilityXP", obj.getSetting("FacilityXP").ToString());
						insins.AddValue("OpenCloseState", obj.getSetting("OpenCloseState"));
						insins.AddValue("TargetType", obj.getSetting("TargetType"));
						insins.AddValue("TargetID", obj.getSetting("TargetID"));

						bFoundStatic = true;
						break;
					}
				}
			}

			if (!bFoundStatic)
			{
				// Debug.Log("KK: No KKStatic found. Creating a new node.");

				ConfigNode newStatic = new ConfigNode("KKStatic");
				newStatic.AddValue("RadialPosition", obj.getSetting("RadialPosition").ToString());
				newStatic.AddValue("LqFCurrent", obj.getSetting("LqFCurrent").ToString());
				newStatic.AddValue("OxFCurrent", obj.getSetting("OxFCurrent").ToString());
				newStatic.AddValue("MoFCurrent", obj.getSetting("MoFCurrent").ToString());

				newStatic.AddValue("StaffCurrent", obj.getSetting("StaffCurrent").ToString());
				newStatic.AddValue("TrackingShort", obj.getSetting("TrackingShort").ToString());
				newStatic.AddValue("TrackingAngle", obj.getSetting("TrackingAngle").ToString());
				newStatic.AddValue("FacilityXP", obj.getSetting("FacilityXP").ToString());
				newStatic.AddValue("TargetType", obj.getSetting("TargetType"));
				newStatic.AddValue("TargetID", obj.getSetting("TargetID"));
				newStatic.AddValue("OpenCloseState", obj.getSetting("OpenCloseState"));

				cnHolder.AddNode(newStatic);
			}

			// Debug.Log("KK: rootNode.save");
			rootNode.Save(saveConfigPath);
		}

		public static void loadStaticPersistence(StaticObject obj)
		{
			// Debug.Log("KK: loadStaticPersistence");
			var FacilityKey = obj.getSetting("RadialPosition");
			// Debug.Log("KK: FacilityKey is " + FacilityKey.ToString());

			string saveConfigPath = string.Format("{0}saves/{1}/KKFacilities.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			ConfigNode rootNode = new ConfigNode();

			if (!File.Exists(saveConfigPath))
			{
				ConfigNode GameNode = rootNode.AddNode("GAME");
				ConfigNode ScenarioNode = GameNode.AddNode("SCENARIO");
				ScenarioNode.AddValue("Name", "KKStatics");
				rootNode.Save(saveConfigPath);
			}

			rootNode = ConfigNode.Load(saveConfigPath);
			ConfigNode rootrootNode = rootNode.GetNode("GAME");

			Boolean bMatch = false;

			foreach (ConfigNode ins in rootrootNode.GetNodes("SCENARIO"))
			{
				if (ins.GetValue("Name") == "KKStatics")
				{
					// Debug.Log("KK: Found SCENARIO named KKStatics");

					foreach (ConfigNode insins in ins.GetNodes("KKStatic"))
					{
						// Debug.Log("KK: Found a KKStatic");
						string sRadPos = insins.GetValue("RadialPosition");
						if (sRadPos == FacilityKey.ToString())
						{
							// Debug.Log("KK: Got a KKStatic key match");
							obj.setSetting("LqFCurrent", float.Parse(insins.GetValue("LqFCurrent")));
							obj.setSetting("OxFCurrent", float.Parse(insins.GetValue("OxFCurrent")));
							obj.setSetting("MoFCurrent", float.Parse(insins.GetValue("MoFCurrent")));

							obj.setSetting("StaffCurrent", float.Parse(insins.GetValue("StaffCurrent")));
							obj.setSetting("FacilityXP", float.Parse(insins.GetValue("FacilityXP")));
							
							//if (insins.GetValue("TrackingShort") != null)
								obj.setSetting("TrackingShort", float.Parse(insins.GetValue("TrackingShort")));
							//else
								//obj.setSetting("TrackingShort", (float)85000f);

							//if (insins.GetValue("TrackingAngle") != null)
								obj.setSetting("TrackingAngle", float.Parse(insins.GetValue("TrackingAngle")));
							//else
								//obj.setSetting("TrackingAngle", (float)65f);

							obj.setSetting("TargetType", insins.GetValue("TargetType"));
							obj.setSetting("OpenCloseState", insins.GetValue("OpenCloseState"));
							obj.setSetting("TargetID", insins.GetValue("TargetID"));
							bMatch = true;
							break;
						}
						// else
						// Debug.Log("KK: No KKStatic key match");
					}
					break;
				}
			}

			if (!bMatch)
			{
				// Debug.Log("KK: KKStatic not yet persistent for this save. Initialising KKStatic");
				obj.setSetting("LqFCurrent", 0.00f);
				obj.setSetting("OxFCurrent", 0.00f);
				obj.setSetting("MoFCurrent", 0.00f);

				obj.setSetting("StaffCurrent", 1f);
				obj.setSetting("FacilityXP", 0f);

				if  (obj.getSetting("TrackingShort") == null)
					obj.setSetting("TrackingShort", 0f);

				if (obj.getSetting("TrackingAngle") == null)
					obj.setSetting("TrackingAngle", 0f);

				obj.setSetting("TargetType", "None");
				obj.setSetting("TargetID", "No Target");
				obj.setSetting("OpenCloseState", "Closed");
				saveStaticPersistence(obj);
			}
		}
	}
}
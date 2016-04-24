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

		public static List<string> pAttributes = new List<string>{
			"FacilityType", "RadialPosition", "InStorage", "FacilityLengthUsed", "FacilityWidthUsed", "FacilityHeightUsed",
			"FacilityMassUsed", "TrackingShort", "TrackingAngle", "TargetType", "TargetID", "FacilityXP",
			"StaffMax", "StaffCurrent", "LqFCurrent", "OxFCurrent", "MoFCurrent",
			"ECCurrent", "OreCurrent", "PrOreCurrent", "ScienceOMax", "ScienceOCurrent",
			"RepOMax", "RepOCurrent", "FundsOMax", "FundsOCurrent", "LastCheck",
			"ProductionRateMax", "ProductionRateCurrent", "Producing", "OpenCloseState"};

		public static List<string> pDefaultAttributes = new List<string>{
			"FacilityType", "StaffMax", "ScienceOMax", "RepOMax", "FundsOMax", "ProductionRateMax"};

		public static List<string> pStringAttributes = new List<string>{
			"FacilityType", "InStorage", "TargetType", "TargetID", "Producing", "OpenCloseState", "MissionLog"};

		public static List<string> pFloatAttributes = new List<string>{
			"FacilityLengthUsed", "FacilityWidthUsed", "FacilityHeightUsed", "FacilityMassUsed",
			"TrackingShort", "TrackingAngle", "FacilityXP",
			"StaffMax", "StaffCurrent", "LqFCurrent", "OxFCurrent", "MoFCurrent",
			"ECCurrent", "OreCurrent", "PrOreCurrent", "ScienceOMax", "ScienceOCurrent",
			"RepOMax", "RepOCurrent", "FundsOMax", "FundsOCurrent", "LastCheck",
			"ProductionRateMax", "ProductionRateCurrent", "MissionCount"};

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
						foreach (string sAtt in pAttributes)
						{
							if (insins.HasValue(sAtt))
								insins.RemoveValue(sAtt);

							if (obj.getSetting(sAtt) != null)
								insins.AddValue(sAtt, obj.getSetting(sAtt).ToString());
						}

						bFoundStatic = true;
						break;
					}
				}
			}

			if (!bFoundStatic)
			{
				// Debug.Log("KK: No KKStatic found. Creating a new node.");
				ConfigNode newStatic = new ConfigNode("KKStatic");

				foreach (string sAtt in pAttributes)
				{
					if (obj.getSetting(sAtt) != null)
						newStatic.AddValue(sAtt, obj.getSetting(sAtt).ToString());
				}

				cnHolder.AddNode(newStatic);
			}

			// Debug.Log("KK: rootNode.save");
			rootNode.Save(saveConfigPath);
		}

		/* public static void UpdateRTGroundStations()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;
			
			string sGuID;
			string sTSName;
			string sDLat;
			string sDLon;
			string sHeight;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation")
				{
					if ((string)obj.getSetting("OpenCloseState") == "Open")
					{
						sGuID = obj.getSetting("RadialPosition").ToString();
						sGuID = sGuID.Trim(new Char[] { ' ', ',', '.', '(', ')' });
						sGuID = sGuID.Replace(" ", "").Replace(",", "").Replace(".", "").Replace(")", "").Replace("(", "").Replace("-", "1");

						CelestialBody CelBody = (CelestialBody)obj.getSetting("CelestialBody");
						var objectpos = CelBody.transform.InverseTransformPoint(obj.gameObject.transform.position);
						var dObjectLat = NavUtils.GetLatitude(objectpos);
						var dObjectLon = NavUtils.GetLongitude(objectpos);
						var disObjectLat = dObjectLat * 180 / Math.PI;
						var disObjectLon = dObjectLon * 180 / Math.PI;

						sDLat = disObjectLat.ToString();
						sDLon = disObjectLon.ToString();
						sTSName = "Stn at Lt " + sDLat + " Ln " + sDLon;
						sHeight = obj.getSetting("RadiusOffset").ToString();
					}
					else
					{

					}
				}
			}
		}

		public static void saveRTCareerBackup()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string rtPath = string.Format("{0}GameData/RemoteTech/", KSPUtil.ApplicationRootPath);
			string rtConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);
			if (!Directory.Exists(rtPath)) return;
			if (!File.Exists(rtConfigPath)) return;

			// Re-write the config
			/* ConfigNode rootNode = new ConfigNode();

			rootNode = ConfigNode.Load(rtConfigPath);

			ConfigNode rootrootNode = rootNode.GetNode("RemoteTechSettings");
			ConfigNode rrrNode = rootrootNode.GetNode("GroundStations");
			rrrNode.RemoveNodes("STATION");

			string sGuID;
			string sTSName;
			string sDLat;
			string sDLon;
			string sHeight;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation" && (string)obj.getSetting("OpenCloseState") == "Open") 
				{
					sGuID = obj.getSetting("RadialPosition").ToString();
					sGuID = sGuID.Trim(new Char[] { ' ', ',', '.', '(',')' });
					sGuID = sGuID.Replace(" ", "").Replace(",", "").Replace(".", "").Replace(")", "").Replace("(", "").Replace("-","1");

					CelestialBody CelBody = (CelestialBody)obj.getSetting("CelestialBody");
					var objectpos = CelBody.transform.InverseTransformPoint(obj.gameObject.transform.position);
					var dObjectLat = NavUtils.GetLatitude(objectpos);
					var dObjectLon = NavUtils.GetLongitude(objectpos);
					var disObjectLat = dObjectLat * 180 / Math.PI;
					var disObjectLon = dObjectLon * 180 / Math.PI;

					sDLat = disObjectLat.ToString();
					sDLon = disObjectLon.ToString();
					sTSName = "Station at Lat " + sDLat + " Lon "+ sDLon;
					sHeight = obj.getSetting("RadiusOffset").ToString();

					ConfigNode newComm = new ConfigNode("STATION");
					newComm.AddValue("Guid", sGuID);
					newComm.AddValue("Name", sTSName);
					newComm.AddValue("Latitude", sDLat);
					newComm.AddValue("Longitude", sDLon);
					newComm.AddValue("Height", sHeight);
					newComm.AddValue("Body", "1");
					newComm.AddValue("MarkColor", "1,1,0,1");

					ConfigNode cnAntennas = new ConfigNode("Antennas");

					ConfigNode cnAntenna = new ConfigNode("ANTENNA");
					cnAntenna.AddValue("Omni", "5E+06");

					cnAntennas.AddNode(cnAntenna);
					newComm.AddNode(cnAntennas);
					rrrNode.AddNode(newComm);
				}
			}
			rootNode.Save(rtConfigPath);

			UpdateRTGroundStations();

			// Back it up
			string backupConfigPath = string.Format("{0}saves/{1}/KKRTCareerBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			File.Copy(rtConfigPath, backupConfigPath, true);
		} */

		/* public static void loadRTCareerBackup()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string rtPath = string.Format("{0}GameData/RemoteTech/", KSPUtil.ApplicationRootPath);
			string rtConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);
			if (!Directory.Exists(rtPath)) return;
			if (!File.Exists(rtConfigPath)) return;

			// Write the backup over the current
			string backupConfigPath = string.Format("{0}saves/{1}/KKRTCareerBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			if (!File.Exists(backupConfigPath)) return;
			File.Copy(backupConfigPath, rtConfigPath, true);

			UpdateRTGroundStations();
		} */

		public static void backupRemoteTechConfig()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string saveConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);
			string backupConfigPath = KerbalKonstructs.installDir + "/KKRTBack.cfg";

			if (File.Exists(saveConfigPath))
				File.Copy(saveConfigPath, backupConfigPath, true);
		}

		public static void restoreRemoteTechConfig()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string saveConfigPath = KerbalKonstructs.installDir + "/KKRTBack.cfg";
			string restoreConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);

			if (File.Exists(saveConfigPath) && File.Exists(restoreConfigPath))
				File.Copy(saveConfigPath, restoreConfigPath, true);
		}

		public static void savePersistenceBackup()
		{
			string saveConfigPath = string.Format("{0}saves/{1}/KKFacilities.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			string backupConfigPath = string.Format("{0}saves/{1}/KKFacilitiesBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			if (File.Exists(saveConfigPath))
				File.Copy(saveConfigPath, backupConfigPath, true);

			string saveConfigPath2 = string.Format("{0}saves/{1}/KK.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			string backupConfigPath2 = string.Format("{0}saves/{1}/KKBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			if (File.Exists(saveConfigPath2))
				File.Copy(saveConfigPath2, backupConfigPath2, true);
		}

		public static void loadPersistenceBackup()
		{
			string saveConfigPath = string.Format("{0}saves/{1}/KKFacilities.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			string backupConfigPath = string.Format("{0}saves/{1}/KKFacilitiesBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			if (File.Exists(backupConfigPath))
				File.Copy(backupConfigPath, saveConfigPath, true);

			string saveConfigPath2 = string.Format("{0}saves/{1}/KK.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			string backupConfigPath2 = string.Format("{0}saves/{1}/KKBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			if (File.Exists(backupConfigPath2))
				File.Copy(backupConfigPath2, saveConfigPath2, true);
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
							foreach (string sAtt in pStringAttributes)
							{
								if (insins.GetValue(sAtt) != null)
									obj.setSetting(sAtt, insins.GetValue(sAtt));
							}
							foreach (string sAtt2 in pFloatAttributes)
							{
								if (insins.GetValue(sAtt2) != null)
									obj.setSetting(sAtt2, float.Parse(insins.GetValue(sAtt2)));
							}
							// Debug.Log("KK: Got a KKStatic key match");
							bMatch = true;
							break;
						}
					}
					break;
				}

			}

			if (!bMatch)
			{
				// Debug.Log("KK: KKStatic not yet persistent for this save. Initialising KKStatic");
				// Model defaults initialisation
				foreach (string sAtt5 in pDefaultAttributes)
				{
					string sDefault = "Default" + sAtt5;

					if (obj.getSetting(sAtt5) == null)
					{
						if (obj.model.getSetting(sDefault) == null) continue;
						else
							obj.setSetting(sAtt5, obj.model.getSetting(sDefault));
					}
				}

				foreach (string sAtt3 in pStringAttributes)
				{
					if (obj.getSetting(sAtt3) == null)
						obj.setSetting(sAtt3, "None");
				}
				foreach (string sAtt4 in pFloatAttributes)
				{
					if (obj.getSetting(sAtt4) == null)
						obj.setSetting(sAtt4, 0f);
				}

				// Fixed intialisation
				obj.setSetting("StaffCurrent", 1f);
				obj.setSetting("TargetID", "None");
				obj.setSetting("OpenCloseState", "Closed");
				
				saveStaticPersistence(obj);
			}
		}
	}
}
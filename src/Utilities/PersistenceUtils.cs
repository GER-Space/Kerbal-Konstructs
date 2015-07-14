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
						insins.AddValue("FacilityXP", obj.getSetting("FacilityXP").ToString());
						insins.AddValue("OpenCloseState", obj.getSetting("OpenCloseState").ToString());
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
				newStatic.AddValue("RadialPosition", obj.getSetting("RadialPosition"));
				newStatic.AddValue("LqFCurrent", obj.getSetting("LqFCurrent"));
				newStatic.AddValue("OxFCurrent", obj.getSetting("OxFCurrent"));
				newStatic.AddValue("MoFCurrent", obj.getSetting("MoFCurrent"));

				newStatic.AddValue("StaffCurrent", obj.getSetting("StaffCurrent"));
				newStatic.AddValue("TrackingShort", obj.getSetting("TrackingShort"));
				newStatic.AddValue("FacilityXP", obj.getSetting("FacilityXP"));
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
							obj.setSetting("TrackingShort", float.Parse(insins.GetValue("TrackingShort")));
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
				obj.setSetting("TrackingShort", 85000f);
				obj.setSetting("TargetType", "None");
				obj.setSetting("TargetID", "No Target");
				obj.setSetting("OpenCloseState", "Closed");
				saveStaticPersistence(obj);
			}
		}
	}
}
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;
using Upgradeables;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
	class KSCManager
	{
		Rect KSCmanagerRect = new Rect(150, 50, 640, 380);

		public Texture tTexture = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscadmin", false);

		public Texture tAdmin = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscadmin", false);
		public Texture tTracking = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ksctracking", false);
		public Texture tRND = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscrnd", false);
		public Texture tLaunch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ksclaunch", false);
		public Texture tRunway = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscrun", false);
		public Texture tFacVAB = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscvab", false);
		public Texture tFacSPH = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscsph", false);
		public Texture tAstro = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscastro", false);
		public Texture tControl = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ksccommand", false);
		public Texture tTick = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingstick", false);
		public Texture tCross = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingscross", false);

		public String sSelectedfacility = "";
		public String strLaunchpad = "";
		public String strRunway = "";
		public String strVAB = "";
		public String strSPH = "";
		public String strTracking = "";
		public String strAstro = "";
		public String strControl = "";
		public String strRND = "";
		public String strAdmin = "";

		public String sFacilityUseRange = "";

		public bool bDetermined = false;

		string sString = "";

		float fFacLvl = 0f;
		float fmaxLvl = 0f;

		public double dUpdater = 0;

		public void drawKSCManager()
		{
			KSCmanagerRect = GUI.Window(0xC00B1E2, KSCmanagerRect, drawKSCmanagerWindow, "Base Boss : KSC Manager");
		}

		public void DetermineFacilityLevels()
		{
			foreach (UpgradeableFacility facility in GameObject.FindObjectsOfType<UpgradeableFacility>())
			{
				fFacLvl = 1 + (facility.FacilityLevel);
				fmaxLvl = 1 + (facility.MaxLevel);
				sString = "Lvl " + fFacLvl + "/" + fmaxLvl;

				if (facility.name == "LaunchPad")
					strLaunchpad = sString;
				if (facility.name == "Runway")
					strRunway = sString;
				if (facility.name == "VehicleAssemblyBuilding")
					strVAB = sString;
				if (facility.name == "SpaceplaneHangar")
					strSPH = sString;
				if (facility.name == "TrackingStation")
					strTracking = sString;
				if (facility.name == "AstronautComplex")
					strAstro = sString;
				if (facility.name == "MissionControl")
					strControl = sString;
				if (facility.name == "ResearchAndDevelopment")
					strRND = sString;
				if (facility.name == "Administration")
					strAdmin = sString;
			}
		}

		public void drawKSCmanagerWindow(int WindowID)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Box("LaunchPad", GUILayout.Width(66));
				GUILayout.Box("Runway", GUILayout.Width(66));
				GUILayout.Box("VAB", GUILayout.Width(66));
				GUILayout.Box("SPH", GUILayout.Width(66));
				GUILayout.Box("Tracking", GUILayout.Width(66));
				GUILayout.Box("Astro", GUILayout.Width(66));
				GUILayout.Box("Control", GUILayout.Width(66));
				GUILayout.Box("R&D", GUILayout.Width(66));
				GUILayout.Box("Admin", GUILayout.Width(66));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = !(sSelectedfacility == "LaunchPad");
				if (GUILayout.Button(tLaunch, GUILayout.Width(66)))
					sSelectedfacility = "LaunchPad";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "Runway");
				if (GUILayout.Button(tRunway, GUILayout.Width(66)))
					sSelectedfacility = "Runway";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "VAB");
				if (GUILayout.Button(tFacVAB, GUILayout.Width(66)))
					sSelectedfacility = "VAB";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "SPH");
				if (GUILayout.Button(tFacSPH, GUILayout.Width(66)))
					sSelectedfacility = "SPH";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "Tracking");
				if (GUILayout.Button(tTracking, GUILayout.Width(66)))
					sSelectedfacility = "Tracking";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "Astro");
				if (GUILayout.Button(tAstro, GUILayout.Width(66)))
					sSelectedfacility = "Astro";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "Control");
				if (GUILayout.Button(tControl, GUILayout.Width(66)))
					sSelectedfacility = "Control";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "R&D");
				if (GUILayout.Button(tRND, GUILayout.Width(66)))
					sSelectedfacility = "R&D";
				GUI.enabled = true;
				GUI.enabled = !(sSelectedfacility == "Admin");
				if (GUILayout.Button(tAdmin, GUILayout.Width(66)))
					sSelectedfacility = "Admin";
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if (!bDetermined)
				{
					DetermineFacilityLevels();
					bDetermined = true;
				}

				double dTicker = Planetarium.GetUniversalTime();
				if ((dTicker - dUpdater) > 10)
				{
					dUpdater = Planetarium.GetUniversalTime();
					bDetermined = false;
				}

				GUILayout.Box(strLaunchpad, GUILayout.Width(66));
				GUILayout.Box(strRunway, GUILayout.Width(66));
				GUILayout.Box(strVAB, GUILayout.Width(66));
				GUILayout.Box(strSPH, GUILayout.Width(66));
				GUILayout.Box(strTracking, GUILayout.Width(66));
				GUILayout.Box(strAstro, GUILayout.Width(66));
				GUILayout.Box(strControl, GUILayout.Width(66));
				GUILayout.Box(strRND, GUILayout.Width(66));
				GUILayout.Box(strAdmin, GUILayout.Width(66));
			}
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("KERBAL KONSTRUCTS SETTINGS"))
			{
				if (!KerbalKonstructs.instance.showSettings)
					KerbalKonstructs.instance.showSettings = true;
				else
					KerbalKonstructs.instance.showSettings = false;
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
	}
}

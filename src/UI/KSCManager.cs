using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
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
		Rect KSCmanagerRect = new Rect(150, 50, 410, 680);

		public Texture tTexture = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscadminister", false);
		public Texture tBanner = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/banner", false);

		public Texture tAdmin = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscadminister", false);
		public Texture tTracking = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ksctrack", false);
		public Texture tRND = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ksclabs", false);
		public Texture tLaunch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscpad", false);
		public Texture tRunway = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscway", false);
		public Texture tFacVAB = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscassembly", false);
		public Texture tFacSPH = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kschangar", false);
		public Texture tAstro = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscquarters", false);
		public Texture tControl = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ksccontrol", false);
		public Texture tTick = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingstick", false);
		public Texture tCross = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingscross", false);

		GUIStyle Yellowtext;
		GUIStyle TextAreaNoBorder;
		GUIStyle KKWindow;
		GUIStyle BoxNoBorder;
		GUIStyle SmallButton;
		GUIStyle LabelWhite;
		GUIStyle KKWindowTitle;
		GUIStyle LabelInfo;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle LabelTip;

		public String sSelectedfacility = "None";
		public String sFacilityUseRange = "";
		public String sLevel;

		public bool bDetermined = false;

		public static float fFacLvl = 0f;
		public static float fmaxLvl = 0f;

		public double dUpdater = 0;

		public void drawKSCManager()
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3, 3, 5, 5);

			KSCmanagerRect = GUI.Window(0xC00B1E2, KSCmanagerRect, drawKSCmanagerWindow, "", KKWindow);
		}

		public static string DetermineFacilityLevel(string sFacility)
		{
			string slLevel = "Lvl 1";
			string sString;
			foreach (UpgradeableFacility facility in GameObject.FindObjectsOfType<UpgradeableFacility>())
			{
				fFacLvl = 1 + (facility.FacilityLevel);
				fmaxLvl = 1 + (facility.MaxLevel);
				sString = "Lvl " + fFacLvl + "/" + fmaxLvl;

				if (facility.name == sFacility)
				{
					slLevel = sString;
					return slLevel;
				}
			}

			return slLevel;
		}

		public void drawKSCmanagerWindow(int WindowID)
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

			Yellowtext = new GUIStyle(GUI.skin.box);
			Yellowtext.normal.textColor = Color.yellow;
			Yellowtext.normal.background = null;

			TextAreaNoBorder = new GUIStyle(GUI.skin.textArea);
			TextAreaNoBorder.normal.background = null;
			TextAreaNoBorder.normal.textColor = Color.white;
			TextAreaNoBorder.fontSize = 12;
			TextAreaNoBorder.padding.left = 1;
			TextAreaNoBorder.padding.right = 1;
			TextAreaNoBorder.padding.top = 4;

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

			LabelWhite = new GUIStyle(GUI.skin.label);
			LabelWhite.normal.background = null;
			LabelWhite.normal.textColor = Color.white;
			LabelWhite.fontSize = 12;
			LabelWhite.padding.left = 1;
			LabelWhite.padding.right = 1;
			LabelWhite.padding.top = 4;

			LabelTip = new GUIStyle(GUI.skin.label);
			LabelTip.normal.background = null;
			LabelTip.normal.textColor = Color.yellow;
			LabelTip.fontSize = 12;
			LabelTip.padding.left = 1;
			LabelTip.padding.right = 1;
			LabelTip.padding.top = 4;

			LabelInfo = new GUIStyle(GUI.skin.label);
			LabelInfo.normal.background = null;
			LabelInfo.normal.textColor = Color.white;
			LabelInfo.fontSize = 13;
			LabelInfo.fontStyle = FontStyle.Bold;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

			KKWindowTitle = new GUIStyle(GUI.skin.box);
			KKWindowTitle.normal.background = null;
			KKWindowTitle.normal.textColor = Color.white;
			KKWindowTitle.fontSize = 14;
			KKWindowTitle.fontStyle = FontStyle.Bold;

			SmallButton = new GUIStyle(GUI.skin.button);
			SmallButton.normal.textColor = Color.red;
			SmallButton.hover.textColor = Color.white;
			SmallButton.padding.top = 1;
			SmallButton.padding.left = 1;
			SmallButton.padding.right = 1;
			SmallButton.padding.bottom = 4;
			SmallButton.normal.background = null;
			SmallButton.hover.background = null;
			SmallButton.fontSize = 12;

			GUILayout.Box(tBanner, BoxNoBorder);
			GUILayout.Space(5);

			if (MiscUtils.isCareerGame())
			{
				GUILayout.Box("KSC Primary Facilities");
				GUILayout.BeginHorizontal();
				{
					GUI.enabled = !(sSelectedfacility == "LaunchPad");
					if (GUILayout.Button(tLaunch, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "LaunchPad";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "Runway");
					if (GUILayout.Button(tRunway, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "Runway";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "VehicleAssemblyBuilding");
					if (GUILayout.Button(tFacVAB, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "VehicleAssemblyBuilding";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "SpaceplaneHangar");
					if (GUILayout.Button(tFacSPH, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "SpaceplaneHangar";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "TrackingStation");
					if (GUILayout.Button(tTracking, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "TrackingStation";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "AstronautComplex");
					if (GUILayout.Button(tAstro, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "AstronautComplex";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "MissionControl");
					if (GUILayout.Button(tControl, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "MissionControl";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "ResearchAndDevelopment");
					if (GUILayout.Button(tRND, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "ResearchAndDevelopment";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;

					GUI.enabled = !(sSelectedfacility == "Administration");
					if (GUILayout.Button(tAdmin, GUILayout.Height(30), GUILayout.Width(35)))
					{
						sSelectedfacility = "Administration";
						sLevel = DetermineFacilityLevel(sSelectedfacility);
					}
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Selected Facility: " + sSelectedfacility, LabelInfo);
					GUILayout.FlexibleSpace();
					GUILayout.Label(sLevel, LabelInfo);
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(3);
				GUILayout.Box("KSC Secondary Facilities");
				GUILayout.Label("Interface under construction. Please use a door and go talk to them.", LabelInfo);

				GUILayout.Space(3);
				GUILayout.Box("Remote Base Management");
				GUILayout.Label("Interface under construction. Please use other interfaces available through primary KSC facilities or launch a plane and go and see them.", LabelInfo);

				GUILayout.Space(3);
				GUILayout.Box("United Kerbin Council");
				GUILayout.Label("All funding of KSC staffing and management covered, with a few exclusions. Like snacks. And secondary facilities. And you still need to fund your vehicle construction. And facility repairs. Talk to Gene for funding sources. Not Mortimer.", LabelInfo);
				GUILayout.Label("Additional Grants", LabelInfo);
				GUILayout.Label("None", LabelInfo);
				GUILayout.Label("None", LabelInfo);
				GUILayout.Label("None", LabelInfo);

				GUILayout.Space(3);
				GUILayout.Box("KSP International Support & Enmity");
				GUILayout.Label("All nations in full support of KSP through the UKC. No enmities of concern. Maybe some jealousy and abject terror in a few places.", LabelInfo);

				GUILayout.Space(3);
				GUILayout.Box("Competing International Programs");
				GUILayout.Label("None known. Yet.", LabelInfo);

				GUILayout.Space(3);
				GUILayout.Box("Independent Commercial Programs");
				GUILayout.Label("None known. Yet.", LabelInfo);
			}
			else
			{
				GUILayout.Box("Many features of Kerbal Konstructs are disabled when not in Career Mode");
			}

			GUILayout.FlexibleSpace();
			GUILayout.Label("TIP: To use the KK editor, hit Ctrl+K when inflight, preferably when landed near the base you want to edit or the location you want to make a new base.", LabelTip);
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Mod Settings"))
			{
                WindowManager.instance.ToggleWindow(KerbalKonstructs.instance.GUI_Settings.drawKKSettingsGUI);
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
	}
}

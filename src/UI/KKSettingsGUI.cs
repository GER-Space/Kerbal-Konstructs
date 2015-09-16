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
using KerbalKonstructs.Utilities;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
	class KKSettingsGUI
	{
		Rect KKSettingsRect = new Rect(400, 20, 375, 550);

		public Vector2 scrollSettings;

		public List<LaunchSite> sites;

		public Texture tTick = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingstick", false);
		public Texture tCross = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingscross", false);
		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

		public String sFacilityUseRange = "";
		public String sDefaultRange = "";
		public String sDefaultFactor = "";
		public String sVisRange = "";

		public bool bDetermined = false;

		public bool bChangeSPHDefault = false;
		public bool bChangeVABDefault = false;

		GUIStyle Yellowtext;
		GUIStyle KKWindow;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle BoxNoBorder;
		GUIStyle LabelInfo;
		GUIStyle LabelYellow;

		public void drawKKSettingsGUI()
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3, 3, 5, 5);

			KKSettingsRect = GUI.Window(0xC10B3A8, KKSettingsRect, drawKKSettingsWindow, "", KKWindow);
		}

		public void drawKKSettingsWindow(int WindowID)
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
			LabelInfo.fontStyle = FontStyle.Normal;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

			LabelYellow = new GUIStyle(GUI.skin.label);
			LabelYellow.normal.background = null;
			LabelYellow.normal.textColor = Color.yellow;
			LabelYellow.fontSize = 13;
			LabelYellow.fontStyle = FontStyle.Bold;
			LabelYellow.padding.left = 3;
			LabelYellow.padding.top = 0;
			LabelYellow.padding.bottom = 0;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Settings", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
				{
					KerbalKonstructs.instance.showSettings = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			scrollSettings = GUILayout.BeginScrollView(scrollSettings);
			GUILayout.Box("Default VAB Launchsite");
			if (GUILayout.Button("" + KerbalKonstructs.instance.defaultVABlaunchsite, GUILayout.Height(23)))
			{
				bChangeVABDefault = true;
			}

			if (bChangeVABDefault)
			{
				if (sites == null) sites = LaunchSiteManager.getLaunchSites();

				sites.Sort(delegate(LaunchSite a, LaunchSite b)
				{
					return (a.name).CompareTo(b.name);
				});

				GUILayout.Label("Select a default site from the list that follows.", LabelInfo);

				foreach (LaunchSite site in sites)
				{
					if (MiscUtils.isCareerGame())
					{
						if (site.openclosestate != "Open" && site.opencost != 0)
							continue;
					}

					if (!KerbalKonstructs.instance.launchFromAnySite)
					{
						if (site.type == SiteType.SPH) continue;
					}

					string sButtonName = "";
					sButtonName = site.name;
					if (site.name == "Runway") sButtonName = "KSC Runway";
					if (site.name == "LaunchPad") sButtonName = "KSC LaunchPad";

					if (GUILayout.Button(sButtonName, GUILayout.Height(21)))
					{
						KerbalKonstructs.instance.defaultVABlaunchsite = site.name;
						bChangeVABDefault = false;
					}
				}

				if (GUILayout.Button("CANCEL - NO CHANGE", GUILayout.Height(21)))
				{
					bChangeVABDefault = false;
				}
			}

			GUILayout.Box("Default SPH Launchsite");
			if (GUILayout.Button("" + KerbalKonstructs.instance.defaultSPHlaunchsite, GUILayout.Height(23)))
			{
				bChangeSPHDefault = true;
			}

			if (bChangeSPHDefault)
			{
				if (sites == null) sites = LaunchSiteManager.getLaunchSites();

				sites.Sort(delegate(LaunchSite a, LaunchSite b)
				{
					return (a.name).CompareTo(b.name);
				});

				GUILayout.Label("Select a default site from the list that follows.", LabelInfo);

				foreach (LaunchSite site in sites)
				{
					if (MiscUtils.isCareerGame())
					{
						if (site.openclosestate != "Open" && site.opencost != 0)
							continue;
					}

					if (!KerbalKonstructs.instance.launchFromAnySite)
					{
						if (site.type == SiteType.VAB) continue;
					}

					string sButtonName = "";
					sButtonName = site.name;
					if (site.name == "Runway") sButtonName = "KSC Runway";
					if (site.name == "LaunchPad") sButtonName = "KSC LaunchPad";

					if (GUILayout.Button(sButtonName, GUILayout.Height(21)))
					{
						KerbalKonstructs.instance.defaultSPHlaunchsite = site.name;
						bChangeSPHDefault = false;
					}
				}

				if (GUILayout.Button("CANCEL - NO CHANGE", GUILayout.Height(21)))
				{
					bChangeSPHDefault = false;
				}
			}

			GUILayout.Space(3);

			GUILayout.Box("General Settings");
			GUILayout.Label("Does launching from the VAB or SPH limit the type of launchsite that can be used?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.launchFromAnySite)
			{
				if (GUILayout.Button("Launch From Any Site", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.launchFromAnySite = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Launch From Any Site", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.launchFromAnySite = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Should the career strategy features of KK be enabled? WARNING: Disabling them in the middle of a career game is not advised!", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.disableCareerStrategyLayer)
			{
				if (GUILayout.Button("Disable Career Strategy", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableCareerStrategyLayer = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Disable Career Strategy", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableCareerStrategyLayer = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Should it be possible to launch from sites other than the KSC?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.disableCustomLaunchsites)
			{
				if (GUILayout.Button("Disable Custom Launchsites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableCustomLaunchsites = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Disable Custom Launchsites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableCustomLaunchsites = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Should it only be possible to open bases at the base?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.disableRemoteBaseOpening)
			{
				if (GUILayout.Button("Can Only Open Bases at the Base", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableRemoteBaseOpening = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Can Only Open Bases at the Base", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableRemoteBaseOpening = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Inflight Instruments Defaults");
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.enableATC)
			{
				if (GUILayout.Button("ATC Enabled", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.enableATC = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("ATC Enabled", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.enableATC = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.enableNGS)
			{
				if (GUILayout.Button("NGS Enabled", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.enableNGS = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("NGS Enabled", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.enableNGS = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.enableDownlink)
			{
				if (GUILayout.Button("Downlink Enabled", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.enableDownlink = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Downlink Enabled", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.enableDownlink = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Facility Defaults");
			GUILayout.Label("Some facilities require a craft or kerbal to be close to use them.", LabelInfo);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Use Range: " + KerbalKonstructs.instance.facilityUseRange.ToString("#0") + " m", LabelYellow);
			GUILayout.FlexibleSpace();
			sFacilityUseRange = GUILayout.TextField(sFacilityUseRange, 4, GUILayout.Width(70));
			GUILayout.Label(" m", LabelYellow);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Update", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.facilityUseRange = float.Parse(sFacilityUseRange);

				if (KerbalKonstructs.instance.facilityUseRange < 1f) KerbalKonstructs.instance.facilityUseRange = 1f;
				if (KerbalKonstructs.instance.facilityUseRange > 5000f) KerbalKonstructs.instance.facilityUseRange = 5000f;
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Tracking/Map View Settngs");
			GUILayout.Label("Never show the icons of closed launchsites?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.disableDisplayClosed)
			{
				if (GUILayout.Button("Never Display Closed Launchsites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableDisplayClosed = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Never Display Closed Launchsites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableDisplayClosed = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Toggle icon display with Base Boss display?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.toggleIconsWithBB)
			{
				if (GUILayout.Button("Toggle Icons With Base Boss", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.toggleIconsWithBB = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Toggle Icons With Base Boss", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.toggleIconsWithBB = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Tracking/Map View Defaults");

			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowOpen)
			{
				if (GUILayout.Button("Show Open Sites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowOpen = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Open Sites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowOpen = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowClosed)
			{
				if (GUILayout.Button("Show Closed Sites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowClosed = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Closed Sites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowClosed = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowOpenT)
			{
				if (GUILayout.Button("Show Tracking Stations", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowOpenT = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Tracking Stations", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowOpenT = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowHelipads)
			{
				if (GUILayout.Button("Show Helipads", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowHelipads = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Helipads", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowHelipads = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowRunways)
			{
				if (GUILayout.Button("Show Runways", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowRunways = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Runways", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowRunways = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowRocketbases)
			{
				if (GUILayout.Button("Show Rocket Bases", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowRocketbases = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Rocket Bases", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowRocketbases = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.mapShowOther)
			{
				if (GUILayout.Button("Show Other Sites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowOther = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Show Other Sites", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.mapShowOther = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Base Recovery Settings");
			GUILayout.Label("Should custom bases carry out recovery, or should it only be KSC?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.disableRemoteRecovery)
			{
				if (GUILayout.Button("Disable Recovery by Custom Bases", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableRemoteRecovery = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Disable Recovery by Custom Bases", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.disableRemoteRecovery = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Base Recovery Capability Defaults");

			GUILayout.Label("A percentage of the base amount recovered (modified by distance per KSP functions), IF the recovery is made by a base outside its effective range.", LabelInfo);
			GUILayout.BeginHorizontal();

			GUILayout.Label("Recovery Factor: " + KerbalKonstructs.instance.defaultRecoveryFactor.ToString("#0") + "%", LabelYellow);
			GUILayout.FlexibleSpace();
			sDefaultFactor = GUILayout.TextField(sDefaultFactor, 3, GUILayout.Width(70));
			GUILayout.Label("%", LabelYellow);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Update", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.defaultRecoveryFactor = float.Parse(sDefaultFactor);

				if (KerbalKonstructs.instance.defaultRecoveryFactor < 0f) KerbalKonstructs.instance.defaultRecoveryFactor = 0f;
				if (KerbalKonstructs.instance.defaultRecoveryFactor > 100f) KerbalKonstructs.instance.defaultRecoveryFactor = 100f;
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("100% of the base amount is recovered (modified by distance per KSP functions), IF the recovery is made by a base within its effective range.", LabelInfo);

			GUILayout.BeginHorizontal();

			GUILayout.Label("Effective Range: " + KerbalKonstructs.instance.defaultEffectiveRange.ToString("#0") + "m", LabelYellow);
			GUILayout.FlexibleSpace();
			sDefaultRange = GUILayout.TextField(sDefaultRange, 6, GUILayout.Width(70));
			GUILayout.Label("m", LabelYellow);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Update", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.defaultEffectiveRange = float.Parse(sDefaultRange);

				if (KerbalKonstructs.instance.defaultEffectiveRange < 1000f) KerbalKonstructs.instance.defaultEffectiveRange = 1000f;
				if (KerbalKonstructs.instance.defaultEffectiveRange > 250000f) KerbalKonstructs.instance.defaultEffectiveRange = 250000f;
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Editor Tools Defaults");
			GUILayout.BeginHorizontal();

			GUILayout.Label("Max Visibility Range: " + KerbalKonstructs.instance.maxEditorVisRange.ToString("#0") + "m", LabelYellow);
			GUILayout.FlexibleSpace();
			sVisRange = GUILayout.TextField(sVisRange, 15, GUILayout.Width(70));
			GUILayout.Label(" m", LabelYellow);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Update", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.maxEditorVisRange = float.Parse(sVisRange);

				if (KerbalKonstructs.instance.maxEditorVisRange < 0f) KerbalKonstructs.instance.maxEditorVisRange = 0f;
				if (KerbalKonstructs.instance.maxEditorVisRange > 1000000000000f) KerbalKonstructs.instance.maxEditorVisRange = 1000000000000f;
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Should a preview of a static model be spawned when editing a model config?", LabelInfo);
			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.spawnPreviewModels)
			{
				if (GUILayout.Button("Spawn Preview Static Models", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.spawnPreviewModels = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Spawn Preview Static Models", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.spawnPreviewModels = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Box("Modes");
			GUILayout.Label("WARNING: This will lag your game because of additional writes to KSP.log. Do not switch it on unless a developer of the mod asks you to.", LabelInfo);

			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.DebugMode)
			{
				if (GUILayout.Button("Debug Mode", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.DebugMode = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Debug Mode", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.DebugMode = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("WARNING: This will enable unfinished features that will NOT WORK properly. They may break your save games. Any changes made to static instances will NOT be tagged as custom and CANNOT be exported.", LabelInfo);

			GUILayout.BeginHorizontal();
			if (!KerbalKonstructs.instance.DevMode)
			{
				if (GUILayout.Button("Developer Mode", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.DevMode = true;
				}

				GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			else
			{
				if (GUILayout.Button("Developer Mode", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.DevMode = false;
				}

				GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();
			GUILayout.Space(2);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);
			if (GUILayout.Button("Save Config Settings", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.saveConfig();
			}
			if (GUILayout.Button("Reload Saved Settings", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.loadConfig();
			}
			if (GUILayout.Button("Reset To Factory Settings", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.defaultVABlaunchsite = "LaunchPad";
				KerbalKonstructs.instance.defaultSPHlaunchsite = "Runway";
				KerbalKonstructs.instance.launchFromAnySite = false;
				KerbalKonstructs.instance.disableCareerStrategyLayer = false;
				KerbalKonstructs.instance.disableCustomLaunchsites = false;
				KerbalKonstructs.instance.disableRemoteBaseOpening = false;
				KerbalKonstructs.instance.enableATC = true;
				KerbalKonstructs.instance.enableNGS = true;
				KerbalKonstructs.instance.enableDownlink = true;
				KerbalKonstructs.instance.facilityUseRange = 100;
				KerbalKonstructs.instance.disableDisplayClosed = false;
				KerbalKonstructs.instance.toggleIconsWithBB = false;
				KerbalKonstructs.instance.mapShowOpen = true;
				KerbalKonstructs.instance.mapShowClosed = false;
				KerbalKonstructs.instance.mapShowOpenT = true;
				KerbalKonstructs.instance.mapShowHelipads = true;
				KerbalKonstructs.instance.mapShowRunways = true;
				KerbalKonstructs.instance.mapShowRocketbases = true;
				KerbalKonstructs.instance.mapShowOther = false;
				KerbalKonstructs.instance.disableRemoteRecovery = false;
				KerbalKonstructs.instance.defaultRecoveryFactor = 50;
				KerbalKonstructs.instance.defaultEffectiveRange = 100000;
				KerbalKonstructs.instance.maxEditorVisRange = 100000;
				KerbalKonstructs.instance.spawnPreviewModels = true;
				KerbalKonstructs.instance.DebugMode = false;	
				KerbalKonstructs.instance.DevMode = false;
				
				KerbalKonstructs.instance.saveConfig();
			}
			GUILayout.Space(2);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
	}
}

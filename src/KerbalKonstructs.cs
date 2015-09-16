using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System.Reflection;
using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.API;
using KerbalKonstructs.API.Config;
using Upgradeables;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs
{
	[KSPAddonFixed(KSPAddon.Startup.MainMenu, true, typeof(KerbalKonstructs))]
	public class KerbalKonstructs : MonoBehaviour
	{
		// Hello
		public static KerbalKonstructs instance;		
		public static string installDir = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbalKonstructs));
		private Dictionary<UpgradeableFacility, int> facilityLevels = new Dictionary<UpgradeableFacility, int>();

		#region Holders
		public StaticObject selectedObject;
		public StaticModel selectedModel;
		public StaticObject snapTargetInstance;
		public StaticDatabase staticDB = new StaticDatabase();
		public CameraController camControl = new CameraController();
		private CelestialBody currentBody;

		public int iMenuCount = 0;

		public Double VesselCost = 0;
		public Double RefundAmount = 0;

		string savedObjectPath = "";
		public string lastRecoveryBase = "";
		public float lastRecoveryDistance = 0f;
		public float fRecovFactor = 0f;
		public float fRecovRange = 0f;
		public float fLaunchRefund = 0f;
		public Double dRecoveryValue = 0;
		public Double dActualRecoveryValue = 0;
		#endregion

		#region Switches
		private Boolean atMainMenu = false;
		public Boolean InitialisedFacilities = false;
		public Boolean VesselLaunched = false;
		public Boolean bImportedCustom = false;

		public Boolean bDisablePositionEditing = false;
		#endregion

		#region GUI Windows
		private EditorGUI GUI_Editor = new EditorGUI();
		private StaticsEditorGUI GUI_StaticsEditor = new StaticsEditorGUI();
		private NavGuidanceSystem GUI_NGS = new NavGuidanceSystem();
		private DownlinkGUI GUI_Downlink = new DownlinkGUI();
		private BaseBossFlight GUI_FlightManager = new BaseBossFlight();
		private FacilityManager GUI_FacilityManager = new FacilityManager();
		private LaunchSiteSelectorGUI GUI_LaunchSiteSelector = new LaunchSiteSelectorGUI();
		private MapIconManager GUI_MapIconManager = new MapIconManager();
		private KSCManager GUI_KSCManager = new KSCManager();
		private AirRacing GUI_AirRacingApp = new AirRacing();
		private BaseManager GUI_BaseManager = new BaseManager();
		private KKSettingsGUI GUI_Settings = new KKSettingsGUI();
		public ModelInfo GUI_ModelInfo = new ModelInfo();
		#endregion

		#region Show Toggles
		public Boolean showEditor = false;
		public Boolean showSiteSelector = false;
		public Boolean showBaseManager = false;
		public Boolean showFlightManager = false;
		public Boolean showMapIconManager = false;
		public Boolean showKSCmanager = false;
		public Boolean showNGS = false;
		public Boolean showRacingApp = false;
		public Boolean showFacilityManager = false;
		public Boolean showDownlink = false;
		public Boolean showATC = false;
		public Boolean showSettings = false;
		public Boolean showModelInfo = false;

		public GameObject go = null;

		public Boolean bPreviewModel = false;
		#endregion

		#region App Buttons
		private ApplicationLauncherButton siteSelector;
		private ApplicationLauncherButton flightManager;
		private ApplicationLauncherButton mapManager;
		private ApplicationLauncherButton KSCmanager;
		#endregion

		#region Configurable Variables
		[KSPField]
		public String defaultVABlaunchsite = "LaunchPad";
		[KSPField]
		public String defaultSPHlaunchsite = "Runway";
		[KSPField]
		public Boolean launchFromAnySite = false;
		[KSPField]
		public Boolean disableCareerStrategyLayer = false;
		[KSPField]
		public Boolean disableCustomLaunchsites = false;
		[KSPField]
		public Boolean disableRemoteBaseOpening = false;
		[KSPField]
		public Boolean disableAllInstanceEditing = true;
		[KSPField]
		public Boolean enableATC = true;
		[KSPField]
		public Boolean enableNGS = true;
		[KSPField]
		public Boolean enableDownlink = true;
		[KSPField]
		public Double facilityUseRange = 100;
		[KSPField]
		public Boolean disableDisplayClosed = false;
		[KSPField]
		public Boolean toggleIconsWithBB = false;
		[KSPField]
		public Boolean mapShowOpen = true;
		[KSPField]
		public Boolean mapShowClosed = false;
		[KSPField]
		public Boolean mapShowOpenT = true;
		[KSPField]
		public Boolean mapShowHelipads = true;
		[KSPField]
		public Boolean mapShowRunways = true;
		[KSPField]
		public Boolean mapShowRocketbases = true;
		[KSPField]
		public Boolean mapShowOther = false;
		[KSPField]
		public Boolean disableRemoteRecovery = false;
		[KSPField]
		public Double defaultRecoveryFactor = 50;
		[KSPField]
		public Double defaultEffectiveRange = 100000;
		[KSPField]
		public Double maxEditorVisRange = 100000;
		[KSPField]
		public Boolean spawnPreviewModels = true;
		[KSPField]
		public Boolean DebugMode = false;
		[KSPField]
		public Boolean DevMode = false;
		#endregion

		#region Other Mods Wrappers
		// StageRecovery Wrapping
		void SRProcessingFinished(Vessel vessel)
		{
			OnVesselRecovered(vessel.protoVessel);
		}
		#endregion

		void Awake()
		{
			instance = this;

			#region Game Event Hooks
			GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
			GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
			GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
			GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
			GameEvents.OnFundsChanged.Add(OnDoshChanged);
			GameEvents.onVesselRecovered.Add(OnVesselRecovered);
			GameEvents.onVesselRecoveryProcessing.Add(OnProcessRecovery);
			GameEvents.onGameStateSave.Add(SaveState);
			GameEvents.onGameStateLoad.Add(LoadState);
			GameEvents.OnKSCFacilityUpgraded.Add(OnKSCFacilityUpgraded);
			GameEvents.OnKSCFacilityUpgrading.Add(OnKSCFacilityUpgrading);
			GameEvents.OnUpgradeableObjLevelChange.Add(OnUpgradeableObjLevelChange);
			GameEvents.OnVesselRollout.Add(OnVesselLaunched);
			#endregion

			#region Other Mods Hooks
			if (StageRecoveryWrapper.StageRecoveryAvailable)
			{
				StageRecoveryWrapper.AddRecoveryProcessingStartListener(OnVesselRecoveryRequested);
				StageRecoveryWrapper.AddRecoveryProcessingFinishListener(SRProcessingFinished);
			}
			#endregion

			#region Model API
			KKAPI.addModelSetting("mesh", new ConfigFile());
			ConfigGenericString authorConfig = new ConfigGenericString();
			authorConfig.setDefaultValue("Unknown");
			KKAPI.addModelSetting("author", authorConfig);
			KKAPI.addModelSetting("DefaultLaunchPadTransform", new ConfigGenericString());
			KKAPI.addModelSetting("title", new ConfigGenericString());
			KKAPI.addModelSetting("category", new ConfigGenericString());
			KKAPI.addModelSetting("cost", new ConfigFloat());
			KKAPI.addModelSetting("manufacturer", new ConfigGenericString());
			KKAPI.addModelSetting("description", new ConfigGenericString());
			KKAPI.addModelSetting("DefaultLaunchSiteLength", new ConfigFloat());
			KKAPI.addModelSetting("DefaultLaunchSiteWidth", new ConfigFloat());
			KKAPI.addModelSetting("pointername", new ConfigGenericString());
			KKAPI.addModelSetting("name", new ConfigGenericString());
			#endregion

			#region Instance API
			// Position
			KKAPI.addInstanceSetting("CelestialBody", new ConfigCelestialBody());
			KKAPI.addInstanceSetting("RadialPosition", new ConfigVector3());
			KKAPI.addInstanceSetting("Orientation", new ConfigVector3());
			KKAPI.addInstanceSetting("RadiusOffset", new ConfigFloat());
			KKAPI.addInstanceSetting("RotationAngle", new ConfigFloat());

			// Calculated References - do not set, it will not work
			KKAPI.addInstanceSetting("RefLatitude", new ConfigFloat());
			KKAPI.addInstanceSetting("RefLongitude", new ConfigFloat());

			// Visibility and Grouping
			ConfigFloat visibilityConfig = new ConfigFloat();
			visibilityConfig.setDefaultValue(25000f);
			KKAPI.addInstanceSetting("VisibilityRange", visibilityConfig);
			ConfigGenericString groupConfig = new ConfigGenericString();
			groupConfig.setDefaultValue("Ungrouped");
			KKAPI.addInstanceSetting("Group", groupConfig);

			// Launchsite
			KKAPI.addInstanceSetting("LaunchSiteName", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchPadTransform", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchSiteAuthor", new ConfigGenericString());
			ConfigGenericString descriptionConfig = new ConfigGenericString();
			descriptionConfig.setDefaultValue("No description available.");
			KKAPI.addInstanceSetting("LaunchSiteDescription", descriptionConfig);
			KKAPI.addInstanceSetting("LaunchSiteLogo", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchSiteIcon", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchSiteType", new ConfigSiteType());
			ConfigGenericString category = new ConfigGenericString();
			category.setDefaultValue("Other");
			KKAPI.addInstanceSetting("Category", category);
			KKAPI.addInstanceSetting("LaunchSiteLength", new ConfigFloat());
			KKAPI.addInstanceSetting("LaunchSiteWidth", new ConfigFloat());

			// Career Mode Strategy Instances
			ConfigFloat openCost = new ConfigFloat();
			openCost.setDefaultValue(0f);
			KKAPI.addInstanceSetting("OpenCost", openCost);
			ConfigFloat closeValue = new ConfigFloat();
			closeValue.setDefaultValue(0f);
			KKAPI.addInstanceSetting("CloseValue", closeValue);
			ConfigGenericString opencloseState = new ConfigGenericString();
			opencloseState.setDefaultValue("Closed");
			KKAPI.addInstanceSetting("OpenCloseState", opencloseState);
			ConfigGenericString favouriteSite = new ConfigGenericString();
			favouriteSite.setDefaultValue("No");
			KKAPI.addInstanceSetting("FavouriteSite", favouriteSite);

			// Facility Types
			ConfigGenericString facilityrole = new ConfigGenericString();
			facilityrole.setDefaultValue("None");
			KKAPI.addModelSetting("DefaultFacilityType", facilityrole);
			ConfigGenericString instfacilityrole = new ConfigGenericString();
			instfacilityrole.setDefaultValue("None");
			KKAPI.addInstanceSetting("FacilityType", instfacilityrole);
			KKAPI.addModelSetting("DefaultFacilityLength", new ConfigFloat());
			KKAPI.addModelSetting("DefaultFacilityWidth", new ConfigFloat());
			KKAPI.addModelSetting("DefaultFacilityHeight", new ConfigFloat());
			KKAPI.addModelSetting("DefaultFacilityMassCapacity", new ConfigFloat());
			KKAPI.addModelSetting("DefaultFacilityCraftCapacity", new ConfigFloat());
			KKAPI.addInstanceSetting("FacilityLengthUsed", new ConfigFloat());
			KKAPI.addInstanceSetting("FacilityWidthUsed", new ConfigFloat());
			KKAPI.addInstanceSetting("FacilityHeightUsed", new ConfigFloat());
			KKAPI.addInstanceSetting("FacilityMassUsed", new ConfigFloat());
			KKAPI.addInstanceSetting("InStorage", new ConfigGenericString());

			// Local to a specific save - constructed in a specific save-game
			// WIP for founding
			ConfigGenericString LocalToSave = new ConfigGenericString();
			LocalToSave.setDefaultValue("False");
			KKAPI.addInstanceSetting("LocalToSave", LocalToSave);
				
			// Custom instances - added or modified by player with the editor
			ConfigGenericString CustomInstance = new ConfigGenericString();
			CustomInstance.setDefaultValue("False");
			KKAPI.addInstanceSetting("CustomInstance", CustomInstance);

			// Facility Ratings

			// Tracking station max short range in m
			KKAPI.addInstanceSetting("TrackingShort", new ConfigFloat());
			// Max tracking angle
			KKAPI.addInstanceSetting("TrackingAngle", new ConfigFloat());

			// Target Type and ID
			KKAPI.addInstanceSetting("TargetType", new ConfigGenericString());
			KKAPI.addInstanceSetting("TargetID", new ConfigGenericString());

			//XP
			KKAPI.addInstanceSetting("FacilityXP", new ConfigFloat());

			// Staff
			KKAPI.addModelSetting("DefaultStaffMax", new ConfigFloat());
			KKAPI.addInstanceSetting("StaffMax", new ConfigFloat());
			KKAPI.addInstanceSetting("StaffCurrent", new ConfigFloat());

			// Fueling
			KKAPI.addModelSetting("LqFMax", new ConfigFloat());
			KKAPI.addInstanceSetting("LqFCurrent", new ConfigFloat());
			KKAPI.addModelSetting("OxFMax", new ConfigFloat());
			KKAPI.addInstanceSetting("OxFCurrent", new ConfigFloat());
			KKAPI.addModelSetting("MoFMax", new ConfigFloat());
			KKAPI.addInstanceSetting("MoFCurrent", new ConfigFloat());

			KKAPI.addModelSetting("ECMax", new ConfigFloat());
			KKAPI.addModelSetting("ECRechargeRate", new ConfigFloat());
			KKAPI.addInstanceSetting("ECCurrent", new ConfigFloat());

			// Industry
			KKAPI.addModelSetting("DefaultProductionRateMax", new ConfigFloat());
			KKAPI.addInstanceSetting("ProductionRateMax", new ConfigFloat());
			KKAPI.addInstanceSetting("ProductionRateCurrent", new ConfigFloat());
			KKAPI.addInstanceSetting("Producing", new ConfigGenericString());

			KKAPI.addModelSetting("OreMax", new ConfigFloat());
			KKAPI.addInstanceSetting("OreCurrent", new ConfigFloat());
			KKAPI.addModelSetting("PrOreMax", new ConfigFloat());
			KKAPI.addInstanceSetting("PrOreCurrent", new ConfigFloat());

			// Science Rep Funds generation
			KKAPI.addModelSetting("DefaultScienceOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("ScienceOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("ScienceOCurrent", new ConfigFloat());
			KKAPI.addModelSetting("DefaultRepOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("RepOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("RepOCurrent", new ConfigFloat());
			KKAPI.addModelSetting("DefaultFundsOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("FundsOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("FundsOCurrent", new ConfigFloat());
				
			// Launch and Recovery
			ConfigFloat flaunchrefund = new ConfigFloat();
			flaunchrefund.setDefaultValue(0f);
			KKAPI.addInstanceSetting("LaunchRefund", flaunchrefund);
			ConfigFloat frecoveryfactor = new ConfigFloat();
			frecoveryfactor.setDefaultValue((float)defaultRecoveryFactor);
			KKAPI.addInstanceSetting("RecoveryFactor", frecoveryfactor);
			ConfigFloat frecoveryrange = new ConfigFloat();
			frecoveryrange.setDefaultValue((float)defaultEffectiveRange);
			KKAPI.addInstanceSetting("RecoveryRange", frecoveryrange);
			
			// Activity logging
			KKAPI.addInstanceSetting("LastCheck", new ConfigFloat());

			#endregion

			SpaceCenterManager.setKSC();

			loadConfig();
			saveConfig();
			
			DontDestroyOnLoad(this);
			loadObjects();
			importCustomInstances();
		}

		#region Game Events

		public void LoadState(ConfigNode configNode)
		{
			if (DebugMode) Debug.Log("KK: LoadState");
			PersistenceUtils.loadPersistenceBackup();
		}

		public void SaveState(ConfigNode configNode)
		{
			PersistenceUtils.savePersistenceBackup();
			if (DebugMode) Debug.Log("KK: SaveState");
		}

		void OnVesselLaunched(ShipConstruct vVessel)
		{
			if (vVessel == null) return;

			PersistenceUtils.savePersistenceBackup();

			if (EditorLogic.fetch.launchSiteName != null)
			{
				StaticObject soTemp = null;

				foreach (StaticObject soThis in KerbalKonstructs.instance.getStaticDB().getAllStatics())
				{
					if (soThis.getSetting("LaunchSiteName") == null)
						continue;
					else
						if ((string)soThis.getSetting("LaunchSiteName") == (string)EditorLogic.fetch.launchSiteName)
						{
							soTemp = soThis;
							break;
						}
				}

				// Don't know why newly created launchsites don't appear without a restart of KSP still.
				// This doesn't seem to help.
				if (soTemp != null)
				{
					if (DebugMode) Debug.Log("KK: Got launchsite gameobject" + (string)EditorLogic.fetch.launchSiteName);
					
					soTemp.SetActiveRecursively(soTemp.gameObject, true);
				}
			}

			if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
			{
				if (EditorLogic.fetch.launchSiteName == null)
				{
					if (DebugMode) Debug.Log("KK: onVesselLaunched launchSiteName was null.");
					return;
				}
				if (EditorLogic.fetch.launchSiteName == "")
				{
					if (DebugMode) Debug.Log("KK: onVesselLaunched launchSiteName was empty.");
					return;
				}

				VesselLaunched = true;
				string sitename = EditorLogic.fetch.launchSiteName;

				float dryCost = 0f;
				float fuelCost = 0f;
				float total = vVessel.GetShipCosts(out dryCost, out fuelCost);

				var cm = CurrencyModifierQuery.RunQuery(TransactionReasons.VesselRollout, total, 0f, 0f);
				total += cm.GetEffectDelta(Currency.Funds);

				double launchcost = total;
				float fRefund = 0f;
				LaunchSiteManager.getSiteLaunchRefund((string)EditorLogic.fetch.launchSiteName, out fRefund);

				if (fRefund < 1) return;

				RefundAmount = (launchcost / 100) * fRefund;
				VesselCost = launchcost - (RefundAmount);
				if (fRefund > 0)
				{
					string sMessage = "This launch normally costs " + launchcost.ToString("#0") +
						" but " + sitename + " provides a " + fRefund + "% refund. \n\nSo " + RefundAmount.ToString("#0") + " funds has been credited to you. \n\nEnjoy and thanks for using " +
						sitename + ". Have a safe flight.";
					MiscUtils.PostMessage("Launch Refund", sMessage, MessageSystemButton.MessageButtonColor.GREEN, MessageSystemButton.ButtonIcons.ALERT);
					Funding.Instance.AddFunds(RefundAmount, TransactionReasons.Cheating);
				}
			}
		}

		void onLevelWasLoaded(GameScenes data)
		{
			bool bTreatBodyAsNullForStatics = true;
			DeletePreviewObject();

			staticDB.ToggleActiveAllStatics(false);

			if (selectedObject != null)
			{
				deselectObject(false, true);
				camControl.active = false;
			}

			if (!data.Equals(GameScenes.FLIGHT))
			{
				DownlinkGUI.DisAudio.Stop();
			}

			if (data.Equals(GameScenes.FLIGHT))
			{
				bTreatBodyAsNullForStatics = false;

				InputLockManager.RemoveControlLock("KKEditorLock");
				InputLockManager.RemoveControlLock("KKEditorLock2");

				PersistenceUtils.savePersistenceBackup();

				if (FlightGlobals.ActiveVessel != null)
				{
					staticDB.ToggleActiveStaticsOnPlanet(FlightGlobals.ActiveVessel.mainBody, true, true);
					currentBody = FlightGlobals.ActiveVessel.mainBody;
					staticDB.onBodyChanged(FlightGlobals.ActiveVessel.mainBody);
					DoHangaredCraftCheck();
				}
				else
				{
					if (DebugMode) Debug.Log("KK: Flight scene load. No activevessel. Activating all statics.");

					staticDB.ToggleActiveAllStatics(true);
				}

				InvokeRepeating("updateCache", 0, 1);
			}
			else
			{
				CancelInvoke("updateCache");
			}

			if (data.Equals(GameScenes.SPACECENTER))
			{
				InputLockManager.RemoveControlLock("KKEditorLock");

				// Tighter control over what statics are active
				bTreatBodyAsNullForStatics = false;
				currentBody = KKAPI.getCelestialBody("Kerbin");
				//staticDB.onBodyChanged(KKAPI.getCelestialBody("Kerbin"));
				//staticDB.onBodyChanged(null);
				staticDB.ToggleActiveStaticsInGroup("KSCUpgrades", true);
				staticDB.ToggleActiveStaticsInGroup("KSCRace", true);
				// *********

				if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
				{
					if (DebugMode) Debug.Log("KK: Load launchsite openclose states for career game");
					PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				}
			}

			if (data.Equals(GameScenes.MAINMENU))
			{
				if (!bImportedCustom)
				{
					bImportedCustom = true;
				}
				// Close all the launchsite objects
				LaunchSiteManager.setAllLaunchsitesClosed();
				atMainMenu = true;
				// CHANGED 19082015
				bTreatBodyAsNullForStatics = false;
				//currentBody = KKAPI.getCelestialBody("Kerbin");
				//staticDB.onBodyChanged(KKAPI.getCelestialBody("Kerbin"));
				iMenuCount = iMenuCount + 1;
				InitialisedFacilities = false;
			}

			if (data.Equals(GameScenes.EDITOR))
			{
				// Prevent abuse if selector left open when switching to from VAB and SPH
				GUI_LaunchSiteSelector.Close();

				// Default selected launchsite when switching between save games
				switch (EditorDriver.editorFacility)
				{
					case EditorFacility.SPH:
						GUI_LaunchSiteSelector.setEditorType(SiteType.SPH);
						if (atMainMenu)
						{
							LaunchSiteManager.setLaunchSite(LaunchSiteManager.runway);
							atMainMenu = false;
						}
						break;
					case EditorFacility.VAB:
						GUI_LaunchSiteSelector.setEditorType(SiteType.VAB);
						if (atMainMenu)
						{
							LaunchSiteManager.setLaunchSite(LaunchSiteManager.launchpad);
							atMainMenu = false;
						}
						break;
					default:
						GUI_LaunchSiteSelector.setEditorType(SiteType.Any);
						break;
				}
			}

			if (bTreatBodyAsNullForStatics) staticDB.onBodyChanged(null);
		}

		void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
		{
			staticDB.ToggleActiveStaticsOnPlanet(data.to, true, true);
			currentBody = data.to;
			staticDB.onBodyChanged(data.to);
			updateCache();
		}

		void OnKSCFacilityUpgraded(Upgradeables.UpgradeableFacility Facility, int iLevel)
		{
		}

		void OnKSCFacilityUpgrading(Upgradeables.UpgradeableFacility Facility, int iLevel)
		{
		}

		void OnUpgradeableObjLevelChange(Upgradeables.UpgradeableObject uObject, int iLevel)
		{
		}

		void OnDoshChanged(double amount, TransactionReasons reason)
		{
			//PersistenceUtils.savePersistenceBackup();
		}

		void OnProcessRecovery(ProtoVessel vessel, MissionRecoveryDialog dialog, float fFloat)
		{
			dRecoveryValue = dialog.fundsEarned;
			PersistenceUtils.savePersistenceBackup();
		}

		void OnVesselRecoveryRequested(Vessel data)
		{
			if (!disableRemoteRecovery)
			{
				if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
				{
					// Change the Space Centre to the nearest open base
					fRecovFactor = 0;
					float fDist = 0f;
					float fRecovFact = 0f;
					float fRecovRng = 0f;
					string sBaseName = "";

					SpaceCenter csc;
					SpaceCenterManager.getClosestSpaceCenter(data.gameObject.transform.position, out csc, out fDist, out fRecovFact, out fRecovRng, out sBaseName);
					SpaceCenter.Instance = csc;

					lastRecoveryBase = sBaseName;
					if (sBaseName == "Runway" || sBaseName == "LaunchPad") lastRecoveryBase = "KSC";
					if (sBaseName == "KSC") lastRecoveryBase = "KSC";

					lastRecoveryDistance = fDist;
					fRecovFactor = fRecovFact;
					fRecovRange = fRecovRng;
				}
			}
		}

		void OnVesselRecovered(ProtoVessel vessel)
		{
			if (!disableRemoteRecovery)
			{
				if (vessel == null)
				{
					if (DebugMode) Debug.Log("KK: onVesselRecovered vessel was null but we don't care");
				}

				if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
				{
					// Put the KSC back as the Space Centre
					if (DebugMode) Debug.Log("KK: Resetting SpaceCenter to KSC");

					SpaceCenter.Instance = SpaceCenterManager.KSC;

					if (lastRecoveryBase != "")
					{
						float fRecoveryDistance = lastRecoveryDistance / 1000;
						float fBaseRecRange = fRecovRange;

						if (lastRecoveryBase == "KSC") fRecovFactor = 100;

						if (fRecovFactor > 0)
						{
							MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.GREEN;

							if (fRecovRange >= lastRecoveryDistance) fRecovFactor = 100;
							float fRefund = 0f;
							LaunchSiteManager.getSiteLaunchRefund((string)lastRecoveryBase, out fRefund);
							if (lastRecoveryDistance < 10000) fRecovFactor = 100 - fRefund;

							if (lastRecoveryBase == "KSC") fRecovFactor = 100;

							string sMessage = "";
							if (fRecovFactor == 100)
							{
								sMessage = "\n\nRecovery value of " + dRecoveryValue.ToString("#0") + " funds is paid in full.";
								dActualRecoveryValue = dRecoveryValue;
							}
							else
							{
								dActualRecoveryValue = (dRecoveryValue / 100) * fRecovFactor;
								sMessage = "\n\nRecovery value of " + dRecoveryValue.ToString("#0") + " funds is reduced to " + dActualRecoveryValue.ToString("#0") + " funds.";

								double dDeduct = dRecoveryValue - dActualRecoveryValue;
								Funding.Instance.AddFunds(-dDeduct, TransactionReasons.Cheating);
							}

							if (!vessel.vesselName.Contains(" Debris"))
							{
								MiscUtils.PostMessage("Recovery Complete", vessel.vesselName +
									" was recovered by " + lastRecoveryBase + ".\n\nDistance to vessel was " +
									fRecoveryDistance.ToString() + " km" +
									"\n\nRecovery Factor of " + lastRecoveryBase + " at this distance is "
									+ fRecovFactor + "%" + sMessage, color, MessageSystemButton.ButtonIcons.ALERT);
							}

							lastRecoveryBase = "";
							fRecovFactor = 0;
							fRecovRange = 0;
							dRecoveryValue = 0;
							dActualRecoveryValue = 0;
						}
					}
				}
			}
		}

		void LateUpdate()
		{
			if (HighLogic.LoadedScene == (GameScenes)5 && (!InitialisedFacilities))
			{
				string saveConfigPath = string.Format("{0}saves/{1}/persistent.sfs", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
				if (File.Exists(saveConfigPath))
				{
					if (DebugMode) Debug.Log("KK: Found persistent.sfs");

					ConfigNode rootNode = ConfigNode.Load(saveConfigPath);
					ConfigNode rootrootNode = rootNode.GetNode("GAME");
					foreach (ConfigNode ins in rootrootNode.GetNodes())
					{
						// Debug.Log("KK: ConfigNode is " + ins);
						if (ins.GetValue("name") == "ScenarioUpgradeableFacilities")
						{
							if (DebugMode) Debug.Log("KK: Found ScenarioUpgradeableFacilities in persistent.sfs");

							foreach (var s in new List<string> { 
							"SpaceCenter/LaunchPad", 
							"SpaceCenter/Runway", 
							"SpaceCenter/VehicleAssemblyBuilding", 
							"SpaceCenter/SpaceplaneHangar", 
							"SpaceCenter/TrackingStation", 
							"SpaceCenter/AstronautComplex", 
							"SpaceCenter/MissionControl", 
							"SpaceCenter/ResearchAndDevelopment",
							"SpaceCenter/Administration",
							"SpaceCenter/FlagPole" })
							{
								ConfigNode n = ins.GetNode(s);
								if (n == null)
								{
									if (DebugMode) Debug.Log("KK: Could not find " + s + " node. Creating node.");

									n = ins.AddNode(s);
									n.AddValue("lvl", 0);
									rootNode.Save(saveConfigPath);
									InitialisedFacilities = true;
								}
							}
							break;
						}
					}

					if (InitialisedFacilities)
					{
						rootNode.Save(saveConfigPath);
						foreach (UpgradeableFacility facility in GameObject.FindObjectsOfType<UpgradeableFacility>())
						{
							facility.SetLevel(0);
						}
					}

					if (DebugMode) Debug.Log("KK: loadCareerObjects");

					loadCareerObjects();
					InitialisedFacilities = true;
				}
			}

			if (camControl.active)
			{
				camControl.updateCamera();
			}

			if (selectedObject != null)
			{
				Vector3 pos = Vector3.zero;
				float alt = 0;
				bool changed = false;

				if (showEditor)
				{
					if (Input.GetKey(KeyCode.W))
					{
						pos.y += GUI_Editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.S))
					{
						pos.y -= GUI_Editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.D))
					{
						pos.x += GUI_Editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.A))
					{
						pos.x -= GUI_Editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.E))
					{
						pos.z += GUI_Editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.Q))
					{
						pos.z -= GUI_Editor.getIncrement();
						changed = true;
					}

					if (Input.GetKey(KeyCode.Equals) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
					{
						alt += (GUI_Editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.KeypadPlus))
					{
						alt += (GUI_Editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.PageUp))
					{
						alt += GUI_Editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.Minus) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
					{
						alt -= (GUI_Editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.KeypadMinus))
					{
						alt -= (GUI_Editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.PageDown))
					{
						alt -= GUI_Editor.getIncrement();
						changed = true;
					}

					if (Input.GetKey(KeyCode.KeypadMultiply))
					{
						GUI_Editor.setIncrement(true, GUI_Editor.getIncrement());
					}
					if (Input.GetKey(KeyCode.KeypadDivide))
					{
						GUI_Editor.setIncrement(false, (GUI_Editor.getIncrement() / 2));
					}
				}

				if (changed)
				{
					pos += (Vector3)selectedObject.getSetting("RadialPosition");
					alt += (float)selectedObject.getSetting("RadiusOffset");
					selectedObject.setSetting("RadialPosition", pos);
					selectedObject.setSetting("RadiusOffset", alt);
					EditorGUI.updateSelection(selectedObject);
				}
			}

			if (Input.GetKeyDown(KeyCode.K) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				ToggleEditor();
			}

		}

		public void ToggleEditor()
		{
			if (selectedObject != null)
				deselectObject(true, true);

			showEditor = !showEditor;

			if (snapTargetInstance != null)
			{
				Color highlightColor = new Color(0, 0, 0, 0);
				snapTargetInstance.HighlightObject(highlightColor);
				snapTargetInstance = null;
			}
		}
		#endregion

		#region GUI Methods

		void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready)
			{
				bool vis;

				if (!disableCustomLaunchsites)
				{
					if (siteSelector == null || !ApplicationLauncher.Instance.Contains(siteSelector, out vis))
						siteSelector = ApplicationLauncher.Instance.AddModApplication(onSiteSelectorOn, onSiteSelectorOff,
							onSiteSelectorOnHover, doNothing, doNothing, doNothing,
							ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
							GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SiteToolbarIcon", false));
				}

				if (flightManager == null || !ApplicationLauncher.Instance.Contains(flightManager, out vis))				
					flightManager = ApplicationLauncher.Instance.AddModApplication(onFlightManagerOn, onFlightManagerOff, 
						doNothing, doNothing, doNothing, doNothing, 
						ApplicationLauncher.AppScenes.FLIGHT, 
						GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/BaseManagerIcon", false));

				if (mapManager == null || !ApplicationLauncher.Instance.Contains(mapManager, out vis))
					mapManager = ApplicationLauncher.Instance.AddModApplication(onMapManagerOn, onMapManagerOff, 
						doNothing, doNothing, doNothing, doNothing, 
						ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.MAPVIEW, 
						GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/BaseManagerIcon", false));

				if (KSCmanager == null || !ApplicationLauncher.Instance.Contains(KSCmanager, out vis))
					KSCmanager = ApplicationLauncher.Instance.AddModApplication(onKSCmanagerOn, onKSCmanagerOff, 
						doNothing, doNothing, doNothing, doNothing, 
						ApplicationLauncher.AppScenes.SPACECENTER, 
						GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/BaseManagerIcon", false));
			}
		}

		void OnGUI()
		{
			GUI.skin = HighLogic.Skin;

			if (HighLogic.LoadedScene == GameScenes.EDITOR)
			{
				if (!disableCustomLaunchsites)
				{
					if (showSiteSelector)
					{
						GUI_LaunchSiteSelector.drawSelector();

						if (showBaseManager)
							GUI_BaseManager.drawBaseManager();
					}
				}
			}

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				if (showKSCmanager)
				{
					GUI_KSCManager.drawKSCManager();

					if (showSettings)
						GUI_Settings.drawKKSettingsGUI();
				}
			}

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				if (!MapView.MapIsEnabled)
				{
					if (showEditor)
					{
						GUI_StaticsEditor.drawEditor();

						if (selectedObject != null)
						{
							if (selectedObject.preview)
							{ }
							else
								GUI_Editor.drawEditor(selectedObject);
						}

						if (showModelInfo)
						{
							GUI_ModelInfo.drawModelInfoGUI(selectedModel);
						}
					}
					else
						DeletePreviewObject();

					if (showFlightManager)
					{
						GUI_FlightManager.drawManager(selectedObject);

						if (showFacilityManager)
						{
							GUI_FacilityManager.drawFacilityManager(selectedObject);
						}
					}

					if (showRacingApp)
					{
						GUI_AirRacingApp.drawRacing();
					}

					if (showDownlink)
					{
						GUI_Downlink.drawDownlink();
					}
					else
					{
						if (DownlinkGUI.DisAudio != null)
							DownlinkGUI.DisAudio.Stop();
					}
				}

				if (showNGS)
				{
					GUI_NGS.drawNGS();
				}
			}
			else
			{
				if (DownlinkGUI.DisAudio != null)
					DownlinkGUI.DisAudio.Stop();

				if (DownlinkGUI.Dis != null)
					DownlinkGUI.Dis.SetActive(false);
			}

			if (MapView.MapIsEnabled)
			{
				if (HighLogic.LoadedScene == GameScenes.EDITOR) return;
				if (HighLogic.LoadedScene == GameScenes.SPACECENTER) return;
				if (HighLogic.LoadedScene == GameScenes.MAINMENU) return;

				if (showMapIconManager)
				{
					GUI_MapIconManager.drawManager();

					if (showFacilityManager)
						GUI_FacilityManager.drawFacilityManager(selectedObject);

					if (showBaseManager)
						GUI_BaseManager.drawBaseManager();

					if (toggleIconsWithBB)
						GUI_MapIconManager.drawIcons();
				}

				if (!toggleIconsWithBB)
					GUI_MapIconManager.drawIcons();
			}
		}

		void onSiteSelectorOnHover()
		{
			string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
			ScreenMessages.PostScreenMessage(hovermessage, 10, 0);
		}

		#endregion

		#region Object Methods

		public void DeletePreviewObject()
		{
			if (selectedModel != null)
			{
				if (ModelInfo.currPreview != null)
				{
					/*InputLockManager.RemoveControlLock("KKShipLock");
					InputLockManager.RemoveControlLock("KKEVALock");
					InputLockManager.RemoveControlLock("KKCamControls");
					InputLockManager.RemoveControlLock("KKCamModes"); */

					//camControl.disable();

					ModelInfo.DestroyPreviewInstance(null);
				}

				//selectedModel = null;
			}
		}

		public void DoHangaredCraftCheck()
		{
			foreach (StaticObject obj in getStaticDB().getAllStatics())
			{
				string sFacType = (string)obj.getSetting("FacilityType");

				if (sFacType != "Hangar") continue;

				if (sFacType == "Hangar")
				{
					PersistenceUtils.loadStaticPersistence(obj);
					string sInStorage = (string)obj.getSetting("InStorage");
					string sInStorage2 = (string)obj.getSetting("TargetID");
					string sInStorage3 = (string)obj.getSetting("TargetType");

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
							bHangarHasStoredCraft1 = "InStorage";
						else
						{
							// Craft no longer exists. Clear this hangar space.
							if (DebugMode) Debug.Log("KK: Craft InStorage no longer exists. Emptying this hangar space.");
							
							obj.setSetting("InStorage", "None");
							PersistenceUtils.saveStaticPersistence(obj);
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
							bHangarHasStoredCraft2 = "TargetID";
						else
						{
							// Craft no longer exists. Clear this hangar space.
							if (DebugMode) Debug.Log("KK: Craft TargetID no longer exists. Emptying this hangar space.");
							
							obj.setSetting("TargetID", "None");
							PersistenceUtils.saveStaticPersistence(obj);
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
							bHangarHasStoredCraft3 = "TargetType";
						else
						{
							// Craft no longer exists. Clear this hangar space.
							if (DebugMode) Debug.Log("KK: Craft TargetType no longer exists. Emptying this hangar space.");
							
							obj.setSetting("TargetType", "None");
							PersistenceUtils.saveStaticPersistence(obj);
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
								sHangarSpace = "InStorage";

							if (vVesselStored.id.ToString() == sInStorage2)
								sHangarSpace = "TargetID";

							if (vVesselStored.id.ToString() == sInStorage3)
								sHangarSpace = "TargetType";

							// If a vessel is hangared
							if (vVesselStored.id.ToString() == sInStorage || vVesselStored.id.ToString() == sInStorage2 || vVesselStored.id.ToString() == sInStorage3)
							{
								if (vVesselStored == FlightGlobals.ActiveVessel)
								{
									// Craft has been taken control
									// Empty the hangar
									if (DebugMode) Debug.Log("KK: Craft has been been taken control of. Emptying " + sHangarSpace + " hangar space.");
									
									obj.setSetting(sHangarSpace, "None");
									PersistenceUtils.saveStaticPersistence(obj);
								}
								else
								{
									if (DebugMode) Debug.Log("KK: Hiding vessel " + vVesselStored.vesselName + ". It is in the hangar.");
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

		public void updateCache()
		{
			if (HighLogic.LoadedSceneIsGame)
			{
				Vector3 playerPos = Vector3.zero;
				if (selectedObject != null)
				{
					playerPos = selectedObject.gameObject.transform.position;

					if (DebugMode)
						Debug.Log("KK: updateCache using selectedObject as playerPos");
				}
				else if (FlightGlobals.ActiveVessel != null)
				{
					playerPos = FlightGlobals.ActiveVessel.transform.position;

					if (DebugMode)
						Debug.Log("KK: updateCache using ActiveVessel as playerPos" + FlightGlobals.ActiveVessel.vesselName);
				}
				else if (Camera.main != null)
				{
					playerPos = Camera.main.transform.position;

					if (DebugMode)
						Debug.Log("KK: updateCache using Camera.main as playerPos");
				}
				else
				{
					if (DebugMode)
						Debug.Log("KK: KerbalKonstructs.updateCache could not determine playerPos. All hell now happens.");
				}

				staticDB.updateCache(playerPos);
			}
		}

		public void loadInstances(ConfigNode confconfig, StaticModel model, bool bSecondPass = false)
		{
			if (model == null)
			{
				Debug.Log("KK: Attempting to loadInstances for a null model. Check your model and config.");
				return;
			}

			if (confconfig == null)
			{
				Debug.Log("KK: Attempting to loadInstances for a null ConfigNode. Check your model and config.");
				return;
			}

			foreach (ConfigNode ins in confconfig.GetNodes("Instances"))
			{
				StaticObject obj = new StaticObject();
				obj.model = model;
				
				obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));

				if (obj.gameObject == null)
				{
					Debug.Log("KK: Could not find " + model.getSetting("mesh") + ".mu! Did the modder forget to include it or did you actually install it?");
					continue;
				}

				obj.settings = KKAPI.loadConfig(ins, KKAPI.getInstanceSettings());

				if (obj.settings == null)
				{
					Debug.Log("KK: Error loading instances for " + model.getSetting("mesh") + ".mu! Check your model and config.");
					continue;
				}

				if (bSecondPass)
				{
					Vector3 secondInstanceKey = (Vector3)obj.getSetting("RadialPosition");
					bool bSpaceOccupied = false;

					foreach (StaticObject soThis in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						Vector3 firstInstanceKey = (Vector3)soThis.getSetting("RadialPosition");

						if (firstInstanceKey == secondInstanceKey)
						{
							string sThisMesh = (string)soThis.model.getSetting("mesh");
							string sThatMesh = (string)obj.model.getSetting("mesh");

							if (DebugMode) 
								Debug.Log("KK: Custom instance has a RadialPosition that already has an instance."
								+ sThisMesh + ":"
								+ (string)soThis.getSetting("Group") + ":" + firstInstanceKey.ToString() + "|"
								+ sThatMesh + ":"
								+ (string)obj.getSetting("Group") + ":" + secondInstanceKey.ToString());

							if (sThisMesh == sThatMesh)
							{
								float fThisOffset = (float)soThis.getSetting("RadiusOffset");
								float fThatOffset = (float)obj.getSetting("RadiusOffset");
								float fThisRotation = (float)soThis.getSetting("RotationAngle");
								float fThatRotation = (float)obj.getSetting("RotationAngle");

								if ((fThisOffset == fThatOffset) && (fThisRotation == fThatRotation))
								{
									bSpaceOccupied = true;
									break;
								}
								else
								{
									if (DebugMode) Debug.Log("KK: Different rotation or offset. Allowing. Could be a feature of the same model such as a doorway being used. Will cause z tearing probably.");
								}
							}
							else
							{
								if (DebugMode) Debug.Log("KK: Different models. Allowing. Could be a terrain foundation or integrator.");
							}
						}
					}

					if (bSpaceOccupied)
					{
						Debug.Log("KK: Attempted to import identical custom instance to same RadialPosition as existing instance. Skipped. Check for duplicate custom statics you have installed. Did you export the custom instances to make a pack? If not, ask the mod-makers if they are duplicating the same stuff as each other.");
						continue;
					}
				}

				if (!obj.settings.ContainsKey("LaunchPadTransform") && obj.settings.ContainsKey("LaunchSiteName"))
				{
					if (model.settings.Keys.Contains("DefaultLaunchPadTransform"))
					{
						obj.settings.Add("LaunchPadTransform", model.getSetting("DefaultLaunchPadTransform"));
					}
					else
					{
						Debug.Log("KK: Launch site is missing a transform. Defaulting to " + obj.getSetting("LaunchSiteName") + "_spawn...");

						if (obj.gameObject.transform.Find(obj.getSetting("LaunchSiteName") + "_spawn") != null)
						{
							obj.settings.Add("LaunchPadTransform", obj.getSetting("LaunchSiteName") + "_spawn");
						}
						else
						{
							Debug.Log("KK: FAILED: " + obj.getSetting("LaunchSiteName") + "_spawn does not exist! Attempting to use any transform with _spawn in the name.");
							Transform lastResort = obj.gameObject.transform.Cast<Transform>().FirstOrDefault(trans => trans.name.EndsWith("_spawn"));

							if (lastResort != null)
							{
								Debug.Log("KK: Using " + lastResort.name + " as launchpad transform");
								obj.settings.Add("LaunchPadTransform", lastResort.name);
							}
							else
							{
								Debug.Log("KK: All attempts at finding a launchpad transform have failed (╯°□°）╯︵ ┻━┻ This static isn't configured for KK properly. Tell the modder.");
							}
						}
					}
				}

				staticDB.addStatic(obj);
				obj.spawnObject(false, false);

				if (obj.settings.ContainsKey("LaunchPadTransform") && obj.settings.ContainsKey("LaunchSiteName"))
					LaunchSiteManager.createLaunchSite(obj);
			}
		}

		public void loadCareerObjects()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");

			foreach (UrlDir.UrlConfig conf in configs)
			{
				StaticModel model = new StaticModel();
				model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
				model.config = conf.url;
				model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
				model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());

				string sConfigName = (Path.GetFileName(conf.url)) + ".cfg";

				savedObjectPath = string.Format("{0}saves/{1}/{2}", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder, sConfigName);

				if (!File.Exists(savedObjectPath))
					continue;

				ConfigNode CareerConfig = ConfigNode.Load(savedObjectPath);
				ConfigNode CareerConfigRoot = CareerConfig.GetNode("STATIC");

				loadInstances(CareerConfigRoot, model, true);
			}
		}

		public void importCustomInstances()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");

			foreach (UrlDir.UrlConfig conf in configs)
			{
				StaticModel model = new StaticModel();
				model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
				model.config = conf.url;
				model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
				model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());

				if (!model.settings.ContainsKey("pointername"))
					continue;

				string sPointerName = (string)model.getSetting("pointername");

				foreach (StaticModel model2 in staticDB.getModels())
				{
					ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model2.config);

					if ((string)modelConfig.GetValue("name") == sPointerName)
					{
						loadInstances(conf.config, model2, true);
						break;
					}
				}
			}
		}

		public void loadObjects()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");
			
			foreach(UrlDir.UrlConfig conf in configs)
			{
				StaticModel model = new StaticModel();
				model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
				model.config = conf.url;
				model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
				model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());

				if (model.settings.ContainsKey("LocalToSave"))
				{
					if ((string)model.getSetting("LocalToSave") == "True")
						continue;
					// Ignore it for second pass
				}

				if (model.settings.ContainsKey("pointername"))
				{
					if ((string)model.getSetting("pointername") != "None")
						continue;
					// Ignore it for second pass
				}

				foreach (ConfigNode ins in conf.config.GetNodes("MODULE"))
				{
					StaticModule module = new StaticModule();
					foreach (ConfigNode.Value value in ins.values)
					{
						switch (value.name)
						{
							case "namespace":
								module.moduleNamespace = value.value;
								break;
							case "name":
								module.moduleClassname = value.value;
								break;
							default:
								module.moduleFields.Add(value.name, value.value);
								break;
						}
					}
					model.modules.Add(module);
				}

				loadInstances(conf.config, model);
				
				staticDB.registerModel(model);
			}
		}

		public void saveObjects()
		{
			foreach (StaticModel model in staticDB.getModels())
			{
				ConfigNode staticNode = new ConfigNode("STATIC");
				ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);
							
				modelConfig.RemoveNodes("Instances");

				foreach (StaticObject obj in staticDB.getObjectsFromModel(model))
				{
					ConfigNode inst = new ConfigNode("Instances");
					foreach (KeyValuePair<string, object> setting in obj.settings)
					{
						inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
					}
					modelConfig.nodes.Add(inst);
				}

				staticNode.AddNode(modelConfig);
				staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + model.configPath, "Generated by Kerbal Konstructs");
			}
		}

		public void exportCustomInstances(string sPackName = "MyStaticPack", string sBaseName = "All", string sGroup = "", Boolean bLocal = false)
		{
			bool HasCustom = false;
			string sBase = "";

			if (sGroup != "") sBase = sGroup;
			else
				sBase = sBaseName;

			foreach (StaticModel model in staticDB.getModels())
			{
				HasCustom = false;
				ConfigNode staticNode = new ConfigNode("STATIC");
				ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

				modelConfig.RemoveNodes("Instances");

				foreach (StaticObject obj in staticDB.getObjectsFromModel(model))
				{
					string sCustom = (string)obj.getSetting("CustomInstance");
					string sInstGroup = (string)obj.getSetting("Group");

					if (sGroup != "")
					{
						if (sInstGroup != sGroup)
						{
							sInstGroup = "";
							continue;
						}
					}

					if (KerbalKonstructs.instance.DevMode)
					{
						sCustom = "True";
						obj.setSetting("CustomInstance", "True");
					}

					if (sCustom == "True")
					{
						HasCustom = true;
						ConfigNode inst = new ConfigNode("Instances");
						foreach (KeyValuePair<string, object> setting in obj.settings)
						{
							inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
						}
						modelConfig.nodes.Add(inst);
					}
				}

				if (HasCustom)
				{
					string sModelName = modelConfig.GetValue("name");
					modelConfig.AddValue("pointername", sModelName);

					modelConfig.RemoveValue("name");
					modelConfig.AddValue("name", sPackName + "_" + sBase + "_" + sModelName);
					
					staticNode.AddNode(modelConfig);

					Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + sBase + "/" + model.path);
					staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + sBase + "/" + model.configPath, "Exported custom instances by Kerbal Konstructs");
				}
			}
		}

		public void deleteObject(StaticObject obj)
		{
			if (selectedObject == obj)
				deselectObject(true, false);

			InputLockManager.RemoveControlLock("KKShipLock");
			InputLockManager.RemoveControlLock("KKEVALock");
			InputLockManager.RemoveControlLock("KKCamControls");
			InputLockManager.RemoveControlLock("KKCamModes");

			if (camControl.active) camControl.disable();

			if (snapTargetInstance == obj)
				snapTargetInstance = null;

			if (DebugMode) Debug.Log("KK: deleteObject");

			staticDB.deleteObject(obj);
		}

		public void setSnapTarget(StaticObject obj)
		{
			snapTargetInstance = obj;
		}

		public void selectObject(StaticObject obj, bool isEditing, bool bFocus, bool bPreview)
		{
			if (bFocus)
			{
				InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
				InputLockManager.SetControlLock(ControlTypes.EVA_INPUT, "KKEVALock");
				InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");
				InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "KKCamModes");

				if (selectedObject != null)
					deselectObject(true, true);

				if (camControl.active)
					camControl.disable();

				camControl.enable(obj.gameObject);
			}
			else
			{
				if (selectedObject != null)
					deselectObject(true, true);
			}
			
			//obj.preview = bPreview;
			if (DebugMode) Debug.Log("KK: obj.preview is " + obj.preview.ToString());

			selectedObject = obj;
			if (DebugMode) Debug.Log("KK: selectedObject.preview is " + selectedObject.preview.ToString());
			
			if (isEditing)
			{
				selectedObject.editing = true;
				selectedObject.ToggleAllColliders(false);
			}
		}

		public void deselectObject(Boolean disableCam, Boolean enableColliders)
		{
			if (selectedObject != null)
			{
				selectedObject.editing = false;
				if (enableColliders) selectedObject.ToggleAllColliders(true);

				Color highlightColor = new Color(0, 0, 0, 0);
				selectedObject.HighlightObject(highlightColor);

				selectedObject = null;
			}

			InputLockManager.RemoveControlLock("KKShipLock");
			InputLockManager.RemoveControlLock("KKEVALock");
			InputLockManager.RemoveControlLock("KKCamControls");
			InputLockManager.RemoveControlLock("KKCamModes");

			if (disableCam)
				camControl.disable();
		}

		#endregion

		#region App Button Toggles
		void onKKSettingsOn()
		{
			showSettings = true;
		}

		void onKKSettingsOff()
		{
			showSettings = false;
		}

		void onKSCmanagerOn()
		{
			showKSCmanager = true;
		}

		void onSiteSelectorOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showSiteSelector = true;
		}

		void onFlightManagerOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showFlightManager = true;
		}

		void onMapManagerOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showMapIconManager = true;
		}

		void onKSCmanagerOff()
		{
			showKSCmanager = false;
		}

		void onSiteSelectorOff()
		{
			showSiteSelector = false;
			InputLockManager.RemoveControlLock("KKEditorLock");
			PersistenceFile<LaunchSite>.SaveList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
		}

		void onFlightManagerOff()
		{
			showFlightManager = false;
			if (selectedObject != null)
				deselectObject(true, true);
		}

		void onMapManagerOff()
		{
			showMapIconManager = false;
		}
		#endregion

		#region Get Methods
		public StaticDatabase getStaticDB()
		{
			return staticDB;
		}

		public CelestialBody getCurrentBody()
		{
			return currentBody;
		}
		#endregion

		#region Config Methods
		public bool loadConfig()
		{
			string saveConfigPath = installDir + "/KerbalKonstructs.cfg";

			ConfigNode cfg = ConfigNode.Load(saveConfigPath);
			if (cfg != null)
			{
				foreach (FieldInfo f in GetType().GetFields())
				{
					if (Attribute.IsDefined(f, typeof(KSPField)))
					{
						if(cfg.HasValue(f.Name))
							f.SetValue(this, Convert.ChangeType(cfg.GetValue(f.Name), f.FieldType));
					}
					else
					{
						if (DebugMode) Debug.Log("KK: Attribute not defined as KSPField. This is harmless.");
						
						continue;
					}
				}
				return true;

			}
			return false;

		}

		public void saveConfig()
		{
			ConfigNode cfg = new ConfigNode();

			foreach (FieldInfo f in GetType().GetFields())
			{
				if (Attribute.IsDefined(f, typeof(KSPField)))
				{
					cfg.AddValue(f.Name, f.GetValue(this));
				}
			}

			Directory.CreateDirectory(installDir);
			string saveConfigPath = installDir + "/KerbalKonstructs.cfg";
			cfg.Save(saveConfigPath, "Kerbal Konstructs Settings");
		}
		#endregion

		#region Utilities
		void doNothing()
		{
			// wow so robust
		}


		#endregion
	}
}

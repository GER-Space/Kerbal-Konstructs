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
		#endregion

		#region GUI Windows
		private EditorGUI editor = new EditorGUI();
		private NavGuidanceSystem eNGS = new NavGuidanceSystem();
		private BaseBossFlight manager = new BaseBossFlight();
		private EditorGUI facilitymanager = new EditorGUI();
		private LaunchSiteSelectorGUI selector = new LaunchSiteSelectorGUI();
		private MapIconManager mapIconManager = new MapIconManager();
		private FacilityManager GUIFacilityManager = new FacilityManager();
		private KSCManager KSCFacilityManager = new KSCManager();
		private AirRacing airracing = new AirRacing();
		#endregion

		#region Show Toggles
		private Boolean showEditor = false;
		private Boolean showSelector = false;
		public Boolean showBaseManager = false;
		private Boolean showMapManager = false;
		private Boolean showKSCmanager = false;
		public Boolean showNGS = false;
		public Boolean showRacing = false;
		#endregion

		#region App Buttons
		private ApplicationLauncherButton siteSelector;
		private ApplicationLauncherButton baseManager;
		private ApplicationLauncherButton mapManager;
		private ApplicationLauncherButton KSCmanager;
		#endregion

		#region Configurable Variables
		[KSPField]
		public Boolean launchFromAnySite = false;
		[KSPField]
		public Boolean disableCareerStrategyLayer = false;
		[KSPField]
		public Boolean enableATC = true;
		[KSPField]
		public Boolean enableNGS = true;
		[KSPField]
		public Double facilityUseRange = 100;
		[KSPField]
		public Boolean mapShowOpen = true;
		[KSPField]
		public Boolean mapShowClosed = false;
		[KSPField]
		public Boolean mapShowOpenT = true;
		[KSPField]
		public Boolean mapShowClosedT = false;
		[KSPField]
		public Boolean mapShowHelipads = true;
		[KSPField]
		public Boolean mapShowRunways = true;
		[KSPField]
		public Boolean mapShowRocketbases = true;
		[KSPField]
		public Boolean mapShowOther = false;
		[KSPField]
		public Double defaultRecoveryFactor = 50;
		[KSPField]
		public Double defaultEffectiveRange = 100000;
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
			#endregion

			#region Instance API
			// Position
			KKAPI.addInstanceSetting("CelestialBody", new ConfigCelestialBody());
			KKAPI.addInstanceSetting("RadialPosition", new ConfigVector3());
			KKAPI.addInstanceSetting("Orientation", new ConfigVector3());
			KKAPI.addInstanceSetting("RadiusOffset", new ConfigFloat());
			KKAPI.addInstanceSetting("RotationAngle", new ConfigFloat());
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

			// Career Mode Strategy Models
			ConfigGenericString facilityrole = new ConfigGenericString();
			facilityrole.setDefaultValue("None");
			KKAPI.addModelSetting("DefaultFacilityType", facilityrole);

			ConfigGenericString instfacilityrole = new ConfigGenericString();
			instfacilityrole.setDefaultValue("None");
			KKAPI.addInstanceSetting("FacilityType", instfacilityrole);

			// Local to a specific save - constructed in a specific save-game
			ConfigGenericString LocalToSave = new ConfigGenericString();
			LocalToSave.setDefaultValue("False");
			KKAPI.addInstanceSetting("LocalToSave", LocalToSave);
				
			// Custom instances - added or modified by player with the editor
			ConfigGenericString CustomInstance = new ConfigGenericString();
			CustomInstance.setDefaultValue("False");
			KKAPI.addInstanceSetting("CustomInstance", CustomInstance);

			// Facility Ratings

			// Tracking station max short range in m
			ConfigFloat ftrackingshort = new ConfigFloat();
			ftrackingshort.setDefaultValue(85000f);
			KKAPI.addInstanceSetting("TrackingShort", ftrackingshort);

			// Target Type and ID
			KKAPI.addInstanceSetting("TargetType", new ConfigGenericString());
			KKAPI.addInstanceSetting("TargetID", new ConfigGenericString());

			//XP
			KKAPI.addInstanceSetting("FacilityXP", new ConfigFloat());

			// Staff
			KKAPI.addModelSetting("StaffMax", new ConfigFloat());
			KKAPI.addInstanceSetting("StaffCurrent", new ConfigFloat());

			// Fueling
			KKAPI.addModelSetting("LqFMax", new ConfigFloat());
			KKAPI.addInstanceSetting("LqFCurrent", new ConfigFloat());
			KKAPI.addModelSetting("OxFMax", new ConfigFloat());
			KKAPI.addInstanceSetting("OxFCurrent", new ConfigFloat());
			KKAPI.addModelSetting("MoFMax", new ConfigFloat());
			KKAPI.addInstanceSetting("MoFCurrent", new ConfigFloat());

			// Science Rep Funds generation
			KKAPI.addModelSetting("ScienceOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("ScienceOCurrent", new ConfigFloat());
			KKAPI.addModelSetting("RepOMax", new ConfigFloat());
			KKAPI.addInstanceSetting("RepOCurrent", new ConfigFloat());
			KKAPI.addModelSetting("FundsOMax", new ConfigFloat());
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
				
			KKAPI.addInstanceSetting("LastCheck", new ConfigFloat());

			#endregion

			SpaceCenterManager.setKSC();

			loadConfig();
			saveConfig();
			
			DontDestroyOnLoad(this);
			loadObjects();
		}

		#region Game Events

		public void LoadState(ConfigNode configNode)
		{
			// Debug.Log("KK: LoadState");
		}

		public void SaveState(ConfigNode configNode)
		{
			// Debug.Log("KK: SaveState");
		}

		void OnVesselLaunched(ShipConstruct vVessel)
		{
			if (vVessel == null) return;


			if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
			{
				if (EditorLogic.fetch.launchSiteName == null)
				{
					Debug.Log("KK: onVesselLaunched launchSiteName was null.");
					return;
				}
				if (EditorLogic.fetch.launchSiteName == "")
				{
					Debug.Log("KK: onVesselLaunched launchSiteName was empty.");
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
			bool bNullBody = true;

			if (selectedObject != null)
			{
				deselectObject(false);
				camControl.active = false;
			}

			if (data.Equals(GameScenes.FLIGHT))
			{
				InputLockManager.RemoveControlLock("KKEditorLock");
				InvokeRepeating("updateCache", 0, 1);
				bNullBody = false;
			}
			else
			{
				CancelInvoke("updateCache");
			}

			if (data.Equals(GameScenes.SPACECENTER))
			{
				InputLockManager.RemoveControlLock("KKEditorLock");
				currentBody = KKAPI.getCelestialBody("Kerbin");
				staticDB.onBodyChanged(KKAPI.getCelestialBody("Kerbin"));
				updateCache();

				if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
				{
					Debug.Log("KK: Load launchsite openclose states for career game");
					PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				}

				bNullBody = false;
			}

			if (data.Equals(GameScenes.MAINMENU))
			{
				// Close all the launchsite objects
				LaunchSiteManager.setAllLaunchsitesClosed();
				atMainMenu = true;
				bNullBody = false;
				iMenuCount = iMenuCount + 1;
				InitialisedFacilities = false;
			}

			if (data.Equals(GameScenes.EDITOR))
			{
				// Prevent abuse if selector left open when switching to from VAB and SPH
				selector.Close();

				// Default selected launchsite when switching between save games
				switch (EditorDriver.editorFacility)
				{
					case EditorFacility.SPH:
						selector.setEditorType(SiteType.SPH);
						if (atMainMenu)
						{
							LaunchSiteManager.setLaunchSite(LaunchSiteManager.runway);
							atMainMenu = false;
						}
						break;
					case EditorFacility.VAB:
						selector.setEditorType(SiteType.VAB);
						if (atMainMenu)
						{
							LaunchSiteManager.setLaunchSite(LaunchSiteManager.launchpad);
							atMainMenu = false;
						}
						break;
					default:
						selector.setEditorType(SiteType.Any);
						break;
				}
			}

			if (bNullBody) staticDB.onBodyChanged(null);
		}

		void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
		{
			currentBody = data.to;
			staticDB.onBodyChanged(data.to);
		}

		void OnKSCFacilityUpgraded(Upgradeables.UpgradeableFacility Facility, int iLevel)
		{
			// Debug.Log("KK: FacilityUpgraded " + Facility + " " + iLevel);
		}

		void OnKSCFacilityUpgrading(Upgradeables.UpgradeableFacility Facility, int iLevel)
		{
			// Debug.Log("KK: FacilityUpgrading " + Facility + " " + iLevel);
		}

		void OnUpgradeableObjLevelChange(Upgradeables.UpgradeableObject uObject, int iLevel)
		{
			// Debug.Log("KK: UpgradeableObjLevelChange " + uObject + " " + iLevel);
		}

		void OnDoshChanged(double amount, TransactionReasons reason)
		{
			// Debug.Log("KK: Funds changed - " + amount + " because " + reason);
			/* if (CareerStrategyEnabled(HighLogic.CurrentGame))
			{
				if (reason == (TransactionReasons)16)
				{
					string smessage = "Vessel launched for "+ amount.ToString();
					ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;
					ScreenMessages.PostScreenMessage(smessage, 60, smsStyle);
					VesselCost = amount;
					VesselLaunched = true;
				}
			} */
		}

		void OnProcessRecovery(ProtoVessel vessel, MissionRecoveryDialog dialog, float fFloat)
		{
			dRecoveryValue = dialog.fundsEarned;
		}

		void OnVesselRecoveryRequested(Vessel data)
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

		void OnVesselRecovered(ProtoVessel vessel)
		{
			if (vessel == null)
				Debug.Log("KK: onVesselRecovered vessel was null but we don't care");

			if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
			{
				// Put the KSC back as the Space Centre
				// Debug.Log("KK: Resetting SpaceCenter to KSC");
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

		void LateUpdate()
		{
			if (HighLogic.LoadedScene == (GameScenes)5 && (!InitialisedFacilities))
			{
				string saveConfigPath = string.Format("{0}saves/{1}/persistent.sfs", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
				if (File.Exists(saveConfigPath))
				{
					//Debug.Log("KK: Found persistent.sfs");
					ConfigNode rootNode = ConfigNode.Load(saveConfigPath);
					ConfigNode rootrootNode = rootNode.GetNode("GAME");
					foreach (ConfigNode ins in rootrootNode.GetNodes())
					{
						// Debug.Log("KK: ConfigNode is " + ins);
						if (ins.GetValue("name") == "ScenarioUpgradeableFacilities")
						{
							//Debug.Log("KK: Found ScenarioUpgradeableFacilities in persistent.sfs");

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
									Debug.Log("KK: Could not find " + s + " node. Creating node.");
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

					// Debug.Log("KK: loadCareerObjects");
					loadCareerObjects();

					InitialisedFacilities = true;
					// Debug.Log("KK: InitialisedFacilities check complete");
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
						pos.y += editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.S))
					{
						pos.y -= editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.D))
					{
						pos.x += editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.A))
					{
						pos.x -= editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.E))
					{
						pos.z += editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.Q))
					{
						pos.z -= editor.getIncrement();
						changed = true;
					}

					// ASH 08112014 Fix clashing with camera zooming
					if (Input.GetKey(KeyCode.Equals) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
					{
						alt += (editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.KeypadPlus))
					{
						alt += (editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.PageUp))
					{
						alt += editor.getIncrement();
						changed = true;
					}
					if (Input.GetKey(KeyCode.Minus) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
					{
						alt -= (editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.KeypadMinus))
					{
						alt -= (editor.getIncrement()) / 10;
						changed = true;
					}
					if (Input.GetKey(KeyCode.PageDown))
					{
						alt -= editor.getIncrement();
						changed = true;
					}
					// Added increment keys
					if (Input.GetKey(KeyCode.KeypadMultiply))
					{
						editor.setIncrement(true, editor.getIncrement());
					}
					if (Input.GetKey(KeyCode.KeypadDivide))
					{
						editor.setIncrement(false, (editor.getIncrement() / 2));
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
				if (selectedObject != null)
					deselectObject();

				showEditor = !showEditor;

				if (snapTargetInstance != null)
				{
					Color highlightColor = new Color(0, 0, 0, 0);
					snapTargetInstance.HighlightObject(highlightColor);
					snapTargetInstance = null;
				}
			}

		}
		#endregion

		#region GUI Methods
		void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready)
			{
				bool vis;
				
				if (siteSelector == null || !ApplicationLauncher.Instance.Contains(siteSelector, out vis))				
					siteSelector = ApplicationLauncher.Instance.AddModApplication(onSiteSelectorOn, onSiteSelectorOff, onSiteSelectorOnHover, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SiteToolbarIcon", false));

				if (baseManager == null || !ApplicationLauncher.Instance.Contains(baseManager, out vis))				
					baseManager = ApplicationLauncher.Instance.AddModApplication(onBaseManagerOn, onBaseManagerOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.FLIGHT, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/BaseManagerIcon", false));

				if (mapManager == null || !ApplicationLauncher.Instance.Contains(mapManager, out vis))
					mapManager = ApplicationLauncher.Instance.AddModApplication(onMapManagerOn, onMapManagerOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.MAPVIEW, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/BaseManagerIcon", false));

				if (KSCmanager == null || !ApplicationLauncher.Instance.Contains(KSCmanager, out vis))
					KSCmanager = ApplicationLauncher.Instance.AddModApplication(onKSCmanagerOn, onKSCmanagerOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.SPACECENTER, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/BaseManagerIcon", false));
			}
		}

		void OnGUI()
		{
			GUI.skin = HighLogic.Skin;

			if (showSelector && (HighLogic.LoadedScene.Equals(GameScenes.EDITOR)))//Disable selector when not in the editor
				selector.drawSelector();

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				if (showKSCmanager)
				{
					KSCFacilityManager.drawKSCManager();
				}
			}

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				if (!MapView.MapIsEnabled)
				{
					if (showEditor)
					{
						editor.drawEditor(selectedObject);
					}

					if (showBaseManager)
					{
						manager.drawManager(selectedObject);
					}
				}

				if (showNGS)
				{
					eNGS.drawNGS();
				}

				if (showRacing)
				{
					airracing.drawRacing();
				}
			}

			if (MapView.MapIsEnabled)
			{
				if (HighLogic.LoadedScene == GameScenes.EDITOR) return;
				if (HighLogic.LoadedScene == GameScenes.SPACECENTER) return;
				if (HighLogic.LoadedScene == GameScenes.MAINMENU) return;

				if (showMapManager)
					GUIFacilityManager.drawManager();

				mapIconManager.drawIcons();
			}
		}

		void onSiteSelectorOnHover()
		{
			string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
			ScreenMessages.PostScreenMessage(hovermessage, 10, 0);
		}
		#endregion

		#region Object Methods
		public void updateCache()
		{
			if (HighLogic.LoadedSceneIsGame)
			{
				Vector3 playerPos = Vector3.zero;
				if (selectedObject != null)
				{
					playerPos = selectedObject.gameObject.transform.position;
				}
				else if (FlightGlobals.ActiveVessel != null)
				{
					playerPos = FlightGlobals.ActiveVessel.transform.position;
				}
				else if (Camera.main != null)
				{
					playerPos = Camera.main.transform.position;
				}
				else
				{
					Debug.Log("KK: KerbalKonstructs.updateCache could not determine playerPos. All hell now happens.");
				}

				staticDB.updateCache(playerPos);
			}
		}

		public void loadInstances(ConfigNode confconfig, StaticModel model)
		{
			foreach (ConfigNode ins in confconfig.GetNodes("Instances"))
			{
				StaticObject obj = new StaticObject();
				obj.model = model;
				obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));

				if (obj.gameObject == null)
				{
					Debug.Log("KK: Could not find " + model.getSetting("mesh") + ".mu! Did the mod forget to include it or did you actually install it?");
					continue;
				}

				obj.settings = KKAPI.loadConfig(ins, KKAPI.getInstanceSettings());

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
				obj.spawnObject(false);
				if (obj.settings.ContainsKey("LaunchSiteName"))
				{
					LaunchSiteManager.createLaunchSite(obj);
				}
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
				{
					//  Debug.Log("KK: No " + savedObjectPath);
					continue;
				}

				// Debug.Log("KK: Career has object instances in " + savedObjectPath);

				ConfigNode CareerConfig = ConfigNode.Load(savedObjectPath);
				ConfigNode CareerConfigRoot = CareerConfig.GetNode("STATIC");

				loadInstances(CareerConfigRoot, model);
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
					{
						// Debug.Log("KK: Static Config is local to save. Skipping model and its instances.");
						continue;
					}
				}

				foreach (ConfigNode ins in conf.config.GetNodes("MODULE"))
				{
					// Debug.Log("KK: Found module: "+ins.name+" in "+conf.name);
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
					// Debug.Log("KK: Adding module");
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

				// Debug.Log("KK: model.config " + model.config);
							
				modelConfig.RemoveNodes("Instances");

				foreach (StaticObject obj in staticDB.getObjectsFromModel(model))
				{
					ConfigNode inst = new ConfigNode("Instances");
					foreach (KeyValuePair<string, object> setting in obj.settings)
					{
						// Debug.Log("KK: setting.Key " + setting.Key);
						inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
					}
					modelConfig.nodes.Add(inst);
				}

				staticNode.AddNode(modelConfig);
				staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + model.configPath, "Generated by Kerbal Konstructs");
			}
		}

		public void exportCustomInstances()
		{
			bool HasCustom;

			foreach (StaticModel model in staticDB.getModels())
			{
				HasCustom = false;
				ConfigNode staticNode = new ConfigNode("STATIC");
				ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

				modelConfig.RemoveNodes("Instances");

				foreach (StaticObject obj in staticDB.getObjectsFromModel(model))
				{
					string sCustom = (string)obj.getSetting("CustomInstance");

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
					staticNode.AddNode(modelConfig);
					Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/medsouz/KerbalKonstructs/ExportedInstances/" + model.path);
					staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/medsouz/KerbalKonstructs/ExportedInstances/" + model.configPath + ".exp", "Exported custom instances by Kerbal Konstructs");
				}
			}
		}

		public void deleteObject(StaticObject obj)
		{
			if (selectedObject == obj)
			{
				deselectObject();
			}

			if (snapTargetInstance == obj) snapTargetInstance = null;

			staticDB.deleteObject(obj);
		}

		public void setSnapTarget(StaticObject obj)
		{
			snapTargetInstance = obj;
		}

		public void selectObject(StaticObject obj, bool isEditing = true)
		{
			InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
			InputLockManager.SetControlLock(ControlTypes.EVA_INPUT, "KKEVALock");
			InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");
			InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "KKCamModes");
			
			if (selectedObject != null)
				deselectObject();
			
			selectedObject = obj;
			
			if (isEditing)
			{
				selectedObject.editing = true;
				selectedObject.ToggleAllColliders(false);
			}
			
			if(camControl.active)
				camControl.disable();
			
			camControl.enable(obj.gameObject);
		}

		public void deselectObject(Boolean disableCam = true)
		{
			if (selectedObject != null)
			{
				selectedObject.editing = false;
				selectedObject.ToggleAllColliders(true);

				/*Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
				List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();
			
				foreach (GameObject collider in colliderList)
				{
					collider.collider.enabled = true;
				}*/

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

		#region GUI Toggles
		void onKSCmanagerOn()
		{
			showKSCmanager = true;
		}

		void onSiteSelectorOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showSelector = true;
		}

		void onBaseManagerOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showBaseManager = true;
		}

		void onMapManagerOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showMapManager = true;
		}

		void onKSCmanagerOff()
		{
			showKSCmanager = false;
		}

		void onSiteSelectorOff()
		{
			showSelector = false;
			InputLockManager.RemoveControlLock("KKEditorLock");
			PersistenceFile<LaunchSite>.SaveList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
		}

		void onBaseManagerOff()
		{
			showBaseManager = false;
			if (selectedObject != null)
				deselectObject();
		}

		void onMapManagerOff()
		{
			showMapManager = false;
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
			ConfigNode cfg = ConfigNode.Load(installDir + @"\KerbalKonstructs.cfg".Replace('/', '\\'));
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
						Debug.Log("KK: Attribute not defined!");
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
			cfg.Save(installDir + "/KerbalKonstructs.cfg", "Kerbal Konstructs");
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

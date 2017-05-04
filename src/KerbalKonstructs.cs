using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs.KerbinNations;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System.Reflection;
using KerbalKonstructs.API;
using KerbalKonstructs.API.Config;
using KSP.UI.Screens;
using Upgradeables;
using KerbalKonstructs.Addons;
using KerbalKonstructs.Modules;

using Debug = UnityEngine.Debug;

namespace KerbalKonstructs
{
    [KSPAddonFixed(KSPAddon.Startup.MainMenu, true, typeof(KerbalKonstructs))]
    public class KerbalKonstructs : MonoBehaviour
    {
        // Hello
        public static KerbalKonstructs instance;
        public static string installDir = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbalKonstructs));
        private Dictionary<UpgradeableFacility, int> facilityLevels = new Dictionary<UpgradeableFacility, int>();

        public static readonly string sKKVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        #region Holders
        public StaticObject selectedObject;
        public StaticModel selectedModel;
        public StaticObject snapTargetInstance;
        public StaticDatabase staticDB = new StaticDatabase();
        public CameraController camControl = new CameraController();
        private CelestialBody currentBody;
        internal static bool InitialisedFacilities = false;

        public int iMenuCount = 0;

        public Double VesselCost = 0;
        public Double RefundAmount = 0;

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
        public Boolean VesselLaunched = false;
        public Boolean bImportedCustom = false;
        public Boolean bStylesSet = false;

        public Boolean bDisablePositionEditing = false;
        #endregion

        #region GUI Windows
        internal static EditorGUI GUI_Editor = new EditorGUI();
        internal static StaticsEditorGUI GUI_StaticsEditor = new StaticsEditorGUI();
        internal static NavGuidanceSystem GUI_NGS = new NavGuidanceSystem();
        internal static BaseBossFlight GUI_FlightManager = new BaseBossFlight();
        internal static FacilityManager GUI_FacilityManager = new FacilityManager();
        internal static LaunchSiteSelectorGUI GUI_LaunchSiteSelector = new LaunchSiteSelectorGUI();
        internal static Modules.MapIconManager GUI_MapIconManager = new Modules.MapIconManager();
        internal static Modules.MapIconDraw GUI_MapIcons = new Modules.MapIconDraw();
        internal static KSCManager GUI_KSCManager = new KSCManager();
        internal static AirRacing GUI_AirRacingApp = new AirRacing();
        internal static BaseManager GUI_BaseManager = new BaseManager();
        internal static KKSettingsGUI GUI_Settings = new KKSettingsGUI();
        internal static ModelInfo GUI_ModelInfo = new ModelInfo();
        internal static LandingGuide GUI_Landinguide = new LandingGuide();
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
        public Boolean mapShowOpenT = false;
        [KSPField]
        public Boolean mapShowHelipads = true;
        [KSPField]
        public Boolean mapShowRunways = true;
        [KSPField]
        public Boolean mapShowRocketbases = true;
        [KSPField]
        public Boolean mapShowOther = false;
        [KSPField]
        public Boolean mapShowRadar = false;
        [KSPField]
        public Boolean mapShowDownlinks = false;
        [KSPField]
        public Boolean mapShowUplinks = false;
        [KSPField]
        public Boolean mapShowGroundComms = false;
        [KSPField]
        public Boolean mapHideIconsBehindBody = true;
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
        [KSPField]
        public Boolean enableRT = true;
        [KSPField]
        public Boolean enableCommNet = true;
        #endregion

        private List<StaticObject> deletedInstances = new List<StaticObject>();

        void Awake()
        {
            instance = this;
            var TbController = new ToolbarController();
            Log.PerfStart("Awake Function");

            #region Game Event Hooks
            GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            GameEvents.onGUIApplicationLauncherReady.Add(TbController.OnGUIAppLauncherReady);
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
            GameEvents.onVesselRecovered.Add(OnVesselRecovered);
            GameEvents.onVesselRecoveryProcessing.Add(OnProcessRecovery);
            GameEvents.OnVesselRollout.Add(OnVesselLaunched);
            // draw map icons when needed
            GameEvents.OnMapEntered.Add(GUI_MapIcons.Open);
            GameEvents.OnMapExited.Add(GUI_MapIcons.Close);
            //process save game loading
            GameEvents.onGameStateSave.Add(SaveState);
            GameEvents.onGameStateLoad.Add(LoadState);
            #endregion

            #region Other Mods Hooks
            StageRecovery.AttachStageRecovery();
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
            KKAPI.addModelSetting("keepConvex", new ConfigGenericString());
            ConfigGenericString isScanable = new ConfigGenericString();
            isScanable.setDefaultValue("false");
            KKAPI.addInstanceSetting("isScanable", isScanable);
            #endregion

            #region Instance API
            // Position
            KKAPI.addInstanceSetting("CelestialBody", new ConfigCelestialBody());
            KKAPI.addInstanceSetting("RadialPosition", new ConfigVector3());
            KKAPI.addInstanceSetting("Orientation", new ConfigVector3());
            KKAPI.addInstanceSetting("RadiusOffset", new ConfigFloat());
            KKAPI.addInstanceSetting("RotationAngle", new ConfigFloat());

            // Calculated References - do not set, it will not work
            KKAPI.addInstanceSetting("RefLatitude", new ConfigDouble());
            KKAPI.addInstanceSetting("RefLongitude", new ConfigDouble());

            // Visibility and Grouping
            ConfigFloat visibilityConfig = new ConfigFloat();
            visibilityConfig.setDefaultValue(25000f);
            KKAPI.addInstanceSetting("VisibilityRange", visibilityConfig);
            ConfigGenericString groupConfig = new ConfigGenericString();
            groupConfig.setDefaultValue("Ungrouped");
            KKAPI.addInstanceSetting("Group", groupConfig);
            ConfigGenericString groupCenter = new ConfigGenericString();
            groupCenter.setDefaultValue("false");
            KKAPI.addInstanceSetting("GroupCenter", groupCenter);
            KKAPI.addInstanceSetting("RefCenter", new ConfigVector3());

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
            KKAPI.addInstanceSetting("LaunchSiteNation", new ConfigGenericString());
            ConfigGenericString launchSiteHidden = new ConfigGenericString();
            launchSiteHidden.setDefaultValue("false");
            KKAPI.addInstanceSetting("LaunchSiteIsHidden", launchSiteHidden);

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
            ConfigFloat missionCount = new ConfigFloat();
            missionCount.setDefaultValue(0f);
            KKAPI.addInstanceSetting("MissionCount", missionCount);
            ConfigGenericString missionlog = new ConfigGenericString();
            missionlog.setDefaultValue("No missions logged");
            KKAPI.addInstanceSetting("MissionLog", missionlog);

            // Facility Types
            ConfigGenericString facilityrole = new ConfigGenericString();
            facilityrole.setDefaultValue("None");
            KKAPI.addModelSetting("DefaultFacilityType", facilityrole);
            KKAPI.addModelSetting("DefaultFacilityLength", new ConfigFloat());
            KKAPI.addModelSetting("DefaultFacilityWidth", new ConfigFloat());
            KKAPI.addModelSetting("DefaultFacilityHeight", new ConfigFloat());
            KKAPI.addModelSetting("DefaultFacilityMassCapacity", new ConfigFloat());
            KKAPI.addModelSetting("DefaultFacilityCraftCapacity", new ConfigFloat());

            KKAPI.addInstanceSetting("FacilityMassCapacity", new ConfigFloat());
            KKAPI.addInstanceSetting("FacilityCraftCapacity", new ConfigFloat());


            ConfigGenericString instfacilityrole = new ConfigGenericString();
            instfacilityrole.setDefaultValue("None");
            KKAPI.addInstanceSetting("FacilityType", instfacilityrole);
            KKAPI.addInstanceSetting("FacilityLengthUsed", new ConfigFloat());
            KKAPI.addInstanceSetting("FacilityWidthUsed", new ConfigFloat());
            KKAPI.addInstanceSetting("FacilityHeightUsed", new ConfigFloat());
            KKAPI.addInstanceSetting("FacilityMassUsed", new ConfigFloat());
            KKAPI.addInstanceSetting("InStorage", new ConfigGenericString());

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
            KKAPI.addInstanceSetting("LqFMax", new ConfigFloat());
            KKAPI.addInstanceSetting("LqFCurrent", new ConfigFloat());
            KKAPI.addModelSetting("OxFMax", new ConfigFloat());
            KKAPI.addInstanceSetting("OxFMax", new ConfigFloat());
            KKAPI.addInstanceSetting("OxFCurrent", new ConfigFloat());
            KKAPI.addModelSetting("MoFMax", new ConfigFloat());
            KKAPI.addInstanceSetting("MoFMax", new ConfigFloat());
            KKAPI.addInstanceSetting("MoFCurrent", new ConfigFloat());

            KKAPI.addInstanceSetting("LqFAlt", new ConfigGenericString());
            KKAPI.addInstanceSetting("OxFAlt", new ConfigGenericString());
            KKAPI.addInstanceSetting("MoFAlt", new ConfigGenericString());

            KKAPI.addModelSetting("ECMax", new ConfigFloat());
            KKAPI.addModelSetting("ECRechargeRate", new ConfigFloat());
            KKAPI.addInstanceSetting("ECMax", new ConfigFloat());
            KKAPI.addInstanceSetting("ECRechargeRate", new ConfigFloat());
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

            // Local to a specific save - constructed in a specific save-game
            // WIP for founding
            ConfigGenericString LocalToSave = new ConfigGenericString();
            LocalToSave.setDefaultValue("False");
            KKAPI.addInstanceSetting("LocalToSave", LocalToSave);

            // Custom instances - added or modified by player with the editor
            ConfigGenericString CustomInstance = new ConfigGenericString();
            CustomInstance.setDefaultValue("False");
            KKAPI.addInstanceSetting("CustomInstance", CustomInstance);

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

            // Model Scale
            ConfigFloat modelScale = new ConfigFloat();
            modelScale.setDefaultValue(1f);
            KKAPI.addInstanceSetting("ModelScale", modelScale);

            // Nation API
            KKAPI.addNationSetting("nationName", new ConfigGenericString());
            KKAPI.addNationSetting("shortname", new ConfigGenericString());
            KKAPI.addNationSetting("abbreviation", new ConfigGenericString());
            KKAPI.addNationSetting("nationIcon", new ConfigGenericString());
            KKAPI.addNationSetting("description", new ConfigGenericString());
            KKAPI.addNationSetting("rulership", new ConfigGenericString());
            ConfigFloat fnationratingresources = new ConfigFloat();
            fnationratingresources.setDefaultValue(0f);
            KKAPI.addNationSetting("ratingresources", fnationratingresources);
            ConfigFloat fnationratingmilitary = new ConfigFloat();
            fnationratingmilitary.setDefaultValue(0f);
            KKAPI.addNationSetting("ratingmilitary", fnationratingmilitary);
            ConfigFloat fnationratingtech = new ConfigFloat();
            fnationratingtech.setDefaultValue(0f);
            KKAPI.addNationSetting("ratingtech", fnationratingtech);

            #endregion

            SpaceCenterManager.setKSC();
            loadConfig();
            saveConfig();

            DontDestroyOnLoad(this);
            Log.PerfStart("Object loading1");

            LoadSquadModels();

            LoadModels();
            Log.PerfStop("Object loading1");
            Log.PerfStart("Object loading2");

            LoadModelInstances();

            Log.PerfStop("Object loading2");

            Log.UserInfo("Version is " + sKKVersion + " .");

            Log.UserInfo("StaticDB has: " + staticDB.GetAllStatics().Count() + "Entries");
            UIMain.setTextures();
            Log.PerfStop("Awake Function");

        }

        #region Game Events


        /// <summary>
        /// Updates the mission log and processes the launch refund.
        /// </summary>
        /// <param name="vVessel"></param>
        void OnVesselLaunched(ShipConstruct vVessel)
        {
            Log.Normal("OnVesselLaunched");
            if (!MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
            {
                return;
            }
            else
            {
                Log.Normal("OnVesselLaunched is Career");
                string sitename = LaunchSiteManager.getCurrentLaunchSite();
                if (sitename == "Runway") return;
                if (sitename == "LaunchPad") return;
                if (sitename == "KSC") return;
                if (sitename == "") return;

                LaunchSite lsSite = LaunchSiteManager.getLaunchSiteByName(sitename);
                float fMissionCount = lsSite.missionCount;
                lsSite.missionCount = fMissionCount + 1;
                double dSecs = HighLogic.CurrentGame.UniversalTime;

                double hours = dSecs / 60.0 / 60.0;
                double kHours = Math.Floor(hours % 6.0);
                double kMinutes = Math.Floor((dSecs / 60.0) % 60.0);
                double kSeconds = Math.Floor(dSecs % 60.0);
                double kYears = Math.Floor(hours / 2556.5402) + 1; // Kerbin year is 2556.5402 hours
                double kDays = Math.Floor(hours % 2556.5402 / 6.0) + 1;

                string sDate = "Y" + kYears.ToString() + " D" + kDays.ToString() + " " + " " + kHours.ToString("00") + ":" + kMinutes.ToString("00") + ":" + kSeconds.ToString("00");

                string sCraft = vVessel.shipName;
                string sWeight = vVessel.GetTotalMass().ToString();
                string sLogEntry = lsSite.missionLog + sDate + ", Launched " + sCraft + ", Mass " + sWeight + " t|";
                lsSite.missionLog = sLogEntry;

                VesselLaunched = true;

                float dryCost = 0f;
                float fuelCost = 0f;
                float total = vVessel.GetShipCosts(out dryCost, out fuelCost);

                var cm = CurrencyModifierQuery.RunQuery(TransactionReasons.VesselRollout, total, 0f, 0f);
                total += cm.GetEffectDelta(Currency.Funds);
                double launchcost = total;
                float fRefund = 0f;
                LaunchSiteManager.getSiteLaunchRefund(sitename, out fRefund);
                Log.Normal("Launch Refund: " + fRefund);
                if (fRefund < 1) return;

                RefundAmount = (launchcost / 100) * fRefund;
                VesselCost = launchcost - (RefundAmount);
                if (fRefund > 0)
                {
                    string sMessage = "This launch normally costs " + launchcost.ToString("#0") +
                        " but " + sitename + " provides a " + fRefund + "% refund. \n\nSo " + RefundAmount.ToString("#0") + " funds has been credited to you. \n\nEnjoy and thanks for using " +
                        sitename + ". Have a safe flight.";
                    MiscUtils.PostMessage("Launch Refund", sMessage, MessageSystemButton.MessageButtonColor.GREEN, MessageSystemButton.ButtonIcons.ALERT);
                    Funding.Instance.AddFunds(RefundAmount, TransactionReasons.VesselRollout);
                }
            }
        }


        public void LoadState(ConfigNode node)
        {
            Log.Normal("Load State");
            if (CareerUtils.isCarrerGame)
            {
                Log.Normal("Load openclose states for career game");
                Log.PerfStart("Load Fac");
                CareerState.Load();
                Log.PerfStop("Load Fac");
            }
            RemoteNet.LoadGroundStations();
            //LoadSquadModels();
        }

        public void SaveState(ConfigNode configNode)
        {
            Log.Normal("Save State");
            CareerState.Save();
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

            if (data.Equals(GameScenes.FLIGHT))
            {
                bTreatBodyAsNullForStatics = false;

                InputLockManager.RemoveControlLock("KKEditorLock");
                InputLockManager.RemoveControlLock("KKEditorLock2");


                if (FlightGlobals.ActiveVessel != null)
                {
                    staticDB.ToggleActiveStaticsOnPlanet(FlightGlobals.ActiveVessel.mainBody, true, true);
                    currentBody = FlightGlobals.ActiveVessel.mainBody;
                    staticDB.OnBodyChanged(FlightGlobals.ActiveVessel.mainBody);
                    DoHangaredCraftCheck();
                }
                else
                {
                    Log.Debug("Flight scene load. No activevessel. Activating all statics.");
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
                currentBody = KKAPI.getCelestialBody("HomeWorld");
                Log.Normal("Homeworld is " + currentBody.name);
                //staticDB.onBodyChanged(KKAPI.getCelestialBody("Kerbin"));
                //staticDB.onBodyChanged(null);
                staticDB.ToggleActiveStaticsInGroup("KSCUpgrades", true);
                staticDB.ToggleActiveStaticsInGroup("KSCRace", true);
                // *********

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
                bTreatBodyAsNullForStatics = false;
                iMenuCount = iMenuCount + 1;
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

            if (bTreatBodyAsNullForStatics) staticDB.OnBodyChanged(null);
        }

        void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
        {
            staticDB.ToggleActiveStaticsOnPlanet(data.to, true, true);
            currentBody = data.to;
            staticDB.OnBodyChanged(data.to);
            updateCache();
        }


        void OnProcessRecovery(ProtoVessel vessel, MissionRecoveryDialog dialog, float fFloat)
        {
            if (!disableRemoteRecovery)
            {
                if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
                {
                    Log.Normal("OnProcessRecovery");
                    if (vessel == null) return;
                    if (dialog == null) return;
                    Log.Normal("OnProcessRecovery");
                    dRecoveryValue = dialog.fundsEarned;
                }
            }
        }

        /// <summary>
        /// GameEvent handle. This is called first.
        /// </summary>
        /// <param name="data"></param>
		void OnVesselRecoveryRequested(Vessel vessel)
        {
            Log.Normal("OnVesselRecoveryRequested");
            if (!disableRemoteRecovery)
            {
                if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
                {
                    Log.Normal("OnVesselRecoveryRequested is career");
                    // Change the Space Centre to the nearest open base
                    fRecovFactor = 0f;
                    float fDist = 0f;
                    float fRecovFact = 0f;
                    float fRecovRng = 0f;
                    string sBaseName = "";

                    SpaceCenter csc = SpaceCenter.Instance;
                    SpaceCenterManager.getClosestSpaceCenter(vessel, out csc, out fDist, out fRecovFact, out fRecovRng, out sBaseName);

                    lastRecoveryBase = sBaseName;
                    if (sBaseName == "Runway" || sBaseName == "LaunchPad") lastRecoveryBase = "KSC";
                    if (sBaseName == "KSC") lastRecoveryBase = "KSC";

                    lastRecoveryDistance = fDist;
                    fRecovFactor = fRecovFact;
                    fRecovRange = fRecovRng;
                    Log.Normal("Recovery stats: " + lastRecoveryBase + " " + lastRecoveryDistance);
                }
            }
            return;
        }


        /// <summary>
        /// Gameevent handle. This is called after OnVesselRecoveryRequested
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="quick"></param>
		public void OnVesselRecovered(ProtoVessel vessel, Boolean quick)
        {
            Log.Normal("onVesselRecovered called");
            if (!disableRemoteRecovery)
            {
                if (vessel == null)
                {
                    Log.Warning("onVesselRecovered vessel was null");
                    return;
                }
                if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
                {

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

                            // Don't spam messages if we recover trash
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

        /// <summary>
        /// Unity Late Update
        /// </summary>
        void LateUpdate()
        {
            // Check if we don't have the KSC Buildings in the savegame and save them there if missing.
            // this is needed, because for some reason we set all buildings directly to max level without.
            CareerState.FixKSCFacilities();

            GUI_Editor.CheckEditorKeys();

            if (Input.GetKeyDown(KeyCode.K) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                GUI_StaticsEditor.ToggleEditor();
            }
            if (Input.GetKeyDown(KeyCode.LeftShift) && GUI_StaticsEditor.IsOpen())
            {
                GUI_StaticsEditor.SelectMouseObject();
            }

        }

        #endregion

        #region GUI Methods

        void OnGUI()
        {

        }
        #endregion

        #region Object Methods

        public void DeletePreviewObject()
        {
            if (selectedModel != null)
            {
                if (ModelInfo.currPreview != null)
                {
                    ModelInfo.DestroyPreviewInstance(null);
                }
            }
        }

        public void DoHangaredCraftCheck()
        {
            foreach (StaticObject obj in getStaticDB().GetAllStatics())
            {
                string sFacType = (string)obj.getSetting("FacilityType");

                if (sFacType != "Hangar") continue;

                if (sFacType == "Hangar")
                {
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
                            Log.Debug("Craft InStorage no longer exists. Emptying this hangar space.");
                            obj.setSetting("InStorage", "None");
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
                            Log.Debug("Craft TargetID no longer exists. Emptying this hangar space.");
                            obj.setSetting("TargetID", "None");
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
                            Log.Debug("Craft TargetType no longer exists. Emptying this hangar space.");

                            obj.setSetting("TargetType", "None");
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
                                    Log.Debug("Craft has been been taken control of. Emptying " + sHangarSpace + " hangar space.");
                                    obj.setSetting(sHangarSpace, "None");
                                }
                                else
                                {
                                    Log.Debug("Hiding vessel " + vVesselStored.vesselName + ". It is in the hangar.");
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
                    Log.Debug("updateCache using selectedObject as playerPos");
                }
                else if (FlightGlobals.ActiveVessel != null)
                {
                    playerPos = FlightGlobals.ActiveVessel.transform.position;
                    Log.Debug("updateCache using ActiveVessel as playerPos" + FlightGlobals.ActiveVessel.vesselName);
                }
                else if (Camera.main != null)
                {
                    playerPos = Camera.main.transform.position;
                    Log.Debug("updateCache using Camera.main as playerPos");
                }
                else
                {
                    Log.Debug("KerbalKonstructs.updateCache could not determine playerPos. All hell now happens.");
                }

                staticDB.UpdateCache(playerPos);
            }
        }


        /// <summary>
        /// Loads and places all model instances from the confignode.
        /// </summary>
        /// <param name="configurl"></param>
        /// <param name="model"></param>
        /// <param name="bSecondPass"></param>
		public void loadInstances(UrlDir.UrlConfig configurl, StaticModel model, bool bSecondPass = false)
        {
            if (model == null)
            {
                Log.UserError("KK: Attempting to loadInstances for a null model. Check your model and config.");
                return;
            }

            if (configurl == null)
            {
                Log.UserError("KK: Attempting to loadInstances for a null ConfigNode. Check your model and config.");
                return;
            }

            foreach (ConfigNode instance in configurl.config.GetNodes("Instances"))
            {
                StaticObject obj = new StaticObject();
                obj.model = model;
                obj.configUrl = configurl;
                obj.configPath = configurl.url.Substring(0, configurl.url.LastIndexOf('/')) + ".cfg";
                //obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));
                obj.gameObject = Instantiate(model.prefab);
                if (obj.gameObject == null)
                {
                    Log.UserError("KK: Could not find " + model.getSetting("mesh") + ".mu! Did the modder forget to include it or did you actually install it?");
                    continue;
                }

                obj.settings = KKAPI.loadConfig(instance, KKAPI.getInstanceSettings());

                if (obj.settings == null)
                {
                    Log.UserError("KK: Error loading instances for " + model.getSetting("mesh") + ".mu! Check your model and config.");
                    continue;
                }

                CelestialBody body = (CelestialBody)obj.getSetting("CelestialBody");
                if (body == null)
                    continue;

                // create RadialPosition, If we don't have one.
                if (!obj.settings.ContainsKey("RadialPosition"))
                {
                    if (obj.settings.ContainsKey("RefLatitude") && obj.settings.ContainsKey("RefLongitude"))
                    {
                        double lat = (double)obj.getSetting("RefLatitude");
                        double lon = (double)obj.getSetting("RefLongitude");
                        Vector3 newPostion = body.GetRelSurfaceNVector(lat, lon) * body.Radius;
                        obj.setSetting("RadialPosition", newPostion);
                        Log.UserInfo("creating new Radialposition for: " + obj.gameObject.name);
                    } else
                    {
                        Log.UserError("Neither RadialPosition or RefLatitude+RefLongitude found: " + obj.gameObject.name);
                        continue;
                    }
                }

                // sometimes we need a second pass.. (do we???)
                // 

                if (bSecondPass)
                {
                    Vector3 secondInstanceKey = (Vector3)obj.getSetting("RadialPosition");
                    bool bSpaceOccupied = false;

                    foreach (StaticObject soThis in KerbalKonstructs.instance.getStaticDB().GetAllStatics())
                    {
                        Vector3 firstInstanceKey = (Vector3)soThis.getSetting("RadialPosition");

                        if (firstInstanceKey == secondInstanceKey)
                        {
                            string sThisMesh = (string)soThis.model.getSetting("mesh");
                            string sThatMesh = (string)obj.model.getSetting("mesh");

                            Log.Debug("Custom instance has a RadialPosition that already has an instance."
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
                                    Log.Debug("KK: Attempted to import identical custom instance to same RadialPosition as existing instance: Check for duplicate custom statics: " + Environment.NewLine
                                    + sThisMesh + " : " + (string)soThis.getSetting("Group") + " : " + firstInstanceKey.ToString() + " | "
                                    + sThatMesh + " : " + (string)obj.getSetting("Group") + " : " + secondInstanceKey.ToString());
                                    break;
                                }
                                else
                                {
                                    Log.Debug("Different rotation or offset. Allowing. Could be a feature of the same model such as a doorway being used. Will cause z tearing probably.");
                                }
                            }
                            else
                            {
                                Log.Debug("Different models. Allowing. Could be a terrain foundation or integrator.");
                            }
                        }
                    }

                    if (bSpaceOccupied)
                    {
                        //Debug.LogWarning("KK: Attempted to import identical custom instance to same RadialPosition as existing instance. Skipped. Check for duplicate custom statics you have installed. Did you export the custom instances to make a pack? If not, ask the mod-makers if they are duplicating the same stuff as each other.");
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
                        Log.Normal("Launch site is missing a transform. Defaulting to " + obj.getSetting("LaunchSiteName") + "_spawn...");

                        if (obj.gameObject.transform.Find(obj.getSetting("LaunchSiteName") + "_spawn") != null)
                        {
                            obj.settings.Add("LaunchPadTransform", obj.getSetting("LaunchSiteName") + "_spawn");
                        }
                        else
                        {
                            Log.Normal("FAILED: " + obj.getSetting("LaunchSiteName") + "_spawn does not exist! Attempting to use any transform with _spawn in the name.");
                            Transform lastResort = obj.gameObject.transform.Cast<Transform>().FirstOrDefault(trans => trans.name.EndsWith("_spawn"));

                            if (lastResort != null)
                            {
                                Log.Normal("Using " + lastResort.name + " as launchpad transform");
                                obj.settings.Add("LaunchPadTransform", lastResort.name);
                            }
                            else
                            {
                                Log.Error("All attempts at finding a launchpad transform have failed (╯°□°）╯︵ ┻━┻ This static isn't configured for KK properly. Tell the modder.");
                            }
                        }
                    }
                }
                staticDB.AddStatic(obj);
                obj.spawnObject(false, false);

                if (obj.settings.ContainsKey("LaunchPadTransform") && obj.settings.ContainsKey("LaunchSiteName"))
                    LaunchSiteManager.createLaunchSite(obj);

            }

        }


        public void loadNations()
        {
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("KERBINNATION");

            foreach (UrlDir.UrlConfig conf in configs)
            {
                KerbinNation nation = new KerbinNation();
            }
        }

        /// <summary>
        /// Tes for getting information out of the prebuild static models.
        /// </summary>
        public void LoadSquadModels ()
        {

            // first we find get all upgradeable facilities
            Upgradeables.UpgradeableObject[] upgradeablefacilities;
            upgradeablefacilities = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>();

            foreach (var facility in upgradeablefacilities)
            {
                for (int i = 0; i < facility.UpgradeLevels.Length; i++)
                {

                    string modelName = "KSC_" + facility.name + "_level_" + (i + 1).ToString();
                    string modelTitle = "KSC " + facility.name + " level " + (i + 1).ToString();

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (staticDB.GetModels().Select(x => x.name).Contains(modelName))
                        continue;

                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.setSetting("keepConvex", "true");
                    model.setSetting("title", modelTitle);
                    model.setSetting("mesh", modelName);
                    model.setSetting("category", "Squad KSC");
                    model.setSetting("Author", "Squad");
                    model.setSetting("manufacturer", "Squad");
                    model.setSetting("description", "Squad original " + modelTitle);

                    model.isSquad = true;

                    // the runways have all the same spawnpoint.
                    if (facility.name.Equals("Runway", StringComparison.CurrentCultureIgnoreCase))
                        model.setSetting("DefaultLaunchPadTransform", "End09/SpawnPoint");

                    // Launchpads also 
                    if (facility.name.Equals("LaunchPad", StringComparison.CurrentCultureIgnoreCase))
                        model.setSetting("DefaultLaunchPadTransform", "LaunchPad_spawn");

                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = facility.UpgradeLevels[i].facilityPrefab;


                    staticDB.RegisterModel(model, modelName);
                }
            }

        }

        /// <summary>
        /// Loads the models and creates the prefab objects, which are referenced by the instance loader
        /// </summary>
		public void LoadModels()
        {
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");

            foreach (UrlDir.UrlConfig conf in configs)
            {
                // ignore referenced objects
                if (conf.config.HasValue("pointername"))
                {
                    if ((!String.IsNullOrEmpty(conf.config.GetValue("pointername")) && !conf.config.GetValue("pointername").Equals("none", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        continue;
                    }
                }
                // Check if an modelname is set we can use, else set one
                string modelName = conf.config.GetValue("name");
                if (String.IsNullOrEmpty(modelName))
                {
                    Log.UserWarning("No Name Found in configuration : " + conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg");
                    modelName = Regex.Replace(conf.config.GetValue("title"), @"\s+", "");
                    if (String.IsNullOrEmpty(modelName))
                    {
                        modelName = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
                    }
                    if (!String.IsNullOrEmpty(modelName))
                    {
                        conf.config.SetValue("name", modelName, true);
                    }
                    else
                    {
                        Log.Error("No Name Found in configuration : " + conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg");
                        continue;
                    }
                }

                StaticModel model = new StaticModel();
                model.name = modelName;
                model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
                model.config = conf.url;
                model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
                model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());

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
                model.prefab = GameDatabase.Instance.GetModelPrefab(model.path + "/" + model.getSetting("mesh"));

                if (model.prefab == null)
                {
                    Debug.Log("KK: Could not find " + model.getSetting("mesh") + ".mu! Did the modder forget to include it or did you actually install it?");
                    continue;
                }
                if (((string)model.getSetting("keepConvex")).Equals("true", StringComparison.CurrentCultureIgnoreCase))
                { }
                else
                {
                    MeshCollider[] concave = model.prefab.GetComponentsInChildren<MeshCollider>(true);
                    foreach (MeshCollider collider in concave)
                    {
                        Log.Debug("Making collider " + collider.name + " concave.");
                        collider.convex = false;
                    }
                }
                staticDB.RegisterModel(model, modelName);
                // most mods will not load without beeing loaded here
                loadInstances(conf, model, false);
            }
        }

        /// <summary>
        /// loads all statics with a pointername?!?
        /// </summary>
        public void LoadModelInstances()
        {
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");
            string modelname = null;
            foreach (UrlDir.UrlConfig conf in configs)
            {
                if (conf.config.HasValue("pointername") && !String.IsNullOrEmpty(conf.config.GetValue("pointername")))
                {
                    modelname = conf.config.GetValue("pointername");
                }
                else
                {
                    continue;
                    //modelname = conf.config.GetValue("name");
                }

                StaticModel model = staticDB.GetModel(modelname);
                if (model != null)
                {
                    loadInstances(conf, model, true);
                }
                else { Log.UserError("No Model named " + modelname + " found as defined in: " + conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg"); }
            }
        }

        /// <summary>
        /// saves the model definition and the direct instances
        /// </summary>
        /// <param name="mModelToSave"></param>
        public void saveModelConfig(StaticModel mModelToSave)
        {
            StaticModel model = staticDB.GetModel(mModelToSave.name);


            ConfigNode staticNode = new ConfigNode("STATIC");
            ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

            foreach (KeyValuePair<string, object> modelsetting in model.settings)
            {
                if (modelsetting.Key == "mesh") continue;

                if (modelConfig.HasValue(modelsetting.Key))
                {
                    modelConfig.RemoveValue(modelsetting.Key);
                    modelConfig.AddValue(modelsetting.Key, KKAPI.getModelSettings()[modelsetting.Key].convertValueToConfig(modelsetting.Value));
                }
                else
                {
                    modelConfig.AddValue(modelsetting.Key, KKAPI.getModelSettings()[modelsetting.Key].convertValueToConfig(modelsetting.Value));
                }
            }

            modelConfig.RemoveNodes("Instances");

            foreach (StaticObject obj in staticDB.GetDirectInstancesFromModel(model))
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


        /// <summary>
        /// this saves the pointer references to thier configfiles.
        /// </summary>
        /// <param name="pathname"></param>
        internal void SaveInstanceByCfg(string pathname)
        {
            List<StaticObject> allInstances = staticDB.GetAllStatics().Where(instance => instance.configPath == pathname).ToList();
            StaticObject firstInstance = allInstances.First();
            ConfigNode instanceConfig = null;

            ConfigNode staticNode = new ConfigNode("STATIC");

            if (firstInstance.configUrl == null) //this are newly spawned instances
            {
                instanceConfig = new ConfigNode("STATIC");
                instanceConfig.AddValue("pointername", firstInstance.model.name);
            } else
            {

                instanceConfig = GameDatabase.Instance.GetConfigNode(firstInstance.configUrl.url);
                instanceConfig.RemoveNodes("Instances");
            }

            staticNode.AddNode(instanceConfig);
            foreach (StaticObject obj in allInstances)
            {
                ConfigNode inst = new ConfigNode("Instances");
                foreach (KeyValuePair<string, object> setting in obj.settings)
                {
                    inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
                }
                instanceConfig.nodes.Add(inst);
            }


            staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + firstInstance.configPath, "Generated by Kerbal Konstructs");
        }



        /// <summary>
        /// This saves all satic objects to thier instance files.. 
        /// </summary>
        public void saveObjects()
        {
            List<String> processedInstances = new List<string>();
            List<StaticObject> allInstances = staticDB.GetAllStatics();
            foreach (StaticObject instance in allInstances)
            {
                // ignore allready processed cfg files
                if (processedInstances.Contains(instance.configPath)) { continue; }

                if (instance.configPath == instance.model.configPath)
                {
                    saveModelConfig(instance.model);
                }
                else
                {
                    // find all instances with the same configPath. 
                    SaveInstanceByCfg(instance.configPath);
                }

                processedInstances.Add(instance.configPath);
            }

            // check for orqhaned files
            foreach (StaticObject deletedInstance in deletedInstances)
            {
                if (!processedInstances.Contains(deletedInstance.configPath))
                {
                    if (deletedInstance.configPath == deletedInstance.model.configPath)
                    {
                        // keep the mode definition
                        saveModelConfig(deletedInstance.model);
                    }
                    else
                    {
                        // remove the file
                        File.Delete(KSPUtil.ApplicationRootPath + "GameData/" + deletedInstance.configPath);
                    }
                }
                processedInstances.Add(deletedInstance.configPath);

            }
        }




        public void exportCustomInstances(string sPackName = "MyStaticPack", string sBaseName = "All", string sGroup = "", Boolean bLocal = false)
        {
            bool HasCustom = false;
            string sBase = "";

            if (sGroup != "") sBase = sGroup;
            else
                sBase = sBaseName;

            foreach (StaticModel model in staticDB.GetModels())
            {
                HasCustom = false;
                ConfigNode staticNode = new ConfigNode("STATIC");
                ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

                modelConfig.RemoveNodes("Instances");

                foreach (StaticObject obj in staticDB.GetObjectsFromModel(model))
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

                    if (DevMode)
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
                    if (DevMode)
                    {
                        Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sBase);
                        staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sBase + "/" + sModelName + ".cfg", "Exported custom instances by Kerbal Konstructs");
                    }
                    else
                    {
                        Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + sBase + "/" + model.path);
                        staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + sBase + "/" + model.configPath, "Exported custom instances by Kerbal Konstructs");
                    }
                }
            }
        }

        public void exportMasters()
        {
            string sBase = "";
            string activeBodyName = "";

            Dictionary<string, Dictionary<string, StaticGroup>> groupList = new Dictionary<string, Dictionary<string, StaticGroup>>();

            foreach (StaticObject obj in getStaticDB().GetAllStatics())
            {
                String bodyName = ((CelestialBody)obj.getSetting("CelestialBody")).bodyName;
                String groupName = (string)obj.getSetting("Group");

                if (!groupList.ContainsKey(bodyName))
                {
                    groupList.Add(bodyName, new Dictionary<string, StaticGroup>());
                    Debug.Log("Added " + bodyName);
                }

                if (!groupList[bodyName].ContainsKey(groupName))
                {
                    StaticGroup group = new StaticGroup(groupName, bodyName);
                    groupList[bodyName].Add(groupName, group);
                    Debug.Log("Added " + groupName);
                }
            }

            foreach (CelestialBody cBody in FlightGlobals.Bodies)
            {
                activeBodyName = cBody.name;
                Debug.Log("activeBodyName is " + cBody.name);

                if (!groupList.ContainsKey(activeBodyName)) continue;

                foreach (StaticGroup group in groupList[activeBodyName].Values)
                {
                    sBase = group.groupName;
                    Debug.Log("sBase is " + sBase);

                    foreach (StaticModel model in staticDB.GetModels())
                    {
                        ConfigNode staticNode = new ConfigNode("STATIC");
                        ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

                        //Debug.Log("Model is " + model.getSetting("name"));

                        modelConfig.RemoveNodes("Instances");
                        bool bNoInstances = true;

                        foreach (StaticObject obj in staticDB.GetObjectsFromModel(model))
                        {
                            string sObjGroup = (string)obj.getSetting("Group");
                            if (sObjGroup != sBase) continue;

                            ConfigNode inst = new ConfigNode("Instances");

                            foreach (KeyValuePair<string, object> setting in obj.settings)
                            {
                                inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
                            }
                            modelConfig.nodes.Add(inst);
                            bNoInstances = false;
                        }

                        if (bNoInstances) continue;

                        string sModelName = modelConfig.GetValue("name");
                        modelConfig.AddValue("pointername", sModelName);

                        modelConfig.RemoveValue("name");
                        modelConfig.AddValue("name", "Master" + "_" + sBase + "_" + sModelName);

                        staticNode.AddNode(modelConfig);

                        Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/Master/" + sBase + "/");
                        staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/Master/" + sBase + "/" + sModelName + ".cfg", "Exported master instances by Kerbal Konstructs");
                    }
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

            Log.Debug("deleteObject");

            // check later when saving if this file is empty
            deletedInstances.Add(obj);

            staticDB.DeleteObject(obj);
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
            Log.Debug("obj.preview is " + obj.preview.ToString());
            selectedObject = obj;
            Log.Debug("selectedObject.preview is " + selectedObject.preview.ToString());
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
                /* selectedObject.editing = false;
				if (enableColliders) selectedObject.ToggleAllColliders(true);

				Color highlightColor = new Color(0, 0, 0, 0);
				selectedObject.HighlightObject(highlightColor); */

                selectedObject.deselectObject(enableColliders);
                selectedObject = null;
            }

            InputLockManager.RemoveControlLock("KKShipLock");
            InputLockManager.RemoveControlLock("KKEVALock");
            InputLockManager.RemoveControlLock("KKCamModes");

            if (disableCam)
                camControl.disable();
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
            //ToDo: FlightGlobals.currentMainBody;
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
                        if (cfg.HasValue(f.Name))
                            f.SetValue(this, Convert.ChangeType(cfg.GetValue(f.Name), f.FieldType));
                    }
                    else
                    {
                        Log.Debug("Attribute not defined as KSPField. This is harmless.");
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

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System.Reflection;
using KSP.UI.Screens;
using Upgradeables;
using KerbalKonstructs.Addons;
using KerbalKonstructs.Modules;

using Debug = UnityEngine.Debug;


namespace KerbalKonstructs
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbalKonstructs : MonoBehaviour
    {
        // Hello
        public static KerbalKonstructs instance;
        public static string installDir = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbalKonstructs));
        private Dictionary<UpgradeableFacility, int> facilityLevels = new Dictionary<UpgradeableFacility, int>();

        public static readonly string sKKVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        #region Holders
        public StaticInstance selectedObject;
        internal StaticModel selectedModel;
        public StaticInstance snapTargetInstance;
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
        public Boolean bStylesSet = false;

        public Boolean bDisablePositionEditing = false;
        #endregion

        #region GUI Windows
        internal static EditorGUI GUI_Editor = new EditorGUI();
        internal static LaunchSiteEditor GUI_LSEditor = new LaunchSiteEditor();
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
        internal static LandingGuideUI GUI_Landinguide = new LandingGuideUI();
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
        public Boolean mapShowWaterLaunch = true;
        [KSPField]
        public Boolean mapShowOther = false;
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

        private List<StaticInstance> deletedInstances = new List<StaticInstance>();


        /// <summary>
        /// Unity GameObject Awake function
        /// </summary>
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

            SpaceCenterManager.setKSC();

            DontDestroyOnLoad(this);
            Log.PerfStart("Object loading1");

            LoadSquadModels();

            LoadModels();
          //  SDTest.WriteTextures();

            Log.PerfStop("Object loading1");
            Log.PerfStart("Object loading2");

            LoadModelInstances();

            Log.PerfStop("Object loading2");

            Log.UserInfo("Version is " + sKKVersion + " .");

            Log.UserInfo("StaticDatabase has: " + StaticDatabase.allStaticInstances.Count() + "Entries");
            UIMain.setTextures();
            Log.PerfStop("Awake Function");
            //Log.PerfStart("Model Test");
            //SDTest.GetModelStats();
            //Log.PerfStop("Model Test");
            //SDTest.GetShaderStats();
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
                float fMissionCount = lsSite.MissionCount;
                lsSite.MissionCount = fMissionCount + 1;
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
                string sLogEntry = lsSite.MissionLog + sDate + ", Launched " + sCraft + ", Mass " + sWeight + " t|";
                lsSite.MissionLog = sLogEntry;

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

        /// <summary>
        /// GameEvent function for loading Career Sate
        /// </summary>
        /// <param name="node"></param>
        public void LoadState(ConfigNode configNode)
        {
            ConfigNode kkNode;

            Log.Normal("Load State");
            if (configNode.HasNode("KerbalKonstructs"))
            {
                kkNode = configNode.GetNode("KerbalKonstructs");
            }  else
            {
                kkNode = configNode.AddNode("KerbalKonstructs");
            }

            LoadKKConfig(kkNode);


            if (CareerUtils.isCareerGame)
            {
                Log.Normal("Load openclose states for career game");
                Log.PerfStart("Load Fac");
                CareerState.Load(kkNode);
                Log.PerfStop("Load Fac");
            }
            RemoteNet.LoadGroundStations();
        }

        /// <summary>
        /// GameEvent function for saving Career State
        /// </summary>
        /// <param name="configNode"></param>
        public void SaveState(ConfigNode configNode)
        {
            ConfigNode kkNode;

            Log.Normal("Save State");
            if (configNode.HasNode("KerbalKonstructs"))
            {
                kkNode = configNode.GetNode("KerbalKonstructs");
            }
            else
            {
                kkNode = configNode.AddNode("KerbalKonstructs");
            }

            SaveKKConfig(kkNode);

            if (CareerUtils.isCareerGame)
            {
                CareerState.Save(kkNode);
            }
        }


        /// <summary>
        /// GameEvent function for toggeling the visiblility of Statics
        /// </summary>
        /// <param name="data"></param>
        void onLevelWasLoaded(GameScenes data)
        {

            bool bTreatBodyAsNullForStatics = true;
            DeletePreviewObject();

            StaticDatabase.ToggleActiveAllStatics(false);

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
                    StaticDatabase.ToggleActiveStaticsOnPlanet(FlightGlobals.ActiveVessel.mainBody, true, true);
                    currentBody = FlightGlobals.ActiveVessel.mainBody;
                    StaticDatabase.OnBodyChanged(FlightGlobals.ActiveVessel.mainBody);
                    Hangar.DoHangaredCraftCheck();
                }
                else
                {
                    Log.Debug("Flight scene load. No activevessel. Activating all statics.");
                    StaticDatabase.ToggleActiveAllStatics(true);
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
                currentBody = ConfigUtil.GetCelestialBody("HomeWorld");
                Log.Normal("Homeworld is " + currentBody.name);
                //StaticDatabase.onBodyChanged(KKAPI.getCelestialBody("Kerbin"));
                //StaticDatabase.onBodyChanged(null);
                //StaticDatabase.ToggleActiveStaticsInGroup("KSCUpgrades", true);
                //StaticDatabase.ToggleActiveStaticsInGroup("KSCRace", true);
                updateCache();
                // *********

            }

            if (data.Equals(GameScenes.MAINMENU))
            {
                CareerState.ResetFacilitiesOpenState();
                //// Close all the launchsite objects
                //LaunchSiteManager.setAllLaunchsitesClosed();
                atMainMenu = true;
                bTreatBodyAsNullForStatics = false;
                iMenuCount = iMenuCount + 1;
                // reset this for the next Newgame
                if (InitialisedFacilities)
                {
                    InitialisedFacilities = false;
                }
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

            if (bTreatBodyAsNullForStatics) StaticDatabase.OnBodyChanged(null);
        }


        void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
        {
            StaticDatabase.ToggleActiveStaticsOnPlanet(data.to, true, true);
            currentBody = data.to;
            StaticDatabase.OnBodyChanged(data.to);
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
        /// Unity Late Update. Used for KeyCodes and fixing facility levels on new games...
        /// </summary>
        void LateUpdate()
        {

            // Check if we don't have the KSC Buildings in the savegame and save them there if missing.
            // this is needed, because for some reason we set all buildings directly to max level without.
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                CareerState.FixKSCFacilities();

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                GUI_Editor.CheckEditorKeys();

                if (Input.GetKeyDown(KeyCode.K) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    GUI_StaticsEditor.ToggleEditor();
                }
                if (Input.GetKeyDown(KeyCode.Tab) && GUI_StaticsEditor.IsOpen())
                {
                    GUI_StaticsEditor.SelectMouseObject();
                }
            }
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
      
        /// <summary>
        /// Invoked by invoke repeating and onLevelWasLoaded gameevent. controls the visiblility of Statics
        /// </summary>
        public void updateCache()
        {
            if (HighLogic.LoadedSceneIsGame)
            {
                Vector3 playerPos = Vector3.zero;
                if (selectedObject != null)
                {
                    playerPos = selectedObject.gameObject.transform.position;
                    //Log.Normal("updateCache using selectedObject as playerPos");
                }
                else if (FlightGlobals.ActiveVessel != null)
                {
                    playerPos = FlightGlobals.ActiveVessel.transform.position;
                    //Log.Normal("updateCache using ActiveVessel as playerPos" + FlightGlobals.ActiveVessel.vesselName);
                }
                else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    var spaceCenterCam = (Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)) as SpaceCenterCamera2 []).First();
                    playerPos = spaceCenterCam.transform.position;
                    //Log.Normal("updateCache using SpaceCenter Camera 2 as playerPos");
                    StaticDatabase.activeBodyName = SpaceCenter.Instance.cb.name;
                }
                else if (Camera.main != null)
                {
                    playerPos = Camera.main.transform.position;
                    //Log.Normal("updateCache using Camera.main as playerPos");
                }
                else
                {
                    Log.UserInfo("KerbalKonstructs.updateCache could not determine playerPos. All hell now happens.");
                }

                StaticDatabase.UpdateCache(playerPos);
            }
        }


        /// <summary>
        /// Loads and places all model instances from the confignode.
        /// </summary>
        /// <param name="configurl"></param>
        /// <param name="model"></param>
        /// <param name="bSecondPass"></param>
		internal void loadInstances(UrlDir.UrlConfig configurl, StaticModel model, bool bSecondPass = false)
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

            foreach (ConfigNode instanceCfgNode in configurl.config.GetNodes("Instances"))
            {
                StaticInstance instance = new StaticInstance();
                instance.model = model;
                instance.configUrl = configurl;
                instance.configPath = configurl.url.Substring(0, configurl.url.LastIndexOf('/')) + ".cfg";
                instance.gameObject = Instantiate(model.prefab);
                if (instance.gameObject == null)
                {
                    Log.UserError("KK: Could not find " + model.mesh + ".mu! Did the modder forget to include it or did you actually install it?");
                    continue;
                }

                ConfigParser.ParseInstanceConfig(instance, instanceCfgNode);

                if (instance.CelestialBody == null)
                    continue;

                // create RadialPosition, If we don't have one.
                if (instance.RadialPosition.Equals(Vector3.zero) )
                {
                    if (instance.RefLatitude != 361f && instance.RefLongitude != 361f )
                    {                   
                        instance.RadialPosition = (instance.CelestialBody.GetRelSurfaceNVector(instance.RefLatitude , instance.RefLongitude).normalized * instance.CelestialBody.Radius);
                        Log.UserInfo("creating new RadialPosition for: " + instance.configPath + " " + instance.RadialPosition.ToString());
                    }
                    else
                    {
                        Log.UserError("Neither RadialPosition or RefLatitude+RefLongitude found: " + instance.gameObject.name);
                        continue;
                    }
                } else
                {
                    // create LAT & LON out of Radialposition
                    instance.RefLatitude = KKMath.GetLatitudeInDeg(instance.RadialPosition);
                    instance.RefLongitude = KKMath.GetLongitudeInDeg(instance.RadialPosition);
                }

                // sometimes we need a second pass.. (do we???)
                // 

                if (bSecondPass)
                {
                    Vector3 secondInstanceKey = instance.RadialPosition;
                    bool bSpaceOccupied = false;

                    
                    foreach (StaticInstance soThis in StaticDatabase.GetAllStatics().Where(x => x.RadialPosition == instance.RadialPosition))
                    {
                        Vector3 firstInstanceKey = soThis.RadialPosition;                      

                            if (soThis.model.mesh == instance.model.mesh)
                            {
                                if ((soThis.RadiusOffset == instance.RadiusOffset) && (soThis.RotationAngle == instance.RotationAngle))
                                {
                                    bSpaceOccupied = true;
                                    Log.UserWarning("Attempted to import identical custom instance to same RadialPosition as existing instance: Check for duplicate custom statics: " + Environment.NewLine
                                    + soThis.model.mesh + " : " + firstInstanceKey.ToString() + Environment.NewLine +
                                    "File1: " + soThis.configPath + Environment.NewLine +
                                    "File2: " + instance.configPath);
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

                    if (bSpaceOccupied)
                    {
                        //Debug.LogWarning("KK: Attempted to import identical custom instance to same RadialPosition as existing instance. Skipped. Check for duplicate custom statics you have installed. Did you export the custom instances to make a pack? If not, ask the mod-makers if they are duplicating the same stuff as each other.");
                        continue;
                    }
                }

                instance.spawnObject(false, false);

                AttachFacilities(instance, instanceCfgNode);

                LaunchSiteManager.AttachLaunchSite(instance, instanceCfgNode);

            }

        }


        /// <summary>
        /// Loads all Squad assets into the ModelDatabase
        /// </summary>
        internal void LoadSquadModels()
        {
            LoadSquadKSCModels();
            LoadSquadAnomalies();
            LoadSquadAnomaliesLevel2();
            LoadSquadAnomaliesLevel3();
        }


        /// <summary>
        /// Loads the Models from the KSC into the model Database
        /// </summary>
        public void LoadSquadKSCModels()
        {

            // first we find get all upgradeable facilities
            Upgradeables.UpgradeableObject[] upgradeablefacilities;
            upgradeablefacilities = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>();

            foreach (var facility in upgradeablefacilities)
            {
                for (int i = 0; i < facility.UpgradeLevels.Length; i++)
                {

                    string modelName = "KSC_" + facility.name + "_level_" + (i + 1).ToString();
                    string modelTitle = "KSC " + facility.name + " lv " + (i + 1).ToString();

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                        continue;

                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.keepConvex = true;
                    model.title = modelTitle;
                    model.mesh = modelName;
                    model.category = "Squad KSC";
                    model.author = "Squad";
                    model.manufacturer = "Squad";
                    model.description = "Squad original " + modelTitle;

                    model.isSquad = true;

                    // the runways have all the same spawnpoint.
                    if (facility.name.Equals("Runway", StringComparison.CurrentCultureIgnoreCase))
                        model.DefaultLaunchPadTransform = "End09/SpawnPoint";

                    // Launchpads also 
                    if (facility.name.Equals("LaunchPad", StringComparison.CurrentCultureIgnoreCase))
                        model.DefaultLaunchPadTransform = "LaunchPad_spawn";

                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = facility.UpgradeLevels[i].facilityPrefab;


                    StaticDatabase.RegisterModel(model, modelName);

                    // try to extract the wrecks from the facilities
                    var transforms = model.prefab.transform.GetComponentsInChildren<Transform>(true);
                    int wreckCount = 0;
                    foreach (var transform in transforms)
                    {

                        if (transform.name.Equals("wreck", StringComparison.InvariantCultureIgnoreCase))
                        {
                            wreckCount++;
                            StaticModel wreck = new StaticModel();
                            string wreckName = modelName + "_wreck_" + wreckCount.ToString();
                            wreck.name = wreckName;

                            // Fill in FakeNews errr values
                            wreck.path = "KerbalKonstructs/" + wreckName;
                            wreck.configPath = wreck.path + ".cfg";
                            wreck.keepConvex = true;
                            wreck.title = modelTitle + " wreck " + wreckCount.ToString();
                            wreck.mesh = wreckName;
                            wreck.category = "Squad KSC";
                            wreck.author = "Squad";
                            wreck.manufacturer = "Squad";
                            wreck.description = "Squad original " + wreck.title;

                            wreck.isSquad = true;
                            wreck.prefab = transform.gameObject;
                            wreck.prefab.GetComponent<Transform>().parent = null;
                            StaticDatabase.RegisterModel(wreck, wreck.name);

                        }
                    }


                }
            }

        }

        /// <summary>
        /// Loads all non KSC models into the ModelDatabase
        /// </summary>
        public static void LoadSquadAnomalies()
        {

            foreach (PQSCity pqs in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (pqs.gameObject.name == "KSC" || pqs.gameObject.name == "KSC2" || pqs.gameObject.name == "Pyramids"  || pqs.gameObject.name == "Pyramid")
                    continue;


                string modelName = "SQUAD_" + pqs.gameObject.name;
                string modelTitle = "Squad " + pqs.gameObject.name;

                // don't double register the models a second time (they will do this) 
                // maybe with a "without green flag" and filter that our later at spawn in mangle
                if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                    continue;

                StaticModel model = new StaticModel();
                model.name = modelName;

                // Fill in FakeNews errr values
                model.path = "KerbalKonstructs/" + modelName;
                model.configPath = model.path + ".cfg";
                model.keepConvex = true;
                model.title = modelTitle;
                model.mesh = modelName;
                model.category = "Squad Anomalies";
                model.author = "Squad";
                model.manufacturer = "Squad";
                model.description = "Squad original " + modelTitle;

                model.isSquad = true;


                // we reference only the original prefab, as we cannot instantiate an instance for some reason
                model.prefab = pqs.gameObject;


                StaticDatabase.RegisterModel(model, modelName);

            }

            foreach (PQSCity2 pqs2 in Resources.FindObjectsOfTypeAll<PQSCity2>())
            {

                string modelName = "SQUAD_" + pqs2.gameObject.name;
                string modelTitle = "Squad " + pqs2.gameObject.name;

                // don't double register the models a second time (they will do this) 
                // maybe with a "without green flag" and filter that our later at spawn in mangle
                if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                    continue;

                StaticModel model = new StaticModel();
                model.name = modelName;

                // Fill in FakeNews errr values
                model.path = "KerbalKonstructs/" + modelName;
                model.configPath = model.path + ".cfg";
                model.keepConvex = true;
                model.title = modelTitle;
                model.mesh = modelName;
                model.category = "Squad Anomalies";
                model.author = "Squad";
                model.manufacturer = "Squad";
                model.description = "Squad original " + modelTitle;

                model.isSquad = true;


                // we reference only the original prefab, as we cannot instantiate an instance for some reason
                model.prefab = pqs2.gameObject;
                StaticDatabase.RegisterModel(model, modelName);
            }
        }

        /// <summary>
        /// Loads the statics of the KSC2
        /// </summary>
        public static void LoadSquadAnomaliesLevel2()
        {

            foreach (PQSCity pqs in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (pqs.gameObject.name != "KSC2" && pqs.gameObject.name != "Pyramids")
                    continue;

                GameObject baseGameObject = pqs.gameObject;
                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject)
                    {
                        continue;
                    }

                    string modelName = "SQUAD_" + pqs.gameObject.name + "_" + child.gameObject.name;
                    string modelTitle = "Squad " + pqs.gameObject.name + " " + child.gameObject.name;

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                        continue;

                    // filter out some unneded stuff
                    if (modelName.Contains("ollider") || modelName.Contains("onolit"))
                        continue;


                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.keepConvex = true;
                    model.title = modelTitle;
                    model.mesh = modelName;
                    model.category = "Squad Anomalies";
                    model.author = "Squad";
                    model.manufacturer = "Squad";
                    model.description = "Squad original " + modelTitle;

                    model.isSquad = true;


                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = child.gameObject;


                    StaticDatabase.RegisterModel(model, modelName);
                }
            }
        }

        /// <summary>
        /// used for loading the pyramid parts
        /// </summary>
        public static void LoadSquadAnomaliesLevel3()
        {

            foreach (PQSCity pqs in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (pqs.gameObject.name != "Pyramids") 
                    continue;


                // find the lv2 parent
                GameObject baseGameObject = pqs.gameObject;
                GameObject baseGameObject2 = null;

                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject)
                    {
                        continue;
                    }
                    else
                    {
                        baseGameObject2 = child.gameObject;
                    }
                }


                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject2)
                    {
                        continue;
                    }

                    string modelName = "SQUAD_" + pqs.gameObject.name + "_" + child.gameObject.name;
                    string modelTitle = "Squad " + pqs.gameObject.name + " " + child.gameObject.name;

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                        continue;

                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.keepConvex = true;
                    model.title = modelTitle;
                    model.mesh = modelName;
                    model.category = "Squad Anomalies";
                    model.author = "Squad";
                    model.manufacturer = "Squad";
                    model.description = "Squad original " + modelTitle;

                    model.isSquad = true;


                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = child.gameObject;


                    StaticDatabase.RegisterModel(model, modelName);
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
                ConfigParser.ParseModelConfig(model, conf.config);
                model.name = modelName;
                model.mesh = model.mesh.Substring(0, model.mesh.LastIndexOf('.'));
                model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
                model.config = conf.url;
                model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
                //                model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());


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
                    if (model.modules == null)
                        model.modules = new List<StaticModule>();

                    model.modules.Add(module);
                }
                model.prefab = GameDatabase.Instance.GetModelPrefab(model.path + "/" + model.mesh);

                if (model.prefab == null)
                {
                    Debug.Log("KK: Could not find " + model.mesh + ".mu! Did the modder forget to include it or did you actually install it?");
                    continue;
                }
                if (model.keepConvex == true)
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
                StaticDatabase.RegisterModel(model, modelName);
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

                StaticModel model = StaticDatabase.GetModelByName(modelname);
                if (model != null)
                {
                    loadInstances(conf, model, true);
                }
                else { Log.UserError("No Model named " + modelname + " found as defined in: " + conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg"); }
            }
        }


        /// <summary>
        /// Parses a cfgnode and adds a corresponding facility component to the static instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="cfgNode"></param>
        internal static void AttachFacilities(StaticInstance instance, ConfigNode cfgNode)
        {            
            if (!cfgNode.HasValue("FacilityType") && !cfgNode.HasNode("Facility"))
            {
                return;
            }
            KKFacilityType facType;
            try
            {
                facType = (KKFacilityType)Enum.Parse(typeof(KKFacilityType), cfgNode.GetValue("FacilityType"), true);                
            }
            catch
            {
                instance.legacyfacilityID = cfgNode.GetValue("FacilityType");
                instance.FacilityType = "None";
                instance.facilityType = KKFacilityType.None;
                facType = KKFacilityType.None;
                //Log.UserError("Unknown Facility Type: " + cfgNode.GetValue("FacilityType") + " in file: " + instance.configPath );
            }


            if (facType == KKFacilityType.None && !cfgNode.HasNode("Facility"))
            {
                return;
                
            }
            // Stuff for recursive Facilities
            instance.hasFacilities = true;
            instance.facilityType = facType;
            instance.FacilityType = cfgNode.GetValue("FacilityType");

            switch (facType)
            {
                case KKFacilityType.GroundStation:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TrackingStation:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    instance.facilityType = KKFacilityType.GroundStation;
                    break;
                case KKFacilityType.FuelTanks:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<FuelTanks>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Research:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<Research>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Business:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<Business>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Hangar:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<Hangar>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Barracks:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<Barracks>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.LandingGuide:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<LandingGuide>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TouchdownGuideL:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<TouchdownGuideL>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TouchdownGuideR:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<TouchdownGuideR>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.RadarStation:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<RadarStation>().ParseConfig(cfgNode));
                    break;
            }

            
            //attach multiple failities
            foreach (ConfigNode facNode in cfgNode.GetNodes("Facility"))
            {                
                AttachFacilities(instance, facNode);
            }

        }

        /// <summary>
        /// saves the model definition and the direct instances
        /// </summary>
        /// <param name="mModelToSave"></param>
        internal void saveModelConfig(StaticModel mModelToSave)
        {
            StaticModel model = StaticDatabase.GetModelByName(mModelToSave.name);


            ConfigNode staticNode = new ConfigNode("STATIC");
            ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

            ConfigParser.WriteModelConfig(model, modelConfig);

            modelConfig.RemoveNodes("Instances");

            foreach (StaticInstance instance in StaticDatabase.GetInstancesFromModel(model))
            {
                ConfigNode inst = new ConfigNode("Instances");
                ConfigParser.WriteInstanceConfig(instance, inst);
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
            Log.Normal("Saving File: " + pathname);
            StaticInstance [] allInstances = StaticDatabase.allStaticInstances.Where(instance => instance.configPath == pathname).ToArray();          
            StaticInstance firstInstance = allInstances.First();
            ConfigNode instanceConfig = null;

            ConfigNode staticNode = new ConfigNode("STATIC");

            if (firstInstance.configUrl == null) //this are newly spawned instances
            {
                instanceConfig = new ConfigNode("STATIC");
                instanceConfig.AddValue("pointername", firstInstance.model.name);
            }
            else
            {
                //instanceConfig = GameDatabase.Instance.GetConfigNode(firstInstance.configUrl.url);
                //instanceConfig.RemoveNodes("Instances");
                //instanceConfig.RemoveValues();
                instanceConfig = new ConfigNode("STATIC");
                instanceConfig.AddValue("pointername", firstInstance.model.name);

            }

            staticNode.AddNode(instanceConfig);
            foreach (StaticInstance instance in allInstances)
            {
                ConfigNode inst = new ConfigNode("Instances");
                ConfigParser.WriteInstanceConfig(instance, inst);
                instanceConfig.nodes.Add(inst);
            }
            staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + firstInstance.configPath, "Generated by Kerbal Konstructs");
        }



        /// <summary>
        /// This saves all satic objects to thier instance files.. 
        /// </summary>
        public void saveObjects()
        {
            HashSet<String> processedInstances = new HashSet<string>();
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                // ignore allready processed cfg files
                if (processedInstances.Contains(instance.configPath))
                {
                    continue;
                }

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
            foreach (StaticInstance deletedInstance in deletedInstances)
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




        //public void exportCustomInstances(string sPackName = "MyStaticPack", string sBaseName = "All", string sGroup = "", Boolean bLocal = false)
        //{
        //    bool HasCustom = false;
        //    string sBase = "";

        //    if (sGroup != "") sBase = sGroup;
        //    else
        //        sBase = sBaseName;

        //    foreach (StaticModel model in StaticDatabase.allStaticModels)
        //    {
        //        HasCustom = false;
        //        ConfigNode staticNode = new ConfigNode("STATIC");
        //        ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

        //        modelConfig.RemoveNodes("Instances");

        //        foreach (StaticObject instance in StaticDatabase.GetInstancesFromModel(model))
        //        {
                    
        //            string sInstGroup = (string)instance.getSetting("Group");

        //            if (sGroup != "")
        //            {
        //                if (sInstGroup != sGroup)
        //                {
        //                    sInstGroup = "";
        //                    continue;
        //                }
        //            }

        //            if (DevMode)
        //            {
        //                sCustom = "True";
        //                //obj.setSetting("CustomInstance", "True");
        //            }

        //            if (sCustom == "True")
        //            {
        //                HasCustom = true;
        //                ConfigNode inst = new ConfigNode("Instances");
        //                foreach (KeyValuePair<string, object> setting in instance.settings)
        //                {
        //                    inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
        //                }
        //                modelConfig.nodes.Add(inst);
        //            }
        //        }

        //        if (HasCustom)
        //        {
        //            string sModelName = modelConfig.GetValue("name");
        //            modelConfig.AddValue("pointername", sModelName);

        //            modelConfig.RemoveValue("name");
        //            modelConfig.AddValue("name", sPackName + "_" + sBase + "_" + sModelName);

        //            staticNode.AddNode(modelConfig);
        //            if (DevMode)
        //            {
        //                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sBase);
        //                staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sBase + "/" + sModelName + ".cfg", "Exported custom instances by Kerbal Konstructs");
        //            }
        //            else
        //            {
        //                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + sBase + "/" + model.path);
        //                staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + sBase + "/" + model.configPath, "Exported custom instances by Kerbal Konstructs");
        //            }
        //        }
        //    }
        //}

        public void exportMasters()
        {
            string sBase = "";
            string activeBodyName = "";

            Dictionary<string, Dictionary<string, StaticGroup>> groupList = new Dictionary<string, Dictionary<string, StaticGroup>>();

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                String bodyName = instance.CelestialBody.bodyName;
                String groupName = instance.Group;

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

                    foreach (StaticModel model in StaticDatabase.allStaticModels)
                    {
                        ConfigNode staticNode = new ConfigNode("STATIC");
                        ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

                        //Debug.Log("Model is " + model.getSetting("name"));

                        modelConfig.RemoveNodes("Instances");
                        bool bNoInstances = true;

                        foreach (StaticInstance obj in StaticDatabase.GetInstancesFromModel(model))
                        {
                            string sObjGroup = obj.Group;
                            if (sObjGroup != sBase) continue;

                            ConfigNode inst = new ConfigNode("Instances");

                            ConfigParser.WriteInstanceConfig(obj,inst);
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

        public void deleteObject(StaticInstance obj)
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

            StaticDatabase.RemoveStatic(obj);
        }

        public void setSnapTarget(StaticInstance obj)
        {
            snapTargetInstance = obj;
        }

        public void selectObject(StaticInstance obj, bool isEditing, bool bFocus, bool bPreview)
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


        public CelestialBody getCurrentBody()
        {
            return currentBody;
            //ToDo: FlightGlobals.currentMainBody;
        }

        #endregion

        #region Config Methods

        /// <summary>
        /// Loads the settings of KK
        /// </summary>
        /// <returns></returns>
        public void LoadKKConfig(ConfigNode kkConfigNode)
        {

            ConfigNode cfg = null;

            if (kkConfigNode.HasNode("KKSettings"))
            {
                cfg = kkConfigNode.GetNode("KKSettings");
            }

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
            }
        }

        /// <summary>
        /// Saves the default settings of KK
        /// </summary>
        public void SaveKKConfig(ConfigNode kkConfigNode)
        {
            ConfigNode cfg;
            if (kkConfigNode.HasNode("KKSettings"))
            {
                cfg = kkConfigNode.GetNode("KKSettings");
                cfg.ClearData();
            }
            else
            {
                cfg = kkConfigNode.AddNode("KKSettings");
            }


            foreach (FieldInfo f in GetType().GetFields())
            {
                if (Attribute.IsDefined(f, typeof(KSPField)))
                {
                    cfg.AddValue(f.Name, f.GetValue(this));
                }
            }
        }

        #endregion

    }
}

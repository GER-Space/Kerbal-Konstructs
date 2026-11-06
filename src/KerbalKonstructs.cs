using KerbalKonstructs.Addons;
using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.UI;
using KSP.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


namespace KerbalKonstructs
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbalKonstructs : MonoBehaviour
    {
        // Hello
        public static KerbalKonstructs instance;

        internal static readonly string sKKVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        #region Holders
        internal static StaticInstance selectedInstance = null;
        internal StaticModel selectedModel;
        internal static CameraController camControl = new CameraController();
        private CelestialBody currentBody;
        internal static bool InitialisedFacilities = false;

        internal static bool scCamWasAltered = false;

        internal double recoveryExraRefund = 0;

        internal static double gameTime = -1d;
        internal static float localGroupRange = 25000f;
        internal static bool convertLegacyConfigs = false;

        internal static int vectorLayer = 11;

        #endregion


        #region Configurable Variables    
        internal bool enableRT
        {
            get
            {
                if (RemoteTechAddon.isInstalled)
                {
                    return HighLogic.CurrentGame?.Parameters?.CustomParams<KKCustomParameters0>()?.enableRT == true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().enableRT = value;
            }
        }
        internal bool enableCommNet
        {
            get
            {
                if (CommNet.CommNetScenario.CommNetEnabled)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().enableCommNet;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().enableCommNet = value;
            }
        }
        internal bool launchFromAnySite { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().launchFromAnySite; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().launchFromAnySite = value; } }
        internal bool discoverAllBases 
        {
            get
            {
                if (HighLogic.CurrentGame != null)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().discoverAllBases;
                } else
                {
                    return false;
                }
            }
            set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().discoverAllBases = value; }
        }

        internal bool disableCareerStrategyLayer
        {
            get
            {
                if (HighLogic.CurrentGame != null)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().disableCareerStrategyLayer;
                } 
                else
                {
                    return false;
                }
            }
            set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().disableCareerStrategyLayer = value; }
        }
        internal bool disableRemoteBaseOpening { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteBaseOpening; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteBaseOpening = value; } }
        internal double facilityUseRange { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().facilityUseRange; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().facilityUseRange = (float)value; } }
        internal bool disableRemoteRecovery { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteRecovery; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteRecovery = value; } }
        //internal double defaultRecoveryFactor { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultRecoveryFactor; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultRecoveryFactor = (float)value; } }
        //internal double defaultEffectiveRange { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultEffectiveRange; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultEffectiveRange = (float)value; } }
        internal bool toggleIconsWithBB { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().toggleIconsWithBB; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().toggleIconsWithBB = value; } }
        internal static float soundMasterVolume { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().soundMasterVolume; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().soundMasterVolume = value; } }
        internal double maxEditorVisRange { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().maxEditorVisRange; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().maxEditorVisRange = (float)value; } }
        internal bool debugMode
        {
            get
            {
                if (HighLogic.CurrentGame?.Parameters?.CustomParams<KKCustomParameters1>() is KKCustomParameters1 parms)
                    return parms.DebugMode;

                return false;
            }
            set
            {
                if (HighLogic.CurrentGame?.Parameters?.CustomParams<KKCustomParameters1>() is KKCustomParameters1 parms)
                    parms.DebugMode = value;
            }
        }
        internal bool spawnPreviewModels { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().spawnPreviewModels; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().spawnPreviewModels = value; } }
        internal static string newInstancePath { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().newInstancePath; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().newInstancePath = value; } }
        internal static bool useLegacyCamera => HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().useLegacyCamera;
        internal static bool useCam => HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().useCamUnderdroudColors;
        internal static bool enableInflightHighlight => HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().enableInflightHighlight;

        internal static bool focusLastLaunchSite { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().focusLastLaunchSite; } }
        internal bool dontRemoveStockCommNet { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().dontRemoveStockCommNet; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().disableCareerStrategyLayer = value; } }

        // map icon settings. These are saved manually
        [KSPField]
        public Boolean mapShowOpen = true;
        [KSPField]
        public Boolean mapShowClosed = false;
        [KSPField]
        public Boolean mapShowGroundStation = false;
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
        public Boolean mapShowRecovery = false;
        [KSPField]
        public string defaultVABlaunchsite = "LaunchPad";
        [KSPField]
        public string defaultSPHlaunchsite = "Runway";
        [KSPField]
        public string lastLaunchSiteUsed = "LaunchPad";

        #endregion

        internal static List<StaticInstance> deletedInstances = new List<StaticInstance>();
        internal static List<GroupCenter> deletedGroups = new List<GroupCenter>();

        internal static bool calledByAlterMHSelector = false;

        /// <summary>
        /// Unity GameObject Awake function
        /// </summary>
        public void Awake()
        {
            instance = this;
            var TbController = new ToolbarController();
            Log.PerfStart("Awake Function");

            #region Game Event Hooks
            GameEvents.onDominantBodyChange.Add(OnDominantBodyChange);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoad);
            GameEvents.onGUIApplicationLauncherReady.Add(TbController.OnGUIAppLauncherReady);
            GameEvents.onVesselRecoveryProcessing.Add(OnProcessRecoveryProcessing);
            GameEvents.OnVesselRollout.Add(OnVesselLaunched);
            // draw map icons when needed
            GameEvents.OnMapEntered.Add(MapIconDraw.instance.Open);
            GameEvents.OnMapExited.Add(MapIconDraw.instance.Close);
            GameEvents.OnGameDatabaseLoaded.Add(OnGameDatabaseLoaded);
            GameEvents.onVesselGoOffRails.Add(FixWaterLaunch);
            GameEvents.onGameSceneSwitchRequested.Add(OnSceneSwitchRequested);
            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {

                GameEvents.onGUILaunchScreenSpawn.Add(OnSelectorLoaded);
                GameEvents.onEditorRestart.Add(OnEditorRestart);
                //GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            }
            #endregion

            #region Other Mods Hooks
            StageRecovery.AttachStageRecovery();
            #endregion

            KKGraphics.LoadShaderBundles();
            //SDTest.LoadBundles();

            SpaceCenterManager.setKSC();
            ConnectionManager.ScanForStockCommNet();

            DontDestroyOnLoad(this);

            KKClassExtentions.FakeGameObjectTags();

            // Squad stoff
            SquadStatics.LoadSquadModels();
            MapDecalUtils.GetSquadMaps();

            ConfigParser.LoadAllMapDecalMaps();
            BuiltinCenters.AddAllBuiltinCenters();

            LaunchSiteManager.AddKSCSites();

            ConfigParser.LoadAllGroupCenter();

            ConfigParser.LoadAllMapDecals();



            ScExtention.TuneFacilities();
            LaunchSiteChecks.PrepareSystem();


            Log.PerfStart("Object loading1");

            LoadModels();
            //  SDTest.WriteTextures();

            Log.PerfStop("Object loading1");
            Log.PerfStart("Object loading2");

            LoadModelInstances();

            Log.PerfStop("Object loading2");


            Log.UserInfo("Version is " + sKKVersion + " .");

            Log.UserInfo("StaticDatabase has: " + StaticDatabase.allStaticInstances.Count() + "Entries");

            Log.PerfStop("Awake Function");
            //Log.PerfStart("Model Test");
            //SDTest.GetModelStats();
            //Log.PerfStop("Model Test");
            //SDTest.GetShaderStats();
            //SDTest.ScanParticles();
            //KKGraphics.LoadShaders();
            //KKGraphics.GetBuiltinTexture("", 0);

            //SDTest.InstallDetour();

        }

        public void Start()
        {
            //Log.PerfStart("Object loading1");

            //LoadModels();
            ////  SDTest.WriteTextures();

            //Log.PerfStop("Object loading1");
            //Log.PerfStart("Object loading2");

            //LoadModelInstances();

            //Log.PerfStop("Object loading2");
            UI2.WindowManager2.Initialize();

        }




        #region Game Events

        /// <summary>
        /// called by onVesselGoOffRails
        /// </summary>
        void FixWaterLaunch(Vessel vessel)
        {
            if (vessel.situation == Vessel.Situations.PRELAUNCH)
            {
                KKLaunchSite lastSite = LaunchSiteManager.GetCurrentLaunchSite();
                if ((lastSite.sitecategory == LaunchSiteCategory.Waterlaunch && !lastSite.ToggleLaunchPositioning) || (lastSite.sitecategory != LaunchSiteCategory.Waterlaunch && lastSite.ToggleLaunchPositioning))
                {
                    Log.Normal("Trying to bring the vessel back to the spawn point.");
                    vessel.SetPosition(lastSite.staticInstance.mesh.transform.Find(lastSite.LaunchPadTransform).position);
                }
            }
        }

        internal void OnSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> fromTo)
        {
            //GameScenes targetScene = fromTo.to;

            //if (targetScene == GameScenes.SPACECENTER && fromTo.from != GameScenes.MAINMENU)
            //{
            //    Log.Normal("Requested scene is SpaceCenter");
            //    FuckUpKSP();
            //}
        }

        internal void FuckUpKSP()
        {

            if (!KerbalKonstructs.focusLastLaunchSite)
            {
                return;
            }

            Log.Normal("Let the fun begin");

            KKLaunchSite currentSite = LaunchSiteManager.GetCurrentLaunchSite();
            if (currentSite == null)
            {
                currentSite = LaunchSiteManager.GetLaunchSiteByName("LaunchPad");
            }
            currentBody = currentSite.body;

            if (FlightGlobals.currentMainBody != currentBody)
            {
                FlightGlobals.currentMainBody = currentBody;
                PQSMod_CelestialBodyTransform bodyPQS = currentBody.GetComponentInChildren<PQSMod_CelestialBodyTransform>(true);

                if (bodyPQS == null)
                {
                    Log.Normal("cannot find CB PQS: " + currentBody.name);
                    return;
                }

                Log.Normal("switching to: " + currentBody.name);

                PSystemSetup.Instance.GetType().GetField("pqs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(PSystemSetup.Instance, currentBody.pqsController);
                PSystemSetup.Instance.GetType().GetField("cb", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(PSystemSetup.Instance, bodyPQS);
                PSystemSetup.Instance.GetType().GetField("scTransform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(PSystemSetup.Instance, currentSite.staticInstance.transform);
                //    currentBody.pqsController.target = currentSite.lsGameObject.transform;
                //    currentBody.pqsController.StartUpSphere();
                //currentBody.pqsController.ForceStart();

                //  currentBody.pqsController.EnableSphere();

                //    PSystemSetup.Instance.GetType().GetMethod("SetSpaceCentre", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(PSystemSetup.Instance, null);


                ConfigUtil.GetCelestialBody("HomeWorld").isHomeWorld = false;
                ConfigUtil.bodiesInitialized = false;
                currentBody.isHomeWorld = true;
                Planetarium.fetch.Home = currentBody;

            }

            StaticDatabase.lastActiveBody = currentSite.body;


            foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
            {
                scCam.GetType().GetField("pqs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(scCam, currentBody.pqsController);
            }



            if (!currentSite.isSquad)
            {
                currentSite.staticInstance.groupCenter.SetInstancesEnabled(true);
            }


        }

        /// <summary>
        /// Updates the mission log and processes the launch refund.
        /// </summary>
        /// <param name="vVessel"></param>
        void OnVesselLaunched(ShipConstruct vVessel)
        {

            string sitename = LaunchSiteManager.getCurrentLaunchSite();
            FlightGlobals.ActiveVessel.SetLandedAt(sitename);
            FlightGlobals.ActiveVessel.protoVessel.landedAt = sitename;

            if (sitename == "Runway" || sitename == "LaunchPad" || sitename == "KSC" || string.IsNullOrEmpty(sitename))
            {
                return;
            }
            // reset the name to the site, so it can be fetched again
            //KKLaunchSite lastSite = LaunchSiteManager.GetCurrentLaunchSite();
            //lastSite.spaceCenterFacility.name = lastSite.LaunchSiteName;



            if (!CareerUtils.isCareerGame)
            {
                return;
            }
            else
            {
                Log.Normal("OnVesselLaunched is Career");

                KKLaunchSite lsSite = LaunchSiteManager.GetLaunchSiteByName(sitename);
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
            }
        }

        /// <summary>
        /// GameEvent function for toggeling the visiblility of Statics
        /// </summary>
        /// <param name="data"></param>
        void OnLevelWasLoad(GameScenes data)
        {
            

            CameraController.ResetNearCam();

            DeletePreviewObject();

            StaticDatabase.ToggleActiveAllStatics(false);

            if (selectedInstance != null)
            {
                DeselectObject(false, true);
                camControl.active = false;
            }
            CancelInvoke("UpdateCache");

            //FlightCamera.fetch.cameras[0].farClipPlane = 400f;
            //FlightCamera.fetch.cameras[1].nearClipPlane = 397f;

            switch (data)
            {
                case GameScenes.FLIGHT:
                    {

                        //EditorGUI.camInitialized = false;

                        InputLockManager.RemoveControlLock("KKShipLock");
                        InputLockManager.RemoveControlLock("KKEVALock");
                        InputLockManager.RemoveControlLock("KKCamModes");
                        InputLockManager.RemoveControlLock("KKCamModes2");
                        InputLockManager.RemoveControlLock("KKActions");


                        if (FlightGlobals.ActiveVessel != null)
                        {
                            //StaticDatabase.ToggleActiveStaticsOnPlanet(FlightGlobals.ActiveVessel.mainBody, true, true);
                            currentBody = FlightGlobals.ActiveVessel.mainBody;
                            StaticDatabase.OnBodyChanged(FlightGlobals.currentMainBody);
                            UpdateCache();
                            //Hangar.DoHangaredCraftCheck();
                        }
                        else
                        {
                            Log.Debug("Flight scene load. No activevessel. Activating all statics.");
                            StaticDatabase.ToggleActiveAllStatics(true);
                        }
                        InvokeRepeating("UpdateCache", 0, 1);
                    }
                    break;
                case GameScenes.EDITOR:
                    {

                        // Prevent abuse if selector left open when switching to from VAB and SPH
                        LaunchSiteSelectorGUI.instance.Close();
                        KKLaunchSite currentSite = LaunchSiteManager.GetCurrentLaunchSite();

                        //if (currentSite.LaunchSiteType == SiteType.Any)
                        //{
                        //    currentSite.spaceCenterFacility.editorFacility = EditorDriver.editorFacility;
                        //    LaunchSiteManager.SetupKSPFacilities();
                        //}

                        //Log.Normal("");
                        //Log.Normal("Valid sites");
                        //foreach (var site in EditorDriver.ValidLaunchSites)
                        //{
                        //    Log.Normal("Stock site: " + site);
                        //}

                        // Check if the selected LaunchSite is valid
                        if (LaunchSiteManager.CheckLaunchSiteIsValid(currentSite) == false)
                        {
                            Log.Normal("LS not valid: " + LaunchSiteManager.getCurrentLaunchSite());
                            currentSite = LaunchSiteManager.GetDefaultSite();
                        }
                        LaunchSiteManager.setLaunchSite(currentSite);
                        if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
                        {
                            LaunchSiteManager.AlterMHSelector();
                        }

                    }
                    break;
                case GameScenes.SPACECENTER:
                    {
                        Log.PerfStart("SC Scene");
                        if (convertLegacyConfigs)
                        {
                            Log.UserWarning("KK converts your configs to a new format");
                            SaveObjects();
                            convertLegacyConfigs = false;
                        }
                        InputLockManager.RemoveControlLock("KKShipLock");
                        InputLockManager.RemoveControlLock("KKEVALock");
                        InputLockManager.RemoveControlLock("KKCamModes");
                        InputLockManager.RemoveControlLock("KKCamModes2");
                        InputLockManager.RemoveControlLock("KKActions");
                        //SquadStatics.PimpLevel2Runway();
                        KKLaunchSite currentSite = LaunchSiteManager.GetCurrentLaunchSite();
                        if (currentSite == null)
                        {
                            currentSite = LaunchSiteManager.GetLaunchSiteByName("LaunchPad");
                        }

                        //currentBody = currentSite.body;
                        currentBody = ConfigUtil.GetCelestialBody("HomeWorld");
                        // 
                        //}
                        Log.Normal("SC Body is: " + currentBody.name);
                        StaticDatabase.OnBodyChanged(currentBody);
                        //updateCache();
                        if (scCamWasAltered || focusLastLaunchSite)
                        {
                            CameraController.SetSpaceCenterCam(currentSite);
                        }
                        UpdateCache();
                        Log.PerfStop("SC Scene");
                        // LaunchSiteManager.AlterMHSelector();
                    }
                    break;
                case GameScenes.MAINMENU:
                    {
                        gameTime = -1f;
                        CareerState.ResetFacilitiesOpenState();
                        scCamWasAltered = false;
                        // reset this for the next Newgame
                        if (InitialisedFacilities)
                        {
                            InitialisedFacilities = false;
                        }
                    }
                    break;
                default:
                    break;
            }

        }


        void OnDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
        {
            StaticDatabase.OnBodyChanged(data.to);
            currentBody = data.to;
            UpdateCache();
        }


        public void OnSelectorLoaded(GameEvents.VesselSpawnInfo info)
        {
            if (!HighLogic.CurrentGame.Parameters.Difficulty.AllowOtherLaunchSites)
            {
                return;
            }
            Log.Normal("Reseting LaunchSite Lists on Selector spawn");
            IEnumerator coroutine = WaitAndReset(info);
            StartCoroutine(coroutine);
        }


        public IEnumerator WaitAndReset(GameEvents.VesselSpawnInfo info)
        {
            yield return new WaitForSeconds(2);
            //LaunchSiteManager.ResetLaunchSites();
            LaunchSiteManager.RegisterMHLaunchSites(info.callingFacility.facilityType);

            KSP.UI.UILaunchsiteController uILaunchsiteController = Resources.FindObjectsOfTypeAll<KSP.UI.UILaunchsiteController>().FirstOrDefault();
            if (uILaunchsiteController == null)
            {
                Log.UserWarning("LaunchSitecontroller not found");
            }
            else
            {
                uILaunchsiteController.GetType().GetMethod("resetItems", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(uILaunchsiteController, null);
            }
        }

        public void OnEditorRestart()
        {
            //Log.Normal("On Editor Restart");

            // this is a switch to prevent endless loops. we will call this GameEvent function from within AlterMHSelector so it does some magic for us. 
            // is need to call call it by itself because there might be some editor switch, so we implement this semaphore.
            if (!KerbalKonstructs.calledByAlterMHSelector)
            {
                LaunchSiteManager.AlterMHSelector();
            }
        }


        /// <summary>
        /// fills the basic values of the 
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="dialog"></param>
        /// <param name="recovery"></param>
        void OnProcessRecoveryProcessing(ProtoVessel vessel, MissionRecoveryDialog dialog, float recovery)
        {
            if (!disableRemoteRecovery && CareerUtils.isCareerGame && (vessel != null))
            {

                Log.Normal("OnProcessRecovery");
                CustomSpaceCenter customSC = null;

                double smallestDist = SpaceCenterManager.KSC.GreatCircleDistance(SpaceCenterManager.KSC.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));

                Log.Normal("Distance to KSC is " + smallestDist);

                foreach (CustomSpaceCenter csc in SpaceCenterManager.spaceCenters)
                {
                    if (csc.isOpen == false)
                    {
                        continue;
                    }

                    SpaceCenter closestSpaceCenter = csc.GetSpaceCenter();
                    double dist = closestSpaceCenter.GreatCircleDistance(csc.staticInstance.CelestialBody.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));

                    if (dist < smallestDist)
                    {
                        customSC = csc;
                        smallestDist = dist;
                        Log.Normal("closest updated to " + csc.SpaceCenterName + ", distance " + smallestDist);
                    }
                }

                if (customSC != null)
                {
                    float oldRecovery = recovery;
                    double shipvalue = dialog.fundsEarned / oldRecovery;
                    double missingvalue = shipvalue - dialog.fundsEarned;
                    if (smallestDist < 2500d)
                    {
                        recovery = 1;
                        dialog.fundsEarned = shipvalue;
                        dialog.recoveryFactor = "100%";
                        dialog.recoveryLocation = "Landed at " + customSC.SpaceCenterName;
                    }
                    else
                    {
                        recovery = (float)Math.Max(0, (0.98 - ((smallestDist / 1000) / 2150)));
                        dialog.fundsEarned = shipvalue * recovery;
                        dialog.recoveryFactor = Math.Round(recovery * 100, 1).ToString() + "%";
                        dialog.recoveryLocation = Math.Round(smallestDist / 1000, 1) + "km from " + customSC.SpaceCenterName;
                    }

                    foreach (var part in dialog.GetComponentsInChildren<KSP.UI.Screens.SpaceCenter.MissionSummaryDialog.PartWidget>(true))
                    {
                        Log.Normal("Part: " + part.partValue);
                        part.partValue = part.partValue / oldRecovery * recovery;
                        part.totalValue = part.totalValue / oldRecovery * recovery;
                    }

                    foreach (var resource in dialog.GetComponentsInChildren<KSP.UI.Screens.SpaceCenter.MissionSummaryDialog.ResourceWidget>(true))
                    {
                        Log.Normal("Part: " + resource.unitValue);
                        resource.unitValue = resource.unitValue / oldRecovery * recovery;
                        resource.totalValue = resource.totalValue / oldRecovery * recovery;
                    }

                    Funding.Instance.AddFunds(missingvalue, TransactionReasons.VesselRecovery);
                }
            }
        }


        /// <summary>
        /// Highly experimental Code for reloading all assets after the GameDatabase reloaded
        /// </summary>
        void OnGameDatabaseLoaded()
        {
            Log.UserWarning("GameDatabase Load Event triggered. Trying to rebuild all assets");

            // Delete everything

            // instances
            if (selectedInstance != null)
            {
                DeselectObject(true, false);
            }
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.hasLauchSites)
                {
                    LaunchSiteManager.DeleteLaunchSite(instance.launchSite);
                }
                DeleteInstance(instance);
            }
            deletedInstances.Clear();

            // reset the Database to empty state, as nothing should be loaded anymore
            StaticDatabase.Reset();
            ConnectionManager.ResetAll();
            DecalsDatabase.ResetAll();

            // Load up everything
            // PQSMapDecal
            Log.PerfStart("loading MapDecals");
            MapDecalUtils.GetSquadMaps();
            ConfigParser.LoadAllMapDecalMaps();
            ConfigParser.LoadAllMapDecals();
            Log.PerfStop("loading MapDecals");
            // end PQSMapDecal
            Log.PerfStart("Loading Instances");

            SquadStatics.LoadSquadModels();

            LoadModels();
            //  SDTest.WriteTextures();

            LoadModelInstances();
            Log.PerfStop("Loading Instances");
            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
            {
                //ConfigNode careerNode = ConfigNode.Load(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs");
                //if (careerNode == null)
                //{
                //    Log.Error("Cannot find persitence file");
                //}
                //else
                //{
                //    CareerState.Load(careerNode.GetNode("SCENARIO", "name", "KerbalKonstructsSettings"));
                //}
                UpdateCache();
            }


        }

        /// <summary>
        /// Unity Late Update. Used for KeyCodes and fixing facility levels on new games...
        /// </summary>
        public void LateUpdate()
        {

            // Check if we don't have the KSC Buildings in the savegame and save them there if missing.
            // this is needed, because for some reason we set all buildings directly to max level without.
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                CareerState.FixKSCFacilities();
            }

            if (Input.GetKeyDown(KeyCode.L) && ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))))
            {
                UI2.EditorModeSelector.Toggle();
            }


            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                EditorGUI.instance.CheckEditorKeys();

                GroupEditor.instance.CheckEditorKeys();


                if (Input.GetKeyDown(KeyCode.K) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    StaticsEditorGUI.instance.ToggleEditor();
                }
                if (Input.GetKeyDown(KeyCode.Tab) && StaticsEditorGUI.instance.IsOpen())
                {
                    StaticsEditorGUI.instance.SelectMouseObject();
                }
                //if (Input.GetMouseButtonDown(0)&& StaticsEditorGUI.instance.IsOpen())
                //{
                //    StaticsEditorGUI.instance.SelectMouseObject();
                //}

                //if (KerbalKonstructs.instance.debugMode && (Input.GetKeyDown(KeyCode.L) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))))
                //{
                //    API.SpawnObject("KKflagDemo");
                //}

                if (useLegacyCamera && camControl.active)
                {
                    camControl.updateCamera();
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
        public void UpdateCache()
        {
            KerbalKonstructs.gameTime = HighLogic.CurrentGame.UniversalTime;

            if (HighLogic.LoadedSceneIsGame)
            {
                // Don't update visiblility when Editor is open
                if (StaticsEditorGUI.instance.IsOpen())
                {
                    return;
                }

                Vector3 playerPos = Vector3.zero;
                if (selectedInstance != null)
                {
                    playerPos = selectedInstance.position;
                    //Log.Normal("updateCache using selectedObject as playerPos");
                }
                else if (FlightGlobals.ActiveVessel != null)
                {
                    playerPos = FlightGlobals.ActiveVessel.GetWorldPos3D();
                    //Log.Normal("updateCache using ActiveVessel as playerPos" + FlightGlobals.ActiveVessel.vesselName);
                }
                else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    SpaceCenterCamera2 spaceCenterCam = Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>().FirstOrDefault();
                    if (spaceCenterCam.gameObject.transform.parent.transform.parent != null)
                    {
                        Log.Normal("using SpaceCenterCam2 as position");
                        Log.Normal("SC2Name: " + spaceCenterCam.gameObject.transform.name);
                        playerPos = spaceCenterCam.gameObject.transform.position;
                    }
                    else
                    {
                        Log.Normal("No SpaceCenterCam Found in SpaceCenter Scene");
                        // we can try the current LaunchSite as fallback
                        playerPos = LaunchSiteManager.GetCurrentLaunchSite().staticInstance.transform.position;
                        //playerPos = SpaceCenter.Instance.transform.position;
                    }
                    //StaticDatabase.activeBodyName = SpaceCenter.Instance.cb.name;
                    // playerPos = SpaceCenter.Instance.transform.position;
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
		internal void LoadInstances(UrlDir.UrlConfig configurl, StaticModel model)
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
                StaticInstance instance = new StaticInstance
                {
                    model = model,
                    configUrl = configurl,
                    configPath = configurl.url.Substring(0, configurl.url.LastIndexOf('/')) + ".cfg",
                };
                ConfigParser.ParseInstanceConfig(instance, instanceCfgNode);

                if (instance.CelestialBody == null)
                {
                    instance = null;
                    continue;
                }

                if (instance.Group == null)
                {
                    instance = null;
                    continue;
                }

                //instance.mesh = Instantiate(model.prefab);
                //if (instance.mesh == null)
                //{
                //    Log.UserError("KK: Could not find " + model.mesh + ".mu! Did the modder forget to include it or did you actually install it?");
                //    instance = null;
                //    continue;
                //}

                // create RadialPosition, If we don't have one.
                if (instance.RadialPosition.Equals(Vector3.zero) && instance.RelativePosition.Equals(Vector3.zero))
                {
                    if (instance.RefLatitude != 361f && instance.RefLongitude != 361f)
                    {
                        instance.RadialPosition = (instance.CelestialBody.GetRelSurfaceNVector(instance.RefLatitude, instance.RefLongitude).normalized * instance.CelestialBody.Radius);
                        Log.UserInfo("creating new RadialPosition for: " + instance.configPath + " " + instance.RadialPosition.ToString());
                    }
                    else
                    {
                        Log.UserError("Neither RelativePosition, RadialPosition or RefLatitude+RefLongitude found: " + instance.configPath);
                        continue;
                    }
                }

                instance.Orientate();

                if (!StaticDatabase.HasInstance(instance))
                {
                    instance.Destroy();
                    continue;
                }

                //instance.TrySpawn();

                LaunchSiteManager.AttachLaunchSite(instance, instanceCfgNode);
                AttachFacilities(instance, instanceCfgNode);



                instance.grassColor2Configs = instanceCfgNode.GetNodes("GrassColor2").ToList();

                // update the references
                foreach (var facility in instance.myFacilities)
                {
                    facility.staticInstance = instance;
                }

                instance.Deactivate();

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

                StaticModel model = new StaticModel
                {
                    path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url)).Replace("\\", "/"),
                    name = modelName,
                    config = conf.url,
                    configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg"
                };

                ConfigParser.ParseModelConfig(model, conf.config);

                if (model.mesh.Contains('.'))
                {
                    model.mesh = model.mesh.Substring(0, model.mesh.LastIndexOf('.'));
                    //                model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());
                }
                model.prefab = GameDatabase.Instance.GetModelPrefab(model.path + "/" + model.mesh);

                if (model.prefab == null)
                {
                    Log.UserError("Could not find " + model.path + "/" + model.mesh + ".mu!");
                    continue;
                }

                //foreach (MeshRenderer renderer in model.prefab.GetComponentsInChildren<MeshRenderer>(true))
                //{
                //    renderer.sharedMaterial.shader = KKGraphics.GetShader("Standard");
                //}
                //model.prefab.isStatic = true;
                //StaticBatchingUtility.Combine(model.prefab);

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

                    // check for unused AdvTexture Modules
                    if (module.moduleClassname == "AdvancedTextures")
                    {
                        bool transformFound = false;
                        string transforms = "";
                        string[] seperators = new string[] { " ", ",", ";" };
                        List<string> targetTransforms = new List<string> { "Any" };

                        if (module.moduleFields.ContainsKey("transforms"))
                        {
                            transforms = module.moduleFields["transforms"];
                            targetTransforms = transforms.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
                            foreach (MeshRenderer renderer in model.prefab.GetComponentsInChildren<MeshRenderer>(true))
                            {
                                if (!transforms.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !targetTransforms.Contains(renderer.transform.name))
                                {
                                    continue;
                                }
                                transformFound = true;
                            }
                        }
                        else
                        {
                            transformFound = true;
                        }
                        if (!transformFound)
                        {
                            //Log.Normal("Adv Texture Preload: transforms not found: " + transforms + " on model: " + model.name);
                            continue;
                        }
                    }
                    if (model.modules == null)
                    {
                        model.modules = new List<StaticModule>();
                    }
                    model.modules.Add(module);
                }

                if (model.keepConvex != true)
                {
                    foreach (MeshCollider collider in model.prefab.GetComponentsInChildren<MeshCollider>(true))
                    {
                        Log.Debug("Making collider " + collider.name + " concave.");
                        collider.convex = false;
                    }
                }

                StaticDatabase.RegisterModel(model, modelName);
            }
        }

        /// <summary>
        /// loads all statics with a pointername?!?
        /// </summary>
        public void LoadModelInstances()
        {
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");
            foreach (UrlDir.UrlConfig conf in configs)
            {
                string modelname;
                if (conf.config.HasValue("pointername") && !String.IsNullOrEmpty(conf.config.GetValue("pointername")))
                {
                    modelname = conf.config.GetValue("pointername");
                }
                else
                {
                    //continue;
                    modelname = conf.config.GetValue("name");
                }

                StaticModel model = StaticDatabase.GetModelByName(modelname);
                if (model != null)
                {
                    LoadInstances(conf, model);
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
            catch(ArgumentException ex)
            {
                instance.legacyfacilityID = cfgNode.GetValue("FacilityType");
                instance.FacilityType = "None";
                instance.facilityType = KKFacilityType.None;
                facType = KKFacilityType.None;
                Log.UserWarning(ex.Message);
                //Log.UserError("Unknown Facility Type: " + cfgNode.GetValue("FacilityType") + " in file: " + instance.configPath );
            }


            if (facType == KKFacilityType.None && !cfgNode.HasNode("Facility"))
            {
                return;

            }
            // Stuff for recursive Facilities
            instance.facilityType = facType;
            instance.FacilityType = cfgNode.GetValue("FacilityType");

            switch (facType)
            {
                case KKFacilityType.Merchant:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<Merchant>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Storage:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<Storage>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.GroundStation:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TrackingStation:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    instance.facilityType = KKFacilityType.GroundStation;
                    break;
                case KKFacilityType.FuelTanks:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<FuelTanks>().ParseConfig(cfgNode));
                    instance.facilityType = KKFacilityType.Merchant;
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
                case KKFacilityType.RecoveryBase:
                    instance.myFacilities.Add(instance.gameObject.AddComponent<RecoveryBase>().ParseConfig(cfgNode));
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
        internal void SaveModelConfig(StaticModel mModelToSave)
        {
            StaticModel model = StaticDatabase.GetModelByName(mModelToSave.name);


            ConfigNode staticNode = new ConfigNode("STATIC");
            ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);

            ConfigParser.WriteModelConfig(model, modelConfig);


            modelConfig.RemoveNodes("Instances");

            foreach (StaticInstance instance in StaticDatabase.GetInstancesFromModel(model))
            {
                if (instance.isInSavegame) continue;

                ConfigNode inst = new ConfigNode("Instances");
                ConfigParser.WriteInstanceConfig(instance, inst);
                modelConfig.nodes.Add(inst);
            }

            staticNode.AddNode(modelConfig);
            staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + model.configPath, "Generated by Kerbal Konstructs");

        }

        /// <summary>
        /// This saves all satic objects to thier instance files.. 
        /// </summary>
        public void SaveObjects()
        {
            ConfigUtil.CreateNewInstanceDirIfNeeded();

            SaveGroupCenters();

            HashSet<string> processedInstances = new HashSet<string>();
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.isInSavegame)
                {
                    continue;
                }

                // ignore allready processed cfg files
                if (processedInstances.Contains(instance.configPath))
                {
                    continue;
                }

                if (instance.configPath == instance.model.configPath)
                {
                    SaveModelConfig(instance.model);
                }
                else
                {
                    // find all instances with the same configPath. 
                    instance.SaveConfig();
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
                        SaveModelConfig(deletedInstance.model);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(deletedInstance.configPath))
                        {
                            // remove the file
                            File.Delete(KSPUtil.ApplicationRootPath + "GameData/" + deletedInstance.configPath);
                        }
                    }
                }
                processedInstances.Add(deletedInstance.configPath);

            }
            deletedInstances.Clear();
        }

        internal void SaveGroupCenters()
        {
            StaticDatabase.allGroupCenters.Where(g => g.isInSavegame && !string.IsNullOrEmpty(g.configPath)).ToList().ForEach(g => deletedGroups.Add(g));

            foreach (GroupCenter center in deletedGroups)
            {
                if (File.Exists(KSPUtil.ApplicationRootPath + "GameData/" + center.configPath))
                {
                    File.Delete(KSPUtil.ApplicationRootPath + "GameData/" + center.configPath);
                }
            }
            deletedGroups.Clear();

            foreach (GroupCenter center in StaticDatabase.allGroupCenters)
            {
                center.Save();
            }
        }


        internal bool hasDeletedInstances
        {
            get
            {
                return (deletedInstances.Count > 0 || deletedGroups.Count > 0);
            }
        }



        public void DeleteInstance(StaticInstance Instance)
        {
            if (selectedInstance == Instance)
            {
                DeselectObject(true, false);
            }

            InputLockManager.RemoveControlLock("KKShipLock");
            InputLockManager.RemoveControlLock("KKEVALock");
            InputLockManager.RemoveControlLock("KKCamModes");
            InputLockManager.RemoveControlLock("KKCamModes2");
            InputLockManager.RemoveControlLock("KKActions");


            if (camControl.active)
            {
                camControl.disable();
            }

            if (StaticsEditorGUI.instance.snapTargetInstance == Instance)
            {
                StaticsEditorGUI.instance.snapTargetInstance = null;
            }

            Log.Debug("deleteObject");

            // check later when saving if this file is empty
            deletedInstances.Add(Instance);

            Instance.Destroy();
        }



        /// <summary>
        /// Selects an object for editing
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="isEditing"></param>
        /// <param name="bFocus"></param>
        /// <param name="bPreview"></param>
        public static void SelectInstance(StaticInstance instance, bool isEditing)
        {
            InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
            InputLockManager.SetControlLock(ControlTypes.EVA_INPUT, "KKEVALock");
            InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "KKCamModes");
            InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "KKCamModes2");
            InputLockManager.SetControlLock(ControlTypes.ACTIONS_ALL, "KKActions");

            if (selectedInstance != null)
            {
                DeselectObject(true, true);
            }

            if (camControl.active)
            {
                camControl.disable();
            }

            camControl.enable(instance.gameObject);

            //obj.preview = bPreview;
            selectedInstance = instance;
            if (isEditing)
            {
                if (!selectedInstance.isActive)
                {
                    selectedInstance.Activate();
                }
                selectedInstance.ToggleAllColliders(false);
            }
        }

        public static void DeselectObject(bool disableCam, bool enableColliders)
        {
            if (selectedInstance != null)
            {
                /* selectedObject.editing = false;
				if (enableColliders) selectedObject.ToggleAllColliders(true);
                */

                selectedInstance.DeselectObject(enableColliders);
                selectedInstance = null;
            }

            InputLockManager.RemoveControlLock("KKShipLock");
            InputLockManager.RemoveControlLock("KKEVALock");
            InputLockManager.RemoveControlLock("KKCamModes");
            InputLockManager.RemoveControlLock("KKCamModes2");
            InputLockManager.RemoveControlLock("KKActions");

            if (disableCam)
            {
                camControl.disable();
            }
        }

        #endregion


        #region Get Methods


        public CelestialBody GetCurrentBody()
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
                foreach (FieldInfo f in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (Attribute.IsDefined(f, typeof(KSPField)))
                    {
                        if (cfg.HasValue(f.Name))
                        {
                            //Log.Normal("setting value of: " + f.Name + " to´: " + Convert.ChangeType(cfg.GetValue(f.Name), f.FieldType).ToString());
                            f.SetValue(this, Convert.ChangeType(cfg.GetValue(f.Name), f.FieldType));
                        }
                    }
                }
            }
            else
            {
                Log.UserWarning("Settings could not be loaded");
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

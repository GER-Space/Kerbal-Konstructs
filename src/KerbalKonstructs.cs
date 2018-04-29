using System;
using System.Collections;
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
using KerbalKonstructs.Addons;
using KerbalKonstructs.Modules;

using Debug = UnityEngine.Debug;


namespace KerbalKonstructs
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbalKonstructs : MonoBehaviour
    {
        // Hello
        internal static KerbalKonstructs instance;       

        internal static readonly string sKKVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        #region Holders
        internal StaticInstance selectedObject;
        internal StaticModel selectedModel;
        internal CameraController camControl = new CameraController();
        private CelestialBody currentBody;
        internal static bool InitialisedFacilities = false;

        internal static bool scCamWasAltered = false;

        //internal double VesselCost = 0;
        //internal double RefundAmount = 0;

        internal double recoveryExraRefund = 0;


        #endregion


        #region Configurable Variables    
        internal bool enableRT
        {
            get
            {
                if (RemoteTechAddon.isInstalled)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().enableRT;
                } else
                {
                    return false;
                }
            }
            set
            { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().enableRT = value;
            }
        }
        internal bool enableCommNet
        {
            get
            {   if (CommNet.CommNetScenario.CommNetEnabled)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().enableCommNet;
                } else
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
        internal bool disableCareerStrategyLayer { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().disableCareerStrategyLayer; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().disableCareerStrategyLayer = value; } }
        internal bool disableRemoteBaseOpening { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteBaseOpening; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteBaseOpening = value; } }
        internal double facilityUseRange { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().facilityUseRange; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().facilityUseRange = (float)value; } }
        internal bool disableRemoteRecovery { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteRecovery; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().disableRemoteRecovery = value; } }
        internal double defaultRecoveryFactor { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultRecoveryFactor;  } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultRecoveryFactor = (float)value; } }
        internal double defaultEffectiveRange { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultEffectiveRange; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().defaultEffectiveRange = (float)value; } }
        internal bool toggleIconsWithBB { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().toggleIconsWithBB; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().toggleIconsWithBB = value; } }
        internal static float soundMasterVolume { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().soundMasterVolume; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().soundMasterVolume = value; } }
        internal double maxEditorVisRange { get { return (double)HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().maxEditorVisRange; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().maxEditorVisRange = (float)value; } }
        internal bool DebugMode
        {
            get
            {
                if (KKCustomParameters1.instance != null)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().DebugMode;
                } else
                {
                    return false;
                }
            }
            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().DebugMode = value;
            }
        }
        internal bool spawnPreviewModels { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().spawnPreviewModels; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().spawnPreviewModels = value; } }
        internal static string newInstancePath { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().newInstancePath; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().newInstancePath = value; } }
        internal static bool useLegacyCamera { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters1>().useLegacyCamera; } }

        internal static bool focusLastLaunchSite { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>().focusLastLaunchSite; } }
        internal bool dontRemoveStockCommNet { get { return HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().dontRemoveStockCommNet; } set { HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>().disableCareerStrategyLayer = value; } }
        // map icon settings. These are saved manually
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
        public  string defaultVABlaunchsite = "LaunchPad";
        [KSPField]
        public string defaultSPHlaunchsite = "Runway";
        [KSPField]
        public string lastLaunchSiteUsed = "LaunchPad";

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
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoad);
            GameEvents.onGUIApplicationLauncherReady.Add(TbController.OnGUIAppLauncherReady);
            GameEvents.onVesselRecoveryProcessing.Add(OnProcessRecoveryProcessing);
            GameEvents.OnVesselRollout.Add(OnVesselLaunched);
            // draw map icons when needed
            GameEvents.OnMapEntered.Add(MapIconDraw.instance.Open);
            GameEvents.OnMapExited.Add(MapIconDraw.instance.Close);
            GameEvents.OnGameDatabaseLoaded.Add(OnGameDatabaseLoaded);
            GameEvents.onVesselGoOffRails.Add(FixWaterLaunch);
            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {

                GameEvents.onGUILaunchScreenSpawn.Add(OnSelectorLoaded);
                GameEvents.onEditorRestart.Add(OnEditorRestart);
            }
            #endregion

            #region Other Mods Hooks
            StageRecovery.AttachStageRecovery();
            #endregion

            SpaceCenterManager.setKSC();
            ConnectionManager.ScanForStockCommNet();

            DontDestroyOnLoad(this);

            // for Terrain Rescaling
            SDRescale.SetTerrainRescales();

            // PQSMapDecal
            Log.PerfStart("loading MapDecals");
            MapDecalUtils.GetSquadMaps();
            ConfigParser.LoadAllMapDecalMaps();
            ConfigParser.LoadAllMapDecals();
            Log.PerfStop("loading MapDecals");
            // end PQSMapDecal
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

            Log.PerfStop("Awake Function");
            //Log.PerfStart("Model Test");
            //SDTest.GetModelStats();
            //Log.PerfStop("Model Test");
            //SDTest.GetShaderStats();
            //SDTest.ScanParticles();
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
                if (lastSite.sitecategory == LaunchSiteCategory.Waterlaunch)
                {
                    Log.Normal("Trying to bring the vessel back to the surface.");
                    vessel.SetPosition(lastSite.lsGameObject.transform.Find(lastSite.LaunchPadTransform).position); 
                }

            }
            
        }


        /// <summary>
        /// Updates the mission log and processes the launch refund.
        /// </summary>
        /// <param name="vVessel"></param>
        void OnVesselLaunched(ShipConstruct vVessel)
        {
            Log.Normal("OnVesselLaunched");
            string sitename = LaunchSiteManager.getCurrentLaunchSite();
            if (sitename == "Runway" || sitename == "LaunchPad" || sitename == "KSC" || sitename == "")
            {
                return;
            }
            // reset the name to the site, so it can be fetched again
            KKLaunchSite lastSite = LaunchSiteManager.GetCurrentLaunchSite();
            lastSite.spaceCenterFacility.name = lastSite.LaunchSiteName;

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
            DeletePreviewObject();

            StaticDatabase.ToggleActiveAllStatics(false);

            if (selectedObject != null)
            {
                deselectObject(false, true);
                camControl.active = false;
            }
            CancelInvoke("updateCache");

            switch (data)
            {
                case GameScenes.FLIGHT:
                    {
                        InputLockManager.RemoveControlLock("KKEditorLock");
                        InputLockManager.RemoveControlLock("KKEditorLock2");


                        if (FlightGlobals.ActiveVessel != null)
                        {
                            //StaticDatabase.ToggleActiveStaticsOnPlanet(FlightGlobals.ActiveVessel.mainBody, true, true);
                            currentBody = FlightGlobals.ActiveVessel.mainBody;
                            StaticDatabase.OnBodyChanged(FlightGlobals.ActiveVessel.mainBody);
                            updateCache();
                            Hangar.DoHangaredCraftCheck();
                        }
                        else
                        {
                            Log.Debug("Flight scene load. No activevessel. Activating all statics.");
                            StaticDatabase.ToggleActiveAllStatics(true);
                        }
                        InvokeRepeating("updateCache", 0, 1);
                    }
                    break;
                case GameScenes.EDITOR:
                    {
                        if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
                        {
                            LaunchSiteManager.OnEditorLoaded();
                        }

                        // Prevent abuse if selector left open when switching to from VAB and SPH
                        LaunchSiteSelectorGUI.instance.Close();
                        KKLaunchSite currentSite = LaunchSiteManager.GetLaunchSiteByName(lastLaunchSiteUsed);

                        //if (currentSite.LaunchSiteType == SiteType.Any)
                        //{
                        //    currentSite.spaceCenterFacility.editorFacility = EditorDriver.editorFacility;
                        //    LaunchSiteManager.SetupKSPFacilities();
                        //}

                        Log.Normal("");
                        Log.Normal("Valid sites");
                        foreach (var site in EditorDriver.ValidLaunchSites)
                        {
                            Log.Normal("Stock site: " + site);
                        }


                        // Check if the selected LaunchSite is valid
                        if (LaunchSiteManager.CheckLaunchSiteIsValid(currentSite) == false)
                        {
                            Log.Normal("LS not valid: " + currentSite.LaunchSiteName);
                            currentSite = LaunchSiteManager.GetDefaultSite();
                        }
                        LaunchSiteManager.setLaunchSite(currentSite);
                    }
                    break;
                case GameScenes.SPACECENTER:
                    {
                        InputLockManager.RemoveControlLock("KKEditorLock");
                        LaunchSiteManager.KKSitesToKSP();

                        KKLaunchSite currentSite = LaunchSiteManager.GetCurrentLaunchSite();
                        //currentBody = currentSite.body;
                        currentBody = ConfigUtil.GetCelestialBody("HomeWorld");
                        // 
                        //// This is currently broken. I have no idea why
                        ////
                        //if (!currentBody.pqsController.isActive)
                        //{
                        //    Log.Normal("Activating Body: " + currentBody.name);
                        //    currentBody.pqsController.SetTarget(currentSite.lsGameObject.transform);
                        //    currentBody.pqsController.SetSecondaryTarget(currentSite.lsGameObject.transform);
                        //    currentBody.pqsController.ActivateSphere();
                        //    FlightGlobals.currentMainBody = currentBody;
                        //    //currentBody.pqsController.RebuildSphere();
                        //    Log.Normal("Body is active: " + currentBody.pqsController.isActive.ToString());
                        //}
                        Log.Normal("SC Body is: " + currentBody.name);
                        StaticDatabase.OnBodyChanged(currentBody);
                        updateCache();
                        if (scCamWasAltered || focusLastLaunchSite)
                        {
                            CameraController.SetSpaceCenterCam(currentSite);
                        }
                        updateCache();
                    }
                    break;
                case GameScenes.MAINMENU:
                    {
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


        void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
        {
            StaticDatabase.OnBodyChanged(data.to);
            currentBody = data.to;
            updateCache();
        }


        public void OnSelectorLoaded(GameEvents.VesselSpawnInfo info)
        {
            Log.Normal("Reseting LaunchSite Lists on Selector spawn");
            IEnumerator coroutine = WaitAndReset(info);
            StartCoroutine(coroutine);
        }


        public IEnumerator WaitAndReset(GameEvents.VesselSpawnInfo info)
        {
            yield return new WaitForSeconds(1);
            LaunchSiteManager.ResetLaunchSites();
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
            Log.Normal("On Editor Restart");
            LaunchSiteManager.OnEditorLoaded();
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

                SpaceCenter closestSpaceCenter = SpaceCenterManager.KSC;
                CustomSpaceCenter customSC = null;

                double smallestDist = SpaceCenterManager.KSC.GreatCircleDistance(SpaceCenterManager.KSC.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
                Log.Normal("Distance to KSC is " + smallestDist);

                foreach (CustomSpaceCenter csc in SpaceCenterManager.spaceCenters)
                {
                    if (csc.staticInstance.launchSite.isOpen == false )
                    {
                        continue;
                    }

                    closestSpaceCenter = csc.GetSpaceCenter();
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
                        dialog.recoveryFactor =  Math.Round(recovery,1).ToString() + "%";
                        dialog.recoveryLocation = Math.Round(smallestDist/1000, 1) + "km from " + customSC.SpaceCenterName;
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

                    Funding.Instance.AddFunds(missingvalue,TransactionReasons.VesselRecovery);
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
            if (selectedObject != null)
            {
                deselectObject(true, false);
            }
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.hasLauchSites)
                {
                    LaunchSiteManager.DeleteLaunchSite(instance.launchSite);
                }
                DeleteObject(instance);
            }
            deletedInstances.Clear();

            // reset the Database to empty state, as nothing should be loaded anymore
            StaticDatabase.Reset();
            ConnectionManager.ResetAll();
            //DecalsDatabase.ResetAll();

            // Load up everything
            // PQSMapDecal
            //Log.PerfStart("loading MapDecals");
            //MapDecalUtils.GetSquadMaps();
            //ConfigParser.LoadAllMapDecalMaps();
            //ConfigParser.LoadAllMapDecals();
            //Log.PerfStop("loading MapDecals");
            // end PQSMapDecal
            Log.PerfStart("Loading Instances");

            LoadSquadModels();

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
                updateCache();
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
                EditorGUI.instance.CheckEditorKeys();

                if (Input.GetKeyDown(KeyCode.K) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    StaticsEditorGUI.instance.ToggleEditor();
                }
                if (Input.GetKeyDown(KeyCode.Tab) && StaticsEditorGUI.instance.IsOpen())
                {
                    StaticsEditorGUI.instance.SelectMouseObject();
                }

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
        public void updateCache()
        {
            if (HighLogic.LoadedSceneIsGame)
            {
                // Don't update visiblility when Editor is open
                if (StaticsEditorGUI.instance.IsOpen())
                {
                    return;
                }

                Vector3 playerPos = Vector3.zero;
                if (selectedObject != null)
                {
                    playerPos = selectedObject.gameObject.transform.position;
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
                        Log.Normal("SC2Name: " +spaceCenterCam.gameObject.transform.name);
                        playerPos = spaceCenterCam.gameObject.transform.position;
                    }
                    else
                    {
                        Log.Normal("No SpaceCenterCam Found in SpaceCenter Scene");
                        // we can try the current LaunchSite as fallback
                        playerPos = LaunchSiteManager.GetCurrentLaunchSite().lsGameObject.transform.position;
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
                StaticInstance instance = new StaticInstance
                {
                    model = model,
                    configUrl = configurl,
                    configPath = configurl.url.Substring(0, configurl.url.LastIndexOf('/')) + ".cfg",
                    gameObject = Instantiate(model.prefab)
                };
                if (instance.gameObject == null)
                {
                    Log.UserError("KK: Could not find " + model.mesh + ".mu! Did the modder forget to include it or did you actually install it?");
                    continue;
                }

                ConfigParser.ParseInstanceConfig(instance, instanceCfgNode);

                if (instance.CelestialBody == null)
                    continue;

                // create RadialPosition, If we don't have one.
                if (instance.RadialPosition.Equals(Vector3.zero))
                {
                    if (instance.RefLatitude != 361f && instance.RefLongitude != 361f)
                    {
                        instance.RadialPosition = (instance.CelestialBody.GetRelSurfaceNVector(instance.RefLatitude, instance.RefLongitude).normalized * instance.CelestialBody.Radius);
                        Log.UserInfo("creating new RadialPosition for: " + instance.configPath + " " + instance.RadialPosition.ToString());
                    }
                    else
                    {
                        Log.UserError("Neither RadialPosition or RefLatitude+RefLongitude found: " + instance.gameObject.name);
                        continue;
                    }
                }
                else
                {
                    // create LAT & LON out of Radialposition, when not changed by config
                    if (instance.RefLatitude == 361f || instance.RefLongitude == 361f)
                    {
                        instance.RefLatitude = KKMath.GetLatitudeInDeg(instance.RadialPosition);
                        instance.RefLongitude = KKMath.GetLongitudeInDeg(instance.RadialPosition);
                    }
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

                // update the references
                foreach (var facility in instance.myFacilities)
                {
                    facility.staticInstance = instance;
                }

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
                    {
                        model.DefaultLaunchPadTransform = "End09/SpawnPoint";
                    }

                    // Launchpads also 
                    if (facility.name.Equals("LaunchPad", StringComparison.CurrentCultureIgnoreCase))
                    {
                        model.DefaultLaunchPadTransform = "LaunchPad_spawn";


                        //Log.Normal("LauchPadFX: " + model.name);

                        //foreach (var psystem in facility.UpgradeLevels[i].facilityPrefab.GetComponentsInChildren<ParticleSystem>(true))
                        //{
                        //    Log.Normal("LauchPadFX: ParticleSystem: " + psystem.name + " : " + psystem.gameObject.name );
                        //}

                        //foreach (var pr in facility.UpgradeLevels[i].facilityPrefab.GetComponentsInChildren<ParticleSystemRenderer>(true))
                        //{
                        //    Log.Normal("LauchPadFX: ParticleSystemRenderer: " + pr.name + " : " + pr.gameObject.name);
                        //}


                    }

                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = facility.UpgradeLevels[i].facilityPrefab;

                    // Register new GasColor Module
                    bool hasGrasMaterial = false;
                    foreach (Renderer renderer in model.prefab.GetComponentsInChildren<Renderer>(true))
                    {
                        foreach (Material material in renderer.materials.Where(mat => mat.name == "ksc_exterior_terrain_grass_02 (Instance)"))
                        {
                            //Log.Normal("gras: " + material.name + " : " + material.color.ToString() + " : " + material.mainTexture.name);
                            hasGrasMaterial = true;
                            break;
                        }
                    }
                    if (hasGrasMaterial)
                    {
                        //Log.Normal("Adding GrasColor to: " + model.name);
                        StaticModule module = new StaticModule();
                        module.moduleNamespace = "KerbalKonstructs";
                        module.moduleClassname = "GrasColor";
                        module.moduleFields.Add("GrasTextureImage", "BUILTIN:/terrain_grass00_new");
                        model.modules = new List<StaticModule>();
                        model.modules.Add(module);
                    }

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
                if (pqs.gameObject.name == "KSC" || pqs.gameObject.name == "KSC2" || pqs.gameObject.name == "Pyramids"  || pqs.gameObject.name == "Pyramid" || pqs.gameObject.name == "CommNetDish")
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
                if (pqs.gameObject.name != "KSC2" && pqs.gameObject.name != "Pyramids" && pqs.gameObject.name != "CommNetDish")
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

                    if (model.name.Equals("SQUAD_KSC2_launchpad", StringComparison.CurrentCultureIgnoreCase))
                    {
                        model.DefaultLaunchPadTransform = "PlatformPlane";
                    }

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
                    {
                        model.modules = new List<StaticModule>();
                    }

                    model.modules.Add(module);
                }
                model.prefab = GameDatabase.Instance.GetModelPrefab(model.path + "/" + model.mesh);

                if (model.prefab == null)
                {
                    Debug.Log("KK: Could not find " + model.mesh + ".mu! Did the modder forget to include it or did you actually install it?");
                    continue;
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
                    sBase = group.name;
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

        public void DeleteObject(StaticInstance obj)
        {
            if (selectedObject == obj)
                deselectObject(true, false);

            InputLockManager.RemoveControlLock("KKShipLock");
            InputLockManager.RemoveControlLock("KKEVALock");
            InputLockManager.RemoveControlLock("KKCamModes");


            if (camControl.active) camControl.disable();

            if ( StaticsEditorGUI.instance.snapTargetInstance == obj)
                StaticsEditorGUI.instance.snapTargetInstance = null;

            Log.Debug("deleteObject");

            // check later when saving if this file is empty
            deletedInstances.Add(obj);

            StaticDatabase.DeleteStatic(obj);
        }


        /// <summary>
        /// Selects an object for editing
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isEditing"></param>
        /// <param name="bFocus"></param>
        /// <param name="bPreview"></param>
        public void selectObject(StaticInstance obj, bool isEditing, bool bFocus, bool bPreview)
        {
            // enable any object for editing
            if (StaticsEditorGUI.instance.IsOpen())
            {
                InstanceUtil.SetActiveRecursively(obj, true);
            }


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
            } else
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

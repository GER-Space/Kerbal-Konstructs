using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using KSP.UI;
using System.Collections.Generic;
using KerbalKonstructs.UI;
using UnityEngine.UI;


namespace KerbalKonstructs.Core
{
    public class LaunchSiteManager
    {
        private static List<KKLaunchSite> launchSites = new List<KKLaunchSite>();
        private static string currentLaunchSite
        {
            get
            {
                return KerbalKonstructs.instance.lastLaunchSiteUsed;
            }
            set
            {
                KerbalKonstructs.instance.lastLaunchSiteUsed = value;
            }
        }
        private static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
        public static float rangeNearestOpenBase = 0f;
        public static string nearestOpenBase = "";
        public static float rangeNearestBase = 0f;
        public static string nearestBase = "";

        internal static KKLaunchSite runway = new KKLaunchSite();
        internal static KKLaunchSite launchpad = new KKLaunchSite();
        internal static KKLaunchSite ksc2 = new KKLaunchSite();

        private static List<PSystemSetup.SpaceCenterFacility> KKFacilities = null;

        // Handy get of all launchSites
        public static KKLaunchSite[] allLaunchSites = null;

        public static List<string> launchSiteNames = new List<string>();


        // API for Kerbal Construction Time not for internal use
        public static List<KKLaunchSite> AllLaunchSites
        {
            get
            {
                return allLaunchSites.ToList();
            }
        }


        private static float getKSCLon
        {
            get
            {
                CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
                var mods = body.pqsController.transform.GetComponentsInChildren(typeof(PQSCity), true);
                double retval = 0d;

                foreach (var m in mods)
                {
                    PQSCity mod = m as PQSCity;
                    if (mod.name == "KSC")
                    {
                        retval = (KKMath.GetLongitudeInDeg(mod.repositionRadial));
                        break;
                    }
                }
                return (float)retval;
            }
        }

        private static float getKSCLat
        {
            get
            {
                CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
                var mods = body.pqsController.transform.GetComponentsInChildren(typeof(PQSCity), true);
                double retval = 0d;

                foreach (var m in mods)
                {
                    PQSCity mod = m as PQSCity;
                    if (mod.name == "KSC")
                    {
                        retval = (KKMath.GetLatitudeInDeg(mod.repositionRadial));
                        break;
                    }
                }
                return (float)retval;
            }
        }

        /// <summary>
        /// prefills LaunchSiteManager with the runway and the KSC
        /// </summary>
        private static void AddKSC()
        {

            runway.LaunchSiteName = "Runway";
            runway.LaunchSiteAuthor = "Squad";
            runway.LaunchSiteType = SiteType.SPH;
            runway.sitecategory = LaunchSiteCategory.Runway;
            runway.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false);
            runway.LaunchSiteDescription = "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.";
            runway.body = ConfigUtil.GetCelestialBody("HomeWorld");
            runway.refLat = getKSCLat;
            runway.refLon = getKSCLon;
            runway.refAlt = 69f;
            runway.LaunchSiteLength = 2500f;
            runway.LaunchSiteWidth = 75f;
            runway.InitialCameraRotation = -60f;
            runway.lsGameObject = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "ResearchAndDevelopment").First().gameObject;
            runway.SetOpen();

            launchpad.LaunchSiteName = "LaunchPad";
            launchpad.LaunchSiteAuthor = "Squad";
            launchpad.LaunchSiteType = SiteType.VAB;
            launchpad.sitecategory = LaunchSiteCategory.RocketPad;
            launchpad.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCLaunchpad", false);
            launchpad.LaunchSiteDescription = "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...";
            launchpad.body = ConfigUtil.GetCelestialBody("HomeWorld");
            launchpad.refLat = getKSCLat;
            launchpad.refLon = getKSCLon;
            launchpad.refAlt = 72;
            launchpad.LaunchSiteLength = 20f;
            launchpad.LaunchSiteWidth = 20f;
            launchpad.InitialCameraRotation = -60f;
            launchpad.lsGameObject = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "ResearchAndDevelopment").First().gameObject;
            launchpad.SetOpen();


            AddLaunchSite(runway);
            AddLaunchSite(launchpad);
        }

        public static void AddKSC2()
        {
            CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
            var mods = body.pqsController.transform.GetComponentsInChildren<PQSCity>(true);
            PQSCity ksc2PQS = null;



            foreach (var m in mods)
            {
                if (m.name == "KSC2")
                {
                    ksc2PQS = m;
                    break;
                }
            }

            if (ksc2PQS == null)
            {
                return;
            }

            StaticInstance ksc2Instance = new StaticInstance();

            ksc2Instance.gameObject = ksc2PQS.gameObject;
            //ksc2Instance.gameObject = ksc2PQS.gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name.Equals("launchpad", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault().gameObject;
            ksc2Instance.hasLauchSites = true;
            ksc2Instance.pqsCity = ksc2PQS;
            ksc2Instance.RadialPosition = ksc2PQS.repositionRadial;
            ksc2Instance.RefLatitude = KKMath.GetLatitudeInDeg(ksc2PQS.repositionRadial);
            ksc2Instance.RefLongitude = KKMath.GetLongitudeInDeg(ksc2PQS.repositionRadial);
            ksc2Instance.CelestialBody = body;


            ksc2.staticInstance = ksc2Instance;
            ksc2.LaunchSiteName = "KSC2";
            ksc2.LaunchPadTransform = "launchpad/PlatformPlane";

            ksc2.LaunchSiteAuthor = "KerbalKonstructs";
            ksc2.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
            ksc2.LaunchSiteType = SiteType.VAB;
            ksc2.sitecategory = LaunchSiteCategory.RocketPad;
            ksc2.LaunchSiteDescription = "The hidden KSC2";
            ksc2.body = ConfigUtil.GetCelestialBody("HomeWorld");
            ksc2.refLat = (float)ksc2Instance.RefLatitude;
            ksc2.refLon = (float)ksc2Instance.RefLongitude;
            ksc2.refAlt = (float)(body.pqsController.GetSurfaceHeight(ksc2PQS.repositionRadial) - body.Radius);
            ksc2.LaunchSiteLength = 15f;
            ksc2.LaunchSiteWidth = 15f;
            ksc2.InitialCameraRotation = 135f;
            //            ksc2.lsGameObject = ksc2PQS.gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name.Equals("launchpad", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault().gameObject;
            ksc2.lsGameObject = ksc2PQS.gameObject;
            ksc2.OpenCost = 1f;
            ksc2.SetClosed();
            ksc2.LaunchSiteIsHidden = true;

            ksc2Instance.launchSite = ksc2;
            RegisterLaunchSite(ksc2);
        }

        /// <summary>
        /// contructor
        /// </summary>
        static LaunchSiteManager()
        {
            AddKSC();
            AddKSC2();
        }

        internal static void AttachLaunchSite(StaticInstance instance, ConfigNode instanceNode)
        {
            if (instanceNode.HasValue("LaunchPadTransform") && !string.IsNullOrEmpty(instanceNode.GetValue("LaunchPadTransform")) && instanceNode.HasValue("LaunchSiteName") && !string.IsNullOrEmpty(instanceNode.GetValue("LaunchSiteName")))
            {
                // legacy Launchsite within instanceNode
                CreateLaunchSite(instance, instanceNode);
            }
            else
            {
                // check for new LaunchSite ConfigNode
                if (instanceNode.HasNode("LaunchSite"))
                {
                    ConfigNode lsNode = instanceNode.GetNode("LaunchSite");
                    if (lsNode.HasValue("LaunchPadTransform") && !string.IsNullOrEmpty(lsNode.GetValue("LaunchPadTransform")) && lsNode.HasValue("LaunchSiteName") && !string.IsNullOrEmpty(lsNode.GetValue("LaunchSiteName")))
                    {
                        // legacy Launchsite within instanceNode
                        CreateLaunchSite(instance, lsNode);
                    }
                }
            }
        }


        /// <summary>
        /// Called when a Launchsite is closed
        /// </summary>
        internal static void CloseLaunchSite(KKLaunchSite site)
        {
            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory") && HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Log.Normal("Close: " + site.LaunchSiteName);
                AlterMHSelector();
            }
        }

        /// <summary>
        /// Function that is called when a launchSite is opened
        /// </summary>
        internal static void OpenLaunchSite(KKLaunchSite site)
        {
            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory") && HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Log.Normal("OpenSite: " + site.LaunchSiteName);
                AlterMHSelector();
            }
        }


        /// <summary>
        /// Creates a new LaunchSite out of a cfg-node and Registers it with RegisterLaunchSite
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="cfgNode"></param>
        internal static void CreateLaunchSite(StaticInstance instance, ConfigNode cfgNode)
        {
            KKLaunchSite mySite = new KKLaunchSite();
            mySite.ParseLSConfig(instance, cfgNode);
            instance.hasLauchSites = true;
            instance.launchSite = mySite;
            RegisterLaunchSite(mySite);
        }

        /// <summary>
        /// Registers the a created LaunchSite to the PSystemSetup and LaunchSiteManager
        /// </summary>
        /// <param name="site"></param>
        internal static void RegisterLaunchSite(KKLaunchSite site , bool isSquad = false)
        {
            if (!string.IsNullOrEmpty(site.LaunchSiteName) && site.lsGameObject.transform.Find(site.LaunchPadTransform) != null)
            {

                    site.lsGameObject.transform.name = site.LaunchSiteName;
                    site.lsGameObject.name = site.LaunchSiteName;
                
                if (KKFacilities == null)
                {
                    KKFacilities = PSystemSetup.Instance.SpaceCenterFacilities.ToList();
                }

                if (KKFacilities.Where(fac => fac.facilityName == site.LaunchSiteName).FirstOrDefault() == null)
                {
                    //Log.Normal("Registering LaunchSite: " + site.LaunchSiteName);
                    PSystemSetup.SpaceCenterFacility spaceCenterFacility = new PSystemSetup.SpaceCenterFacility();
                    spaceCenterFacility.name = site.LaunchSiteName;
                    spaceCenterFacility.facilityDisplayName = site.LaunchSiteName;
                    spaceCenterFacility.facilityName = site.LaunchSiteName;
                    spaceCenterFacility.facilityPQS = site.staticInstance.CelestialBody.pqsController;
                    spaceCenterFacility.facilityTransformName = site.staticInstance.gameObject.name;
                    // newFacility.facilityTransform = site.lsGameObject.transform.Find(site.LaunchPadTransform);
                    //     newFacility.facilityTransformName = instance.gameObject.transform.name;
                    spaceCenterFacility.pqsName = site.body.pqsController.name;
                    PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
                    spawnPoint.name = site.LaunchSiteName;
                    spawnPoint.spawnTransformURL = site.LaunchPadTransform;
                    spaceCenterFacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[1];
                    spaceCenterFacility.spawnPoints[0] = spawnPoint;
                    if (site.LaunchSiteType == SiteType.VAB)
                    {
                        spaceCenterFacility.editorFacility = EditorFacility.VAB;
                    }
                    else
                    {
                        spaceCenterFacility.editorFacility = EditorFacility.SPH;
                    }

                    KKFacilities.Add(spaceCenterFacility);
                    site.spaceCenterFacility = spaceCenterFacility;

                    AddLaunchSite(site);
                }
                else
                {
                    Log.Error("Launch site " + site.LaunchSiteName + " already exists.");
                }

                RegisterLaunchSitesStock(site);
                

                if (site.staticInstance.gameObject != null)
                {
                    CustomSpaceCenter.CreateFromLaunchsite(site);
                }

            }
            else
            {
                Log.UserWarning("Launch pad transform \"" + site.LaunchPadTransform + "\" missing for " + site.LaunchSiteName);
            }
        }


        public static void AlterMHSelector()
        {
            ResetLaunchSiteFacilityName();
            Log.Normal("AMH: Reseting LaunchSite to: " + EditorDriver.editorFacility.ToString());
            Log.Normal("AMH: Current site: " + currentLaunchSite);
            RegisterMHLaunchSites(EditorDriver.editorFacility);
            KSP.UI.UILaunchsiteController uILaunchsiteController = Resources.FindObjectsOfTypeAll<KSP.UI.UILaunchsiteController>().FirstOrDefault();
            if (uILaunchsiteController == null)
            {
                Log.UserWarning("LaunchSitecontroller not found");
            }
            else
            {
                //GameEvents.onEditorRestart.Fire();
                uILaunchsiteController.GetType().GetMethod("resetItems", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(uILaunchsiteController, null);
                //uILaunchsiteController.GetType().GetMethod("setupToggleGroup", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(uILaunchsiteController, null);
                //uILaunchsiteController.GetType().GetMethod("setSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(uILaunchsiteController, null);
            }

//            var bla = Type.GetType("KSP.UI.Screens.EditorLaunchPadItem");

            var launchPadItems = (System.Collections.IList)uILaunchsiteController.GetType().GetField("launchPadItems", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(uILaunchsiteController);
            if (launchPadItems == null)
            {
                Log.Normal("No launchPadItems found ");
            }
            else
            {
                Log.Normal("launchPadItems found ");
                foreach (var item in launchPadItems)
                {
                    var type = item.GetType();
                    if (type == null)
                    {
                        Log.Warning("Cound not retrieve type");
                    }
                    else
                    {
                        string siteName = (string)item.GetType().GetField("siteName").GetValue(item);
                        Log.Normal(" altering button for siteName: " + siteName);

                        Button button = (Button)item.GetType().GetField("buttonLaunch").GetValue(item);
                        Toggle toggleSetDefault = (Toggle)item.GetType().GetField("toggleSetDefault").GetValue(item);
                        GameObject gameObj = (GameObject)item.GetType().GetProperty("gameObject").GetValue(item, null);

                        LaunchSiteButton buttonFixer = gameObj.AddComponent<LaunchSiteButton>();

                        buttonFixer.siteName = siteName;

                        if (siteName == currentLaunchSite)
                        {
                            toggleSetDefault.isOn = true;
                        }
                        else
                        {
                            toggleSetDefault.isOn = false;
                        }

                        if (launchSiteNames.Contains(siteName))
                        {
                            toggleSetDefault.onValueChanged.AddListener(buttonFixer.SetDefault);
                            button.onClick.RemoveAllListeners();
                            button.onClick.AddListener(buttonFixer.LauchVessel);
                        }

                    }
                }

            }

            //KKLaunchSite currentSite = LaunchSiteManager.GetLaunchSiteByName(currentLaunchSite);
            //// Check if the selected LaunchSite is valid
            //if (LaunchSiteManager.CheckLaunchSiteIsValid(currentSite) == false)
            //{
            //    Log.Normal("LS not valid: " + currentSite.LaunchSiteName);
            //    currentSite = LaunchSiteManager.GetDefaultSite();
            //    LaunchSiteManager.setLaunchSite(currentSite);
            //    AlterMHSelector();
            //}
            //else
            //{
            //    LaunchSiteManager.setLaunchSite(currentSite);
            //}
        }



        internal static void RegisterMHLaunchSites(EditorFacility facility)
        {
            foreach (KKLaunchSite site in allLaunchSites)
            {
                if (facility == EditorFacility.SPH && site.LaunchSiteType == SiteType.VAB)
                {
                    continue;
                }
                if (facility == EditorFacility.VAB && site.LaunchSiteType == SiteType.SPH)
                {
                    continue;
                }

                if (site.LaunchSiteName == "LaunchPad" || site.LaunchSiteName == "Runway")
                {
                    continue;
                }

                if (site.isOpen)
                {
                    site.spaceCenterFacility.editorFacility = facility;
                    //CreateMHLaunchSite(site, facility);
                }
                else
                {
                    site.spaceCenterFacility.editorFacility = EditorFacility.None;
                }
            }

            typeof(EditorDriver).GetMethod("setupValidLaunchSites", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

        }




        //internal static void CreateMHLaunchSite(KKLaunchSite site, EditorFacility facility)
        //{
        //    Log.Normal("Creating MH LaunchSite for: " + site.LaunchSiteName + " " + facility.ToString());
        //    LaunchSite.SpawnPoint spawnPoint = new LaunchSite.SpawnPoint();
        //    spawnPoint.spawnTransformURL = site.LaunchPadTransform;
        //    spawnPoint.name = site.LaunchSiteName;

        //    LaunchSite.SpawnPoint[] spawnPoints = new LaunchSite.SpawnPoint[1] { spawnPoint };

        //    LaunchSite launchSite = new LaunchSite(site.LaunchSiteName, site.body.name, site.LaunchSiteName, spawnPoints, site.lsGameObject.transform.name, facility);

        //    launchSite.Setup(site.staticInstance.pqsCity, new PQS[1] { site.body.pqsController });
        //    PSystemSetup.Instance.AddLaunchSite(launchSite);
        //    Log.Normal("created: " + site.LaunchSiteName);

        //}

        internal static void RegisterLaunchSitesStock(KKLaunchSite site)
        {
            SetupKSPFacilities();
        }



        internal static void SetupKSPFacilities()
        {

            // Log.Normal("SetupKSPFacilities Called");

            PSystemSetup.Instance.SpaceCenterFacilities = KKFacilities.ToArray();

            MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateSitesMI == null)
            {
                Log.UserError("You are screwed. Failed to find SetupFacilities().");
            }
            else
            {
                updateSitesMI.Invoke(PSystemSetup.Instance, null);
            }
        }


        /// <summary>
        /// Removes the launchSite from the facilities
        /// </summary>
        /// <param name="site"></param>
        internal static void UnregisterLaunchSite(KKLaunchSite site)
        {
            if (site.isOpen)
            {
                CloseLaunchSite(site);
            }

            List<PSystemSetup.SpaceCenterFacility> spaceCenters = PSystemSetup.Instance.SpaceCenterFacilities.ToList();
            PSystemSetup.SpaceCenterFacility spaceToDel = spaceCenters.Where(x => x.facilityName == site.LaunchSiteName).FirstOrDefault();

            if (spaceToDel != null)
            {
                spaceCenters.Remove(spaceToDel);
                PSystemSetup.Instance.SpaceCenterFacilities = spaceCenters.ToArray();
                Log.Normal("Launchsite: " + site.LaunchSiteName + " sucessfully unregistered");
            }

            KKFacilities.Remove(site.spaceCenterFacility);

        }


        /// <summary>
        /// Deletes a LaunchSite from the internal Database
        /// </summary>
        /// <param name="site2delete"></param>
        internal static void DeleteLaunchSite(KKLaunchSite site2delete)
        {
            if (launchSites.Contains(site2delete))
            {
                launchSites.Remove(site2delete);

                CustomSpaceCenter csc = SpaceCenterManager.GetCSC(site2delete.LaunchSiteName);
                if (csc != null)
                {
                    SpaceCenterManager.spaceCenters.Remove(csc);
                }

                allLaunchSites = launchSites.ToArray();
                launchSiteNames.Remove(site2delete.LaunchSiteName);
                UnregisterLaunchSite(site2delete);
            }
        }

        /// <summary>
        /// Adds a LaunchSite to the internal Database
        /// </summary>
        /// <param name="site2add"></param>
        internal static void AddLaunchSite(KKLaunchSite site2add)
        {

            launchSites.Add(site2add);
            List<KKLaunchSite> tmpList = launchSites.ToList();
            tmpList.Sort(delegate (KKLaunchSite a, KKLaunchSite b)
            {
                return (a.LaunchSiteName).CompareTo(b.LaunchSiteName);
            });
            allLaunchSites = tmpList.ToArray();
            launchSiteNames.Add(site2add.LaunchSiteName);
        }

        internal static KKLaunchSite GetCurrentLaunchSite()
        {
            Log.Normal("retuning CurrentSite: " + currentLaunchSite);
            return GetLaunchSiteByName(currentLaunchSite);
        }


        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="sState"></param>
        public static void setSiteOpenCloseState(string siteName, string state)
        {
            if (checkLaunchSiteExists(siteName))
            {
                if (state == "Open")
                {
                    GetLaunchSiteByName(siteName).SetOpen();
                }
                if (state == "Closed")
                {
                    GetLaunchSiteByName(siteName).SetClosed();
                }


            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        public static void setSiteLocked(string siteName)
        {
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        public static void setSiteUnlocked(string siteName)
        {
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="sOpenCloseState"></param>
        /// <param name="fOpenCost"></param>
        public static void getSiteOpenCloseState(string siteName, out string sOpenCloseState, out float fOpenCost)
        {
            if (checkLaunchSiteExists(siteName))
            {
                KKLaunchSite site = GetLaunchSiteByName(siteName);
                sOpenCloseState = site.OpenCloseState;
                fOpenCost = site.OpenCost;
            }
            else
            {
                sOpenCloseState = "Open";
                fOpenCost = 0;
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteLocked(string siteName)
        {
            return false;
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteOpen(string siteName)
        {
            if (checkLaunchSiteExists(siteName))
            {
                return GetLaunchSiteByName(siteName).isOpen;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteClosed(string siteName)
        {
            if (checkLaunchSiteExists(siteName))
            {
                return (!GetLaunchSiteByName(siteName).isOpen);

            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool checkLaunchSiteExists(string siteName)
        {
            return (launchSites.Where(x => x.LaunchSiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase)).Count() > 0);
        }


        // Returns a specific Launchsite, keyed by site.name
        public static KKLaunchSite GetLaunchSiteByName(string siteName)
        {
            KKLaunchSite mySite = null;
            if (checkLaunchSiteExists(siteName))
            {

                foreach (KKLaunchSite site in allLaunchSites)
                {
                    if (site.LaunchSiteName.Equals(siteName))
                    {
                        mySite = site;
                    }
                }
                Log.Normal("Returning LS: " + mySite.LaunchSiteName);
                return mySite;
            }
            else
            {
                Log.UserError("Could not find Launchsite in list: " + siteName);
                return null;
            }
        }


        // Returns the distance in m from a position to a specified Launchsite
        public static float getDistanceToBase(Vector3 position, KKLaunchSite site)
        {
            return Vector3.Distance(position, site.lsGameObject.transform.position);
        }

        // Returns the nearest open Launchsite to a position and range to the Launchsite in m
        // The basic ATC feature is in here for now
        public static void GetNearestOpenBase(Vector3 position, out string sBase, out float flRange, out KKLaunchSite lNearest)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            KKLaunchSite lNearestBase = null;
            KKLaunchSite lKSC = null;

            foreach (KKLaunchSite site in allLaunchSites)
            {

                if (site.isOpen)
                {
                    var radialposition = site.lsGameObject.transform.position;
                    var dist = Vector3.Distance(position, radialposition);

                    if (site.LaunchSiteName == "Runway")
                    {
                        if (lNearestBase == null)
                        {
                            lNearestBase = site;
                        }

                        lKSC = site;
                    }
                    else
                        if (site.LaunchSiteName != "LaunchPad")
                    {
                        if ((float)dist < (float)smallestDist)
                        {
                            {
                                sNearestBase = site.LaunchSiteName;
                                lNearestBase = site;
                                smallestDist = dist;
                            }
                        }
                    }
                    else
                    {
                        lKSC = site;
                    }
                }
            }

            if (sNearestBase == "")
            {
                sNearestBase = "KSC";
                lNearestBase = lKSC;
            }

            rangeNearestOpenBase = (float)smallestDist;

            // Air traffic control messaging
            if (LandingGuideUI.instance.IsOpen())
            {
                if (sNearestBase != nearestOpenBase)
                {
                    if (rangeNearestOpenBase < 25000)
                    {
                        nearestOpenBase = sNearestBase;
                        MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.BLUE;
                        MessageSystem.Message m = new MessageSystem.Message("KK ATC", "You have entered the airspace of " + sNearestBase + " ATC. Please keep this channel open and obey all signal lights. Thank you. " + sNearestBase + " Air Traffic Control out.", color, MessageSystemButton.ButtonIcons.MESSAGE);
                        MessageSystem.Instance.AddMessage(m);
                    }
                    else
                        if (nearestOpenBase != "")
                    {
                        // you have left ...
                        MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.GREEN;
                        MessageSystem.Message m = new MessageSystem.Message("KK ATC", "You are now leaving the airspace of " + sNearestBase + ". Safe journey. " + sNearestBase + " Air Traffic Control out.", color, MessageSystemButton.ButtonIcons.MESSAGE);
                        MessageSystem.Instance.AddMessage(m);
                        nearestOpenBase = "";
                    }
                }
            }

            sBase = sNearestBase;
            flRange = rangeNearestOpenBase;
            lNearest = lNearestBase;
        }

        // Returns the nearest Launchsite to a position and range in m to the Launchsite, regardless of whether it is open or closed
        public static void getNearestBase(Vector3 position, out string sBase, out string sBase2, out float flRange, out KKLaunchSite lSite, out KKLaunchSite lSite2)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            var lastSmallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            KKLaunchSite lTargetSite = null;
            KKLaunchSite lLastSite = null;
            KKLaunchSite lKSC = null;
            string sLastNearest = "";


            foreach (KKLaunchSite site in allLaunchSites)
            {
                if (site.lsGameObject == null)
                    continue;

                var radialposition = site.lsGameObject.transform.position;
                var dist = Vector3.Distance(position, radialposition);

                if (radialposition == position)
                    continue;

                if (site.LaunchSiteName == "Runway" || site.LaunchSiteName == "LaunchPad")
                {
                    lKSC = site;
                }
                else
                {
                    if ((float)dist < (float)smallestDist)
                    {
                        sLastNearest = sNearestBase;
                        lLastSite = lTargetSite;
                        lastSmallestDist = smallestDist;
                        sNearestBase = site.LaunchSiteName;
                        smallestDist = dist;
                        lTargetSite = site;
                    }
                    else if (dist < lastSmallestDist)
                    {
                        sLastNearest = site.LaunchSiteName;
                        lastSmallestDist = dist;
                        lLastSite = site;
                    }
                }
            }

            if (sNearestBase == "")
            {
                sNearestBase = "KSC";
                lTargetSite = lKSC;
            }
            if (sLastNearest == "")
            {
                sLastNearest = "KSC";
                lLastSite = lKSC;
            }

            rangeNearestBase = (float)smallestDist;

            sBase = sNearestBase;
            sBase2 = sLastNearest;
            flRange = rangeNearestBase;
            lSite = lTargetSite;
            lSite2 = lLastSite;
        }

        public static void ResetLaunchSiteFacilityName()
        {
            Log.Normal("ResetLaunchSiteName");
            ;
            if (currentLaunchSite == "Runway" || currentLaunchSite == "LaunchPad" || currentLaunchSite == "KSC" || currentLaunchSite == "")
            {
                return;
            }
            // reset the name to the site, so it can be fetched again
            KKLaunchSite lastSite = LaunchSiteManager.GetCurrentLaunchSite();
            lastSite.spaceCenterFacility.name = lastSite.LaunchSiteName;
        }


        // Pokes KSP to change the launchsite to use. There's near hackery here again that may get broken by Squad
        // This only works because they use multiple variables to store the same value, basically its black magic
        // Original author: medsouz
        public static void setLaunchSite(KKLaunchSite site)
        {
            // set the facilityname of the old site to its original value, so it can be found later
            ResetLaunchSiteFacilityName();

            //Log.Normal("EditorDriver thinks this is: " + EditorDriver.SelectedLaunchSiteName);
            // without detouring some internal functions we have to fake the facility name... which is pretty bad
            if (site.spaceCenterFacility != null)
            {
                if (EditorDriver.editorFacility == EditorFacility.SPH)
                {
                    site.spaceCenterFacility.name = "Runway";
                }
                else
                {
                    site.spaceCenterFacility.name = "LaunchPad";
                }

                //var field = typeof(EditorDriver).GetField("selectedlaunchSiteName", BindingFlags.Static | BindingFlags.NonPublic);
                //field.SetValue(null, site.LaunchSiteName);
            }
            Log.Normal("Setting LaunchSite to " + site.LaunchSiteName);
            currentLaunchSite = site.LaunchSiteName;

            EditorLogic.fetch.launchSiteName = site.LaunchSiteName;
            //Log.Normal("EditorDriver still thinks this is: " + EditorDriver.SelectedLaunchSiteName);
        }



        // Returns the internal launchSite that KSP has been told is the launchsite
        public static string getCurrentLaunchSite()
        {
            return currentLaunchSite;

        }

        internal static bool CheckLaunchSiteIsValid(KKLaunchSite site)
        {
            if (!KerbalKonstructs.instance.launchFromAnySite && (EditorDriver.editorFacility == EditorFacility.VAB) && (site.LaunchSiteType == SiteType.SPH))
            {
                return false;
            }
            if (!KerbalKonstructs.instance.launchFromAnySite && (EditorDriver.editorFacility == EditorFacility.SPH) && (site.LaunchSiteType == SiteType.VAB))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the currently available default LaunchSite in a editor
        /// </summary>
        /// <returns></returns>
        internal static KKLaunchSite GetDefaultSite()
        {
            KKLaunchSite defaultSite = null;
            if (EditorDriver.editorFacility == EditorFacility.VAB)
            {
                try
                {

                    defaultSite = GetLaunchSiteByName(KerbalKonstructs.instance.defaultVABlaunchsite);
                    if (defaultSite != null && defaultSite.isOpen)
                    {
                        return defaultSite;
                    }
                    else
                    {
                        Log.UserError("DefaultSite is null");
                        defaultSite = GetLaunchSiteByName("LaunchPad");
                        KerbalKonstructs.instance.defaultSPHlaunchsite = "LaunchPad";
                        return defaultSite;
                    }

                }
                catch
                {

                    Log.Error("LaunchSite is broken");
                    defaultSite = GetLaunchSiteByName("LaunchPad");
                    KerbalKonstructs.instance.defaultSPHlaunchsite = "LaunchPad";
                    return defaultSite;
                }
            }

            else
            {
                try
                {
                    defaultSite = GetLaunchSiteByName(KerbalKonstructs.instance.defaultSPHlaunchsite);
                    if (defaultSite.isOpen)
                    {
                        Log.Normal("defaultSite retuned: " + defaultSite.LaunchSiteName);
                        return defaultSite;
                    }
                    else
                    {
                        Log.Error("LaunchSite is invalid");
                        defaultSite = GetLaunchSiteByName("Runway");
                        KerbalKonstructs.instance.defaultSPHlaunchsite = "Runway";
                        return defaultSite;
                    }
                }
                catch
                {
                    Log.Error("LaunchSite is broken");
                    defaultSite = GetLaunchSiteByName("Runway");
                    KerbalKonstructs.instance.defaultSPHlaunchsite = "Runway";
                    return defaultSite;
                }

            }
        }

    }
}
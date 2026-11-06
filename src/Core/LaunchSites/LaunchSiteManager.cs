using KerbalKonstructs.UI;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


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
        
        public static float rangeNearestOpenBase = 0f;
        public static string nearestOpenBase = "";
        public static float rangeNearestBase = 0f;
        public static string nearestBase = "";

        internal static KKLaunchSite runway = new KKLaunchSite { isSquad = true };
        internal static KKLaunchSite launchpad = new KKLaunchSite { isSquad = true };
        internal static KKLaunchSite ksc2 = new KKLaunchSite { isSquad = true };

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
            StaticInstance rwInstance = new StaticInstance();
            rwInstance.gameObject = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "FlagPole").First().gameObject;
            rwInstance.RefLatitude = getKSCLat;
            rwInstance.RefLongitude = getKSCLon;
            rwInstance.CelestialBody = ConfigUtil.GetCelestialBody("HomeWorld");
            rwInstance.RadialPosition = rwInstance.radialPosition;
            rwInstance.hasLauchSites = true;
            rwInstance.launchSite = runway;

            StaticInstance padInstance = new StaticInstance();
            padInstance.gameObject = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "FlagPole").First().gameObject;
            padInstance.RefLatitude = getKSCLat;
            padInstance.RefLongitude = getKSCLon;
            padInstance.CelestialBody = ConfigUtil.GetCelestialBody("HomeWorld");
            padInstance.RadialPosition = rwInstance.radialPosition;
            padInstance.hasLauchSites = true;
            padInstance.launchSite = launchpad;

            runway.staticInstance = rwInstance;
            runway.isSquad = true;
            runway.LaunchSiteName = "Runway";
            runway.LaunchSiteAuthor = "Squad";
            runway.LaunchSiteType = SiteType.SPH;
            runway.sitecategory = LaunchSiteCategory.Runway;
            runway.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false);
            runway.LaunchSiteDescription = "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.";
            runway.body = ConfigUtil.GetCelestialBody("HomeWorld");
            runway.refLat = (float)rwInstance.RefLatitude;
            runway.refLon = (float)rwInstance.RefLongitude;
            runway.refAlt = 69f;
            runway.LaunchSiteLength = 2500f;
            runway.LaunchSiteWidth = 0f;
            runway.InitialCameraRotation = -60f;
            runway.staticInstance.mesh = rwInstance.gameObject;
            runway.SetOpen();

            launchpad.staticInstance = padInstance;
            launchpad.isSquad = true;
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
            launchpad.LaunchSiteLength = 0f;
            launchpad.LaunchSiteWidth = 0f;
            launchpad.InitialCameraRotation = -60f;
            launchpad.staticInstance.mesh = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "FlagPole").First().gameObject;
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
            //ksc2Instance.pqsCity = ksc2PQS;
            ksc2Instance.RadialPosition = ksc2PQS.repositionRadial;
            ksc2Instance.RefLatitude = KKMath.GetLatitudeInDeg(ksc2PQS.repositionRadial);
            ksc2Instance.RefLongitude = KKMath.GetLongitudeInDeg(ksc2PQS.repositionRadial);
            ksc2Instance.CelestialBody = body;
            ksc2Instance.groupCenter = StaticDatabase.GetGroupCenter(body.name + "_KSC2_Builtin");

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
            ksc2.staticInstance.mesh = ksc2PQS.gameObject;
            ksc2.OpenCost = 1f;
            ksc2.SetClosed();
            ksc2.LaunchSiteIsHidden = true;
            ksc2.isSquad = true;

            ksc2Instance.launchSite = ksc2;
            ksc2Instance.groupCenter.launchsites.Add(ksc2);

            RegisterLaunchSite(ksc2);
        }


        internal static void AddKSCSites ()
        {
            AddKSC();
            AddKSC2();
        }


        /// <summary>
        /// contructor
        /// </summary>
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
        public static void CloseLaunchSite(KKLaunchSite site)
        {
            site.SetClosed();
            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory") && HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Log.Normal("Close: " + site.LaunchSiteName);
                AlterMHSelector();
            }
        }

        /// <summary>
        /// Function that is called when a launchSite is opened
        /// </summary>
        public static void OpenLaunchSite(KKLaunchSite site)
        {
            if (site.staticInstance.isSpawned && site.staticInstance.destructible != null)
            {
                site.staticInstance.TrySpawn();
                site.staticInstance.destructible.Reset();
            }

            site.staticInstance.isDestroyed = false;
            site.SetOpen();
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
            //instance.TrySpawn();
            KKLaunchSite mySite = new KKLaunchSite();
            mySite.ParseLSConfig(instance, cfgNode);
            instance.hasLauchSites = true;
            instance.launchSite = mySite;
            RegisterLaunchSite(mySite);
            instance.groupCenter.launchsites.Add(mySite);
            if (mySite.LaunchSiteIsHidden)
            {
                instance.groupCenter.hidden = true;
            }
        }

        /// <summary>
        /// Registers the a created LaunchSite to the PSystemSetup and LaunchSiteManager
        /// </summary>
        /// <param name="site"></param>
        public static void RegisterLaunchSite(KKLaunchSite site)
        {

            if (string.IsNullOrEmpty(site.LaunchSiteName))
            {
                Log.UserWarning("No LaunchSiteName specified:" + site);
                return;
            }


            if (site.isSquad)
            {
                if (site.staticInstance.mesh.transform.Find(site.LaunchPadTransform) == null)
                {
                    Log.UserWarning("Launch pad transform \"" + site.LaunchPadTransform + "\" missing for " + site.LaunchSiteName);
                    return;
                }
            }
            else
            {
                if (site.staticInstance.model.prefab.transform.Find(site.LaunchPadTransform) == null)
                {
                    Log.UserWarning("Launch pad transform \"" + site.LaunchPadTransform + "\" missing for " + site.LaunchSiteName);
                    return;
                }
            }

            KKFacilities = PSystemSetup.Instance.SpaceCenterFacilities.ToList();
            if (KKFacilities.Where(fac => fac.facilityName == site.LaunchSiteName).FirstOrDefault() != null)
            {

                Log.Error("Launch site " + site.LaunchSiteName + " already exists.");
                return;
            }


            //site.staticInstance.gameObject.transform.name = site.LaunchSiteName;
            //site.staticInstance.gameObject.name = site.LaunchSiteName;
            Log.Normal("Registering LaunchSite: " + site.LaunchSiteName + " isHidden: " + site.LaunchSiteIsHidden) ;
            PSystemSetup.SpaceCenterFacility spaceCenterFacility = new PSystemSetup.SpaceCenterFacility();
            spaceCenterFacility.name = site.LaunchSiteName;
            spaceCenterFacility.facilityDisplayName = site.LaunchSiteName;
            spaceCenterFacility.facilityName = site.LaunchSiteName;
            spaceCenterFacility.facilityPQS = site.staticInstance.CelestialBody.pqsController;
            spaceCenterFacility.hostBody = site.staticInstance.CelestialBody;
            spaceCenterFacility.facilityTransform = site.staticInstance.mesh.transform;
            if (site.LaunchSiteType == SiteType.VAB)
            {
                spaceCenterFacility.editorFacility = EditorFacility.VAB;
            }
            else
            {
                spaceCenterFacility.editorFacility = EditorFacility.SPH;
            }
            spaceCenterFacility.pqsName = site.body.pqsController.name;


            //if (site.staticInstance.groupCenter == null)
            //{
            //    spaceCenterFacility.facilityTransformName = site.staticInstance.gameObject.name;
            //}
            //else
            //{
            //    spaceCenterFacility.facilityTransformName = site.staticInstance.groupCenter.gameObject.name + "/" + site.staticInstance.gameObject.name + "/Mesh";
            //}
            // newFacility.facilityTransform = site.lsGameObject.transform.Find(site.LaunchPadTransform);
            //     newFacility.facilityTransformName = instance.gameObject.transform.name;

            PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint
            {
                name = site.LaunchSiteName,
                spawnTransformURL = site.LaunchPadTransform
            };
            spawnPoint.Setup(spaceCenterFacility);
            spawnPoint.SetSpawnPointLatLonAlt();
            spaceCenterFacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[] { spawnPoint };

            //spaceCenterFacility.Setup(new PQS[] { site.staticInstance.CelestialBody.pqsController });

            KKFacilities.Add(spaceCenterFacility);
            KKFacilities.Sort(delegate (PSystemSetup.SpaceCenterFacility a, PSystemSetup.SpaceCenterFacility b)
            {
                if (a.editorFacility == EditorFacility.None)
                {
                    return 1;
                }
                return (a.facilityDisplayName).CompareTo(b.facilityDisplayName);
            });

            PSystemSetup.Instance.SpaceCenterFacilities = KKFacilities.ToArray();
            site.spaceCenterFacility = spaceCenterFacility;

            if (site.staticInstance.destructible != null)
            {
                ScenarioDestructibles.RegisterDestructible(site.staticInstance.destructible, site.LaunchSiteName);
            }

            AddLaunchSite(site);

            if (site.staticInstance.gameObject != null)
            {
                CustomSpaceCenter.CreateFromLaunchsite(site);
            }


            //if (PSystemSetup.Instance.SpaceCenterFacilities.ToList().Where(fac => fac.facilityName == site.LaunchSiteName).FirstOrDefault() != null)
            //{
            //    Log.Normal("LaunchSite registered: " + site.LaunchSiteName);
            //}
            //else
            //{
            //    Log.Normal("LaunchSite registration failed: " + site.LaunchSiteName);
            //}

        }


        public static void AlterMHSelector()
        {
            Log.Normal("working on launchsites");
            if (!HighLogic.CurrentGame.Parameters.Difficulty.AllowOtherLaunchSites)
            {
                return;
            }

            RegisterMHLaunchSites(EditorDriver.editorFacility);

            KerbalKonstructs.calledByAlterMHSelector = true;
            GameEvents.onEditorRestart.Fire();

            KerbalKonstructs.calledByAlterMHSelector = false;
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

        /// <summary>
        /// Removes the launchSite from the facilities
        /// </summary>
        /// <param name="site"></param>
        public static void UnregisterLaunchSite(KKLaunchSite site)
        {
            if (site.isOpen)
            {
                CloseLaunchSite(site);
            }

            List<PSystemSetup.SpaceCenterFacility> spaceCenters = new List<PSystemSetup.SpaceCenterFacility>();

            foreach (PSystemSetup.SpaceCenterFacility center in PSystemSetup.Instance.SpaceCenterFacilities)
            {
                if (center.facilityDisplayName == site.LaunchSiteName)
                {
                    Log.Normal("Launchsite: " + site.LaunchSiteName + " sucessfully unregistered");
                    continue;
                }
                else
                {
                    spaceCenters.Add(center);
                }
            }

            PSystemSetup.Instance.SpaceCenterFacilities = spaceCenters.ToArray();

        }


        /// <summary>
        /// Deletes a LaunchSite from the internal Database
        /// </summary>
        /// <param name="site2delete"></param>
        public static void DeleteLaunchSite(KKLaunchSite site2delete)
        {
            if (launchSites.Contains(site2delete))
            {
                launchSites.Remove(site2delete);

                CustomSpaceCenter csc = SpaceCenterManager.GetCSC(site2delete.LaunchSiteName);
                if (csc != null)
                {
                    SpaceCenterManager.spaceCenters.Remove(csc);
                }

                allLaunchSites = launchSites.OrderBy(ls => ls.staticInstance.Group).ThenBy(ls => ls.LaunchSiteName).ToArray();
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
            launchSites.Sort(delegate (KKLaunchSite a, KKLaunchSite b)
            {
                return (a.LaunchSiteName).CompareTo(b.LaunchSiteName);
            });
            allLaunchSites = launchSites.OrderBy(ls => ls.staticInstance.Group).ThenBy(ls => ls.LaunchSiteName).ToArray();
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
            return (launchSiteNames.Contains(siteName));
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
                        Log.Normal("found LS: " + mySite.LaunchSiteName);
                    }
                }
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
            return Vector3.Distance(position, site.staticInstance.transform.position);
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
                    var radialposition = site.staticInstance.transform.position;
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

            if (sNearestBase.Length == 0)
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
                        if (nearestOpenBase.Length != 0)
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
                if (site.staticInstance.gameObject == null)
                {
                    continue;
                }

                var radialposition = site.staticInstance.gameObject.transform.position;
                var dist = Vector3.Distance(position, radialposition);

                if (radialposition == position)
                {
                    continue;
                }

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

            if (sNearestBase.Length == 0)
            {
                sNearestBase = "KSC";
                lTargetSite = lKSC;
            }
            if (sLastNearest.Length == 0)
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
            if (currentLaunchSite == "Runway" || currentLaunchSite == "LaunchPad" || currentLaunchSite == "KSC" || currentLaunchSite.Length == 0)
            {
                return;
            }
            // reset the name to the site, so it can be fetched again
            KKLaunchSite lastSite = LaunchSiteManager.GetCurrentLaunchSite();
            if (lastSite != null)
            {
                lastSite.spaceCenterFacility.name = lastSite.LaunchSiteName;
            }
            else
            {
                // do nothing
            }
        }


        // Pokes KSP to change the launchsite to use. There's near hackery here again that may get broken by Squad
        // This only works because they use multiple variables to store the same value, basically its black magic
        // Original author: medsouz
        public static void setLaunchSite(KKLaunchSite site)
        {

            ResetLaunchSiteFacilityName();

            ////Log.Normal("EditorDriver thinks this is: " + EditorDriver.SelectedLaunchSiteName);
            //// without detouring some internal functions we have to fake the facility name... which is pretty bad
            //if (site.spaceCenterFacility != null)
            //{
            //    if (EditorDriver.editorFacility == EditorFacility.SPH)
            //    {
            //        site.spaceCenterFacility.name = "Runway";
            //    }
            //    else
            //    {
            //        site.spaceCenterFacility.name = "LaunchPad";
            //    }

            //    //var field = typeof(EditorDriver).GetField("selectedlaunchSiteName", BindingFlags.Static | BindingFlags.NonPublic);
            //    //field.SetValue(null, site.LaunchSiteName);
            //}

            Log.Normal("Setting LaunchSite to " + site.LaunchSiteName);
            currentLaunchSite = site.LaunchSiteName;

            EditorLogic.fetch.launchSiteName = site.LaunchSiteName;
        }



        // Returns the internal launchSite that KSP has been told is the launchsite
        public static string getCurrentLaunchSite()
        {
            return currentLaunchSite;

        }

        internal static bool CheckLaunchSiteIsValid(KKLaunchSite site)
        {
            // check for deleted launchsites
            if (site == null)
            {
                return false;
            }

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
            KKLaunchSite defaultSite ;
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
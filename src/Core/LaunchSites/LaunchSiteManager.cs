using KerbalKonstructs.Utilities;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using System.Collections.Generic;


namespace KerbalKonstructs.Core
{
    public class LaunchSiteManager
    {
        private static List<LaunchSite> launchSites = new List<LaunchSite>();
        private static string currentLaunchSite = "Runway";
        private static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
        public static float rangeNearestOpenBase = 0f;
        public static string nearestOpenBase = "";
        public static float rangeNearestBase = 0f;
        public static string nearestBase = "";

        internal static LaunchSite runway = new LaunchSite();
        internal static LaunchSite launchpad = new LaunchSite();



        // Handy get of all launchSites
        public static List<LaunchSite> AllLaunchSites { get { return launchSites; } }

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
            runway.Category = "Runway";
            runway.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false);
            runway.LaunchSiteDescription = "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.";
            runway.OpenCloseState = "Open";
            runway.body = ConfigUtil.GetCelestialBody("HomeWorld");
            runway.refLat = getKSCLat;
            runway.refLon = getKSCLon;
            runway.refAlt = 69f;
            runway.LaunchSiteLength = 2500f;
            runway.LaunchSiteWidth = 75f;
            runway.RecoveryFactor = 100f;
            runway.RecoveryRange = 0f;
            runway.lsGameObject = SpaceCenter.Instance.gameObject;

            launchpad.LaunchSiteName = "LaunchPad";
            launchpad.LaunchSiteAuthor = "Squad";
            launchpad.LaunchSiteType = SiteType.VAB;
            launchpad.Category = "RocketPad";
            launchpad.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCLaunchpad", false);
            launchpad.LaunchSiteDescription = "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...";
            launchpad.OpenCloseState = "Open";
            launchpad.body = ConfigUtil.GetCelestialBody("HomeWorld");
            launchpad.refLat = getKSCLat;
            launchpad.refLon = getKSCLon;
            launchpad.refAlt = 72;
            launchpad.LaunchSiteLength = 20f;
            launchpad.LaunchSiteWidth = 20f;
            launchpad.RecoveryFactor = 100f;
            launchpad.RecoveryRange = 0f;
            launchpad.lsGameObject = SpaceCenter.Instance.gameObject;


            launchSites.Add(runway);
            launchSites.Add(launchpad);
        }

        static LaunchSiteManager()
        {
            AddKSC();
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
        /// Creates a new LaunchSite out of a cfg-node and Registers it with RegisterLaunchSite
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="cfgNode"></param>
        internal static void CreateLaunchSite(StaticInstance instance, ConfigNode cfgNode)
        {
            LaunchSite newSite = instance.gameObject.AddComponent<LaunchSite>().ParseConfig(cfgNode) as LaunchSite;
            instance.hasLauchSites = true;
            instance.launchSite = newSite;
            newSite.parentInstance = instance;
            RegisterLaunchSite(newSite);
        }

        /// <summary>
        /// Registers the a created LaunchSite to the PSystemSetup and LaunchSiteManager
        /// </summary>
        /// <param name="site"></param>
        internal static void RegisterLaunchSite(LaunchSite site)
        {
            if (! string.IsNullOrEmpty(site.LaunchSiteName) && site.parentInstance.gameObject.transform.Find(site.LaunchPadTransform) != null)
            {
                site.parentInstance.gameObject.transform.name = site.LaunchSiteName;
                site.parentInstance.gameObject.name = site.LaunchSiteName;

                FieldInfo sitesField = typeof(PSystemSetup).GetField("facilities", BindingFlags.NonPublic | BindingFlags.Instance);
                List<PSystemSetup.SpaceCenterFacility> facilities = ((PSystemSetup.SpaceCenterFacility[])sitesField.GetValue(PSystemSetup.Instance)).ToList();            
                if (PSystemSetup.Instance.GetSpaceCenterFacility(site.LaunchSiteName) == null)
                {
                    PSystemSetup.SpaceCenterFacility newFacility = new PSystemSetup.SpaceCenterFacility();
                    newFacility.name = "";
                    newFacility.facilityName = site.LaunchSiteName;
                    newFacility.facilityPQS = site.parentInstance.CelestialBody.pqsController;
                    newFacility.facilityTransformName = site.parentInstance.gameObject.name;
                    // newFacility.facilityTransform = site.lsGameObject.transform.Find(site.LaunchPadTransform);
                    //     newFacility.facilityTransformName = instance.gameObject.transform.name;
                    newFacility.pqsName = site.body.pqsController.name;
                    PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
                    spawnPoint.name = site.LaunchSiteName;
                    spawnPoint.spawnTransformURL = site.LaunchPadTransform;
                    newFacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[1];
                    newFacility.spawnPoints[0] = spawnPoint;

                    facilities.Add(newFacility);
                    sitesField.SetValue(PSystemSetup.Instance, facilities.ToArray());

                    site.facility = newFacility;

                    launchSites.Add(site);
                }
                else
                {
                    Log.Error("Launch site " + site.name + " already exists.");
                }
                MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
                if (updateSitesMI == null)
                    Log.UserError("You are screwed. Failed to find SetupFacilities().");
                else
                    updateSitesMI.Invoke(PSystemSetup.Instance, null);

                if (site.parentInstance.gameObject != null)
                {                    
                    CustomSpaceCenter.CreateFromLaunchsite(site);
                }
            }
            else
            {
                Log.UserWarning("Launch pad transform \"" + site.LaunchPadTransform + "\" missing for " + site.LaunchSiteName);
            }

        }


        internal static void DeleteLaunchSite (LaunchSite site2delete)
        {
            if (launchSites.Contains(site2delete))
            {
                launchSites.Remove(site2delete);

                CustomSpaceCenter csc = SpaceCenterManager.GetCSC(site2delete.LaunchSiteName);
                if (csc != null)
                {
                    SpaceCenterManager.spaceCenters.Remove(csc);
                }
            } 
        }


        internal static LaunchSite GetCurrentLaunchSite()
        {
            return launchSites.Where(site => site.LaunchSiteName == currentLaunchSite).FirstOrDefault();
        }

        // Legacy Functions used by other mods

        // Returns a list of launchsites. Supports category filtering.
        public static List<LaunchSite> getLaunchSites(String usedFilter = "ALL")
        {
            List<LaunchSite> sites = new List<LaunchSite>();
            foreach (LaunchSite site in launchSites)
            {
                if (usedFilter.Equals("ALL"))
                {
                    sites.Add(site);
                }
                else
                {
                    if (site.Category.Equals(usedFilter))
                    {
                        sites.Add(site);
                    }
                }
            }
            return sites;
        }

        // Returns a list of launchsites. Supports sitetype and category filtering.
        public static List<LaunchSite> getLaunchSites(SiteType type, Boolean allowAny = true, String appliedFilter = "ALL")
        {
            List<LaunchSite> sites = new List<LaunchSite>();
            foreach (LaunchSite site in launchSites)
            {
                if (site.LaunchSiteType.Equals(type) || (site.LaunchSiteType.Equals(SiteType.Any) && allowAny))
                {
                    if (appliedFilter.Equals("ALL"))
                    {
                        sites.Add(site);
                    }
                    else
                    {
                        if (site.Category.Equals(appliedFilter))
                        {
                            sites.Add(site);
                        }
                    }
                }
            }
            return sites;
        }

        // Open or close a launchsite
        public static void setSiteOpenCloseState(string sSiteName, string sState)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    site.OpenCloseState = sState;
                    return;
                }
            }
        }

        // Lock a launchsite so it cannot be opened or closed or launched from
        public static void setSiteLocked(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    site.OpenCloseState = site.OpenCloseState + "Locked";
                    return;
                }
            }
        }

        // Unlock a launchsite so it can be opened or closed or launched from
        public static void setSiteUnlocked(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    if (site.OpenCloseState == "OpenLocked")
                        site.OpenCloseState = "Open";
                    else
                        site.OpenCloseState = "Closed";
                    return;
                }
            }
        }

        // Returns whether a site is open or closed and how much it costs to open
        public static void getSiteOpenCloseState(string sSiteName, out string sOpenCloseState, out float fOpenCost)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    sOpenCloseState = site.OpenCloseState;
                    fOpenCost = site.OpenCost;
                    return;
                }
            }

            sOpenCloseState = "Open";
            fOpenCost = 0;
        }

        // Returns whether a site is locked
        public static bool getIsSiteLocked(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    if (site.OpenCloseState == "OpenLocked" || site.OpenCloseState == "ClosedLocked")
                        return true;
                }
            }
            return false;
        }

        // Returns whether a site is open
        public static bool getIsSiteOpen(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    if ((site.OpenCloseState == "Open") || (site.OpenCloseState == "OpenLocked"))
                        return true;
                }
            }
            return false;
        }

        // Returns whether a site is closed
        public static bool getIsSiteClosed(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    if (site.OpenCloseState == "Closed")
                        return true;
                }
            }
            return false;
        }

        // Returns the launch refund percentage of a site
        public static void getSiteLaunchRefund(string sSiteName, out float fRefund)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    fRefund = site.LaunchRefund;
                    return;
                }
            }

            fRefund = 0;
        }

        // Returns the GameObject of a site
        public static GameObject getSiteGameObject(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    return site.lsGameObject;
                }
            }

            return null;
        }

        // Returns the StaticObject of a site. Can provide a sitename or a GameObject
        public static StaticInstance getSiteStaticObject(string siteName, GameObject go = null)
        {
            StaticInstance soSite = null;
            if (go != null)
            {
                soSite = InstanceUtil.GetStaticInstanceForGameObject(go);

                if (soSite == null) return null;
                return soSite;
            }

            string sName = "";
            object oName = null;
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances.Where(inst => inst.hasLauchSites == true))
            {

                oName = instance.launchSite.LaunchSiteName;
                if (oName == null) continue;

                oName = null;

                sName = instance.launchSite.LaunchSiteName;
                if (sName == siteName) return instance;
            }

            return null;
        }

        public static string sBaseMem = "";
        public static LaunchSite launchsitemem = null;

        // Returns if a launchsite exists. Hook used by KerKonConConExt
        public static bool checkLaunchSiteExists(string sSiteName)
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    return true;
                }
            }

            return false;
        }

        // Returns a specific Launchsite, keyed by site.name
        public static LaunchSite getLaunchSiteByName(string sSiteName, bool bRemember = false)
        {
            if (bRemember)
            {
                if (sBaseMem == sSiteName) return launchsitemem;
            }

            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site.LaunchSiteName == sSiteName)
                {
                    if (bRemember)
                    {
                        sBaseMem = sSiteName;
                        launchsitemem = site;
                    }
                    return site;
                }
            }

            return null;
        }

        // Closes all launchsites. Necessary when leaving a career game and going to the main menu
        public static void setAllLaunchsitesClosed()
        {
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                site.OpenCloseState = "Closed";
            }
        }

        // Returns the distance in m from a position to a specified Launchsite
        public static float getDistanceToBase(Vector3 position, LaunchSite lTarget)
        {
            float flRange = 0f;

            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            foreach (LaunchSite site in sites)
            {
                if (site == lTarget)
                {
                    var radialposition = site.lsGameObject.transform.position;
                    var dist = Vector3.Distance(position, radialposition);
                    flRange = dist;
                }
            }

            return flRange;
        }

        // Returns the nearest open Launchsite to a position and range to the Launchsite in m
        // The basic ATC feature is in here for now
        public static void GetNearestOpenBase(Vector3 position, out string sBase, out float flRange, out LaunchSite lNearest)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            string sOpenCloseState = "";
            LaunchSite lNearestBase = null;
            LaunchSite lKSC = null;

            List<LaunchSite> basesites = LaunchSiteManager.getLaunchSites();

            foreach (LaunchSite site in basesites)
            {
                sOpenCloseState = site.OpenCloseState;

                if (!MiscUtils.isCareerGame())
                    sOpenCloseState = "Open";

                if (sOpenCloseState == "Open")
                {
                    var radialposition = site.lsGameObject.transform.position;
                    var dist = Vector3.Distance(position, radialposition);

                    if (site.LaunchSiteName == "Runway")
                    {
                        if (lNearestBase == null)
                            lNearestBase = site;
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
            if (KerbalKonstructs.GUI_Landinguide.IsOpen())
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
        public static void getNearestBase(Vector3 position, out string sBase, out string sBase2, out float flRange, out LaunchSite lSite, out LaunchSite lSite2)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            var lastSmallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            LaunchSite lTargetSite = null;
            LaunchSite lLastSite = null;
            LaunchSite lKSC = null;
            string sLastNearest = "";

            List<LaunchSite> basesites = LaunchSiteManager.getLaunchSites();

            foreach (LaunchSite site in basesites)
            {
                if (site.lsGameObject == null) continue;

                var radialposition = site.lsGameObject.transform.position;
                var dist = Vector3.Distance(position, radialposition);

                if (radialposition == position) continue;

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

        // Pokes KSP to change the launchsite to use. There's near hackery here again that may get broken by Squad
        // This only works because they use multiple variables to store the same value, basically its black magic
        // Original author: medsouz
        public static void setLaunchSite(LaunchSite site)
        {
            if (site.facility != null)
            {
                if (EditorDriver.editorFacility.Equals(EditorFacility.SPH))
                {
                    site.facility.name = "Runway";
                }
                else
                {
                    site.facility.name = "LaunchPad";
                }
            }
            currentLaunchSite = site.LaunchSiteName;
            EditorLogic.fetch.launchSiteName = site.LaunchSiteName;
        }

        // Returns the internal launchSite that KSP has been told is the launchsite
        public static string getCurrentLaunchSite()
        {
            return currentLaunchSite;

        }
    }
}
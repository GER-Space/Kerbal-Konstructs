using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using System.Collections.Generic;
using KerbalKonstructs.UI;


namespace KerbalKonstructs.Core
{
    public class LaunchSiteManager
    {
        private static Dictionary<string,LaunchSite> launchSites = new Dictionary<string, LaunchSite>();
        private static string currentLaunchSite = "Runway";
        private static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
        public static float rangeNearestOpenBase = 0f;
        public static string nearestOpenBase = "";
        public static float rangeNearestBase = 0f;
        public static string nearestBase = "";

        internal static LaunchSite runway = new LaunchSite();
        internal static LaunchSite launchpad = new LaunchSite();



        // Handy get of all launchSites
        public static LaunchSite[] allLaunchSites = null;


        // API for Kerbal Construction Time not for internal use
        public static List<LaunchSite> AllLaunchSites
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
            runway.Category = LaunchSiteCategory.Runway;
            runway.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false);
            runway.LaunchSiteDescription = "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.";
            runway.OpenCloseState = "Open";
            runway.isOpen = true;
            runway.body = ConfigUtil.GetCelestialBody("HomeWorld");
            runway.refLat = getKSCLat;
            runway.refLon = getKSCLon;
            runway.refAlt = 69f;
            runway.LaunchSiteLength = 2500f;
            runway.LaunchSiteWidth = 75f;
            runway.lsGameObject = SpaceCenter.Instance.gameObject;

            launchpad.LaunchSiteName = "LaunchPad";
            launchpad.LaunchSiteAuthor = "Squad";
            launchpad.LaunchSiteType = SiteType.VAB;
            launchpad.Category = LaunchSiteCategory.RocketPad;
            launchpad.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCLaunchpad", false);
            launchpad.LaunchSiteDescription = "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...";
            launchpad.OpenCloseState = "Open";
            launchpad.isOpen = true;
            launchpad.body = ConfigUtil.GetCelestialBody("HomeWorld");
            launchpad.refLat = getKSCLat;
            launchpad.refLon = getKSCLon;
            launchpad.refAlt = 72;
            launchpad.LaunchSiteLength = 20f;
            launchpad.LaunchSiteWidth = 20f;
            launchpad.lsGameObject = SpaceCenter.Instance.gameObject;


            AddLaunchSite(runway);
            AddLaunchSite(launchpad);
        }

        /// <summary>
        /// contructor
        /// </summary>
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
            LaunchSite newSite = (instance.gameObject.AddComponent<LaunchSite>()).ParseConfig(cfgNode) as LaunchSite;
            instance.hasLauchSites = true;
            instance.launchSite = newSite;
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

				List<PSystemSetup.SpaceCenterFacility> facilities = PSystemSetup.Instance.SpaceCenterFacilities.ToList();

                if (facilities.Where(fac => fac.facilityName == site.LaunchSiteName).FirstOrDefault() == null )
                {
                    //Log.Normal("Registering LaunchSite: " + site.LaunchSiteName);
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
					PSystemSetup.Instance.SpaceCenterFacilities = facilities.ToArray();

                    site.facility = newFacility;

                    AddLaunchSite(site);
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
            if (launchSites.ContainsKey(site2delete.LaunchSiteName))
            {
                launchSites.Remove(site2delete.LaunchSiteName);

                CustomSpaceCenter csc = SpaceCenterManager.GetCSC(site2delete.LaunchSiteName);
                if (csc != null)
                {
                    SpaceCenterManager.spaceCenters.Remove(csc);
                }

                allLaunchSites = launchSites.Values.ToArray();
            } 
        }


        internal static void AddLaunchSite(LaunchSite site2add)
        {
            launchSites.Add(site2add.LaunchSiteName, site2add);
            List<LaunchSite> tmpList = launchSites.Values.ToList();
            tmpList.Sort(delegate (LaunchSite a, LaunchSite b)
            {
                return (a.LaunchSiteName).CompareTo(b.LaunchSiteName);
            });
            allLaunchSites = tmpList.ToArray();
        }

        internal static LaunchSite GetCurrentLaunchSite()
        {
            return launchSites[currentLaunchSite];
        }

        // Legacy Functions used by other mods

        //// Returns a list of launchsites. Supports category filtering.
        //public static List<LaunchSite> getLaunchSites(String usedFilter = "ALL")
        //{
        //    List<LaunchSite> sites = new List<LaunchSite>();
        //    foreach (LaunchSite site in launchSites)
        //    {
        //        if (usedFilter.Equals("ALL"))
        //        {
        //            sites.Add(site);
        //        }
        //        else
        //        {
        //            if (site.Category.Equals(usedFilter))
        //            {
        //                sites.Add(site);
        //            }
        //        }
        //    }
        //    return sites;
        //}

        //// Returns a list of launchsites. Supports sitetype and category filtering.
        //public static List<LaunchSite> getLaunchSites(SiteType type, Boolean allowAny = true, String appliedFilter = "ALL")
        //{
        //    List<LaunchSite> sites = new List<LaunchSite>();
        //    foreach (LaunchSite site in launchSites)
        //    {
        //        if (site.LaunchSiteType.Equals(type) || (site.LaunchSiteType.Equals(SiteType.Any) && allowAny))
        //        {
        //            if (appliedFilter.Equals("ALL"))
        //            {
        //                sites.Add(site);
        //            }
        //            else
        //            {
        //                if (site.Category.Equals(appliedFilter))
        //                {
        //                    sites.Add(site);
        //                }
        //            }
        //        }
        //    }
        //    return sites;
        //}

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="sState"></param>
        public static void setSiteOpenCloseState(string siteName, string state)
        {
            if (launchSites.ContainsKey(siteName))
            {
                launchSites[siteName].OpenCloseState = state;
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        public static void setSiteLocked(string siteName)
        {
            if (launchSites.ContainsKey(siteName))
            {
                LaunchSite site = launchSites[siteName];
                launchSites[siteName].OpenCloseState = launchSites[siteName].OpenCloseState + "Locked";                
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        public static void setSiteUnlocked(string siteName)
        {
            if (launchSites.ContainsKey(siteName))
            {
                LaunchSite site = launchSites[siteName];
                if (site.OpenCloseState == "OpenLocked")
                {
                    site.OpenCloseState = "Open";
                }
                else
                {
                    site.OpenCloseState = "Closed";
                }
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="sOpenCloseState"></param>
        /// <param name="fOpenCost"></param>
        public static void getSiteOpenCloseState(string siteName, out string sOpenCloseState, out float fOpenCost)
        {
            if (launchSites.ContainsKey(siteName))
            {
                LaunchSite site = launchSites[siteName];
                sOpenCloseState = site.OpenCloseState;
                fOpenCost = site.OpenCost;
            }

            sOpenCloseState = "Open";
            fOpenCost = 0;
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteLocked(string siteName)
        {
            if (launchSites.ContainsKey(siteName))
            {
                LaunchSite site = launchSites[siteName];
                if (site.OpenCloseState == "OpenLocked" || site.OpenCloseState == "ClosedLocked")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteOpen(string siteName)
        {
            if (launchSites.ContainsKey(siteName))
            {
                return launchSites[siteName].isOpen;             
            } else
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
            if (launchSites.ContainsKey(siteName))
            {
                return (!launchSites[siteName].isOpen);

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
            return launchSites.ContainsKey(siteName);
        }


        // Returns a specific Launchsite, keyed by site.name
        public static LaunchSite GetLaunchSiteByName(string siteName)
        {

            if (checkLaunchSiteExists(siteName))
            {
                return launchSites[siteName];
            }
            else
            {
                return null;
            }
        }


        // Returns the distance in m from a position to a specified Launchsite
        public static float getDistanceToBase(Vector3 position, LaunchSite site)
        {
            return Vector3.Distance(position, site.lsGameObject.transform.position);
        }

        // Returns the nearest open Launchsite to a position and range to the Launchsite in m
        // The basic ATC feature is in here for now
        public static void GetNearestOpenBase(Vector3 position, out string sBase, out float flRange, out LaunchSite lNearest)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            LaunchSite lNearestBase = null;
            LaunchSite lKSC = null;

            foreach (LaunchSite site in allLaunchSites)
            {

                if (site.isOpen)
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


            foreach (LaunchSite site in allLaunchSites)
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
                if (EditorDriver.editorFacility == EditorFacility.SPH)
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

            KerbalKonstructs.instance.lastLaunchSiteUsed = site.LaunchSiteName;

           // SetSpaceCenterCam(site);
        }

        internal static void SetSpaceCenterCam(LaunchSite site)
        {
            SpaceCenterCamera2 scCam = Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>().FirstOrDefault();
            if (scCam != null)
            {
                scCam.initialPositionTransformName = site.lsGameObject.transform.name;
                scCam.transform.parent = site.lsGameObject.transform;
                scCam.transform.position = site.lsGameObject.transform.position;
               // scCam.ResetCamera();
            }
        }


        // Returns the internal launchSite that KSP has been told is the launchsite
        public static string getCurrentLaunchSite()
        {
            return currentLaunchSite;

        }

        internal static bool CheckLaunchSiteIsValid(LaunchSite site)
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
        internal static LaunchSite GetDefaultSite()
        {
            LaunchSite defaultSite = null;
            if (EditorDriver.editorFacility == EditorFacility.VAB)
            {
                defaultSite = GetLaunchSiteByName(KerbalKonstructs.instance.defaultVABlaunchsite);
                if (defaultSite == null || !defaultSite.isOpen)
                {
                    defaultSite = GetLaunchSiteByName("LaunchPad");
                    KerbalKonstructs.instance.defaultVABlaunchsite = "LaunchPad";
                }
            }

            if (EditorDriver.editorFacility == EditorFacility.SPH)
            {
                defaultSite = GetLaunchSiteByName(KerbalKonstructs.instance.defaultSPHlaunchsite);
                if (defaultSite == null || !defaultSite.isOpen)
                {
                    defaultSite = GetLaunchSiteByName("Runway");
                    KerbalKonstructs.instance.defaultSPHlaunchsite = "Runway";
                }
            }

            return defaultSite;
        }


    }
}
using KerbalKonstructs.Core;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;


namespace KerbalKonstructs.Core
{
    public class LaunchSiteManager
    {
        private static List<LaunchSite> launchSites = new List<LaunchSite>();
        private static string lastLaunchSite = "Runway";
        private static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
        public static float rangeNearestOpenBase = 0f;
        public static string nearestOpenBase = "";
        public static float rangeNearestBase = 0f;
        public static string nearestBase = "";

        internal static LaunchSite runway = new LaunchSite();
        internal static LaunchSite launchpad = new LaunchSite();



        private static float getKSCLon
        {
            get
            {
                CelestialBody body = KKAPI.getCelestialBody("HomeWorld");
                var mods = body.pqsController.transform.GetComponentsInChildren(typeof(PQSCity), true);
                double retval = 0d ;

                foreach (var m in mods)
                {
                    PQSCity mod = m as PQSCity;
                    if (mod.name == "KSC")
                    {
                        retval = (NavUtils.GetLongitude(mod.repositionRadial) * KKMath.rad2deg);
                        break;
                    }
                }
                return (float)((360 + retval) % 360);
            }
        }

        private static float getKSCLat
        {
            get
            {
                CelestialBody body = KKAPI.getCelestialBody("HomeWorld");
                var mods = body.pqsController.transform.GetComponentsInChildren(typeof(PQSCity), true);
                double retval = 0d;

                foreach (var m in mods)
                {
                    PQSCity mod = m as PQSCity;
                    if (mod.name == "KSC")
                    {
                        retval = (NavUtils.GetLatitude(mod.repositionRadial) * KKMath.rad2deg);
                        break;
                    }
                }
                return (float)retval;
            }
        }

        /// <summary>
        /// prefills LaunchSiteManager with the runway and the KSC
        /// </summary>
        private static void AddKSC ()
        {

            runway.name = "Runway";
            runway.author = "Squad";
            runway.type = SiteType.SPH;
            runway.category = "Runway";
            runway.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false);
            runway.description = "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.";
            runway.openCloseState = "Open";
            runway.body = KKAPI.getCelestialBody("HomeWorld");
            runway.refLat = getKSCLat;
            runway.refLon = getKSCLon;
            runway.refAlt = 69f;
            runway.siteLength = 2500f;
            runway.siteWidth = 75f;
            runway.recoveryFactor = 100f;
            runway.recoveryRange = 0f;
            runway.gameObject = SpaceCenter.Instance.gameObject;

            launchpad.name = "LaunchPad";
            launchpad.author = "Squad";
            launchpad.type = SiteType.VAB;
            launchpad.category = "RocketPad";
            launchpad.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCLaunchpad", false);
            launchpad.description = "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...";
            launchpad.openCloseState = "Open";
            launchpad.body = KKAPI.getCelestialBody("HomeWorld");
            launchpad.refLat = getKSCLat;
            launchpad.refLon = getKSCLon;
            launchpad.refAlt = 72;
            launchpad.siteLength = 20f;
            launchpad.siteWidth = 20f;
            launchpad.recoveryFactor = 100f;
            launchpad.recoveryRange = 0f;
            launchpad.gameObject = SpaceCenter.Instance.gameObject;


            launchSites.Add(runway);
            launchSites.Add(launchpad);
        }

        static LaunchSiteManager()
        {
            AddKSC();
        }




        /// <summary>
        /// Add a launchsite to the KK launchsite and custom space centre database
		/// Please note there's some near hackery here to get KSP to recognise additional launchsites and space centres
        /// </summary>
        /// <param name="obj"></param>
        public static void createLaunchSite(StaticObject obj)
		{
			if (obj.settings.ContainsKey("LaunchSiteName") && obj.gameObject.transform.Find((string) obj.getSetting("LaunchPadTransform")) != null)
			{
				obj.gameObject.transform.name = (string) obj.getSetting("LaunchSiteName");
				obj.gameObject.name = (string) obj.getSetting("LaunchSiteName");

				CelestialBody CelBody = (CelestialBody)obj.getSetting("CelestialBody");
				var objectpos = CelBody.transform.InverseTransformPoint(obj.gameObject.transform.position);
				var dObjectLat = NavUtils.GetLatitude(objectpos);
				var dObjectLon = NavUtils.GetLongitude(objectpos);
				var disObjectLat = dObjectLat * 180 / Math.PI;
				var disObjectLon = dObjectLon * 180 / Math.PI;

				if (disObjectLon < 0) disObjectLon = disObjectLon + 360;
				obj.setSetting("RefLatitude", (float)disObjectLat);
				obj.setSetting("RefLongitude", (float)disObjectLon);

				foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (fi.FieldType.Name == "SpaceCenterFacility[]")
					{
						PSystemSetup.SpaceCenterFacility[] facilities = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
						if (PSystemSetup.Instance.GetSpaceCenterFacility((string) obj.getSetting("LaunchSiteName")) == null)
						{
							PSystemSetup.SpaceCenterFacility newFacility = new PSystemSetup.SpaceCenterFacility();
							newFacility.name = "FacilityName";
							newFacility.facilityName = (string) obj.getSetting("LaunchSiteName");
							newFacility.facilityPQS = ((CelestialBody) obj.getSetting("CelestialBody")).pqsController;
							newFacility.facilityTransformName = obj.gameObject.name;
							newFacility.pqsName = ((CelestialBody) obj.getSetting("CelestialBody")).pqsController.name;
							PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
							spawnPoint.name = (string) obj.getSetting("LaunchSiteName");
							spawnPoint.spawnTransformURL = (string) obj.getSetting("LaunchPadTransform");
							newFacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[1];
							newFacility.spawnPoints[0] = spawnPoint;
							PSystemSetup.SpaceCenterFacility[] newFacilities = new PSystemSetup.SpaceCenterFacility[facilities.Length + 1];
							for (int i = 0; i < facilities.Length; ++i)
							{
								newFacilities[i] = facilities[i];
							}
							newFacilities[newFacilities.Length - 1] = newFacility;
							fi.SetValue(PSystemSetup.Instance, newFacilities);
							facilities = newFacilities;
							

                            launchSites.Add(new LaunchSite(obj, newFacility));
						}
						else
						{
							Debug.Log("KK: Launch site " + obj.getSetting("LaunchSiteName") + " already exists.");
						}
					}
				}

				MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
				if (updateSitesMI == null)
					Debug.Log("KK: You are screwed. Failed to find SetupFacilities().");
				else
					updateSitesMI.Invoke(PSystemSetup.Instance, null);

				if (obj.gameObject != null)
					CustomSpaceCenter.CreateFromLaunchsite((string)obj.getSetting("LaunchSiteName"), obj.gameObject);
			}
			else
			{
				Debug.Log("KK: Launch pad transform \"" + obj.getSetting("LaunchPadTransform") + "\" missing for " + obj.getSetting("LaunchSiteName"));
			}
		}

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
					if (site.category.Equals(usedFilter))
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
				if (site.type.Equals(type) || (site.type.Equals(SiteType.Any) && allowAny))
				{
					if (appliedFilter.Equals("ALL"))
					{
						sites.Add(site);
					}
					else
					{
						if (site.category.Equals(appliedFilter))
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
				if (site.name == sSiteName)
				{
					site.openCloseState = sState;
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
				if (site.name == sSiteName)
				{
					site.openCloseState = site.openCloseState + "Locked";
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
				if (site.name == sSiteName)
				{
					if (site.openCloseState == "OpenLocked")
						site.openCloseState = "Open";
					else
						site.openCloseState = "Closed";
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
				if (site.name == sSiteName)
				{
					sOpenCloseState = site.openCloseState;
					fOpenCost = site.openCost;
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
				if (site.name == sSiteName)
				{
					if (site.openCloseState == "OpenLocked" || site.openCloseState == "ClosedLocked")
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
				if (site.name == sSiteName)
				{
					if ( (site.openCloseState == "Open") || (site.openCloseState == "OpenLocked") ) 
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
				if (site.name == sSiteName)
				{
					if (site.openCloseState == "Closed")
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
				if (site.name == sSiteName)
				{
					fRefund = site.launchRefund;
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
				if (site.name == sSiteName)
				{
					return site.gameObject;
				}
			}

			return null;
		}

		// Returns the StaticObject of a site. Can provide a sitename or a GameObject
		public static StaticObject getSiteStaticObject(string sSiteName, GameObject go = null)
		{
			StaticObject soSite = null;
			if (go != null)
			{
				soSite = StaticUtils.getStaticFromGameObject(go);

				if (soSite == null) return null;
				return soSite;
			}

			string sName = "";
			object oName = null;
			foreach (StaticObject obj in KerbalKonstructs.instance.staticDB.getAllStatics())
			{
				oName = obj.getSetting("LaunchSiteName");
				if (oName == null) continue;

				oName = null;

				sName = (string)obj.getSetting("LaunchSiteName");
				if (sName == sSiteName) return obj;
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
				if (site.name == sSiteName)
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
				if (site.name == sSiteName)
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
				site.openCloseState = "Closed";
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
					var radialposition = site.gameObject.transform.position;
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
				sOpenCloseState = site.openCloseState;

				if (!MiscUtils.isCareerGame())
					sOpenCloseState = "Open";

				if (sOpenCloseState == "Open")
				{
					var radialposition = site.gameObject.transform.position;
					var dist = Vector3.Distance(position, radialposition);

					if (site.name == "Runway")
					{
						if (lNearestBase == null)
							lNearestBase = site;
							lKSC = site;
					}
					else 
						if (site.name != "LaunchPad")
						{
							if ((float)dist < (float)smallestDist)
							{
								{
									sNearestBase = site.name;
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
				if (site.gameObject == null) continue;

				var radialposition = site.gameObject.transform.position;
				var dist = Vector3.Distance(position, radialposition);

				if (radialposition == position) continue;

				if (site.name == "Runway" || site.name == "LaunchPad")
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
						sNearestBase = site.name;
						smallestDist = dist;
						lTargetSite = site;
					}
					else if (dist < lastSmallestDist)
					{
						sLastNearest = site.name;
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
            lastLaunchSite = site.name;
			EditorLogic.fetch.launchSiteName = site.name;
		}

        // Returns the internal launchSite that KSP has been told is the launchsite
        public static string getCurrentLaunchSite()
		{
            return lastLaunchSite;

        }

		// Handy get of all launchSites
		public static List<LaunchSite> AllLaunchSites { get { return launchSites; } }
	}
}
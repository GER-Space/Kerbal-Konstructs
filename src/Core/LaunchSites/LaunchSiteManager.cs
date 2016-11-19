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
		public static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
		public static float RangeNearestOpenBase = 0f;
		public static string NearestOpenBase = "";
		public static float RangeNearestBase = 0f;
		public static string NearestBase = "";

		public static LaunchSite runway = new LaunchSite("Runway", "Squad", SiteType.SPH, 
			GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false), null, 
			"The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.", 
			"Runway", 0, 0, "Open", 285.37f, -0.09f, 69, 2500f, 75f, 0f, 100f, 0f, SpaceCenter.Instance.gameObject);
		public static LaunchSite launchpad = new LaunchSite("LaunchPad", "Squad", SiteType.VAB, 
			GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCLaunchpad", false), null, 
			"The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...", 
			"RocketPad", 0, 0, "Open", 285.37f, -0.09f, 72, 20f, 20f, 0f, 100f, 0f, SpaceCenter.Instance.gameObject);

		static LaunchSiteManager()
		{
			launchSites.Add(runway);
			launchSites.Add(launchpad);
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
							
							Texture logo = null;
							Texture icon = null;

							if (obj.settings.ContainsKey("LaunchSiteLogo"))
							{
								string sLogoPath = (string)obj.getSetting("LaunchSiteLogo");
								logo = GameDatabase.Instance.GetTexture(sLogoPath, false);
							
								if (logo == null)
									logo = GameDatabase.Instance.GetTexture(obj.model.path + "/" + obj.getSetting("LaunchSiteLogo"), false);
							}
							
							if (logo == null)
								logo = defaultLaunchSiteLogo;

							if(obj.settings.ContainsKey("LaunchSiteIcon"))
							{
								string sIconPath = (string)obj.getSetting("LaunchSiteIcon");
								icon = GameDatabase.Instance.GetTexture(sIconPath, false);

								if (icon == null)
									icon = GameDatabase.Instance.GetTexture(obj.model.path + "/" + obj.getSetting("LaunchSiteIcon"), false);
							}							
							
							// TODO This is still hard-code and needs to use an API properly
							launchSites.Add(new LaunchSite(
								(string)obj.getSetting("LaunchSiteName"), 
								(obj.settings.ContainsKey("LaunchSiteAuthor")) ? (string)obj.getSetting("LaunchSiteAuthor") : (string)obj.model.getSetting("author"), 
								(SiteType)obj.getSetting("LaunchSiteType"), 
								logo, 
								icon, 
								(string)obj.getSetting("LaunchSiteDescription"), 
								(string)obj.getSetting("Category"), 
								(float)obj.getSetting("OpenCost"), 
								(float)obj.getSetting("CloseValue"), 
								"Closed", 
								(float)obj.getSetting("RefLongitude"), 
								(float)obj.getSetting("RefLatitude"), 
								(float)obj.getSetting("RadiusOffset"), 
								(obj.settings.ContainsKey("LaunchSiteLength")) ? 
									(float)obj.getSetting("LaunchSiteLength") : (float)obj.model.getSetting("DefaultLaunchSiteLength"), 
								(obj.settings.ContainsKey("LaunchSiteWidth")) ? 
									(float)obj.getSetting("LaunchSiteWidth") : (float)obj.model.getSetting("DefaultLaunchSiteWidth"),
								(float)obj.getSetting("LaunchRefund"),
								(float)obj.getSetting("RecoveryFactor"),
								(float)obj.getSetting("RecoveryRange"),
								obj.gameObject, 
								newFacility,
								"No log",
								(string)obj.getSetting("LaunchSiteNation")
								));
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
					site.openclosestate = sState;
					PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
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
					site.openclosestate = site.openclosestate + "Locked";
					PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
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
					if (site.openclosestate == "OpenLocked")
						site.openclosestate = "Open";
					else
						site.openclosestate = "Closed";
					
					PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
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
					sOpenCloseState = site.openclosestate;
					fOpenCost = site.opencost;
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
					if (site.openclosestate == "OpenLocked" || site.openclosestate == "ClosedLocked")
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
					if (site.openclosestate == "Open")
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
					if (site.openclosestate == "Closed")
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
					fRefund = site.launchrefund;
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
					return site.GameObject;
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
				site.openclosestate = "Closed";
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
					var radialposition = site.GameObject.transform.position;
					var dist = Vector3.Distance(position, radialposition);
					flRange = dist;
				}
			}

			return flRange;
		}

		// Returns the nearest open Launchsite to a position and range to the Launchsite in m
		// The basic ATC feature is in here for now
		public static void getNearestOpenBase(Vector3 position, out string sBase, out float flRange, out LaunchSite lNearest)
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
				sOpenCloseState = site.openclosestate;

				if (!MiscUtils.isCareerGame())
					sOpenCloseState = "Open";

				if (sOpenCloseState == "Open")
				{
					var radialposition = site.GameObject.transform.position;
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

			RangeNearestOpenBase = (float)smallestDist;

			// Air traffic control messaging
			if (KerbalKonstructs.GUI_Landinguide.IsOpen())
			{
				if (sNearestBase != NearestOpenBase)
				{
					if (RangeNearestOpenBase < 25000)
					{
						NearestOpenBase = sNearestBase;
						MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.BLUE;
						MessageSystem.Message m = new MessageSystem.Message("KK ATC", "You have entered the airspace of " + sNearestBase + " ATC. Please keep this channel open and obey all signal lights. Thank you. " + sNearestBase + " Air Traffic Control out.", color, MessageSystemButton.ButtonIcons.MESSAGE);
						MessageSystem.Instance.AddMessage(m);
					}
					else
						if (NearestOpenBase != "")
						{
							// you have left ...
							MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.GREEN;
							MessageSystem.Message m = new MessageSystem.Message("KK ATC", "You are now leaving the airspace of " + sNearestBase + ". Safe journey. " + sNearestBase + " Air Traffic Control out.", color, MessageSystemButton.ButtonIcons.MESSAGE);
							MessageSystem.Instance.AddMessage(m);
							NearestOpenBase = "";
						}
				}
			}

			sBase = sNearestBase;
			flRange = RangeNearestOpenBase;
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
				if (site.GameObject == null) continue;

				var radialposition = site.GameObject.transform.position;
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

			RangeNearestBase = (float)smallestDist;

			sBase = sNearestBase;
			sBase2 = sLastNearest;
			flRange = RangeNearestBase;
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
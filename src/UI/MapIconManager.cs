using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class MapIconManager
	{
		public Texture VABIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/VABMapIcon", false);
		public Texture SPHIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SPHMapIcon", false);
		public Texture ANYIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/ANYMapIcon", false);
		public Texture TrackingStationIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/TrackingMapIcon", false);

		private Boolean displayingTooltip = false;
		
		static LaunchSite selectedSite = null;
		static StaticObject selectedFacility = null;

		public float iFundsOpen = 0;
		public float iFundsClose = 0;
		public Boolean isOpen = false;

		public float iFundsOpen2 = 0;
		public float iFundsClose2 = 0;
		public Boolean isOpen2 = false;
		public Boolean bChangeTargetType = false;
		public Boolean bChangeTarget = false;

		public Vector2 sitesScrollPosition;
		public Vector2 descriptionScrollPosition;

		Vector3 ObjectPos = new Vector3(0, 0, 0);

		public static LaunchSite getSelectedTarget()
		{
			LaunchSite thisSite = selectedSite;
			return thisSite;
		}

		public void displayMapIconToolTip(string sitename, Vector3 pos)
		{
			displayingTooltip = true;
			GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 200, 20), sitename);
		}

		public void drawIcons()
		{
			displayingTooltip = false;
			MapObject target = PlanetariumCamera.fetch.target;
			
			if (target.type == MapObject.MapObjectType.CELESTIALBODY)
			{
				// Do tracking stations first
				foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
				{
					bool display2 = false;
					if ((string)obj.getSetting("FacilityType") == "TrackingStation")
					{
						if (!isOccluded(obj.gameObject.transform.position, target.celestialBody))
						{
							if (MiscUtils.isCareerGame())
							{
								string openclosed2 = (string)obj.getSetting("OpenCloseState");
								// To do manage open and close state of tracking stations
								if (KerbalKonstructs.instance.mapShowOpenT) // && openclosed == "Open")
									display2 = true;
								if (!KerbalKonstructs.instance.mapShowClosed && openclosed2 == "Closed")
									display2 = false;
								if (!KerbalKonstructs.instance.mapShowOpen && openclosed2 == "Open")
									display2 = false;
							}
							else
							{ // Not a career game
							}

							if (display2)
							{
								Vector3 pos = MapView.MapCamera.camera.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(obj.gameObject.transform.position));
								Rect screenRect2 = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
								Graphics.DrawTexture(screenRect2, TrackingStationIcon);

								if (screenRect2.Contains(Event.current.mousePosition) && !displayingTooltip)
								{

									var objectpos2 = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(obj.gameObject.transform.position);
									var dObjectLat2 = NavUtils.GetLatitude(objectpos2);
									var dObjectLon2 = NavUtils.GetLongitude(objectpos2);
									var disObjectLat2 = dObjectLat2 * 180 / Math.PI;
									var disObjectLon2 = dObjectLon2 * 180 / Math.PI;

									if (disObjectLon2 < 0) disObjectLon2 = disObjectLon2 + 360;

									//Only display one tooltip at a time
									displayMapIconToolTip("Tracking Station " + "\n(Lat." + disObjectLat2.ToString("#0.00") + "/ Lon." + disObjectLon2.ToString("#0.00") + ")", pos);
									// TO DO Display Lat and Lon in tooltip too

									if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
									{
										//ScreenMessages.PostScreenMessage("Selected base is " + sToolTip + ".", 5f, ScreenMessageStyle.LOWER_CENTER);
										PersistenceUtils.loadStaticPersistence(obj);
										selectedSite = null;
										FacilityManager.setSelectedSite(null);
										selectedFacility = obj;
										FacilityManager.setSelectedFacility(obj);
										//EditorGUI.setTargetSite(selectedSite);
									}
								}
								else
								{ // Mouse is not over tooltip
								}
							}
							else
							{ // Filter set to not display
							}
						}
						else
						{ // is occluded
						}
					}
					else
					{ // Not a tracking station
					}
				} //end foreach

				// Then do launchsites
				List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
				foreach (LaunchSite site in sites)
				{
					bool display = false;
					PSystemSetup.SpaceCenterFacility facility = PSystemSetup.Instance.GetSpaceCenterFacility(site.name);
					if (facility != null)
					{
						PSystemSetup.SpaceCenterFacility.SpawnPoint sp = facility.GetSpawnPoint(site.name);
						if (sp != null)
						{
							if (facility.facilityPQS == target.celestialBody.pqsController)
							{
								if (!isOccluded(sp.GetSpawnPointTransform().position, target.celestialBody))
								{
									Vector3 pos = MapView.MapCamera.camera.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(sp.GetSpawnPointTransform().position));
									Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

									string openclosed = site.openclosestate;
									string category = site.category;

									if (KerbalKonstructs.instance.mapShowHelipads && category == "Helipad")
										display = true;
									if (KerbalKonstructs.instance.mapShowOther && category == "Other")
										display = true;
									if (KerbalKonstructs.instance.mapShowRocketbases && category == "RocketPad")
										display = true;
									if (KerbalKonstructs.instance.mapShowRunways && category == "Runway")
										display = true;

									if (display && MiscUtils.isCareerGame())
									{
										if (!KerbalKonstructs.instance.mapShowOpen && openclosed == "Open")
											display = false;
										if (!KerbalKonstructs.instance.mapShowClosed && openclosed == "Closed")
											display = false;
									}

									if (display)
									{
										if (site.icon != null)
										{
											Graphics.DrawTexture(screenRect, site.icon);
										}
										else
										{
											switch (site.type)
											{
												case SiteType.VAB:
													Graphics.DrawTexture(screenRect, VABIcon);
													break;
												case SiteType.SPH:
													Graphics.DrawTexture(screenRect, SPHIcon);
													break;
												default:
													Graphics.DrawTexture(screenRect, ANYIcon);
													break;
											}
										}

										// Tooltip
										if (screenRect.Contains(Event.current.mousePosition) && !displayingTooltip)
										{
											//Only display one tooltip at a time
											string sToolTip = "";
											sToolTip = site.name;
											if (site.name == "Runway") sToolTip = "KSC Runway";
											if (site.name == "LaunchPad") sToolTip = "KSC LaunchPad";
											displayMapIconToolTip(sToolTip, pos);

											// Select a base by clicking on the icon
											if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
											{
												ScreenMessages.PostScreenMessage("Selected base is " + sToolTip + ".", 5f, ScreenMessageStyle.LOWER_CENTER);
												selectedFacility = null;
												FacilityManager.setSelectedSite(site);
												selectedSite = site;
												FacilityManager.setSelectedFacility(null);
												NavGuidanceSystem.setTargetSite(selectedSite);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private bool isOccluded(Vector3d loc, CelestialBody body)
		{
			Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

			if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
				return false;

			return true;
		}
	}
}
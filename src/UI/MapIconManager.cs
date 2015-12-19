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
		public Texture VABIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/VABMapIcon", false);
		public Texture SPHIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SPHMapIcon", false);
		public Texture ANYIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ANYMapIcon", false);
		public Texture TrackingStationIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/TrackingMapIcon", false);

		public Texture2D tNormalButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonNormal", false);
		public Texture2D tHoverButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonHover", false);

		public Texture tOpenBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOn", false);
		public Texture tOpenBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOff", false);
		public Texture tClosedBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOn", false);
		public Texture tClosedBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOff", false);
		public Texture tHelipadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOn", false);
		public Texture tHelipadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOff", false);
		public Texture tRunwaysOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOn", false);
		public Texture tRunwaysOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOff", false);
		public Texture tTrackingOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOn", false);
		public Texture tTrackingOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOff", false);
		public Texture tLaunchpadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOn", false);
		public Texture tLaunchpadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOff", false);
		public Texture tOtherOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOn", false);
		public Texture tOtherOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOff", false);

		Rect mapManagerRect = new Rect(250, 40, 320, 70);

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
		public Boolean showBaseManager = false;

		public Vector2 sitesScrollPosition;
		public Vector2 descriptionScrollPosition;

		Vector3 ObjectPos = new Vector3(0, 0, 0);

		bool loadedPersistence = false;

		GUIStyle Yellowtext;
		GUIStyle TextAreaNoBorder;
		GUIStyle BoxNoBorder;
		GUIStyle ButtonKK;
		GUIStyle ButtonRed;

		GUIStyle navStyle = new GUIStyle();

		public MapIconManager()
		{
			navStyle.padding.left = 0;
			navStyle.padding.right = 0;
			navStyle.padding.top = 1;
			navStyle.padding.bottom = 3;
			navStyle.normal.background = null;
		}

		public static LaunchSite getSelectedSite()
		{
			LaunchSite thisSite = selectedSite;
			return thisSite;
		}

		public void displayMapIconToolTip(string sitename, Vector3 pos)
		{
			displayingTooltip = true;
			GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 200, 20), sitename);
		}

		public void drawManager()
		{
			mapManagerRect = GUI.Window(0xB00B2E7, mapManagerRect, drawMapManagerWindow, "", navStyle);
		}

		void drawMapManagerWindow(int windowID)
		{
			ButtonRed = new GUIStyle(GUI.skin.button);
			ButtonRed.normal.textColor = Color.red;
			ButtonRed.active.textColor = Color.red;
			ButtonRed.focused.textColor = Color.red;
			ButtonRed.hover.textColor = Color.red;

			ButtonKK = new GUIStyle(GUI.skin.button);
			ButtonKK.padding.left = 0;
			ButtonKK.padding.right = 0;
			ButtonKK.normal.background = tNormalButton;
			ButtonKK.hover.background = tHoverButton;

			Yellowtext = new GUIStyle(GUI.skin.box);
			Yellowtext.normal.textColor = Color.yellow;
			Yellowtext.normal.background = null;

			TextAreaNoBorder = new GUIStyle(GUI.skin.textArea);
			TextAreaNoBorder.normal.background = null;

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;

			if (!loadedPersistence && MiscUtils.isCareerGame())
			{
				PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
				{
					if ((string)obj.getSetting("FacilityType") == "TrackingStation")
						PersistenceUtils.loadStaticPersistence(obj);
				}

				loadedPersistence = true;
			}

			GUILayout.BeginHorizontal();
			GUILayout.Box(" ", BoxNoBorder, GUILayout.Height(34));
			
			GUI.enabled = (MiscUtils.isCareerGame());
			if (!MiscUtils.isCareerGame())
			{
				GUILayout.Button(tOpenBasesOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Button(tClosedBasesOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Box(" ", BoxNoBorder, GUILayout.Height(34));
				GUILayout.Button(tTrackingOff, GUILayout.Width(32), GUILayout.Height(32));
			}
			else
			{
				if (KerbalKonstructs.instance.mapShowOpen)
				{
					if (GUILayout.Button(new GUIContent(tOpenBasesOn, "Opened"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpen = false;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(tOpenBasesOff, "Opened"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpen = true;
				}

				if (!KerbalKonstructs.instance.disableDisplayClosed)
				{
					if (KerbalKonstructs.instance.mapShowClosed)
					{
						if (GUILayout.Button(new GUIContent(tClosedBasesOn, "Closed"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
							KerbalKonstructs.instance.mapShowClosed = false;
					}
					else
					{
						if (GUILayout.Button(new GUIContent(tClosedBasesOff, "Closed"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
							KerbalKonstructs.instance.mapShowClosed = true;
					}
				}

				GUILayout.Box(" ", BoxNoBorder, GUILayout.Height(34));
				if (KerbalKonstructs.instance.mapShowOpenT)
				{
					if (GUILayout.Button(new GUIContent(tTrackingOn, "Tracking Stations"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpenT = false;
				}
				else
				{

					if (GUILayout.Button(new GUIContent(tTrackingOff, "Tracking Stations"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpenT = true;
				}
			}
			GUI.enabled = true;

			GUILayout.Box(" ", BoxNoBorder, GUILayout.Height(34));

			if (KerbalKonstructs.instance.mapShowRocketbases)
			{
				if (GUILayout.Button(new GUIContent(tLaunchpadsOn, "Rocketpads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRocketbases = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(tLaunchpadsOff, "Rocketpads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRocketbases = true;
			}

			if (KerbalKonstructs.instance.mapShowHelipads)
			{
				if (GUILayout.Button(new GUIContent(tHelipadsOn, "Helipads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowHelipads = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(tHelipadsOff, "Helipads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowHelipads = true;
			}

			if (KerbalKonstructs.instance.mapShowRunways)
			{
				if (GUILayout.Button(new GUIContent(tRunwaysOn, "Runways"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRunways = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(tRunwaysOff, "Runways"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRunways = true;
			}

			if (KerbalKonstructs.instance.mapShowOther)
			{
				if (GUILayout.Button(new GUIContent(tOtherOn, "Other"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowOther = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(tOtherOff, "Other"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowOther = true;
			}

			GUILayout.Box(" ", BoxNoBorder, GUILayout.Height(34));

			if (GUILayout.Button("X", ButtonRed, GUILayout.Height(20), GUILayout.Width(20)))
			{
				loadedPersistence = false;
				KerbalKonstructs.instance.showMapIconManager = false;
			}

			GUILayout.EndHorizontal();

			if (GUI.tooltip != "")
			{
				var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
				GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 2, labelSize.y + 2), GUI.tooltip, BoxNoBorder);
			}
			
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
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
								//PersistenceUtils.loadStaticPersistence(obj);
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
									CelestialBody cPlanetoid = (CelestialBody)obj.getSetting("CelestialBody");

									var objectpos2 = cPlanetoid.transform.InverseTransformPoint(obj.gameObject.transform.position);
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
										//MiscUtils.HUDMessage("Selected base is " + sToolTip + ".", 5f, 3);
										Debug.Log("KK: Selected station in map");
										float sTrackAngle = (float)obj.getSetting("TrackingAngle");
										Debug.Log("KK: Before save load " + sTrackAngle.ToString());
										float sTrackRange = (float)obj.getSetting("TrackingShort");
										Debug.Log("KK: Before save load " + sTrackRange.ToString());
										
										//PersistenceUtils.saveStaticPersistence(obj);
										PersistenceUtils.loadStaticPersistence(obj);

										float sTrackAngle2 = (float)obj.getSetting("TrackingAngle");
										Debug.Log("KK: After save load " + sTrackAngle2.ToString());
										float sTrackRange2 = (float)obj.getSetting("TrackingShort");
										Debug.Log("KK: After save load " + sTrackRange2.ToString());
										
										selectedFacility = obj;
										FacilityManager.setSelectedFacility(obj);
										KerbalKonstructs.instance.showFacilityManager = true;
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
										if (KerbalKonstructs.instance.disableDisplayClosed && openclosed == "Closed")
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
												MiscUtils.HUDMessage("Selected base is " + sToolTip + ".", 5f, 3);
												BaseManager.setSelectedSite(site);
												selectedSite = site;
												NavGuidanceSystem.setTargetSite(selectedSite);
												KerbalKonstructs.instance.showBaseManager = true;
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
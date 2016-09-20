using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class MapIconManager
	{
		Rect mapManagerRect = new Rect(250, 40, 515, 75);

		private Boolean displayingTooltip = false;
		private Boolean displayingTooltip2 = false;
		
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
		public Boolean bHideOccluded = false;
		public Boolean bHideOccluded2 = false;

		public Vector2 sitesScrollPosition;
		public Vector2 descriptionScrollPosition;

		public int iRadarCounter;

		Vector3 ObjectPos = new Vector3(0, 0, 0);

		bool loadedPersistence = false;

		public MapIconManager()
		{
		}

		public static LaunchSite getSelectedSite()
		{
			LaunchSite thisSite = selectedSite;
			return thisSite;
		}

		public void displayMapIconToolTip(string sitename, Vector3 pos)
		{
			displayingTooltip = true;
			GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 210, 25), sitename);
		}

		public void drawManager()
		{
			mapManagerRect = GUI.Window(0xB00B2E7, mapManagerRect, drawMapManagerWindow, "", UIMain.navStyle);
		}

		void drawMapManagerWindow(int windowID)
		{
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
			GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
			
			GUI.enabled = (MiscUtils.isCareerGame());
			if (!MiscUtils.isCareerGame())
			{
				GUILayout.Button(UIMain.tOpenBasesOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Button(UIMain.tClosedBasesOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
				GUILayout.Button(UIMain.tTrackingOff, GUILayout.Width(32), GUILayout.Height(32));
			}
			else
			{
				if (KerbalKonstructs.instance.mapShowOpen)
				{
					if (GUILayout.Button(new GUIContent(UIMain.tOpenBasesOn, "Opened"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpen = false;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(UIMain.tOpenBasesOff, "Opened"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpen = true;
				}

				if (!KerbalKonstructs.instance.disableDisplayClosed)
				{
					if (KerbalKonstructs.instance.mapShowClosed)
					{
						if (GUILayout.Button(new GUIContent(UIMain.tClosedBasesOn, "Closed"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
							KerbalKonstructs.instance.mapShowClosed = false;
					}
					else
					{
						if (GUILayout.Button(new GUIContent(UIMain.tClosedBasesOff, "Closed"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
							KerbalKonstructs.instance.mapShowClosed = true;
					}
				}

				GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
				if (KerbalKonstructs.instance.mapShowOpenT)
				{
					if (GUILayout.Button(new GUIContent(UIMain.tTrackingOn, "Tracking Stations"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpenT = false;
				}
				else
				{

					if (GUILayout.Button(new GUIContent(UIMain.tTrackingOff, "Tracking Stations"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpenT = true;
				}
			}
			GUI.enabled = true;

			GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

			if (KerbalKonstructs.instance.mapShowRocketbases)
			{
				if (GUILayout.Button(new GUIContent(UIMain.tLaunchpadsOn, "Rocketpads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRocketbases = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(UIMain.tLaunchpadsOff, "Rocketpads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRocketbases = true;
			}

			if (KerbalKonstructs.instance.mapShowHelipads)
			{
				if (GUILayout.Button(new GUIContent(UIMain.tHelipadsOn, "Helipads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowHelipads = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(UIMain.tHelipadsOff, "Helipads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowHelipads = true;
			}

			if (KerbalKonstructs.instance.mapShowRunways)
			{
				if (GUILayout.Button(new GUIContent(UIMain.tRunwaysOn, "Runways"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRunways = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(UIMain.tRunwaysOff, "Runways"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRunways = true;
			}

			if (KerbalKonstructs.instance.mapShowOther)
			{
				if (GUILayout.Button(new GUIContent(UIMain.tOtherOn, "Other"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowOther = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(UIMain.tOtherOff, "Other"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowOther = true;
			}

			GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

			GUI.enabled = (MiscUtils.isCareerGame());
			if (!MiscUtils.isCareerGame())
			{
				GUILayout.Button(UIMain.tDownlinksOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Button(UIMain.tUplinksOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Button(UIMain.tRadarOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Button(UIMain.tGroundCommsOff, GUILayout.Width(32), GUILayout.Height(32));
			}
			else
			{
				if (KerbalKonstructs.instance.mapShowDownlinks)
				{
					if (GUILayout.Button(new GUIContent(UIMain.tDownlinksOn, "Downlinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowDownlinks = false;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(UIMain.tDownlinksOff, "Downlinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowDownlinks = true;
				}
				
				if (KerbalKonstructs.instance.mapShowUplinks)
				{
					if (GUILayout.Button(new GUIContent(UIMain.tUplinksOn, "Uplinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowUplinks = false;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(UIMain.tUplinksOff, "Uplinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowUplinks = true;
				}

				if (KerbalKonstructs.instance.mapShowRadar)
				{
					if (GUILayout.Button(new GUIContent(UIMain.tRadarOn, "Radar"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowRadar = false;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(UIMain.tRadarOff, "Radar"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowRadar = true;
				}

				if (KerbalKonstructs.instance.mapShowGroundComms)
				{
					if (GUILayout.Button(new GUIContent(UIMain.tGroundCommsOn, "Ground Comms"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowGroundComms = false;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(UIMain.tGroundCommsOff, "Ground Comms"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowGroundComms = true;
				}

			}
			GUI.enabled = true;

			GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

			if (KerbalKonstructs.instance.mapHideIconsBehindBody)
			{
				if (GUILayout.Button(new GUIContent(UIMain.tHideOn, "Occlude"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapHideIconsBehindBody = false;
			}
			else
			{
				if (GUILayout.Button(new GUIContent(UIMain.tHideOff, "Occlude"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapHideIconsBehindBody = true;
			}

			GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

			if (GUILayout.Button("X", UIMain.ButtonRed, GUILayout.Height(20), GUILayout.Width(20)))
			{
				loadedPersistence = false;
				KerbalKonstructs.instance.showMapIconManager = false;
			}

			GUILayout.EndHorizontal();

			if (GUI.tooltip != "")
			{
				var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
				GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 5, labelSize.y + 6), GUI.tooltip, UIMain.KKToolTip);
			}
			
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void drawIcons()
		{
			MapObject target = PlanetariumCamera.fetch.target;

			if (target.type != MapObject.ObjectType.CelestialBody) return;

			drawTrackingStations(target);
			drawLaunchsites(target);
		}

		public void drawGroundComms(StaticObject obj, LaunchSite lSite = null)
		{
			string Base = "";
			string Base2 = "";
			float Range = 0f;
			LaunchSite lBase = null;
			LaunchSite lBase2 = null;
			Vector3 pos = Vector3.zero;

			if (lSite != null)
			{
				GameObject golSite = lSite.GameObject;
				pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(golSite.transform.position));
				LaunchSiteManager.getNearestBase(golSite.transform.position, out Base, out Base2, out Range, out lBase, out lBase2);
			}

			if (obj != null)
			{
				pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(obj.gameObject.transform.position));
				LaunchSiteManager.getNearestBase(obj.gameObject.transform.position, out Base, out Base2, out Range, out lBase, out lBase2);
			}

			Vector3 vNeighbourPos = Vector3.zero;
			Vector3 vNeighbourPos2 = Vector3.zero;
			Vector3 vBasePos = Vector3.zero;
			Vector3 vBasePos2 = Vector3.zero;

			GameObject goNeighbour = null;
			GameObject goNeighbour2 = null;

			if (Base != "")
			{
				if (Base == "KSC")
				{
					goNeighbour = SpaceCenterManager.KSC.gameObject;
				}
				else
					goNeighbour = LaunchSiteManager.getSiteGameObject(Base);
			}

			if (Base2 != "")
			{

				if (Base2 == "KSC")
				{
					goNeighbour2 = SpaceCenterManager.KSC.gameObject;
				}
				else
					goNeighbour2 = LaunchSiteManager.getSiteGameObject(Base2);
			}

			if (goNeighbour != null)
			{
				vNeighbourPos = goNeighbour.transform.position;
				vBasePos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(vNeighbourPos));
			}

			if (goNeighbour2 != null)
			{
				vNeighbourPos2 = goNeighbour2.transform.position;
				vBasePos2 = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(vNeighbourPos2));
			}

			if (goNeighbour != null && vNeighbourPos != Vector3.zero && vBasePos != Vector3.zero)
			{
				NavUtils.CreateLineMaterial(1);

				GL.Begin(GL.LINES);
				NavUtils.lineMaterial1.SetPass(0);
				GL.Color(new Color(1f, 1f, 1f, 0.7f));
				GL.Vertex3(pos.x - Screen.width / 2, pos.y - Screen.height / 2, pos.z);
				GL.Vertex3(vBasePos.x - Screen.width / 2, vBasePos.y - Screen.height / 2, vBasePos.z);
				GL.End();
			}

			if (goNeighbour2 != null && vNeighbourPos2 != Vector3.zero && vBasePos2 != Vector3.zero)
			{
				NavUtils.CreateLineMaterial(2);

				GL.Begin(GL.LINES);
				NavUtils.lineMaterial2.SetPass(0);
				GL.Color(new Color(1f, 1f, 1f, 0.7f));
				GL.Vertex3(pos.x - Screen.width / 2, pos.y - Screen.height / 2, pos.z);
				GL.Vertex3(vBasePos2.x - Screen.width / 2, vBasePos2.y - Screen.height / 2, vBasePos2.z);
				GL.End();
			}
		}

		public void drawTrackingStations(MapObject target)
		{
			displayingTooltip2 = false;

			// Do tracking stations first
			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if (!MiscUtils.isCareerGame()) break;

				bool display2 = false;
				string openclosed3 = "Closed";

				if ((string)obj.getSetting("FacilityType") != "TrackingStation")
					continue;

				if (isOccluded(obj.gameObject.transform.position, target.celestialBody))
				{
					if (KerbalKonstructs.instance.mapHideIconsBehindBody)
						continue;
				}

				openclosed3 = (string)obj.getSetting("OpenCloseState");

				if ((float)obj.getSetting("OpenCost") == 0) openclosed3 = "Open";

				if (KerbalKonstructs.instance.mapShowOpenT)
					display2 = true;
				if (!KerbalKonstructs.instance.mapShowClosed && openclosed3 == "Closed")
					display2 = false;
				if (!KerbalKonstructs.instance.mapShowOpen && openclosed3 == "Open")
					display2 = false;

				if (!display2)
					continue;

				Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(obj.gameObject.transform.position));
				Rect screenRect6 = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
				// Distance between camera and spawnpoint sort of
				float fPosZ = pos.z;
				float fRadarRadius = 12800 / fPosZ;
				
				if (fRadarRadius > 15) Graphics.DrawTexture(screenRect6, UIMain.TrackingStationIcon);

				string sTarget = (string)obj.getSetting("TargetID");
				float fStRange = (float)obj.getSetting("TrackingShort");
				float fStAngle = (float)obj.getSetting("TrackingAngle");

				if (openclosed3 == "Open" && KerbalKonstructs.instance.mapShowGroundComms)
					drawGroundComms(obj);

				if ((string)obj.getSetting("TargetType") == "Craft" && sTarget != "None")
				{
					Vessel vTargetVessel = TrackingStationGUI.GetTargetVessel(sTarget);
					if (vTargetVessel == null)
					{ }
					else
					{
						if (vTargetVessel.state == Vessel.State.DEAD)
						{ }
						else
						{
							CelestialBody cbTStation = (CelestialBody)obj.getSetting("CelestialBody");
							CelestialBody cbTCraft = vTargetVessel.mainBody;

							if (cbTStation == cbTCraft && openclosed3 == "Open" && KerbalKonstructs.instance.mapShowUplinks)
							{
								Vector3 vCraftPos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(vTargetVessel.gameObject.transform.position));

								float fRangeToTarget = TrackingStationGUI.GetRangeToCraft(obj, vTargetVessel);
								int iUplink = TrackingStationGUI.GetUplinkQuality(fStRange, fRangeToTarget);
								float fUplink = (float)iUplink / 100;

								float fRed = 1f;
								float fGreen = 0f;
								float fBlue = fUplink;
								float fAlpha = 1f;

								if (iUplink > 45)
								{
									fRed = 1f;
									fGreen = 0.65f + (fUplink / 10);
									fBlue = 0f;
								}

								if (iUplink > 85)
								{
									fRed = 0f;
									fGreen = fUplink;
									fBlue = 0f;
								}

								float fStationLOS = TrackingStationGUI.StationHasLOS(obj, vTargetVessel);

								if (fStationLOS > fStAngle)
								{
									fRed = 1f;
									fGreen = 0f;
									fBlue = 0f;
									fAlpha = 0.5f;
								}

								NavUtils.CreateLineMaterial(3);

								GL.Begin(GL.LINES);
								NavUtils.lineMaterial3.SetPass(0);
								GL.Color(new Color(fRed, fGreen, fBlue, fAlpha));
								GL.Vertex3(pos.x - Screen.width / 2, pos.y - Screen.height / 2, pos.z);
								GL.Vertex3(vCraftPos.x - Screen.width / 2, vCraftPos.y - Screen.height / 2, vCraftPos.z);
								GL.End();
							}
						}
					}
				}

				if (screenRect6.Contains(Event.current.mousePosition) && !displayingTooltip2)
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

					if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
					{
						float sTrackAngle = (float)obj.getSetting("TrackingAngle");
						float sTrackRange = (float)obj.getSetting("TrackingShort");

						PersistenceUtils.loadStaticPersistence(obj);

						float sTrackAngle2 = (float)obj.getSetting("TrackingAngle");
						float sTrackRange2 = (float)obj.getSetting("TrackingShort");

						selectedFacility = obj;
						FacilityManager.setSelectedFacility(obj);
						KerbalKonstructs.instance.showFacilityManager = true;
					}
				}
			}
		}

		public void drawRadar(Vector3 pos, string category, string openclosed)
		{
			if (openclosed != "Open") return;

			float fPosZ = pos.z;
			float fRadarRadius = 12800 / fPosZ;
			float fRadarOffset = fRadarRadius / 2;

			int iPulseRate = 180;

			Rect screenRect2 = new Rect((pos.x - fRadarOffset), (Screen.height - pos.y) - fRadarOffset, fRadarRadius, fRadarRadius);
			Rect screenRect3 = new Rect((pos.x - (fRadarOffset / 2)), (Screen.height - pos.y) - (fRadarOffset / 2), fRadarRadius / 2, fRadarRadius / 2);
			Rect screenRect4 = new Rect((pos.x - (fRadarOffset / 3)), (Screen.height - pos.y) - (fRadarOffset / 3), fRadarRadius / 3, fRadarRadius / 3);
			Rect screenRect5 = new Rect((pos.x - (fRadarOffset / 4)), (Screen.height - pos.y) - (fRadarOffset / 4), fRadarRadius / 4, fRadarRadius / 4);

			if (fRadarRadius > 15)
			{
				if (category == "Runway")
				{
					if (iRadarCounter > iPulseRate / 2)
						Graphics.DrawTexture(screenRect2, UIMain.tRadarCover);
					if (iRadarCounter > iPulseRate / 3)
						Graphics.DrawTexture(screenRect3, UIMain.tRadarCover);
					if (iRadarCounter > iPulseRate / 4)
						Graphics.DrawTexture(screenRect4, UIMain.tRadarCover);
					if (iRadarCounter > iPulseRate / 5)
						Graphics.DrawTexture(screenRect5, UIMain.tRadarCover);
				}

				if (category == "Helipad")
				{
					if (iRadarCounter > iPulseRate / 2)
						Graphics.DrawTexture(screenRect3, UIMain.tRadarCover);
					if (iRadarCounter > iPulseRate / 3)
						Graphics.DrawTexture(screenRect4, UIMain.tRadarCover);
					if (iRadarCounter > iPulseRate / 4)
						Graphics.DrawTexture(screenRect5, UIMain.tRadarCover);
				}
			}
		}

		public void drawLaunchsites(MapObject target)
		{
            displayingTooltip = false;
			int iPulseRate = 180;

			iRadarCounter = iRadarCounter + 1;
			if (iRadarCounter > iPulseRate)
                iRadarCounter = 0;

            // Then do launchsites
			List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            for (int index = 0; index < sites.Count; index++)
            {
                LaunchSite site = sites[index];

                PSystemSetup.SpaceCenterFacility facility = PSystemSetup.Instance.GetSpaceCenterFacility(site.name);

                if(facility == null)
					continue;

                PSystemSetup.SpaceCenterFacility.SpawnPoint sp = facility.GetSpawnPoint(site.name);

                if (sp == null)
					continue;

                if (facility.facilityPQS != target.celestialBody.pqsController)
					continue;

                Transform spawnPointTransform = sp.GetSpawnPointTransform();
                if (spawnPointTransform == null)
                    continue;

                if (isOccluded(spawnPointTransform.position, target.celestialBody))
				{
                    if(KerbalKonstructs.instance.mapHideIconsBehindBody)
                        continue;
				}

                Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(spawnPointTransform.position));
				Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;

				float fRadarRadius = 12800 / fPosZ;
				float fRadarOffset = fRadarRadius / 2;

				string openclosed = site.openclosestate;
				string category = site.category;

				bool display = true;

                if(!KerbalKonstructs.instance.mapShowHelipads && category == "Helipad")
					display = false;
				if (!KerbalKonstructs.instance.mapShowOther && category == "Other")
					display = false;
				if (!KerbalKonstructs.instance.mapShowRocketbases && category == "RocketPad")
					display = false;
				if (!KerbalKonstructs.instance.mapShowRunways && category == "Runway")
					display = false;

                if (display && MiscUtils.isCareerGame())
				{
					if (!KerbalKonstructs.instance.mapShowOpen && openclosed == "Open")
						display = false;
					if (!KerbalKonstructs.instance.mapShowClosed && openclosed == "Closed")
						display = false;
					if (KerbalKonstructs.instance.disableDisplayClosed && openclosed == "Closed")
						display = false;
					if (openclosed == "OpenLocked" || openclosed == "ClosedLocked")
						display = false;
				}

                if (!display)
                    continue;

				if (KerbalKonstructs.instance.mapShowRadar)
					drawRadar(pos, category, openclosed);

                if (openclosed == "Open" && KerbalKonstructs.instance.mapShowGroundComms)
				{
					drawGroundComms(null, site);
				}

                if (site.icon != null)
				{
					if (fRadarRadius > 15)
						Graphics.DrawTexture(screenRect, site.icon);
				}
				else
				{
                    if (fRadarRadius > 15)
					{
						switch (site.type)
						{
							case SiteType.VAB:
								Graphics.DrawTexture(screenRect, UIMain.VABIcon);
								break;
							case SiteType.SPH:
								Graphics.DrawTexture(screenRect, UIMain.SPHIcon);
								break;
							default:
								Graphics.DrawTexture(screenRect, UIMain.ANYIcon);
								break;
						}
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

		private bool isOccluded(Vector3d loc, CelestialBody body)
		{
			Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

			if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
				return false;

			return true;
		}
	}
}
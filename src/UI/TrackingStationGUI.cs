using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class TrackingStationGUI
	{
		public static string sTargetType = "None";
		public static string sTarget = "None";
		public static string sSelectedTrackingTarget = "None";
		public static string sDisplayTarget = "None";
		public static string sDisplayRange = "0m";

		public static StaticObject selectedStation = null;

		public static float fRange = 0f;
		public static float fMaxAngle = 45f;
		public static float StationLOS = 0f;

		public static Boolean bChangeTargetType = false;

		public static Vector2 scrollPos;

		public static GUIStyle LabelInfo;
		public static GUIStyle BoxInfo;
		public static GUIStyle ButtonSmallText;

		public static string sButtonText = "";

		public static float fAlt = 0f;

		public static CelestialBody cPlanetoid = null;

		public static Vector3 ObjectPos = new Vector3(0, 0, 0);
		public static Double dObjectLat = 0;
		public static Double dObjectLon = 0;
		public static Double disObjectLat = 0;
		public static Double disObjectLon = 0;

		public static Boolean bGUIenabled = false;

		public static void TrackingInterface(StaticObject soStation)
		{
			fRange = (float)soStation.getSetting("TrackingShort");
			sTargetType = (string)soStation.getSetting("TargetType");
			sTarget = (string)soStation.getSetting("TargetID");
			fMaxAngle = (float)soStation.getSetting("TrackingAngle");

			LabelInfo = new GUIStyle(GUI.skin.label);
			LabelInfo.normal.background = null;
			LabelInfo.normal.textColor = Color.white;
			LabelInfo.fontSize = 13;
			LabelInfo.fontStyle = FontStyle.Bold;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

			BoxInfo = new GUIStyle(GUI.skin.box);
			BoxInfo.normal.textColor = Color.cyan;
			BoxInfo.fontSize = 13;
			BoxInfo.padding.top = 2;
			BoxInfo.padding.bottom = 1;
			BoxInfo.padding.left = 5;
			BoxInfo.padding.right = 5;
			BoxInfo.normal.background = null;

			GUILayout.Space(2);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(5);
				GUILayout.Label("Short Range: " + (fRange / 1000f).ToString("#0") + "km", LabelInfo, GUILayout.Height(25));
				GUILayout.FlexibleSpace();
				GUILayout.Label("Max Angle: " + fMaxAngle.ToString("#0") + "°", LabelInfo, GUILayout.Height(25));

				if (KerbalKonstructs.instance.DevMode)
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Upgrade", GUILayout.Height(20), GUILayout.Width(70)))
					{
						ScreenMessages.PostScreenMessage("Unable to upgrade this facility. Insufficient materials.", 10, ScreenMessageStyle.LOWER_CENTER);
					}
				}
			}
			GUILayout.EndHorizontal();

			if (KerbalKonstructs.instance.DevMode)
			{
				GUILayout.Space(1);
				GUILayout.Box("To upgrade a kerbonaut Scientist and Engineer must be at the station. Then processed ore can be ordered for delivery to the station.", BoxInfo);
			}
			
			GUILayout.Space(2);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(5);
				GUILayout.Label("Target Type: " + sTargetType, LabelInfo, GUILayout.Height(25));
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Change", GUILayout.Height(20), GUILayout.Width(70)))
				{
					bChangeTargetType = true;
					sTarget = "None";

					if (sTargetType == "None") sTargetType = "Craft";
					else
						if (sTargetType == "Craft") sTargetType = "Celestial Body";
						else
							if (sTargetType == "Celestial Body") sTargetType = "Asteroid";
							else
								if (sTargetType == "Asteroid") sTargetType = "Station's Discretion";
								else
									if (sTargetType == "Station's Discretion") sTargetType = "None";

					FacilityManager.changeTargetType(sTargetType);

					soStation.setSetting("TargetID", sTarget);
					soStation.setSetting("TargetType", sTargetType);
					PersistenceUtils.saveStaticPersistence(soStation);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(5);

				int iU = sTarget.IndexOf("(");
				if (iU < 2) iU = sTarget.Length + 1;

				sDisplayTarget = sTarget.Substring(0, iU - 1);

				GUILayout.Label("Tracking: " + sDisplayTarget + " ", LabelInfo, GUILayout.Height(25));
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Change", GUILayout.Height(20), GUILayout.Width(70)))
				{
					if (sTargetType == "None" || sTargetType == "Station's Discretion")
						ScreenMessages.PostScreenMessage("Please select a target type first.", 10, ScreenMessageStyle.LOWER_CENTER);
					else
					{
						sSelectedTrackingTarget = "None";
						FacilityManager.changeTarget(true);
					}
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(5);

				GUILayout.Label("Status: ", LabelInfo, GUILayout.Height(25));
				string sStationStatus = "Offline";

				if (sTarget != "None")
				{
					sStationStatus = "Error. Please reset";

					if (sTargetType == "Craft")
					{
						Vessel vTargetVessel = GetTargetVessel(sTarget);

						if (vTargetVessel == null) sStationStatus = "Cannot find craft";
						else
						{
							if (vTargetVessel.state == Vessel.State.DEAD) sStationStatus = "Cannot find craft";
							else
							{
								StationLOS = StationHasLOS(soStation, vTargetVessel);

								if (StationLOS <= fMaxAngle)
								{
									sDisplayRange = "0m";
									float fRangeToTarget = GetRangeToCraft(soStation, vTargetVessel);

									if (fRangeToTarget > 100000f)
										sDisplayRange = (fRangeToTarget / 1000f).ToString("#0") + " km";
									else
										sDisplayRange = fRangeToTarget.ToString("#0") + "m";

									if (fRangeToTarget > fRange) sStationStatus = "Lock " + StationLOS.ToString("#0") 
										+ "° " + sDisplayRange;
									else
										sStationStatus = "Lock " + StationLOS.ToString("#0") + "° " + sDisplayRange;
								}
								else sStationStatus = "No lock";
							}
						}
					}

					if (sTargetType == "Asteroid")
					{
						Vessel vTargetVessel = GetTargetVessel(sTarget);

						bool bAsteroidDetected = true;
						if (vTargetVessel == null) bAsteroidDetected = false;
						else
						{
							if (vTargetVessel.state == Vessel.State.DEAD) bAsteroidDetected = false;
						}

						if (!bAsteroidDetected) sStationStatus = "Cannot find asteroid";
						else
						{
							StationLOS = StationHasLOS(soStation, vTargetVessel);

							vTargetVessel.DiscoveryInfo.SetLastObservedTime(Planetarium.GetUniversalTime());
							vTargetVessel.DiscoveryInfo.SetLevel((DiscoveryLevels)29);

							if (StationLOS <= fMaxAngle)
							{
								sDisplayRange = "0m";
								float fRangeToTarget = GetRangeToCraft(soStation, vTargetVessel);

								if (fRangeToTarget > 100000f)
									sDisplayRange = (fRangeToTarget / 1000f).ToString("#0") + " km";
								else
									sDisplayRange = fRangeToTarget.ToString("#0") + "m";

								if (fRangeToTarget > fRange) sStationStatus = "Lock " + StationLOS.ToString("#0") + "° " + sDisplayRange;
								else
									sStationStatus = "Lock " + StationLOS.ToString("#0") + "° " + sDisplayRange;
							}
							else sStationStatus = "No lock";
						}
					}

					if (sTargetType == "Celestial Body")
					{
						CelestialBody cTargetPlanet = GetTargetPlanet(sTarget);

						if (cTargetPlanet == null) sStationStatus = "Cannot find body";
						else
						{
							StationLOS = StationHasLOStoPlanet(soStation, cTargetPlanet);

							if (StationLOS <= fMaxAngle)
							{
								sDisplayRange = "0m";
								float fRangeToTarget = GetRangeToPlanet(soStation, cTargetPlanet);

								sDisplayRange = (fRangeToTarget / 1000f).ToString("#0") + "km";
								sStationStatus = "Sighted " + StationLOS.ToString("#0") + "° " + sDisplayRange;
							}
							else sStationStatus = "No sighting";
						}
					}
				}

				if (sTargetType == "Station's Discretion") sStationStatus = " Busy ";

				GUILayout.Label(sStationStatus, LabelInfo, GUILayout.Height(30));
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Stop", GUILayout.Height(20), GUILayout.Width(70)))
				{
					sTargetType = "None";
					sTarget = "None";
					soStation.setSetting("TargetType", sTargetType);
					soStation.setSetting("TargetID", sTarget);
					PersistenceUtils.saveStaticPersistence(soStation);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
		}

		public static void TargetSelector(string sTargetTypeSelected, StaticObject selectedFacility = null)
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			{
				if (sTargetTypeSelected == "Station")
				{
					sSelectedTrackingTarget = "None";

					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						if ((string)obj.getSetting("FacilityType") == "TrackingStation")
						{
							if ((string)obj.getSetting("OpenCloseState") == "Closed") continue;

							GUILayout.BeginHorizontal();
							{
								fAlt = (float)obj.getSetting("RadiusOffset");

								cPlanetoid = (CelestialBody)obj.getSetting("CelestialBody");

								ObjectPos = cPlanetoid.transform.InverseTransformPoint(obj.gameObject.transform.position);
								dObjectLat = NavUtils.GetLatitude(ObjectPos);
								dObjectLon = NavUtils.GetLongitude(ObjectPos);
								disObjectLat = dObjectLat * 180 / Math.PI;
								disObjectLon = dObjectLon * 180 / Math.PI;

								if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

								sButtonText = cPlanetoid.name + " Station\nAltitude: " + fAlt.ToString("#0") + "m\nLat. "
									+ disObjectLat.ToString("#0.000") + " Lon. " + disObjectLon.ToString("#0.000");

								GUI.enabled = (obj != selectedStation);
								//GUILayout.Box(sButtonText, GUILayout.Height(50));

								ButtonSmallText = new GUIStyle(GUI.skin.button);
								ButtonSmallText.fontSize = 12;
								ButtonSmallText.fontStyle = FontStyle.Normal;

								if (GUILayout.Button("" + sButtonText, ButtonSmallText, GUILayout.Height(55)))
								{
									selectedStation = obj;
								}

								GUI.enabled = true;
							}
							GUILayout.EndHorizontal();
						}
					}
				}
				
				if (sTargetTypeSelected == "Craft")
				{
					selectedStation = null;

					foreach (Vessel vVessel in FlightGlobals.Vessels)
					{
						if (vVessel.vesselType == VesselType.SpaceObject) continue;
						if (vVessel.vesselType == VesselType.Debris) continue;
						if (vVessel.vesselType == VesselType.EVA) continue;
						if (vVessel.vesselType == VesselType.Flag) continue;
						if (vVessel.vesselType == VesselType.Unknown) continue;
						if (vVessel == FlightGlobals.ActiveVessel) continue;

						int iU = vVessel.name.IndexOf("(");
						if (iU < 2) iU = vVessel.name.Length + 1;
						string sDisplayTarget = vVessel.name.Substring(0, iU - 1);

						GUI.enabled = (sSelectedTrackingTarget != vVessel.name + "_" + vVessel.id.ToString());
						if (GUILayout.Button(sDisplayTarget, GUILayout.Height(20))) sSelectedTrackingTarget = vVessel.name + "_" + vVessel.id.ToString();
						GUI.enabled = true;
					}
				}

				if (sTargetTypeSelected == "Celestial Body")
				{
					foreach (CelestialBody cBody in FlightGlobals.Bodies)
					{
						GUI.enabled = (sSelectedTrackingTarget != cBody.name);
						if (GUILayout.Button(cBody.name, GUILayout.Height(20))) sSelectedTrackingTarget = cBody.name;
						GUI.enabled = true;
					}
				}

				if (sTargetTypeSelected == "Asteroid")
				{
					foreach (Vessel vVessel in FlightGlobals.Vessels)
					{
						if (vVessel.vesselType != VesselType.SpaceObject && vVessel.vesselType != VesselType.Unknown) continue;

						int iU = vVessel.name.IndexOf("(");
						if (iU < 2) iU = vVessel.name.Length + 1;
						string sDisplayTarget = vVessel.name.Substring(0, iU - 1);

						GUI.enabled = (sSelectedTrackingTarget != vVessel.name + "_" + vVessel.id.ToString());
						if (GUILayout.Button(sDisplayTarget, GUILayout.Height(20))) sSelectedTrackingTarget = vVessel.name + "_" + vVessel.id.ToString();
						GUI.enabled = true;
					}
				}

				if (sTargetTypeSelected == "Station's Discretion")
				{
					GUILayout.Box("Target is selected by the station.");
				}

				if (sTargetTypeSelected == "None")
				{
					GUILayout.Box("Select a target type.");
				}
			}
			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			{
				if (sSelectedTrackingTarget != "None") bGUIenabled = true;
				if (selectedStation != null) bGUIenabled = true;

				GUI.enabled = bGUIenabled;
				if (GUILayout.Button("Select", GUILayout.Height(25)))
				{
					sTarget = sSelectedTrackingTarget;
					if (selectedFacility != null)
					{
						selectedFacility.setSetting("TargetID", sTarget);
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}
					else
					{
						if (selectedStation != null)
						{
							DownlinkGUI.soTargetStation = selectedStation;
							var FacilityKey = selectedStation.getSetting("RadialPosition");
							DownlinkGUI.sStationRadial = FacilityKey.ToString();
						}
						else
						{
							DownlinkGUI.sTarget = sTarget;
						}
						
						DownlinkGUI.SaveCommsState();
					}
				}
				GUI.enabled = true;

				if (GUILayout.Button("Close", GUILayout.Height(25)))
				{
					if (selectedFacility != null)
						FacilityManager.changeTarget(false);
					else
						DownlinkGUI.changeTarget(false);
				}
			}
			GUILayout.EndHorizontal();
		}

		public static float GetRangeToCraft(StaticObject soFacility, Vessel vVessel)
		{
			float fReturnRange = 0f;
			fReturnRange = Vector3.Distance(soFacility.gameObject.transform.position, vVessel.gameObject.transform.position);
			return fReturnRange;
		}

		public static float GetRangeToPlanet(StaticObject soFacility, CelestialBody cPlanet)
		{
			var fReturnRange = 0f;
			fReturnRange = Vector3.Distance(soFacility.gameObject.transform.position, cPlanet.gameObject.transform.position);
			return fReturnRange;
		}

		public static float StationHasLOS(StaticObject soFacility, Vessel vVessel)
		{
			if (vVessel == null || soFacility == null)
			{
				Debug.Log("KK: StationHasLOS borked");
				return 0f;
			}

			float fHasLOS = 0f;
			Vector3d FacPos = soFacility.gameObject.transform.position;
			Vector3d VesselPos = vVessel.gameObject.transform.position;

			Vector3d vDirection = (VesselPos - FacPos);

			float fAngle = Vector3.Angle(soFacility.gameObject.transform.up, vDirection);
			fHasLOS = fAngle;

			return fHasLOS;
		}

		public static float StationHasLOStoPlanet(StaticObject soFacility, CelestialBody cPlanet)
		{
			float bHasLOS = 0f;
			Vector3d FacPos = soFacility.gameObject.transform.position;
			Vector3d cPlanetPos = cPlanet.gameObject.transform.position;

			Vector3d vDirection = (cPlanetPos - FacPos);

			float fAngle = Vector3.Angle(soFacility.gameObject.transform.up, vDirection);
			bHasLOS = fAngle;

			return bHasLOS;
		}

		public static Vessel GetTargetVessel(string sVessel)
		{
			Vessel vReturnVessel = null;

			foreach (Vessel vVessel in FlightGlobals.Vessels)
			{
				string sVesselID = vVessel.name + "_" + vVessel.id.ToString();
				if (sVesselID == sVessel) vReturnVessel = vVessel;
			}

			return vReturnVessel;
		}

		public static CelestialBody GetTargetPlanet(string sPlanet)
		{
			CelestialBody vReturnPlanet = null;

			foreach (CelestialBody cPlanet in FlightGlobals.Bodies)
			{
				string sPlanetID = cPlanet.name;
				if (sPlanetID == sPlanet) vReturnPlanet = cPlanet;
			}

			return vReturnPlanet;
		}

	}
}

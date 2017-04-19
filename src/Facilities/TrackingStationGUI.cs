using KerbalKonstructs.Core;
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
		public static string sUplink = "None";
		public static string sFacLvl = "Lvl 1/3";
		public static string sGroup = "Ungrouped";

		public static StaticObject selectedStation = null;

		public static float fRange = 0f;
		public static float fMaxAngle = 45f;
		public static float StationLOS = 0f;
		public static float fTSRange = 90000f;
		public static float fTSAngle = 45f;

		public static Boolean bChangeTargetType = false;

		public static Vector2 scrollPos;

		public static GUIStyle LabelInfo;
		public static GUIStyle BoxInfo;
		public static GUIStyle ButtonSmallText;

		public static string sButtonText = "";

		public static float fAlt = 0f;

		public static CelestialBody cPlanetoid = null;

		public static Vector3 ObjectPos = new Vector3(0, 0, 0);

		public static Double disObjectLat = 0;
		public static Double disObjectLon = 0;

		public static Boolean bGUIenabled = false;
		public static Boolean bCraftLock = false;

		public static Boolean bNotInit = false;

		public static void TrackingInterface(StaticObject soStation)
		{
			fRange = (float)soStation.getSetting("TrackingShort");
			sTargetType = (string)soStation.getSetting("TargetType");
			sTarget = (string)soStation.getSetting("TargetID");
			fMaxAngle = (float)soStation.getSetting("TrackingAngle");
			sGroup = (string)soStation.getSetting("Group");
			bCraftLock = false;
			
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
				GUILayout.Label("Range: " + (fRange / 1000f).ToString("#0") + "km", LabelInfo, GUILayout.Height(25));			}
			GUILayout.EndHorizontal();

			/* if (KerbalKonstructs.instance.DevMode)
			{
				GUILayout.Space(1);
				GUILayout.Box("To upgrade a kerbonaut Scientist and Engineer must be at the station. Then processed ore can be ordered for delivery to the station.", BoxInfo);
			} */
			
			GUILayout.Space(2);



			GUILayout.BeginHorizontal();
			
			GUILayout.EndHorizontal();
			if (sTargetType == "Craft" && bCraftLock)
			{
				GUILayout.Label("Uplink Quality:" + sUplink, LabelInfo);
			}
			GUILayout.Space(5);
		}

		public static int GetUplinkQuality(float flRange, float fRangeToTarget)
		{
			int iUplink = 95;
			if (fRangeToTarget > (flRange * 2)) iUplink = 90;
			if (fRangeToTarget > (flRange * 5)) iUplink = 85;
			if (fRangeToTarget > (flRange * 10)) iUplink = 75;
			if (fRangeToTarget > (flRange * 100)) iUplink = 50;
			if (fRangeToTarget > (flRange * 1000)) iUplink = 25;
			if (fRangeToTarget > (flRange * 10000)) iUplink = 10;
			if (fRangeToTarget > (flRange * 100000)) iUplink = 5;
			if (fRangeToTarget > (flRange * 1000000)) iUplink = 1;
			return iUplink;
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

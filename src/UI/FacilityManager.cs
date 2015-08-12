using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class FacilityManager
	{
		Rect targetSelectorRect = new Rect(640, 120, 200, 400);
		public static Rect facilityManagerRect = new Rect(150, 75, 300, 500);

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/horizontalsep3", false);

		public Vector2 descriptionScrollPosition;
		
		public static LaunchSite selectedSite = null;
		public static StaticObject selectedFacility = null;

		float fLqFMax = 0;
		float fOxFMax = 0;
		float fMoFMax = 0;

		public float iFundsOpen = 0;
		public float iFundsClose = 0;
		public float iFundsOpen2 = 0;
		public float iFundsClose2 = 0;
		float fAlt = 0f;

		public Boolean isOpen = false;
		public Boolean isOpen2 = false;
		public Boolean bChangeTargetType = false;
		public static Boolean bChangeTarget = false;

		string sFacilityName = "Unknown";
		string sFacilityType = "Unknown";
		public static string sTargetType = "None";

		Vector3 ObjectPos = new Vector3(0, 0, 0);

		double dObjectLat = 0;
		double dObjectLon = 0;
		double disObjectLat = 0;
		double disObjectLon = 0;

		GUIStyle Yellowtext;
		GUIStyle KKWindow;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle BoxNoBorder;
		GUIStyle LabelInfo;

		void drawTargetSelector(int windowID)
		{
			TrackingStationGUI.TargetSelector(sTargetType, selectedFacility);
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void drawFacilityManager(StaticObject soObject)
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3, 3, 5, 5);

			facilityManagerRect = GUI.Window(0xB01B2B5, facilityManagerRect, drawFacilityManagerWindow, "", KKWindow);
		
			if (bChangeTarget)
				targetSelectorRect = GUI.Window(0xB71B2A1, targetSelectorRect, drawTargetSelector, "Select Target");
		}

		void drawFacilityManagerWindow(int windowID)
		{
			DeadButton = new GUIStyle(GUI.skin.button);
			DeadButton.normal.background = null;
			DeadButton.hover.background = null;
			DeadButton.active.background = null;
			DeadButton.focused.background = null;
			DeadButton.normal.textColor = Color.white;
			DeadButton.hover.textColor = Color.white;
			DeadButton.active.textColor = Color.white;
			DeadButton.focused.textColor = Color.white;
			DeadButton.fontSize = 14;
			DeadButton.fontStyle = FontStyle.Bold;

			DeadButtonRed = new GUIStyle(GUI.skin.button);
			DeadButtonRed.normal.background = null;
			DeadButtonRed.hover.background = null;
			DeadButtonRed.active.background = null;
			DeadButtonRed.focused.background = null;
			DeadButtonRed.normal.textColor = Color.red;
			DeadButtonRed.hover.textColor = Color.yellow;
			DeadButtonRed.active.textColor = Color.red;
			DeadButtonRed.focused.textColor = Color.red;
			DeadButtonRed.fontSize = 12;
			DeadButtonRed.fontStyle = FontStyle.Bold;

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

			Yellowtext = new GUIStyle(GUI.skin.box);
			Yellowtext.normal.textColor = Color.yellow;
			Yellowtext.normal.background = null;

			LabelInfo = new GUIStyle(GUI.skin.label);
			LabelInfo.normal.background = null;
			LabelInfo.normal.textColor = Color.white;
			LabelInfo.fontSize = 13;
			LabelInfo.fontStyle = FontStyle.Bold;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Facility Manager", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
				{
					PersistenceUtils.saveStaticPersistence(selectedFacility);
					selectedFacility = null;
					KerbalKonstructs.instance.showFacilityManager = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			if (selectedFacility != null)
			{
				sFacilityType = (string)selectedFacility.getSetting("FacilityType");

				if (sFacilityType == "TrackingStation") sFacilityName = "Tracking Station";
				else
					sFacilityName = (string)selectedFacility.model.getSetting("title");

				GUILayout.Box("" + sFacilityName, Yellowtext);
				GUILayout.Space(5);

				fAlt = (float)selectedFacility.getSetting("RadiusOffset");

				ObjectPos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(selectedFacility.gameObject.transform.position);
				dObjectLat = NavUtils.GetLatitude(ObjectPos);
				dObjectLon = NavUtils.GetLongitude(ObjectPos);
				disObjectLat = dObjectLat * 180 / Math.PI;
				disObjectLon = dObjectLon * 180 / Math.PI;

				if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(5);
					GUILayout.Label("Alt. " + fAlt.ToString("#0.0") + "m", LabelInfo);
					GUILayout.FlexibleSpace();
					GUILayout.Label("Lat. " + disObjectLat.ToString("#0.000"), LabelInfo);
					GUILayout.FlexibleSpace();
					GUILayout.Label("Lon. " + disObjectLon.ToString("#0.000"), LabelInfo);
					GUILayout.Space(5);
				}
				GUILayout.EndHorizontal();
				
				GUILayout.Space(5);

				if (sFacilityType == "TrackingStation")
				{
					SharedInterfaces.OpenCloseFacility(selectedFacility);

					iFundsOpen2 = (float)selectedFacility.getSetting("OpenCost");
					isOpen2 = ((string)selectedFacility.getSetting("OpenCloseState") == "Open");

					if (iFundsOpen2 == 0) isOpen2 = true;

					GUI.enabled = isOpen2;

					GUILayout.Space(2);
					GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
					GUILayout.Space(3);

					TrackingStationGUI.TrackingInterface(selectedFacility);

					GUILayout.Space(2);
					GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
					GUILayout.Space(2);

					StaffGUI.StaffingInterface(selectedFacility);

					GUI.enabled = true;
				}
				else
				{
					fLqFMax = (float)selectedFacility.model.getSetting("LqFMax");
					fOxFMax = (float)selectedFacility.model.getSetting("OxFMax");
					fMoFMax = (float)selectedFacility.model.getSetting("MoFMax");

					if (fLqFMax > 0 || fOxFMax > 0 || fMoFMax > 0)
					{
						GUILayout.Space(2);
						GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
						GUILayout.Space(3);

						FuelTanksGUI.FuelTanksInterface(selectedFacility);
					}

					GUILayout.Space(2);
					GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
					GUILayout.Space(2);

					StaffGUI.StaffingInterface(selectedFacility);
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(3);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static void setSelectedFacility(StaticObject oFacility)
		{
			selectedFacility = oFacility;
		}

		public static void changeTarget(Boolean bChange)
		{
			bChangeTarget = bChange;
		}

		public static void changeTargetType(string sType)
		{
			sTargetType = sType;
		}
	}
}

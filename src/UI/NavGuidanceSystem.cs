using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.UI
{
	class NavGuidanceSystem
	{
		Rect NGSRect = new Rect(250, 50, 395, 110);

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

		public bool bClosing = false;
		public int iCorrection = 3;

		public string sTargetSiteName = "NO TARGET";

		public float fRangeToTarget = 0f;
		float fOldRange = 0f;

		private Vector3 vCrft;
		private Vector3 vSPos;
		private Vector3 vHead;

		private Double disLat;
		private Double disLon;
		private Double disBaseLat;
		private Double disBaseLon;
		private double dshipheading;
		private double dreqheading;

		static LaunchSite lTargetSite = null;

		public Texture tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
		public Texture tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
		public Texture tLeftOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lefton", false);
		public Texture tLeftOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/leftoff", false);
		public Texture tRightOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/righton", false);
		public Texture tRightOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/rightoff", false);
		public Texture tTextureLeft = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/leftoff", false);
		public Texture tTextureRight = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/rightoff", false);
		public Texture tTextureMiddle = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);

		GUIStyle navStyle = new GUIStyle();
		GUIStyle BoxNoBorder;
		GUIStyle DeadButtonRed;

		public NavGuidanceSystem()
		{
			navStyle.padding.left = 0;
			navStyle.padding.right = 0;
			navStyle.padding.top = 1;
			navStyle.padding.bottom = 3;
			navStyle.normal.background = null;
		}

		public void drawNGS()
		{
			if (KerbalKonstructs.instance.showNGS)
			{
				NGSRect = GUI.Window(0xB00B1E9, NGSRect, drawNGSWindow, "", navStyle);
			}
		}

		public static void setTargetSite(LaunchSite lsTarget, string sName = "")
		{
			lTargetSite = lsTarget;
		}

		public static LaunchSite getTargetSite()
		{
			return lTargetSite;
		}

		void prepNGS()
		{
			if (lTargetSite != null)
			{
				sTargetSiteName = lTargetSite.name;

				fRangeToTarget = LaunchSiteManager.getDistanceToBase(FlightGlobals.ActiveVessel.GetTransform().position, lTargetSite);
				if (fRangeToTarget > fOldRange) bClosing = false;
				if (fRangeToTarget < fOldRange) bClosing = true;

				var basepos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(lTargetSite.GameObject.transform.position);
				var dBaseLat = NavUtils.GetLatitude(basepos);
				var dBaseLon = NavUtils.GetLongitude(basepos);
				disBaseLat = dBaseLat * 180 / Math.PI;
				disBaseLon = dBaseLon * 180 / Math.PI;

				fOldRange = fRangeToTarget;

				if (bClosing)
				{
					tTextureMiddle = tIconOpen;
				}
				else
				{
					tTextureMiddle = tIconClosed;
				}

				Vector3 vcraftpos = FlightGlobals.ActiveVessel.GetTransform().position;
				vCrft = vcraftpos;
				Vector3 vsitepos = lTargetSite.GameObject.transform.position;
				vSPos = vsitepos;
				Vector3 vHeading = FlightGlobals.ActiveVessel.transform.up;
				vHead = vHeading;

				disLat = FlightGlobals.ActiveVessel.latitude;
				var dLat = disLat / 180 * Math.PI;
				disLon = FlightGlobals.ActiveVessel.longitude;
				var dLon = disLon / 180 * Math.PI;

				var y = Math.Sin(dBaseLon - dLon) * Math.Cos(dBaseLat);
				var x = (Math.Cos(dLat) * Math.Sin(dBaseLat)) - (Math.Sin(dLat) * Math.Cos(dBaseLat) * Math.Cos(dBaseLon - dLon));
				var requiredHeading = Math.Atan2(y, x) * 180 / Math.PI;
				dreqheading = (requiredHeading + 360) % 360;

				var diff = (360 + 180 + requiredHeading - FlightGlobals.ship_heading) % 360 - 180;
				dshipheading = (FlightGlobals.ship_heading + 360) % 360;

				if (diff > 5)
					iCorrection = 2;
				else if (diff < -5)
					iCorrection = 1;
				else
					iCorrection = 0;

				if (bClosing)
				{
					tTextureLeft = tLeftOff;
					tTextureRight = tRightOff;
				}
				else
				{
					tTextureLeft = tLeftOn;
					tTextureRight = tRightOn;
				}

				if (iCorrection == 1)
				{
					tTextureLeft = tLeftOn;
					tTextureRight = tRightOff;
				}
				if (iCorrection == 2)
				{
					tTextureLeft = tLeftOff;
					tTextureRight = tRightOn;
				}
			}
			else
			{
				tTextureMiddle = tIconClosed;
				tTextureLeft = tLeftOff;
				tTextureRight = tRightOff;
			}
		}

		void drawNGSWindow(int windowID)
		{
			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

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

			GUILayout.BeginHorizontal();
			{
				string sTargetSiteAlias = "";
				sTargetSiteAlias = sTargetSiteName;
				if (sTargetSiteName == "Runway") sTargetSiteAlias = "KSC Runway";
				if (sTargetSiteName == "LaunchPad") sTargetSiteAlias = "KSC LaunchPad";

				GUILayout.Box("-KK-", BoxNoBorder, GUILayout.Width(32), GUILayout.Height(16));
				GUILayout.Space(2);
				GUILayout.Box("NGS: " + sTargetSiteAlias, GUILayout.Height(16));
				GUILayout.Space(2);
				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Width(32), GUILayout.Height(16)))
				{
					KerbalKonstructs.instance.enableNGS = false;
					KerbalKonstructs.instance.showNGS = false;
				}
				GUILayout.Space(2);
			}
			GUILayout.EndHorizontal();

			if (fRangeToTarget < 10000)
				GUILayout.Box(fRangeToTarget.ToString("#0.0") + " m", GUILayout.Height(17));
			else
				GUILayout.Box((fRangeToTarget / 1000).ToString("#0.0") + " km", GUILayout.Height(17));

			GUILayout.BeginHorizontal();
			{
				GUILayout.Box(tTextureLeft, BoxNoBorder, GUILayout.Height(25), GUILayout.Width(165));
				GUILayout.FlexibleSpace();
				GUILayout.Box(tTextureMiddle, BoxNoBorder, GUILayout.Height(25), GUILayout.Width(25));
				GUILayout.FlexibleSpace();
				GUILayout.Box(tTextureRight, BoxNoBorder, GUILayout.Height(25), GUILayout.Width(165));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Box("CRFT", GUILayout.Width(60), GUILayout.Height(18));
				GUILayout.Box("Lat.", GUILayout.Height(18));
				GUILayout.Box(disLat.ToString("#0.00"), GUILayout.Width(60), GUILayout.Height(18));
				GUILayout.Box("Lon.", GUILayout.Height(18));
				
				if (disLon < 0) disLon = disLon + 360;
				
				GUILayout.Box(disLon.ToString("#0.00"), GUILayout.Width(60), GUILayout.Height(18));
				GUILayout.Box("Head", GUILayout.Height(18));
				GUILayout.Box(dshipheading.ToString("#0"), GUILayout.Width(35), GUILayout.Height(18));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Box("BASE", GUILayout.Width(60), GUILayout.Height(18));
				GUILayout.Box("Lat.", GUILayout.Height(18));
				GUILayout.Box(disBaseLat.ToString("#0.00"), GUILayout.Width(60), GUILayout.Height(18));
				GUILayout.Box("Lon.", GUILayout.Height(18));
				
				if (disBaseLon < 0) disBaseLon = disBaseLon + 360;
				
				GUILayout.Box(disBaseLon.ToString("#0.00"), GUILayout.Width(60), GUILayout.Height(18));
				GUILayout.Box("Head", GUILayout.Height(18));
				GUILayout.Box(dreqheading.ToString("#0"), GUILayout.Width(35), GUILayout.Height(18));
			}
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(1);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));

			prepNGS();
		}
	}
}

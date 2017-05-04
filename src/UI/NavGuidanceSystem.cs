using KerbalKonstructs.Core;
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
	class NavGuidanceSystem :KKWindow
	{
		Rect NGSRect = new Rect(250, 50, 400, 120);

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

		public Texture tTextureLeft = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/leftoff", false);
		public Texture tTextureRight = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/rightoff", false);
		public Texture tTextureMiddle = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);

		public NavGuidanceSystem()
		{
		}

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }
            drawNGS();
        }

        public void drawNGS()
		{
				NGSRect = GUI.Window(0xB00B1E9, NGSRect, drawNGSWindow, "", UIMain.navStyle);
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

				var basepos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(lTargetSite.gameObject.transform.position);
				var dBaseLat = NavUtils.GetLatitude(basepos);
				var dBaseLon = NavUtils.GetLongitude(basepos);
				disBaseLat = dBaseLat * 180 / Math.PI;
				disBaseLon = dBaseLon * 180 / Math.PI;

				fOldRange = fRangeToTarget;

				if (bClosing)
				{
					tTextureMiddle = UIMain.tIconOpen;
				}
				else
				{
					tTextureMiddle = UIMain.tIconClosed;
				}

				Vector3 vcraftpos = FlightGlobals.ActiveVessel.GetTransform().position;
				vCrft = vcraftpos;
				Vector3 vsitepos = lTargetSite.gameObject.transform.position;
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
					tTextureLeft = UIMain.tLeftOff;
					tTextureRight = UIMain.tRightOff;
				}
				else
				{
					tTextureLeft = UIMain.tLeftOn;
					tTextureRight = UIMain.tRightOn;
				}

				if (iCorrection == 1)
				{
					tTextureLeft = UIMain.tLeftOn;
					tTextureRight = UIMain.tRightOff;
				}
				if (iCorrection == 2)
				{
					tTextureLeft = UIMain.tLeftOff;
					tTextureRight = UIMain.tRightOn;
				}
			}
			else
			{
				tTextureMiddle = UIMain.tIconClosed;
				tTextureLeft = UIMain.tLeftOff;
				tTextureRight = UIMain.tRightOff;
			}
		}

		void drawNGSWindow(int windowID)
		{
			GUILayout.BeginHorizontal();
			{
				string sTargetSiteAlias = "";
				sTargetSiteAlias = sTargetSiteName;
				if (sTargetSiteName == "Runway") sTargetSiteAlias = "KSC Runway";
				if (sTargetSiteName == "LaunchPad") sTargetSiteAlias = "KSC LaunchPad";

				GUILayout.Box("-KK-", UIMain.BoxNoBorderW, GUILayout.Width(32), GUILayout.Height(16));
				GUILayout.Space(2);
				GUILayout.Box("NGS: " + sTargetSiteAlias, GUILayout.Height(16));
				GUILayout.Space(2);
				if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Width(32), GUILayout.Height(16)))
				{
                    this.Close();
                    return;
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
				GUILayout.Box(tTextureLeft, UIMain.BoxNoBorderW, GUILayout.Height(25), GUILayout.Width(165));
				GUILayout.FlexibleSpace();
				GUILayout.Box(tTextureMiddle, UIMain.BoxNoBorderW, GUILayout.Height(25), GUILayout.Width(25));
				GUILayout.FlexibleSpace();
				GUILayout.Box(tTextureRight, UIMain.BoxNoBorderW, GUILayout.Height(25), GUILayout.Width(165));
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
			GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorderW, GUILayout.Height(4));
			GUILayout.Space(1);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));

			prepNGS();
		}
	}
}

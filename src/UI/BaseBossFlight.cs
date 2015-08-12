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
	class BaseBossFlight
	{
		public StaticObject selectedObject = null;

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/horizontalsep3", false);
		public Texture tIconClosed = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteclosed", false);
		public Texture tIconOpen = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);
		public Texture tToggle = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);
		public Texture tToggle2 = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);
		
		Rect managerRect = new Rect(10, 25, 300, 500);
		Rect facilityRect = new Rect(150, 75, 400, 620);
		Rect targetSelectorRect = new Rect(450, 150, 250, 550);
		Rect downlickRect = new Rect(300, 50, 140, 350);
		
		public Boolean managingFacility = false;
		public Boolean foundingBase = false;
		
		public static Boolean bChangeTarget = false;
		
		public static string sTargetType = "None";

		Vector2 scrollPos;

		GUIStyle KKWindow;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle BoxNoBorder;
		GUIStyle LabelInfo;

		public void drawManager(StaticObject obj)
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3, 3, 5, 5);

			if (obj != null)
			{
				if (selectedObject != obj)
					EditorGUI.updateSelection(obj);
			}

			managerRect = GUI.Window(0xB00B1E2, managerRect, drawBaseManagerWindow, "", KKWindow);
		}

		void drawBaseManagerWindow(int windowID)
		{
			string Base;
			float Range;
			LaunchSite lNearest;
			LaunchSite lBase;
			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

			LabelInfo = new GUIStyle(GUI.skin.label);
			LabelInfo.normal.background = null;
			LabelInfo.normal.textColor = Color.white;
			LabelInfo.fontSize = 13;
			LabelInfo.fontStyle = FontStyle.Bold;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

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

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Inflight Base Boss", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
				{
					KerbalKonstructs.instance.showFlightManager = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(5);
			GUILayout.Box("Flight Tools", BoxNoBorder);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(2);
				GUILayout.Label("ATC ", LabelInfo);

				if (KerbalKonstructs.instance.enableATC)
					tToggle = tIconOpen;
				else
					tToggle = tIconClosed;

				if (GUILayout.Button(tToggle, GUILayout.Height(18), GUILayout.Width(18)))
				{
					if (KerbalKonstructs.instance.enableATC)
						KerbalKonstructs.instance.enableATC = false;
					else
						KerbalKonstructs.instance.enableATC = true;
				}

				KerbalKonstructs.instance.showATC = (KerbalKonstructs.instance.enableATC);

				GUILayout.FlexibleSpace();
				GUILayout.Label("NGS ", LabelInfo);

				if (KerbalKonstructs.instance.enableNGS)
					tToggle2 = tIconOpen;
				else
					tToggle2 = tIconClosed;

				if (GUILayout.Button(tToggle2, GUILayout.Height(18), GUILayout.Width(18)))
				{
					if (KerbalKonstructs.instance.enableNGS)
						KerbalKonstructs.instance.enableNGS = false;
					else
						KerbalKonstructs.instance.enableNGS = true;
				}

				KerbalKonstructs.instance.showNGS = (KerbalKonstructs.instance.enableNGS);

				GUILayout.FlexibleSpace();
				GUILayout.Label("Downlink ", LabelInfo);

				if (KerbalKonstructs.instance.enableDownlink)
					tToggle2 = tIconOpen;
				else
					tToggle2 = tIconClosed;

				if (GUILayout.Button(tToggle2, GUILayout.Height(18), GUILayout.Width(18)))
				{
					if (KerbalKonstructs.instance.enableDownlink)
						KerbalKonstructs.instance.enableDownlink = false;
					else
						KerbalKonstructs.instance.enableDownlink = true;
				}

				KerbalKonstructs.instance.showDownlink = (KerbalKonstructs.instance.enableDownlink);

				GUILayout.Space(2);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(6);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(2);

			GUILayout.Box("Base Proximity", BoxNoBorder);

			if (MiscUtils.isCareerGame())
			{
				GUILayout.BeginHorizontal();
				{
					string snearestopen = "";
					LaunchSiteManager.getNearestOpenBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range, out lNearest);
					if (Range < 10000)
						snearestopen = Base + " at " + Range.ToString("#0.0") + " m";
					else
						snearestopen = Base + " at " + (Range / 1000).ToString("#0.0") + " km";

					GUILayout.Space(5);
					GUILayout.Label("Nearest Open: ", LabelInfo);
					GUILayout.Label(snearestopen, LabelInfo, GUILayout.Width(150));
					
					if (KerbalKonstructs.instance.enableNGS)
					{
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("NGS", GUILayout.Height(21)))
						{
							NavGuidanceSystem.setTargetSite(lNearest);
							smessage = "NGS set to " + Base;
							ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
						}
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(2);
			}

			GUILayout.BeginHorizontal();
			{
				string sNearestbase = "";
				LaunchSiteManager.getNearestBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range, out lBase);

				if (Range < 10000)
					sNearestbase = Base + " at " + Range.ToString("#0.0") + " m";
				else
					sNearestbase = Base + " at " + (Range / 1000).ToString("#0.0") + " km";

				GUILayout.Space(5);
				GUILayout.Label("Nearest Base: ", LabelInfo);
				GUILayout.Label(sNearestbase, LabelInfo, GUILayout.Width(150));

				if (KerbalKonstructs.instance.enableNGS)
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("NGS", GUILayout.Height(21)))
					{
						NavGuidanceSystem.setTargetSite(lBase);

						smessage = "NGS set to " + Base;
						ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
					}
				}
			}
			GUILayout.EndHorizontal();

			if (MiscUtils.isCareerGame())
			{
				bool bLanded = (FlightGlobals.ActiveVessel.Landed);

				if (Range < 2000)
				{
					string sClosed;
					float fOpenCost;
					LaunchSiteManager.getSiteOpenCloseState(Base, out sClosed, out fOpenCost);
					fOpenCost = fOpenCost / 2f;

					if (bLanded && sClosed == "Closed")
					{
						GUILayout.Space(2);
						GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
						GUILayout.Space(2);
						if (GUILayout.Button("Open Base for " + fOpenCost + " funds", GUILayout.Height(23)))
						{
							double currentfunds = Funding.Instance.Funds;

							if (fOpenCost > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this site!", 10, 0);
							}
							else
							{
								Funding.Instance.AddFunds(-fOpenCost, TransactionReasons.Cheating);

								LaunchSiteManager.setSiteOpenCloseState(Base, "Open");
								smessage = Base + " opened";
								ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
							}
						}
					}

					if (bLanded && sClosed == "Open")
					{
						GUI.enabled = false;
						GUILayout.Button("Base is Open", GUILayout.Height(23));
						GUI.enabled = true;
					}

					GUILayout.Space(2);
					GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
					GUILayout.Space(2);
				}

				if (Range > 100000)
				{
					if (bLanded)
					{
						GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
						GUILayout.Space(2);
						if (GUILayout.Button("Found a New Base", GUILayout.Height(23)))
						{
							foundingBase = true;
						}
						GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
						GUILayout.Space(2);
					}
				}
			}

			if (FlightGlobals.ActiveVessel.Landed)
			{
				GUILayout.Box("Nearby Facilities", BoxNoBorder);

				scrollPos = GUILayout.BeginScrollView(scrollPos);
				{
					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						bool isLocal = true;
						if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
						{
							var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
							isLocal = dist < 2000f;
						}
						else
							isLocal = false;

						if ((string)obj.model.getSetting("DefaultFacilityType") == "None")
						{

						}

						if (isLocal)
						{
							if (GUILayout.Button((string)obj.model.getSetting("title"), GUILayout.Height(23)))
							{
								selectedObject = obj;
								KerbalKonstructs.instance.selectObject(obj, false);
								PersistenceUtils.loadStaticPersistence(obj);
								FacilityManager.setSelectedFacility(obj);
								KerbalKonstructs.instance.showFacilityManager = true;
							}
						}
					}
				}
				GUILayout.EndScrollView();
			}

			GUILayout.Space(2);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(2);
			GUILayout.FlexibleSpace();
			GUILayout.Space(3);
			if (GUILayout.Button("I want to race!", GUILayout.Height(23)))
			{
				KerbalKonstructs.instance.showRacingApp = true;
				AirRacing.runningRace = true;
				KerbalKonstructs.instance.showNGS = false;
				KerbalKonstructs.instance.showFlightManager = false;
			}
			GUILayout.Space(5);

			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(2);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
	}
}

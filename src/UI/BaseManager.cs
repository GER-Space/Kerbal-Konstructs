using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class BaseManager
	{
		public static Rect BaseManagerRect = new Rect(250, 60, 165, 680);

		public Texture tTitleIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/titlebaricon", false);
		public Texture tSmallClose = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/littleclose", false);
		public Texture tStatusLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/setaslaunchsite", false);
		public Texture tSetLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/setaslaunchsite", false);
		public Texture tOpenedLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/openedlaunchsite", false);
		public Texture tClosedLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/closedlaunchsite", false);
		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep", false);
		public Texture tMakeFavourite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/makefavourite", false);
		public Texture tVerticalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/verticalsep", false);
		public Texture tFaveTemp = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/makefavourite", false);
		public Texture tIsFave = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/isFavourite", false);
		public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
		public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
		public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
		
		public static LaunchSite selectedSite = null;

		GUIStyle Yellowtext;
		GUIStyle TextAreaNoBorder;
		GUIStyle KKWindow;
		GUIStyle BoxNoBorder;
		GUIStyle SmallButton;
		GUIStyle LabelWhite;
		GUIStyle KKWindowTitle;
		GUIStyle LabelInfo;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;

		Vector2 descriptionScrollPosition;
		Vector2 logScrollPosition;

		public float iFundsOpen = 0;
		public float iFundsClose = 0;
		public float rangekm = 0;

		bool loadedPersistence = false;

		public Boolean isOpen = false;
		public Boolean isFavourite = false;
		public Boolean displayStats = true;
		public Boolean displayLog = false;
		public Boolean foldedIn = false;
		public Boolean doneFold = false;

		public void drawBaseManager()
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3,3,5,5);

			if (foldedIn)
			{
				if (!doneFold)
					BaseManagerRect = new Rect(BaseManagerRect.xMin, BaseManagerRect.yMin, BaseManagerRect.width, BaseManagerRect.height - 255);
				
				doneFold = true;
			}

			if (!foldedIn)
			{
				if (doneFold)
					BaseManagerRect = new Rect(BaseManagerRect.xMin, BaseManagerRect.yMin, BaseManagerRect.width, BaseManagerRect.height + 255);
				
				doneFold = false;
			}

			BaseManagerRect = GUI.Window(0xC00B8B7, BaseManagerRect, drawBaseManagerWindow, "", KKWindow);

			if (BaseManagerRect.Contains(Event.current.mousePosition))
			{
				InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, "KKEditorLock");
			}
			else
			{
				InputLockManager.RemoveControlLock("KKEditorLock");
			}
		}

		public void drawBaseManagerWindow(int windowID)
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

			Yellowtext = new GUIStyle(GUI.skin.box);
			Yellowtext.normal.textColor = Color.yellow;
			Yellowtext.normal.background = null;

			TextAreaNoBorder = new GUIStyle(GUI.skin.textArea);
			TextAreaNoBorder.normal.background = null;
			TextAreaNoBorder.normal.textColor = Color.white;
			TextAreaNoBorder.fontSize = 12;
			TextAreaNoBorder.padding.left = 1;
			TextAreaNoBorder.padding.right = 1;
			TextAreaNoBorder.padding.top = 4;

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

			LabelWhite = new GUIStyle(GUI.skin.label);
			LabelWhite.normal.background = null;
			LabelWhite.normal.textColor = Color.white;
			LabelWhite.fontSize = 12;
			LabelWhite.padding.left = 1;
			LabelWhite.padding.right = 1;
			LabelWhite.padding.top = 4;

			LabelInfo = new GUIStyle(GUI.skin.label);
			LabelInfo.normal.background = null;
			LabelInfo.normal.textColor = Color.white;
			LabelInfo.fontSize = 13;
			LabelInfo.fontStyle = FontStyle.Bold;
			LabelInfo.padding.left = 3;
			LabelInfo.padding.top = 0;
			LabelInfo.padding.bottom = 0;

			KKWindowTitle = new GUIStyle(GUI.skin.box);
			KKWindowTitle.normal.background = null;
			KKWindowTitle.normal.textColor = Color.white;
			KKWindowTitle.fontSize = 14;
			KKWindowTitle.fontStyle = FontStyle.Bold;

			SmallButton = new GUIStyle(GUI.skin.button);
			SmallButton.normal.textColor = Color.red;
			SmallButton.hover.textColor = Color.white;
			SmallButton.padding.top = 1;
			SmallButton.padding.left = 1;
			SmallButton.padding.right = 1;
			SmallButton.padding.bottom = 4;
			SmallButton.normal.background = null;
			SmallButton.hover.background = null;
			SmallButton.fontSize = 12;

			if (!loadedPersistence && MiscUtils.isCareerGame())
			{
				PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				loadedPersistence = true;
			}

			string sButtonName = "";
			sButtonName = selectedSite.name;
			if (selectedSite.name == "Runway") sButtonName = "KSC Runway";
			if (selectedSite.name == "LaunchPad") sButtonName = "KSC LaunchPad";

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Base Manager", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (HighLogic.LoadedScene != GameScenes.EDITOR)
				{
					if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
					{
						InputLockManager.RemoveControlLock("KKEditorLock");
						selectedSite = null;
						loadedPersistence = false;
						KerbalKonstructs.instance.showBaseManager = false;
					}
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			if (selectedSite.name == "Runway")
				GUILayout.Box("KSC Runway", Yellowtext);
			else
				if (selectedSite.name == "LaunchPad")
					GUILayout.Box("KSC LaunchPad", Yellowtext);
				else
					GUILayout.Box("" + selectedSite.name, Yellowtext);

			if (!foldedIn)
			{
				GUILayout.Space(5);

				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(2);
					GUILayout.Box(tVerticalSep, BoxNoBorder, GUILayout.Width(4), GUILayout.Height(135));
					GUILayout.FlexibleSpace();
					GUILayout.Box(selectedSite.logo, BoxNoBorder, GUILayout.Height(135), GUILayout.Width(135));
					GUILayout.FlexibleSpace();
					GUILayout.Box(tVerticalSep, BoxNoBorder, GUILayout.Width(4), GUILayout.Height(135));
					GUILayout.Space(2);
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(3);

				descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition, GUILayout.Height(120));
				{
					GUI.enabled = false;
					GUILayout.Label(selectedSite.description, LabelWhite);
					GUI.enabled = true;
				}
				GUILayout.EndScrollView();
			}

			GUILayout.Space(1);

			isFavourite = (selectedSite.favouritesite == "Yes");

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = (!displayStats);
				if(GUILayout.Button("Stats", GUILayout.Height(23)))
				{
					displayLog = false;
					displayStats = true;
				}
				GUI.enabled = true;

				GUI.enabled = (!displayLog);
				if(GUILayout.Button("Log", GUILayout.Height(23)))
				{
					displayLog = true;
					displayStats = false;
				}
				GUI.enabled = true;

				if (isFavourite) 
					tFaveTemp = tIsFave;
				else 
					tFaveTemp = tMakeFavourite;
				
				if (GUILayout.Button(tFaveTemp, GUILayout.Height(23), GUILayout.Width(23)))
				{
					if (isFavourite)
						selectedSite.favouritesite = "No";							
					else
						selectedSite.favouritesite = "Yes";

					List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
					PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
				}

				if (foldedIn) tFolded = tFoldOut;
				if (!foldedIn) tFolded = tFoldIn;

				if (GUILayout.Button(tFolded, GUILayout.Height(23), GUILayout.Width(23)))
				{
					if (foldedIn) foldedIn = false;
					else
						foldedIn = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);

			if (displayStats)
			{
				GUILayout.Label("Altitude: " + selectedSite.refalt.ToString("#0.0") + " m", LabelInfo);
				GUILayout.Label("Longitude: " + selectedSite.reflon.ToString("#0.000"), LabelInfo);
				GUILayout.Label("Latitude: " + selectedSite.reflat.ToString("#0.000"), LabelInfo);
				GUILayout.Space(3);
				GUILayout.Label("Length: " + selectedSite.sitelength.ToString("#0" + " m"), LabelInfo);
				GUILayout.Label("Width: " + selectedSite.sitewidth.ToString("#0" + " m"), LabelInfo);

				GUILayout.FlexibleSpace();
			}

			iFundsOpen = selectedSite.opencost;
			iFundsClose = selectedSite.closevalue;

			bool isAlwaysOpen = false;
			bool cannotBeClosed = false;

			if (iFundsOpen == 0)
				isAlwaysOpen = true;

			if (iFundsClose == 0)
				cannotBeClosed = true;

			if (MiscUtils.isCareerGame())
			{
				if (displayStats)
				{
					if (!KerbalKonstructs.instance.disableRemoteRecovery)
					{
						if (selectedSite.recoveryfactor > 0)
						{
							GUILayout.Label("Recovery Factor: " + selectedSite.recoveryfactor.ToString() + "%", LabelInfo);
							if (selectedSite.name != "Runway" && selectedSite.name != "LaunchPad")
							{
								if (selectedSite.recoveryrange > 0)
									rangekm = selectedSite.recoveryrange / 1000;
								else
									rangekm = 0;

								GUILayout.Label("Effective Range: " + rangekm.ToString() + " km", LabelInfo);
							}
							else
								GUILayout.Label("Effective Range: Unlimited", LabelInfo);
						}
						else
							GUILayout.Label("No Recovery Capability", LabelInfo);
					}

					GUILayout.FlexibleSpace();
					GUILayout.Label("Launch Refund: " + selectedSite.launchrefund.ToString() + "%", LabelInfo);
				}

				if (displayLog)
				{
					logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(120));
					{
						GUILayout.Box("Feature is WIP");
						GUILayout.Box("Log Entry");
						GUILayout.Box("Log Entry");
						GUILayout.Box("Log Entry");
					}
					GUILayout.EndScrollView();

					GUILayout.FlexibleSpace();
				}

				isOpen = (selectedSite.openclosestate == "Open");
				GUI.enabled = !isOpen;
				List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
				if (!isAlwaysOpen)
				{
					if (!KerbalKonstructs.instance.disableRemoteBaseOpening)
					{
						if (GUILayout.Button("Open Base for \n" + iFundsOpen + " funds", GUILayout.Height(40)))
						{
							double currentfunds = Funding.Instance.Funds;

							if (iFundsOpen > currentfunds)
								ScreenMessages.PostScreenMessage("Insufficient funds to open this base!", 10,
									ScreenMessageStyle.LOWER_CENTER);
							else
							{
								selectedSite.openclosestate = "Open";
								Funding.Instance.AddFunds(-iFundsOpen, TransactionReasons.Cheating);
								PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
							}
						}
					}
				}
				GUI.enabled = true;

				GUI.enabled = isOpen;
				if (!cannotBeClosed)
				{
					if (GUILayout.Button("Close Base for \n" + iFundsClose + " funds", GUILayout.Height(40)))
					{
						Funding.Instance.AddFunds(iFundsClose, TransactionReasons.Cheating);
						selectedSite.openclosestate = "Closed";

						PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
					}
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();

				if (HighLogic.LoadedScene == GameScenes.EDITOR)
				{
					GUILayout.BeginHorizontal();
					{
						if (selectedSite.name == EditorLogic.fetch.launchSiteName)
							tStatusLaunchsite = tSetLaunchsite;
						else
							if (isOpen || isAlwaysOpen)
								tStatusLaunchsite = tOpenedLaunchsite;
							else
								tStatusLaunchsite = tClosedLaunchsite;

						GUILayout.Label(tStatusLaunchsite, GUILayout.Height(32), GUILayout.Width(32));
						
						GUI.enabled = (isOpen || isAlwaysOpen) && !(selectedSite.name == EditorLogic.fetch.launchSiteName);
						if (GUILayout.Button("Set as \nLaunchsite", GUILayout.Height(40)))
						{
							LaunchSiteManager.setLaunchSite(selectedSite);
							string smessage = sButtonName + " has been set as the launchsite";
							ScreenMessages.PostScreenMessage(smessage, 10, 0);
						}
						GUI.enabled = true;

					}
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(3);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(1);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static LaunchSite getSelectedSite()
		{
			LaunchSite thisSite = selectedSite;
			return thisSite;
		}

		public static void setSelectedSite(LaunchSite soSite)
		{
			selectedSite = soSite;
		}
	}
}

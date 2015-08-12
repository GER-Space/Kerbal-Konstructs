using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.API;
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.UI
{
	public class LaunchSiteSelectorGUI
	{
		public Texture tIconClosed = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteclosed", false);
		public Texture tIconOpen = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);
		public Texture tSet = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/setaslaunchsite", false);
		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/horizontalsep2", false);

		public Texture tOpenBasesOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapOpenBasesOn", false);
		public Texture tOpenBasesOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapOpenBasesOff", false);
		public Texture tClosedBasesOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapClosedBasesOn", false);
		public Texture tClosedBasesOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapClosedBasesOff", false);
		public Texture tHelipadsOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapHelipadsOn", false);
		public Texture tHelipadsOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapHelipadsOff", false);
		public Texture tRunwaysOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapRunwaysOn", false);
		public Texture tRunwaysOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapRunwaysOff", false);
		public Texture tTrackingOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapTrackingOn", false);
		public Texture tTrackingOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapTrackingOff", false);
		public Texture tLaunchpadsOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapLaunchpadsOn", false);
		public Texture tLaunchpadsOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapLaunchpadsOff", false);
		public Texture tOtherOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapOtherOn", false);
		public Texture tOtherOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapOtherOff", false);

		public Texture tFavesOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapFavouritesOn", false);
		public Texture tFavesOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapFavouritesOff", false);

		public Texture tHolder = null;

		public Texture2D tNormalButton = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapButtonNormal", false);
		public Texture2D tHoverButton = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/mapButtonHover", false);

		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle KKWindow;
		GUIStyle BoxNoBorder;
		GUIStyle ButtonKK;

		LaunchSite selectedSite;
		private SiteType editorType = SiteType.Any;

		public float rangekm = 0;
		public string sCurrentSite = "";

		Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 390, 450);

		public void drawSelector()
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3, 3, 5, 5);

			windowRect = GUI.Window(0xB00B1E6, windowRect, drawSelectorWindow, "", KKWindow);

			if (windowRect.Contains(Event.current.mousePosition))
			{
				InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, "KKEditorLock2");
			}
			else
			{
				InputLockManager.RemoveControlLock("KKEditorLock2");
			}
		}

		public Vector2 sitesScrollPosition;
		public Vector2 descriptionScrollPosition;
		public List<LaunchSite> sites;

		public bool bOpenOn = true;
		public bool bClosedOn = true;
		public bool bRocketpadsOn = true;
		public bool bRunwaysOn = true;
		public bool bHelipadsOn = true;
		public bool bOtherOn = true;
		public bool bFavesOnly = false;

		public void drawSelectorWindow(int id)
		{
			ButtonKK = new GUIStyle(GUI.skin.button);
			ButtonKK.padding.left = 0;
			ButtonKK.padding.right = 0;
			//ButtonKK.normal.background = tNormalButton;
			//ButtonKK.hover.background = tHoverButton;

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

			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Launchsite Selector", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
				{
					InputLockManager.RemoveControlLock("KKEditorLock");
					InputLockManager.RemoveControlLock("KKEditorLock2");
					KerbalKonstructs.instance.showSiteSelector = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(5);
				
				if (MiscUtils.isCareerGame())
				{
					if (bOpenOn) tHolder = tOpenBasesOn;
					else tHolder = tOpenBasesOff;

					if (GUILayout.Button(new GUIContent(tHolder, "Open"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					{
						if (bOpenOn)
						{
							bOpenOn = false;
							bClosedOn = true;
						}
						else bOpenOn = true;
					}

					if (bClosedOn) tHolder = tClosedBasesOn;
					else tHolder = tClosedBasesOff;

					if (GUILayout.Button(new GUIContent(tHolder, "Closed"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
					{
						if (bClosedOn)
						{
							bClosedOn = false;
							bOpenOn = true;
						}
						else bClosedOn = true;
					}

					GUILayout.FlexibleSpace();
				}

				if (bFavesOnly) tHolder = tFavesOn;
				else tHolder = tFavesOff;

				if (GUILayout.Button(new GUIContent(tHolder, "Only Favourites"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
				{
					if (bFavesOnly)
					{
						bFavesOnly = false;
					}
					else bFavesOnly = true;
				}

				GUILayout.FlexibleSpace();

				if (editorType == SiteType.SPH)
					GUI.enabled = false;

				if (bRocketpadsOn) tHolder = tLaunchpadsOn;
				else tHolder = tLaunchpadsOff;

				if (GUILayout.Button(new GUIContent(tHolder, "Rocketpads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
				{
					bRocketpadsOn = true;
					bHelipadsOn = false;
					bRunwaysOn = false;
					bOtherOn = false;

					sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
						LaunchSiteManager.getLaunchSites(editorType, true, "RocketPad");
				}

				GUI.enabled = true;
				GUILayout.Space(2);

				if (editorType == SiteType.VAB)
					GUI.enabled = false;

				if (bRunwaysOn) tHolder = tRunwaysOn;
				else tHolder = tRunwaysOff;

				if (GUILayout.Button(new GUIContent(tHolder, "Runways"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
				{
					bRunwaysOn = true;
					bHelipadsOn = false;
					bRocketpadsOn = false;
					bOtherOn = false;

					sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
						LaunchSiteManager.getLaunchSites(editorType, true, "Runway");
				}

				GUI.enabled = true;
				GUILayout.Space(2);

				if (editorType == SiteType.VAB)
					GUI.enabled = false;

				if (bHelipadsOn) tHolder = tHelipadsOn;
				else tHolder = tHelipadsOff;

				if (GUILayout.Button(new GUIContent(tHolder, "Helipads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
				{
					bRocketpadsOn = false;
					bHelipadsOn = true;
					bRunwaysOn = false;
					bOtherOn = false;

					sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
						LaunchSiteManager.getLaunchSites(editorType, true, "Helipad");
				}

				GUI.enabled = true;
				GUILayout.Space(2);

				if (bOtherOn) tHolder = tOtherOn;
				else tHolder = tOtherOff;

				if (GUILayout.Button(new GUIContent(tHolder, "Other"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
				{
					bRocketpadsOn = false;
					bHelipadsOn = false;
					bRunwaysOn = false;
					bOtherOn = true;

					sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
						LaunchSiteManager.getLaunchSites(editorType, true, "Other");
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("ALL", GUILayout.Width(32), GUILayout.Height(32)))
				{
					bRocketpadsOn = true;
					bHelipadsOn = true;
					bRunwaysOn = true;
					bOtherOn = true;
					sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
						LaunchSiteManager.getLaunchSites(editorType, true, "ALL");
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			sitesScrollPosition = GUILayout.BeginScrollView(sitesScrollPosition);
			{
				if (sites == null) sites = (editorType == SiteType.Any) ?
					LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "ALL");

				sites.Sort(delegate(LaunchSite a, LaunchSite b)
				{
					return (a.name).CompareTo(b.name);
				});

				foreach (LaunchSite site in sites)
				{
					if (bFavesOnly)
					{
						if (site.favouritesite != "Yes")
							continue;
					}

					if (MiscUtils.isCareerGame())
					{
						if (!bOpenOn)
						{
							if (site.openclosestate == "Open" || site.opencost == 0)
								continue;
						}

						if (!bClosedOn)
						{
							if (site.openclosestate == "Closed")
								continue;
						}
						
						GUILayout.BeginHorizontal();
						if (site.openclosestate == "Open" || site.opencost == 0)
						{
							site.openclosestate = "Open";
							GUILayout.Label(tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
						}
						else
							GUILayout.Label(tIconClosed, GUILayout.Height(30), GUILayout.Width(30));
					}

					GUI.enabled = !(selectedSite == site);

					string sButtonName = "";
					sButtonName = site.name;
					if (site.name == "Runway") sButtonName = "KSC Runway";
					if (site.name == "LaunchPad") sButtonName = "KSC LaunchPad";

					if (GUILayout.Button(sButtonName, GUILayout.Height(30)))
					{
						selectedSite = site;

						if (!MiscUtils.isCareerGame())
						{
							LaunchSiteManager.setLaunchSite(site);
							smessage = "Launchsite set to " + sButtonName;
							ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
						}
					}
					GUI.enabled = true;

					if (MiscUtils.isCareerGame())
					{
						if (site.openclosestate == "Open" || site.opencost == 0)
						{
							site.openclosestate = "Open";
							GUILayout.Label(tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
						}
						else
							GUILayout.Label(tIconClosed, GUILayout.Height(30), GUILayout.Width(30));

						GUILayout.EndHorizontal();
					}
				}
			}
			GUILayout.EndScrollView();

			GUILayout.Space(5);

			sCurrentSite = LaunchSiteManager.getCurrentLaunchSite();

			if (sCurrentSite != null)
			{
				if (sCurrentSite == "Runway") 
					GUILayout.Box("Current Launchsite: KSC Runway");
				else
					if (sCurrentSite == "LaunchPad") 
						GUILayout.Box("Current Launchsite: KSC LaunchPad");
					else
						GUILayout.Box("Current Launchsite: " + sCurrentSite);
			}

			GUILayout.BeginHorizontal();

			if (editorType == SiteType.SPH)
				GUI.enabled = (KerbalKonstructs.instance.defaultSPHlaunchsite != sCurrentSite);

			if (editorType == SiteType.VAB)
				GUI.enabled = (KerbalKonstructs.instance.defaultVABlaunchsite != sCurrentSite);

			if (GUILayout.Button("Set as Default", GUILayout.Height(23)))
			{
				if (sCurrentSite != null)
				{
					if (editorType == SiteType.SPH)
						KerbalKonstructs.instance.defaultSPHlaunchsite = sCurrentSite;

					if (editorType == SiteType.VAB)
						KerbalKonstructs.instance.defaultVABlaunchsite = sCurrentSite;
				}
			}
			GUI.enabled = true;

			LaunchSite DefaultSite = null;

			if (GUILayout.Button("Use Default", GUILayout.Height(23)))
			{
				if (editorType == SiteType.SPH)
				{
					foreach (LaunchSite site in sites)
					{
						if (site.name == KerbalKonstructs.instance.defaultSPHlaunchsite)
							DefaultSite = site;
					}

					if (DefaultSite != null)
						LaunchSiteManager.setLaunchSite(DefaultSite);
				}

				if (editorType == SiteType.VAB)
				{
					foreach (LaunchSite site in sites)
					{
						if (site.name == KerbalKonstructs.instance.defaultVABlaunchsite)
							DefaultSite = site;
					}

					if (DefaultSite != null)
						LaunchSiteManager.setLaunchSite(DefaultSite);
				}

				if (DefaultSite != null)
				{
					smessage = DefaultSite.name + " has been set as the launchsite";
					ScreenMessages.PostScreenMessage(smessage, 10, 0);
				}
				else
				{
					smessage = "KK could not determine the default launchsite.";
					ScreenMessages.PostScreenMessage(smessage, 10, 0);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUI.enabled = true;

			if (selectedSite != null)
			{
				BaseManager.setSelectedSite(selectedSite);
				KerbalKonstructs.instance.showBaseManager = true;
			}
			else
			{
				if (LaunchSiteManager.getLaunchSites().Count > 0)
				{
					selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
					LaunchSiteManager.setLaunchSite(selectedSite);
					BaseManager.setSelectedSite(selectedSite);
					KerbalKonstructs.instance.showBaseManager = true;
				}
				else
				{
					Debug.Log("KK: ERROR Launch Selector cannot find KSC Runway or Launch Pad! PANIC! Runaway! Hide!");
				}
			}

			if (GUI.tooltip != "")
			{
				var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
				GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 2, labelSize.y + 2), GUI.tooltip, BoxNoBorder);
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		// ASH 05112014 Having the right panel always drawn might fix the selector centering issue on the right panel
		/* private void drawRightSelectorWindow()
		{
			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			GUILayout.BeginArea(new Rect(385, 25, 180, 550));
				string sButtonName = "";
				sButtonName = selectedSite.name;
				if (selectedSite.name == "Runway") sButtonName = "KSC Runway";
				if (selectedSite.name == "LaunchPad") sButtonName = "KSC LaunchPad";

				GUILayout.Box(sButtonName);
				GUILayout.Box("By " + selectedSite.author);
				GUILayout.BeginHorizontal();
				GUILayout.Box("", GUILayout.Height(150));
				GUILayout.FlexibleSpace();
				GUILayout.Box(selectedSite.logo, GUILayout.Width(160), GUILayout.Height(150));
				GUILayout.FlexibleSpace();
				GUILayout.Box("", GUILayout.Height(150));
				GUILayout.EndHorizontal();
				GUILayout.Box("Altitude: " + selectedSite.refalt.ToString("#0.0") + " m");
				GUILayout.Box("Longitude: " + selectedSite.reflon.ToString("#0.000"));
				GUILayout.Box("Latitude: " + selectedSite.reflat.ToString("#0.000"));
				descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition);
				GUI.enabled = false;
				GUILayout.TextArea(selectedSite.description);//, GUILayout.ExpandHeight(true));
				GUI.enabled = true;
				GUILayout.EndScrollView();
				GUILayout.Box("Length: " + selectedSite.sitelength.ToString("#0" + " m"));
				GUILayout.Box("Width: " + selectedSite.sitewidth.ToString("#0" + " m"));

				float iFundsClose = 0;
				// Career mode - get cost to open and value of opening from launchsite (defined in the cfg)
				iFundsOpen = selectedSite.opencost;
				iFundsClose = selectedSite.closevalue;

				bool isAlwaysOpen = false;
				bool cannotBeClosed = false;

				// Career mode
				// If a launchsite is 0 to open it is always open
				if (iFundsOpen == 0)
					isAlwaysOpen = true;

				// If it is 0 to close you cannot close it
				if (iFundsClose == 0)
					cannotBeClosed = true;
				
				if (isCareerGame())
				{	
					// Determine if a site is open or closed
					// If persistence says the site is open then isOpen = true;
					// If persistence file says nothing or site is closed then isOpen = false;
					// STUB IN KerbalKonstructs OnSiteSelectorOn()
					// Easier to just load the openclosestate of all launchsites on to the from so its ready when we get here

					GUILayout.Space(10);
					if (selectedSite.recoveryfactor > 0)
					{
						GUILayout.Box("Recovery Factor: " + selectedSite.recoveryfactor.ToString() + "%");
						if (selectedSite.name != "Runway" && selectedSite.name != "LaunchPad")
						{
							if (selectedSite.recoveryrange > 0)
								rangekm = selectedSite.recoveryrange / 1000;
							else
								rangekm = 0;

							GUILayout.Box("Effective Range: " + rangekm.ToString() + " km");
						}
						else
							GUILayout.Box("Effective Range: Unlimited");
					}
					else
					{
						GUILayout.Box("No Recovery Capability");
					}

					isOpen = (selectedSite.openclosestate == "Open");

					GUI.enabled = !isOpen;
					
					if (!isAlwaysOpen)
					{
						if (GUILayout.Button("Open Site for " + iFundsOpen + " Funds", GUILayout.Height(28)))
						{
							double currentfunds = Funding.Instance.Funds;

							if (iFundsOpen > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this site!", 10, 0);
							}
							else
							{
								// Open the site - save to instance
								selectedSite.openclosestate = "Open";

								// Charge some funds
								Funding.Instance.AddFunds(-iFundsOpen, TransactionReasons.Cheating);

								// Save new state to persistence
								PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");

								smessage = selectedSite.name + " has been opened";
								ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
							}
						}
					}
					GUI.enabled = true;
					
					GUI.enabled = isOpen;
					if (!cannotBeClosed)
					{
						if (GUILayout.Button("Close Site for " + iFundsClose + " Funds", GUILayout.Height(23)))
						{
							// Close the site - save to instance
							// Pay back some funds
							Funding.Instance.AddFunds(iFundsClose, TransactionReasons.Cheating);
							selectedSite.openclosestate = "Closed";

							smessage = selectedSite.name + " has been closed";
							ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);

							// Save new state to persistence
							PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
						}
					}
					GUI.enabled = true;

					// If the site is open and it isn't the selected launchsite, enable the set as launchsite button
					// in the right pane
					GUILayout.Box("Launch Refund: " + selectedSite.launchrefund.ToString() + "%");

					GUILayout.BeginHorizontal();
					GUILayout.Box(tSet, GUILayout.Height(35), GUILayout.Width(35));
					GUI.enabled = (isOpen || isAlwaysOpen) && !(selectedSite.name == EditorLogic.fetch.launchSiteName);
					if (GUILayout.Button("SET AS LAUNCHSITE", GUILayout.Height(34)))
					{
						LaunchSiteManager.setLaunchSite(selectedSite);
						smessage = sButtonName + " has been set as the launchsite";
						ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
					}
					GUI.enabled = true;
					GUILayout.EndHorizontal();
				}
			GUILayout.EndArea();
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		} */

		public void setEditorType(SiteType type)
		{
			editorType = (KerbalKonstructs.instance.launchFromAnySite) ? SiteType.Any : type;
			if (selectedSite != null)
			{
				if (selectedSite.type != editorType && selectedSite.type != SiteType.Any)
				{
					selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
				}
				// ASH Career Mode Unlocking
				// In career the launchsite is not set by the launchsite list but rather in the launchsite description
				// panel on the right
				// if (!isCareerGame())
				LaunchSiteManager.setLaunchSite(selectedSite);
			}
		}
		
		// ASH and Ravencrow 28102014
		// Need to handle if Launch Selector is still open when switching from VAB to from SPH
		// otherwise abuse possible!
		public void Close()
		{
			sites = null;
		}
	}
}
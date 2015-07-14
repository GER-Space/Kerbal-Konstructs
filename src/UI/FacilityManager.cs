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
		Rect mapManagerRect = new Rect(200, 150, 320, 550);

		public Vector2 descriptionScrollPosition;
		
		public static LaunchSite selectedSite = null;
		public static StaticObject selectedFacility = null;

		public float iFundsOpen = 0;
		public float iFundsClose = 0;
		public float iFundsOpen2 = 0;
		public float iFundsClose2 = 0;
		float fAlt = 0f;
		float fRange = 0f;
		float fXP = 0f;
		float fStaff = 0f;

		public Boolean isOpen = false;
		public Boolean isOpen2 = false;
		public Boolean bChangeTargetType = false;
		public Boolean bChangeTarget = false;
		bool loadedPersistence = false;

		string sFacilityName = "Unknown";
		string sFacilityType = "Unknown";
		string sTargetType = "None";
		string sTarget = "None";

		Vector3 ObjectPos = new Vector3(0, 0, 0);

		double dObjectLat = 0;
		double dObjectLon = 0;
		double disObjectLat = 0;
		double disObjectLon = 0;

		GUIStyle Yellowtext;
		GUIStyle TextAreaNoBorder;
		GUIStyle BoxNoBorder;

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

		public static void setSelectedSite(LaunchSite lSite)
		{
			selectedSite = lSite;
		}

		public static void setSelectedFacility(StaticObject oFacility)
		{
			selectedFacility = oFacility;
		}

		private static double GetLongitude(Vector3d radialPosition)
		{
			Vector3d norm = radialPosition.normalized;
			double longitude = Math.Atan2(norm.z, norm.x);
			return (!double.IsNaN(longitude) ? longitude : 0.0);
		}

		private static double GetLatitude(Vector3d radialPosition)
		{
			double latitude = Math.Asin(radialPosition.normalized.y);
			return (!double.IsNaN(latitude) ? latitude : 0.0);
		}

		public void drawManager()
		{
			mapManagerRect = GUI.Window(0xB00B2E7, mapManagerRect, drawMapManagerWindow, "Base Boss : Facility Manager");
		}

		void drawMapManagerWindow(int windowID)
		{
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
				loadedPersistence = true;
			}

			GUILayout.BeginHorizontal();

			GUI.enabled = (MiscUtils.isCareerGame());
			if (!MiscUtils.isCareerGame())
			{
				GUILayout.Button(tOpenBasesOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Button(tClosedBasesOff, GUILayout.Width(32), GUILayout.Height(32));
				GUILayout.Space(3);
				GUILayout.Button(tTrackingOff, GUILayout.Width(32), GUILayout.Height(32));
			}
			else
			{
				if (KerbalKonstructs.instance.mapShowOpen)
				{
					if (GUILayout.Button(tOpenBasesOn, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpen = false;
				}
				else
				{
					if (GUILayout.Button(tOpenBasesOff, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpen = true;
				}

				if (KerbalKonstructs.instance.mapShowClosed)
				{
					if (GUILayout.Button(tClosedBasesOn, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowClosed = false;
				}
				else
				{
					if (GUILayout.Button(tClosedBasesOff, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowClosed = true;
				}


				GUILayout.Space(3);
				if (KerbalKonstructs.instance.mapShowOpenT)
				{
					if (GUILayout.Button(tTrackingOn, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpenT = false;
				}
				else
				{

					if (GUILayout.Button(tTrackingOff, GUILayout.Width(32), GUILayout.Height(32)))
						KerbalKonstructs.instance.mapShowOpenT = true;
				}
			}
			GUI.enabled = true;

			GUILayout.Space(3);

			if (KerbalKonstructs.instance.mapShowRocketbases)
			{
				if (GUILayout.Button(tLaunchpadsOn, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRocketbases = false;
			}
			else
			{
				if (GUILayout.Button(tLaunchpadsOff, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRocketbases = true;
			}

			if (KerbalKonstructs.instance.mapShowHelipads)
			{
				if (GUILayout.Button(tHelipadsOn, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowHelipads = false;
			}
			else
			{
				if (GUILayout.Button(tHelipadsOff, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowHelipads = true;
			}

			if (KerbalKonstructs.instance.mapShowRunways)
			{
				if (GUILayout.Button(tRunwaysOn, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRunways = false;
			}
			else
			{
				if (GUILayout.Button(tRunwaysOff, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowRunways = true;
			}

			if (KerbalKonstructs.instance.mapShowOther)
			{
				if (GUILayout.Button(tOtherOn, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowOther = false;
			}
			else
			{
				if (GUILayout.Button(tOtherOff, GUILayout.Width(32), GUILayout.Height(32)))
					KerbalKonstructs.instance.mapShowOther = true;
			}

			GUILayout.EndHorizontal();

			GUILayout.Space(3);

			if (selectedFacility != null)
			{
				GUILayout.Box("Selected Facility");

				sFacilityType = (string)selectedFacility.getSetting("FacilityType");

				if (sFacilityType == "TrackingStation") sFacilityName = "Tracking Station";
				else
					sFacilityName = sFacilityType;

				GUILayout.Box("" + sFacilityName, Yellowtext);
				GUILayout.Space(5);

				fAlt = (float)selectedFacility.getSetting("RadiusOffset");

				ObjectPos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(selectedFacility.gameObject.transform.position);
				dObjectLat = GetLatitude(ObjectPos);
				dObjectLon = GetLongitude(ObjectPos);
				disObjectLat = dObjectLat * 180 / Math.PI;
				disObjectLon = dObjectLon * 180 / Math.PI;

				if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

				GUILayout.Box("Altitude: " + fAlt.ToString("#0.0") + " m");
				GUILayout.Box("Latitude: " + disObjectLat.ToString("#0.000"));
				GUILayout.Box("Longitude: " + disObjectLon.ToString("#0.000"));
				GUILayout.Space(5);

				iFundsOpen2 = (float)selectedFacility.getSetting("OpenCost");
				iFundsClose2 = (float)selectedFacility.getSetting("CloseValue");

				bool isAlwaysOpen2 = false;
				bool cannotBeClosed2 = false;

				// Career mode
				// If a launchsite is 0 to open it is always open
				if (iFundsOpen2 == 0)
					isAlwaysOpen2 = true;

				// If it is 0 to close you cannot close it
				if (iFundsClose2 == 0)
					cannotBeClosed2 = true;

				if (MiscUtils.isCareerGame())
				{
					isOpen2 = ((string)selectedFacility.getSetting("OpenCloseState") == "Open");

					GUI.enabled = !isOpen2;

					if (!isAlwaysOpen2)
					{
						if (GUILayout.Button("Open Facility for " + iFundsOpen2 + " Funds"))
						{
							double currentfunds2 = Funding.Instance.Funds;

							if (iFundsOpen2 > currentfunds2)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this facility!", 10, ScreenMessageStyle.LOWER_CENTER);
							}
							else
							{
								// Open the site - save to instance
								selectedFacility.setSetting("OpenCloseState", "Open");

								// Charge some funds
								Funding.Instance.AddFunds(-iFundsOpen2, TransactionReasons.Cheating);

								// Save new state to persistence
								PersistenceUtils.saveStaticPersistence(selectedFacility);
							}
						}
					}
					GUI.enabled = true;

					GUI.enabled = isOpen2;
					if (!cannotBeClosed2)
					{
						if (GUILayout.Button("Close Facility for " + iFundsClose2 + " Funds"))
						{
							// Close the site - save to instance
							// Pay back some funds
							Funding.Instance.AddFunds(iFundsClose2, TransactionReasons.Cheating);
							selectedFacility.setSetting("OpenCloseState", "Closed");

							// Save new state to persistence
							PersistenceUtils.saveStaticPersistence(selectedFacility);
						}
					}
					GUI.enabled = true;
				}

				isOpen2 = ((string)selectedFacility.getSetting("OpenCloseState") == "Open");

				GUI.enabled = (isOpen2 || isAlwaysOpen2);
				if (sFacilityType == "TrackingStation")
				{
					fRange = (float)selectedFacility.getSetting("TrackingShort");
					sTargetType = (string)selectedFacility.getSetting("TargetType");
					sTarget = (string)selectedFacility.getSetting("TargetID");

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Short Range: ");
					GUILayout.Box("" + fRange.ToString("#0") + " m", GUILayout.Height(30), GUILayout.Width(150));
					if (GUILayout.Button("Upgrade", GUILayout.Height(26), GUILayout.Width(70)))
					{
						ScreenMessages.PostScreenMessage("Unable to upgrade this facility. Insufficient materials.", 10, ScreenMessageStyle.LOWER_CENTER);
					}
					GUILayout.EndHorizontal();
					GUILayout.Label("To upgrade a kerbonaut Scientist and Engineer must first visit the station. Then processed ore will need to be delivered to the station.");

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Target Type: ");
					GUILayout.Box("" + sTargetType, GUILayout.Height(30), GUILayout.Width(150));
					if (GUILayout.Button("Change", GUILayout.Height(26), GUILayout.Width(70)))
					{
						bChangeTargetType = true;
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Tracking: ");
					GUILayout.Box(" " + sTarget, GUILayout.Height(30), GUILayout.Width(150));
					if (GUILayout.Button("Change", GUILayout.Height(26), GUILayout.Width(70)))
					{
						if (sTargetType == "None")
							ScreenMessages.PostScreenMessage("Please select a target type first.", 10, ScreenMessageStyle.LOWER_CENTER);
						else
							bChangeTarget = true;
					}
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Status: ");
					GUILayout.Box(" ", GUILayout.Height(30), GUILayout.Width(150));
					if (GUILayout.Button("Stop", GUILayout.Height(26), GUILayout.Width(70)))
					{ }
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
				}

				fStaff = (float)selectedFacility.getSetting("StaffCurrent");
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Staff: ");
				GUILayout.Box("" + fStaff.ToString("#0"), GUILayout.Height(30), GUILayout.Width(150));
				if (GUILayout.Button("Hire", GUILayout.Height(26), GUILayout.Width(33)))
				{ }
				if (GUILayout.Button("Fire", GUILayout.Height(26), GUILayout.Width(33)))
				{ }
				GUILayout.EndHorizontal();

				fXP = (float)selectedFacility.getSetting("FacilityXP");
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Facility XP: ");
				GUILayout.Box("" + fXP.ToString("#0.00"), GUILayout.Height(30), GUILayout.Width(150));
				if (GUILayout.Button("Spend", GUILayout.Height(26), GUILayout.Width(70)))
				{ }
				GUILayout.EndHorizontal();
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Done"))
				{
					PersistenceUtils.saveStaticPersistence(selectedFacility);
					selectedFacility = null;
				}
			}

			if (selectedSite != null)
			{
				GUILayout.Box("Selected Base");

				if (selectedSite.name == "Runway")
					GUILayout.Box("KSC Runway", Yellowtext);
				else
					if (selectedSite.name == "LaunchPad")
						GUILayout.Box("KSC LaunchPad", Yellowtext);
					else
						GUILayout.Box("" + selectedSite.name, Yellowtext);

				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Box(selectedSite.logo, GUILayout.Width(145), GUILayout.Height(140));
				descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition, GUILayout.Height(140));
				GUI.enabled = false;
				GUILayout.TextArea(selectedSite.description, TextAreaNoBorder);
				GUI.enabled = true;
				GUILayout.EndScrollView();
				GUILayout.EndHorizontal();

				GUILayout.Space(5);
				GUILayout.Box("Altitude: " + selectedSite.refalt.ToString("#0.0") + " m");
				GUILayout.Box("Longitude: " + selectedSite.reflon.ToString("#0.000"));
				GUILayout.Box("Latitude: " + selectedSite.reflat.ToString("#0.000"));
				GUILayout.Space(5);

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

				if (MiscUtils.isCareerGame())
				{
					isOpen = (selectedSite.openclosestate == "Open");

					GUI.enabled = !isOpen;
					List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();

					if (!isAlwaysOpen)
					{
						if (GUILayout.Button("Open Base for " + iFundsOpen + " Funds"))
						{
							double currentfunds = Funding.Instance.Funds;

							if (iFundsOpen > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this base!", 10, ScreenMessageStyle.LOWER_CENTER);
							}
							else
							{
								// Open the site - save to instance
								selectedSite.openclosestate = "Open";

								// Charge some funds
								Funding.Instance.AddFunds(-iFundsOpen, TransactionReasons.Cheating);

								// Save new state to persistence
								PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
							}
						}
					}
					GUI.enabled = true;

					GUI.enabled = isOpen;
					if (!cannotBeClosed)
					{
						if (GUILayout.Button("Close Base for " + iFundsClose + " Funds"))
						{
							// Close the site - save to instance
							// Pay back some funds
							Funding.Instance.AddFunds(iFundsClose, TransactionReasons.Cheating);
							selectedSite.openclosestate = "Closed";

							// Save new state to persistence
							PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
						}
					}
					GUI.enabled = true;
				}
			}
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
	}
}

using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;
using LibNoise.Unity.Operator;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.UI
{
	public class FacilityManager
	{
		Rect targetSelectorRect = new Rect(640, 120, 200, 400);
		public static Rect facilityManagerRect = new Rect(150, 75, 300, 650);

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);

		public Vector2 descriptionScrollPosition;
		public Vector2 scrollNearbyCraft;
		public Vector2 scrollOreTransfer;
		public Vector2 scrollOreTransfer2;
		
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

		public Boolean bHalfwindow = false;
		public Boolean bHalvedWindow = false;

		public Boolean bTransferOreToF = false;
		public Boolean bTransferOreToC = false;

		string sFacilityName = "Unknown";
		string sFacilityType = "Unknown";
		string sInStorage = "None";
		string sInStorage2 = "None";
		string sInStorage3 = "None";
		public static string sTargetType = "None";

		// string sOreTransferAmount = "0";

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
		GUIStyle ButtonSmallText;

		void drawTargetSelector(int windowID)
		{
			TrackingStationGUI.TargetSelector(sTargetType, selectedFacility);
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void drawFacilityManager(StaticObject soObject)
		{
			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(3, 3, 5, 5);

			if (bHalfwindow)
			{
				if (!bHalvedWindow)
				{
					facilityManagerRect = new Rect(facilityManagerRect.xMin, facilityManagerRect.yMin, facilityManagerRect.width, facilityManagerRect.height - 200);
					bHalvedWindow = true;
				}
			}

			if (!bHalfwindow)
			{
				if (bHalvedWindow)
				{
					facilityManagerRect = new Rect(facilityManagerRect.xMin, facilityManagerRect.yMin, facilityManagerRect.width, facilityManagerRect.height + 200);
					bHalvedWindow = false;
				}
			}

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

			ButtonSmallText = new GUIStyle(GUI.skin.button);
			ButtonSmallText.fontSize = 12;
			ButtonSmallText.fontStyle = FontStyle.Normal;

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

				if (sFacilityType == "TrackingStation")
				{
					sFacilityName = "Tracking Station";
					bHalfwindow = true;
				}
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

				string sPurpose = "";

				if (sFacilityType == "Hangar")
				{
					sPurpose = "Craft can be stored in this building for launching from the base at a later date. The building has limited space.";
					bHalfwindow = true;
				}
				else
					if (sFacilityType == "RocketAssembly")
					{
						sPurpose = "This facility can construct craft that have been designed in KSC's VAB and can store a constructed craft for launching from the base at a later date.";
						bHalfwindow = false;
					}
					else
						if (sFacilityType == "PlaneAssembly")
						{
							sPurpose = "This facility can construct craft that have been designed in KSC's SPH and can store a constructed craft for launching from the base at a later date.";
							bHalfwindow = false;
						}
						else
							if (sFacilityType == "ControlTower")
							{
								sPurpose = "This facility manages incoming and outgoing air-traffic to and from the base, as well as administrating most other base operations.";
							}
							else
								if (sFacilityType == "Barracks")
								{
									sPurpose = "This facility provides a temporary home for base-staff. Other facilities can draw staff from the pool available at this facility.";
									bHalfwindow = true;
								}
								else
									if (sFacilityType == "RadarStation")
									{
										sPurpose = "This facility tracks craft in the planet's atmosphere at a limited range. It provides bonuses for recovery operations by the nearest open base.";
										bHalfwindow = true;
									}
									else
										if (sFacilityType == "Research")
										{
											sPurpose = "This facility carries out research and generates Science.";
											bHalfwindow = true;
										}
										else
											if (sFacilityType == "Mining")
											{
												sPurpose = "This facility excavates useful minerals and materials and thus generates Ore.";
											}
											else
												if (sFacilityType == "Refining")
												{
													sPurpose = "This facility converts Ore into fuels.";
												}
												else
													if (sFacilityType == "Manufacturing")
													{
														sPurpose = "This facility converts Ore into Processed Ore, for use in manufacturing craft in lieu of Funds, constructing and upgrading facilities.";
													}
													else
														if (sFacilityType == "Business")
														{
															sPurpose = "This facility carries out business related to the space program in order to generate Funds.";
															bHalfwindow = true;
														}
														else
															if (sFacilityType == "Training")
															{
																sPurpose = "This facility can provide professional skills and experience to rookie Kerbonauts.";
															}
															else
																if (sFacilityType == "Medical")
																{
																	sPurpose = "This facility can aid Kerbonaut recovery after long missions or injury.";
																}
																else
																	if (sFacilityType == "TrackingStation")
																	{
																		sPurpose = "This facility can track a variety of off-Kerbin targets, including spacecraft, celestial bodies and asteroids.";
																		bHalfwindow = true;
																	}
																	else
																		if (sFacilityType == "FuelTanks")
																		{
																			sPurpose = "This facility stores fuel for craft.";
																			bHalfwindow = false;
																		}
																		else
																			if (sFacilityType == "Storage")
																			{
																				sPurpose = "This facility stores construction materials (Processed Ore).";
																			}
																			else
																				if (sFacilityType == "CraftAssembly")
																				{
																					sPurpose = "This facility can construct craft that have been designed in KSC's VAB or SPH and can store a constructed craft for launching from the base at a later date.";
																					bHalfwindow = false;
																				}

				GUILayout.Label(sPurpose, LabelInfo);
				GUILayout.Space(2);
				GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
				GUILayout.Space(3);

				SharedInterfaces.OpenCloseFacility(selectedFacility);
				
				iFundsOpen2 = (float)selectedFacility.getSetting("OpenCost");
				isOpen2 = ((string)selectedFacility.getSetting("OpenCloseState") == "Open");
				float iFundsDefaultCost = (float)selectedFacility.model.getSetting("cost");
				if (iFundsOpen2 == 0) iFundsOpen2 = iFundsDefaultCost;

				if (iFundsOpen2 == 0) isOpen2 = true;

				GUILayout.Space(2);
				GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
				GUILayout.Space(3);

				GUI.enabled = isOpen2;

				if (sFacilityType == "TrackingStation")
				{
					TrackingStationGUI.TrackingInterface(selectedFacility);
				}

				if (sFacilityType == "Hangar" || sFacilityType == "RocketAssembly" || sFacilityType == "PlaneAssembly" || sFacilityType == "CraftAssembly")
				{
					sInStorage = (string)selectedFacility.getSetting("InStorage");
					sInStorage2 = (string)selectedFacility.getSetting("TargetID");
					sInStorage3 = (string)selectedFacility.getSetting("TargetType");

					float fMaxMass = (float)selectedFacility.model.getSetting("DefaultFacilityMassCapacity");
					if (fMaxMass < 1) fMaxMass = 25f;
					float fMaxCrafts = (float)selectedFacility.model.getSetting("DefaultFacilityCraftCapacity");
					if (fMaxCrafts < 1 || fMaxCrafts > 3) fMaxCrafts = 2;

					GUILayout.Space(2);
					GUILayout.Label("Where necessary craft are disassembled for storage or re-assembled before being rolled out. Please note that for game purposes, this procedure is instantaneous.", LabelInfo);
					GUILayout.BeginHorizontal();
					GUILayout.Label("Max Craft: " + fMaxCrafts.ToString("#0"), LabelInfo);
					GUILayout.FlexibleSpace();
					GUILayout.Label("Max Mass/Craft: " + fMaxMass.ToString("#0") + " T", LabelInfo);
					GUILayout.EndHorizontal();
 
					if (sInStorage == null || sInStorage == "")
					{
						sInStorage = "None";
						selectedFacility.setSetting("InStorage", "None");
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}
					if (sInStorage2 == null || sInStorage2 == "")
					{
						sInStorage2 = "None";
						selectedFacility.setSetting("TargetID", "None");
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}
					if (sInStorage3 == null || sInStorage3 == "")
					{
						sInStorage3 = "None";
						selectedFacility.setSetting("TargetType", "None");
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}

					if (sInStorage == "None" && sInStorage2 == "None" && sInStorage3 == "None")
						GUILayout.Label("No craft currently held in this facility.", LabelInfo);
					else
					{
						int iNumberCrafts = NumberCraftHangared(selectedFacility);

						GUILayout.Box("Stored Craft (" + iNumberCrafts.ToString() + "/" + fMaxCrafts.ToString("#0") + ")", Yellowtext);

						List<Vessel> lVessels = FlightGlobals.Vessels;
						
						foreach (Vessel vVesselStored in lVessels)
						{
							if (vVesselStored.id.ToString() == sInStorage)
							{
								if (GUILayout.Button("" + vVesselStored.vesselName, ButtonSmallText, GUILayout.Height(20)))
								{
									// Empty the hangar
									if (HangarwayIsClear(selectedFacility))
									{
										sInStorage = "None";
										UnhangarCraft(vVesselStored, selectedFacility);
										sInStorage = "None";
									}
									else
									{
										MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10,
											3);
									}
								}
								break;
							}
						}

						foreach (Vessel vVesselStored in lVessels)
						{
							if (vVesselStored.id.ToString() == sInStorage2)
							{
								if (GUILayout.Button("" + vVesselStored.vesselName, ButtonSmallText, GUILayout.Height(20)))
								{
									// Empty the hangar
									if (HangarwayIsClear(selectedFacility))
									{
										sInStorage2 = "None";
										UnhangarCraft(vVesselStored, selectedFacility);
										sInStorage2 = "None";
									}
									else
									{
										MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10,
											3);
									}
								}
								break;
							}
						}
						
						foreach (Vessel vVesselStored in lVessels)
						{
							if (vVesselStored.id.ToString() == sInStorage3)
							{
								if (GUILayout.Button("" + vVesselStored.vesselName, ButtonSmallText, GUILayout.Height(20)))
								{
									// Empty the hangar
									if (HangarwayIsClear(selectedFacility))
									{
										sInStorage3 = "None";
										UnhangarCraft(vVesselStored, selectedFacility);
										sInStorage3 = "None";
									}
									else
									{
										MiscUtils.HUDMessage("Cannot roll craft out. Clear the way first!", 10,
											3);
									}
								}
								break;
							}
						}
					}

					GUILayout.Space(5);

					scrollNearbyCraft = GUILayout.BeginScrollView(scrollNearbyCraft);

					GUILayout.Box("Nearby Craft", Yellowtext);

					bool bNearbyCraft = false;

					foreach (Vessel vVessel in FlightGlobals.Vessels)
					{
						if (vVessel == null) continue;
						if (!vVessel.loaded) continue;
						if (vVessel.vesselType == VesselType.SpaceObject) continue;
						if (vVessel.vesselType == VesselType.Debris) continue;
						if (vVessel.vesselType == VesselType.EVA) continue;
						if (vVessel.vesselType == VesselType.Flag) continue;
						if (vVessel.vesselType == VesselType.Unknown) continue;
						if (vVessel == FlightGlobals.ActiveVessel) continue;
						if (vVessel.situation != Vessel.Situations.LANDED) continue;
						if (vVessel.GetCrewCount() > 0) continue;

						var vDistToCraft = Vector3.Distance(vVessel.gameObject.transform.position, selectedFacility.gameObject.transform.position);
						if (vDistToCraft > 250) continue;

						bNearbyCraft = true;

						if (GUILayout.Button(" " + vVessel.vesselName + " ", ButtonSmallText, GUILayout.Height(20)))
						{
							float fMass = vVessel.GetTotalMass();

							if (fMass > fMaxMass)
							{
								MiscUtils.HUDMessage("Craft too heavy for this facility. Max " + fMaxMass.ToString("#0") + "T per craft.", 10,
									3);
							}
							else
							{
								float fMaxCraft = (float)selectedFacility.model.getSetting("DefaultFacilityCraftCapacity");
								if (fMaxCraft < 1 || fMaxCraft > 3) fMaxCraft = 2;

								int iNumberCraft = NumberCraftHangared(selectedFacility);

								if (iNumberCraft < (int)fMaxCraft)
									HangarCraft(vVessel, selectedFacility, (int)fMaxCraft);
								else
									MiscUtils.HUDMessage("This facility is full. Max craft: " + fMaxCraft.ToString("#0"), 10,
										3);
							}
						}
					}

					if (!bNearbyCraft)
						GUILayout.Label("There are no craft close enough to store in this facility.", LabelInfo);

					GUILayout.EndScrollView();

					GUILayout.FlexibleSpace();
				}

/*				if (sFacilityType == "RocketAssembly" || sFacilityType == "PlaneAssembly" || sFacilityType == "CraftAssembly")
				{
					string sProducing = (string)selectedFacility.getSetting("Producing");

					if (sProducing == null || sProducing == "")
					{
						sProducing = "None";
						selectedFacility.setSetting("Producing", "None");
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}

					if (GUILayout.Button("Construct a Craft", ButtonSmallText, GUILayout.Height(20)))
					{ 
						if (sProducing != "None")
							MiscUtils.HUDMessage("Only one craft can be constructed at a time.", 10,
								3);
					}

					GUILayout.Space(3);
					if (sProducing == "None")
						GUILayout.Label("No craft currently under construction in this facility.", LabelInfo);
					else
					{
						GUILayout.Label("Craft Under Construction: ", LabelInfo);

						// TO DO List of craft
						GUILayout.Label("Cost of Construction: X Funds / X Materials", LabelInfo);
						GUILayout.Label("Total Construction Time: X hours", LabelInfo);
						GUILayout.Label("Time to Completion: X hours", LabelInfo);
						if (GUILayout.Button("Assign a Kerbonaut Engineer", ButtonSmallText, GUILayout.Height(20)))
						{ }
					}

					if (GUILayout.Button("Upgrade Production", ButtonSmallText, GUILayout.Height(20)))
					{ }

					float fAvailableMaterials;

					fAvailableMaterials = (float)selectedFacility.getSetting("PrOreCurrent");

					GUILayout.Space(3);
					GUILayout.Label("Available Materials (Processed Ore): " + fAvailableMaterials.ToString("#0.0"), LabelInfo);
				} */

				float fStaffing = 0;
				float fProductionRate = 0;
				float fLastCheck = 0;

				if (sFacilityType == "Research" || sFacilityType == "Business" || sFacilityType == "Mining" ||
					sFacilityType == "RocketAssembly" || sFacilityType == "PlaneAssembly" || sFacilityType == "CraftAssembly")
				{
					// Check production since last check
					fStaffing = (float)selectedFacility.getSetting("StaffCurrent");
					fProductionRate = (float)selectedFacility.getSetting("ProductionRateCurrent") * (fStaffing / 2f);

					if (fProductionRate < 0.01f)
					{
						float fDefaultRate = 0.01f;

						if (sFacilityType == "Business") fDefaultRate = 0.10f;
						if (sFacilityType == "Mining") fDefaultRate = 0.05f;

						selectedFacility.setSetting("ProductionRateCurrent", fDefaultRate);
						PersistenceUtils.saveStaticPersistence(selectedFacility);
						fProductionRate = fDefaultRate * (fStaffing / 2f);
					}

					fLastCheck = (float)selectedFacility.getSetting("LastCheck");

					if (fLastCheck == 0)
					{
						fLastCheck = (float)Planetarium.GetUniversalTime();
						selectedFacility.setSetting("LastCheck", fLastCheck);
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}
				}

				if (sFacilityType == "Research" || sFacilityType == "Business" || sFacilityType == "Mining")
				{
					string sProduces = "";
					float fMax = 0f;
					float fCurrent = 0f;

					if (sFacilityType == "Research")
					{
						sProduces = "Science";
						fMax = (float)selectedFacility.getSetting("ScienceOMax");

						if (fMax < 1)
						{
							fMax = (float)selectedFacility.model.getSetting("DefaultScienceOMax");

							if (fMax < 1) fMax = 10f;

							selectedFacility.setSetting("ScienceOMax", fMax);
							PersistenceUtils.saveStaticPersistence(selectedFacility);
						}

						fCurrent = (float)selectedFacility.getSetting("ScienceOCurrent");
					}
					if (sFacilityType == "Business")
					{
						sProduces = "Funds";
						fMax = (float)selectedFacility.getSetting("FundsOMax");

						if (fMax < 1)
						{
							fMax = (float)selectedFacility.model.getSetting("DefaultFundsOMax");

							if (fMax < 1) fMax = 10000f;

							selectedFacility.setSetting("FundsOMax", fMax);
							PersistenceUtils.saveStaticPersistence(selectedFacility);
						}

						fCurrent = (float)selectedFacility.getSetting("FundsOCurrent");
					}
					if (sFacilityType == "Mining")
					{
						sProduces = "Ore";
						fMax = (float)selectedFacility.model.getSetting("OreMax");

						if (fMax < 1)
						{
							fMax = 500f;
						}

						fCurrent = (float)selectedFacility.getSetting("OreCurrent");
					}

					double dTime = Planetarium.GetUniversalTime();

					// Deal with revert exploits
					if (fLastCheck > (float)dTime)
					{
						selectedFacility.setSetting("LastCheck", (float)dTime);
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}

					if ((float)dTime - fLastCheck > 43200)
					{
						float fDays = (((float)dTime - fLastCheck) / 43200);

						float fProduced = fDays * fProductionRate;

						fCurrent = fCurrent + fProduced;
						if (fCurrent > fMax) fCurrent = fMax;

						if (sFacilityType == "Research")
						{
							selectedFacility.setSetting("ScienceOCurrent", fCurrent);
						}
						if (sFacilityType == "Business")
						{
							selectedFacility.setSetting("FundsOCurrent", fCurrent);
						}
						if (sFacilityType == "Mining")
						{
							selectedFacility.setSetting("OreCurrent", fCurrent);
						}

						selectedFacility.setSetting("LastCheck", (float)dTime);
						PersistenceUtils.saveStaticPersistence(selectedFacility);
					}

					GUILayout.BeginHorizontal();
					GUILayout.Label("Produces: " + sProduces, LabelInfo);
					GUILayout.FlexibleSpace();
					GUILayout.Label("Current: " + fCurrent.ToString("#0") + " | Max: " + fMax.ToString("#0"), LabelInfo);
					GUILayout.EndHorizontal();
					//if (GUILayout.Button("Upgrade Max Capacity", ButtonSmallText, GUILayout.Height(20)))
					//{ }

					if (sFacilityType == "Research")
					{
						if (GUILayout.Button("Transfer Science to KSC R&D", ButtonSmallText, GUILayout.Height(20)))
						{
							ResearchAndDevelopment.Instance.AddScience(fCurrent, TransactionReasons.Cheating);
							selectedFacility.setSetting("ScienceOCurrent", 0f);
							PersistenceUtils.saveStaticPersistence(selectedFacility);
						}
						/* GUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("Assign a Special Project", ButtonSmallText, GUILayout.Height(20)))
							{ }
							if (GUILayout.Button("Deliver Research Materials", ButtonSmallText, GUILayout.Height(20)))
							{ }
						}
						GUILayout.EndHorizontal();
						if (GUILayout.Button("Assign a Kerbonaut Scientist", ButtonSmallText, GUILayout.Height(20)))
						{ } */
					}
					if (sFacilityType == "Business")
					{
						if (GUILayout.Button("Transfer Funds to KSC Account", ButtonSmallText, GUILayout.Height(20)))
						{
							Funding.Instance.AddFunds((double)fCurrent, TransactionReasons.Cheating);
							selectedFacility.setSetting("FundsOCurrent", 0f);
							PersistenceUtils.saveStaticPersistence(selectedFacility);
						}
					}
					/* if (sFacilityType == "Mining")
					{
						if (GUILayout.Button("Transfer Ore to/from Craft", ButtonSmallText, GUILayout.Height(20)))
						{
							if (bTransferOreToC) bTransferOreToC = false;
							else bTransferOreToC = true;
						}

						if (bTransferOreToC)
						{
							// Ore transfer to craft GUI
							GUILayout.Label("Select Craft & Container", LabelInfo);
							scrollOreTransfer = GUILayout.BeginScrollView(scrollOreTransfer);
							GUILayout.Label("Select Craft & Container", LabelInfo);
							GUILayout.Label("Select Craft & Container", LabelInfo);
							GUILayout.Label("Select Craft & Container", LabelInfo);
							GUILayout.EndScrollView();
							GUILayout.BeginHorizontal();
							if (GUILayout.Button("Into Craft", GUILayout.Height(23)))
							{

							}
							if (GUILayout.Button("Out of Craft", GUILayout.Height(23)))
							{

							}
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							GUILayout.Label("Amount: ", LabelInfo);
							sOreTransferAmount = GUILayout.TextField(sOreTransferAmount, 7, GUILayout.Width(120));
							if (GUILayout.Button("Max", GUILayout.Height(23)))
							{

							}
							GUILayout.EndHorizontal();
							if (GUILayout.Button("Proceed", GUILayout.Height(23)))
							{

							}

							GUILayout.FlexibleSpace();
						}

						if (GUILayout.Button("Transfer Ore to Facility", ButtonSmallText, GUILayout.Height(20)))
						{
							if (bTransferOreToF) bTransferOreToF = false;
							else bTransferOreToF = true;
						
						}

						if (bTransferOreToF)
						{
							// Ore transfer to Facility GUI
							GUILayout.Label("Select Destination Facility", LabelInfo);
							scrollOreTransfer2 = GUILayout.BeginScrollView(scrollOreTransfer2);
							GUILayout.Label("Select Destination Facility", LabelInfo);
							GUILayout.Label("Select Destination Facility", LabelInfo);
							GUILayout.Label("Select Destination Facility", LabelInfo);
							GUILayout.EndScrollView();

							GUILayout.BeginHorizontal();
							GUILayout.Label("Amount: ", LabelInfo);
							sOreTransferAmount = GUILayout.TextField(sOreTransferAmount, 7, GUILayout.Width(120));
							if (GUILayout.Button("Max", GUILayout.Height(23)))
							{

							}
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							GUILayout.Label("Transfer Cost: X Funds");
							if (GUILayout.Button("Proceed", GUILayout.Height(23)))
							{

							}
							GUILayout.EndHorizontal();
							GUILayout.FlexibleSpace();
						}

						if (GUILayout.Button("Assign a Kerbonaut Engineer", ButtonSmallText, GUILayout.Height(20)))
						{ }
					} */

					GUILayout.Space(5);
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("Production Rate: Up to " + fProductionRate.ToString("#0.00") + " per 12 hrs", LabelInfo);
						GUILayout.FlexibleSpace();
						//if (GUILayout.Button(" Upgrade ", ButtonSmallText, GUILayout.Height(20)))
						//{ }
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(3);
				}

				fLqFMax = (float)selectedFacility.model.getSetting("LqFMax");
				fOxFMax = (float)selectedFacility.model.getSetting("OxFMax");
				fMoFMax = (float)selectedFacility.model.getSetting("MoFMax");

				if (fLqFMax > 0 || fOxFMax > 0 || fMoFMax > 0)
				{
					FuelTanksGUI.FuelTanksInterface(selectedFacility);
				}

				GUI.enabled = true;

				GUILayout.Space(2);
				GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
				GUILayout.Space(2);

				GUI.enabled = isOpen2;
				StaffGUI.StaffingInterface(selectedFacility);
				GUI.enabled = true;
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

		public static Boolean HangarwayIsClear(StaticObject soHangar)
		{
			Boolean bIsClear = true;

			foreach (Vessel vVessel in FlightGlobals.Vessels)
			{
				if (vVessel == null) continue;
				if (!vVessel.loaded) continue;
				if (vVessel.vesselType == VesselType.EVA) continue;
				if (vVessel.vesselType == VesselType.Flag) continue;
				if (vVessel.situation != Vessel.Situations.LANDED) continue;

				var vDistToCraft = Vector3.Distance(vVessel.gameObject.transform.position, soHangar.gameObject.transform.position);
				if (vDistToCraft > 260) continue;
				else
					bIsClear = false;
			}

			return bIsClear;
		}

		public static void HangarCraft(Vessel vVessel, StaticObject soHangar, int iMax = 2)
		{
			string sSpace = GetHangarSpace(soHangar, iMax);

			if (sSpace == "None")
			{
				MiscUtils.HUDMessage("This facility is full.", 10,
					3);
			}
			else
			{
				string sVesselID = vVessel.id.ToString();
				soHangar.setSetting(sSpace, sVesselID);
				PersistenceUtils.saveStaticPersistence(soHangar);

				// Hangar the vessel - hide it
				foreach (Part p in vVessel.Parts)
				{
					if (p != null && p.gameObject != null)
						p.gameObject.SetActive(false);
					else
						continue;
				}

				vVessel.MakeInactive();
				vVessel.enabled = false;
				vVessel.Unload();
			}
		}

		public static string GetHangarSpace(StaticObject soHangar, int iMax = 2)
		{
			string sSpace = "None";

			if ((string)soHangar.getSetting("InStorage") == "None")
			{
				sSpace = "InStorage";
				if (iMax < 2) return sSpace;
			}
			else
				if ((string)soHangar.getSetting("TargetID") == "None")
				{
					sSpace = "TargetID";
					if (iMax == 2) return sSpace;
				}
				else
					if ((string)soHangar.getSetting("TargetType") == "None")
						sSpace = "TargetType";

			return sSpace;
		}

		public static int NumberCraftHangared(StaticObject soHangar)
		{
			int iNumber = 0;

			if ((string)soHangar.getSetting("InStorage") != "None") iNumber = iNumber + 1;
			if ((string)soHangar.getSetting("TargetID") != "None") iNumber = iNumber + 1;
			if ((string)soHangar.getSetting("TargetType") != "None") iNumber = iNumber + 1;
			
			return iNumber;
		}

		public static void RemoveCorrectCraft(Vessel vVessel, StaticObject soHangar)
		{
			string sSpace = "InStorage";
			string sVesselID = vVessel.id.ToString();

			if (sVesselID == (string)soHangar.getSetting("TargetID"))
				sSpace = "TargetID";
			if (sVesselID == (string)soHangar.getSetting("TargetType"))
				sSpace = "TargetType";

			selectedFacility.setSetting(sSpace, "None");
			PersistenceUtils.saveStaticPersistence(selectedFacility);
		}

		public static void UnhangarCraft(Vessel vVesselStored, StaticObject soHangar)
		{
			RemoveCorrectCraft(vVesselStored, soHangar);

			// Convert the stored protovessel to a new protovessel.
			// Use BackupVessel because that seems to work and protovessel does not. `\o/`
			ProtoVessel pVessel = vVesselStored.BackupVessel();

			// Get rid of the original hidden vessel - even though it was unloaded KSP still 'sees' the original craft.
			// I do not care why. :|
			vVesselStored.state = Vessel.State.DEAD;

			foreach (Part p in vVesselStored.Parts)
			{
				if (p != null && p.gameObject != null)
					p.gameObject.DestroyGameObject();
			}

			// Load the new protovessel we made. KSP won't reload (or rather render) a vessel that was unloaded - it will only load a protovessel. :$
			pVessel.Load(FlightDriver.FlightStateCache.flightState);
			// I suspect this is actually a KSP bug since the same crap happens with newly spawned static objects. If you query the active state
			// of the invisible craft, it claims it is active. And no, forcing the renderers doesn't work either.

			// Convert protovessel to vessel
			Vessel vNewVessel = pVessel.vesselRef;

			// Unload then reload the vessel - this seems to be the way to properly re-initialise flightstate etc.
			// Don't do this and you get a craft with a stuck surface velocity reading. It looks like KSP transposes orbital
			// and surface velocity or some other stupid s**t. I don't care.
			// And yes, this time KSP does load an unloaded vessel with no need for protovessel b******t. I don't care why.
			vNewVessel.Unload();
			vNewVessel.Load();
		}
	}
}

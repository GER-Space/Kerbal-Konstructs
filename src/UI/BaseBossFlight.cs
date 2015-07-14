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
		
		Rect managerRect = new Rect(10, 25, 400, 450);
		Rect facilityRect = new Rect(150, 75, 400, 620);
		
		public Boolean managingFacility = false;
		public Boolean foundingBase = false;

		Vector2 scrollPos;
		Vector2 scrollPos3;

		public void drawManager(StaticObject obj)
		{
			if (obj != null)
			{
				if (selectedObject != obj)
					EditorGUI.updateSelection(obj);

				if (managingFacility)
					facilityRect = GUI.Window(0xB00B1E1, facilityRect, drawFacilityManagerWindow, "Base Boss : Facility Manager");
			}

			managerRect = GUI.Window(0xB00B1E2, managerRect, drawBaseManagerWindow, "Base Boss");
		}

		void drawBaseManagerWindow(int windowID)
		{
			string Base;
			float Range;
			LaunchSite lNearest;
			LaunchSite lBase;
			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			GUILayout.BeginArea(new Rect(10, 30, 380, 420));

			GUILayout.Space(3);
			GUILayout.Box("Settings");

			GUILayout.BeginHorizontal();
			{
				KerbalKonstructs.instance.enableATC = GUILayout.Toggle(KerbalKonstructs.instance.enableATC, "Enable ATC", GUILayout.Width(175));
				KerbalKonstructs.instance.enableNGS = GUILayout.Toggle(KerbalKonstructs.instance.enableNGS, "Enable NGS", GUILayout.Width(175));
				KerbalKonstructs.instance.showNGS = (KerbalKonstructs.instance.enableNGS);
			}
			GUILayout.EndHorizontal();

			GUILayout.Box("Base");

			if (MiscUtils.isCareerGame())
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Nearest Open Base: ", GUILayout.Width(100));
					LaunchSiteManager.getNearestOpenBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range, out lNearest);
					GUILayout.Label(Base + " at ", GUILayout.Width(130));
					GUI.enabled = false;
					GUILayout.TextField(" " + Range + " ", GUILayout.Width(75));
					GUI.enabled = true;
					GUILayout.Label("m");
					if (KerbalKonstructs.instance.enableNGS)
					{
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
				GUILayout.Label("Nearest Base: ", GUILayout.Width(100));
				LaunchSiteManager.getNearestBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range, out lBase);
				GUILayout.Label(Base + " at ", GUILayout.Width(130));
				GUI.enabled = false;
				GUILayout.TextField(" " + Range + " ", GUILayout.Width(75));
				GUI.enabled = true;
				GUILayout.Label("m");
				if (KerbalKonstructs.instance.enableNGS)
				{
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
						if (GUILayout.Button("Open Base for " + fOpenCost + " Funds"))
						{
							double currentfunds = Funding.Instance.Funds;

							if (fOpenCost > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this site!", 10, 0);
							}
							else
							{
								// Charge some funds
								Funding.Instance.AddFunds(-fOpenCost, TransactionReasons.Cheating);

								// Open the site - save to instance
								LaunchSiteManager.setSiteOpenCloseState(Base, "Open");
								smessage = Base + " opened";
								ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
							}
						}
					}

					if (bLanded && sClosed == "Open")
					{
						GUI.enabled = false;
						GUILayout.Button("Base is Open");
						GUI.enabled = true;
					}

					GUILayout.Space(2);
				}

				if (Range > 100000)
				{
					if (bLanded)
					{
						if (GUILayout.Button("Found a New Base"))
						{
							foundingBase = true;
						}
					}
				}
			}

			if (FlightGlobals.ActiveVessel.Landed)
			{
				GUILayout.Box("Facilities");

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

						if (isLocal)
						{
							if (GUILayout.Button((string)obj.model.getSetting("title")))
							{
								selectedObject = obj;
								// KerbalKonstructs.instance.selectedObject = obj;
								KerbalKonstructs.instance.selectObject(obj, false);
								PersistenceUtils.loadStaticPersistence(obj);
								managingFacility = true;
							}
						}
					}
				}
				GUILayout.EndScrollView();
			}

			GUILayout.FlexibleSpace();
			GUILayout.Space(10);
			if (GUILayout.Button("I want to race!", GUILayout.Height(22)))
			{
				KerbalKonstructs.instance.showRacing = true;
				AirRacing.runningRace = true;
				KerbalKonstructs.instance.showNGS = false;
				KerbalKonstructs.instance.showBaseManager = false;
			}
			GUILayout.Space(5);

			GUILayout.EndArea();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		void drawFacilityManagerWindow(int windowID)
		{
			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			string sFacilityName = (string)selectedObject.model.getSetting("title");
			string sFacilityRole = (string)selectedObject.model.getSetting("FacilityRole");

			float fStaffMax = (float)selectedObject.model.getSetting("StaffMax");
			float fStaffCurrent = (float)selectedObject.getSetting("StaffCurrent");

			float fScienceOMax = (float)selectedObject.model.getSetting("ScienceOMax");
			float fScienceOCurrent = (float)selectedObject.getSetting("ScienceOCurrent");

			float fFundsOMax = (float)selectedObject.model.getSetting("FundsOMax");
			float fFundsOCurrent = (float)selectedObject.getSetting("FundsOCurrent");

			fLqFMax = (float)selectedObject.model.getSetting("LqFMax");
			fLqFCurrent = (float)selectedObject.getSetting("LqFCurrent");
			fOxFMax = (float)selectedObject.model.getSetting("OxFMax");
			fOxFCurrent = (float)selectedObject.getSetting("OxFCurrent");
			fMoFMax = (float)selectedObject.model.getSetting("MoFMax");
			fMoFCurrent = (float)selectedObject.getSetting("MoFCurrent");

			float fPurchaseRate = fTransferRate * 100f;

			GUILayout.Box(sFacilityName);

			if (!FlightGlobals.ActiveVessel.Landed)
			{
				GUILayout.Box("VESSEL MUST BE LANDED TO USE THIS FACILITY!");
				LockFuelTank();
			}

			var vDist = Vector3.Distance(selectedObject.gameObject.transform.position, FlightGlobals.ActiveVessel.transform.position);

			if ((double)vDist < KerbalKonstructs.instance.facilityUseRange)
			{ }
			else
			{
				GUILayout.Box("VESSEL MUST BE IN RANGE TO USE THIS FACILITY!");
				LockFuelTank();
			}

			if (fLqFMax > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Max LiquidFuel");
				GUI.enabled = false;
				GUILayout.TextField(string.Format("{0}", fLqFMax));
				GUI.enabled = true;
				GUILayout.Label("Current LiquidFuel");
				GUI.enabled = false;
				GUILayout.TextField(fLqFCurrent.ToString("#0.00"));
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Order LiquidFuel"))
				{
					LockFuelTank();
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedLqF = true;
				}
				GUI.enabled = !bLqFIn;
				if (GUILayout.Button("Transfer In"))
				{
					bLqFIn = true;
					bLqFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = !bLqFOut;
				if (GUILayout.Button("Transfer Out"))
				{
					bLqFOut = true;
					bLqFIn = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = bLqFIn || bLqFOut;
				if (GUILayout.Button("Stop"))
				{
					bLqFIn = false;
					bLqFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer stopped";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (bOrderedLqF)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.RepeatButton("-"))
				{
					fLqFAmount = (float.Parse(fLqFAmount) - fPurchaseRate).ToString();
					if ((float.Parse(fLqFAmount)) < 0f) fLqFAmount = "0.00";
				}
				GUILayout.TextField(fLqFAmount);
				if (GUILayout.RepeatButton("+"))
				{
					fLqFAmount = (float.Parse(fLqFAmount) + fPurchaseRate).ToString();
					if ((float.Parse(fLqFAmount)) > (fLqFMax - fLqFCurrent)) fLqFAmount = (fLqFMax - fLqFCurrent).ToString();
				}

				if (GUILayout.Button("Max"))
				{
					fLqFAmount = (fLqFMax - fLqFCurrent).ToString();
					if ((float.Parse(fLqFAmount)) < 0f) fLqFAmount = "0.00";
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}

				float flqFPrice = 0.5f;

				float fLqFCost = (float.Parse(fLqFAmount)) * flqFPrice;
				GUILayout.Label("Cost: " + fLqFCost + " \\F");
				if (GUILayout.Button("Buy"))
				{
					if ((float)selectedObject.getSetting("LqFCurrent") + (float.Parse(fLqFAmount)) > fLqFMax)
					{
						ScreenMessages.PostScreenMessage("Insufficient fuel capacity!", 10, 0);
						fLqFAmount = "0.00";
					}
					else
					{
						if (MiscUtils.isCareerGame())
						{
							double currentfunds = Funding.Instance.Funds;

							if (fLqFCost > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds!", 10, 0);
							}
							else
							{
								Funding.Instance.AddFunds(-fLqFCost, TransactionReasons.Cheating);
								selectedObject.setSetting("LqFCurrent", (float)selectedObject.getSetting("LqFCurrent") + (float.Parse(fLqFAmount)));
							}
						}
						else
						{
							selectedObject.setSetting("LqFCurrent", (float)selectedObject.getSetting("LqFCurrent") + (float.Parse(fLqFAmount)));
						}
					}

					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				if (GUILayout.Button("Done"))
				{
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedLqF = false;
				}
				GUILayout.EndHorizontal();
			}

			if (fOxFMax > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Max Oxidizer");
				GUI.enabled = false;
				GUILayout.TextField(string.Format("{0}", fOxFMax));
				GUI.enabled = true;
				GUILayout.Label("Current Oxidizer");
				GUI.enabled = false;
				GUILayout.TextField(fOxFCurrent.ToString("#0.00"));
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Order Oxidizer"))
				{
					LockFuelTank();
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedOxF = true;
				}
				GUI.enabled = !bOxFIn;
				if (GUILayout.Button("Transfer In"))
				{
					bOxFIn = true;
					bOxFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = !bOxFOut;
				if (GUILayout.Button("Transfer Out"))
				{
					bOxFOut = true;
					bOxFIn = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = bOxFIn || bOxFOut;
				if (GUILayout.Button("Stop"))
				{
					bOxFIn = false;
					bOxFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer stopped";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (bOrderedOxF)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.RepeatButton("-"))
				{
					fOxFAmount = (float.Parse(fOxFAmount) - fPurchaseRate).ToString();
					if ((float.Parse(fOxFAmount)) < 0f) fOxFAmount = "0.00";
				}
				GUILayout.TextField(fOxFAmount);
				if (GUILayout.RepeatButton("+"))
				{
					fOxFAmount = (float.Parse(fOxFAmount) + fPurchaseRate).ToString();
					if ((float.Parse(fOxFAmount)) > (fOxFMax - fOxFCurrent)) fOxFAmount = (fOxFMax - fOxFCurrent).ToString();
				}

				if (GUILayout.Button("Max"))
				{
					fOxFAmount = (fOxFMax - fOxFCurrent).ToString();
					if ((float.Parse(fOxFAmount)) < 0f) fOxFAmount = "0.00";
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}

				float fOxFPrice = 1.5f;

				float fOxFCost = (float.Parse(fOxFAmount)) * fOxFPrice;
				GUILayout.Label("Cost: " + fOxFCost + " \\F");
				if (GUILayout.Button("Buy"))
				{
					if ((float)selectedObject.getSetting("OxFCurrent") + (float.Parse(fOxFAmount)) > fOxFMax)
					{
						ScreenMessages.PostScreenMessage("Insufficient fuel capacity!", 10, 0);
						fOxFAmount = "0.00";
					}
					else
					{
						if (MiscUtils.isCareerGame())
						{
							double currentfunds = Funding.Instance.Funds;

							if (fOxFCost > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds!", 10, 0);
							}
							else
							{
								Funding.Instance.AddFunds(-fOxFCost, TransactionReasons.Cheating);
								selectedObject.setSetting("OxFCurrent", (float)selectedObject.getSetting("OxFCurrent") + (float.Parse(fOxFAmount)));
							}
						}
						else
						{
							selectedObject.setSetting("OxFCurrent", (float)selectedObject.getSetting("OxFCurrent") + (float.Parse(fOxFAmount)));
						}
					}

					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				if (GUILayout.Button("Done"))
				{
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedOxF = false;
				}
				GUILayout.EndHorizontal();
			}

			if (fMoFMax > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Max MonoProp.");
				GUI.enabled = false;
				GUILayout.TextField(string.Format("{0}", fMoFMax));
				GUI.enabled = true;
				GUILayout.Label("Current MonoProp.");
				GUI.enabled = false;
				GUILayout.TextField(fMoFCurrent.ToString("#0.00"));
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Order MonoProp."))
				{
					LockFuelTank();
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedMoF = true;
				}
				GUI.enabled = !bMoFIn;
				if (GUILayout.Button("Transfer In"))
				{
					bMoFIn = true;
					bMoFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = !bMoFOut;
				if (GUILayout.Button("Transfer Out"))
				{
					bMoFOut = true;
					bMoFIn = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = bMoFIn || bMoFOut;
				if (GUILayout.Button("Stop"))
				{
					bMoFIn = false;
					bMoFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer stopped";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (bOrderedMoF)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.RepeatButton("-"))
				{
					fMoFAmount = (float.Parse(fMoFAmount) - fPurchaseRate).ToString();
					if ((float.Parse(fMoFAmount)) < 0f) fMoFAmount = "0.00";
				}
				GUILayout.TextField(fMoFAmount);
				if (GUILayout.RepeatButton("+"))
				{
					fMoFAmount = (float.Parse(fMoFAmount) + fPurchaseRate).ToString();
					if ((float.Parse(fMoFAmount)) > (fMoFMax - fMoFCurrent)) fMoFAmount = (fMoFMax - fMoFCurrent).ToString();
				}

				if (GUILayout.Button("Max"))
				{
					fMoFAmount = (fMoFMax - fMoFCurrent).ToString();
					if ((float.Parse(fMoFAmount)) < 0f) fMoFAmount = "0.00";
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}

				float fMoFPrice = 1.2f;

				float fMoFCost = (float.Parse(fMoFAmount)) * fMoFPrice;
				GUILayout.Label("Cost: " + fMoFCost + " \\F");
				if (GUILayout.Button("Buy"))
				{
					if ((float)selectedObject.getSetting("MoFCurrent") + (float.Parse(fMoFAmount)) > fMoFMax)
					{
						ScreenMessages.PostScreenMessage("Insufficient fuel capacity!", 10, 0);
						fMoFAmount = "0.00";
					}
					else
					{
						if (MiscUtils.isCareerGame())
						{
							double currentfunds = Funding.Instance.Funds;

							if (fMoFCost > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds!", 10, 0);
							}
							else
							{
								Funding.Instance.AddFunds(-fMoFCost, TransactionReasons.Cheating);
								selectedObject.setSetting("MoFCurrent", (float)selectedObject.getSetting("MoFCurrent") + (float.Parse(fMoFAmount)));
							}
						}
						else
						{
							selectedObject.setSetting("MoFCurrent", (float)selectedObject.getSetting("MoFCurrent") + (float.Parse(fMoFAmount)));
						}
					}

					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				if (GUILayout.Button("Done"))
				{
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedMoF = false;
				}
				GUILayout.EndHorizontal();
			}

			if (fOxFMax > 0 || fLqFMax > 0 || fMoFMax > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Active Vessel");
				GUILayout.Box("" + FlightGlobals.ActiveVessel.vesselName, GUILayout.Height(25));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Transfer Rate");

				GUI.enabled = (fTransferRate != 0.01f);
				if (GUILayout.Button("x1"))
				{
					fTransferRate = 0.01f;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer rate set to x1";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUI.enabled = (fTransferRate != 0.04f);
				if (GUILayout.Button("x4"))
				{
					fTransferRate = 0.04f;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer rate set to x4";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUI.enabled = (fTransferRate != 0.1f);
				if (GUILayout.Button("x10"))
				{
					fTransferRate = 0.1f;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer rate set to x10";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				if (!FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.Landed)
				{
					GUILayout.Label("Vessel's Tanks");

					scrollPos3 = GUILayout.BeginScrollView(scrollPos3);
					foreach (Part fTank in FlightGlobals.ActiveVessel.parts)
					{
						foreach (PartResource rResource in fTank.Resources)
						{
							if (rResource.resourceName == "LiquidFuel" || rResource.resourceName == "Oxidizer" || rResource.resourceName == "MonoPropellant")
							{
								if (SelectedTank == fTank && SelectedResource == rResource)
									PartSelected = true;
								else
									PartSelected = false;

								GUILayout.BeginHorizontal();
								GUILayout.Box("" + fTank.gameObject.name);
								GUILayout.Box("" + rResource.resourceName);
								GUILayout.EndHorizontal();

								GUILayout.BeginHorizontal();
								GUILayout.Label("Fuel");
								GUILayout.TextField("" + rResource.amount.ToString("#0.00"));

								GUI.enabled = !PartSelected;
								if (GUILayout.Button("Select"))
								{
									SelectedResource = rResource;
									SelectedTank = fTank;
									PersistenceUtils.saveStaticPersistence(selectedObject);
								}

								GUI.enabled = PartSelected;
								if (GUILayout.Button("Deselect"))
								{
									SelectedResource = null;
									SelectedTank = null;
									PersistenceUtils.saveStaticPersistence(selectedObject);
								}
								GUI.enabled = true;
								GUILayout.EndHorizontal();
							}
							else
								continue;
						}
					}
					GUILayout.EndScrollView();

					GUI.enabled = true;

					if (SelectedResource != null && SelectedTank != null)
					{
						// Debug.Log("KK: doFuelIn or doFuelOut");
						if (bMoFOut || bOxFOut || bLqFOut)
							doFuelOut();
						if (bMoFIn || bOxFIn || bLqFIn)
							doFuelIn();
					}
				}
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Close"))
			{
				LockFuelTank();
				managingFacility = false;
				// Debug.Log("KK: saveStaticPersistence");
				PersistenceUtils.saveStaticPersistence(selectedObject);
				//KerbalKonstructs.instance.selectedObject = null;
				KerbalKonstructs.instance.deselectObject();
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		#region Fuel Tanks
		// Working facilities handling
		Boolean bLqFIn = false;
		Boolean bLqFOut = false;
		Boolean bOxFIn = false;
		Boolean bOxFOut = false;
		Boolean bMoFIn = false;
		Boolean bMoFOut = false;
		Boolean PartSelected = false;

		Boolean bOrderedLqF = false;
		Boolean bOrderedOxF = false;
		Boolean bOrderedMoF = false;

		public PartResource SelectedResource = null;
		public Part SelectedTank = null;

		float fLqFMax = 0;
		float fLqFCurrent = 0;
		float fOxFMax = 0;
		float fOxFCurrent = 0;
		float fMoFMax = 0;
		float fMoFCurrent = 0;

		float fTransferRate = 0.01f;

		string fOxFAmount = "0.00";
		string fLqFAmount = "0.00";
		string fMoFAmount = "0.00";

		void doFuelOut()
		{
			if (SelectedResource == null) return;
			if (SelectedTank == null) return;

			// Debug.Log("KK: doFuelOut " + SelectedResource.resourceName);

			if (SelectedResource.resourceName == "MonoPropellant" && !bMoFOut) return;
			if (SelectedResource.resourceName == "LiquidFuel" && !bLqFOut) return;
			if (SelectedResource.resourceName == "Oxidizer" && !bOxFOut) return;

			if (SelectedResource.resourceName == "MonoPropellant" && fMoFCurrent <= 0) return;
			if (SelectedResource.resourceName == "LiquidFuel" && fLqFCurrent <= 0) return;
			if (SelectedResource.resourceName == "Oxidizer" && fOxFCurrent <= 0) return;

			if (SelectedResource.amount >= SelectedResource.maxAmount) return;

			float dStaticFuel;

			// Debug.Log("KK: doFuelOut " + SelectedResource.resourceName);

			SelectedResource.amount = SelectedResource.amount + fTransferRate;
			if (SelectedResource.amount > SelectedResource.maxAmount) SelectedResource.amount = SelectedResource.maxAmount;

			if (SelectedResource.resourceName == "MonoPropellant")
			{
				dStaticFuel = ((float)selectedObject.getSetting("MoFCurrent")) - fTransferRate;
				if (dStaticFuel < 0) dStaticFuel = 0;
				selectedObject.setSetting("MoFCurrent", dStaticFuel);
			}
			if (SelectedResource.resourceName == "LiquidFuel")
			{
				dStaticFuel = ((float)selectedObject.getSetting("LqFCurrent")) - fTransferRate;
				if (dStaticFuel < 0) dStaticFuel = 0;
				selectedObject.setSetting("LqFCurrent", dStaticFuel);
			}
			if (SelectedResource.resourceName == "Oxidizer")
			{
				dStaticFuel = ((float)selectedObject.getSetting("OxFCurrent")) - fTransferRate;
				if (dStaticFuel < 0) dStaticFuel = 0;
				selectedObject.setSetting("OxFCurrent", dStaticFuel);
			}
		}

		void doFuelIn()
		{
			if (SelectedResource == null) return;
			if (SelectedTank == null) return;

			// Debug.Log("KK: doFuelIn " + SelectedResource.resourceName);

			if (SelectedResource.resourceName == "MonoPropellant" && !bMoFIn) return;
			if (SelectedResource.resourceName == "LiquidFuel" && !bLqFIn) return;
			if (SelectedResource.resourceName == "Oxidizer" && !bOxFIn) return;

			if (SelectedResource.resourceName == "MonoPropellant" && fMoFCurrent >= fMoFMax) return;
			if (SelectedResource.resourceName == "LiquidFuel" && fLqFCurrent >= fLqFMax) return;
			if (SelectedResource.resourceName == "Oxidizer" && fOxFCurrent >= fOxFMax) return;

			if (SelectedResource.amount <= 0) return;

			// Debug.Log("KK: doFuelIn " + SelectedResource.amount);

			float dStaticFuel;

			SelectedResource.amount = SelectedResource.amount - fTransferRate;
			if (SelectedResource.amount < 0) SelectedResource.amount = 0;

			if (SelectedResource.resourceName == "MonoPropellant")
			{
				dStaticFuel = ((float)selectedObject.getSetting("MoFCurrent")) + fTransferRate;
				if (dStaticFuel > fMoFMax) dStaticFuel = fMoFMax;
				selectedObject.setSetting("MoFCurrent", dStaticFuel);
			}
			if (SelectedResource.resourceName == "LiquidFuel")
			{
				dStaticFuel = ((float)selectedObject.getSetting("LqFCurrent")) + fTransferRate;
				if (dStaticFuel > fLqFMax) dStaticFuel = fLqFMax;
				selectedObject.setSetting("LqFCurrent", dStaticFuel);
			}
			if (SelectedResource.resourceName == "Oxidizer")
			{
				dStaticFuel = ((float)selectedObject.getSetting("OxFCurrent")) + fTransferRate;
				if (dStaticFuel > fOxFMax) dStaticFuel = fOxFMax;
				selectedObject.setSetting("OxFCurrent", dStaticFuel);
			}
		}

		void LockFuelTank()
		{
			SelectedResource = null;
			SelectedTank = null;
			bLqFIn = false;
			bLqFOut = false;
			bOxFIn = false;
			bOxFOut = false;
			bMoFIn = false;
			bMoFOut = false;
		}
		#endregion
	}
}

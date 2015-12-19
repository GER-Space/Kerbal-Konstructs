using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class FuelTanksGUI
	{
		public static Boolean bLqFIn = false;
		public static Boolean bLqFOut = false;
		public static Boolean bOxFIn = false;
		public static Boolean bOxFOut = false;
		public static Boolean bMoFIn = false;
		public static Boolean bMoFOut = false;
		public static Boolean PartSelected = false;

		public static Boolean bOrderedLqF = false;
		public static Boolean bOrderedOxF = false;
		public static Boolean bOrderedMoF = false;

		public static PartResource SelectedResource = null;
		public static Part SelectedTank = null;

		public static float fLqFMax = 0;
		public static float fLqFCurrent = 0;
		public static float fOxFMax = 0;
		public static float fOxFCurrent = 0;
		public static float fMoFMax = 0;
		public static float fMoFCurrent = 0;

		public static float fTransferRate = 0.01f;

		public static string fOxFAmount = "0.00";
		public static string fLqFAmount = "0.00";
		public static string fMoFAmount = "0.00";

		public static Vector2 scrollPos3;
		public static Vector2 scrollPos4;
		public static GUIStyle LabelInfo;
		public static GUIStyle BoxInfo;

		public static void FuelTanksInterface(StaticObject selectedObject)
		{
			string smessage = "";

			string sFacilityName = (string)selectedObject.model.getSetting("title");
			string sFacilityRole = (string)selectedObject.getSetting("FacilityType");

			fLqFMax = (float)selectedObject.model.getSetting("LqFMax");
			fLqFCurrent = (float)selectedObject.getSetting("LqFCurrent");
			fOxFMax = (float)selectedObject.model.getSetting("OxFMax");
			fOxFCurrent = (float)selectedObject.getSetting("OxFCurrent");
			fMoFMax = (float)selectedObject.model.getSetting("MoFMax");
			fMoFCurrent = (float)selectedObject.getSetting("MoFCurrent");

			float fPurchaseRate = fTransferRate * 100f;

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

			if (!FlightGlobals.ActiveVessel.Landed)
			{
				GUILayout.Box("A vessel must be landed to use this facility.", BoxInfo);
				LockFuelTank();
			}

			var vDist = Vector3.Distance(selectedObject.gameObject.transform.position, FlightGlobals.ActiveVessel.transform.position);

			if ((double)vDist < KerbalKonstructs.instance.facilityUseRange)
			{ }
			else
			{
				GUILayout.Box("A vessel must be in range to use this facility.", BoxInfo);
				LockFuelTank();
			}

			GUILayout.Space(3);
			GUILayout.Label("Fuel Stores", LabelInfo);
			scrollPos4 = GUILayout.BeginScrollView(scrollPos4);
			if (fLqFMax > 0)
			{
				GUILayout.Label("LiquidFuel", LabelInfo);
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Max ", LabelInfo);
				GUI.enabled = false;
				GUILayout.TextField(string.Format("{0}", fLqFMax), GUILayout.Height(18));
				GUI.enabled = true;
				GUILayout.Label("Current ", LabelInfo);
				GUI.enabled = false;
				GUILayout.TextField(fLqFCurrent.ToString("#0.00"), GUILayout.Height(18));
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Order", GUILayout.Height(18)))
				{
					LockFuelTank();
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedLqF = true;
				}
				GUI.enabled = !bLqFIn;
				if (GUILayout.Button("In", GUILayout.Height(18)))
				{
					bLqFIn = true;
					bLqFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = !bLqFOut;
				if (GUILayout.Button("Out", GUILayout.Height(18)))
				{
					bLqFOut = true;
					bLqFIn = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = bLqFIn || bLqFOut;
				if (GUILayout.Button("Stop", GUILayout.Height(18)))
				{
					bLqFIn = false;
					bLqFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer stopped";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (bOrderedLqF)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.RepeatButton("-", GUILayout.Height(18)))
				{
					fLqFAmount = (float.Parse(fLqFAmount) - fPurchaseRate).ToString();
					if ((float.Parse(fLqFAmount)) < 0f) fLqFAmount = "0.00";
				}
				GUI.enabled = false;
				GUILayout.TextField(fLqFAmount, GUILayout.Height(18));
				GUI.enabled = true;
				if (GUILayout.RepeatButton("+", GUILayout.Height(18)))
				{
					fLqFAmount = (float.Parse(fLqFAmount) + fPurchaseRate).ToString();
					if ((float.Parse(fLqFAmount)) > (fLqFMax - fLqFCurrent)) fLqFAmount = (fLqFMax - fLqFCurrent).ToString();
				}

				if (GUILayout.Button("Max", GUILayout.Height(18)))
				{
					fLqFAmount = (fLqFMax - fLqFCurrent).ToString();
					if ((float.Parse(fLqFAmount)) < 0f) fLqFAmount = "0.00";
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}

				float flqFPrice = 0.5f;

				float fLqFCost = (float.Parse(fLqFAmount)) * flqFPrice;
				GUILayout.Label("Cost: " + fLqFCost.ToString("#0") + " \\F", LabelInfo);
				if (GUILayout.Button("Buy", GUILayout.Height(18)))
				{
					if ((float)selectedObject.getSetting("LqFCurrent") + (float.Parse(fLqFAmount)) > fLqFMax)
					{
						MiscUtils.HUDMessage("Insufficient fuel capacity!", 10, 0);
						fLqFAmount = "0.00";
					}
					else
					{
						if (MiscUtils.isCareerGame())
						{
							double currentfunds = Funding.Instance.Funds;

							if (fLqFCost > currentfunds)
							{
								MiscUtils.HUDMessage("Insufficient funds!", 10, 0);
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
				if (GUILayout.Button("Done", GUILayout.Height(18)))
				{
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedLqF = false;
				}
				GUILayout.EndHorizontal();
			}

			if (fOxFMax > 0)
			{
				GUILayout.Label("Oxidizer", LabelInfo);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Max ", LabelInfo);
				GUI.enabled = false;
				GUILayout.TextField(string.Format("{0}", fOxFMax), GUILayout.Height(18));
				GUI.enabled = true;
				GUILayout.Label("Current ", LabelInfo);
				GUI.enabled = false;
				GUILayout.TextField(fOxFCurrent.ToString("#0.00"), GUILayout.Height(18));
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Order", GUILayout.Height(18)))
				{
					LockFuelTank();
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedOxF = true;
				}
				GUI.enabled = !bOxFIn;
				if (GUILayout.Button("In", GUILayout.Height(18)))
				{
					bOxFIn = true;
					bOxFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = !bOxFOut;
				if (GUILayout.Button("Out", GUILayout.Height(18)))
				{
					bOxFOut = true;
					bOxFIn = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = bOxFIn || bOxFOut;
				if (GUILayout.Button("Stop", GUILayout.Height(18)))
				{
					bOxFIn = false;
					bOxFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer stopped";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (bOrderedOxF)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.RepeatButton("-", GUILayout.Height(18)))
				{
					fOxFAmount = (float.Parse(fOxFAmount) - fPurchaseRate).ToString();
					if ((float.Parse(fOxFAmount)) < 0f) fOxFAmount = "0.00";
				}
				GUI.enabled = false;
				GUILayout.TextField(fOxFAmount, GUILayout.Height(18));
				GUI.enabled = true;
				if (GUILayout.RepeatButton("+", GUILayout.Height(18)))
				{
					fOxFAmount = (float.Parse(fOxFAmount) + fPurchaseRate).ToString();
					if ((float.Parse(fOxFAmount)) > (fOxFMax - fOxFCurrent)) fOxFAmount = (fOxFMax - fOxFCurrent).ToString();
				}

				if (GUILayout.Button("Max", GUILayout.Height(18)))
				{
					fOxFAmount = (fOxFMax - fOxFCurrent).ToString();
					if ((float.Parse(fOxFAmount)) < 0f) fOxFAmount = "0.00";
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}

				float fOxFPrice = 1.5f;

				float fOxFCost = (float.Parse(fOxFAmount)) * fOxFPrice;
				GUILayout.Label("Cost: " + fOxFCost.ToString("#0") + " \\F", LabelInfo);
				if (GUILayout.Button("Buy", GUILayout.Height(18)))
				{
					if ((float)selectedObject.getSetting("OxFCurrent") + (float.Parse(fOxFAmount)) > fOxFMax)
					{
						MiscUtils.HUDMessage("Insufficient fuel capacity!", 10, 0);
						fOxFAmount = "0.00";
					}
					else
					{
						if (MiscUtils.isCareerGame())
						{
							double currentfunds = Funding.Instance.Funds;

							if (fOxFCost > currentfunds)
							{
								MiscUtils.HUDMessage("Insufficient funds!", 10, 0);
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
				if (GUILayout.Button("Done", GUILayout.Height(18)))
				{
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedOxF = false;
				}
				GUILayout.EndHorizontal();
			}

			if (fMoFMax > 0)
			{
				GUILayout.Label("Monopropellant", LabelInfo);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Max ", LabelInfo);
				GUI.enabled = false;
				GUILayout.TextField(string.Format("{0}", fMoFMax), GUILayout.Height(18));
				GUI.enabled = true;
				GUILayout.Label("Current ", LabelInfo);
				GUI.enabled = false;
				GUILayout.TextField(fMoFCurrent.ToString("#0.00"), GUILayout.Height(18));
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Order", GUILayout.Height(18)))
				{
					LockFuelTank();
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedMoF = true;
				}
				GUI.enabled = !bMoFIn;
				if (GUILayout.Button("In", GUILayout.Height(18)))
				{
					bMoFIn = true;
					bMoFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = !bMoFOut;
				if (GUILayout.Button("Out", GUILayout.Height(18)))
				{
					bMoFOut = true;
					bMoFIn = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}
				GUI.enabled = bMoFIn || bMoFOut;
				if (GUILayout.Button("Stop", GUILayout.Height(18)))
				{
					bMoFIn = false;
					bMoFOut = false;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer stopped";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (bOrderedMoF)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.RepeatButton("-", GUILayout.Height(18)))
				{
					fMoFAmount = (float.Parse(fMoFAmount) - fPurchaseRate).ToString();
					if ((float.Parse(fMoFAmount)) < 0f) fMoFAmount = "0.00";
				}
				GUI.enabled = false;
				GUILayout.TextField(fMoFAmount, GUILayout.Height(18));
				GUI.enabled = true;
				if (GUILayout.RepeatButton("+", GUILayout.Height(18)))
				{
					fMoFAmount = (float.Parse(fMoFAmount) + fPurchaseRate).ToString();
					if ((float.Parse(fMoFAmount)) > (fMoFMax - fMoFCurrent)) fMoFAmount = (fMoFMax - fMoFCurrent).ToString();
				}

				if (GUILayout.Button("Max", GUILayout.Height(18)))
				{
					fMoFAmount = (fMoFMax - fMoFCurrent).ToString();
					if ((float.Parse(fMoFAmount)) < 0f) fMoFAmount = "0.00";
					PersistenceUtils.saveStaticPersistence(selectedObject);
				}

				float fMoFPrice = 1.2f;

				float fMoFCost = (float.Parse(fMoFAmount)) * fMoFPrice;
				GUILayout.Label("Cost: " + fMoFCost.ToString("#0") + " \\F", LabelInfo);
				if (GUILayout.Button("Buy", GUILayout.Height(18)))
				{
					if ((float)selectedObject.getSetting("MoFCurrent") + (float.Parse(fMoFAmount)) > fMoFMax)
					{
						MiscUtils.HUDMessage("Insufficient fuel capacity!", 10, 0);
						fMoFAmount = "0.00";
					}
					else
					{
						if (MiscUtils.isCareerGame())
						{
							double currentfunds = Funding.Instance.Funds;

							if (fMoFCost > currentfunds)
							{
								MiscUtils.HUDMessage("Insufficient funds!", 10, 0);
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
				if (GUILayout.Button("Done", GUILayout.Height(18)))
				{
					PersistenceUtils.saveStaticPersistence(selectedObject);
					bOrderedMoF = false;
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

			if (fOxFMax > 0 || fLqFMax > 0 || fMoFMax > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Transfer Rate", LabelInfo);

				GUI.enabled = (fTransferRate != 0.01f);
				if (GUILayout.Button(" x1", GUILayout.Height(18)))
				{
					fTransferRate = 0.01f;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer rate set to x1";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = (fTransferRate != 0.04f);
				if (GUILayout.Button(" x4", GUILayout.Height(18)))
				{
					fTransferRate = 0.04f;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer rate set to x4";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = (fTransferRate != 0.1f);
				if (GUILayout.Button("x10", GUILayout.Height(18)))
				{
					fTransferRate = 0.1f;
					PersistenceUtils.saveStaticPersistence(selectedObject);
					smessage = "Fuel transfer rate set to x10";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				if (!FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.Landed)
				{
					GUILayout.Label(FlightGlobals.ActiveVessel.vesselName + "'s Tanks", LabelInfo);

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
								GUILayout.Box("" + fTank.gameObject.name, GUILayout.Height(18));
								GUILayout.Box("" + rResource.resourceName, GUILayout.Height(18));
								GUILayout.EndHorizontal();

								GUILayout.BeginHorizontal();
								GUILayout.Label("Fuel", LabelInfo);
								GUI.enabled = false;
								GUILayout.TextField("" + rResource.amount.ToString("#0.00"), GUILayout.Height(18));
								GUI.enabled = true;

								GUI.enabled = !PartSelected;
								if (GUILayout.Button(" Select ", GUILayout.Height(18)))
								{
									SelectedResource = rResource;
									SelectedTank = fTank;
									PersistenceUtils.saveStaticPersistence(selectedObject);
								}

								GUI.enabled = PartSelected;
								if (GUILayout.Button("Deselect", GUILayout.Height(18)))
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
						if (bMoFOut || bOxFOut || bLqFOut)
							doFuelOut(selectedObject);
						if (bMoFIn || bOxFIn || bLqFIn)
							doFuelIn(selectedObject);
					}
				}
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static void doFuelOut(StaticObject selectedObject)
		{
			if (SelectedResource == null) return;
			if (SelectedTank == null) return;

			if (SelectedResource.resourceName == "MonoPropellant" && !bMoFOut) return;
			if (SelectedResource.resourceName == "LiquidFuel" && !bLqFOut) return;
			if (SelectedResource.resourceName == "Oxidizer" && !bOxFOut) return;

			if (SelectedResource.resourceName == "MonoPropellant" && fMoFCurrent <= 0) return;
			if (SelectedResource.resourceName == "LiquidFuel" && fLqFCurrent <= 0) return;
			if (SelectedResource.resourceName == "Oxidizer" && fOxFCurrent <= 0) return;

			if (SelectedResource.amount >= SelectedResource.maxAmount) return;

			float dStaticFuel;

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

		public static void doFuelIn(StaticObject selectedObject)
		{
			if (SelectedResource == null) return;
			if (SelectedTank == null) return;

			if (SelectedResource.resourceName == "MonoPropellant" && !bMoFIn) return;
			if (SelectedResource.resourceName == "LiquidFuel" && !bLqFIn) return;
			if (SelectedResource.resourceName == "Oxidizer" && !bOxFIn) return;

			if (SelectedResource.resourceName == "MonoPropellant" && fMoFCurrent >= fMoFMax) return;
			if (SelectedResource.resourceName == "LiquidFuel" && fLqFCurrent >= fLqFMax) return;
			if (SelectedResource.resourceName == "Oxidizer" && fOxFCurrent >= fOxFMax) return;

			if (SelectedResource.amount <= 0) return;

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

		public static void LockFuelTank()
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
	}
}

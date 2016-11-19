using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class ProductionGUI
	{
		public static GUIStyle Yellowtext;
		public static GUIStyle KKWindow;
		public static GUIStyle DeadButton;
		public static GUIStyle DeadButtonRed;
		public static GUIStyle BoxNoBorder;
		public static GUIStyle LabelInfo;
		public static GUIStyle ButtonSmallText;

		public static void ProductionInterface(StaticObject selectedFacility, string sFacilityType)
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

			float fStaffing = 0;
			float fProductionRate = 0;
			float fLastCheck = 0;

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
		}
	}
}

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

				selectedFacility.setSetting("ProductionRateCurrent", fDefaultRate);
				fProductionRate = fDefaultRate * (fStaffing / 2f);
			}

			fLastCheck = (float)selectedFacility.getSetting("LastCheck");

			if (fLastCheck == 0)
			{
				fLastCheck = (float)Planetarium.GetUniversalTime();
				selectedFacility.setSetting("LastCheck", fLastCheck);
			}

			if (sFacilityType == "Research" || sFacilityType == "Business")
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
						fMax = (float)selectedFacility.model.defaultScienceOMax;

						if (fMax < 1) fMax = 10f;

						selectedFacility.setSetting("ScienceOMax", fMax);
					}

					fCurrent = (float)selectedFacility.getSetting("ScienceOCurrent");
				}
				if (sFacilityType == "Business")
				{
					sProduces = "Funds";
					fMax = (float)selectedFacility.getSetting("FundsOMax");

					if (fMax < 1)
					{
						fMax = (float)selectedFacility.model.defaultFundsOMax;

						if (fMax < 1) fMax = 10000f;

						selectedFacility.setSetting("FundsOMax", fMax);
					}

					fCurrent = (float)selectedFacility.getSetting("FundsOCurrent");
				}				

				double dTime = Planetarium.GetUniversalTime();

				// Deal with revert exploits
				if (fLastCheck > (float)dTime)
				{
					selectedFacility.setSetting("LastCheck", (float)dTime);
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

					selectedFacility.setSetting("LastCheck", (float)dTime);
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label("Produces: " + sProduces, LabelInfo);
				GUILayout.FlexibleSpace();
				GUILayout.Label("Current: " + fCurrent.ToString("#0") + " | Max: " + fMax.ToString("#0"), LabelInfo);
				GUILayout.EndHorizontal();


				if (sFacilityType == "Research")
				{
					if (GUILayout.Button("Transfer Science to KSC R&D", ButtonSmallText, GUILayout.Height(20)))
					{
						ResearchAndDevelopment.Instance.AddScience(fCurrent, TransactionReasons.Cheating);
						selectedFacility.setSetting("ScienceOCurrent", 0f);
					}

				}
				if (sFacilityType == "Business")
				{
					if (GUILayout.Button("Transfer Funds to KSC Account", ButtonSmallText, GUILayout.Height(20)))
					{
						Funding.Instance.AddFunds((double)fCurrent, TransactionReasons.Cheating);
						selectedFacility.setSetting("FundsOCurrent", 0f);
					}
				}				

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

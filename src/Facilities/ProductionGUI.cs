using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;
using System.Reflection;

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

        private static bool isInitialized = false;

        private static Type facField;
        private static Research myResearch;
        private static Business myBusiness;

        private static float currentStaff = 0;
        private static float currentProductionRate = 0;
        private static float lastCheckTime = 0;

        private static float defaultProductionRate;

        private static string produces = "";
        private static float maxProduced = 0f;
        private static float currentProduced = 0f;

        private static float currentTime;
        private static float daysPast;



    internal static void InitializeLayout ()
        {
            isInitialized = true;

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
        }



		public static void ProductionInterface(StaticInstance selectedFacility, string facilityType)
		{
            if (selectedFacility.myFacilities.Count == 0)
                return;


            if (isInitialized == false)
            {
                InitializeLayout();
            }

            if (facilityType == "Research")
            {
                facField = typeof(Research);
            }else
            {
                facField = typeof(Business);
            }
            myResearch = selectedFacility.myFacilities[0] as Research;
            myBusiness = selectedFacility.myFacilities[0] as Business;

            lastCheckTime = myBusiness.LastCheck;

			if (lastCheckTime == 0)
			{
				lastCheckTime = (float)Planetarium.GetUniversalTime();
                myBusiness.LastCheck =  lastCheckTime;
			}

            if (facilityType == "Research" || facilityType == "Business")
			{
				produces = "";
				maxProduced = 0f;
                defaultProductionRate = (float)facField.GetField("ProductionRateCurrent").GetValue(selectedFacility.myFacilities[0]);

                if (facilityType == "Research")
				{
					produces = "Science";
					maxProduced = myResearch.ScienceOMax;

                    if (defaultProductionRate < 0.1f)
                    {
                        defaultProductionRate = 0.1f;
                    }

                    if (maxProduced < 1)
                    {
                        maxProduced = selectedFacility.model.DefaultScienceOMax;

                        if (maxProduced < 1) maxProduced = 10f;
                        {
                            myResearch.ScienceOMax = maxProduced;
                        }
                    }
                    maxProduced = myResearch.ScienceOMax;
                }

				if (facilityType == "Business")
				{
					produces = "Funds";
					maxProduced = myBusiness.FundsOMax;

                    if (defaultProductionRate < 10f)
                    {
                        defaultProductionRate = 10f;
                    }

                    if (maxProduced < 1)
					{
						maxProduced = selectedFacility.model.DefaultFundsOMax;

                        if (maxProduced < 1)
                        {
                            myBusiness.FundsOMax = 10000f;
                        }
					}
                    maxProduced = myBusiness.FundsOMax;
                }

                facField.GetField("ProductionRateCurrent").SetValue(selectedFacility.myFacilities[0], defaultProductionRate);

                currentStaff = myBusiness.StaffCurrent;
                currentProductionRate = defaultProductionRate * currentStaff;

                currentTime = (float)Planetarium.GetUniversalTime();
             //   Log.Normal("Current time: " + currentTime);

				// Deal with revert exploits
				if (lastCheckTime > currentTime)
				{
                    myBusiness.LastCheck = currentTime;
				}

                daysPast = ((currentTime - lastCheckTime) / 21600f);
                currentProduced = daysPast * currentProductionRate;
                
                if (currentProduced > maxProduced)
                {
                    currentProduced = maxProduced;

                }
				GUILayout.BeginHorizontal();
				GUILayout.Label("Produces: " + produces, LabelInfo);
				GUILayout.FlexibleSpace();
				GUILayout.Label("Current: " + currentProduced.ToString("#0") + " | Max: " + maxProduced.ToString("#0"), LabelInfo);
				GUILayout.EndHorizontal();

                if (facilityType == "Research")
				{
					if (GUILayout.Button("Transfer Science to KSC R&D", ButtonSmallText, GUILayout.Height(20)))
					{
						ResearchAndDevelopment.Instance.AddScience(currentProduced, TransactionReasons.Cheating);
                        myResearch.ScienceOCurrent = 0f;
                        myResearch.LastCheck = currentTime;
                    }

				}
				if (facilityType == "Business")
				{
					if (GUILayout.Button("Transfer Funds to KSC Account", ButtonSmallText, GUILayout.Height(20)))
					{
						Funding.Instance.AddFunds(currentProduced, TransactionReasons.Cheating);
                        myBusiness.FundsOCurrent = 0f;
                        myBusiness.LastCheck = currentTime;
                    }
				}				

				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Production Rate: Up to " + currentProductionRate.ToString("#0.00") + " a Kerbin day", LabelInfo);
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

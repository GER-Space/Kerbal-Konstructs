using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class CraftConstructionGUI
	{
		public static GUIStyle Yellowtext;
		public static GUIStyle KKWindow;
		public static GUIStyle DeadButton;
		public static GUIStyle DeadButtonRed;
		public static GUIStyle BoxNoBorder;
		public static GUIStyle LabelInfo;
		public static GUIStyle ButtonSmallText;

		public static void CraftConstructionInterface(StaticObject selectedFacility)
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
		}
	}
}

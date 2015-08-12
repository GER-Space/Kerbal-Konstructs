using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class StaffGUI
	{
		public static float fStaff;
		public static float fMaxStaff;
		public static float fXP;
		public static GUIStyle LabelInfo;
		public static GUIStyle BoxInfo;

		public static Texture tKerbal = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/billeted", false);
		public static Texture tNoKerbal = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/unbilleted", false);
		public static Texture tXPGained = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/xpgained", false);
		public static Texture tXPUngained = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/xpungained", false);

		public static void StaffingInterface(StaticObject selectedFacility)
		{
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

			fStaff = (float)selectedFacility.getSetting("StaffCurrent");
			fMaxStaff = (float)selectedFacility.getSetting("StaffMax");

			if (fMaxStaff < 1)
			{
				fMaxStaff = (float)selectedFacility.model.getSetting("DefaultStaffMax");

				if (fMaxStaff < 1)
				{
					selectedFacility.setSetting("StaffMax", (float)0);
					PersistenceUtils.saveStaticPersistence(selectedFacility);
				}
				else
				{
					selectedFacility.setSetting("StaffMax", (float)fMaxStaff);
					PersistenceUtils.saveStaticPersistence(selectedFacility);
				}
			}

			if (fMaxStaff > 0)
			{
				float fHireFundCost = 5000 * (fStaff * fStaff);
				float fFireRefund = 2500 * ((fStaff-1) * (fStaff-1));
				float fFireRepCost = fStaff;

				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(5);
					GUILayout.Label("Staff: ", LabelInfo, GUILayout.Width(55), GUILayout.Height(38));

					float CountCurrent = fStaff;
					float CountEmpty = fMaxStaff - fStaff;

					while (CountCurrent > 0)
					{
						GUILayout.Box(tKerbal, GUILayout.Width(23));
						CountCurrent = CountCurrent - 1;
					}

					while (CountEmpty > 0)
					{
						GUILayout.Box(tNoKerbal, GUILayout.Width(23));
						CountEmpty = CountEmpty - 1;
					}
					
					//GUILayout.Box("" + fStaff.ToString("#0"), GUILayout.Height(30), GUILayout.Width(160));

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Hire", GUILayout.Height(38), GUILayout.Width(33)))
					{
						if (fStaff == fMaxStaff)
						{
							ScreenMessages.PostScreenMessage("Facility is already fully staffed.", 10, 
								ScreenMessageStyle.LOWER_CENTER);
						}
						else
						{
							double currentfunds = Funding.Instance.Funds;

							if (fHireFundCost > currentfunds)
								ScreenMessages.PostScreenMessage("Insufficient funds to hire staff!", 10,
									ScreenMessageStyle.LOWER_CENTER);
							else
							{
								selectedFacility.setSetting("StaffCurrent", (float)fStaff + 1);
								PersistenceUtils.saveStaticPersistence(selectedFacility);
								Funding.Instance.AddFunds(-fHireFundCost, TransactionReasons.Cheating);
							}
						}

					}
					if (GUILayout.Button("Fire", GUILayout.Height(38), GUILayout.Width(33)))
					{
						if (fStaff < 2)
						{
							ScreenMessages.PostScreenMessage("Facility must have at least one caretaker.", 10, ScreenMessageStyle.LOWER_CENTER);
						}
						else
						{
							selectedFacility.setSetting("StaffCurrent", (float)fStaff - 1);
							PersistenceUtils.saveStaticPersistence(selectedFacility);
							Funding.Instance.AddFunds(fFireRefund, TransactionReasons.Cheating);
							Reputation.Instance.AddReputation(-fFireRepCost, TransactionReasons.Cheating);
						}
					}
				}
				GUILayout.EndHorizontal();

				string sHireTip = " Cost to hire next kerbal: " + fHireFundCost.ToString("#0") + " funds.";
				string sFireTip = " Refund for firing: " + fFireRefund.ToString("#0") + " funds. Rep lost: " + fFireRepCost.ToString("#0") + ".";

				GUILayout.Space(4);

				if (fStaff < fMaxStaff)
					GUILayout.Box(sHireTip, BoxInfo, GUILayout.Height(16));

				if (fStaff > 1)
					GUILayout.Box(sFireTip, BoxInfo, GUILayout.Height(16));

				GUILayout.Space(5);

				fXP = (float)selectedFacility.getSetting("FacilityXP");
				GUILayout.BeginHorizontal();
				{ 
					GUILayout.Space(5);
					GUILayout.Label("XP: ", LabelInfo, GUILayout.Height(30), GUILayout.Width(55));

					float CountCurrentXP = fXP;
					float CountEmptyXP = 5 - fXP;

					while (CountCurrentXP > 0)
					{
						GUILayout.Box(tXPGained, GUILayout.Width(23));
						CountCurrentXP = CountCurrentXP - 1;
					}

					while (CountEmptyXP > 0)
					{
						GUILayout.Box(tXPUngained, GUILayout.Width(23));
						CountEmptyXP = CountEmptyXP - 1;
					}					
					
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Spend", GUILayout.Height(20), GUILayout.Width(70)))
					{
						if (fXP < 1)
						{
							ScreenMessages.PostScreenMessage("No XP to spend!", 10, ScreenMessageStyle.LOWER_CENTER);
						}
					
					}
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.FlexibleSpace();
				GUILayout.Box("This facility is managed by base staff.", BoxInfo, GUILayout.Height(16));
			}
		}

	}
}

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
		public static GUIStyle ButtonSmallText;

		public static Vector2 scrollPos;

		public static Texture tKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/billeted", false);
		public static Texture tNoKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/unbilleted", false);
		public static Texture tXPGained = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/xpgained", false);
		public static Texture tXPUngained = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/xpungained", false);

		public static Boolean bIsOpen = false;
		public static float iFundsOpen2 = 0f;

		public static bool bIsBarracks = false;

		public static float TotalBarracksPool(StaticObject selectedFacility, bool bUnassigned = true)
		{
			float fKerbals = 0f;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				//if ((string)obj.model.getSetting("DefaultFacilityType") == "None") continue;

				if ((string)obj.getSetting("FacilityType") != "Barracks")
				{
					if ((string)obj.model.getSetting("DefaultFacilityType") != "Barracks") continue;
				}

				var dist = Vector3.Distance(selectedFacility.gameObject.transform.position, obj.gameObject.transform.position);
				if (dist > 5000f) continue;

				if (bUnassigned)
				{
					fKerbals = fKerbals + (float)obj.getSetting("ProductionRateCurrent");
				}
				else
				{
					fKerbals = fKerbals + ((float)obj.getSetting("StaffCurrent") - (float)obj.getSetting("ProductionRateCurrent"));	
				}
			}

			return fKerbals;
		}

		public static StaticObject NearestBarracks(StaticObject selectedFacility, bool bUnassigned = true)
		{
			StaticObject soNearest = null;
			float fKerbals = 0f;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				//if ((string)obj.model.getSetting("DefaultFacilityType") == "None") continue;

				if ((string)obj.getSetting("FacilityType") != "Barracks")
				{
					if ((string)obj.model.getSetting("DefaultFacilityType") != "Barracks") continue;
				}

				if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
				{
					var dist = Vector3.Distance(selectedFacility.gameObject.transform.position, obj.gameObject.transform.position);
					if (dist > 5000f) continue;
				}
				else
					continue;

				if (bUnassigned)
				{
					fKerbals = (float)obj.getSetting("ProductionRateCurrent");

					if (fKerbals < 1) continue;
					else
					{
						soNearest = obj;
						break;
					}
				}
				else
				{
					if ((float)obj.getSetting("StaffCurrent") == 1) continue;

					if (((float)obj.getSetting("StaffCurrent") -1) == (float)obj.getSetting("ProductionRateCurrent"))
						continue;
					else
					{
						soNearest = obj;
						break;
					}
				}
			}

			return soNearest;
		}

		public static void DrawFromBarracks(StaticObject selectedFacility)
		{
			selectedFacility.setSetting("ProductionRateCurrent", (float)selectedFacility.getSetting("ProductionRateCurrent") - 1);
		}

		public static void UnassignToBarracks(StaticObject selectedFacility)
		{
			selectedFacility.setSetting("ProductionRateCurrent", (float)selectedFacility.getSetting("ProductionRateCurrent") + 1);
		}

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

			ButtonSmallText = new GUIStyle(GUI.skin.button);
			ButtonSmallText.fontSize = 12;
			ButtonSmallText.fontStyle = FontStyle.Normal;

			fStaff = (float)selectedFacility.getSetting("StaffCurrent");
			fMaxStaff = (float)selectedFacility.getSetting("StaffMax");

			bIsBarracks = false;

			if ((string)selectedFacility.getSetting("FacilityType") == "Barracks")
				bIsBarracks = true;
			else
				if ((string)selectedFacility.model.getSetting("DefaultFacilityType") == "Barracks")
					bIsBarracks = true;

			if (fMaxStaff < 1)
			{
				fMaxStaff = (float)selectedFacility.model.getSetting("DefaultStaffMax");

				if (fMaxStaff < 1)
				{
					selectedFacility.setSetting("StaffMax", (float)0);
					//PersistenceUtils.saveStaticPersistence(selectedFacility);
				}
				else
				{
					selectedFacility.setSetting("StaffMax", (float)fMaxStaff);
					PersistenceUtils.saveStaticPersistence(selectedFacility);
				}
			}

			if (fMaxStaff > 0)
			{
				float fHireFundCost = 5000;
				float fFireRefund = 2500;
				float fFireRepCost = 1;

				bIsOpen = ((string)selectedFacility.getSetting("OpenCloseState") == "Open");

				if (!bIsOpen)
				{
					iFundsOpen2 = (float)selectedFacility.model.getSetting("cost");
					if (iFundsOpen2 == 0) bIsOpen = true;
				}

				GUILayout.Space(5);

				float CountCurrent = fStaff;
				float CountEmpty = fMaxStaff - fStaff;
				float funassigned = (float)selectedFacility.getSetting("ProductionRateCurrent");

				scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(58));
				{
					GUILayout.BeginHorizontal();
					{
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
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();

				GUI.enabled = bIsOpen;

				if (!bIsBarracks)
				{
					GUILayout.Box("Assigned Staff: " + fStaff.ToString("#0") + "/" + fMaxStaff.ToString("#0"), BoxInfo);
				}

				if (bIsBarracks)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Staff: " + fStaff.ToString("#0") + "/" + fMaxStaff.ToString("#0"), LabelInfo);
					GUILayout.FlexibleSpace();
					GUILayout.Label("Unassigned: " + funassigned.ToString("#0") + "/" + fStaff.ToString("#0"), LabelInfo);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Hire", ButtonSmallText, GUILayout.Height(20)))
						{
							if (fStaff == fMaxStaff)
							{
								ScreenMessages.PostScreenMessage("Facility is full.", 10,
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
									Funding.Instance.AddFunds(-fHireFundCost, TransactionReasons.Cheating);
									selectedFacility.setSetting("ProductionRateCurrent", (float)selectedFacility.getSetting("ProductionRateCurrent") + 1);
									PersistenceUtils.saveStaticPersistence(selectedFacility);
								}
							}

						}
						if (GUILayout.Button("Fire", ButtonSmallText, GUILayout.Height(20)))
						{
							if (fStaff < 2)
							{
								ScreenMessages.PostScreenMessage("This facility must have at least one caretaker.", 10, ScreenMessageStyle.LOWER_CENTER);
							}
							else
							{
								if ((float)selectedFacility.getSetting("ProductionRateCurrent") < 1)
								{
									ScreenMessages.PostScreenMessage("All staff are assigned to duties. Staff must be unassigned in order to fire them.", 10, ScreenMessageStyle.LOWER_CENTER);
								}
								else
								{
									selectedFacility.setSetting("StaffCurrent", (float)fStaff - 1);
									selectedFacility.setSetting("ProductionRateCurrent", (float)selectedFacility.getSetting("ProductionRateCurrent") - 1);
									PersistenceUtils.saveStaticPersistence(selectedFacility);
									Funding.Instance.AddFunds(fFireRefund, TransactionReasons.Cheating);
									Reputation.Instance.AddReputation(-fFireRepCost, TransactionReasons.Cheating);
								}
							}
						}
					}

					GUI.enabled = true;
					GUILayout.EndHorizontal();

					string sHireTip = " Cost to hire next kerbal: " + fHireFundCost.ToString("#0") + " funds.";
					string sFireTip = " Refund for firing: " + fFireRefund.ToString("#0") + " funds. Rep lost: " + fFireRepCost.ToString("#0") + ".";

					GUILayout.Space(2);

					if (fStaff < fMaxStaff)
						GUILayout.Box(sHireTip, BoxInfo, GUILayout.Height(16));

					if (fStaff > 1)
						GUILayout.Box(sFireTip, BoxInfo, GUILayout.Height(16));
				}
				else
				{
					GUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Assign", ButtonSmallText, GUILayout.Height(20)))
						{
							if (fStaff == fMaxStaff)
							{
								ScreenMessages.PostScreenMessage("Facility is fully staffed.", 10,
									ScreenMessageStyle.LOWER_CENTER);
							}
							else
							{
								float fAvailable = TotalBarracksPool(selectedFacility);

								if (fAvailable < 1)
								{
									ScreenMessages.PostScreenMessage("No unassigned staff available.", 10, ScreenMessageStyle.LOWER_CENTER);
								}
								else
								{
									StaticObject soNearestBarracks = NearestBarracks(selectedFacility);

									if (soNearestBarracks != null)
									{
										DrawFromBarracks(soNearestBarracks);

										selectedFacility.setSetting("StaffCurrent", (float)fStaff + 1);
										PersistenceUtils.saveStaticPersistence(selectedFacility);
										PersistenceUtils.saveStaticPersistence(soNearestBarracks);
									}
									else
										ScreenMessages.PostScreenMessage("No facility with available staff is nearby.", 10, ScreenMessageStyle.LOWER_CENTER);
								}
							}
						}

						if (GUILayout.Button("Unassign", ButtonSmallText, GUILayout.Height(20)))
						{
							if (fStaff < 2)
							{
								ScreenMessages.PostScreenMessage("An open facility must have one resident caretaker.", 10, ScreenMessageStyle.LOWER_CENTER);
							}
							else
							{
								StaticObject soAvailableSpace = NearestBarracks(selectedFacility, false);

								if (soAvailableSpace != null)
								{
									UnassignToBarracks(soAvailableSpace);
									selectedFacility.setSetting("StaffCurrent", (float)fStaff - 1);
									PersistenceUtils.saveStaticPersistence(selectedFacility);
									PersistenceUtils.saveStaticPersistence(soAvailableSpace);
								}
								else
								{
									ScreenMessages.PostScreenMessage("There's no room left in a barracks or apartment for this kerbal to go to.", 10, ScreenMessageStyle.LOWER_CENTER);
								}
							}
						}
					}

					GUI.enabled = true;
					GUILayout.EndHorizontal();
				}

				GUILayout.Space(5);

				if (KerbalKonstructs.instance.DevMode)
				{
					fXP = (float)selectedFacility.getSetting("FacilityXP");
					GUILayout.BeginHorizontal();
					{
						GUILayout.Space(5);
						GUILayout.Label("XP: ", LabelInfo, GUILayout.Height(23), GUILayout.Width(55));

						float CountCurrentXP = fXP;
						float CountEmptyXP = 5 - fXP;

						while (CountCurrentXP > 0)
						{
							GUILayout.Button(tXPGained, GUILayout.Height(23), GUILayout.Width(23));
							CountCurrentXP = CountCurrentXP - 1;
						}

						while (CountEmptyXP > 0)
						{
							GUILayout.Button(tXPUngained, GUILayout.Height(23), GUILayout.Width(23));
							CountEmptyXP = CountEmptyXP - 1;
						}

						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Spend", ButtonSmallText, GUILayout.Height(23)))
						{
							if (fXP < 1)
							{
								ScreenMessages.PostScreenMessage("No XP to spend!", 10, ScreenMessageStyle.LOWER_CENTER);
							}

						}
					}
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.FlexibleSpace();
				GUILayout.Box("This facility does not require staff assigned to it.", BoxInfo, GUILayout.Height(16));
			}
		}

	}
}

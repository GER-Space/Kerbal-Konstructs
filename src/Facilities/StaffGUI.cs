using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    public class StaffGUI : VerticalLayout
    {
        public static float fXP;
        public static GUIStyle LabelInfo;
        public static GUIStyle BoxInfo;
        public static GUIStyle ButtonSmallText;

        public static Vector2 scrollPos;

        public static Texture tKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/billeted", false);
        public static Texture tNoKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/unbilleted", false);
        public static Texture tXPGained = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/xpgained", false);
        public static Texture tXPUngained = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/xpungained", false);

		ListView staffList;
		StaffItem.List staffItems;
		VerticalLayout barracksGroup;
		VerticalLayout nonbarracksGroup;
		InfoLine assignedStaff;
		InfoLine staff;
		InfoLine unassignedStaff;
		InfoLine hireTip;
		InfoLine fireTipCost;
		InfoLine fireTipRep;
		UIText noStaffRequired;

		Barracks barracks;
		int maxStaff;
		int currentStaff;

		const float hireFundCost = 5000;
		const float fireRefund = 2500;
		const float fireRepCost = 1;

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)
				.Add<ListView>(out staffList)
					.PreferredHeight(58)
					.Finish()
				.Add<VerticalLayout>(out barracksGroup)
					.Add<InfoLine>(out staff)
						.Label(KKLocalization.Staff)
						.Finish()
					.Add<InfoLine>(out unassignedStaff)
						.Label(KKLocalization.StaffUnassigned)
						.Finish()
					.Add<HorizontalLayout>()
						.Add<UIButton>()
							.Text(KKLocalization.StaffHire)
							.OnClick(HireStaff)
							.Finish()
						.Add<UIButton>()
							.Text(KKLocalization.StaffFire)
							.OnClick(FireStaff)
							.Finish()
						.Finish()
					.Add<FixedSpace>() .Size(2) .Finish()
					.Add<InfoLine>(out hireTip)
						.Label(KKLocalization.StaffCostToHire)
						.Finish()
					.Add<InfoLine>(out fireTipCost)
						.Label(KKLocalization.StaffRefundForFiring)
						.Finish()
					.Add<InfoLine>(out fireTipRep)
						.Label(KKLocalization.StaffReputationLost)
						.Finish()
					.Finish()
				.Add<VerticalLayout>(out nonbarracksGroup)
					.Add<InfoLine>(out assignedStaff)
						.Label(KKLocalization.StaffAssigned)
						.Finish()
					.Add<HorizontalLayout>()
						.Add<UIButton>()
							.Text(KKLocalization.StaffAssign)
							.OnClick(AssignStaff)
							.Finish()
						.Add<UIButton>()
							.Text(KKLocalization.StaffAssign)
							.OnClick(UnassignStaff)
							.Finish()
						.Finish()
					.Finish()
					.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>(out noStaffRequired)
					.Text(KKLocalization.StaffNoStaffRequired)
					.Finish()
				;

			staffItems = new StaffItem.List(staffList.Group);
			staffItems.Content = staffList.Content;
		}

		void HireStaff()
		{
			if (currentStaff == maxStaff) {
				MiscUtils.HUDMessage(KKLocalization.StaffFacilityIsFull, 10, 3);
			} else {
				double currentfunds = Funding.Instance.Funds;

				if (hireFundCost > currentfunds) {
					MiscUtils.HUDMessage(KKLocalization.StaffInsufficientFunds, 10, 3);
				} else {
					barracks.StaffCurrent = currentStaff + 1;
					Funding.Instance.AddFunds(-hireFundCost, TransactionReasons.Cheating);
					barracks.ProductionRateCurrent = barracks.ProductionRateCurrent + 1f;
					UpdateUI(barracks.staticInstance);
				}
			}

		}

		void FireStaff()
		{
			if (currentStaff < 2) {
				MiscUtils.HUDMessage(KKLocalization.StaffMustHaveCaretaker, 10, 3);
			} else {
				if (barracks.ProductionRateCurrent < 1) {
					MiscUtils.HUDMessage(KKLocalization.StaffAllStaffAssigned, 10, 3);
				} else {
					barracks.StaffCurrent = currentStaff - 1;
					barracks.ProductionRateCurrent = barracks.ProductionRateCurrent - 1f;
					Funding.Instance.AddFunds(fireRefund, TransactionReasons.Cheating);
					Reputation.Instance.AddReputation(-fireRepCost, TransactionReasons.Cheating);
					UpdateUI(barracks.staticInstance);
				}
			}
		}

		void AssignStaff()
		{
			if (currentStaff == maxStaff) {
				MiscUtils.HUDMessage(KKLocalization.StaffFullyStaffed, 10, 3);
			} else {
				float fAvailable = TotalBarracksPool(barracks.staticInstance);

				if (fAvailable < 1) {
					MiscUtils.HUDMessage(KKLocalization.StaffNoUnassignedStaffAvailable, 10, 3);
				} else {
					StaticInstance soNearestBarracks = NearestBarracks(barracks.staticInstance);

					if (soNearestBarracks != null) {
						DrawFromBarracks(soNearestBarracks);

						barracks.StaffCurrent = currentStaff + 1;
					} else {
						MiscUtils.HUDMessage(KKLocalization.StaffNoFacilityWithStaff, 10, 3);
					}
					UpdateUI(barracks.staticInstance);
				}
			}
		}

		void UnassignStaff()
		{
			if (currentStaff < 2) {
				MiscUtils.HUDMessage(KKLocalization.StaffMustHaveCaretaker, 10, 3);
			} else {
				StaticInstance availableSpace = NearestBarracks(barracks.staticInstance, false);

				if (availableSpace != null) {
					UnassignToBarracks(availableSpace);
					barracks.StaffCurrent = currentStaff - 1;
					UpdateUI(barracks.staticInstance);
				} else {
					MiscUtils.HUDMessage(KKLocalization.StaffNoRoom, 10, 3);
				}
			}
		}

        public static float TotalBarracksPool(StaticInstance staticInstance)
        {
            float fKerbals = 0f;

            foreach (StaticInstance instance in StaticDatabase.GetAllStatics())
            {
                //if ((string)obj.model.getSetting("DefaultFacilityType") == "None") continue;

                if (instance.FacilityType != "Barracks")
                {
                    if (instance.model.DefaultFacilityType != "Barracks") continue;
                }

                var dist = Vector3.Distance(staticInstance.position, instance.position);
                if (dist > 5000f) continue;

                Barracks foundBarracks = instance.gameObject.GetComponent<Barracks>();

                fKerbals = fKerbals + foundBarracks.ProductionRateCurrent;


            }

            return fKerbals;
        }

        public static StaticInstance NearestBarracks(StaticInstance staticInstance, bool bUnassigned = true)
        {
            StaticInstance soNearest = null;
            float fKerbals = 0f;

            foreach (StaticInstance instance in StaticDatabase.GetAllStatics())
            {
                //if ((string)obj.model.getSetting("DefaultFacilityType") == "None") continue;

                if (instance.FacilityType != "Barracks")
                {
                    if (instance.model.DefaultFacilityType != "Barracks") continue;
                }

                if (instance.CelestialBody.name == FlightGlobals.currentMainBody.name)
                {
                    var dist = Vector3.Distance(staticInstance.position, instance.position);
                    if (dist > 5000f) continue;
                }
                else
                    continue;

                Barracks foundBarracks = instance.gameObject.GetComponent<Barracks>();
                if (bUnassigned)
                {
                    fKerbals = foundBarracks.ProductionRateCurrent;

                    if (fKerbals < 1) continue;
                    else
                    {
                        soNearest = instance;
                        break;
                    }
                }
                else
                {
                    if (foundBarracks.StaffCurrent == 1) continue;

                    if ((foundBarracks.StaffCurrent - 1f) == foundBarracks.ProductionRateCurrent)
                        continue;
                    else
                    {
                        soNearest = instance;
                        break;
                    }
                }
            }

            return soNearest;
        }

        public static void DrawFromBarracks(StaticInstance staticInstance)
        {
            Barracks foundBarracks = staticInstance.gameObject.GetComponent<Barracks>();
            foundBarracks.ProductionRateCurrent--;
        }

        public static void UnassignToBarracks(StaticInstance staticInstance)
        {
            Barracks foundBarracks = staticInstance.gameObject.GetComponent<Barracks>();
            foundBarracks.ProductionRateCurrent++;
        }

		void BuildStaffList(int currentStaff, int maxStaff)
		{
			int empty = maxStaff - currentStaff;

			staffItems.Clear();
			while (currentStaff-- > 0) {
				staffItems.Add(new StaffItem (true));
			}
			while (empty-- > 0) {
				staffItems.Add(new StaffItem (false));
			}
			staffList.Items(staffItems);
		}

        public void UpdateUI(StaticInstance staticInstance)
        {
            barracks = staticInstance.myFacilities[0] as Barracks;

            bool isBarraks = false;
			// assume we don't have any staffing
			maxStaff = 0;
			currentStaff = 0;

            if (staticInstance.FacilityType == "Barracks") {
                isBarraks = true;
			} else {
                if (staticInstance.model.DefaultFacilityType == "Barracks") {
					isBarraks = true;
				}
			}

            // check if we can access the staffing variables
            if (staticInstance.FacilityType == "Barracks" || staticInstance.FacilityType == "Business" || staticInstance.FacilityType == "Research") {
                currentStaff = (int) barracks.StaffCurrent;//XXX
                maxStaff = (int) barracks.StaffMax;//XXX

                if (maxStaff < 1) {
                    maxStaff = staticInstance.model.DefaultStaffMax;

                    if (maxStaff < 1) {
                        barracks.StaffMax = 0;
                    } else {
                        barracks.StaffMax = maxStaff;
                    }
                }
            }


            if (maxStaff > 0)
            {
                float CountEmpty = maxStaff - currentStaff;
                int unassigned = (int) barracks.ProductionRateCurrent;//XXX

				BuildStaffList(currentStaff, maxStaff);

				staffList.SetActive(true);
				noStaffRequired.SetActive(false);
				barracksGroup.SetActive(isBarraks);
				nonbarracksGroup.SetActive(!isBarraks);
				if (isBarraks) {
					staff.Info($"{currentStaff} / {maxStaff}");
					unassignedStaff.Info($"{unassigned} / {currentStaff}");

					hireTip.Info($"{hireFundCost:F0}");
					fireTipCost.Info($"{fireRefund:F0}");
					fireTipRep.Info($"{fireRepCost:F0}");
				} else {
					assignedStaff.Info($"{currentStaff} / {maxStaff}");
				}
            } else {
				staffList.SetActive(false);
				noStaffRequired.SetActive(true);
				barracksGroup.SetActive(false);
				nonbarracksGroup.SetActive(false);
            }
        }
    }
}

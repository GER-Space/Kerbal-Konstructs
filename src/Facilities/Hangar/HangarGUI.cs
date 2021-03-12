using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KSP.Localization;

using KodeUI;

namespace KerbalKonstructs.UI
{
    public class HangarGUI : VerticalLayout
    {
		InfoLine maxCraft;
		InfoLine maxMassPerCraft;
		UIText noCraft;
		VerticalLayout storedCraftBlock;
		UIText storedCraftHeader;
		InfoLine vesselTooHeavy;
		UIText hangarIsFull;
		UIText hangarTooFar;
		UIButton storeVessel;

		StaticInstance selectedFacility;
		Hangar hangar;

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)
				.Add<FixedSpace>() .Size(2) .Finish()
				.Add<LayoutAnchor>()
					.DoPreferredHeight(true)
					.FlexibleLayout(true, false)
					.Add<UIText>()
						.Text(KKLocalization.HangarMessage)
						.Anchor(AnchorPresets.StretchAll)
						.SizeDelta(0, 0)
						.Finish()
					.Finish()
				.Add<HorizontalLayout>()
					.Add<InfoLine>(out maxCraft)
						.Label(KKLocalization.HangarMaxCraft)
						.Finish()
					.Add<FlexibleSpace>() .Finish()
					.Add<InfoLine>(out maxMassPerCraft)
						.Label(KKLocalization.HangarMaxMassPerCraft)
						.Finish()
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<LayoutAnchor>()
					.DoPreferredHeight(true)
					.FlexibleLayout(true, false)
					.Add<UIText>(out noCraft)
						.Text(KKLocalization.HangarNoCraft)
						.Alignment(TextAlignmentOptions.Top)
						.Anchor(AnchorPresets.StretchAll)
						.SizeDelta(0, 0)
						.Finish()
					.Add<VerticalLayout>(out storedCraftBlock)
						.Anchor(AnchorPresets.StretchAll)
						.SizeDelta(0, 0)
						.Add<UIText>(out storedCraftHeader)
							.Alignment(TextAlignmentOptions.Center)
							.Finish()
						.Finish()
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<InfoLine>(out vesselTooHeavy)
					.Label(KKLocalization.HangarTooHeavy)
					.Finish()
				.Add<UIText>(out hangarIsFull)
					.Text(KKLocalization.HangarIsFull)
					.Alignment(TextAlignmentOptions.Center)
					.Finish()
				.Add<UIText>(out hangarTooFar)
					.Alignment(TextAlignmentOptions.Center)
					.Finish()
				.Add<UIButton>(out storeVessel)
					.Text(KKLocalization.HangarStoreVessel)
					.OnClick(StoreVessel)
					.Finish()
				;
		}

		void RolloutCraft()
		{
			if (HangarwayIsClear(selectedFacility)) {
				//Vessel newVessel = Hangar.RollOutVessel(vessel, hangar);
				//newVessel.Load();
				//newVessel.MakeActive() ;
				//XXX ick?
				UI2.HangarKSCGUI.useFromFlight = true;
				UI2.HangarKSCGUI.selectedFacility = selectedFacility;
				UI2.HangarKSCGUI.Open();
			} else {
				MiscUtils.HUDMessage(KKLocalization.HangarBlocked, 10, 3);
			}
		}

		void StoreVessel()
		{
			Hangar.StoreVessel(FlightGlobals.ActiveVessel, hangar);
		}

		void BuildCraftList()
		{
		}

        public void UpdateUI(StaticInstance selectedFacility)
        {
			this.selectedFacility = selectedFacility;
            hangar = selectedFacility.myFacilities[0] as Hangar;

			int craftOccupancy = hangar.storedVessels.Count;
			int craftCapacity = hangar.FacilityCraftCapacity;
			float maxMass = hangar.FacilityMassCapacity;
			if (maxMass < 1) {
				maxMass = 25;
			}
			maxCraft.Info($"{craftCapacity}");
			maxMassPerCraft.Info($"{maxMass:F1} t");

			if (craftOccupancy < 1) {
				noCraft.SetActive(true);
				storedCraftBlock.SetActive(false);
			} else {
				noCraft.SetActive(false);
				storedCraftBlock.SetActive(true);

				storedCraftHeader.Text(Localizer.Format(KKLocalization.HangarStoredCraft, hangar.storedVessels.Count, craftCapacity));
				BuildCraftList();
			}

			float mass = FlightGlobals.ActiveVessel.GetTotalMass();
			double distance = Vector3.Distance(FlightGlobals.ActiveVessel.gameObject.transform.position, selectedFacility.position);

			bool tooHeavy = mass > maxMass;
			bool isFull = craftOccupancy >= craftCapacity;
			double tooFar = distance - KerbalKonstructs.instance.facilityUseRange;
			vesselTooHeavy.SetActive(tooHeavy);
			hangarIsFull.SetActive(isFull);
			hangarTooFar.SetActive(tooFar >= 0);
			if (tooHeavy) {
				vesselTooHeavy.Info($"{mass:F1} t/{maxMass:F1} t");
			}
			if (tooFar >= 0) {
				hangarTooFar.Text(Localizer.Format(KKLocalization.HangarTooFar, tooFar));
			}
			storeVessel.interactable = !(tooHeavy || isFull || tooFar >= 0);
        }

        public Boolean HangarwayIsClear(StaticInstance instance)
        {
            Boolean isClear = true;

            for(int i = FlightGlobals.Vessels.Count; i-- > 0; )
            {
				var vessel = FlightGlobals.Vessels[i];
                if (vessel == null) continue;
                if (!vessel.loaded) continue;
                if (vessel.vesselType == VesselType.EVA) continue;
                if (vessel.vesselType == VesselType.Flag) continue;
                if (vessel.situation != Vessel.Situations.LANDED) continue;

                var distToCraft = Vector3.Distance(vessel.gameObject.transform.position, instance.position);
                if (distToCraft > 260) {
                    continue;
                } else {
                    isClear = false;
                }
            }

            return isClear;
        }

      
    }
}

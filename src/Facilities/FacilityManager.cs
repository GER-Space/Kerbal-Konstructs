using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using System;
using UnityEngine;
using KSP.Localization;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class FacilityManager : Window
    {
        private static FacilityManager _instance = null;
        internal static FacilityManager instance
        {
            get {
                if (_instance == null) {
					_instance = UIKit.CreateUI<FacilityManager> (UIMain.appCanvasRect, "KKFacilityManager");
                }
                return _instance;
            }
        }


        private static Rect facilityManagerRect = new Rect(150, 75, 400, 670);

        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);

        public static StaticInstance selectedInstance = null;



		UIText facilityName;
		PositionLine facilityPosition;
		UIText facilityPurpose;
		UIButton openFacility;
		UIButton closeFacility;

		TrackingStationGUI tractingStation;
		HangarGUI hangar;
		ProductionGUI production;
		MerchantGUI merchant;
		StorageGUI storage;
		StaffGUI staff;

        // string sOreTransferAmount = "0";
		void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
		{
			Close();
		}

		public override void CreateUI()
		{
			GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
			base.CreateUI();

			this.Title(KKLocalization.FacilityManager)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.Padding(3, 3, 5, 5)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<HorizontalSep>("HorizontalSep3") .Space (1, 2) .Finish()
				.Add<UIText>(out facilityName)//box, yellow
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<PositionLine>(out facilityPosition) .Finish()
				.Add<LayoutAnchor>()
					.DoPreferredHeight(true)
					.FlexibleLayout(true, false)
					.Add<UIText>(out facilityPurpose)
						.Anchor(AnchorPresets.StretchAll)
						.SizeDelta(0, 0)
						.Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space (2, 3) .Finish()
				.Add<HorizontalLayout>()
					.Add<UIButton>(out openFacility)
						.OnClick(OpenFacility)
						.FlexibleLayout(true, false)
						.Finish()
					.Add<UIButton>(out closeFacility)
						.OnClick(CloseFacility)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space (2, 3) .Finish()
				.Add<TrackingStationGUI>(out tractingStation) .FlexibleLayout(true, false) .Finish()
				.Add<HangarGUI>(out hangar) .FlexibleLayout(true, false) .Finish()
				.Add<ProductionGUI>(out production) .FlexibleLayout(true, false) .Finish()
				.Add<MerchantGUI>(out merchant) .FlexibleLayout(true, false) .Finish()
				.Add<StorageGUI>(out storage) .FlexibleLayout(true, false) .Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space (2, 2) .Finish()
				.Add<StaffGUI>(out staff) .FlexibleLayout(true, false) .Finish()
				.Add<HorizontalSep>("HorizontalSep3") .SpaceBelow (3) .Finish()
				.Finish();

			UIMain.SetTitlebar(titlebar, Close);
		}

		protected override void OnDestroy()
		{
			GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
		}

        public void Close()
        {
            if (KerbalKonstructs.selectedInstance != null)
                KerbalKonstructs.DeselectObject(true, true);


            MerchantGUI.lastInstance = null;
			SetActive(false);
        }


        public void Open()
        {
			if (selectedInstance != null && selectedInstance.hasFacilities && selectedInstance.myFacilities.Count > 0) {
				MerchantGUI.lastInstance = null;
				SetActive(true);
				UpdateUI();
			}
        }

		void OpenFacility()
		{
			double cost = selectedInstance.myFacilities[0].OpenCost;
			if (cost == 0) {
				cost = selectedInstance.model.cost;
			}
			double funds = Funding.Instance.Funds;

			if (cost > Funding.Instance.Funds) {
				MiscUtils.HUDMessage(KKLocalization.InsuficientFundsToOpenFacility, 10, 0);
			} else {
				selectedInstance.myFacilities[0].SetOpen();
				Funding.Instance.AddFunds(-cost, TransactionReasons.Structures);
			}
			UpdateUI();
		}

		void CloseFacility()
		{
			double value = selectedInstance.myFacilities[0].CloseValue;
			if (value == 0) {
				value = selectedInstance.model.cost;
			}

			Funding.Instance.AddFunds(value, TransactionReasons.Structures);
			selectedInstance.myFacilities[0].SetClosed();
			if (selectedInstance.myFacilities[0].FacilityType == "GroundStation") {
				Modules.ConnectionManager.DetachGroundStation(selectedInstance);
			}
			UpdateUI();
		}

		void UpdateOpenClose()
		{
			double openCost = selectedInstance.myFacilities[0].OpenCost;
			double closeValue = selectedInstance.myFacilities[0].CloseValue;
			double defaultCost = selectedInstance.model.cost;
			bool isOpen = selectedInstance.myFacilities[0].isOpen;

			if (openCost == 0) {
				openCost = defaultCost;
			}

			if (closeValue == 0) {
				closeValue = defaultCost;
			}
			bool alwaysOpen = openCost == 0;
			bool cannotClose = closeValue == 0;

			openFacility.interactable = !alwaysOpen && !isOpen;
			closeFacility.interactable = !cannotClose && isOpen;
			if (alwaysOpen) {
				openFacility.Text(KKLocalization.AlwaysOpen);
			} else {
				openFacility.Text(Localizer.Format(KKLocalization.OpenFacilityForFunds, openCost));
			}
			if (cannotClose) {
				closeFacility.Text(KKLocalization.CannotClose);
			} else {
				closeFacility.Text(Localizer.Format(KKLocalization.CloseFacilityForFunds, closeValue));
			}
		}

        void UpdateUI()
        {
			if (selectedInstance.hasFacilities == false || selectedInstance.myFacilities.Count == 0) {
				selectedInstance = null;
				this.Close();
			}

			string facilityType = selectedInstance.FacilityType;

			if (facilityType == "GroundStation") {
				facilityName.Text(KKLocalization.GroundStation);
			} else {
				if (selectedInstance.facilityType != KKFacilityType.None) {
					facilityName.Text (selectedInstance.GetFacility(selectedInstance.facilityType).FacilityName);
				} else {
					facilityName.Text (selectedInstance.model.title);
				}
			}

			Vector3 pos = KerbalKonstructs.instance.GetCurrentBody().transform.InverseTransformPoint(selectedInstance.position);
			facilityPosition.Altitude(selectedInstance.RadiusOffset).Latitude(KKMath.GetLatitudeInDeg(pos)).Longitude(KKMath.GetLongitudeInDeg(pos));

			UpdateOpenClose();

			bool enableTrackingStation = false;
			bool enableHangar = false;
			bool enableProduction = false;
			bool enableMerchant = false;
			bool enableStorage = false;

			switch (selectedInstance.facilityType) {
				case KKFacilityType.Hangar:
					facilityPurpose.Text (KKLocalization.FacilityPurposeHangar);
					enableHangar = true;
					break;
				case KKFacilityType.Barracks:
					facilityPurpose.Text (KKLocalization.FacilityPurposeBarracks);
					break;
				case KKFacilityType.Research:
					facilityPurpose.Text (KKLocalization.FacilityPurposeResearch);
					enableProduction = true;
					break;
				case KKFacilityType.Business:
					facilityPurpose.Text (KKLocalization.FacilityPurposeBusiness);
					enableProduction = true;
					break;
				case KKFacilityType.GroundStation:
					facilityPurpose.Text (KKLocalization.FacilityPurposeGroundStation);
					enableTrackingStation = true;
					break;
				case KKFacilityType.Merchant:
					facilityPurpose.Text (KKLocalization.FacilityPurposeMerchant);
					enableMerchant = true;
					break;
				case KKFacilityType.Storage:
					facilityPurpose.Text (KKLocalization.FacilityPurposeStorage);
					enableStorage = true;
					break;
			}
			bool isOpen = selectedInstance.myFacilities[0].isOpen;

			tractingStation.SetActive (isOpen && enableTrackingStation);
			hangar.SetActive (isOpen && enableHangar);
			production.SetActive (isOpen && enableProduction);
			merchant.SetActive (isOpen && enableMerchant);
			storage.SetActive (isOpen && enableStorage);
			staff.SetActive (isOpen);

			if (isOpen) {
				switch (selectedInstance.facilityType) {
					case KKFacilityType.GroundStation:
						tractingStation.UpdateUI(selectedInstance);
						break;
					case KKFacilityType.Hangar:
						hangar.UpdateUI(selectedInstance);
						break;
					case KKFacilityType.Research:
					case KKFacilityType.Business:
						production.UpdateUI(selectedInstance, facilityType);
						break;
					case KKFacilityType.Merchant:
						merchant.UpdateUI(selectedInstance);
						break;
					case KKFacilityType.Storage:
						storage.UpdateUI(selectedInstance);
						break;
				}
				staff.UpdateUI(selectedInstance);
			}
        }
    }
}

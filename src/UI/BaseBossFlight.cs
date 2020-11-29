using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

using KodeUI;

namespace KerbalKonstructs.UI
{
    internal class BaseBossFlight : Window
    {
        static BaseBossFlight _instance = null;
        internal static BaseBossFlight instance
        {
            get {
                if (_instance == null) {
					_instance = UIKit.CreateUI<BaseBossFlight> (UIMain.appCanvasRect, "KKBaseBossFlight");
                }
                return _instance;
            }
        }

		IconToggle landingGuide;
		IconToggle navGuidance;
		UIText noBasesBeacon;
		UIText noNearestBase;
		HorizontalLayout nearestBaseGroup;
		InfoLine nearestBase;
		FixedSpace ngsFiller;
		UIButton setNGSTarget;
		VerticalLayout careerGroup;
		UIButton openSite;
		UIText siteIsOpen;
		UIText siteCannoteBeOpened;
		UIText basesCanBeOpened;
		VerticalLayout facilitiesGroup;
		UIButton facilityScan;
		UIText noFacilitiesWithin;
		UIText nearbyFacilities;

		GroupCenter center;
		KKLaunchSite ngsSite;

		LaunchsiteItem.List launchsiteItems;
		KKLaunchSite selectedSite;

		FacilityItem.List facilityItems;
		FacilityItem selectedFacility;

        public float iFundsOpen2 = 0f;

        public bool managingFacility = false;
        public bool foundingBase = false;
        public bool bIsOpen = false;

        List<StaticInstance> allFacilities = new List<StaticInstance>();

		public override void CreateUI()
		{
			base.CreateUI();

			ScrollView launchsiteList;
			UIScrollbar ls_scrollbar;
			ScrollView facilityList;
			UIScrollbar f_scrollbar;

			this.Title(KKLocalization.InflightBaseBoss)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<FixedSpace>() .Size(1) .Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.FlightTools)
					.Finish()
				.Add<HorizontalLayout>()
					.Add<FixedSpace>() .Size(2) .Finish()
					.Add<UIText>()
						.Text(KKLocalization.LandingGuide)
						.Finish()
					.Add<IconToggle>(out landingGuide)
						.OnSprite(UIMain.tIconOpen)
						.OffSprite(UIMain.tIconClosed)
						.OnValueChanged(ToggleLandingGuideUI)
						.PreferredSize(56, 18)
						.Finish()
					.Add<FlexibleSpace>() .Finish()
					.Add<UIText>()
						.Text(KKLocalization.NGS)
						.Finish()
					.Add<IconToggle>(out navGuidance)
						.OnSprite(UIMain.tIconOpen)
						.OffSprite(UIMain.tIconClosed)
						.OnValueChanged(ToggleNavGuidanceSystem)
						.PreferredSize(18, 18)
						.Finish()
					.Add<FixedSpace>() .Size(2) .Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.SelectedBase)
					.Finish()
				.Add<UIText>(out noBasesBeacon)
					.Text(KKLocalization.NoBasesBeacon)
					.Finish()
				.Add<UIText>(out noNearestBase)
					.Text(KKLocalization.NoNearestBase)
					.Finish()
				.Add<HorizontalLayout>(out nearestBaseGroup)
					.Add<InfoLine>(out nearestBase)
						.Label(KKLocalization.NearestBase)
						.Finish()
					.Add<FixedSpace>(out ngsFiller) .Size(21) .Finish()
					.Add<UIButton>(out setNGSTarget)
						.Text(KKLocalization.NGS)
						.OnClick(SetNGSTarget)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.BaseStatus)
					.Finish()
				.Add<VerticalLayout>(out careerGroup)
					.Add<ScrollView>(out launchsiteList)
						.Horizontal(false)
						.Vertical(true)
						.Horizontal()
						.ControlChildSize(true, true)
						.ChildForceExpand(false, true)
						.FlexibleLayout(true, false)
						.PreferredSize(-1, 120)
						.Add<UIScrollbar>(out ls_scrollbar, "Scrollbar")
							.Direction(Scrollbar.Direction.BottomToTop)
							.PreferredWidth(15)
							.FlexibleLayout(false, true)
							.Finish()
						.Finish()
					.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
					.Add<UIButton>(out openSite)
						.OnClick(OpenSite)
						.FlexibleLayout(true, false)
						.Finish()
					.Add<UIText>(out siteIsOpen)
						.Text(KKLocalization.BaseIsOpen)
						.Finish()
					.Add<UIText>(out siteCannoteBeOpened)
						.Text(KKLocalization.BaseCannotBeOpened)
						.Finish()
					.Add<UIText>(out basesCanBeOpened)
						.Text(KKLocalization.BasesCanBeOpened)
						.Finish()
					.Finish()
				.Add<VerticalLayout>(out facilitiesGroup)
					.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
					.Add<UIText>()
						.Text(KKLocalization.OperationalFacilities)
						.Finish()
					.Add<UIButton>(out facilityScan)
						.Text(KKLocalization.ScanForFacilities)
						.OnClick(CacheFacilities)
						.FlexibleLayout(true, false)
						.Finish()
					.Add<ScrollView>(out facilityList)
						.Horizontal(false)
						.Vertical(true)
						.Horizontal()
						.ControlChildSize(true, true)
						.ChildForceExpand(false, true)
						.FlexibleLayout(true, false)
						.PreferredSize(-1, 120)
						.Add<UIScrollbar>(out f_scrollbar, "Scrollbar")
							.Direction(Scrollbar.Direction.BottomToTop)
							.PreferredWidth(15)
							.FlexibleLayout(false, true)
							.Finish()
						.Finish()
					.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
					.Add<UIText>(out noFacilitiesWithin)
						.Text(KKLocalization.NoFacilitiesWithin)
						.Finish()
					.Add<UIText>(out nearbyFacilities)
						.Text(KKLocalization.NearbyFacilitiesCanBeShown)
						.Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space(2, 2) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.OtherFeatures)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.StartAirRacing)
					.OnClick(StartAirRacing)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.BasicOrbitalData)
					.OnClick(BasicOrbitalData)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space(5, 2) .Finish()
				.Finish();

			ToggleGroup launchsiteGroup;
			launchsiteList.VerticalScrollbar = ls_scrollbar;
			launchsiteList.Viewport.FlexibleLayout(true, true);
			launchsiteList.Content
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false, false)
				.Anchor(AnchorPresets.HorStretchTop)
				.PreferredSizeFitter(true, false)
				.WidthDelta(0)
				.ToggleGroup (out launchsiteGroup)
				.Finish();

			UIMain.SetTitlebar(titlebar, Close);

			launchsiteItems = new LaunchsiteItem.List (launchsiteGroup);
			launchsiteItems.Content = launchsiteList.Content;
			launchsiteItems.onSelected = OnLaunchsiteSelected;

			ToggleGroup facilityGroup;
			facilityList.VerticalScrollbar = f_scrollbar;
			facilityList.Viewport.FlexibleLayout(true, true);
			facilityList.Content
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false, false)
				.Anchor(AnchorPresets.HorStretchTop)
				.PreferredSizeFitter(true, false)
				.WidthDelta(0)
				.ToggleGroup (out facilityGroup)
				.Finish();

			facilityItems = new FacilityItem.List (facilityGroup);
			facilityItems.Content = facilityList.Content;
			facilityItems.onSelected = OnFacilitySelected;

			rectTransform.anchoredPosition3D = new Vector2(10, -25);

			GameEvents.onVesselSituationChange.Add (onVesselSituationChange);
		}

		protected override void OnDestroy()
		{
			GameEvents.onVesselSituationChange.Remove (onVesselSituationChange);
		}

		void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> vs)
		{
			if (vs.host == FlightGlobals.ActiveVessel) {
				if (!FlightGlobals.ActiveVessel.Landed) {
					allFacilities.Clear();
					UpdateFaclilitiesGroup();
				}
			}
		}

        public void Close()
        {
            FacilityManager.instance.Close();
			SetActive(false);
        }

		public void Open()
		{
			SetActive(true);
			UpdateUI();
		}

		void ToggleLandingGuideUI(bool on)
		{
			KerbalKonstructs.instance.UpdateCache();
			LandingGuideUI.instance.Toggle();
		}

		void ToggleNavGuidanceSystem(bool on)
		{
			NavGuidanceSystem.instance.Toggle();
		}

		void SetNGSTarget()
		{
			NavGuidanceSystem.setTargetSite(ngsSite);

			string message = Localizer.Format(KKLocalization.NGSSetTo, center.Group);
			MiscUtils.HUDMessage(message, 10, 2);
		}

		void OnLaunchsiteSelected(LaunchsiteItem site)
		{
			selectedSite = site.launchsite;
			UpdateUI();
		}

		void OnFacilitySelected(FacilityItem facility)
		{
			selectedFacility = facility;

			KerbalKonstructs.SelectInstance(selectedFacility.facility, false);
			FacilityManager.selectedInstance = selectedFacility.facility;
			FacilityManager.instance.Open();
		}

		void BuildLaunchsites()
		{
			launchsiteItems.Clear();
			int index = 0;
			for (int i = 0, count = center.launchsites.Count; i < count; i++) {
				var site = center.launchsites[i];
				if (site == selectedSite) {
					index = launchsiteItems.Count;
				}
				launchsiteItems.Add (new LaunchsiteItem (site));
			}
			UIKit.UpdateListContent (launchsiteItems);
			launchsiteItems.Select (index);
		}

		void BuildFacilities()
		{
			facilityItems.Clear();
			for (int i = 0, count = allFacilities.Count; i < count; i++) {
				var facility = allFacilities[i];
				facilityItems.Add (new FacilityItem (facility));
			}
			UIKit.UpdateListContent (facilityItems);
		}

		void OpenSite()
		{
			float openCost = selectedSite.OpenCost / 2;
			double currentfunds = Funding.Instance.Funds;

			if (openCost > currentfunds) {
				MiscUtils.HUDMessage(KKLocalization.InsuficientFundsToOpenSite, 10, 3);
			} else {
				LaunchSiteManager.OpenLaunchSite(selectedSite);
				if (MiscUtils.isCareerGame())
				{
					Funding.Instance.AddFunds(-openCost, TransactionReasons.Cheating);
				}
				string message = Localizer.Format(KKLocalization.SiteOpened, selectedSite.LaunchSiteName);
				MiscUtils.HUDMessage(message, 10, 2);

				launchsiteItems.Update(selectedSite);
				UpdateLaunchsite();
			}
		}

		void StartAirRacing()
		{
			AirRacing.instance.Open();
			AirRacing.runningRace = true;
			NavGuidanceSystem.instance.Close();
			FacilityManager.instance.Close();
		}

		void BasicOrbitalData()
		{
			AirRacing.instance.Open();
			AirRacing.runningRace = false;
			AirRacing.basicorbitalhud = true;
			NavGuidanceSystem.instance.Close();
			FacilityManager.instance.Close();
		}

		void UpdateLaunchsite()
		{
			if (selectedSite == null) {
				openSite.SetActive(false);
				siteIsOpen.SetActive(false);
				siteCannoteBeOpened.SetActive(true);
			} else {
				var site = selectedSite;
				string state = site.OpenCloseState;

				if (state == "Closed") {
					float openCost = site.OpenCost / 2;
					openSite.Text(Localizer.Format(KKLocalization.OpenSiteForFunds, site.LaunchSiteName, openCost));
					openSite.SetActive(true);
					siteIsOpen.SetActive(false);
					siteCannoteBeOpened.SetActive(false);
				} else if (state == "Open") {
					openSite.SetActive(false);
					siteIsOpen.SetActive(true);
					siteCannoteBeOpened.SetActive(false);
				} else if (state == "OpenLocked" || state == "ClosedLocked") {
					openSite.SetActive(false);
					siteIsOpen.SetActive(false);
					siteCannoteBeOpened.SetActive(true);
				}
			}
		}

		void UpdateFaclilitiesGroup()
		{
			BuildFacilities();
			facilityScan.interactable = FlightGlobals.ActiveVessel.Landed;
            if (FlightGlobals.ActiveVessel.Landed) {
                if (allFacilities.Count == 0) {
					noFacilitiesWithin.SetActive(true);
					nearbyFacilities.SetActive(false);
                } else {
					noFacilitiesWithin.SetActive(false);
					nearbyFacilities.SetActive(false);
				}
            } else {
				noFacilitiesWithin.SetActive(false);
				nearbyFacilities.SetActive(true);
            }
		}

        void UpdateUI()
        {
			landingGuide.SetIsOnWithoutNotify(LandingGuideUI.instance.IsOpen());
			navGuidance.SetIsOnWithoutNotify(NavGuidanceSystem.instance.IsOpen());

			var Range = float.PositiveInfinity;

			//FIXME atmo scaling? other worlds? ...
			if (FlightGlobals.ActiveVessel.altitude > 75000) {
				center = null;
				noBasesBeacon.SetActive(true);
				noNearestBase.SetActive(false);
				nearestBaseGroup.SetActive(false);
			} else {
				center = StaticDatabase.GetClosestLaunchCenter();;
				if (center == null) {
					noBasesBeacon.SetActive(false);
					noNearestBase.SetActive(true);
					nearestBaseGroup.SetActive(false);
				} else {
					noBasesBeacon.SetActive(false);
					noNearestBase.SetActive(false);
					nearestBaseGroup.SetActive(true);

					Vector3 vPosition = FlightGlobals.ActiveVessel.GetTransform().position;
					ngsSite = LaunchSiteManager.getNearestBase(center, vPosition);
					Range = Vector3.Distance(center.gameObject.transform.position, vPosition);
					string info;
					if (Range < 10000) {
						info = center.Group + " at " + Range.ToString("#0.0") + " m";
					} else {
						info = center.Group + " at " + (Range / 1000).ToString("#0.0") + " km";
					}
					nearestBase.Info(info);
					bool ngs = NavGuidanceSystem.instance.IsOpen();
					ngsFiller.SetActive (!ngs);
					setNGSTarget.SetActive (ngs);
				}
			}

			if (!MiscUtils.isCareerGame()) {
				careerGroup.SetActive(false);
			} else {
				careerGroup.SetActive(true);

                if (!FlightGlobals.ActiveVessel.Landed || Range > 5000) {
					//FIXME a bouncy landing will be laggy
					if (launchsiteItems.Count > 0) {
						launchsiteItems.Clear();
						UIKit.UpdateListContent (launchsiteItems);
					}
					basesCanBeOpened.SetActive(true);
					openSite.SetActive(false);
					siteIsOpen.SetActive(false);
					siteCannoteBeOpened.SetActive(false);
				} else {
					basesCanBeOpened.SetActive(false);
					if (launchsiteItems.Count != center.launchsites.Count) {
						BuildLaunchsites();
					}
					UpdateLaunchsite();
                }

                //if (Range > 100000)
                //{
                //    if (bLanded)
                //    {
                //        GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                //        GUILayout.Space(2);
                //        GUILayout.Label("This feature is WIP.", LabelInfo);
                //        GUI.enabled = false;
                //        if (GUILayout.Button("Found a New Base", GUILayout.Height(23)))
                //        {
                //            foundingBase = true;
                //        }
                //        GUI.enabled = true;
                //        GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                //        GUILayout.Space(2);
                //    }
                //}
                //else
                //{
                //    GUILayout.Label("This feature is WIP.", LabelInfo);
                //}
            }

			UpdateFaclilitiesGroup();
        }

        /// <summary>
        /// Caches the facilities on button open
        /// </summary>
        void CacheFacilities()
        {

            StaticInstance[] allStatics = StaticDatabase.allStaticInstances;
            allFacilities.Clear();

            for (int i = 0; i < allStatics.Length; i++) {
                // No facility assigned
                if (!allStatics[i].hasFacilities) {
                    continue;
                }
                //not anywhere here
                if (!allStatics[i].isActive) {
                    continue;
                }
                if (allStatics[i].myFacilities.Count == 0) {
                    continue;
                }
                // Facility is more than 5000m away
                if (Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, allStatics[i].position) > 5000f) {
                    continue;
                }
                // is not a facility
                if (string.Equals(allStatics[i].FacilityType, "None", StringComparison.CurrentCultureIgnoreCase)) {
                    continue;
                }

                allFacilities.Add(allStatics[i]);
            }
			UpdateFaclilitiesGroup();
        }


    }
}

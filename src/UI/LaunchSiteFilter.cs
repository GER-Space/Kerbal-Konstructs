using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class LaunchSiteFilter : Layout
    {
        public bool showAllcategorys { get; private set; }
        public LaunchSiteCategory category { get; private set; }

        public bool showOpen { get; private set; }
        public bool showClosed { get; private set; }
        public bool showFavOnly { get; private set; }

		ToggleGroup stateGroup;
		ToggleGroup categoryGroup;

		IconToggle openedBases;
		IconToggle closedBases;
		IconToggle showFavorite;
		IconToggle rocketbases;
		IconToggle helipads;
		IconToggle runways;
		IconToggle waterLaunch;
		IconToggle other;

		public override void CreateUI()
		{
			showAllcategorys = true;
			showOpen = new StateButton.State(true);
			showClosed = new StateButton.State(true);
			showFavOnly = new StateButton.State(false);

			base.CreateUI();

			this.Horizontal()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)

				.Add<UIEmpty>()
					.PreferredSize(8, 32)
					.ToggleGroup(out stateGroup)
					.Finish()
				.Add<IconToggle>(out openedBases, "mapOpenBases")
					.Group(stateGroup)
					.Tooltip(KKLocalization.Opened)
					.OnSprite(UIMain.tOpenBasesOn)
					.OffSprite(UIMain.tOpenBasesOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<IconToggle>(out closedBases, "mapClosedBases")
					.Group(stateGroup)
					.Tooltip(KKLocalization.Opened)
					.OnSprite(UIMain.tClosedBasesOn)
					.OffSprite(UIMain.tClosedBasesOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIEmpty>()
					.PreferredSize(8, 32)
					.Finish()
				.Add<IconToggle>(out showFavorite, "mapFavorites")
					.Tooltip(KKLocalization.OnlyFavorites)
					.OnSprite(UIMain.tFavesOn)
					.OffSprite(UIMain.tFavesOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIEmpty>()
					.PreferredSize(8, 32)
					.ToggleGroup(out categoryGroup)
					.Finish()
				.Add<IconToggle>(out rocketbases, "mapLaunchpads")
					.Group(categoryGroup)
					.OnValueChanged((on) => { if (on) { SetCategory(LaunchSiteCategory.RocketPad); } })
					.Tooltip(KKLocalization.Rocketpads)
					.OnSprite(UIMain.tLaunchpadsOn)
					.OffSprite(UIMain.tLaunchpadsOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<IconToggle>(out runways, "mapRunways")
					.Group(categoryGroup)
					.OnValueChanged((on) => { if (on) { SetCategory(LaunchSiteCategory.Runway); } })
					.Tooltip(KKLocalization.Runways)
					.OnSprite(UIMain.tRunwaysOn)
					.OffSprite(UIMain.tRunwaysOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<IconToggle>(out helipads, "mapHelipads")
					.Group(categoryGroup)
					.OnValueChanged((on) => { if (on) { SetCategory(LaunchSiteCategory.Helipad); } })
					.Tooltip(KKLocalization.Helipads)
					.OnSprite(UIMain.tHelipadsOn)
					.OffSprite(UIMain.tHelipadsOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<IconToggle>(out waterLaunch, "mapWater")
					.Group(categoryGroup)
					.OnValueChanged((on) => { if (on) { SetCategory(LaunchSiteCategory.Waterlaunch); } })
					.Tooltip(KKLocalization.WaterLaunch)
					.OnSprite(UIMain.tWaterOn)
					.OffSprite(UIMain.tWaterOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<IconToggle>(out other, "mapOther")
					.Group(categoryGroup)
					.OnValueChanged((on) => { if (on) { SetCategory(LaunchSiteCategory.Other); } })
					.Tooltip(KKLocalization.Other)
					.OnSprite(UIMain.tOtherOn)
					.OffSprite(UIMain.tOtherOff)
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIEmpty>()
					.FlexibleLayout(true, true)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.ALL)
					.OnClick(ShowAll)
					.PreferredSize(32, 32)
					.Finish()
				.Finish()
				;
		}

		void ShowAll ()
		{
			showAllcategorys = true;
			UpdateIoggles();
		}

		void SetCategory(LaunchSiteCategory cat)
		{
			category = cat;
			showAllcategorys = false;
			UpdateIoggles();
		}

		void UpdateIoggles()
		{
			openedBases.SetIsOnWithoutNotify(showOpen);
			closedBases.SetIsOnWithoutNotify(showClosed);
			showFavorite.SetIsOnWithoutNotify(showFavOnly);
			rocketbases.SetIsOnWithoutNotify(showAllcategorys || (category == LaunchSiteCategory.RocketPad));
			helipads.SetIsOnWithoutNotify(showAllcategorys || (category == LaunchSiteCategory.Helipad));
			runways.SetIsOnWithoutNotify(showAllcategorys || (category == LaunchSiteCategory.Runway));
			waterLaunch.SetIsOnWithoutNotify(showAllcategorys || (category == LaunchSiteCategory.Waterlaunch));
			other.SetIsOnWithoutNotify(showAllcategorys || (category == LaunchSiteCategory.Other));
		}
    }
}

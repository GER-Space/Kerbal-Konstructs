using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class LaunchsiteSelectorGUI : Window
    {
        private static LaunchsiteSelectorGUI _instance = null;
        internal static LaunchsiteSelectorGUI instance
        {
            get
            {
                if (_instance == null)
                {
					_instance = UIKit.CreateUI<LaunchsiteSelectorGUI> (UIMain.appCanvasRect, "KKLaunchsiteSelectorGUI");
                }
                return _instance;
            }
        }


        LaunchsiteFilter launchsiteFilter;
		LaunchsiteItem.List launchsiteItems;

        public float rangekm = 0;

        private static KKLaunchSite defaultSite = null;
        internal static KKLaunchSite selectedSite;

        Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 400, 460);

		InfoLine currentLaunchsite;
		UIButton setLaunchsite;
		UIButton setDefault;
		UIButton useDefault;

		public override void CreateUI()
		{
			base.CreateUI();

			gameObject.AddComponent<Touchable>();

			ScrollView launchsiteList;
			UIScrollbar ls_scrollbar;

			this.Title(KKLocalization.LaunchsiteSelector)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<HorizontalSep>("HorizontalSep") .Space(1, 2) .Finish()
				.Add<LaunchsiteFilter>(out launchsiteFilter)
					.OnFilterChanged(BuildLaunchsites)
					.Finish()
				.Add<FixedSpace>() .Size(10) .Finish()
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
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<InfoLine> (out currentLaunchsite)
					.Label(KKLocalization.CurrentLaunchsite)
					.Finish()
				.Add<UIButton>(out setLaunchsite)
					.Text(KKLocalization.SetLaunchsite)
					.OnClick(SetLaunchsite)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<HorizontalLayout>()
					.Add<UIButton>(out setDefault)
						.Text(KKLocalization.SetDefault)
						.OnClick(SetDefault)
						.FlexibleLayout(true, false)
						.Finish()
					.Add<UIButton>(out useDefault)
						.Text(KKLocalization.UseDefault)
						.OnClick(UseDefault)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep") .SpaceBelow(2) .Finish()
				.Finish();

			UIMain.SetTitlebar(titlebar, Close);

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

			launchsiteItems = new LaunchsiteItem.List (launchsiteGroup);
			launchsiteItems.Content = launchsiteList.Content;
			launchsiteItems.onSelected = OnSelected;
		}

		void OnSelected (LaunchsiteItem site)
		{
			selectedSite = site.launchsite;
			BaseManager.selectedSite = selectedSite;
			UpdateUI();
			BaseManager.instance.UpdateUI();
		}

		void BuildLaunchsites()
		{
			launchsiteItems.Clear();
			int index = 0;
			for (int i = 0, count = LaunchSiteManager.allLaunchSites.Length; i < count; i++) {
				var site = LaunchSiteManager.allLaunchSites[i];
				if (launchsiteFilter.showFavOnly && (site.favouriteSite != "Yes")) {
					continue;
				}

				if (launchsiteFilter.category != site.sitecategory && !launchsiteFilter.showAllcategorys) {
					continue;
				}

				if (LaunchSiteManager.CheckLaunchSiteIsValid(site) == false) {
					continue;
				}
				if ((!launchsiteFilter.showOpen && site.isOpen) || (!launchsiteFilter.showClosed && !site.isOpen)) {
					continue;
				}
				// Don't show hidden closed Bases
				if (site.LaunchSiteIsHidden && (!site.isOpen)) {
					//Log.Normal("Ignoring hidden base: " + site.LaunchSiteName);
					continue;
				}
				if (site == selectedSite) {
					index = launchsiteItems.Count;
				}
				launchsiteItems.Add (new LaunchsiteItem (site));
			}
			UIKit.UpdateListContent (launchsiteItems);
			launchsiteItems.Select (index);
		}

        public void Close()
        {
            BaseManager.instance.Close();
			gameObject.SetActive (false);
        }


        public void Open()
        {
			gameObject.SetActive (true);
            try
            {
				var lastLaunchsite = LaunchSiteManager.GetLaunchSiteByName(KerbalKonstructs.instance.lastLaunchSiteUsed);
                if (LaunchSiteManager.CheckLaunchSiteIsValid(lastLaunchsite))
                {
                    selectedSite = lastLaunchsite;
                    defaultSite = selectedSite;
                }
                if (selectedSite.isOpen == false)
                {
                    Log.Error("LastSiteUsed is invalid, trying default");
                    selectedSite = LaunchSiteManager.GetDefaultSite();
                    defaultSite = selectedSite;
                }
            }
            catch
            {
                selectedSite = LaunchSiteManager.GetDefaultSite();
            }

            BaseManager.selectedSite = selectedSite;
            BaseManager.instance.Open();
            LaunchSiteManager.setLauncsite(selectedSite);
			UpdateUI ();
			BuildLaunchsites();
        }

		void SetLaunchsite()
		{
			LaunchSiteManager.setLauncsite(selectedSite);
			string message = Localizer.Format(KKLocalization.HasBeenSetAsLaunchsite, selectedSite.Name);
			MiscUtils.HUDMessage(message, 10, 0);
		}

		void SetDefault()
		{
			if (selectedSite == null) {
				return;
			}
			if (EditorDriver.editorFacility == EditorFacility.SPH) {
				KerbalKonstructs.instance.defaultSPHlaunchsite = selectedSite.LaunchSiteName;
			} else if (EditorDriver.editorFacility == EditorFacility.VAB) {
				KerbalKonstructs.instance.defaultVABlaunchsite = selectedSite.LaunchSiteName;
			}
			string message = Localizer.Format(KKLocalization.HasBeenSetAsDefault, selectedSite.Name);
			MiscUtils.HUDMessage(message, 10, 0);
		}

		void UseDefault()
		{
			selectedSite = LaunchSiteManager.GetDefaultSite();
			SetLaunchsite();
		}

        public void UpdateUI()
        {
            var currentSite = LaunchSiteManager.GetLaunchSiteByName(LaunchSiteManager.getCurrentLaunchSite());
			currentLaunchsite.Info (currentSite.Name);
			setLaunchsite.interactable = selectedSite.isOpen && selectedSite != currentSite;

			var currentDefault = EditorDriver.editorFacility == EditorFacility.SPH ? KerbalKonstructs.instance.defaultSPHlaunchsite : KerbalKonstructs.instance.defaultVABlaunchsite;
			setDefault.interactable = selectedSite.isOpen && LaunchSiteManager.GetLaunchSiteByName(currentDefault) != selectedSite;
        }
    }
}

using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KSP.Localization;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class BaseManager : Window
    {
        private static BaseManager _instance = null;
        internal static BaseManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIKit.CreateUI<BaseManager>(UIMain.appCanvasRect, "KKBaseManager");
					_instance.rectTransform.anchoredPosition3D = new Vector2(250, -60);
                }
                return _instance;
            }
        }

        public static Sprite tTitleIcon;
        public static Sprite tSmallClose;
        public static Sprite tSetLaunchsite;
        public static Sprite tOpenedLaunchsite;
        public static Sprite tClosedLaunchsite;
        public static Sprite tMakeFavourite;
        public static Sprite tVerticalSep;
        public static Sprite tIsFave;
        public static Sprite tFoldOut;
        public static Sprite tFoldIn;
        public static Sprite tFolded;


        public static KKLaunchSite selectedSite = null;

		UIText siteName;
		UIImage siteLogo;
		UIText siteDescription;
		VerticalLayout foldGroup;
		VerticalLayout statsView;
		UIButton showStats;
		UIButton showLog;
		IconToggle favToggle;
		IconToggle foldToggle;
		InfoLine altitude;
		InfoLine longitude;
		InfoLine latitude;
		InfoLine maxLength;
		InfoLine maxWidth;
		InfoLine maxHeight;
		InfoLine maxMass;
		InfoLine maxParts;
		ScrollView logView;
		UIText logText;
		UIButton openBase;
		UIButton closeBase;
		HorizontalLayout editorView;
		VerticalLayout flightView;
		UIImage launchsiteStatus;
		UIButton setLaunchsite;
		UIButton createWaypoint;
		UIButton deleteWaypoint;

        public float rangekm = 0;


        //public Boolean isOpen = false;
        public Boolean isFavourite = false;
        public Boolean displayStats = true;
        public Boolean displayLog = false;
        public Boolean foldedIn = false;
        public Boolean doneFold = false;
        //public Boolean isLaunch = false;

		public void Close()
		{
			SetActive(false);
		}

		public void Open()
		{
			SetActive(true);
			UpdateUI();
		}

		void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
		{
			Close();
		}

		public override void CreateUI()
		{
			GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
			if (tFolded == null) {
				tTitleIcon = UIMain.MakeSprite("KerbalKonstructs/Assets/titlebaricon");
				tSmallClose = UIMain.MakeSprite("KerbalKonstructs/Assets/littleclose");
				tSetLaunchsite = UIMain.MakeSprite("KerbalKonstructs/Assets/setaslaunchsite");
				tOpenedLaunchsite = UIMain.MakeSprite("KerbalKonstructs/Assets/openedlaunchsite");
				tClosedLaunchsite = UIMain.MakeSprite("KerbalKonstructs/Assets/closedlaunchsite");
				tMakeFavourite = UIMain.MakeSprite("KerbalKonstructs/Assets/makefavourite");
				tVerticalSep = UIMain.MakeSprite("KerbalKonstructs/Assets/verticalsep");
				tIsFave = UIMain.MakeSprite("KerbalKonstructs/Assets/isFavourite");
				tFoldOut = UIMain.MakeSprite("KerbalKonstructs/Assets/foldin");
				tFoldIn = UIMain.MakeSprite("KerbalKonstructs/Assets/foldout");
				tFolded = UIMain.MakeSprite("KerbalKonstructs/Assets/foldout");
			}

			ScrollView siteInfo;
			UIScrollbar info_scrollbar;
			UIScrollbar log_scrollbar;
			//ToggleGroup toggleGroup;

			base.CreateUI();
			Title (KKLocalization.BaseManager)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin ("KK.Default")

				.Add<HorizontalSep>("HorizontalSep") .Space(1, 2) .Finish()
				.Add<UIText>(out siteName)//yellow text
					.Alignment(TextAlignmentOptions.Center)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<VerticalLayout>(out foldGroup)
					.Add<HorizontalLayout>()
						.Add<FixedSpace>() .Size(2) .Finish()
						.Add<UIImage>()
							.Image(tVerticalSep)
							.FlexibleLayout(false, true)
							.PreferredSize(4,135)
							.Finish()
						.Add<FlexibleSpace>() .Finish()
						.Add<UIImage>(out siteLogo)
							.PreferredSize(135,135)
							.Finish()
						.Add<FlexibleSpace>() .Finish()
						.Add<UIImage>()
							.Image(tVerticalSep)
							.FlexibleLayout(false, true)
							.PreferredSize(4,135)
							.Finish()
						.Add<FixedSpace>() .Size(2) .Finish()
						.Finish()
					.Add<FixedSpace>() .Size(3) .Finish()
					.Add<ScrollView>(out siteInfo)
						.Horizontal(false)
						.Vertical(true)
						.Horizontal()
						.ControlChildSize(true, true)
						.ChildForceExpand(false, true)
						.FlexibleLayout(true, false)
						.PreferredSize(-1, 120)
						.Add<UIScrollbar>(out info_scrollbar, "Scrollbar")
							.Direction(Scrollbar.Direction.BottomToTop)
							.PreferredWidth(15)
							.Finish()
						.Finish()
					.Finish()
				.Add<FixedSpace>() .Size(1) .Finish()
				.Add<HorizontalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Add<UIButton>(out showStats)
						.Text(KKLocalization.Stats)
						.OnClick(ShowStats)
						.Finish()
					.Add<FlexibleSpace>() .Finish()
					.Add<UIButton>(out showLog)
						.Text(KKLocalization.Log)
						.OnClick(ShowLog)
						.Finish()
					.Add<FlexibleSpace>() .Finish()
					.Add<IconToggle>(out favToggle)
						.OnSprite(tIsFave)
						.OffSprite(tMakeFavourite)
						.OnValueChanged(SetIsFavorite)
						.PreferredSize(23,23)
						.Finish()
					.Add<FlexibleSpace>() .Finish()
					.Add<IconToggle>(out foldToggle)
						.OnSprite(tFoldOut)
						.OffSprite(tFoldIn)
						.OnValueChanged(SetIsFolded)
						.PreferredSize(23,23)
						.Finish()
					.Finish()
				.Add<VerticalLayout>(out statsView)
					.Add<InfoLine>(out altitude) .Label(KKLocalization.Altitude) .Finish()
					.Add<InfoLine>(out longitude) .Label(KKLocalization.Longitude) .Finish()
					.Add<InfoLine>(out latitude) .Label(KKLocalization.Latitude) .Finish()
					.Add<FixedSpace>() .Size(3) .Finish()
					.Add<InfoLine>(out maxLength) .Label(KKLocalization.MaxLength) .Finish()
					.Add<InfoLine>(out maxWidth) .Label(KKLocalization.MaxWidth) .Finish()
					.Add<InfoLine>(out maxHeight) .Label(KKLocalization.MaxHeight) .Finish()
					.Add<InfoLine>(out maxMass) .Label(KKLocalization.MaxMass) .Finish()
					.Add<InfoLine>(out maxParts) .Label(KKLocalization.MaxParts) .Finish()
					.Finish()
				.Add<ScrollView>(out logView)
					.Horizontal(false)
					.Vertical(true)
					.Horizontal()
					.ControlChildSize(true, true)
					.ChildForceExpand(false, true)
					.FlexibleLayout(true, false)
					.PreferredSize(-1, 120)
					.Add<UIScrollbar>(out log_scrollbar, "Scrollbar")
						.Direction(Scrollbar.Direction.BottomToTop)
						.PreferredWidth(15)
						.Finish()
					.Finish()
				.Add<FixedSpace>() .Size(1) .Finish()
				.Add<UIButton>(out openBase)
					.OnClick(OpenBase)
					.Finish()
				.Add<UIButton>(out closeBase)
					.OnClick(CloseBase)
					.Finish()
				.Add<FlexibleSpace>() .Finish()
				.Add<HorizontalLayout>(out editorView)
					.Add<UIImage>(out launchsiteStatus)
						.PreferredSize(32, 32)
						.Finish()
					.Add<UIButton>(out setLaunchsite)
						.Text(KKLocalization.SetLaunchsite)
						.OnClick(SetLaunchsite)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<VerticalLayout>(out flightView)
					.Add<UIButton>(out createWaypoint)
						.Text(KKLocalization.CreateWaypoint)
						.OnClick(CreateWaypoint)
						.FlexibleLayout(true, false)
						.Finish()
					.Add<UIButton>(out deleteWaypoint)
						.Text(KKLocalization.DeleteWaypoint)
						.OnClick(DeleteWaypoint)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<HorizontalSep>("HorizontalSep") .Space(3, 1) .Finish()
				.Finish();

			ShowStats();

			siteInfo.VerticalScrollbar = info_scrollbar;
			siteInfo.Viewport.FlexibleLayout(true, true);
			siteInfo.Content
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false, false)
				.Anchor(AnchorPresets.HorStretchTop)
				.PreferredSizeFitter(true, false)
				.WidthDelta(0)
				.Add<UIText>(out siteDescription)
					.Alignment (TextAlignmentOptions.TopLeft)
					.Margin(5, 5, 5, 5)
					.FlexibleLayout (true, false)
					.SizeDelta (0, 0)
					.Finish ()
				.Finish();

			logView.VerticalScrollbar = log_scrollbar;
			logView.Viewport.FlexibleLayout(true, true);
			logView.Content
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false, false)
				.Anchor(AnchorPresets.HorStretchTop)
				.PreferredSizeFitter(true, false)
				.WidthDelta(0)
				.Add<UIText>(out logText)
					.Alignment (TextAlignmentOptions.TopLeft)
					.Margin(5, 5, 5, 5)
					.FlexibleLayout (true, false)
					.SizeDelta (0, 0)
					.Finish ()
				.Finish();

			UIMain.SetTitlebar(titlebar, Close);
		}

		protected override void OnDestroy()
		{
			GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
		}


		void SetIsFolded(bool on)
		{
			foldedIn = on;
			foldGroup.SetActive(!on);
			selectedSite.favouriteSite = on ? "Yes" : "No";
		}

		void SetIsFavorite(bool on)
		{
			selectedSite.favouriteSite = on ? "Yes" : "No";
		}

		void ShowStats()
		{
			showStats.interactable = false;
			showLog.interactable = true;
			statsView.SetActive(true);
			logView.SetActive(false);
		}

		void ShowLog()
		{
			showStats.interactable = true;
			showLog.interactable = false;
			statsView.SetActive(false);
			logView.SetActive(true);
		}

		void OpenBase()
		{
			if (CareerUtils.isCareerGame && selectedSite.OpenCost > Funding.Instance.Funds) {
				MiscUtils.HUDMessage(KKLocalization.InsuficientFundsToOpenBase, 10, 3);
			} else {
				LaunchSiteManager.OpenLaunchSite(selectedSite);
				if (CareerUtils.isCareerGame) {
					//FIXME why is this cheating?
					Funding.Instance.AddFunds(-selectedSite.OpenCost, TransactionReasons.Cheating);
				}
			}
		}

		void CloseBase()
		{
			LaunchSiteManager.CloseLaunchSite(selectedSite);
			if (CareerUtils.isCareerGame) {
				//FIXME why is this cheating?
				Funding.Instance.AddFunds(selectedSite.CloseValue, TransactionReasons.Cheating);
			}
		}

		void CreateWaypoint()
		{
			MapIconSelector.CreateWPForLaunchSite(selectedSite);
		}

		void DeleteWaypoint()
		{
			FinePrint.WaypointManager.RemoveWaypoint(selectedSite.wayPoint);
			selectedSite.wayPoint = null;
		}

		void SetLaunchsite()
		{
			LaunchSiteManager.setLauncsite(selectedSite);
			string message = Localizer.Format(KKLocalization.HasBeenSetAsLaunchsite, SiteName);
			MiscUtils.HUDMessage(message, 10, 0);
		}

		string MaxSpec(float spec, string unit)
		{
			if (spec > 0) {
				return $"{spec:F0}{unit}";
			}
			return KKLocalization.Unlimited;
		}

		string SiteName
		{
			get {
				string siteName = "";
				siteName = selectedSite.LaunchSiteName;
				if (selectedSite.LaunchSiteName == "Runway") siteName = "KSC Runway";
				if (selectedSite.LaunchSiteName == "LaunchPad") siteName = "KSC LaunchPad";
				return siteName;
			}
		}

        internal void UpdateUI()
        {
			siteName.Text(SiteName);
			siteLogo.Image(selectedSite.logo);
			siteDescription.Text(selectedSite.LaunchSiteDescription);

			favToggle.SetIsOnWithoutNotify(selectedSite.favouriteSite == "Yes");
			foldToggle.SetIsOnWithoutNotify(foldedIn);

			altitude.Info($"{selectedSite.refAlt:F1} m");
			longitude.Info($"{selectedSite.refLon:F3}");
			latitude.Info($"{selectedSite.refLat:F3}");
			maxLength.Info(MaxSpec(selectedSite.LaunchSiteLength, " m"));
			maxWidth.Info(MaxSpec(selectedSite.LaunchSiteWidth, " m"));
			maxHeight.Info(MaxSpec(selectedSite.LaunchSiteHeight, " m"));
			maxMass.Info(MaxSpec(selectedSite.MaxCraftMass, " t"));
			maxParts.Info(MaxSpec(selectedSite.MaxCraftParts, ""));

			openBase.interactable = !selectedSite.isOpen;
			closeBase.interactable = selectedSite.isOpen;
			openBase.Text(Localizer.Format(KKLocalization.OpenBaseForFunds, selectedSite.OpenCost));
			closeBase.Text(Localizer.Format(KKLocalization.CloseBaseForFunds, selectedSite.CloseValue));

			if (!String.IsNullOrEmpty(selectedSite.MissionLog)) {
				logText.Text(selectedSite.MissionLog.Replace("|", "\n"));
			} else {
				logText.Text(KKLocalization.NoLog);
			}

            if (HighLogic.LoadedScene == GameScenes.EDITOR) {
				editorView.SetActive(true);
				flightView.SetActive(false);
				if (selectedSite.LaunchSiteName == EditorLogic.fetch.launchSiteName) {
					launchsiteStatus.Image(tSetLaunchsite);
				} else if (selectedSite.isOpen) {
					launchsiteStatus.Image(tOpenedLaunchsite);
				} else {
					launchsiteStatus.Image(tClosedLaunchsite);
				}
				setLaunchsite.interactable = (selectedSite.isOpen) && !(selectedSite.LaunchSiteName == EditorLogic.fetch.launchSiteName);
            } else {
				editorView.SetActive(false);
				flightView.SetActive(true);
				if (selectedSite.wayPoint!= null && FinePrint.WaypointManager.FindWaypoint(selectedSite.wayPoint.navigationId) == null) {
					selectedSite.wayPoint = null;
				}
				createWaypoint.SetActive(selectedSite.wayPoint == null);
				deleteWaypoint.SetActive(selectedSite.wayPoint != null);
			}
        }

        public static KKLaunchSite getSelectedSite()
        {
            KKLaunchSite thisSite = selectedSite;
            return thisSite;
        }

        public static void setSelectedSite(KKLaunchSite soSite)
        {
            selectedSite = soSite;
        }
    }
}

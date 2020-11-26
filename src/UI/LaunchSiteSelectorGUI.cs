using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class LaunchSiteSelectorGUI : Layout
    {
        private static LaunchSiteSelectorGUI _instance = null;
        internal static LaunchSiteSelectorGUI instance
        {
            get
            {
                if (_instance == null)
                {
					_instance = UIKit.CreateUI<LaunchSiteSelectorGUI> (UIMain.appCanvasRect, "KKLaunchSiteSelectorGUI");
                }
                return _instance;
            }
        }


        private bool showAllcategorys = true;
        private LaunchSiteCategory category;

        public float rangekm = 0;
        public string sCurrentSite = "";

        public Vector2 sitesScrollPosition;

        public bool showOpen = new StateButton.State(true);
        public bool showClosed = new StateButton.State(true);
        public bool showFavOnly = new StateButton.State(false);

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

        private static KKLaunchSite defaultSite = null;
        internal static KKLaunchSite selectedSite;

        private string launchButtonName = "";

        Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 400, 460);

		public override void CreateUI()
		{
			base.CreateUI();

			gameObject.AddComponent<Touchable>();
			
			this.Horizontal()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

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

        public void Close()
        {
            InputLockManager.RemoveControlLock("KKEditorLock");
            InputLockManager.RemoveControlLock("KKEditorLock2");
            BaseManager.instance.Close();
			gameObject.SetActive (false);
        }


        public void Open()
        {
			gameObject.SetActive (true);
            try
            {

                if (LaunchSiteManager.CheckLaunchSiteIsValid(LaunchSiteManager.GetLaunchSiteByName(KerbalKonstructs.instance.lastLaunchSiteUsed)))
                {
                    selectedSite = LaunchSiteManager.GetLaunchSiteByName(KerbalKonstructs.instance.lastLaunchSiteUsed);
                    defaultSite = selectedSite;
                }
                if (selectedSite.isOpen == false)
                {
                    Log.Error("LastSideUsed is invalid, trying default");
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
            LaunchSiteManager.setLaunchSite(selectedSite);
        }


        public void DrawSelector()
        {

            windowRect = GUI.Window(0xB00B1E6, windowRect, DrawSelectorWindow, "", UIMain.KKWindow);

            if (windowRect.Contains(Event.current.mousePosition))
            {
                InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, "KKEditorLock2");
            }
            else
            {
                InputLockManager.RemoveControlLock("KKEditorLock2");
            }
        }

        public void DrawSelectorWindow(int id)
        {

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Launchsite Selector", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Height(21)))
                {
                    this.Close();
                    return;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            {
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            sitesScrollPosition = GUILayout.BeginScrollView(sitesScrollPosition);
            {
                foreach (KKLaunchSite site in LaunchSiteManager.allLaunchSites)
                {
                    if (showFavOnly && (site.favouriteSite != "Yes"))
                    {
                        continue;
                    }

                    if (category != site.sitecategory && !showAllcategorys)
                    {
                        continue;
                    }

                    if (LaunchSiteManager.CheckLaunchSiteIsValid(site) == false)
                    {
                        continue;
                    }
                    if ((!showOpen && site.isOpen) || (!showClosed && !site.isOpen))
                    {
                        continue;
                    }
                    // Don't show hidden closed Bases
                    if (site.LaunchSiteIsHidden && (!site.isOpen))
                    {
                        //Log.Normal("Ignoring hidden base: " + site.LaunchSiteName);
                        continue;
                    }

                    GUILayout.BeginHorizontal();
                    {
                        ShowOpenStatus(site);
                        ShowCategory(site);
                        launchButtonName = site.LaunchSiteName;
                        if (site.LaunchSiteName == "Runway")
                        {
                            launchButtonName = "KSC Runway";
                        }

                        if (site.LaunchSiteName == "LaunchPad")
                        {
                            launchButtonName = "KSC LaunchPad";
                        }

                        GUI.enabled = (selectedSite != site);
                        if (GUILayout.Button(launchButtonName, GUILayout.Height(30)))
                        {
                            selectedSite = site;
                            BaseManager.selectedSite = selectedSite;

                            //if (!MiscUtils.isCareerGame())
                            //{
                            //    LaunchSiteManager.setLaunchSite(site);
                            //    smessage = "Launchsite set to " + launchButtonName;
                            //    MiscUtils.HUDMessage(smessage, 10, 2);
                            //}
                        }
                        GUI.enabled = true;

                        ShowOpenStatus(site);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(5);

            sCurrentSite = LaunchSiteManager.getCurrentLaunchSite();

            switch (sCurrentSite)
            {
                case "Runway":
                    GUILayout.Box("Current Launchsite: KSC Runway");
                    break;
                case "LaunchPad":
                    GUILayout.Box("Current Launchsite: KSC LaunchPad");
                    break;
                default:
                    GUILayout.Box("Current Launchsite: " + sCurrentSite);
                    break;
            }

            GUI.enabled = (selectedSite.isOpen && (selectedSite.LaunchSiteName != sCurrentSite));
            if (GUILayout.Button("Set as Launchsite", GUILayout.Height(46)))
            {
                LaunchSiteManager.setLaunchSite(selectedSite);
                MiscUtils.HUDMessage(selectedSite.LaunchSiteName + " has been set as the launchsite", 10, 0);
            }
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                if ((selectedSite.isOpen) && (EditorDriver.editorFacility == EditorFacility.SPH) && (KerbalKonstructs.instance.defaultSPHlaunchsite != selectedSite.LaunchSiteName))
                {
                    GUI.enabled = true;
                }

                if ((selectedSite.isOpen) && (EditorDriver.editorFacility == EditorFacility.VAB) && (KerbalKonstructs.instance.defaultVABlaunchsite != selectedSite.LaunchSiteName))
                {
                    GUI.enabled = true;
                }

                if (GUILayout.Button("Set as Default", GUILayout.Height(23)))
                {
                    if (selectedSite != null)
                    {
                        MiscUtils.HUDMessage(selectedSite.LaunchSiteName + " has been set as the default", 10, 0);
                        if (EditorDriver.editorFacility == EditorFacility.SPH)
                        {
                            KerbalKonstructs.instance.defaultSPHlaunchsite = selectedSite.LaunchSiteName;
                        }

                        if (EditorDriver.editorFacility == EditorFacility.VAB)
                        {
                            KerbalKonstructs.instance.defaultVABlaunchsite = selectedSite.LaunchSiteName;
                        }
                    }
                }
                GUI.enabled = true;

                if (GUILayout.Button("Use Default", GUILayout.Height(23)))
                {
                    selectedSite = LaunchSiteManager.GetDefaultSite();
                    LaunchSiteManager.setLaunchSite(selectedSite);
                    MiscUtils.HUDMessage(selectedSite.LaunchSiteName + " has been set as the launchsite", 10, 0);
                    BaseManager.selectedSite = selectedSite;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUI.enabled = true;

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 5, labelSize.y + 6), GUI.tooltip, UIMain.KKToolTip);
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }


        internal static void ShowCategory(KKLaunchSite site)
        {
            switch (site.sitecategory)
            {
                case LaunchSiteCategory.Runway:
                    GUILayout.Button(UIMain.runWayIcon, UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    break;
                case LaunchSiteCategory.Helipad:
                    GUILayout.Button(UIMain.heliPadIcon, UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    break;
                case LaunchSiteCategory.RocketPad:
                    GUILayout.Button(UIMain.VABIcon, UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    break;
                case LaunchSiteCategory.Waterlaunch:
                    GUILayout.Button(UIMain.waterLaunchIcon, UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    break;
                case LaunchSiteCategory.Other:
                    GUILayout.Button(UIMain.ANYIcon, UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    break;
                default:
                    GUILayout.Button("", UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    break;
            }
        }


        internal void ShowOpenStatus(KKLaunchSite site)
        {
            if (site.isOpen)
            {
                GUILayout.Label(UIMain.tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
            }
            else
            {
                GUILayout.Label(UIMain.tIconClosed, GUILayout.Height(30), GUILayout.Width(30));
            }
        }


    }
}

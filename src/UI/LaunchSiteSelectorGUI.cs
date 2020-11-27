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


        private LaunchSiteFilter launchSiteFilter;

        public float rangekm = 0;
        public string sCurrentSite = "";

        public Vector2 sitesScrollPosition;

        private static KKLaunchSite defaultSite = null;
        internal static KKLaunchSite selectedSite;

        private string launchButtonName = "";

        Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 400, 460);

		public override void CreateUI()
		{
			base.CreateUI();

			gameObject.AddComponent<Touchable>();
			
			this.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<LaunchSiteFilter>(out launchSiteFilter)
					.Finish()
				.Finish();
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
                    if (launchSiteFilter.showFavOnly && (site.favouriteSite != "Yes"))
                    {
                        continue;
                    }

                    if (launchSiteFilter.category != site.sitecategory && !launchSiteFilter.showAllcategorys)
                    {
                        continue;
                    }

                    if (LaunchSiteManager.CheckLaunchSiteIsValid(site) == false)
                    {
                        continue;
                    }
                    if ((!launchSiteFilter.showOpen && site.isOpen) || (!launchSiteFilter.showClosed && !site.isOpen))
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

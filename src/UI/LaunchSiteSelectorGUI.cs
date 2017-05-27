using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.UI
{
    class LaunchSiteSelectorGUI : KKWindow
    {
        public Texture tFavesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapFavouritesOn", false);
        public Texture tFavesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapFavouritesOff", false);

        public Texture tHolder = null;

        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle KKWindow;
        GUIStyle BoxNoBorder;
        GUIStyle ButtonKK;
        GUIStyle KKToolTip;

        private LaunchSite selectedSite;
        public List<LaunchSite> sites;
        private SiteType editorType = SiteType.Any;

        public float rangekm = 0;
        public string sCurrentSite = "";

        public Vector2 sitesScrollPosition;

        public bool bOpenOn = true;
        public bool bClosedOn = true;
        public bool bRocketpadsOn = true;
        public bool bRunwaysOn = true;
        public bool bHelipadsOn = true;
        public bool bOtherOn = true;
        internal bool waterLaunchOn = true;
        public bool bFavesOnly = false;

        Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 400, 460);

        public override void Draw()
        {
            drawSelector();
        }

        public override void Close()
        {
            sites = null;
            InputLockManager.RemoveControlLock("KKEditorLock");
            InputLockManager.RemoveControlLock("KKEditorLock2");
            KerbalKonstructs.GUI_BaseManager.Close();
            base.Close();
        }

        public void drawSelector()
        {
            KKWindow = new GUIStyle(GUI.skin.window);
            KKWindow.padding = new RectOffset(3, 3, 5, 5);

            windowRect = GUI.Window(0xB00B1E6, windowRect, drawSelectorWindow, "", KKWindow);

            if (windowRect.Contains(Event.current.mousePosition))
            {
                InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, "KKEditorLock2");
            }
            else
            {
                InputLockManager.RemoveControlLock("KKEditorLock2");
            }
        }

        public void drawSelectorWindow(int id)
        {
            ButtonKK = new GUIStyle(GUI.skin.button);
            ButtonKK.padding.left = 0;
            ButtonKK.padding.right = 0;

            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.white;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.white;
            DeadButton.focused.textColor = Color.white;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Bold;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            KKToolTip = new GUIStyle(GUI.skin.box);
            KKToolTip.normal.textColor = Color.white;
            KKToolTip.fontSize = 11;
            KKToolTip.fontStyle = FontStyle.Normal;

            string smessage = "";

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Launchsite Selector", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    InputLockManager.RemoveControlLock("KKEditorLock");
                    InputLockManager.RemoveControlLock("KKEditorLock2");
                    this.Close();
                    return;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);

                if (MiscUtils.isCareerGame())
                {
                    if (bOpenOn) tHolder = UIMain.tOpenBasesOn;
                    else tHolder = UIMain.tOpenBasesOff;

                    if (GUILayout.Button(new GUIContent(tHolder, "Open"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    {
                        if (bOpenOn)
                        {
                            bOpenOn = false;
                            bClosedOn = true;
                        }
                        else bOpenOn = true;
                    }

                    if (bClosedOn) tHolder = UIMain.tClosedBasesOn;
                    else tHolder = UIMain.tClosedBasesOff;

                    if (GUILayout.Button(new GUIContent(tHolder, "Closed"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    {
                        if (bClosedOn)
                        {
                            bClosedOn = false;
                            bOpenOn = true;
                        }
                        else bClosedOn = true;
                    }

                    GUILayout.FlexibleSpace();
                }

                if (bFavesOnly) tHolder = tFavesOn;
                else tHolder = tFavesOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Only Favourites"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    if (bFavesOnly)
                    {
                        bFavesOnly = false;
                    }
                    else bFavesOnly = true;
                }

                GUILayout.FlexibleSpace();

                if (editorType == SiteType.SPH)
                    GUI.enabled = false;

                if (bRocketpadsOn) tHolder = UIMain.tLaunchpadsOn;
                else tHolder = UIMain.tLaunchpadsOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Rocketpads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = true;
                    bHelipadsOn = false;
                    bRunwaysOn = false;
                    bOtherOn = false;
                    waterLaunchOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "RocketPad");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (editorType == SiteType.VAB)
                    GUI.enabled = false;

                if (bRunwaysOn) tHolder = UIMain.tRunwaysOn;
                else tHolder = UIMain.tRunwaysOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Runways"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRunwaysOn = true;
                    bHelipadsOn = false;
                    bRocketpadsOn = false;
                    bOtherOn = false;
                    waterLaunchOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "Runway");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (editorType == SiteType.VAB)
                    GUI.enabled = false;

                if (bHelipadsOn) tHolder = UIMain.tHelipadsOn;
                else tHolder = UIMain.tHelipadsOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Helipads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = false;
                    bHelipadsOn = true;
                    bRunwaysOn = false;
                    bOtherOn = false;
                    waterLaunchOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "Helipad");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (editorType == SiteType.VAB)
                    GUI.enabled = false;

                if (waterLaunchOn) tHolder = UIMain.tWaterOn;
                else tHolder = UIMain.tWaterOff;

                if (GUILayout.Button(new GUIContent(tHolder, "WalterLaunch"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = false;
                    bHelipadsOn = false;
                    bRunwaysOn = false;
                    bOtherOn = false;
                    waterLaunchOn = true;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "Waterlaunch");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (bOtherOn) tHolder = UIMain.tOtherOn;
                else tHolder = UIMain.tOtherOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Other"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = false;
                    bHelipadsOn = false;
                    bRunwaysOn = false;
                    bOtherOn = true;
                    waterLaunchOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "Other");
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("ALL", GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = true;
                    bHelipadsOn = true;
                    bRunwaysOn = true;
                    bOtherOn = true;
                    waterLaunchOn = true;
                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "ALL");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            sitesScrollPosition = GUILayout.BeginScrollView(sitesScrollPosition);
            {
                if (sites == null) sites = (editorType == SiteType.Any) ?
                    LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "ALL");

                sites.Sort(delegate (LaunchSite a, LaunchSite b)
                {
                    return (a.LaunchSiteName).CompareTo(b.LaunchSiteName);
                });

                foreach (LaunchSite site in sites)
                {
                    if (bFavesOnly)
                    {
                        if (site.favouriteSite != "Yes")
                            continue;
                    }

                    if (MiscUtils.isCareerGame())
                    {
                        if (!bOpenOn)
                        {
                            if (site.isOpen)
                                continue;
                        }

                        if (!bClosedOn)
                        {
                            if (!site.isOpen)
                                continue;
                        }

                        // Don't show hidden closed Bases
                        if (site.LaunchSiteIsHidden && (!site.isOpen))
                            continue;

                        GUILayout.BeginHorizontal();
                        if (site.isOpen)
                        {
                            GUILayout.Label(UIMain.tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
                        }
                        else
                            GUILayout.Label(UIMain.tIconClosed, GUILayout.Height(30), GUILayout.Width(30));
                    }

                    GUI.enabled = !(selectedSite == site);

                    string sButtonName = "";
                    sButtonName = site.LaunchSiteName;
                    if (site.LaunchSiteName == "Runway") sButtonName = "KSC Runway";
                    if (site.LaunchSiteName == "LaunchPad") sButtonName = "KSC LaunchPad";

                    if (GUILayout.Button(sButtonName, GUILayout.Height(30)))
                    {
                        selectedSite = site;

                        if (!MiscUtils.isCareerGame())
                        {
                            LaunchSiteManager.setLaunchSite(site);
                            smessage = "Launchsite set to " + sButtonName;
                            MiscUtils.HUDMessage(smessage, 10, 2);
                        }
                    }
                    GUI.enabled = true;

                    if (MiscUtils.isCareerGame())
                    {
                        if (site.isOpen)
                        {
                            GUILayout.Label(UIMain.tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
                        }
                        else
                            GUILayout.Label(UIMain.tIconClosed, GUILayout.Height(30), GUILayout.Width(30));

                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(5);

            sCurrentSite = LaunchSiteManager.getCurrentLaunchSite();

            if (sCurrentSite != null)
            {
                if (sCurrentSite == "Runway")
                    GUILayout.Box("Current Launchsite: KSC Runway");
                else
                    if (sCurrentSite == "LaunchPad")
                    GUILayout.Box("Current Launchsite: KSC LaunchPad");
                else
                    GUILayout.Box("Current Launchsite: " + sCurrentSite);
            }

            GUI.enabled = (selectedSite != null && !(selectedSite.LaunchSiteName == sCurrentSite) && LaunchSiteManager.getIsSiteOpen(selectedSite.LaunchSiteName));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Set as Launchsite", GUILayout.Height(46)))
            {
                LaunchSiteManager.setLaunchSite(selectedSite);
                MiscUtils.HUDMessage(selectedSite.LaunchSiteName + " has been set as the launchsite", 10, 0);
            }

            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                if (editorType == SiteType.SPH)
                    GUI.enabled = (KerbalKonstructs.instance.defaultSPHlaunchsite != sCurrentSite);

                if (editorType == SiteType.VAB)
                    GUI.enabled = (KerbalKonstructs.instance.defaultVABlaunchsite != sCurrentSite);

                if (GUILayout.Button("Set as Default", GUILayout.Height(23)))
                {
                    if (sCurrentSite != null)
                    {
                        if (editorType == SiteType.SPH)
                            KerbalKonstructs.instance.defaultSPHlaunchsite = sCurrentSite;

                        if (editorType == SiteType.VAB)
                            KerbalKonstructs.instance.defaultVABlaunchsite = sCurrentSite;
                    }
                }
                GUI.enabled = true;

                LaunchSite DefaultSite = null;

                if (GUILayout.Button("Use Default", GUILayout.Height(23)))
                {
                    if (editorType == SiteType.SPH)
                    {
                        foreach (LaunchSite site in sites)
                        {
                            if (site.LaunchSiteName == KerbalKonstructs.instance.defaultSPHlaunchsite)
                                DefaultSite = site;
                        }

                        if (DefaultSite != null)
                        {
                            if (MiscUtils.isCareerGame())
                            {
                                if (!DefaultSite.isOpen)
                                {
                                    smessage = "Default site is closed.";
                                    MiscUtils.HUDMessage(smessage, 10, 0);
                                }
                                else
                                    LaunchSiteManager.setLaunchSite(DefaultSite);
                            }
                            else
                                LaunchSiteManager.setLaunchSite(DefaultSite);
                        }
                    }

                    if (editorType == SiteType.VAB)
                    {
                        foreach (LaunchSite site in sites)
                        {
                            if (site.LaunchSiteName == KerbalKonstructs.instance.defaultVABlaunchsite)
                                DefaultSite = site;
                        }

                        if (DefaultSite != null)
                        {
                            if (MiscUtils.isCareerGame())
                            {
                                if (!DefaultSite.isOpen)
                                {
                                    smessage = "Default site is closed.";
                                    MiscUtils.HUDMessage(smessage, 10, 0);
                                }
                                else
                                    LaunchSiteManager.setLaunchSite(DefaultSite);
                            }
                            else
                                LaunchSiteManager.setLaunchSite(DefaultSite);
                        }
                    }

                    if (DefaultSite != null)
                    {
                        smessage = DefaultSite.LaunchSiteName + " has been set as the launchsite";
                        MiscUtils.HUDMessage(smessage, 10, 0);
                    }
                    else
                    {
                        smessage = "KK could not determine the default launchsite.";
                        MiscUtils.HUDMessage(smessage, 10, 0);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Box(UIMain.tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUI.enabled = true;

            if (selectedSite != null)
            {
                BaseManager.setSelectedSite(selectedSite);
                KerbalKonstructs.GUI_BaseManager.Open();
            }
            else
            {
                if (LaunchSiteManager.getLaunchSites().Count > 0)
                {
                    selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
                    LaunchSiteManager.setLaunchSite(selectedSite);
                    BaseManager.setSelectedSite(selectedSite);
                    KerbalKonstructs.GUI_BaseManager.Open();
                }
                else
                {
                    Log.UserError("ERROR Launch Selector cannot find KSC Runway or Launch Pad! PANIC! Runaway! Hide!");
                }
            }

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 5, labelSize.y + 6), GUI.tooltip, KKToolTip);
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        public void setEditorType(SiteType type)
        {
            editorType = (KerbalKonstructs.instance.launchFromAnySite) ? SiteType.Any : type;
            if (selectedSite != null)
            {
                if (selectedSite.LaunchSiteType != editorType && selectedSite.LaunchSiteType != SiteType.Any)
                {
                    selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
                }

                // if (!isCareerGame())
                LaunchSiteManager.setLaunchSite(selectedSite);
            }
        }

    }
}
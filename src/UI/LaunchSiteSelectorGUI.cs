using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.UI
{
    class LaunchSiteSelectorGUI : KKWindow
    {
        public Texture tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
        public Texture tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
        public Texture tSet = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/setaslaunchsite", false);
        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
        public Texture tOpenBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOn", false);
        public Texture tOpenBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOff", false);
        public Texture tClosedBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOn", false);
        public Texture tClosedBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOff", false);
        public Texture tHelipadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOn", false);
        public Texture tHelipadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOff", false);
        public Texture tRunwaysOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOn", false);
        public Texture tRunwaysOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOff", false);
        public Texture tTrackingOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOn", false);
        public Texture tTrackingOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOff", false);
        public Texture tLaunchpadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOn", false);
        public Texture tLaunchpadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOff", false);
        public Texture tOtherOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOn", false);
        public Texture tOtherOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOff", false);
        public Texture tFavesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapFavouritesOn", false);
        public Texture tFavesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapFavouritesOff", false);
        public Texture2D tNormalButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonNormal", false);
        public Texture2D tHoverButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonHover", false);

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
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);

                if (MiscUtils.isCareerGame())
                {
                    if (bOpenOn) tHolder = tOpenBasesOn;
                    else tHolder = tOpenBasesOff;

                    if (GUILayout.Button(new GUIContent(tHolder, "Open"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    {
                        if (bOpenOn)
                        {
                            bOpenOn = false;
                            bClosedOn = true;
                        }
                        else bOpenOn = true;
                    }

                    if (bClosedOn) tHolder = tClosedBasesOn;
                    else tHolder = tClosedBasesOff;

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

                if (bRocketpadsOn) tHolder = tLaunchpadsOn;
                else tHolder = tLaunchpadsOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Rocketpads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = true;
                    bHelipadsOn = false;
                    bRunwaysOn = false;
                    bOtherOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "RocketPad");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (editorType == SiteType.VAB)
                    GUI.enabled = false;

                if (bRunwaysOn) tHolder = tRunwaysOn;
                else tHolder = tRunwaysOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Runways"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRunwaysOn = true;
                    bHelipadsOn = false;
                    bRocketpadsOn = false;
                    bOtherOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "Runway");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (editorType == SiteType.VAB)
                    GUI.enabled = false;

                if (bHelipadsOn) tHolder = tHelipadsOn;
                else tHolder = tHelipadsOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Helipads"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = false;
                    bHelipadsOn = true;
                    bRunwaysOn = false;
                    bOtherOn = false;

                    sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() :
                        LaunchSiteManager.getLaunchSites(editorType, true, "Helipad");
                }

                GUI.enabled = true;
                GUILayout.Space(2);

                if (bOtherOn) tHolder = tOtherOn;
                else tHolder = tOtherOff;

                if (GUILayout.Button(new GUIContent(tHolder, "Other"), ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                {
                    bRocketpadsOn = false;
                    bHelipadsOn = false;
                    bRunwaysOn = false;
                    bOtherOn = true;

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
                    return (a.name).CompareTo(b.name);
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
                            if (site.openCloseState == "Open" || site.openCost == 0)
                                continue;
                        }

                        if (!bClosedOn)
                        {
                            if (site.openCloseState == "Closed")
                                continue;
                        }

                        // Don't show hidden closed Bases
                        if (site.isHidden && (site.openCloseState == "Closed"))
                            continue;

                        GUILayout.BeginHorizontal();
                        if (site.openCloseState == "Open" || site.openCost == 0 && site.openCloseState != "OpenLocked" && site.openCloseState != "ClosedLocked")
                        {
                            site.openCloseState = "Open";
                            GUILayout.Label(tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
                        }
                        else
                            GUILayout.Label(tIconClosed, GUILayout.Height(30), GUILayout.Width(30));
                    }

                    GUI.enabled = !(selectedSite == site);

                    string sButtonName = "";
                    sButtonName = site.name;
                    if (site.name == "Runway") sButtonName = "KSC Runway";
                    if (site.name == "LaunchPad") sButtonName = "KSC LaunchPad";

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
                        if (site.openCloseState == "Open" || site.openCost == 0 && site.openCloseState != "OpenLocked" && site.openCloseState != "ClosedLocked")
                        {
                            site.openCloseState = "Open";
                            GUILayout.Label(tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
                        }
                        else
                            GUILayout.Label(tIconClosed, GUILayout.Height(30), GUILayout.Width(30));

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

            GUI.enabled = (selectedSite != null && !(selectedSite.name == sCurrentSite) && LaunchSiteManager.getIsSiteOpen(selectedSite.name));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Set as Launchsite", GUILayout.Height(46)))
            {
                LaunchSiteManager.setLaunchSite(selectedSite);
                MiscUtils.HUDMessage(selectedSite.name + " has been set as the launchsite", 10, 0);
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
                            if (site.name == KerbalKonstructs.instance.defaultSPHlaunchsite)
                                DefaultSite = site;
                        }

                        if (DefaultSite != null)
                        {
                            if (MiscUtils.isCareerGame())
                            {
                                if (DefaultSite.openCloseState == "OpenLocked" || DefaultSite.openCloseState == "ClosedLocked" || (DefaultSite.openCloseState != "Open" && DefaultSite.openCost != 0))
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
                            if (site.name == KerbalKonstructs.instance.defaultVABlaunchsite)
                                DefaultSite = site;
                        }

                        if (DefaultSite != null)
                        {
                            if (MiscUtils.isCareerGame())
                            {
                                if (DefaultSite.openCloseState == "OpenLocked" || DefaultSite.openCloseState == "ClosedLocked" || (DefaultSite.openCloseState != "Open" && DefaultSite.openCost != 0))
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
                        smessage = DefaultSite.name + " has been set as the launchsite";
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
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

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
                if (selectedSite.type != editorType && selectedSite.type != SiteType.Any)
                {
                    selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
                }

                // if (!isCareerGame())
                LaunchSiteManager.setLaunchSite(selectedSite);
            }
        }

    }
}
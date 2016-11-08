using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.Utilities;
using KSP.UI.Screens;

namespace KerbalKonstructs.NGUI
{
    class ToolbarController
    {
        static internal KerbalKonstructs main = KerbalKonstructs.instance;
        private static ApplicationLauncherButton masterButton;


        public void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready)
            {
                bool vis;
                if (masterButton == null || !ApplicationLauncher.Instance.Contains(masterButton, out vis))
                    masterButton = ApplicationLauncher.Instance.AddModApplication(ToggleButtonOn, ToggleButtonOff,
                        OnHover, null, null, null,
                        ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.FLIGHT,
                        GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SiteToolbarIcon", false));

            }
        }

        internal static void ToggleButtonOn()
        {
            
            if  ( (!main.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                WindowManager.instance.OpenWindow(WindowManager.instance.GUI_LaunchSiteSelector.drawSelector);
            }
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                main.showFlightManager = true;
            }

            if ( (HighLogic.LoadedScene == GameScenes.TRACKSTATION) || (MapView.MapIsEnabled || MapView.MapCamera != null) )
            {
                main.showMapIconManager = true;
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                WindowManager.instance.OpenWindow(WindowManager.instance.GUI_KSCManager.drawKSCManager);
            }
        }

        internal static void ToggleButtonOff()
        {

            if ((!main.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                WindowManager.instance.CloseWindow(WindowManager.instance.GUI_LaunchSiteSelector.drawSelector);
            }
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                main.showFlightManager = false;
            }

            if ((HighLogic.LoadedScene == GameScenes.TRACKSTATION) || (MapView.MapIsEnabled || MapView.MapCamera != null))
            {
                main.showMapIconManager = false;
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                WindowManager.instance.CloseWindow(WindowManager.instance.GUI_KSCManager.drawKSCManager);
            }
        }

        internal void OnHover()
        {

            if ((!main.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
                MiscUtils.HUDMessage(hovermessage, 10, 0);
            }
        }

    }
}

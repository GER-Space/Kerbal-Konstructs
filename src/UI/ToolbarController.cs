using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.Utilities;
using KSP.UI.Screens;

namespace KerbalKonstructs.UI
{
    class ToolbarController
    {
        static internal KerbalKonstructs KKmain = KerbalKonstructs.instance;
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
            
            if  ( (!KKmain.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                WindowManager.instance.OpenWindow(KKmain.GUI_LaunchSiteSelector.drawSelector);
            }
            if ( (HighLogic.LoadedScene == GameScenes.FLIGHT) && (!MapView.MapIsEnabled) )
            {
                WindowManager.instance.OpenWindow(KKmain.GUI_FlightManager.drawManager);
            }

            if ( (HighLogic.LoadedScene == GameScenes.TRACKSTATION) || (MapView.MapIsEnabled) )
            {
                WindowManager.instance.OpenWindow(KerbalKonstructs.instance.GUI_MapIconManager.drawManager);
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                WindowManager.instance.OpenWindow(KKmain.GUI_KSCManager.drawKSCManager);
            }
        }

        internal static void ToggleButtonOff()
        {

            if ((!KKmain.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                WindowManager.instance.CloseWindow(KKmain.GUI_LaunchSiteSelector.drawSelector);
                WindowManager.instance.CloseWindow(KKmain.GUI_BaseManager.drawBaseManager);
            }
            if ( (HighLogic.LoadedScene == GameScenes.FLIGHT) && (!MapView.MapIsEnabled) )
            {
                WindowManager.instance.CloseWindow(KKmain.GUI_FlightManager.drawManager);
            }

            if ((HighLogic.LoadedScene == GameScenes.TRACKSTATION) || (MapView.MapIsEnabled) )
            {
                WindowManager.instance.CloseWindow(KerbalKonstructs.instance.GUI_MapIconManager.drawManager);
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                WindowManager.instance.CloseWindow(KKmain.GUI_KSCManager.drawKSCManager);
                WindowManager.instance.CloseWindow(KerbalKonstructs.instance.GUI_Settings.drawKKSettingsGUI);
            }
        }

        internal void OnHover()
        {

            if ((!KKmain.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
                MiscUtils.HUDMessage(hovermessage, 10, 0);
            }
        }

    }
}

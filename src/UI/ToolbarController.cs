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
            
            if  (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                LaunchSiteSelectorGUI.instance.Open();
                return;
            }
            if ( (HighLogic.LoadedScene == GameScenes.FLIGHT) && (!MapView.MapIsEnabled) )
            {
                BaseBossFlight.instance.Open();
                return;
            }

            if ( (HighLogic.LoadedScene == GameScenes.TRACKSTATION) || (MapView.MapIsEnabled) )
            {
                Modules.MapIconManager.instance.Open();
                return;
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                KSCManager.instance.Open();
                return;
            }
        }

        internal static void ToggleButtonOff()
        {

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                LaunchSiteSelectorGUI.instance.Close();
                BaseManager.instance.Close();
                return;
            }
            if ( (HighLogic.LoadedScene == GameScenes.FLIGHT) && (!MapView.MapIsEnabled) )
            {
                BaseBossFlight.instance.Close();
                return;
            }

            if ((HighLogic.LoadedScene == GameScenes.TRACKSTATION) || (MapView.MapIsEnabled) )
            {
                Modules.MapIconManager.instance.Close();
                return;
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                KSCManager.instance.Close();
                return;
            }
        }

        internal void OnHover()
        {

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
                MiscUtils.HUDMessage(hovermessage, 10, 0);
            }
        }

    }
}

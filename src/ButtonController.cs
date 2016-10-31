using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs
{
    class ButtonController
    {
        static internal KerbalKonstructs main = KerbalKonstructs.instance;
        public static void ToggleButtonOn()
        {
            
            if  ( (!main.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                main.showSiteSelector = true;
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
                main.showKSCmanager = true;
            }
        }

        public static void ToggleButtonOff()
        {

            if ((!main.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                main.showSiteSelector = false;
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
                main.showKSCmanager = false;
            }
        }

        public static void OnHover()
        {

            if ((!main.disableCustomLaunchsites) && (HighLogic.LoadedScene == GameScenes.EDITOR))
            {
                string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
                MiscUtils.HUDMessage(hovermessage, 10, 0);
            }
        }

    }
}

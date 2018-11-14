using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.UI
{
    class MapIconSelector
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "IconSelector";
        internal static string windowMessage = "choose action";
        internal static string windowTitle = "Kerbal Konstructs";

        internal static Rect windowRect = new Rect(0.5f, 0.5f, 150f, 60f);

        internal static float windowWidth = 150f;
        internal static float windowHeight = 50f;

        internal static KKLaunchSite selectedSite;
        internal static StaticInstance staticInstance;
        internal static bool useLaunchSite;


        internal static void CreateContent()
        {
            if (useLaunchSite)
            {
                windowMessage = selectedSite.LaunchSiteName + "\n" + "Status: " + selectedSite.OpenCloseState;
            }
            else
            {
                windowMessage = staticInstance.GetFacility(KKFacilityType.RecoveryBase).FacilityName + "\n" + "Status: " + staticInstance.GetFacility(KKFacilityType.RecoveryBase).OpenCloseState;
            }

            content = new List<DialogGUIBase>();

            content.Add(new DialogGUIFlexibleSpace());
            content.Add(new DialogGUIVerticalLayout(
                           new DialogGUIFlexibleSpace(),
                           new DialogGUIButton("Create a Waypoint", CreateWayPoint, 140.0f, 30.0f, true),
                           new DialogGUIButton("Open BaseManager", OpenBaseManager, 140.0f, 30.0f, true),
                           new DialogGUIButton("Close", () =>
                           {
                           }, 140.0f, 30.0f, true)
                           ));
        }


        internal static void CreateMultiOptionDialog()
        {
            //optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, HighLogic.UISkin, windowRect, content.ToArray());
            optionDialog = new MultiOptionDialog(windowMessage, windowTitle, HighLogic.UISkin, windowRect, content.ToArray());
        }


        internal static void CreatePopUp()
        {
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), optionDialog,
                   false,
                   HighLogic.UISkin, false);
        }


        internal static Vector2 CreateRectUnderMouse()
        {

            Vector2 mousePos = Event.current.mousePosition;
            Vector2 pos = new Vector2();

            pos﻿.x = (mousePos.x - (windowWidth / 2)) / Screen.width;
            //we need to invert the height
            pos.y = 1 - (mousePos.y / Screen.height);

            return (pos);
        }



        internal static void Open()
        {
            windowRect = new Rect(CreateRectUnderMouse(), new Vector2(windowWidth, windowHeight));

            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
        }


        internal static void Close()
        {
            if (dialog != null)
            {
                dialog.Dismiss();
            }
            dialog = null;
            selectedSite = null;
            staticInstance = null;
        }


        internal static void CreateWayPoint()
        {
            if (useLaunchSite && selectedSite != null)
            {
                CreateWPForLaunchSite(selectedSite);
            }

            if (!useLaunchSite && staticInstance != null)
            {
                CreateWPForBase(staticInstance);
            }
        }


        internal static void CreateWPForLaunchSite(KKLaunchSite site)
        {
            FinePrint.Waypoint lsWP = new FinePrint.Waypoint();

            //lsWP.altitude = selectedSite.staticInstance.surfaceHeight + 20f;
            lsWP.altitude = 0;
            lsWP.height = 20f;
            lsWP.isOnSurface = true;
            lsWP.latitude = site.staticInstance.RefLatitude;
            lsWP.longitude = site.staticInstance.RefLongitude;
            lsWP.name = site.LaunchSiteName;
            lsWP.celestialName = site.body.name;
            lsWP.isNavigatable = true;

            lsWP.navigationId = Guid.NewGuid();
            lsWP.enableMarker = true;
            lsWP.fadeRange = site.body.Radius;
            lsWP.seed = site.LaunchSiteName.GetHashCode();
            lsWP.isCustom = true;

            switch (site.sitecategory)
            {
                case LaunchSiteCategory.RocketPad:
                    lsWP.id = "Squad/PartList/SimpleIcons/R&D_node_icon_heavierrocketry";
                    break;
                case LaunchSiteCategory.Runway:
                    lsWP.id = "Squad/PartList/SimpleIcons/RDicon_aerospaceTech2";
                    break;
                case LaunchSiteCategory.Helipad:
                    lsWP.id = "Squad/PartList/SimpleIcons/R&D_node_icon_landing";
                    break;
                default:
                    lsWP.id = "custom";
                    break;
            }

            FinePrint.WaypointManager.AddWaypoint(lsWP);
            site.wayPoint = lsWP;
        }

        internal static void CreateWPForBase(StaticInstance instance)
        {
            FinePrint.Waypoint lsWP = new FinePrint.Waypoint();
            //lsWP.altitude = selectedSite.staticInstance.surfaceHeight + 20f;
            lsWP.altitude = 0;
            lsWP.height = 20f;
            lsWP.isOnSurface = true;
            lsWP.latitude = instance.RefLatitude;
            lsWP.longitude = instance.RefLongitude;
            lsWP.name = instance.GetFacility(KKFacilityType.RecoveryBase).FacilityName;
            lsWP.celestialName = instance.CelestialBody.name;
            lsWP.isNavigatable = true;

            lsWP.navigationId = Guid.NewGuid();
            lsWP.enableMarker = true;
            lsWP.fadeRange = instance.CelestialBody.Radius;
            lsWP.seed = lsWP.name.GetHashCode();
            lsWP.isCustom = true;

            lsWP.id = "Squad/PartList/SimpleIcons/R&D_node_icon_heavierrocketry";

            FinePrint.WaypointManager.AddWaypoint(lsWP);
        }


        internal static void OpenBaseManager()
        {
            if (useLaunchSite)
            {
                BaseManager.selectedSite = selectedSite;
                BaseManager.instance.Open();
            }
            else
            {
                FacilityManager.selectedInstance = staticInstance;
                FacilityManager.instance.Open();
            }
        }

    }
}

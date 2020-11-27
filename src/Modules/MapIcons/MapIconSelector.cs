using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class MapIconSelector : Window
    {
        internal static string windowName = "IconSelector";
        internal static string windowMessage = "choose action";
        internal static string windowTitle = "Kerbal Konstructs";

        internal static float windowWidth = 150f;
        internal static float windowHeight = 50f;

        internal static KKLaunchSite selectedSite;
        internal static StaticInstance staticInstance;
        internal static bool useLaunchSite;

		internal static PointerEventData eventData;

		static MapIconSelector instance;

		UIText message;

		public override void CreateUI()
		{
			base.CreateUI();

			Title(windowTitle)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.BottomLeft)
				.Pivot(PivotPresets.BottomLeft)
				.PreferredWidth(windowWidth)
				.SetSkin ("KK.Default")

				.Add<UIText>(out message)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.CreateWaypoint)
					.OnClick(CreateWayPoint)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.OpenBaseManager)
					.OnClick(OpenBaseManager)
					.FlexibleLayout(true, false)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.Close)
					.OnClick(Close)
					.FlexibleLayout(true, false)
					.Finish()
				.Finish();
		}

		public override void Style()
		{
			base.Style();
		}


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
			instance.message.Text(windowMessage);
        }


        internal static void Open()
        {
			if (instance == null) {
				instance = UIKit.CreateUI<MapIconSelector>(UIMain.appCanvasRect, "KKMapIconSelector");
			}

			instance.rectTransform.anchoredPosition3D = eventData.position;
            CreateContent();
			instance.SetActive(true);
        }


        internal static void Close()
        {
            selectedSite = null;
            staticInstance = null;
			if (instance) {
				instance.SetActive(false);
			}
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs;
using KerbalKonstructs.UI;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;


namespace KerbalKonstructs.Modules
{
    class MapIconDraw : KKWindow
    {

        private Boolean displayingTooltip = false;
        private Boolean displayingTooltip2 = false;

        static LaunchSite selectedSite = null;
        static StaticObject selectedFacility = null;

        internal static bool mapHideIconsBehindBody = true;

        private static List<StaticObject> groundStations;
        private static LaunchSite[] lauchSites;

        public override void Draw()
        {
            if ((!KerbalKonstructs.instance.toggleIconsWithBB) || (KerbalKonstructs.instance.toggleIconsWithBB && this.IsOpen()))
            {
                drawTrackingStations();
                DrawLaunchsites();
            }
        }

        public override void Open()
        {
            CacheGroundStations();
            CacheLaunchSites();
            base.Open();
        }

        /// <summary>
        /// Generates a list of available groundstations and saves this in the this.groundStations
        /// </summary>
        private void CacheGroundStations()
        {
            groundStations = new List<StaticObject>(15);

            // Test for later
            //var bla = Resources.FindObjectsOfTypeAll<GroundStation>();

            foreach (StaticObject instance in StaticDatabase.allStaticInstances)
            {
                if (instance.facilityType != KKFacilityType.GroundStation && instance.facilityType != KKFacilityType.TrackingStation)
                {
                    continue;                  
                }                   

                if (instance.Group == "KSCUpgrades")
                    continue;

                if (((GroundStation)instance.myFacilities[0]).TrackingShort == 0f)
                    continue;

                groundStations.Add(instance);
            }
           // Log.Normal("GS: Cached GroundStations: " + groundStations.Count.ToString());
        }

        /// <summary>
        /// Caches the launchsites for later use
        /// </summary>
        private void CacheLaunchSites()
        {
            lauchSites = LaunchSiteManager.getLaunchSites().ToArray();
        }



        /// <summary>
        /// Draws the groundStation icons on the map
        /// </summary>
        public void drawTrackingStations()
        {
            if (!MiscUtils.isCareerGame())
                return;
            if (!KerbalKonstructs.instance.mapShowOpenT)
                return;

            bool display2 = false;
            string openclosed3 = "Closed";
            CelestialBody body = PlanetariumCamera.fetch.target.GetReferenceBody();
            StaticObject groundStation;

            displayingTooltip2 = false;

            // Do tracking stations first
            for (int i = 0; i < groundStations.Count; i++)
            {
                groundStation = groundStations[i];
                if ((mapHideIconsBehindBody) && (IsOccluded(groundStation.gameObject.transform.position, body)))
                {
                    continue;
                }

                openclosed3 = ((GroundStation)groundStation.myFacilities[0]).OpenCloseState;


                if (KerbalKonstructs.instance.mapShowOpenT)
                    display2 = true;
                if (!KerbalKonstructs.instance.mapShowClosed && openclosed3 == "Closed")
                    display2 = false;
                if (!KerbalKonstructs.instance.mapShowOpen && openclosed3 == "Open")
                    display2 = false;

                if (!display2)
                    continue;

                Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(groundStation.gameObject.transform.position));

                Rect screenRect6 = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;
                float fRadarRadius = 12800 / fPosZ;

                if (fRadarRadius > 15) GUI.DrawTexture(screenRect6, UIMain.TrackingStationIcon, ScaleMode.ScaleToFit, true);


                if (screenRect6.Contains(Event.current.mousePosition) && !displayingTooltip2)
                {


                    var objectpos2 = groundStation.CelestialBody.transform.InverseTransformPoint(groundStation.gameObject.transform.position);
                    //var dObjectLat2 = NavUtils.GetLatitude(objectpos2);
                    //var dObjectLon2 = NavUtils.GetLongitude(objectpos2);
                    //var disObjectLat2 = dObjectLat2 * 180 / Math.PI;
                    //var disObjectLon2 = dObjectLon2 * 180 / Math.PI;

                    var disObjectLat2 = KKMath.GetLatitudeInDeg(objectpos2);
                    var disObjectLon2 = KKMath.GetLongitudeInDeg(objectpos2);

                    if (disObjectLon2 < 0) disObjectLon2 = disObjectLon2 + 360;

                    //Only display one tooltip at a time
                    DisplayMapIconToolTip("Tracking Station " + "\n(Lat." + disObjectLat2.ToString("#0.00") + "/ Lon." + disObjectLon2.ToString("#0.00") + ")", pos);

                    if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
                    {                    
                        selectedFacility = groundStation;
                        FacilityManager.selectedFacility = groundStation;
                        KerbalKonstructs.GUI_FacilityManager.Open();
                    }
                }
            }
        }


        public void DrawLaunchsites()
        {
            displayingTooltip = false;
            LaunchSite launchSite;
            CelestialBody body = PlanetariumCamera.fetch.target.GetReferenceBody();
            string openclosed, category;

            // Then do launchsites
            for (int index = 0; index < lauchSites.Length; index++)
            {
                launchSite = lauchSites[index];
                // check if we should display the site or not this is the fastst check, so it shoud be first
                openclosed = launchSite.OpenCloseState;
                category = launchSite.Category;

                if (!KerbalKonstructs.instance.mapShowHelipads && category == "Helipad")
                    continue;
                if (!KerbalKonstructs.instance.mapShowOther && ((category == "Other") || (category == "Waterlaunch")))
                    continue;
                if (!KerbalKonstructs.instance.mapShowRocketbases && category == "RocketPad")
                    continue;
                if (!KerbalKonstructs.instance.mapShowRunways && category == "Runway")
                    continue;

                if (MiscUtils.isCareerGame())
                {
                    if (!KerbalKonstructs.instance.mapShowOpen && openclosed == "Open")
                        continue;
                    if (!KerbalKonstructs.instance.mapShowClosed && openclosed == "Closed")
                        continue;
                    if (KerbalKonstructs.instance.disableDisplayClosed && openclosed == "Closed")
                        continue;
                    if (openclosed == "OpenLocked" || openclosed == "ClosedLocked")
                        continue;
                    // don't show hidden bases when closed
                    if (launchSite.LaunchSiteIsHidden && (launchSite.OpenCloseState == "Closed"))
                        continue;
                }

                Vector3 launchSitePosition = (Vector3)launchSite.body.GetWorldSurfacePosition(launchSite.refLat, launchSite.refLon, launchSite.refAlt) - MapView.MapCamera.GetComponent<Camera>().transform.position;

                if (mapHideIconsBehindBody && IsOccluded(launchSitePosition, body))
                {
                    continue;
                }

                Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(launchSitePosition));

                Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;

                float fRadarRadius = 12800 / fPosZ;
                float fRadarOffset = fRadarRadius / 2;


                if (launchSite.icon != null)
                {
                    if (fRadarRadius > 15)
                        GUI.DrawTexture(screenRect, launchSite.icon, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    if (fRadarRadius > 15)
                    {
                        switch (category)
                        {
                            case "RocketPad":
                                GUI.DrawTexture(screenRect, UIMain.VABIcon, ScaleMode.ScaleToFit, true);
                                break;
                            case "Runway":
                                GUI.DrawTexture(screenRect, UIMain.runWayIcon, ScaleMode.ScaleToFit, true);
                                break;
                            case "Helipad":
                                GUI.DrawTexture(screenRect, UIMain.heliPadIcon, ScaleMode.ScaleToFit, true);
                                break;
                            case "Waterlaunch":
                                GUI.DrawTexture(screenRect, UIMain.waterLaunchIcon, ScaleMode.ScaleToFit, true);
                                break;
                            default:
                                GUI.DrawTexture(screenRect, UIMain.ANYIcon, ScaleMode.ScaleToFit, true);
                                break;
                        }
                    }
                }

                // Tooltip
                if (screenRect.Contains(Event.current.mousePosition) && !displayingTooltip)
                {
                    //Only display one tooltip at a time
                    string sToolTip = "";
                    sToolTip = launchSite.LaunchSiteName;
                    if (launchSite.LaunchSiteName == "Runway") sToolTip = "KSC Runway";
                    if (launchSite.LaunchSiteName == "LaunchPad") sToolTip = "KSC LaunchPad";
                    DisplayMapIconToolTip(sToolTip, pos);

                    // Select a base by clicking on the icon
                    if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
                    {
                        MiscUtils.HUDMessage("Selected base is " + sToolTip + ".", 5f, 3);
                        BaseManager.setSelectedSite(launchSite);
                        selectedSite = launchSite;
                        NavGuidanceSystem.setTargetSite(selectedSite);
                        KerbalKonstructs.GUI_BaseManager.Open();
                    }
                }
            }
        }


        private bool IsOccluded(Vector3d loc, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
                return false;

            return true;
        }

        public static LaunchSite getSelectedSite()
        {
            LaunchSite thisSite = selectedSite;
            return thisSite;
        }

        public void DisplayMapIconToolTip(string sitename, Vector3 pos)
        {
            displayingTooltip = true;
            GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 210, 25), sitename);
        }



    }

}

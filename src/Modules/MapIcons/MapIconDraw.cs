using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace KerbalKonstructs.Modules
{
    class MapIconDraw : KKWindow
    {
        private static MapIconDraw _instance = null;
        internal static MapIconDraw instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapIconDraw();

                }
                return _instance;
            }
        }

        private Boolean displayingTooltip = false;
        private Boolean displayingTooltip2 = false;

        static StaticInstance selectedFacility = null;

        internal static bool mapHideIconsBehindBody = true;

        private static List<StaticInstance> groundStations;
        private static KKLaunchSite[] lauchSites;

        private static CustomSpaceCenter[] spaceCenters;

        private KKLaunchSite launchSite;
        private CelestialBody body;
        private StaticInstance groundStation;
        private bool isOpen = false;

        private bool cscIsOpen = false;

        private const float KerbinRadius = 600000f;

        Rect screenRect;

        public override void Draw()
        {
            if ((!KerbalKonstructs.instance.toggleIconsWithBB) || (KerbalKonstructs.instance.toggleIconsWithBB && this.IsOpen()))
            {
                drawTrackingStations();
                DrawLaunchsites();
                DrawSpaceCenters();
            }
        }

        public override void Open()
        {
            CacheGroundStations();
            CacheObjects();
            base.Open();
        }

        /// <summary>
        /// Generates a list of available groundstations and saves this in the this.groundStations
        /// </summary>
        private void CacheGroundStations()
        {
            groundStations = new List<StaticInstance>(15);

            // Test for later
            //var bla = Resources.FindObjectsOfTypeAll<GroundStation>();

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
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
        }

        /// <summary>
        /// Caches the launchsites for later use
        /// </summary>
        private void CacheObjects()
        {
            lauchSites = LaunchSiteManager.allLaunchSites;
            spaceCenters = SpaceCenterManager.spaceCenters.ToArray();

        }



        /// <summary>
        /// Draws the groundStation icons on the map
        /// </summary>
        public void drawTrackingStations()
        {
            if (CareerUtils.isSandboxGame)
            {
                KerbalKonstructs.instance.mapShowClosed = true;
                KerbalKonstructs.instance.mapShowOpen = true;
            }

            if (!KerbalKonstructs.instance.mapShowGroundStation)
            {
                return;
            }

            body = PlanetariumCamera.fetch.target.GetReferenceBody();

            displayingTooltip2 = false;

            // Do tracking stations first
            for (int i = 0; i < groundStations.Count; i++)
            {
                groundStation = groundStations[i];
                if ((mapHideIconsBehindBody) && (IsOccluded(groundStation.position, body)))
                {
                    continue;
                }

                if (groundStation.CelestialBody != body) continue;

                isOpen = ((GroundStation)groundStation.myFacilities[0]).isOpen;


                if (!KerbalKonstructs.instance.mapShowClosed && !isOpen)
                {
                    continue;
                }

                if (!KerbalKonstructs.instance.mapShowOpen && isOpen)
                {
                    continue;
                }

                if (groundStation.groupCenter.isHidden && !isOpen || groundStation.groupCenter.isHidden && (((GroundStation)groundStation.myFacilities[0]).OpenCost == 0))
                {
                    continue;
                }

				Vector3d worldPos = groundStation.CelestialBody.GetWorldSurfacePosition(groundStation.RefLatitude, groundStation.RefLongitude, groundStation.RadiusOffset);
				Vector3d scaledSpacePos = ScaledSpace.LocalToScaledSpace(worldPos);
				Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(scaledSpacePos);

                Rect screenRect6 = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;
                float fRadarRadius = (12800 / fPosZ) * ((float)groundStation.CelestialBody.Radius / KerbinRadius);

                if (fRadarRadius > 15) GUI.DrawTexture(screenRect6, UIMain.TrackingStationIcon, ScaleMode.ScaleToFit, true);


                if (screenRect6.Contains(Event.current.mousePosition) && !displayingTooltip2)
                {


                    var objectpos2 = groundStation.CelestialBody.transform.InverseTransformPoint(groundStation.position);

                    var disObjectLat2 = KKMath.GetLatitudeInDeg(objectpos2);
                    var disObjectLon2 = KKMath.GetLongitudeInDeg(objectpos2);

                    if (disObjectLon2 < 0) disObjectLon2 = disObjectLon2 + 360;

                    //Only display one tooltip at a time
                    DisplayMapIconToolTip("Tracking Station " + "\n(Lat." + disObjectLat2.ToString("#0.00") + "/ Lon." + disObjectLon2.ToString("#0.00") + ")", pos);

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        selectedFacility = groundStation;
                        FacilityManager.selectedInstance = groundStation;
                        FacilityManager.instance.Open();
                    }
                }
            }
        }


        public void DrawLaunchsites()
        {
            displayingTooltip = false;
            body = PlanetariumCamera.fetch.target.GetReferenceBody();

            bool isOpen = false;

            // Then do launchsites
            for (int index = 0; index < lauchSites.Length; index++)
            {
                launchSite = lauchSites[index];
                // check if we should display the site or not this is the fastst check, so it shoud be first
                isOpen = launchSite.isOpen;

                if (!KerbalKonstructs.instance.mapShowHelipads && launchSite.sitecategory == LaunchSiteCategory.Helipad)
                    continue;
                if (!KerbalKonstructs.instance.mapShowOther && (launchSite.sitecategory == LaunchSiteCategory.Other))
                    continue;
                if (!KerbalKonstructs.instance.mapShowWaterLaunch && (launchSite.sitecategory == LaunchSiteCategory.Waterlaunch))
                    continue;
                if (!KerbalKonstructs.instance.mapShowRocketbases && launchSite.sitecategory == LaunchSiteCategory.RocketPad)
                    continue;
                if (!KerbalKonstructs.instance.mapShowRunways && launchSite.sitecategory == LaunchSiteCategory.Runway)
                    continue;

                if (MiscUtils.isCareerGame())
                {
                    if (!KerbalKonstructs.instance.mapShowOpen && isOpen)
                        continue;
                    if (!KerbalKonstructs.instance.mapShowClosed && !isOpen)
                        continue;
                    // don't show hidden bases when closed
                    if (launchSite.LaunchSiteIsHidden && !isOpen)
                        continue;
                }

                //   launchSitePosition = (Vector3)launchSite.lsGameObject.transform.position - MapView.MapCamera.GetComponent<Camera>().transform.position;
                Vector3d launchSitePosition = launchSite.body.GetWorldSurfacePosition(launchSite.staticInstance.RefLatitude, launchSite.staticInstance.RefLongitude, launchSite.staticInstance.RadiusOffset);

                if (mapHideIconsBehindBody && IsOccluded(launchSitePosition, body))
                {
                    continue;
                }

                if (launchSite.body != body) continue;

                Vector3 lsPosition = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(launchSitePosition));
                screenRect = new Rect((lsPosition.x - 8), (Screen.height - lsPosition.y) - 8, 16, 16);

                // Distance between camera and spawnpoint sort of
                float fPosZ = lsPosition.z;

                float fRadarRadius = (12800 / fPosZ) * ((float)launchSite.body.Radius / KerbinRadius);


                if (launchSite.icon != null)
                {
                    if (fRadarRadius > 15)
                        GUI.DrawTexture(screenRect, launchSite.icon, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    if (fRadarRadius > 15)
                    {
                        switch (launchSite.sitecategory)
                        {
                            case LaunchSiteCategory.RocketPad:
                                GUI.DrawTexture(screenRect, UIMain.VABIcon, ScaleMode.ScaleToFit, true);
                                break;
                            case LaunchSiteCategory.Runway:
                                GUI.DrawTexture(screenRect, UIMain.runWayIcon, ScaleMode.ScaleToFit, true);
                                break;
                            case LaunchSiteCategory.Helipad:
                                GUI.DrawTexture(screenRect, UIMain.heliPadIcon, ScaleMode.ScaleToFit, true);
                                break;
                            case LaunchSiteCategory.Waterlaunch:
                                GUI.DrawTexture(screenRect, UIMain.waterLaunchIcon, ScaleMode.ScaleToFit, true);
                                break;
                            default:
                                GUI.DrawTexture(screenRect, UIMain.ANYIcon, ScaleMode.ScaleToFit, true);
                                break;
                        }
                    }
                }

                // Tooltip
                if (!displayingTooltip && screenRect.Contains(Event.current.mousePosition) && fRadarRadius > 15)
                {
                    //Only display one tooltip at a time
                    string sToolTip = "";
                    sToolTip = launchSite.LaunchSiteName;
                    if (launchSite.LaunchSiteName == "Runway") sToolTip = "KSC Runway";
                    if (launchSite.LaunchSiteName == "LaunchPad") sToolTip = "KSC LaunchPad";
                    DisplayMapIconToolTip(sToolTip, lsPosition);

                    // Select a base by clicking on the icon
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        MapIconSelector.Close();
                        MapIconSelector.selectedSite = launchSite;
                        MapIconSelector.useLaunchSite = true;
                        MapIconSelector.Open();
                        NavGuidanceSystem.setTargetSite(launchSite);
                    }
                }
            }
        }


        private void DrawSpaceCenters()
        {
            if (!KerbalKonstructs.instance.mapShowRecovery)
                return;

            body = PlanetariumCamera.fetch.target.GetReferenceBody();

            displayingTooltip2 = false;

            // Do tracking stations first
            foreach (CustomSpaceCenter customSpaceCenter in spaceCenters)
            {
                if ((mapHideIconsBehindBody) && (IsOccluded(customSpaceCenter.gameObject.transform.position, body)))
                {
                    continue;
                }

                if (customSpaceCenter.staticInstance.CelestialBody != body) continue;

                cscIsOpen = customSpaceCenter.isOpen;


                if ((!cscIsOpen && !KerbalKonstructs.instance.mapShowClosed) || (cscIsOpen && !KerbalKonstructs.instance.mapShowOpen))
                {
                    continue;
                }

                if ((customSpaceCenter.staticInstance.groupCenter != null) && (customSpaceCenter.staticInstance.groupCenter.isHidden && !cscIsOpen))
                {
                    continue;
                }


				Vector3d worldPos = body.GetWorldSurfacePosition(customSpaceCenter.staticInstance.RefLatitude, customSpaceCenter.staticInstance.RefLongitude, customSpaceCenter.staticInstance.RadiusOffset);
				Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(worldPos));

                Rect screenRect6 = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;
                float fRadarRadius = (12800 / fPosZ) * ((float)body.Radius / KerbinRadius);

                if (fRadarRadius > 15)
                {
                    GUI.DrawTexture(screenRect6, UIMain.iconRecoveryBase, ScaleMode.ScaleToFit, true);
                }

                if (screenRect6.Contains(Event.current.mousePosition) && !displayingTooltip2)
                {


                    var objectpos2 = customSpaceCenter.staticInstance.CelestialBody.transform.InverseTransformPoint(customSpaceCenter.gameObject.transform.position);

                    var disObjectLat2 = KKMath.GetLatitudeInDeg(objectpos2);
                    var disObjectLon2 = KKMath.GetLongitudeInDeg(objectpos2);

                    if (disObjectLon2 < 0)
                        disObjectLon2 = disObjectLon2 + 360;

                    //Only display one tooltip at a time
                    if (customSpaceCenter.isFromFacility)
                    {
                        DisplayMapIconToolTip(customSpaceCenter.staticInstance.GetFacility(KKFacilityType.RecoveryBase).FacilityName + "\n(Lat." + disObjectLat2.ToString("#0.00") + "/ Lon." + disObjectLon2.ToString("#0.00") + ")", pos);
                    }
                    else
                    {
                        DisplayMapIconToolTip(customSpaceCenter.staticInstance.launchSite.LaunchSiteName + "\n(Lat." + disObjectLat2.ToString("#0.00") + "/ Lon." + disObjectLon2.ToString("#0.00") + ")", pos);
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (customSpaceCenter.isFromFacility)
                        {
                            MapIconSelector.Close();
                            MapIconSelector.useLaunchSite = false;
                            MapIconSelector.staticInstance = customSpaceCenter.staticInstance;
                            MapIconSelector.Open();

                        }
                        else
                        {
                            MapIconSelector.Close();
                            MapIconSelector.selectedSite = customSpaceCenter.staticInstance.launchSite;
                            MapIconSelector.useLaunchSite = true;
                            MapIconSelector.Open();

                        }
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


        public void DisplayMapIconToolTip(string sitename, Vector3 pos)
        {
            displayingTooltip = true;
            GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 210, 25), sitename);
        }



    }

}

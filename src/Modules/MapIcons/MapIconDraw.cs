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
    class MapIconDraw :KKWindow
    {

        private Boolean displayingTooltip = false;
        private Boolean displayingTooltip2 = false;

        static LaunchSite selectedSite = null;
        static StaticObject selectedFacility = null;

        internal static bool mapHideIconsBehindBody = true;

        private int iRadarCounter;

        public override void Draw()
        {
            if ((!KerbalKonstructs.instance.toggleIconsWithBB) || (KerbalKonstructs.instance.toggleIconsWithBB && this.IsOpen()))
            {
                drawTrackingStations();
                drawLaunchsites();
            }
        }

        public void drawGroundComms(StaticObject obj, LaunchSite lSite = null)
        {
            string Base = "";
            string Base2 = "";
            float Range = 0f;
            LaunchSite lBase = null;
            LaunchSite lBase2 = null;
            Vector3 pos = Vector3.zero;

            if (lSite != null)
            {
                GameObject golSite = lSite.GameObject;
                pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(golSite.transform.position));
                LaunchSiteManager.getNearestBase(golSite.transform.position, out Base, out Base2, out Range, out lBase, out lBase2);
            }

            if (obj != null)
            {
                pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(obj.gameObject.transform.position));
                LaunchSiteManager.getNearestBase(obj.gameObject.transform.position, out Base, out Base2, out Range, out lBase, out lBase2);
            }

            Vector3 vNeighbourPos = Vector3.zero;
            Vector3 vNeighbourPos2 = Vector3.zero;
            Vector3 vBasePos = Vector3.zero;
            Vector3 vBasePos2 = Vector3.zero;

            GameObject goNeighbour = null;
            GameObject goNeighbour2 = null;

            if (Base != "")
            {
                if (Base == "KSC")
                {
                    goNeighbour = SpaceCenterManager.KSC.gameObject;
                }
                else
                    goNeighbour = LaunchSiteManager.getSiteGameObject(Base);
            }

            if (Base2 != "")
            {

                if (Base2 == "KSC")
                {
                    goNeighbour2 = SpaceCenterManager.KSC.gameObject;
                }
                else
                    goNeighbour2 = LaunchSiteManager.getSiteGameObject(Base2);
            }

            if (goNeighbour != null)
            {
                vNeighbourPos = goNeighbour.transform.position;
                vBasePos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(vNeighbourPos));
            }

            if (goNeighbour2 != null)
            {
                vNeighbourPos2 = goNeighbour2.transform.position;
                vBasePos2 = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(vNeighbourPos2));
            }

            if (goNeighbour != null && vNeighbourPos != Vector3.zero && vBasePos != Vector3.zero)
            {
                NavUtils.CreateLineMaterial(1);

                GL.Begin(GL.LINES);
                NavUtils.lineMaterial1.SetPass(0);
                GL.Color(new Color(1f, 1f, 1f, 0.7f));
                GL.Vertex3(pos.x - Screen.width / 2, pos.y - Screen.height / 2, pos.z);
                GL.Vertex3(vBasePos.x - Screen.width / 2, vBasePos.y - Screen.height / 2, vBasePos.z);
                GL.End();
            }

            if (goNeighbour2 != null && vNeighbourPos2 != Vector3.zero && vBasePos2 != Vector3.zero)
            {
                NavUtils.CreateLineMaterial(2);

                GL.Begin(GL.LINES);
                NavUtils.lineMaterial2.SetPass(0);
                GL.Color(new Color(1f, 1f, 1f, 0.7f));
                GL.Vertex3(pos.x - Screen.width / 2, pos.y - Screen.height / 2, pos.z);
                GL.Vertex3(vBasePos2.x - Screen.width / 2, vBasePos2.y - Screen.height / 2, vBasePos2.z);
                GL.End();
            }
        }

        public void drawTrackingStations()
        {
            if (!KerbalKonstructs.instance.mapShowOpenT) { return;  }
            displayingTooltip2 = false;
            CelestialBody body = PlanetariumCamera.fetch.target.GetReferenceBody();

            // Do tracking stations first
            foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
            {
                if (!MiscUtils.isCareerGame()) break;

                bool display2 = false;
                string openclosed3 = "Closed";

                if ((string)obj.getSetting("FacilityType") != "TrackingStation")
                    continue;

                if ((float)obj.getSetting("TrackingShort") == 0f)
                    continue;

                if ((string)obj.getSetting("Group") == "KSCUpgrades")
                    continue;

                if ((mapHideIconsBehindBody) && (isOccluded(obj.gameObject.transform.position, body)))
                {
                        continue;
                }

                openclosed3 = (string)obj.getSetting("OpenCloseState");

                if ((float)obj.getSetting("OpenCost") == 0) openclosed3 = "Open";

                if (KerbalKonstructs.instance.mapShowOpenT)
                    display2 = true;
                if (!KerbalKonstructs.instance.mapShowClosed && openclosed3 == "Closed")
                    display2 = false;
                if (!KerbalKonstructs.instance.mapShowOpen && openclosed3 == "Open")
                    display2 = false;

                if (!display2)
                    continue;

                Vector3  pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(obj.gameObject.transform.position));
  
                Rect screenRect6 = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;
                float fRadarRadius = 12800 / fPosZ;

                if (fRadarRadius > 15) GUI.DrawTexture(screenRect6, UIMain.TrackingStationIcon, ScaleMode.ScaleToFit, true);


                if (openclosed3 == "Open" && KerbalKonstructs.instance.mapShowGroundComms)
                    drawGroundComms(obj);


                if (screenRect6.Contains(Event.current.mousePosition) && !displayingTooltip2)
                {
                    CelestialBody cPlanetoid = (CelestialBody)obj.getSetting("CelestialBody");

                    var objectpos2 = cPlanetoid.transform.InverseTransformPoint(obj.gameObject.transform.position);
                    var dObjectLat2 = NavUtils.GetLatitude(objectpos2);
                    var dObjectLon2 = NavUtils.GetLongitude(objectpos2);
                    var disObjectLat2 = dObjectLat2 * 180 / Math.PI;
                    var disObjectLon2 = dObjectLon2 * 180 / Math.PI;

                    if (disObjectLon2 < 0) disObjectLon2 = disObjectLon2 + 360;

                    //Only display one tooltip at a time
                    displayMapIconToolTip("Tracking Station " + "\n(Lat." + disObjectLat2.ToString("#0.00") + "/ Lon." + disObjectLon2.ToString("#0.00") + ")", pos);

                    if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
                    {
                        float sTrackRange = (float)obj.getSetting("TrackingShort");
                        float sTrackRange2 = (float)obj.getSetting("TrackingShort");

                        selectedFacility = obj;
                        FacilityManager.selectedFacility = obj;
                        KerbalKonstructs.GUI_FacilityManager.Open();
                    }
                }  
            } 
        }

        public void drawRadar(Vector3 pos, string category, string openclosed)
        {
            if (openclosed != "Open") return;

            float fPosZ = pos.z;
            float fRadarRadius = 12800 / fPosZ;
            float fRadarOffset = fRadarRadius / 2;

            int iPulseRate = 180;

            Rect screenRect2 = new Rect((pos.x - fRadarOffset), (Screen.height - pos.y) - fRadarOffset, fRadarRadius, fRadarRadius);
            Rect screenRect3 = new Rect((pos.x - (fRadarOffset / 2)), (Screen.height - pos.y) - (fRadarOffset / 2), fRadarRadius / 2, fRadarRadius / 2);
            Rect screenRect4 = new Rect((pos.x - (fRadarOffset / 3)), (Screen.height - pos.y) - (fRadarOffset / 3), fRadarRadius / 3, fRadarRadius / 3);
            Rect screenRect5 = new Rect((pos.x - (fRadarOffset / 4)), (Screen.height - pos.y) - (fRadarOffset / 4), fRadarRadius / 4, fRadarRadius / 4);

            if (fRadarRadius > 15)
            {
                if (category == "Runway")
                {
                    if (iRadarCounter > iPulseRate / 2)
                        GUI.DrawTexture(screenRect2, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                    if (iRadarCounter > iPulseRate / 3)
                        GUI.DrawTexture(screenRect3, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                    if (iRadarCounter > iPulseRate / 4)
                        GUI.DrawTexture(screenRect4, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                    if (iRadarCounter > iPulseRate / 5)
                        GUI.DrawTexture(screenRect5, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                }

                if (category == "Helipad")
                {
                    if (iRadarCounter > iPulseRate / 2)
                        GUI.DrawTexture(screenRect3, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                    if (iRadarCounter > iPulseRate / 3)
                        GUI.DrawTexture(screenRect4, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                    if (iRadarCounter > iPulseRate / 4)
                        GUI.DrawTexture(screenRect5, UIMain.tRadarCover, ScaleMode.ScaleToFit, true);
                }
            }
        }

        public void drawLaunchsites()
        {
            displayingTooltip = false;
            int iPulseRate = 180;
            LaunchSite launchSite;
            CelestialBody body = PlanetariumCamera.fetch.target.GetReferenceBody();

            iRadarCounter = iRadarCounter + 1;
            if (iRadarCounter > iPulseRate)
                iRadarCounter = 0;

            // Then do launchsites
            List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
            for (int index = 0; index < sites.Count; index++)
            {
                launchSite = sites[index];
                // check if we should display the site or not this is the fastst check, so it shoud be first
                string openclosed = launchSite.openclosestate;
                string category = launchSite.category;

                if (!KerbalKonstructs.instance.mapShowHelipads && category == "Helipad")
                    continue;
                if (!KerbalKonstructs.instance.mapShowOther && (( category == "Other" ) || (category == "Waterlaunch") ) )
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
                }

                Vector3 launchSitePosition = (Vector3)launchSite.body.GetWorldSurfacePosition(launchSite.reflat, launchSite.reflon,launchSite.refalt) - MapView.MapCamera.GetComponent<Camera>().transform.position;

                if (mapHideIconsBehindBody && isOccluded(launchSitePosition, body))
                {
                        continue;
                }
                
               Vector3 pos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(launchSitePosition));

                Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

                // Distance between camera and spawnpoint sort of
                float fPosZ = pos.z;

                float fRadarRadius = 12800 / fPosZ;
                float fRadarOffset = fRadarRadius / 2;

                if (KerbalKonstructs.instance.mapShowRadar)
                    drawRadar(pos, category, openclosed);

                if (openclosed == "Open" && KerbalKonstructs.instance.mapShowGroundComms)
                {
                    drawGroundComms(null, launchSite);
                }

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
                    sToolTip = launchSite.name;
                    if (launchSite.name == "Runway") sToolTip = "KSC Runway";
                    if (launchSite.name == "LaunchPad") sToolTip = "KSC LaunchPad";
                    displayMapIconToolTip(sToolTip, pos);

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


        private bool isOccluded(Vector3d loc, CelestialBody body)
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

        public void displayMapIconToolTip(string sitename, Vector3 pos)
        {
            displayingTooltip = true;
            GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 210, 25), sitename);
        }



    }

}

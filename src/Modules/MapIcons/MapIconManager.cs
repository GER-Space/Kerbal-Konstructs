using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.Modules
{
    class MapIconManager : KKWindow
    {
        Rect mapManagerRect = new Rect(250, 40, 515, 75);

        public float iFundsOpen = 0;
        public float iFundsClose = 0;
        public Boolean isOpen = false;

        public float iFundsOpen2 = 0;
        public float iFundsClose2 = 0;
        public Boolean isOpen2 = false;
        public Boolean bChangeTargetType = false;
        public Boolean bChangeTarget = false;
        public Boolean showBaseManager = false;
        public Boolean bHideOccluded = false;
        public Boolean bHideOccluded2 = false;


        Vector3 ObjectPos = new Vector3(0, 0, 0);



        public MapIconManager()
        {
        }

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                drawManager();
            }
            else
            {
                this.Close();
            }
        }

        public void drawManager()
        {
            mapManagerRect = GUI.Window(0xB00B2E7, mapManagerRect, drawMapManagerWindow, "", UIMain.navStyle);
        }

        void drawMapManagerWindow(int windowID)
        {

            GUILayout.BeginHorizontal();
            GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

            GUI.enabled = (MiscUtils.isCareerGame());
            if (!MiscUtils.isCareerGame())
            {
                GUILayout.Button(UIMain.tOpenBasesOff, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Button(UIMain.tClosedBasesOff, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
                GUILayout.Button(UIMain.tTrackingOff, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                if (KerbalKonstructs.instance.mapShowOpen)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tOpenBasesOn, "Opened"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowOpen = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tOpenBasesOff, "Opened"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowOpen = true;
                }

                if (!KerbalKonstructs.instance.disableDisplayClosed)
                {
                    if (KerbalKonstructs.instance.mapShowClosed)
                    {
                        if (GUILayout.Button(new GUIContent(UIMain.tClosedBasesOn, "Closed"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                            KerbalKonstructs.instance.mapShowClosed = false;
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent(UIMain.tClosedBasesOff, "Closed"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                            KerbalKonstructs.instance.mapShowClosed = true;
                    }
                }

                GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
                if (KerbalKonstructs.instance.mapShowOpenT)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tTrackingOn, "Tracking Stations"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowOpenT = false;
                }
                else
                {

                    if (GUILayout.Button(new GUIContent(UIMain.tTrackingOff, "Tracking Stations"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowOpenT = true;
                }
            }
            GUI.enabled = true;

            GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

            if (KerbalKonstructs.instance.mapShowRocketbases)
            {
                if (GUILayout.Button(new GUIContent(UIMain.tLaunchpadsOn, "Rocketpads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowRocketbases = false;
            }
            else
            {
                if (GUILayout.Button(new GUIContent(UIMain.tLaunchpadsOff, "Rocketpads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowRocketbases = true;
            }

            if (KerbalKonstructs.instance.mapShowHelipads)
            {
                if (GUILayout.Button(new GUIContent(UIMain.tHelipadsOn, "Helipads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowHelipads = false;
            }
            else
            {
                if (GUILayout.Button(new GUIContent(UIMain.tHelipadsOff, "Helipads"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowHelipads = true;
            }

            if (KerbalKonstructs.instance.mapShowRunways)
            {
                if (GUILayout.Button(new GUIContent(UIMain.tRunwaysOn, "Runways"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowRunways = false;
            }
            else
            {
                if (GUILayout.Button(new GUIContent(UIMain.tRunwaysOff, "Runways"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowRunways = true;
            }

            if (KerbalKonstructs.instance.mapShowOther)
            {
                if (GUILayout.Button(new GUIContent(UIMain.tOtherOn, "Other"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowOther = false;
            }
            else
            {
                if (GUILayout.Button(new GUIContent(UIMain.tOtherOff, "Other"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    KerbalKonstructs.instance.mapShowOther = true;
            }

            GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

            GUI.enabled = (MiscUtils.isCareerGame());
            if (!MiscUtils.isCareerGame())
            {
                GUILayout.Button(UIMain.tDownlinksOff, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Button(UIMain.tUplinksOff, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Button(UIMain.tRadarOff, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Button(UIMain.tGroundCommsOff, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                if (KerbalKonstructs.instance.mapShowDownlinks)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tDownlinksOn, "Downlinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowDownlinks = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tDownlinksOff, "Downlinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowDownlinks = true;
                }

                if (KerbalKonstructs.instance.mapShowUplinks)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tUplinksOn, "Uplinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowUplinks = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tUplinksOff, "Uplinks"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowUplinks = true;
                }

                if (KerbalKonstructs.instance.mapShowRadar)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tRadarOn, "Radar"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowRadar = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tRadarOff, "Radar"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowRadar = true;
                }

                if (KerbalKonstructs.instance.mapShowGroundComms)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tGroundCommsOn, "Ground Comms"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowGroundComms = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tGroundCommsOff, "Ground Comms"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowGroundComms = true;
                }

            }
            GUI.enabled = true;

            GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

            if (MapIconDraw.mapHideIconsBehindBody)
            {
                if (GUILayout.Button(new GUIContent(UIMain.tHideOn, "Occlude"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    MapIconDraw.mapHideIconsBehindBody = false;
            }
            else
            {
                if (GUILayout.Button(new GUIContent(UIMain.tHideOff, "Occlude"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                    MapIconDraw.mapHideIconsBehindBody = true;
            }

            GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

            if (GUILayout.Button("X", UIMain.ButtonRed, GUILayout.Height(20), GUILayout.Width(20)))
            {
                this.Close();
            }

            GUILayout.EndHorizontal();

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 5, labelSize.y + 6), GUI.tooltip, UIMain.KKToolTip);
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

    }
}
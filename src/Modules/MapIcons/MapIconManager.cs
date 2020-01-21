using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;

namespace KerbalKonstructs.Modules
{
    class MapIconManager : KKWindow
    {
        private static MapIconManager _instance = null;
        internal static MapIconManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapIconManager();

                }
                return _instance;
            }
        }


        Rect mapManagerRect = new Rect(250, 40, 455, 75);

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


        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                DrawManager();
            }
            else
            {
                this.Close();
            }
        }

        public void DrawManager()
        {
            mapManagerRect = GUI.Window(0xB00B2E7, mapManagerRect, DrawMapManagerWindow, "", UIMain.navStyle);
        }

        void DrawMapManagerWindow(int windowID)
        {

            GUILayout.BeginHorizontal();
            {
                GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));

                GUI.enabled = (MiscUtils.isCareerGame());
                if (!MiscUtils.isCareerGame())
                {
                    GUILayout.Button(UIMain.tOpenBasesOff, GUILayout.Width(32), GUILayout.Height(32));
                    GUILayout.Button(UIMain.tClosedBasesOff, GUILayout.Width(32), GUILayout.Height(32));
                    GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
                    //GUILayout.Button(UIMain.tTrackingOff, GUILayout.Width(32), GUILayout.Height(32));
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
                GUI.enabled = true;

                GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));
                if (KerbalKonstructs.instance.mapShowGroundStation)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tTrackingOn, "Tracking Stations"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowGroundStation = false;
                }
                else
                {

                    if (GUILayout.Button(new GUIContent(UIMain.tTrackingOff, "Tracking Stations"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowGroundStation = true;
                }


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

                if (KerbalKonstructs.instance.mapShowWaterLaunch)
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tWaterOn, "WaterLaunch"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowWaterLaunch = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent(UIMain.tWaterOff, "WaterLaunch"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowWaterLaunch = true;
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

                if (KerbalKonstructs.instance.mapShowRecovery)
                {
                    if (GUILayout.Button(new GUIContent("$", "Recovery Bases"), UIMain.ButtonKK, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowRecovery = false;
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("$", "Recovery Bases"), UIMain.ButtonInactive, GUILayout.Width(32), GUILayout.Height(32)))
                        KerbalKonstructs.instance.mapShowRecovery = true;
                }



                GUILayout.Box(" ", UIMain.BoxNoBorder, GUILayout.Height(34));


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
            }
            GUILayout.EndHorizontal();

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + 20, labelSize.x + 5, labelSize.y + 6), GUI.tooltip, UIMain.KKToolTip);
            }

            GUI.DragWindow(new Rect(0, 0, 25000, 25000));
        }

    }
}

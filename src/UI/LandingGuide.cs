using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.UI
{
    class LandingGuideUI :KKWindow
    {
        private static LandingGuideUI _instance = null;
        internal static LandingGuideUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LandingGuideUI();

                }
                return _instance;
            }
        }

        private Vector3 screenListStart = Vector3.zero;
        private Vector3 screenLineEnd = Vector3.zero;
        private Vector3 vTDL = Vector3.zero;
        private Vector3 vTDR = Vector3.zero;
        private StaticInstance soLandingGuide = null;
        private StaticInstance soTDR = null;
        private StaticInstance soTDL = null;

        private Texture tLGt = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lgtop", false);
        private Texture tLGm = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lgmiddle", false);
        private Texture tLGb = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lgbottom", false);

        private Texture tTGL = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/touchdownL", false);
        private Texture tTGR = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/touchdownR", false);

 

        private Rect Marker1;
        private Rect Marker2;
        private Rect Marker3;
        private Rect Marker4;
        private Rect Marker5;
        private Rect Marker6;
        private Rect Marker7;
        private Rect Marker8;



        private Vector3 vlgPos;
        private Vector3 vcrPos;
        private float distance;
        private Vector3 fromShipToEnd;
        private Vector3 horizontalVector;
        private double glideAngle;

        // Warning colors
        private static Color clRight = new Color(0.2f, 0.7f, 0.2f, 0.7f);
        private static Color clWarn = new Color(0.2f, 0.7f, 0.7f, 0.7f);
        private static Color clTooHigh = new Color(0.7f, 0.2f, 0.2f, 0.7f);


        public override void Draw()
        {
            if (!MapView.MapIsEnabled)
            {
                drawLandingGuides();
            }
        }


        public void drawTouchDownGuideL(StaticInstance obj)
        {
            if (obj == null)
            {
                vTDL = Vector3.zero;
                soTDL = null;
                return;
            }
            soTDL = obj;
        }

        public void drawTouchDownGuideR(StaticInstance obj)
        {
            if (obj == null)
            {
                vTDR = Vector3.zero;
                soTDR = null;
                return;
            }
            soTDR = obj;            
        }

        public void drawLandingGuide(StaticInstance instance)
        {
            if (instance == null)
            {
                screenListStart = Vector3.zero;
                screenLineEnd = Vector3.zero;
                soLandingGuide = null;
                return;
            }
            soLandingGuide = instance;
        }


        void drawLandingGuides()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                return;
            }
            if (soLandingGuide == null || FlightGlobals.ActiveVessel == null || soTDL == null || soTDR == null)
            {
                return;
            }

            // the vessel is not active?!? we don't deal with such alien spacecraft. 
            if (FlightGlobals.ActiveVessel.state != Vessel.State.ACTIVE)
            {
                return;
            }

            // Only deal with flying things.
            if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.FLYING)
            {
                return;
            }


            vTDL = Camera.main.WorldToScreenPoint(soTDL.gameObject.transform.position);
            vTDR = Camera.main.WorldToScreenPoint(soTDR.gameObject.transform.position);

            vlgPos = soLandingGuide.gameObject.transform.position;
            vcrPos = FlightGlobals.ActiveVessel.GetWorldPos3D();
            distance = Vector3.Distance(vlgPos, vcrPos);


            if (distance > 15000) return;
            if (distance < 3) return;

            float flgWscale = 1f;
            float flgHscale = 1f;

            if (distance > 10000)
            {
                flgWscale = 250 / distance;
                flgHscale = 100 / distance;
            }
            else if (distance > 5000)
            {
                flgWscale = 500 / distance;
                flgHscale = 250 / distance;
            }
            else if (distance > 2500)
            {
                flgWscale = 750 / distance;
                flgHscale = 500 / distance;
            }
            else if (distance > 1000)
            {
                flgWscale = 1000 / distance;
                flgHscale = 750 / distance;
            }
            else
            {
                flgWscale = 1f;
                flgHscale = 1f;
            }

            

            Vector3 landingGuidePoint = soLandingGuide.gameObject.transform.position;
            Vector3d vesselPosition = FlightGlobals.ActiveVessel.GetWorldPos3D();


            screenListStart = Camera.main.WorldToScreenPoint(landingGuidePoint);
            screenLineEnd = Camera.main.WorldToScreenPoint(vesselPosition);


            fromShipToEnd = landingGuidePoint - vesselPosition;


            if (screenLineEnd != Vector3.zero && screenListStart != Vector3.zero)
            {

                // Draw the landing line depending on the angle
                if (distance < 8000)
                {
                    horizontalVector = Vector3.ProjectOnPlane(fromShipToEnd, soLandingGuide.gameObject.transform.up);
                    glideAngle = Mathf.Rad2Deg * Math.Acos(horizontalVector.magnitude / fromShipToEnd.magnitude);

                    if (glideAngle > 6f)
                    {
                        DebugDrawer.DebugLine(landingGuidePoint, vesselPosition, clTooHigh);
                    }
                    else
                    if (glideAngle > 4.5f)
                    {
                        DebugDrawer.DebugLine(landingGuidePoint, vesselPosition, clWarn);
                    }
                    else
                    if (glideAngle < 1.5f)
                    {
                        DebugDrawer.DebugLine(landingGuidePoint, vesselPosition, clWarn);
                    }
                    else
                    {
                        DebugDrawer.DebugLine(landingGuidePoint, vesselPosition, clRight);
                    }
                }


                // Check if we are past the landing guide
                if (Vector3d.Dot(soLandingGuide.gameObject.transform.forward, fromShipToEnd) < 0)
                {
                    return;
                }


                Marker1 = new Rect((float)(screenListStart.x) - (25 * flgWscale), (float)(Screen.height - screenListStart.y) - 10, 50 * flgWscale, 4);
                Marker2 = new Rect((float)(screenListStart.x) - (50 * flgWscale), (float)(Screen.height - screenListStart.y) - 25, 100 * flgWscale, 4);
                Marker3 = new Rect((float)(screenListStart.x) - (75 * flgWscale), (float)(Screen.height - screenListStart.y) - 40, 150 * flgWscale, 4);

                Marker4 = new Rect((float)(Screen.width / 2) - (25 * flgWscale), (float)(Screen.height / 2) - 10, 50 * flgWscale, 4);
                Marker5 = new Rect((float)(Screen.width / 2) - (50 * flgWscale), (float)(Screen.height / 2) - 25, 100 * flgWscale, 4);
                Marker6 = new Rect((float)(Screen.width / 2) - (75 * flgWscale), (float)(Screen.height / 2) - 40, 150 * flgWscale, 4);

                if (distance < 15000)
                {
                    // Only draw this if we show in the right direction
                    if (Vector3.Dot((vesselPosition - FlightCamera.fetch.mainCamera.transform.position), landingGuidePoint) > 0)
                    {
                        GUI.DrawTexture(Marker1, tLGb, ScaleMode.StretchToFill, true);
                        GUI.DrawTexture(Marker2, tLGm, ScaleMode.StretchToFill, true);
                        GUI.DrawTexture(Marker3, tLGt, ScaleMode.StretchToFill, true);

                        if (vTDL != Vector3.zero && vTDR != Vector3.zero)
                        {
                            vTDR = Camera.main.WorldToScreenPoint(soTDR.gameObject.transform.position);
                            vTDL = Camera.main.WorldToScreenPoint(soTDL.gameObject.transform.position);

                            Marker7 = new Rect((float)(vTDL.x) - (50 * flgWscale), (float)(Screen.height - vTDL.y) - (140 * flgHscale), 100 * flgWscale, 150 * flgHscale);
                            Marker8 = new Rect((float)(vTDR.x) - (50 * flgWscale), (float)(Screen.height - vTDR.y) - (140 * flgHscale), 100 * flgWscale, 150 * flgHscale);

                            GUI.DrawTexture(Marker7, tTGL, ScaleMode.StretchToFill, true);
                            GUI.DrawTexture(Marker8, tTGR, ScaleMode.StretchToFill, true);
                        }

                    }
                    // make them less annoying
                    if (KerbalKonstructs.instance.selectedObject == null)
                    {
                        GUI.DrawTexture(Marker4, tLGb, ScaleMode.StretchToFill, true);
                        GUI.DrawTexture(Marker5, tLGm, ScaleMode.StretchToFill, true);
                        GUI.DrawTexture(Marker6, tLGt, ScaleMode.StretchToFill, true);
                    }
                }
            }
        }
    }
}

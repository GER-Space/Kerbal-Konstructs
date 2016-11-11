using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using KerbalKonstructs;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.UI
{
    class LandingGuide :KKWindow
    {
        private Vector3 vLineStart = Vector3.zero;
        private Vector3 vLineEnd = Vector3.zero;
        private Vector3 vTDL = Vector3.zero;
        private Vector3 vTDR = Vector3.zero;
        private StaticObject soLandingGuide = null;
        private StaticObject soTDR = null;
        private StaticObject soTDL = null;

        private Texture tLGt = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lgtop", false);
        private Texture tLGm = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lgmiddle", false);
        private Texture tLGb = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lgbottom", false);

        private Texture tTGL = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/touchdownL", false);
        private Texture tTGR = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/touchdownR", false);

        public override void Draw()
        {
            if (!MapView.MapIsEnabled)
            {
                drawLandingGuides();
            }
        }


        public void drawTouchDownGuideL(StaticObject obj)
        {
            if (obj == null)
            {
                vTDL = Vector3.zero;
                return;
            }

            if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
            Vessel vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;

            vTDL = Camera.main.WorldToScreenPoint(obj.gameObject.transform.position);
            soTDL = obj;
        }

        public void drawTouchDownGuideR(StaticObject obj)
        {
            if (obj == null)
            {
                vTDR = Vector3.zero;
                return;
            }

            if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
            Vessel vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;

            vTDR = Camera.main.WorldToScreenPoint(obj.gameObject.transform.position);
            soTDR = obj;
        }

        public void drawLandingGuide(StaticObject obj)
        {
            if (obj == null)
            {
                vLineStart = Vector3.zero;
                vLineEnd = Vector3.zero;
                return;
            }

            if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;

            Vessel vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;

            Debug.Log("KK: drawLandingGuide");

            vLineStart = Camera.main.WorldToScreenPoint(obj.gameObject.transform.position);
            vLineEnd = Camera.main.WorldToScreenPoint(vesCraft.transform.position);
            soLandingGuide = obj;
        }


        void drawLandingGuides()
        {
            Vessel vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;
            if (soLandingGuide == null) return;

            Vector3 vlgPos = soLandingGuide.gameObject.transform.position;
            Vector3 vcrPos = vesCraft.transform.position;
            float fDist = Vector3.Distance(vlgPos, vcrPos);

            if (fDist > 15000) return;
            if (fDist < 3) return;

            float flgWscale = 1f;

            if (fDist > 10000) flgWscale = 250 / fDist;
            else
                if (fDist > 5000) flgWscale = 500 / fDist;
            else
                    if (fDist > 2500) flgWscale = 750 / fDist;
            else
                        if (fDist > 1000) flgWscale = 1000 / fDist;
            else
                flgWscale = 1f;

            float flgHscale = 1f;

            if (fDist > 10000) flgHscale = 100 / fDist;
            else
                if (fDist > 5000) flgHscale = 250 / fDist;
            else
                    if (fDist > 2500) flgHscale = 500 / fDist;
            else
                        if (fDist > 1000) flgHscale = 750 / fDist;
            else
                flgHscale = 1f;

            if (vTDL != Vector3.zero && vTDR != Vector3.zero)
            {
                vTDR = Camera.main.WorldToScreenPoint(soTDR.gameObject.transform.position);
                vTDL = Camera.main.WorldToScreenPoint(soTDL.gameObject.transform.position);

                Rect Marker7 = new Rect((float)(vTDL.x) - (50 * flgWscale), (float)(Screen.height - vTDL.y) - (140 * flgHscale), 100 * flgWscale, 150 * flgHscale);
                Rect Marker8 = new Rect((float)(vTDR.x) - (50 * flgWscale), (float)(Screen.height - vTDR.y) - (140 * flgHscale), 100 * flgWscale, 150 * flgHscale);

                GUI.DrawTexture(Marker7, tTGL, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(Marker8, tTGR, ScaleMode.StretchToFill, true);
            }

            if (vLineEnd != Vector3.zero && vLineStart != Vector3.zero)
            {
                vLineEnd = Camera.main.WorldToScreenPoint(vesCraft.transform.position);
                vLineStart = Camera.main.WorldToScreenPoint(soLandingGuide.gameObject.transform.position);

                Color clGood = new Color(0f, 1f, 0f, 0.8f);
                Color clBad = new Color(1f, 0f, 0f, 0.8f);
                Color clLandGuide = new Color(1f, 1f, 1f, 0.8f);

                if (fDist < 8000)
                {
                    NavUtils.CreateLineMaterial(1);

                    GL.Begin(GL.LINES);
                    NavUtils.lineMaterial1.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3(vLineEnd.x - Screen.width / 2, vLineEnd.y - Screen.height / 2, vLineEnd.z);
                    GL.Vertex3(vLineStart.x - Screen.width / 2, vLineStart.y - Screen.height / 2, vLineStart.z);
                    GL.End();

                    NavUtils.CreateLineMaterial(2);

                    GL.Begin(GL.LINES);
                    NavUtils.lineMaterial2.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3(vLineStart.x - Screen.width / 2, (vLineStart.y - Screen.height / 2) + 200, vLineStart.z);
                    GL.Vertex3(vLineStart.x - Screen.width / 2, (vLineStart.y - Screen.height / 2) - 150, vLineStart.z);
                    GL.End();

                    NavUtils.CreateLineMaterial(3);

                    GL.Begin(GL.LINES);
                    NavUtils.lineMaterial3.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) + 100, (vLineStart.y - Screen.height / 2) + 5, vLineStart.z);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) - 100, (vLineStart.y - Screen.height / 2) + 5, vLineStart.z);
                    GL.End();

                    NavUtils.CreateLineMaterial(4);

                    GL.Begin(GL.LINES);
                    NavUtils.lineMaterial4.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) + 150, (vLineStart.y - Screen.height / 2) + 50, vLineStart.z);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) - 150, (vLineStart.y - Screen.height / 2) + 50, vLineStart.z);
                    GL.End();
                }

                Rect Marker1 = new Rect((float)(vLineStart.x) - (25 * flgWscale), (float)(Screen.height - vLineStart.y) - 10, 50 * flgWscale, 4);
                Rect Marker2 = new Rect((float)(vLineStart.x) - (50 * flgWscale), (float)(Screen.height - vLineStart.y) - 25, 100 * flgWscale, 4);
                Rect Marker3 = new Rect((float)(vLineStart.x) - (75 * flgWscale), (float)(Screen.height - vLineStart.y) - 40, 150 * flgWscale, 4);

                Rect Marker4 = new Rect((float)(Screen.width / 2) - (25 * flgWscale), (float)(Screen.height / 2) - 10, 50 * flgWscale, 4);
                Rect Marker5 = new Rect((float)(Screen.width / 2) - (50 * flgWscale), (float)(Screen.height / 2) - 25, 100 * flgWscale, 4);
                Rect Marker6 = new Rect((float)(Screen.width / 2) - (75 * flgWscale), (float)(Screen.height / 2) - 40, 150 * flgWscale, 4);

                if (fDist < 15000)
                {
                    GUI.DrawTexture(Marker1, tLGb, ScaleMode.StretchToFill, true);
                    GUI.DrawTexture(Marker2, tLGm, ScaleMode.StretchToFill, true);
                    GUI.DrawTexture(Marker3, tLGt, ScaleMode.StretchToFill, true);
                }

                GUI.DrawTexture(Marker4, tLGb, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(Marker5, tLGm, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(Marker6, tLGt, ScaleMode.StretchToFill, true);
            }
        }


    }
}

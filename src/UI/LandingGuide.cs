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

        private Vector3 vLineStart = Vector3.zero;
        private Vector3 vLineEnd = Vector3.zero;
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

        private Vessel vesCraft = null;

        private Rect Marker1;
        private Rect Marker2;
        private Rect Marker3;
        private Rect Marker4;
        private Rect Marker5;
        private Rect Marker6;
        private Rect Marker7;
        private Rect Marker8;

        private static Material lineMaterial1;
        private static Material lineMaterial2;
        private static Material lineMaterial3;
        private static Material lineMaterial4;
        private static Material lineMaterial5;

        public override void Draw()
        {
            if (!MapView.MapIsEnabled)
            {
                drawLandingGuides();
            }
        }


        public void drawTouchDownGuideL(StaticInstance obj)
        {
            if (!IsOpen()) { return; }
            if (obj == null)
            {
                vTDL = Vector3.zero;
                return;
            }

            vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;

            vTDL = Camera.main.WorldToScreenPoint(obj.gameObject.transform.position);
            soTDL = obj;
        }

        public void drawTouchDownGuideR(StaticInstance obj)
        {
            if (!IsOpen()) { return; }
            if (obj == null)
            {
                vTDR = Vector3.zero;
                return;
            }

            vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;

            vTDR = Camera.main.WorldToScreenPoint(obj.gameObject.transform.position);
            soTDR = obj;
        }

        public void drawLandingGuide(StaticInstance obj)
        {
            if (!IsOpen()) { return; }
            if (obj == null)
            {
                vLineStart = Vector3.zero;
                vLineEnd = Vector3.zero;
                return;
            }

            vesCraft = FlightGlobals.ActiveVessel;
            if (vesCraft == null) return;

            Log.Debug("KK: drawLandingGuide");

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
            float flgHscale = 1f;

            if (fDist > 10000)
            {
                flgWscale = 250 / fDist;
                flgHscale = 100 / fDist;
            }
            else if (fDist > 5000)
            {
                flgWscale = 500 / fDist;
                flgHscale = 250 / fDist;
            }
            else if (fDist > 2500)
            {
                flgWscale = 750 / fDist;
                flgHscale = 500 / fDist;
            }
            else if (fDist > 1000)
            {
                flgWscale = 1000 / fDist;
                flgHscale = 750 / fDist;
            }
            else
            {
                flgWscale = 1f;
                flgHscale = 1f;
            }

            if (vTDL != Vector3.zero && vTDR != Vector3.zero)
            {
                vTDR = Camera.main.WorldToScreenPoint(soTDR.gameObject.transform.position);
                vTDL = Camera.main.WorldToScreenPoint(soTDL.gameObject.transform.position);

                Marker7 = new Rect((float)(vTDL.x) - (50 * flgWscale), (float)(Screen.height - vTDL.y) - (140 * flgHscale), 100 * flgWscale, 150 * flgHscale);
                Marker8 = new Rect((float)(vTDR.x) - (50 * flgWscale), (float)(Screen.height - vTDR.y) - (140 * flgHscale), 100 * flgWscale, 150 * flgHscale);

                GUI.DrawTexture(Marker7, tTGL, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(Marker8, tTGR, ScaleMode.StretchToFill, true);
            }

            if (vLineEnd != Vector3.zero && vLineStart != Vector3.zero)
            {
                vLineEnd = Camera.main.WorldToScreenPoint(vesCraft.transform.position);
                vLineStart = Camera.main.WorldToScreenPoint(soLandingGuide.gameObject.transform.position);

                // ToDo: Not implemented
                //Color clGood = new Color(0f, 1f, 0f, 0.8f);
                //Color clBad = new Color(1f, 0f, 0f, 0.8f);
                Color clLandGuide = new Color(1f, 1f, 1f, 0.8f);

                if (fDist < 8000)
                {
                    CreateLineMaterial(1);

                    GL.Begin(GL.LINES);
                    lineMaterial1.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3(vLineEnd.x - Screen.width / 2, vLineEnd.y - Screen.height / 2, vLineEnd.z);
                    GL.Vertex3(vLineStart.x - Screen.width / 2, vLineStart.y - Screen.height / 2, vLineStart.z);
                    GL.End();

                    CreateLineMaterial(2);

                    GL.Begin(GL.LINES);
                    lineMaterial2.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3(vLineStart.x - Screen.width / 2, (vLineStart.y - Screen.height / 2) + 200, vLineStart.z);
                    GL.Vertex3(vLineStart.x - Screen.width / 2, (vLineStart.y - Screen.height / 2) - 150, vLineStart.z);
                    GL.End();

                    CreateLineMaterial(3);

                    GL.Begin(GL.LINES);
                    lineMaterial3.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) + 100, (vLineStart.y - Screen.height / 2) + 5, vLineStart.z);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) - 100, (vLineStart.y - Screen.height / 2) + 5, vLineStart.z);
                    GL.End();

                    CreateLineMaterial(4);

                    GL.Begin(GL.LINES);
                    lineMaterial4.SetPass(0);
                    GL.Color(clLandGuide);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) + 150, (vLineStart.y - Screen.height / 2) + 50, vLineStart.z);
                    GL.Vertex3((vLineStart.x - Screen.width / 2) - 150, (vLineStart.y - Screen.height / 2) + 50, vLineStart.z);
                    GL.End();
                }

                Marker1 = new Rect((float)(vLineStart.x) - (25 * flgWscale), (float)(Screen.height - vLineStart.y) - 10, 50 * flgWscale, 4);
                Marker2 = new Rect((float)(vLineStart.x) - (50 * flgWscale), (float)(Screen.height - vLineStart.y) - 25, 100 * flgWscale, 4);
                Marker3 = new Rect((float)(vLineStart.x) - (75 * flgWscale), (float)(Screen.height - vLineStart.y) - 40, 150 * flgWscale, 4);

                Marker4 = new Rect((float)(Screen.width / 2) - (25 * flgWscale), (float)(Screen.height / 2) - 10, 50 * flgWscale, 4);
                Marker5 = new Rect((float)(Screen.width / 2) - (50 * flgWscale), (float)(Screen.height / 2) - 25, 100 * flgWscale, 4);
                Marker6 = new Rect((float)(Screen.width / 2) - (75 * flgWscale), (float)(Screen.height / 2) - 40, 150 * flgWscale, 4);

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

        private static void CreateLineMaterial(int iMat = 1)
        {
            Material mMat = lineMaterial1;
            if (iMat == 2) mMat = lineMaterial2;
            if (iMat == 3) mMat = lineMaterial3;
            if (iMat == 4) mMat = lineMaterial4;
            if (iMat == 5) mMat = lineMaterial5;

            if (mMat == null)
            {
                var shader = Shader.Find("Hidden/Internal-Colored");
                mMat = new Material(shader);
                mMat.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                mMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                mMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                mMat.SetInt("_ZWrite", 0);

                if (iMat == 1) lineMaterial1 = mMat;
                if (iMat == 2) lineMaterial2 = mMat;
                if (iMat == 3) lineMaterial3 = mMat;
                if (iMat == 4) lineMaterial4 = mMat;
                if (iMat == 5) lineMaterial5 = mMat;
            }
        }

    }
}

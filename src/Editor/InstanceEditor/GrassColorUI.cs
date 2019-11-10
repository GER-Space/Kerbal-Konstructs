using System;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using KerbalKonstructs;

namespace KerbalKonstructs.UI
{
    public class GrassColorUI : KKWindow
    {
        private static GrassColorUI _instance = null;
        private static Rect windowRect = new Rect(500, 600, 300, 340);

        internal static StaticInstance selectedInstance;

        private string grasColorRStr, grasColorGStr, grasColorBStr, grasColorAStr;
        string grasTextureName;

        internal static string titleText = "GrasColor UI";

        internal static GrassColorUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrassColorUI();

                }
                return _instance;
            }
        }

        public override void Open()
        {
            base.Open();
        }


        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }

            if (!StaticsEditorGUI.instance.IsOpen() || !EditorGUI.instance.IsOpen() || !EditorGUI.grasColorEnabled || EditorGUI.selectedInstance == null)
            {
                this.Close();
            }

            if (EditorGUI.selectedInstance != selectedInstance)
            {
                selectedInstance = EditorGUI.selectedInstance;
                selectedInstance.HighlightObject(Color.clear);
                SetupFields();
            }

            drawGrasColorUI();
        }

        public override void Close()
        {
            selectedInstance.HighlightObject(XKCDColors.YellowGreen);
            selectedInstance = null;
            base.Close();
        }

        private void drawGrasColorUI()
        {
            windowRect = GUI.Window(0xB31B1F1, windowRect, GrasSelectorWindow, "", UIMain.KKWindow);
        }


        private void GrasSelectorWindow(int WindowID)
        {
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button(titleText, UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Height(21)))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select Preset", GUILayout.Height(23), GUILayout.Width(120)))
                {
                    GrassColorPresetUI.callBack = UpdateCallBack;
                    GrassColorPresetUI.instance.Open();
                }

                GUILayout.Space(20);
                if (GUILayout.Button("Pick Surface Color", GUILayout.Height(23), GUILayout.Width(120)))
                {
                    selectedInstance.GrasColor = GrassColorUtils.GetUnderGroundColor(selectedInstance);
                    selectedInstance.Update();
                    SetupFields();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUI.enabled = true;
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Red: ", GUILayout.Height(23));
                    GUILayout.FlexibleSpace();
                    grasColorRStr = (GUILayout.TextField(grasColorRStr, 7, GUILayout.Width(90), GUILayout.Height(23)));
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Green: ", GUILayout.Height(23));
                    GUILayout.FlexibleSpace();
                    grasColorGStr = (GUILayout.TextField(grasColorGStr, 7, GUILayout.Width(90), GUILayout.Height(23)));
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Blue: ", GUILayout.Height(23));
                    GUILayout.FlexibleSpace();
                    grasColorBStr = (GUILayout.TextField(grasColorBStr, 7, GUILayout.Width(90), GUILayout.Height(23)));
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Alpha: ", GUILayout.Height(23));
                    GUILayout.FlexibleSpace();
                    grasColorAStr = (GUILayout.TextField(grasColorAStr, 7, GUILayout.Width(80), GUILayout.Height(23)));
                }
                GUILayout.EndHorizontal();

            }
            GUI.enabled = true;
            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));


            GUILayout.Label("Grass Texture: ", GUILayout.Height(23));
            grasTextureName = (GUILayout.TextField(grasTextureName, 200, GUILayout.Width(280), GUILayout.Height(23)));

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply", GUILayout.Height(23), GUILayout.Width(80)))
                {
                    ApplySettings();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Cancel", GUILayout.Height(23)))
                {
                    this.Close();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Apply & Close", GUILayout.Height(23)))
                {
                    ApplySettings();
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private void ApplySettings()
        {
            selectedInstance.GrasColor.r = float.Parse(grasColorRStr);
            selectedInstance.GrasColor.g = float.Parse(grasColorGStr);
            selectedInstance.GrasColor.b = float.Parse(grasColorBStr);
            selectedInstance.GrasColor.a = float.Parse(grasColorAStr);

            if (KKGraphics.GetTexture(grasTextureName) != null)
            {
                //Log.Normal("Try to Set Texture to: " + grasTextureName);
                selectedInstance.GrasTexture = grasTextureName;
            }
            else
            {
                Log.UserWarning("GrasColorUI: Texture not found: " + grasTextureName);
                MiscUtils.HUDMessage("GrasColorUI: Texture not found: " + grasTextureName);
            }
            //Log.Normal("found Texture: " + selectedInstance.GrasTexture);
            selectedInstance.Update();
        }


        internal void SetupFields()
        {
            grasColorRStr = selectedInstance.GrasColor.r.ToString();
            grasColorGStr = selectedInstance.GrasColor.g.ToString();
            grasColorBStr = selectedInstance.GrasColor.b.ToString();
            grasColorAStr = selectedInstance.GrasColor.a.ToString();

            if (String.IsNullOrEmpty(selectedInstance.GrasTexture))
            {
                grasTextureName = "BUILTIN:/terrain_grass00_new";
            }
            else
            {
                grasTextureName = selectedInstance.GrasTexture;
            }
        }

        internal void UpdateCallBack(Color newColor, string newTexture)
        {
            selectedInstance.GrasColor = newColor;

            if (KKGraphics.GetTexture(newTexture) != null)
            {
                //Log.Normal("Updating Texture to: " + newTexture);
                selectedInstance.GrasTexture = newTexture;
                grasTextureName = newTexture;
            }
            else
            {
                Log.UserWarning("GrasColorUI: Texture not found: " + newTexture);
                MiscUtils.HUDMessage("GrasColorUI: Texture not found: " + newTexture);
            }
            selectedInstance.Update();
            SetupFields();
        }


    }
}
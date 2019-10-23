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
    public class GrasColorPresetUI : KKWindow
    {
        internal class ColorPreset
        {
            internal string name = "unset";
            internal Color grassColor = Color.clear;
            internal Color tarmacColor = Color.white;
            internal string nearGrassTexture = "BUILTIN:/terrain_grass00_new";
            internal string farGrassTexture = "BUILTIN:/terrain_grass00_new_detail";
            internal string tarmacTexture = "BUILTIN:/ksc_exterior_terrain_asphalt";
        };


        private static GrasColorPresetUI _instance = null;
        private static Rect windowRect = new Rect(300, 700, 400, 340);

        private List<ColorPreset> colors2Display = new List<ColorPreset>();
        private Vector2 scrollPointer;

        private ColorPreset selectedPreset;

        internal static bool showOnlyLocal = true;
        internal static Action<ColorPreset> callBack = delegate { };
        internal static string titleText = "Select a Preset";

        private bool isInitialized = false;

        internal static GrasColorPresetUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrasColorPresetUI();

                }
                return _instance;
            }
        }

        public override void Open()
        {
            CreateColorList();
            base.Open();
        }



        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }

            if (!StaticsEditorGUI.instance.IsOpen() )
            {
                this.Close();
            }

            drawColorSelector();
        }

        public override void Close()
        {
            callBack = delegate {
            };
            selectedPreset = null;
            base.Close();
        }

        private void drawColorSelector()
        {
            windowRect = GUI.Window(0xB17B103, windowRect, ColorSelectorWindow, "", UIMain.KKWindow);
        }


        private void ColorSelectorWindow(int WindowID)
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
                    //KerbalKonstructs.instance.saveObjects();
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));



            scrollPointer = GUILayout.BeginScrollView(scrollPointer);
            {

                foreach (ColorPreset preset in colors2Display)
                {
                    GUI.enabled = (selectedPreset != preset);
                    if (GUILayout.Button(preset.name))
                    {
                        selectedPreset = preset;
                        callBack.Invoke(selectedPreset);
                        this.Close();
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndScrollView();
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                //if (GUILayout.Button("OK", GUILayout.Height(23)))
                //{
                //    callBack.Invoke(selectedPreset.color, selectedPreset.texture);
                //    this.Close();
                //}
                if (GUILayout.Button("Cancel", GUILayout.Height(23)))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }




        private void CreateColorList()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            colors2Display.Clear();
            foreach (ConfigNode colorNode in GameDatabase.Instance.GetConfigNodes("KK_ColorPreset"))
            {

                //if (!colorNode.HasValue("IsGrasColor") || bool.Parse(colorNode.GetValue("IsGrasColor")))
                //{
                //    continue;
                //}

                if (colorNode.HasValue("Name") && colorNode.HasValue("GrassColor"))
                {
                    ColorPreset preset = new ColorPreset();
                    preset.name = colorNode.GetValue("Name");
                    Log.Normal("Adding Color to List" + preset.name);
                    preset.grassColor = ConfigNode.ParseColor(colorNode.GetValue("GrassColor"));
                    if (colorNode.HasValue("NearGrassTexture"))
                    {
                        preset.nearGrassTexture = colorNode.GetValue("NearGrassTexture");
                    }
                    if (colorNode.HasValue("FarGrassTexture"))
                    {
                        preset.farGrassTexture = colorNode.GetValue("FarGrassTexture");
                    }
                    if (colorNode.HasValue("TarmacTexture"))
                    {
                        preset.tarmacTexture = colorNode.GetValue("TarmacTexture");
                    }
                    if (colorNode.HasValue("TarmacColor"))
                    {
                        preset.tarmacColor = ConfigNode.ParseColor(colorNode.GetValue("TarmacColor"));
                    }
                    colors2Display.Add(preset);
                }

            }
        }


    }
}

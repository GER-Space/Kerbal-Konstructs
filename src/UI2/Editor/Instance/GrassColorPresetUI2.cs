using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Lists the grass presets for GrassColor2 (KK_ColorPreset2 config nodes); clicking one calls callBack.
    /// UI2 version of the old IMGUI window.
    /// </summary>
    internal class GrassColorPresetUI2 : KKEditorWindow
    {
        internal class ColorPreset2
        {
            internal string name = "KSC Default";
            internal Color grassColor = GrassColor2.defaultColor;
            internal Color tarmacColor = Color.white;
            internal string nearGrassTexture = "BUILTIN:/terrain_grass00_new";
            internal string farGrassTexture = "BUILTIN:/terrain_grass00_new_detail";
            internal string tarmacTexture = "BUILTIN:/ksc_exterior_terrain_asphalt";
        };


        private static GrassColorPresetUI2 _instance = null;
        internal static GrassColorPresetUI2 instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrassColorPresetUI2();
                }
                return _instance;
            }
        }

        internal static Action<ColorPreset2> callBack = delegate { };
        internal static string titleText = "Select a Preset";

        private static List<ColorPreset2> colors2Display = new List<ColorPreset2>();
        private static bool isInitialized = false;

        protected override string WindowName => "GrassColorPresetUI2";
        protected override string WindowTitle => titleText;
        protected override float WindowWidth => 300f;
        protected override Vector2 DefaultPosition => new Vector2(0.4f, 0.5f);


        internal void Open()
        {
            CreateColorList();
            DismissDialog();
            Build();
        }


        public override void Close()
        {
            callBack = delegate { };
            DismissDialog();
        }


        protected override void OnUpdate()
        {
            if (!KKEditorLogic.IsEditorOpen())
            {
                Close();
            }
        }


        protected override void CreateContent()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            foreach (ColorPreset2 preset in colors2Display)
            {
                ColorPreset2 thisPreset = preset;
                list.Add(new DialogGUIButton(thisPreset.name, delegate { Select(thisPreset); }, 240f, itemHeight, false));
            }
            if (colors2Display.Count == 0)
            {
                list.Add(Label("No presets found"));
            }
            var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            content.Add(new DialogGUIScrollList(new Vector2(WindowWidth - 20f, 260f), false, true, layout));
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIButton("Cancel", Close, 100f, itemHeight, false)));
        }


        private void Select(ColorPreset2 preset)
        {
            Action<ColorPreset2> action = callBack;
            Close();
            action?.Invoke(preset);
        }


        private static void CreateColorList()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            colors2Display.Clear();
            foreach (ConfigNode colorNode in GameDatabase.Instance.GetConfigNodes("KK_ColorPreset2"))
            {
                if (colorNode.HasValue("Name") && colorNode.HasValue("GrassColor"))
                {
                    ColorPreset2 preset = new ColorPreset2();
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

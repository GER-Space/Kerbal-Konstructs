using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Lists the grass presets for the old GrasColor module (KK_ColorPreset config nodes); clicking one calls callBack.
    /// UI2 version of the old IMGUI window.
    /// </summary>
    internal class GrassColorPresetUI : KKEditorWindow
    {
        internal class ColorPreset
        {
            internal string name = "unset";
            internal Color color = Color.clear;
            internal string texture = "BUILTIN:/terrain_grass00_new";
        };


        private static GrassColorPresetUI _instance = null;
        internal static GrassColorPresetUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrassColorPresetUI();
                }
                return _instance;
            }
        }

        internal static Action<Color, string> callBack = delegate { };
        internal static string titleText = "Select a Preset";

        private static List<ColorPreset> colors2Display = new List<ColorPreset>();
        private static bool isInitialized = false;

        protected override string WindowName => "GrassColorPresetUI";
        protected override string WindowTitle => titleText;
        protected override float WindowWidth => 300f;
        protected override Vector2 DefaultPosition => new Vector2(0.4f, 0.5f);


        internal void Open()
        {
            SetColorList();
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
            foreach (ColorPreset preset in colors2Display)
            {
                ColorPreset thisPreset = preset;
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


        private void Select(ColorPreset preset)
        {
            Action<Color, string> action = callBack;
            Close();
            action?.Invoke(preset.color, preset.texture);
        }


        private static void SetColorList()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            colors2Display.Clear();
            foreach (ConfigNode colorNode in GameDatabase.Instance.GetConfigNodes("KK_ColorPreset"))
            {
                if (colorNode.HasValue("Name") && colorNode.HasValue("Color"))
                {
                    ColorPreset preset = new ColorPreset();
                    preset.name = colorNode.GetValue("Name");
                    Log.Normal("Adding Color to List" + preset.name);
                    preset.color = ConfigNode.ParseColor(colorNode.GetValue("Color"));
                    if (colorNode.HasValue("Texture"))
                    {
                        preset.texture = colorNode.GetValue("Texture");
                    }
                    colors2Display.Add(preset);
                }
            }
        }
    }
}

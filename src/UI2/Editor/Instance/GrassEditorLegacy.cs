using KerbalKonstructs.Core;
using System;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Grass editor for statics with the old GrasColor module (one color and one texture per static).
    /// UI2 version of the old GrassColorUI. The texture is a button that opens the TexturePicker;
    /// changes apply at once, "Revert" goes back to the values the window was opened with.
    /// </summary>
    internal class GrassEditorLegacy : KKEditorWindow
    {
        private static GrassEditorLegacy _instance = null;
        internal static GrassEditorLegacy instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrassEditorLegacy();
                }
                return _instance;
            }
        }

        private const string defaultTexture = "BUILTIN:/terrain_grass00_new";

        internal static StaticInstance selectedInstance = null;

        // the values when the window was opened, for "Revert"
        private Color originalColor;
        private string originalTexture;

        protected override string WindowName => "GrassEditorLegacy";
        protected override string WindowTitle => "Grass Color (GrasColor)";
        protected override float WindowWidth => 340f;
        protected override Vector2 DefaultPosition => new Vector2(0.55f, 0.6f);


        internal void Open()
        {
            if (InstanceEditor.selectedInstance == null)
            {
                return;
            }
            DismissDialog();
            selectedInstance = InstanceEditor.selectedInstance;
            selectedInstance.HighlightObject(Color.clear);
            originalColor = selectedInstance.GrasColor;
            originalTexture = selectedInstance.GrasTexture;
            Build();
        }


        public override void Close()
        {
            if (selectedInstance != null && InstanceEditor.instance.IsOpen() && InstanceEditor.selectedInstance == selectedInstance)
            {
                selectedInstance.HighlightObject(XKCDColors.YellowGreen);
            }
            selectedInstance = null;
            TexturePicker.CloseWindow();
            DismissDialog();
        }


        protected override void OnUpdate()
        {
            if (!KKEditorLogic.IsEditorOpen() || !InstanceEditor.instance.IsOpen() || !InstanceEditor.grasColorEnabled || InstanceEditor.selectedInstance != selectedInstance)
            {
                Close();
            }
        }


        protected override void CreateContent()
        {
            DialogGUIButton presetButton = new DialogGUIButton("Select Preset", OpenPresets, 120f, itemHeight, false);
            presetButton.tooltipText = "Set color and texture from a grass preset";
            DialogGUIButton surfaceButton = new DialogGUIButton("Pick Surface Color", delegate { SetColor(GrassColorUtils.GetUnderGroundColor(selectedInstance)); }, 140f, itemHeight, false);
            surfaceButton.tooltipText = "Use the color of the terrain below the static";
            content.Add(new DialogGUIHorizontalLayout(presetButton, new DialogGUIFlexibleSpace(), surfaceButton));

            Separator();
            content.Add(ColorComponentRow("Red:", 0));
            content.Add(ColorComponentRow("Green:", 1));
            content.Add(ColorComponentRow("Blue:", 2));
            content.Add(ColorComponentRow("Alpha:", 3));

            DialogGUIButton editButton = new DialogGUIButton("Edit Color", delegate
            {
                StaticInstance instance = selectedInstance;
                ColorSelector.selectedColor = selectedInstance.GrasColor;
                ColorSelector.callBack = delegate (Color c) { if (instance == selectedInstance) { SetColor(c); } };
                ColorSelector.Open();
            }, 120f, itemHeight, false);
            editButton.tooltipText = "Pick the color";
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), editButton));

            Separator();
            DialogGUIButton textureButton = new DialogGUIButton(
                delegate { return TexturePicker.ShortName(CurrentTexture()); },
                OpenTexturePicker,
                delegate { return true; },
                200f, itemHeight, false);
            textureButton.tooltipText = "Choose the grass texture";
            content.Add(new DialogGUIHorizontalLayout(Label("Grass texture:"), new DialogGUIFlexibleSpace(), textureButton));

            Separator();
            DialogGUIButton revertButton = new DialogGUIButton("Revert", Revert, 100f, itemHeight, false);
            revertButton.tooltipText = "Go back to the color and texture the window was opened with";
            content.Add(new DialogGUIHorizontalLayout(revertButton, new DialogGUIFlexibleSpace(), new DialogGUIButton("Close", Close, 100f, itemHeight, false)));
        }


        private DialogGUIBase ColorComponentRow(string label, int component)
        {
            return TextRow(label,
                delegate { return selectedInstance.GrasColor[component].ToString(System.Globalization.CultureInfo.InvariantCulture); },
                delegate (string s)
                {
                    float value;
                    if (TryParseFloat(s, out value) && value != selectedInstance.GrasColor[component])
                    {
                        Color color = selectedInstance.GrasColor;
                        color[component] = value;
                        SetColor(color);
                    }
                },
                7, 90f);
        }


        private string CurrentTexture()
        {
            return string.IsNullOrEmpty(selectedInstance.GrasTexture) ? defaultTexture : selectedInstance.GrasTexture;
        }


        private void SetColor(Color color)
        {
            selectedInstance.GrasColor = color;
            selectedInstance.Update();
        }


        private void SetTexture(string texture)
        {
            if (KKGraphics.GetTexture(texture) == null)
            {
                Log.UserWarning("GrassEditorLegacy: Texture not found: " + texture);
                MiscUtils.HUDMessage("Texture not found: " + texture);
                return;
            }
            selectedInstance.GrasTexture = texture;
            selectedInstance.Update();
        }


        private void OpenTexturePicker()
        {
            StaticInstance instance = selectedInstance;
            TexturePicker.Open("Grass Texture", TextureUsage.Texture, CurrentTexture,
                delegate (string texture) { if (instance == selectedInstance) { SetTexture(texture); } },
                delegate { return IsOpen() && instance == selectedInstance; });
        }


        private void OpenPresets()
        {
            GrassColorPresetUI.callBack = UpdateCallBack;
            GrassColorPresetUI.instance.Open();
        }


        private void Revert()
        {
            selectedInstance.GrasColor = originalColor;
            selectedInstance.GrasTexture = originalTexture;
            selectedInstance.Update();
        }


        /// <summary>
        /// Callback of the preset selector
        /// </summary>
        internal void UpdateCallBack(Color newColor, string newTexture)
        {
            ApplyPreset(selectedInstance, newColor, newTexture);
        }


        /// <summary>
        /// Sets a preset's color and texture on a static (also used by the Instance editor's "Preset" button)
        /// </summary>
        internal static void ApplyPreset(StaticInstance instance, Color newColor, string newTexture)
        {
            if (instance == null)
            {
                return;
            }
            instance.GrasColor = newColor;
            if (KKGraphics.GetTexture(newTexture) != null)
            {
                instance.GrasTexture = newTexture;
            }
            else
            {
                Log.UserWarning("GrassEditorLegacy: Texture not found: " + newTexture);
                MiscUtils.HUDMessage("Texture not found: " + newTexture);
            }
            instance.Update();
        }
    }
}

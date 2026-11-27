using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Grass Editor: edits the GrassColor2 / KKTerrain module(s) of the static selected in the Instance editor.
    /// KKTerrain: the grass shows the terrain color at the static's position; setting a color here (or a preset) turns on
    /// overwrite_color, the "Terrain color" switch turns it off again. The grass textures are the planet's there.
    /// The texture rows are buttons that open the TexturePicker (pictures in a 3 x 3 grid);
    /// the grayscale / random options are on/off switches. Changes apply at once.
    /// </summary>
    internal class GrassEditor : KKEditorWindow
    {
        private static GrassEditor _instance = null;
        internal static GrassEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrassEditor();
                }
                return _instance;
            }
        }

        protected override string WindowName => "GrassEditor";
        protected override string WindowTitle => "Grass Editor";
        protected override float WindowWidth => 380f;
        protected override Vector2 DefaultPosition => new Vector2(0.55f, 0.5f);

        private const float textureButtonWidth = 200f;
        private const float colorFieldWidth = 150f;

        internal static StaticInstance staticInstance = null;
        internal static GrassColor2 selectedMod = null;


        #region Window handling (static API used by the Instance editor)

        internal static void Open()
        {
            StaticInstance instanceToEdit = InstanceEditor.selectedInstance;
            if (instanceToEdit == null || instanceToEdit.mesh == null)
            {
                return;
            }
            GrassColor2[] mods = instanceToEdit.mesh.GetComponents<GrassColor2>();
            if (mods.Length == 0)
            {
                Log.UserError("No Grass Mod Found");
                return;
            }
            instance.DismissDialog();
            staticInstance = instanceToEdit;
            staticInstance.HighlightObject(Color.clear);
            selectedMod = mods[0];
            instance.Build();
        }


        internal static void CloseWindow()
        {
            instance.Close();
        }


        internal static bool isOpen => (_instance != null && _instance.IsOpen());


        internal static void Toggle()
        {
            if (isOpen)
            {
                CloseWindow();
            }
            else
            {
                Open();
            }
        }


        public override void Close()
        {
            if (InstanceEditor.instance.IsOpen() && staticInstance != null && InstanceEditor.selectedInstance == staticInstance)
            {
                staticInstance.HighlightObject(XKCDColors.Green_Yellow);
            }
            TexturePicker.CloseWindow();
            ColorSelector.Close();
            DismissDialog();
        }


        protected override void OnUpdate()
        {
            // close with the Instance editor, or when another static is selected there
            if (!InstanceEditor.instance.IsOpen() || staticInstance != InstanceEditor.selectedInstance || selectedMod == null)
            {
                Close();
            }
        }

        #endregion


        #region Content

        protected override void CreateContent()
        {
            DialogGUIButton presetButton = new DialogGUIButton("Select Preset", OpenPresets, 120f, itemHeight, false);
            presetButton.tooltipText = "Set colors and textures from a grass preset";
            DialogGUIButton surfaceButton = new DialogGUIButton("Pick Surface Color", PickSurfaceColor, 140f, itemHeight, false);
            surfaceButton.tooltipText = "Use the color of the terrain below the static as grass color";
            content.Add(new DialogGUIHorizontalLayout(presetButton, new DialogGUIFlexibleSpace(), surfaceButton));

            // more than one grass mesh: choose which one to edit
            GrassColor2[] mods = staticInstance.mesh.GetComponents<GrassColor2>();
            if (mods.Length > 1)
            {
                List<DialogGUIBase> meshButtons = new List<DialogGUIBase> { Label("Mesh:"), new DialogGUIFlexibleSpace() };
                foreach (GrassColor2 mod in mods)
                {
                    GrassColor2 thisMod = mod;
                    meshButtons.Add(new DialogGUIButton(thisMod.GrassMeshName, delegate { SelectGrass(thisMod); }, delegate { return selectedMod != thisMod; }, 90f, itemHeight, false));
                }
                content.Add(new DialogGUIHorizontalLayout(meshButtons.ToArray()));
            }

            // grass
            Separator();
            bool terrain = IsTerrain(selectedMod);
            if (terrain)
            {
                content.Add(SwitchRow("Terrain color:", delegate { KKTerrain t = selectedMod as KKTerrain; return t != null && !t.OverwriteColor; },
                    delegate (bool v) { KKTerrain t = selectedMod as KKTerrain; if (t != null) { t.SetOverwriteColor(!v); } }));
            }
            content.Add(ColorRow("Grass color:", delegate { return ShownGrassColor(selectedMod); }, delegate (Color c) { if (!SameColor(c, ShownGrassColor(selectedMod))) { selectedMod.grassColor = c; selectedMod.OnUserColorChange(); } }));
            if (terrain)
            {
                content.Add(new DialogGUIHorizontalLayout(Label("Grass textures:"), new DialogGUIFlexibleSpace(), Label("the planet's (terrain shader)")));
            }
            else
            {
                content.Add(TextureRow("Near texture:", "Near Grass Texture", TextureUsage.Texture, delegate { return selectedMod.nearGrassTextureName; }, delegate (string t) { selectedMod.nearGrassTextureName = t; }));
                content.Add(FloatRow("Near tiling:", delegate { return selectedMod.nearGrassTiling; }, delegate (float v) { selectedMod.nearGrassTiling = v; }));
                content.Add(SwitchRow("Near texture grayscale:", delegate { return selectedMod.nearGrassTextureGray; }, delegate (bool v) { selectedMod.nearGrassTextureGray = v; selectedMod.ApplySettings(); }));
                content.Add(TextureRow("Far texture:", "Far Grass Texture", TextureUsage.Texture, delegate { return selectedMod.farGrassTextureName; }, delegate (string t) { selectedMod.farGrassTextureName = t; }));
                content.Add(FloatRow("Far tiling:", delegate { return selectedMod.farGrassTiling; }, delegate (float v) { selectedMod.farGrassTiling = v; }));
                content.Add(SwitchRow("Far texture grayscale:", delegate { return selectedMod.farGrassTextureGray; }, delegate (bool v) { selectedMod.farGrassTextureGray = v; selectedMod.ApplySettings(); }));
                content.Add(FloatRow("Far blend distance:", delegate { return selectedMod.farGrassBlendDistance; }, delegate (float v) { selectedMod.farGrassBlendDistance = v; }));
            }

            // tarmac
            Separator();
            content.Add(TextureRow("Tarmac texture:", "Tarmac Texture", TextureUsage.Texture, delegate { return selectedMod.tarmacTextureName; }, delegate (string t) { selectedMod.tarmacTextureName = t; }));
            content.Add(ColorRow("Tarmac color:", delegate { return selectedMod.tarmacColor; }, delegate (Color c) { selectedMod.tarmacColor = c; }));
            content.Add(new DialogGUIHorizontalLayout(
                Label("Tarmac tiling U:"),
                TextField(delegate { return selectedMod.tarmacTiling.x.ToString(CultureInfo.InvariantCulture); }, delegate (string s) { float u; if (TryParseFloat(s, out u)) { selectedMod.tarmacTiling = new Vector2(u, selectedMod.tarmacTiling.y); selectedMod.ApplySettings(); } }, 10, 60f),
                new DialogGUIFlexibleSpace(),
                Label("V:"),
                TextField(delegate { return selectedMod.tarmacTiling.y.ToString(CultureInfo.InvariantCulture); }, delegate (string s) { float v; if (TryParseFloat(s, out v)) { selectedMod.tarmacTiling = new Vector2(selectedMod.tarmacTiling.x, v); selectedMod.ApplySettings(); } }, 10, 60f)
                ));
            content.Add(SwitchRow("Tarmac random tiling:", delegate { return selectedMod.tarmacTileRandom; }, delegate (bool v) { selectedMod.tarmacTileRandom = v; selectedMod.ApplySettings(); }));
            content.Add(SwitchRow("Tarmac grayscale:", delegate { return selectedMod.tarmacTextureGray; }, delegate (bool v) { selectedMod.tarmacTextureGray = v; selectedMod.ApplySettings(); }));

            // blend mask
            Separator();
            DialogGUIButton reloadButton = new DialogGUIButton("Reload", ReloadBlendMask, 60f, itemHeight, false);
            reloadButton.tooltipText = "Load the blend mask file again (after editing it outside of KSP)";
            content.Add(new DialogGUIHorizontalLayout(
                Label("Blend mask:"),
                new DialogGUIFlexibleSpace(),
                TextureButton("Blend Mask", TextureUsage.BlendMask, delegate { return selectedMod.blendMaskTextureName; }, delegate (string t) { selectedMod.blendMaskTextureName = t; }, textureButtonWidth - 64f),
                reloadButton
                ));

            // third and fourth texture
            Separator();
            content.Add(TextureRow("Third texture:", "Third Texture", TextureUsage.Texture, delegate { return selectedMod.thirdTextureName; }, delegate (string t) { selectedMod.thirdTextureName = t; }));
            content.Add(ColorRow("Third color:", delegate { return selectedMod.thirdTextureColor; }, delegate (Color c) { selectedMod.thirdTextureColor = c; }));
            content.Add(FloatRow("Third tiling:", delegate { return selectedMod.thirdTextureTiling; }, delegate (float v) { selectedMod.thirdTextureTiling = v; }));
            if (terrain)
            {
                // the terrain shader maps the third texture like the terrain, without random tiling
                content.Add(SwitchRow("Third texture grayscale:", delegate { return selectedMod.thirdTextureGray; }, delegate (bool v) { selectedMod.thirdTextureGray = v; selectedMod.ApplySettings(); }));
            }
            else
            {
                content.Add(TwoSwitchRow(
                    "Random:", delegate { return selectedMod.thirdTextureTileRandom; }, delegate (bool v) { selectedMod.thirdTextureTileRandom = v; selectedMod.ApplySettings(); },
                    "Grayscale:", delegate { return selectedMod.thirdTextureGray; }, delegate (bool v) { selectedMod.thirdTextureGray = v; selectedMod.ApplySettings(); }));
            }

            Separator();
            content.Add(TextureRow("Fourth texture:", "Fourth Texture", TextureUsage.Texture, delegate { return selectedMod.fourthTextureName; }, delegate (string t) { selectedMod.fourthTextureName = t; }));
            content.Add(ColorRow("Fourth color:", delegate { return selectedMod.fourthTextureColor; }, delegate (Color c) { selectedMod.fourthTextureColor = c; }));
            content.Add(FloatRow("Fourth tiling:", delegate { return selectedMod.fourthTextureTiling; }, delegate (float v) { selectedMod.fourthTextureTiling = v; }));
            if (terrain)
            {
                content.Add(SwitchRow("Fourth texture grayscale:", delegate { return selectedMod.fourthTextureGray; }, delegate (bool v) { selectedMod.fourthTextureGray = v; selectedMod.ApplySettings(); }));
            }
            else
            {
                content.Add(TwoSwitchRow(
                    "Random:", delegate { return selectedMod.fourthTextureTileRandom; }, delegate (bool v) { selectedMod.fourthTextureTileRandom = v; selectedMod.ApplySettings(); },
                    "Grayscale:", delegate { return selectedMod.fourthTextureGray; }, delegate (bool v) { selectedMod.fourthTextureGray = v; selectedMod.ApplySettings(); }));
            }

            Separator();
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIButton("Close", Close, 100f, itemHeight, false)));
        }


        /// <summary>
        /// label ...... [texture name] : the button opens the texture picker
        /// </summary>
        private DialogGUIBase TextureRow(string label, string pickerTitle, TextureUsage usage, Func<string> getTexture, Action<string> setTexture)
        {
            return new DialogGUIHorizontalLayout(
                Label(label),
                new DialogGUIFlexibleSpace(),
                TextureButton(pickerTitle, usage, getTexture, setTexture, textureButtonWidth)
                );
        }


        private DialogGUIButton TextureButton(string pickerTitle, TextureUsage usage, Func<string> getTexture, Action<string> setTexture, float width)
        {
            DialogGUIButton button = new DialogGUIButton(
                delegate { return TexturePicker.ShortName(getTexture()); },
                delegate
                {
                    GrassColor2 mod = selectedMod;
                    TexturePicker.Open(pickerTitle, usage, getTexture, delegate (string texture) { if (mod == selectedMod) { setTexture(texture); selectedMod.ApplySettings(); } }, delegate { return isOpen && mod == selectedMod; });
                },
                delegate { return true; },
                width, itemHeight, false);
            button.tooltipText = "Choose the " + pickerTitle.ToLower();
            return button;
        }


        /// <summary>
        /// label ...... [r,g,b,a] [Edit] : Edit opens the color selector
        /// </summary>
        private DialogGUIBase ColorRow(string label, Func<Color> getColor, Action<Color> setColor)
        {
            DialogGUIButton editButton = new DialogGUIButton("Edit", delegate
            {
                GrassColor2 mod = selectedMod;
                ColorSelector.callBack = delegate (Color c) { if (mod == selectedMod) { setColor(c); selectedMod.ApplySettings(); } };
                ColorSelector.selectedColor = getColor();
                ColorSelector.Open();
            }, 46f, itemHeight, false);
            editButton.tooltipText = "Pick the color";

            return new DialogGUIHorizontalLayout(
                Label(label),
                new DialogGUIFlexibleSpace(),
                TextField(delegate { return ConfigNode.WriteColor(getColor()); }, delegate (string s) { Color c; if (TryParseColor(s, out c)) { setColor(c); selectedMod.ApplySettings(); } }, 40, colorFieldWidth),
                editButton
                );
        }


        private DialogGUIBase FloatRow(string label, Func<float> getValue, Action<float> setValue)
        {
            return TextRow(label,
                delegate { return getValue().ToString(CultureInfo.InvariantCulture); },
                delegate (string s) { float value; if (TryParseFloat(s, out value)) { setValue(value); selectedMod.ApplySettings(); } },
                10, numberWidth);
        }


        private DialogGUIBase TwoSwitchRow(string label1, Func<bool> get1, Action<bool> set1, string label2, Func<bool> get2, Action<bool> set2)
        {
            return new DialogGUIHorizontalLayout(
                Label(label1),
                KKControls.Switch(get1, set1),
                new DialogGUIFlexibleSpace(),
                Label(label2),
                KKControls.Switch(get2, set2)
                );
        }


        private static bool TryParseColor(string text, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            string[] parts = text.Split(',');
            if (parts.Length < 3 || parts.Length > 4)
            {
                return false;
            }
            float[] values = new float[] { 1f, 1f, 1f, 1f };
            for (int i = 0; i < parts.Length; i++)
            {
                if (!TryParseFloat(parts[i], out values[i]))
                {
                    return false;
                }
            }
            color = new Color(values[0], values[1], values[2], values[3]);
            return true;
        }

        #endregion


        #region Actions

        private void OpenPresets()
        {
            GrassColor2 mod = selectedMod;
            GrassColorPresetUI2.callBack = delegate (GrassColorPresetUI2.ColorPreset2 preset) { if (mod != null) { mod.OnUserColorChange(); mod.UpdateCallBack(preset); } };
            GrassColorPresetUI2.instance.Open();
        }


        private static void PickSurfaceColor()
        {
            KKTerrain terrain = selectedMod as KKTerrain;
            if (terrain != null && IsTerrain(terrain))
            {
                // the terrain shader takes the terrain color itself
                terrain.SetOverwriteColor(false);
                return;
            }
            selectedMod.grassColor = GrassColorUtils.GetUnderGroundColor(selectedMod);
            selectedMod.ApplySettings();
        }


        /// <summary>
        /// The color field calls its setter whenever it loses the focus: only a real change sets the color (and so turns
        /// on overwrite_color)
        /// </summary>
        private static bool SameColor(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.002f && Mathf.Abs(a.g - b.g) < 0.002f && Mathf.Abs(a.b - b.b) < 0.002f && Mathf.Abs(a.a - b.a) < 0.002f;
        }


        /// <summary>
        /// A KKTerrain module drawn by the terrain shader
        /// </summary>
        private static bool IsTerrain(GrassColor2 mod)
        {
            KKTerrain terrain = mod as KKTerrain;
            return terrain != null && terrain.UsesTerrainShader;
        }


        /// <summary>
        /// The grass color as it is shown: the terrain color of a KKTerrain module that doesn't overwrite it
        /// </summary>
        private static Color ShownGrassColor(GrassColor2 mod)
        {
            KKTerrain terrain = mod as KKTerrain;
            if (terrain != null && IsTerrain(terrain) && !terrain.OverwriteColor)
            {
                Color c = terrain.terrainColor;
                return new Color(c.r, c.g, c.b, 1f);
            }
            return mod.grassColor;
        }


        private static void ReloadBlendMask()
        {
            KKGraphics.RemoveCache(selectedMod.blendMaskTextureName);
            selectedMod.ApplySettings();
        }


        private void SelectGrass(GrassColor2 grass)
        {
            bool rebuild = IsTerrain(grass) != IsTerrain(selectedMod);
            selectedMod = grass;
            TexturePicker.CloseWindow();
            if (rebuild)
            {
                // the rows differ between the terrain shader and GrassColor2
                Build();
            }
        }

        #endregion
    }
}

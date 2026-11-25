using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Picks a texture for the grass editors: the textures are shown as pictures in a grid of 3 x 3
    /// (scrolls when there are more than 9), like the height / color map selector of the Terrain editor.
    /// Below the grid a path can be typed for a texture that is not in the list.
    /// (replaces the text list of the old TextureSelector)
    /// </summary>
    internal class TexturePicker : KKEditorWindow
    {
        private static TexturePicker _instance = null;
        private static TexturePicker instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TexturePicker();
                }
                return _instance;
            }
        }

        private const int columns = 3;
        private const int visibleRows = 3;
        private const float imageSize = 96f;
        private const float nameHeight = 20f;
        private const float spacing = 8f;
        private const float cellHeight = imageSize + nameHeight + 4f;

        private string title = "Select Texture";
        private TextureUsage usage = TextureUsage.Texture;
        private Func<string> getCurrent = null;
        private Action<string> onSelect = null;
        private Func<bool> keepOpen = null;
        private string customPath = "";

        protected override string WindowName => "TexturePicker";
        protected override string WindowTitle => title;
        protected override float WindowWidth => columns * imageSize + (columns - 1) * spacing + 50f;
        protected override Vector2 DefaultPosition => new Vector2(0.6f, 0.55f);


        /// <summary>
        /// Opens the picker.
        /// </summary>
        /// <param name="windowTitle">title, e.g. "Near Grass Texture"</param>
        /// <param name="textureUsage">which presets to show (textures or blend masks)</param>
        /// <param name="current">the texture that is set now; shown in gold and can't be picked again</param>
        /// <param name="select">called with the path of the picked texture</param>
        /// <param name="stayOpen">the picker closes as soon as this returns false (e.g. the grass editor closed)</param>
        internal static void Open(string windowTitle, TextureUsage textureUsage, Func<string> current, Action<string> select, Func<bool> stayOpen)
        {
            TexturePicker picker = instance;
            picker.DismissDialog();
            picker.title = windowTitle;
            picker.usage = textureUsage;
            picker.getCurrent = current ?? delegate { return ""; };
            picker.onSelect = select;
            picker.keepOpen = stayOpen;
            picker.customPath = picker.getCurrent() ?? "";
            picker.Build();
        }


        internal static void CloseWindow()
        {
            instance.Close();
        }


        internal static bool isOpen => (_instance != null && _instance.IsOpen());


        public override void Close()
        {
            DismissDialog();
            onSelect = null;
            keepOpen = null;
        }


        protected override void OnUpdate()
        {
            if (keepOpen != null && !keepOpen())
            {
                Close();
            }
        }


        protected override void CreateContent()
        {
            List<string> textures = TexturePresets.GetTextures(usage);

            // a texture that is set but not in the presets is shown first, so it can be seen
            string current = getCurrent();
            if (!string.IsNullOrEmpty(current) && !textures.Contains(current))
            {
                textures.Insert(0, current);
            }

            content.Add(TextureGrid(textures));
            content.Add(new DialogGUIHorizontalLayout(
                Label("Current:"),
                GoldLabel(delegate { return ShortName(getCurrent()); })
                ));

            // any other texture: type its path
            DialogGUIButton useButton = new DialogGUIButton("Use", delegate { Select(customPath); }, delegate { return !string.IsNullOrEmpty(customPath) && customPath != getCurrent(); }, 50f, itemHeight, false);
            useButton.tooltipText = "Use the texture at this path, e.g. BUILTIN:/terrain_grass00_new or KerbalKonstructs/Assets/...";
            content.Add(new DialogGUIHorizontalLayout(
                Label("Path:"),
                TextField(delegate { return customPath; }, delegate (string s) { customPath = s; }, 200, WindowWidth - 110f),
                useButton
                ));
        }


        /// <summary>
        /// The textures as a grid with 3 pictures per row, in a scroll area 3 rows high
        /// </summary>
        private DialogGUIScrollList TextureGrid(List<string> textures)
        {
            List<DialogGUIBase> rows = new List<DialogGUIBase>();
            rows.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            for (int i = 0; i < textures.Count; i += columns)
            {
                List<DialogGUIBase> row = new List<DialogGUIBase>();
                for (int c = 0; c < columns; c++)
                {
                    if (i + c < textures.Count)
                    {
                        row.Add(TextureCell(textures[i + c]));
                    }
                    else
                    {
                        // keep the last row aligned to the grid
                        row.Add(new DialogGUISpace(imageSize));
                    }
                }
                rows.Add(new DialogGUIHorizontalLayout(false, false, spacing, new RectOffset(), TextAnchor.UpperLeft, row.ToArray()));
            }

            if (textures.Count == 0)
            {
                rows.Add(Label("No textures found"));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, spacing, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, rows.ToArray());
            float viewHeight = visibleRows * cellHeight + (visibleRows - 1) * spacing + 8f;
            return new DialogGUIScrollList(new Vector2(WindowWidth - 20f, viewHeight), false, true, layout);
        }


        /// <summary>
        /// One texture: its picture (click to pick it) with its file name below
        /// </summary>
        private DialogGUIBase TextureCell(string path)
        {
            string thisPath = path;
            Texture2D picture = null;
            try
            {
                picture = KKGraphics.GetTexture(thisPath);
            }
            catch (Exception e)
            {
                Log.Normal("TexturePicker: cannot load " + thisPath + ": " + e.Message);
            }

            DialogGUIButton button;
            if (picture != null)
            {
                button = KKStyle.ImageButton(picture, delegate { Select(thisPath); }, imageSize, imageSize);
            }
            else
            {
                button = new DialogGUIButton("not found", delegate { Select(thisPath); }, imageSize, imageSize, false);
            }
            // the current texture can't be picked again
            button.OptionInteractableCondition = () => getCurrent() != thisPath;
            button.tooltipText = thisPath;

            DialogGUILabel label = new DialogGUILabel(delegate { return ShortName(thisPath); }, (getCurrent() == thisPath) ? KKStyle.goldLabel : KKStyle.whiteLabel);

            return new DialogGUIVerticalLayout(imageSize, cellHeight, 2f, new RectOffset(), TextAnchor.UpperCenter, button, label);
        }


        private void Select(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (KKGraphics.GetTexture(path) == null)
            {
                MiscUtils.HUDMessage("Texture not found: " + path, 10, 3);
                return;
            }
            Action<string> callback = onSelect;
            Close();
            callback?.Invoke(path);
        }


        /// <summary>
        /// The file name without folders and the BUILTIN:/ prefix
        /// </summary>
        internal static string ShortName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "none";
            }
            string name = path.Replace("BUILTIN:/", "").Replace('\\', '/');
            int slash = name.LastIndexOf('/');
            return (slash >= 0 && slash < name.Length - 1) ? name.Substring(slash + 1) : name;
        }
    }
}

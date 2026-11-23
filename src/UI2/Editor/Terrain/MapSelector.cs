using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Shows the height maps or the color maps as pictures in a grid of 3 x 3; scrolls when there are more than 9.
    /// Clicking a picture sets it as the height / color map of the decal in the MapDecal editor.
    /// (was the text lists under the "Height Map" and "Color Map" buttons of the MapDecal editor)
    /// </summary>
    internal class MapSelector
    {
        internal enum MapType
        {
            Height,
            Color
        }
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "MapSelector";
        internal static string windowMessage = null;
        internal static string windowTitle = "Select Height Map";

        // which maps the window shows
        private static MapType mapType = MapType.Height;

        internal static Rect windowRect;

        private const int columns = 3;
        private const int visibleRows = 3;
        private const float imageSize = 96f;
        private const float labelHeight = 20f;
        private const float spacing = 8f;
        private const float cellHeight = imageSize + labelHeight + 4f;

        // 3 columns, plus padding and room for the scrollbar
        internal static float windowWidth = columns * imageSize + (columns - 1) * spacing + 50f;
        internal static float windowHeight = 100f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.6f, 0.55f);

        // the decal the window was opened for; it closes when another decal is selected
        private static MapDecalInstance decal = null;


        internal static void KKTitle()
        {
            if (!showKKTitle)
            {
                return;
            }
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel(windowTitle, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)
                ));
        }


        internal static void CreateContent()
        {
            content.Add(MapGrid());
            content.Add(new DialogGUILabel("Current: " + GetCurrentMapName(), KKStyle.whiteLabel));
        }


        /// <summary>
        /// The height maps as a grid with 3 pictures per row, in a scroll area 3 rows high.
        /// </summary>
        private static DialogGUIScrollList MapGrid()
        {
            List<DialogGUIBase> rows = new List<DialogGUIBase>();
            rows.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<MapDecalsMap> maps = (mapType == MapType.Height) ? DecalsDatabase.allHeightMaps : DecalsDatabase.allColorMaps;
            for (int i = 0; i < maps.Count; i += columns)
            {
                List<DialogGUIBase> row = new List<DialogGUIBase>();
                for (int c = 0; c < columns; c++)
                {
                    if (i + c < maps.Count)
                    {
                        row.Add(MapCell(maps[i + c]));
                    }
                    else
                    {
                        // keep the last row aligned to the grid
                        row.Add(new DialogGUISpace(imageSize));
                    }
                }
                rows.Add(new DialogGUIHorizontalLayout(false, false, spacing, new RectOffset(), TextAnchor.UpperLeft, row.ToArray()));
            }

            if (maps.Count == 0)
            {
                rows.Add(new DialogGUILabel((mapType == MapType.Height) ? "No height maps found" : "No color maps found", KKStyle.whiteLabel));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, spacing, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, rows.ToArray());
            float viewHeight = visibleRows * cellHeight + (visibleRows - 1) * spacing + 8f;
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, viewHeight), false, true, layout);
        }


        /// <summary>
        /// One map: its picture (click to select it) with its name below.
        /// </summary>
        private static DialogGUIBase MapCell(MapDecalsMap map)
        {
            // local copy for the delegates
            MapDecalsMap thisMap = map;
            bool isCurrent = (GetCurrentMapName() == thisMap.Name);

            DialogGUIButton button;
            Texture2D picture = GetPicture(thisMap);
            if (picture != null)
            {
                button = KKStyle.ImageButton(picture, delegate { SelectMap(thisMap); }, imageSize, imageSize);
            }
            else
            {
                // e.g. "None": no picture
                button = new DialogGUIButton(thisMap.Name, delegate { SelectMap(thisMap); }, imageSize, imageSize, false);
            }
            // the current map can't be selected again
            button.OptionInteractableCondition = () => decal == null || GetCurrentMapName() != thisMap.Name;
            button.tooltipText = thisMap.Name;

            DialogGUILabel label = new DialogGUILabel(thisMap.Name, isCurrent ? KKStyle.goldLabel : KKStyle.whiteLabel);

            return new DialogGUIVerticalLayout(imageSize, cellHeight, 2f, new RectOffset(), TextAnchor.UpperCenter,
                button,
                label);
        }


        // height deformity set when a height map is chosen while the deformity is still 0
        private const double defaultHeightMapDeformity = 300;


        // pictures made from map data, for maps without an image file (the stock KSP maps)
        private static Dictionary<MapDecalsMap, Texture2D> previews = new Dictionary<MapDecalsMap, Texture2D>();
        private const int previewSize = 96;


        /// <summary>
        /// The map's image, or for maps without one (the stock KSP maps) a small picture made
        /// from the map data (greyscale for height maps). Null for "None".
        /// </summary>
        private static Texture2D GetPicture(MapDecalsMap map)
        {
            if (map.mapTexture != null)
            {
                return map.mapTexture;
            }
            if (map.map == null)
            {
                return null;
            }
            Texture2D preview;
            if (previews.TryGetValue(map, out preview))
            {
                return preview;
            }
            preview = null;
            try
            {
                int width = map.map.Width;
                int height = map.map.Height;
                if (width > 0 && height > 0)
                {
                    preview = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
                    for (int y = 0; y < previewSize; y++)
                    {
                        for (int x = 0; x < previewSize; x++)
                        {
                            int px = x * width / previewSize;
                            int py = y * height / previewSize;
                            if (map.isHeightMap)
                            {
                                float value = map.map.GetPixelFloat(px, py);
                                preview.SetPixel(x, y, new Color(value, value, value, 1f));
                            }
                            else
                            {
                                Color color = map.map.GetPixelColor(px, py);
                                color.a = 1f;
                                preview.SetPixel(x, y, color);
                            }
                        }
                    }
                    preview.Apply();
                }
            }
            catch (Exception e)
            {
                Log.Normal("MapSelector: no preview for " + map.Name + ": " + e.Message);
                preview = null;
            }
            // also remember a failed preview, so it isn't retried every time the window opens
            previews[map] = preview;
            return preview;
        }


        private static string GetCurrentMapName()
        {
            if (decal == null)
            {
                return "";
            }
            return (mapType == MapType.Height) ? decal.HeightMapName : decal.ColorMapName;
        }


        private static void SelectMap(MapDecalsMap map)
        {
            if (decal != null)
            {
                if (mapType == MapType.Height)
                {
                    decal.HeightMapName = map.Name;
                    // a height map does nothing with a deformity of 0: start with a useful value
                    if (map.Name != "None" && decal.HeightMapDeformity == 0)
                    {
                        decal.HeightMapDeformity = defaultHeightMapDeformity;
                    }
                }
                else
                {
                    decal.ColorMapName = map.Name;
                }
            }
            Close();
        }


        private static void OnUpdate()
        {
            // close with the MapDecal editor, or when another decal is selected there
            if (!MapDecalEditor.Instance.IsOpen() || MapDecalEditor.selectedDecal != decal)
            {
                Close();
            }
        }


        internal static void CreateMultiOptionDialog()
        {
            windowRect = new Rect(WindowManager2.GetPosition(windowName, defaultPosition), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, null, windowRect, content.ToArray());
        }


        internal static void CreatePopUp()
        {
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), optionDialog,
                   false,
                   null, isModal);
            // allow dragging the window up to the screen border
            KKWindowDrag.Attach(dialog);
            if (!showTitle)
            {
                dialog.gameObject.GetChild("Title").SetActive(false);
            }
            dialog.dialogToDisplay.OnUpdate += OnUpdate;
            KKStyle.ApplyIconButtons(content);
            KKStyle.AttachTooltips(content);
        }


        /// <summary>
        /// Opens the window with the height or color maps, for the decal selected in the MapDecal editor
        /// </summary>
        internal static void Open(MapType type)
        {
            if (MapDecalEditor.selectedDecal == null)
            {
                return;
            }
            if (dialog != null)
            {
                if (decal == MapDecalEditor.selectedDecal && mapType == type)
                {
                    return;
                }
                Close();
            }
            decal = MapDecalEditor.selectedDecal;
            mapType = type;
            windowTitle = (type == MapType.Height) ? "Select Height Map" : "Select Color Map";
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
        }


        internal static void Close()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
            decal = null;
        }


        internal static bool isOpen
        {
            get
            {
                return (dialog != null);
            }
        }

        internal static bool IsOpen()
        {
            return (dialog != null);
        }

        /// <summary>
        /// Closes the window if it shows this map type, otherwise opens it with this map type
        /// </summary>
        internal static void Toggle(MapType type)
        {
            if (isOpen && mapType == type)
            {
                Close();
            }
            else
            {
                Open(type);
            }
        }
    }
}

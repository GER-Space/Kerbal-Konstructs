using KerbalKonstructs.Core;
using KSP.UI.TooltipTypes;
using System.Collections.Generic;
using UnityEngine;



namespace KerbalKonstructs.UI2
{
    internal class KKStyle
    {
        internal static UIStyle DeadButtonRed;
        internal static UIStyle whiteLabel;
        internal static UIStyle goldLabel;
        internal static UIStyle windowTitle;

        internal static UIStyle defaultLabel => HighLogic.UISkin.label;

        //private static Sprite clearSprite;

        internal static void Init()
        {
            //CreateSprite();
            DeadButtonRed = defaultStyle;
            DeadButtonRed.name = "DeadButtonRed";
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.highlight.textColor = Color.red;
            DeadButtonRed.disabled.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;
            DeadButtonRed.alignment = TextAnchor.MiddleCenter;
            DeadButtonRed.normal.background = null;
            DeadButtonRed.highlight.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.disabled.background = null;

            whiteLabel = defaultStyle;
            whiteLabel.name = "whiteLabel";
            whiteLabel.normal.textColor = Color.white;
            whiteLabel.active.textColor = Color.white;
            whiteLabel.highlight.textColor = Color.white;
            whiteLabel.disabled.textColor = Color.white;
            whiteLabel.fontSize = 12;
            whiteLabel.fontStyle = FontStyle.Normal;
            whiteLabel.alignment = TextAnchor.MiddleLeft;

            goldLabel = defaultStyle;
            goldLabel.name = "goldLabel";
            goldLabel.normal.textColor = XKCDColors.Gold;
            goldLabel.active.textColor = XKCDColors.Gold;
            goldLabel.highlight.textColor = XKCDColors.Gold;
            goldLabel.disabled.textColor = XKCDColors.Gold;
            goldLabel.fontSize = 12;
            goldLabel.fontStyle = FontStyle.Normal;
            goldLabel.alignment = TextAnchor.MiddleLeft;

            windowTitle = defaultStyle;
            windowTitle.name = "windowTitle";
            windowTitle.normal.textColor = XKCDColors.KSPNeutralUIGrey;
            //windowTitle.active.textColor = XKCDColors.KSPNeutralUIGrey;
            //windowTitle.highlight.textColor = XKCDColors.KSPNeutralUIGrey;
            //windowTitle.disabled.textColor = XKCDColors.KSPNeutralUIGrey;
            windowTitle.fontSize = 12;
            windowTitle.fontStyle = FontStyle.Normal;
            windowTitle.alignment = TextAnchor.MiddleCenter;

        }


        //private static void CreateSprite()
        //{
        //    Color fillColor = new Color(0,0,0,0);
        //    Texture2D newTex = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        //    var fillColorArray = newTex.GetPixels();

        //    for (var i = 0; i < fillColorArray.Length; ++i)
        //    {
        //        fillColorArray[i] = fillColor;
        //    }
        //    clearSprite = Sprite.Create(newTex, new Rect(0, 0, 16, 16), new Vector2(0, 1), 128f);


        //}


        private static Sprite LoadSprite(string textureName)
        {
            Texture2D texture = GameDatabase.Instance.GetTexture(textureName, false);
            if (texture == null)
            {
                return null;
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }


        /// <summary>
        /// Button style that shows an icon for every button state.
        /// Expects baseName.png (normal) and optional baseName_hover / _down / _disabled.png;
        /// missing state images fall back to the normal one. Returns null if baseName.png is missing.
        /// </summary>
        internal static UIStyle CreateIconButtonStyle(string baseName)
        {
            Sprite normal = LoadSprite(baseName);
            if (normal == null)
            {
                Log.UserWarning("Icon not found: " + baseName);
                return null;
            }
            Sprite hover = LoadSprite(baseName + "_hover") ?? normal;
            Sprite down = LoadSprite(baseName + "_down") ?? hover;
            Sprite disabled = LoadSprite(baseName + "_disabled") ?? normal;

            UIStyle style = defaultStyle;
            style.name = "KKIcon_" + baseName;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = normal;
            style.highlight.background = hover;
            style.active.background = down;
            style.disabled.background = disabled;
            return style;
        }


        /// <summary>
        /// KSP stores DialogGUIBase.tooltipText but doesn't create a tooltip from it.
        /// Call this after the dialog was spawned: it adds a tooltip to every created
        /// element (recursively) that has a tooltipText.
        /// </summary>
        internal static void AttachTooltips(IEnumerable<DialogGUIBase> items)
        {
            if (items == null)
            {
                return;
            }
            Tooltip_Text prefab = null;
            foreach (DialogGUIBase item in items)
            {
                AttachTooltip(item, ref prefab);
            }
        }


        private static void AttachTooltip(DialogGUIBase item, ref Tooltip_Text prefab)
        {
            if (item == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(item.tooltipText) && item.uiItem != null)
            {
                if (prefab == null)
                {
                    prefab = AssetBase.GetPrefab<Tooltip_Text>("Tooltip_Text");
                }
                if (prefab != null)
                {
                    TooltipController_Text tooltip = item.uiItem.GetComponent<TooltipController_Text>();
                    if (tooltip == null)
                    {
                        tooltip = item.uiItem.AddComponent<TooltipController_Text>();
                    }
                    tooltip.prefab = prefab;
                    tooltip.SetText(item.tooltipText);
                    // also show the tooltip on disabled buttons (e.g. the active editor mode)
                    tooltip.RequireInteractable = false;
                }
            }

            if (item.children != null)
            {
                foreach (DialogGUIBase child in item.children)
                {
                    AttachTooltip(child, ref prefab);
                }
            }
        }


        private static UIStyle defaultStyle
        {
            get
            {
                UIStyle defaultstyle = new UIStyle();
                defaultstyle.normal = new UIStyleState();
                defaultstyle.active = new UIStyleState();
                defaultstyle.highlight = new UIStyleState();
                defaultstyle.disabled = new UIStyleState();
                defaultstyle.normal.textColor = HighLogic.UISkin.label.normal.textColor;
                defaultstyle.active.textColor = HighLogic.UISkin.label.normal.textColor;
                defaultstyle.highlight.textColor = HighLogic.UISkin.label.normal.textColor;
                defaultstyle.disabled.textColor = HighLogic.UISkin.label.normal.textColor;
                defaultstyle.fontSize = HighLogic.UISkin.label.fontSize;
                defaultstyle.fontStyle = FontStyle.Normal;
                defaultstyle.font = HighLogic.UISkin.label.font;
                defaultstyle.alignment = TextAnchor.MiddleLeft;
                return defaultstyle;
            }
        }


    }
}

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


        // icon buttons waiting for ApplyIconButtons (cleared when applied)
        private static Dictionary<DialogGUIBase, UIStyle> pendingIconButtons = new Dictionary<DialogGUIBase, UIStyle>();
        // image buttons waiting for ApplyIconButtons (cleared when applied)
        private static Dictionary<DialogGUIBase, Sprite> pendingImageButtons = new Dictionary<DialogGUIBase, Sprite>();
        // clickable pictures (e.g. live RenderTextures) waiting for ApplyIconButtons (cleared when applied)
        private static Dictionary<DialogGUIBase, Callback> pendingClickableImages = new Dictionary<DialogGUIBase, Callback>();
        private static Dictionary<DialogGUIBase, Callback> pendingRightClicks = new Dictionary<DialogGUIBase, Callback>();


        /// <summary>
        /// A picture that works as a button. Unlike ImageButton it takes any Texture, also a
        /// RenderTexture that changes every frame. Tinted on hover / press.
        /// Call ApplyIconButtons(content) after the dialog was spawned.
        /// </summary>
        internal static DialogGUIImage ClickableImage(Texture texture, Callback onClick, float width, float height)
        {
            return ClickableImage(texture, onClick, null, width, height);
        }


        /// <summary>
        /// Like ClickableImage, with a separate action for the right mouse button.
        /// </summary>
        internal static DialogGUIImage ClickableImage(Texture texture, Callback onClick, Callback onRightClick, float width, float height)
        {
            DialogGUIImage image = new DialogGUIImage(new Vector2(width, height), Vector2.zero, Color.white, texture);
            pendingClickableImages[image] = onClick;
            if (onRightClick != null)
            {
                pendingRightClicks[image] = onRightClick;
            }
            return image;
        }

        // cached sprites for image buttons, so a rebuilt window doesn't create new ones
        private static Dictionary<Texture2D, Sprite> imageSprites = new Dictionary<Texture2D, Sprite>();


        /// <summary>
        /// Creates a button that shows a picture (e.g. a map decal image). On hover and press
        /// the picture is tinted instead of replaced. Call ApplyIconButtons(content) after the
        /// dialog was spawned.
        /// </summary>
        internal static DialogGUIButton ImageButton(Texture2D texture, Callback onClick, float width, float height)
        {
            DialogGUIButton button = new DialogGUIButton("", onClick, width, height, false);
            Sprite sprite;
            if (!imageSprites.TryGetValue(texture, out sprite) || sprite == null)
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imageSprites[texture] = sprite;
            }
            pendingImageButtons[button] = sprite;
            return button;
        }


        /// <summary>
        /// Creates a button that shows an icon style (from CreateIconButtonStyle) in all states.
        /// Call ApplyIconButtons(content) after the dialog was spawned.
        /// </summary>
        internal static DialogGUIButton IconButton(UIStyle iconStyle, Callback onClick, float width, float height)
        {
            DialogGUIButton button = new DialogGUIButton("", onClick, width, height, false, iconStyle);
            pendingIconButtons[button] = iconStyle;
            return button;
        }


        /// <summary>
        /// KSP doesn't apply the normal-state image of a button style (the icon only showed
        /// while pressed). Call this after the dialog was spawned: it sets the image for every
        /// state directly on the created Unity buttons.
        /// </summary>
        internal static void ApplyIconButtons(IEnumerable<DialogGUIBase> items)
        {
            if (items == null)
            {
                return;
            }
            foreach (DialogGUIBase item in items)
            {
                ApplyIconButton(item);
            }
        }


        private static void ApplyIconButton(DialogGUIBase item)
        {
            if (item == null)
            {
                return;
            }

            Callback clickAction;
            if (pendingClickableImages.TryGetValue(item, out clickAction))
            {
                pendingClickableImages.Remove(item);
                if (item.uiItem != null)
                {
                    UnityEngine.UI.Graphic graphic = item.uiItem.GetComponent<UnityEngine.UI.Graphic>();
                    if (graphic != null)
                    {
                        graphic.raycastTarget = true;
                    }
                    UnityEngine.UI.Button button = item.uiItem.GetComponent<UnityEngine.UI.Button>();
                    if (button == null)
                    {
                        button = item.uiItem.AddComponent<UnityEngine.UI.Button>();
                    }
                    button.targetGraphic = graphic;
                    button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                    UnityEngine.UI.ColorBlock colors = button.colors;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(0.75f, 1f, 0.6f, 1f);
                    colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                    colors.selectedColor = Color.white;
                    colors.colorMultiplier = 1f;
                    button.colors = colors;
                    Callback action = clickAction;
                    button.onClick.AddListener(delegate { if (action != null) { action(); } });

                    Callback rightClickAction;
                    if (pendingRightClicks.TryGetValue(item, out rightClickAction))
                    {
                        KKRightClick rightClick = item.uiItem.GetComponent<KKRightClick>();
                        if (rightClick == null)
                        {
                            rightClick = item.uiItem.AddComponent<KKRightClick>();
                        }
                        rightClick.onRightClick = rightClickAction;
                    }
                }
                pendingRightClicks.Remove(item);
            }

            Sprite picture;
            if (pendingImageButtons.TryGetValue(item, out picture))
            {
                pendingImageButtons.Remove(item);
                if (item.uiItem != null)
                {
                    UnityEngine.UI.Button button = item.uiItem.GetComponent<UnityEngine.UI.Button>();
                    UnityEngine.UI.Image image = (button != null && button.image != null) ? button.image : item.uiItem.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        image.sprite = picture;
                        image.type = UnityEngine.UI.Image.Type.Simple;
                        image.preserveAspect = true;
                        image.color = Color.white;
                    }
                    if (button != null)
                    {
                        // keep the picture, tint it on hover / press
                        button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                        UnityEngine.UI.ColorBlock colors = button.colors;
                        colors.normalColor = Color.white;
                        colors.highlightedColor = new Color(0.75f, 1f, 0.6f, 1f);
                        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                        colors.selectedColor = Color.white;
                        colors.disabledColor = new Color(0.6f, 0.8f, 0.45f, 1f);
                        colors.colorMultiplier = 1f;
                        button.colors = colors;
                    }
                    // remove the button's text so it doesn't cover the picture
                    TMPro.TextMeshProUGUI text = item.uiItem.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                    if (text != null)
                    {
                        text.text = "";
                    }
                }
            }

            UIStyle style;
            if (pendingIconButtons.TryGetValue(item, out style))
            {
                pendingIconButtons.Remove(item);
                if (item.uiItem != null)
                {
                    UnityEngine.UI.Button button = item.uiItem.GetComponent<UnityEngine.UI.Button>();
                    UnityEngine.UI.Image image = (button != null && button.image != null) ? button.image : item.uiItem.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        image.sprite = style.normal.background;
                        image.type = UnityEngine.UI.Image.Type.Simple;
                        image.preserveAspect = true;
                        image.color = Color.white;
                    }
                    if (button != null)
                    {
                        button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
                        UnityEngine.UI.SpriteState states = new UnityEngine.UI.SpriteState();
                        states.highlightedSprite = style.highlight.background;
                        states.pressedSprite = style.active.background;
                        // after a click Unity keeps the button "selected": show the normal icon
                        states.selectedSprite = style.normal.background;
                        states.disabledSprite = style.disabled.background;
                        button.spriteState = states;
                    }
                }
            }

            if (item.children != null)
            {
                foreach (DialogGUIBase child in item.children)
                {
                    ApplyIconButton(child);
                }
            }
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


    /// <summary>
    /// Calls onRightClick when the UI element is clicked with the right mouse button
    /// (Unity's Button only reacts to the left one).
    /// </summary>
    internal class KKRightClick : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        internal Callback onRightClick = null;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right && onRightClick != null)
            {
                onRightClick();
            }
        }
    }
}

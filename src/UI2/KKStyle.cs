using UnityEngine;



namespace KerbalKonstructs.UI2
{
    internal class KKStyle
    {
        internal static UIStyle DeadButtonRed;
        internal static UIStyle whiteLabel;
        internal static UIStyle windowTitle;

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

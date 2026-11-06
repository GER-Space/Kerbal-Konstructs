using UnityEngine;

namespace KerbalKonstructs.UI
{
    public static class UIMain
    {
        internal static bool layoutIsInitialized = false;


        public static Texture VABIcon;
        public static Texture ANYIcon;
        public static Texture TrackingStationIcon;
        public static Texture heliPadIcon;
        public static Texture runWayIcon;
        public static Texture waterLaunchIcon;

        public static Texture2D tNormalButton;
        public static Texture2D tHoverButton;

        public static Texture tOpenBasesOn;
        public static Texture tOpenBasesOff;
        public static Texture tClosedBasesOn;
        public static Texture tClosedBasesOff;
        public static Texture tHelipadsOn;
        public static Texture tHelipadsOff;
        public static Texture tRunwaysOn;
        public static Texture tRunwaysOff;
        public static Texture tTrackingOn;
        public static Texture tTrackingOff;
        public static Texture tLaunchpadsOn;
        public static Texture tLaunchpadsOff;
        public static Texture tOtherOn;
        public static Texture tOtherOff;
        public static Texture tWaterOn;
        public static Texture tWaterOff;
        public static Texture tHideOn;
        public static Texture tHideOff;

        public static Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

        public static Texture tFavesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapFavouritesOn", false);
        public static Texture tFavesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapFavouritesOff", false);

        public static Texture iconWorld = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/world", false);
        public static Texture iconCubes = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cubes", false);
        public static Texture iconTerrain = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/terrain", false);

        public static Texture iconRecoveryBase = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/recoveryBase", false);

        public static Texture tIconClosed;
        public static Texture tIconOpen;
        public static Texture tLeftOn;
        public static Texture tLeftOff;
        public static Texture tRightOn;
        public static Texture tRightOff;

        public static GUIStyle Yellowtext;
        public static GUIStyle TextAreaNoBorder;
        public static GUIStyle BoxNoBorder;
        public static GUIStyle BoxNoBorderW;
        public static GUIStyle ButtonKK;
        public static GUIStyle ButtonInactive;
        public static GUIStyle ButtonRed;
        public static GUIStyle DeadButton3;
        public static GUIStyle DeadButtonRed;
        public static GUIStyle KKToolTip;
        public static GUIStyle LabelWhite;
        public static GUIStyle LabelRed;
        public static GUIStyle DeadButton;
        public static GUIStyle LabelInfo;
        public static GUIStyle ButtonTextYellow;
        public static GUIStyle ButtonTextOrange;
        public static GUIStyle ButtonDefault;

        public static GUIStyle KKWindow;

        public static GUIStyle navStyle;

        public static void SetStyles()
        {
            navStyle = new GUIStyle();
            navStyle.padding.left = 0;
            navStyle.padding.right = 0;
            navStyle.padding.top = 1;
            navStyle.padding.bottom = 3;
            navStyle.normal.background = null;


            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;

            ButtonRed = new GUIStyle(GUI.skin.button);
            ButtonRed.normal.textColor = Color.red;
            ButtonRed.active.textColor = Color.red;
            ButtonRed.focused.textColor = Color.red;
            ButtonRed.hover.textColor = Color.red;

            ButtonKK = new GUIStyle(GUI.skin.button);
            ButtonKK.padding.left = 0;
            ButtonKK.padding.right = 0;
            ButtonKK.normal.background = tNormalButton;
            ButtonKK.hover.background = tHoverButton;

            ButtonInactive = new GUIStyle(GUI.skin.button);
            ButtonInactive.padding.left = 0;
            ButtonInactive.padding.right = 0;
            ButtonInactive.normal.background = tNormalButton;
            ButtonInactive.hover.background = tHoverButton;
            ButtonInactive.normal.textColor = XKCDColors.Grey;
            ButtonInactive.active.textColor = XKCDColors.Grey;
            ButtonInactive.focused.textColor = XKCDColors.Grey;
            ButtonInactive.hover.textColor = XKCDColors.Grey;

            Yellowtext = new GUIStyle(GUI.skin.box);
            Yellowtext.normal.textColor = Color.yellow;
            Yellowtext.normal.background = null;

            TextAreaNoBorder = new GUIStyle(GUI.skin.textArea);
            TextAreaNoBorder.normal.background = null;

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;

            BoxNoBorderW = new GUIStyle(GUI.skin.box);
            BoxNoBorderW.normal.background = null;
            BoxNoBorderW.normal.textColor = Color.white;

            KKToolTip = new GUIStyle(GUI.skin.box);
            KKToolTip.normal.textColor = Color.white;
            KKToolTip.fontSize = 11;
            KKToolTip.fontStyle = FontStyle.Normal;

            LabelWhite = new GUIStyle(GUI.skin.label);
            LabelWhite.normal.textColor = Color.white;
            LabelWhite.fontSize = 13;
            LabelWhite.fontStyle = FontStyle.Normal;
            LabelWhite.padding.bottom = 1;
            LabelWhite.padding.top = 1;

            LabelRed = new GUIStyle(GUI.skin.label);
            LabelRed.normal.textColor = XKCDColors.TomatoRed;
            LabelRed.fontSize = 13;
            LabelRed.fontStyle = FontStyle.Normal;
            LabelRed.padding.bottom = 1;
            LabelRed.padding.top = 1;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.white;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.white;
            DeadButton.focused.textColor = Color.white;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Bold;

            DeadButton3 = new GUIStyle(GUI.skin.button);
            DeadButton3.normal.background = null;
            DeadButton3.hover.background = null;
            DeadButton3.active.background = null;
            DeadButton3.focused.background = null;
            DeadButton3.normal.textColor = Color.white;
            DeadButton3.hover.textColor = Color.white;
            DeadButton3.active.textColor = Color.white;
            DeadButton3.focused.textColor = Color.white;
            DeadButton3.fontSize = 13;
            DeadButton3.fontStyle = FontStyle.Bold;



            KKWindow = new GUIStyle(GUI.skin.window);
            KKWindow.padding = new RectOffset(8, 8, 3, 3);

            ButtonTextYellow = new GUIStyle(GUI.skin.button);
            ButtonTextYellow.normal.textColor = XKCDColors.YellowGreen;
            ButtonTextYellow.active.textColor = XKCDColors.YellowGreen;
            ButtonTextYellow.focused.textColor = XKCDColors.YellowGreen;
            ButtonTextYellow.hover.textColor = XKCDColors.YellowGreen;

            ButtonTextOrange = new GUIStyle(GUI.skin.button);
            ButtonTextOrange.normal.textColor = XKCDColors.PumpkinOrange;
            ButtonTextOrange.active.textColor = XKCDColors.PumpkinOrange;
            ButtonTextOrange.focused.textColor = XKCDColors.PumpkinOrange;
            ButtonTextOrange.hover.textColor = XKCDColors.PumpkinOrange;

            ButtonDefault = new GUIStyle(GUI.skin.button);

        }

        public static void SetTextures()
        {
            VABIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/VABMapIcon", false);
            heliPadIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscHelipadIcon", false);
            ANYIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ANYMapIcon", false);
            TrackingStationIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/TrackingMapIcon", false);
            runWayIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kscRunwayIcon", false);
            waterLaunchIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/boatlaunchIcon", false);

            tNormalButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonNormal", false);
            tHoverButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonHover", false);

            tOpenBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOn", false);
            tOpenBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOff", false);
            tClosedBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOn", false);
            tClosedBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOff", false);
            tHelipadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOn", false);
            tHelipadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOff", false);
            tRunwaysOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOn", false);
            tRunwaysOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOff", false);
            tTrackingOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOn", false);
            tTrackingOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOff", false);
            tLaunchpadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOn", false);
            tLaunchpadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOff", false);
            tOtherOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOn", false);
            tOtherOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOff", false);
            tHideOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHideOn", false);
            tHideOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHideOff", false);
            tWaterOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapWaterOn", false);
            tWaterOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapWaterOff", false);

            tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
            tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
            tLeftOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lefton", false);
            tLeftOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/leftoff", false);
            tRightOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/righton", false);
            tRightOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/rightoff", false);
        }
    }
}
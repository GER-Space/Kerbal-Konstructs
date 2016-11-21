using UnityEngine;

namespace KerbalKonstructs.UI
{
	public static class UIMain
	{
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
		public static Texture tRadarCover;
		public static Texture tRadarOn;
		public static Texture tRadarOff;
		public static Texture tUplinksOn;
		public static Texture tUplinksOff;
		public static Texture tGroundCommsOn;
		public static Texture tGroundCommsOff;
		public static Texture tHideOn;
		public static Texture tHideOff;
		public static Texture tDownlinksOn;
		public static Texture tDownlinksOff;

		public static Texture tHorizontalSep;

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
		public static GUIStyle ButtonRed;
		public static GUIStyle DeadButtonRed;
		public static GUIStyle KKToolTip;

		public static GUIStyle navStyle;

		public static void setStyles()
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
		}

		public static void setTextures()
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
			tRadarCover = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/radarcover", false);
			tRadarOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRadarOn", false);
			tRadarOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRadarOff", false);
			tUplinksOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapUplinksOn", false);
			tUplinksOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapUplinksOff", false);
			tGroundCommsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapGroundCommsOn", false);
			tGroundCommsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapGroundCommsOff", false);
			tHideOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHideOn", false);
			tHideOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHideOff", false);
			tDownlinksOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapDownlinksOn", false);
			tDownlinksOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapDownlinksOff", false);

			tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

			tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
			tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
			tLeftOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/lefton", false);
			tLeftOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/leftoff", false);
			tRightOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/righton", false);
			tRightOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/rightoff", false);
		}
	}
}
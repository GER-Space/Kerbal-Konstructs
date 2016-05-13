using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class UIMain
	{
		public Texture VABIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/VABMapIcon", false);
		public Texture SPHIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SPHMapIcon", false);
		public Texture ANYIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ANYMapIcon", false);
		public Texture TrackingStationIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/TrackingMapIcon", false);

		public Texture2D tNormalButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonNormal", false);
		public Texture2D tHoverButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapButtonHover", false);

		public Texture tOpenBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOn", false);
		public Texture tOpenBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOpenBasesOff", false);
		public Texture tClosedBasesOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOn", false);
		public Texture tClosedBasesOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapClosedBasesOff", false);
		public Texture tHelipadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOn", false);
		public Texture tHelipadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHelipadsOff", false);
		public Texture tRunwaysOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOn", false);
		public Texture tRunwaysOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRunwaysOff", false);
		public Texture tTrackingOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOn", false);
		public Texture tTrackingOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapTrackingOff", false);
		public Texture tLaunchpadsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOn", false);
		public Texture tLaunchpadsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapLaunchpadsOff", false);
		public Texture tOtherOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOn", false);
		public Texture tOtherOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapOtherOff", false);
		public Texture tRadarCover = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/radarcover", false);
		public Texture tRadarOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRadarOn", false);
		public Texture tRadarOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapRadarOff", false);
		public Texture tUplinksOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapUplinksOn", false);
		public Texture tUplinksOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapUplinksOff", false);
		public Texture tGroundCommsOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapGroundCommsOn", false);
		public Texture tGroundCommsOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapGroundCommsOff", false);
		public Texture tHideOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHideOn", false);
		public Texture tHideOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapHideOff", false);
		public Texture tDownlinksOn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapDownlinksOn", false);
		public Texture tDownlinksOff = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/mapDownlinksOff", false);

		public GUIStyle Yellowtext;
		public GUIStyle TextAreaNoBorder;
		public GUIStyle BoxNoBorder;
		public GUIStyle ButtonKK;
		public GUIStyle ButtonRed;
		public GUIStyle KKToolTip;

		public GUIStyle navStyle = new GUIStyle();

		public void setStyles()
		{
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

			KKToolTip = new GUIStyle(GUI.skin.box);
			KKToolTip.normal.textColor = Color.white;
			KKToolTip.fontSize = 11;
			KKToolTip.fontStyle = FontStyle.Normal;
		}
	}
}
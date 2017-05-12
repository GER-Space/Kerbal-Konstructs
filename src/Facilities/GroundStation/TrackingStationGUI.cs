using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class TrackingStationGUI
    {
        public static string sTargetType = "None";
        public static string sTarget = "None";
        public static string sSelectedTrackingTarget = "None";
        public static string sDisplayTarget = "None";
        public static string sDisplayRange = "0m";
        public static string sUplink = "None";
        public static string sFacLvl = "Lvl 1/3";
        public static string sGroup = "Ungrouped";

        public static StaticObject selectedStation = null;

        public static float fRange = 0f;
        public static float fTSRange = 90000f;

        public static Boolean bChangeTargetType = false;

        public static Vector2 scrollPos;

        public static GUIStyle LabelInfo;
        public static GUIStyle BoxInfo;
        public static GUIStyle ButtonSmallText;

        public static string sButtonText = "";

        public static float fAlt = 0f;

        public static CelestialBody cPlanetoid = null;

        public static Vector3 ObjectPos = new Vector3(0, 0, 0);

        public static Double disObjectLat = 0;
        public static Double disObjectLon = 0;

        public static Boolean bGUIenabled = false;
        public static Boolean bCraftLock = false;

        public static Boolean bNotInit = false;

        public static void TrackingInterface(StaticObject soStation)
        {
            fRange = (float)soStation.getSetting("TrackingShort");
            sTargetType = (string)soStation.getSetting("TargetType");
            sTarget = (string)soStation.getSetting("TargetID");
            sGroup = (string)soStation.getSetting("Group");
            bCraftLock = false;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            BoxInfo = new GUIStyle(GUI.skin.box);
            BoxInfo.normal.textColor = Color.cyan;
            BoxInfo.fontSize = 13;
            BoxInfo.padding.top = 2;
            BoxInfo.padding.bottom = 1;
            BoxInfo.padding.left = 5;
            BoxInfo.padding.right = 5;
            BoxInfo.normal.background = null;

            GUILayout.Space(2);


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.Label("Range: " + (fRange).ToString("#0") + " tkm", LabelInfo, GUILayout.Height(25));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);



            GUILayout.BeginHorizontal();

            GUILayout.Label("Member of Group: " + sGroup, LabelInfo, GUILayout.Height(25));

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }



    }
}

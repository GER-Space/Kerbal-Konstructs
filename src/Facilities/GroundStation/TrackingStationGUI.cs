using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    internal class TrackingStationGUI
    {
        internal static string sGroup = "Ungrouped";

        internal static StaticInstance selectedStation = null;

        internal static float fRange = 0f;
        internal static float fTSRange = 90000f;

        internal static Boolean bChangeTargetType = false;



        internal static GUIStyle LabelInfo;
        internal static GUIStyle BoxInfo;


        internal static string sButtonText = "";

        internal static float fAlt = 0f;

        internal static CelestialBody cPlanetoid = null;

        internal static Vector3 ObjectPos = new Vector3(0, 0, 0);

        internal static Double disObjectLat = 0;
        internal static Double disObjectLon = 0;

        internal static Boolean bGUIenabled = false;
        internal static Boolean bCraftLock = false;

        internal static Boolean bNotInit = false;

        internal static void TrackingInterface(StaticInstance instance)
        {
            GroundStation myStation = instance.myFacilities[0] as GroundStation;

            fRange = myStation.TrackingShort;
            sGroup = instance.Group;
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

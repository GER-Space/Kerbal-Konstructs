using System;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using KerbalKonstructs;

namespace KerbalKonstructs.UI
{
    public class GroupSelectorUI : KKWindow
    {
        private static GroupSelectorUI _instance = null;
        private static Rect windowRect = new Rect(300, 700, 400, 340);

        private List<GroupCenter> centers2Display = new List<GroupCenter>();
        private Vector2 scrollPointer;
        private GroupCenter selectedCenter = null;


        internal static bool showOnlyLocal = true;
        internal static Action<GroupCenter> callBack = delegate { };
        internal static string titleText = "unset";

        internal static GroupSelectorUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GroupSelectorUI();

                }
                return _instance;
            }
        }

        public override void Open()
        {
            SetGroupList();
            base.Open();
        }



        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }

            if (!StaticsEditorGUI.instance.IsOpen())
            {
                this.Close();
            }

            drawGroupSelector();
        }

        public override void Close()
        {
            callBack = delegate {};
            selectedCenter = null;
            titleText = "unset";
            base.Close();
        }

        private void drawGroupSelector()
        {
            windowRect = GUI.Window(0xB10B1E3, windowRect, GroupSelectorWindow, "", UIMain.KKWindow);
        }


        private void GroupSelectorWindow(int WindowID)
        {
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button(titleText, UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));



            scrollPointer = GUILayout.BeginScrollView(scrollPointer);
            {

                foreach (GroupCenter center in centers2Display)
                {
                    GUI.enabled = (selectedCenter != center);
                    if (GUILayout.Button(center.CelestialBody.name + ":" + center.Group, GUILayout.Height(23)))
                    {
                        selectedCenter = center;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndScrollView();
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OK", GUILayout.Height(23)))
                {
                    callBack.Invoke(selectedCenter);
                    this.Close();
                }
                if (GUILayout.Button("Cancel", GUILayout.Height(23)))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }




        private void SetGroupList()
        {
            centers2Display.Clear();
            foreach (GroupCenter center in StaticDatabase.allCenters.Values)
            {
                if (showOnlyLocal)
                {

                    if (center.CelestialBody != FlightGlobals.currentMainBody)
                    {
                        continue;
                    }
                    if (Vector3.Distance(center.gameObject.transform.position, FlightGlobals.ActiveVessel.transform.position ) > 25000)
                    {
                        continue;
                    }
                }

                centers2Display.Add(center);
            }

        }


    }
}

using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Internal shit for testing and as a copy/paste Template
/// </summary>

namespace KerbalKonstructs.UI2
{
    internal class KKWindow2
    {

        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "KKWindow";
        internal static string windowMessage = null;
        internal static string windowTitle = "Kerbal Konstructs";

        internal static Rect windowRect;

        internal static float windowWidth = 200f;
        internal static float windowHeight = 300f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = false;
        internal static bool isModal = true;


        internal static bool setToParent = false;

        internal static bool checkForParent = true;
        internal static Func<bool> parentWindow = EditorModeSelector.IsOpen;


        internal static void CreateContent()
        {
            //content.Add(new DialogGUILabel("Hallo", HighLogic.UISkin.label));
            //content.Add(VaiantList);
        }


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



        internal static DialogGUIScrollList VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                foreach (var launchSite in LaunchSiteManager.allLaunchSites)
                {
                    list.Add(new DialogGUIHorizontalLayout(

                        new DialogGUIButton(launchSite.LaunchSiteName, delegate { SetVariant(launchSite.LaunchSiteName); }, 140.0f, 23.0f, true))
                        );
                }
                list.Add(new DialogGUIFlexibleSpace());
                list.Add(new DialogGUIButton("Close", () => { }, true));
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(200, 300), new Vector2(200, 23f * list.Count), false, true, layout);
                return scroll;

            }
        }


        internal static void CreateMultiOptionDialog()
        {
            windowRect = new Rect(WindowManager2.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, null, windowRect, content.ToArray());
            optionDialog.OnFixedUpdate += PlaceToParent;
        }


        internal static void CreatePopUp()
        {
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), optionDialog,
                   false,
                   null, isModal);
            if (!showTitle)
            {
                dialog.gameObject.GetChild("Title").SetActive(false);
            }
            if (checkForParent)
            {
                dialog.dialogToDisplay.OnUpdate += CheckForParent;
            }
        }

        internal static void PlaceToParent()
        {
            Log.Normal(" " + dialog.dialogToDisplay.position.ToString());
        }

        internal static void CheckForParent()
        {
            if (checkForParent)
            {
                if (parentWindow != null && !parentWindow.Invoke())
                {
                    Close();
                }
            }
        }

        //internal static Vector2 CreateRectUnderMouse()
        //{

        //    Vector2 mousePos = (Vector2)Input.mousePosition;

        //    Log.Normal("Parentwindow Position:" + EditorGUI.toolRect.position);


        //    Vector2 pos = new Vector2();

        //    pos﻿.x = (mousePos.x - (windowWidth / 2) - EditorGUI.windowWidth / 2) / Screen.width;
        //    pos.y = (mousePos.y / Screen.height);

        //    return (pos);
        //}

        //internal static Vector2 CreateBesidesMainwindow()
        //{

        //    Vector2 editorWindowPos = EditorGUI.toolRect.position;
        //    Vector2 mousePos = (Vector2)Input.mousePosition;

        //    //Log.Normal("Parentwindow Position:" + EditorGUI.toolRect.position);


        //    Vector2 pos = new Vector2();
        //    if (editorWindowPos.x > (Screen.width / 2))
        //    {
        //        pos﻿.x = (editorWindowPos.x - (windowWidth / 2)) / Screen.width;
        //    }
        //    else
        //    {
        //        pos﻿.x = (editorWindowPos.x + ((windowWidth / 2) + EditorGUI.windowWidth)) / Screen.width;
        //    }

        //    //pos.y = 1 - (editorWindowPos.y / Screen.height);
        //    pos.y = (mousePos.y / Screen.height);

        //    return (pos);
        //}



        internal static void Open()
        {
            KKStyle.Init();
            //windowRect = new Rect(CreateBesidesMainwindow(), new Vector2(windowWidth, windowHeight));
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

        internal static void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }


        internal static void SetVariant(string variantName)
        {
            Log.Normal("Base Selected: " + variantName);
        }


    }
}

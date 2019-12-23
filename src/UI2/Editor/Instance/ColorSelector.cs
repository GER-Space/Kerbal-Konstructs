using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.UI2;
using KerbalKonstructs.UI;


/// <summary>
/// Internal shit for testing and as a copy/paste Template
/// </summary>

namespace KerbalKonstructs.UI2
{
    internal class ColorSelector
    {

        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "ColorSelector";
        internal static string windowMessage = null;
        internal static string windowTitle = "Color Selector";

        internal static Rect windowRect;

        internal static float windowWidth = 300f;
        internal static float windowHeight = 200f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool setToParent = false;

        internal static bool checkForParent = false;
        internal static Func<bool> parentWindow = EditorGUI.instance.IsOpen;

        internal static Color selectedColor = Color.white;

        internal static Action<Color> callBack = delegate { };

        internal static float maxValue = 2.5f;


        internal static void CreateContent()
        {
            //content.Add(new DialogGUILabel("Hallo", HighLogic.UISkin.label));
            //content.Add(VaiantList);

            content.Add(new DialogGUIVerticalLayout(
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel(" Color: R ", KKStyle.whiteLabel),
                    new DialogGUISlider(delegate { return selectedColor.r; }, 0, maxValue, false, 140, 25, delegate (float col) { selectedColor.r = col; callBack.Invoke(selectedColor); } ),
                    new DialogGUITextInput("0", false, 10, SetGrassRStr, delegate { return selectedColor.r.ToString(); }, TMPro.TMP_InputField.ContentType.DecimalNumber, 25)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel(" Color: G ", KKStyle.whiteLabel),
                    new DialogGUISlider(delegate { return selectedColor.g; }, 0, maxValue, false, 140, 25, delegate (float col) { selectedColor.g = col; callBack.Invoke(selectedColor); }),
                    new DialogGUITextInput("0", false, 10, SetGrassGStr, delegate { return selectedColor.g.ToString(); }, TMPro.TMP_InputField.ContentType.DecimalNumber, 25)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel(" Color: B ", KKStyle.whiteLabel),
                    new DialogGUISlider(delegate { return selectedColor.b; }, 0, maxValue, false, 140, 25, delegate (float col) { selectedColor.b = col; callBack.Invoke(selectedColor); }),
                    new DialogGUITextInput("0", false, 10, SetGrassBStr, delegate { return selectedColor.b.ToString(); }, TMPro.TMP_InputField.ContentType.DecimalNumber, 25)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel(" Color: A ", KKStyle.whiteLabel),
                    new DialogGUISlider(delegate { return selectedColor.a; }, 0, maxValue, false, 140, 25, delegate (float col) { selectedColor.a = col; callBack.Invoke(selectedColor); }),
                    new DialogGUITextInput("0", false, 10, SetGrassAStr, delegate { return selectedColor.a.ToString(); }, TMPro.TMP_InputField.ContentType.DecimalNumber, 25))
               ));

        }



        //red
        internal static string SetGrassRStr(string newFloat)
        {
            selectedColor.r = float.Parse(newFloat);
            callBack.Invoke(selectedColor);
            return newFloat;
        }

        //Green

        internal static string SetGrassGStr(string newFloat)
        {
            selectedColor.g = float.Parse(newFloat);
            callBack.Invoke(selectedColor);
            return newFloat;
        }

        //blue
        internal static string SetGrassBStr(string newFloat)
        {
            selectedColor.b = float.Parse(newFloat);
            callBack.Invoke(selectedColor);
            return newFloat;
        }

        //Alpha
        internal static string SetGrassAStr(string newFloat)
        {
            selectedColor.a = float.Parse(newFloat);
            callBack.Invoke(selectedColor);
            return newFloat;
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

using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace KerbalKonstructs.UI2
{
    class InstanceEditorToolbar
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "InstanceToolbar";
        internal static string windowMessage = null;
        internal static string windowTitle = "Instance Toolbar";

        internal static Rect windowRect;

        //internal static float windowWidth = Screen.width * 0.9f;
        internal static float windowWidth = 600f;
        internal static float windowHeight = 30f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool placeToParent = false;
        internal static bool checkForParent = true;

        internal static Func<bool> parentWindow = EditorModeSelector.IsOpen;


        internal static void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Hallo", HighLogic.UISkin.label),
                new DialogGUIButton("show models", delegate { ModelList.Open(); }, false),
                new DialogGUIButton("show nearby instances", null, false),
                new DialogGUIFlexibleSpace()

                ));
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





        internal static void CreateMultiOptionDialog()
        {
            windowRect = new Rect(WindowManager.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, null, windowRect, content.ToArray());

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
            if (placeToParent)
            {
                dialog.dialogToDisplay.OnUpdate += PlaceToParent;
            }

        }

        internal static void PlaceToParent()
        {

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
                WindowManager.SavePosition(dialog);
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

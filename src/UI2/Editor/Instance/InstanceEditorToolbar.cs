using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
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

        // the window is as wide as its buttons
        private const int buttonCount = 2;
        private const float buttonWidth = 150f;
        private const float buttonHeight = 25f;
        private const float buttonSpacing = 4f;
        private const float windowPadding = 28f;

        internal static float windowWidth = buttonCount * buttonWidth + (buttonCount - 1) * buttonSpacing + windowPadding;
        internal static float windowHeight = 30f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool placeToParent = false;
        internal static bool checkForParent = true;

        internal static Func<bool> parentWindow = EditorModeSelector.IsOpen;


        internal static void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(false, false, buttonSpacing, new RectOffset(), TextAnchor.MiddleCenter,
                new DialogGUIButton("Spawn New", ModelList.Toggle, buttonWidth, buttonHeight, false),
                new DialogGUIButton("Local Instances", InstanceList.Toggle, buttonWidth, buttonHeight, false)
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
            windowRect = new Rect(WindowManager2.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, null, windowRect, content.ToArray());

        }


        internal static void CreatePopUp()
        {
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), optionDialog,
                   false,
                   null, isModal);
            // allow dragging the window up to the screen border
            KKWindowDrag.Attach(dialog);
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

            // the local instance list belongs to the instance editor (like the old default mode)
            ConfigUtil.CreateNewInstanceDirIfNeeded();
            StaticsEditorGUI.ResetLocalGroupList();
            InstanceList.Open();
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

            // leaving the instance editor closes its windows
            InstanceList.Close();
            ModelList.Close();
            EditorGUI.instance.Close();
            KerbalKonstructs.instance.DeletePreviewObject();
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

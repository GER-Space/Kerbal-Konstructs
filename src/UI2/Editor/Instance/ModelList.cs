using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace KerbalKonstructs.UI2
{
    class ModelList
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "ModelList";
        internal static string windowMessage = null;
        internal static string windowTitle = "Model List";

        internal static Rect windowRect;

        //internal static float windowWidth = Screen.width * 0.9f;
        internal static float windowWidth = 300f;
        internal static float windowHeight = 600f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool placeToParent = false;
        internal static bool checkForParent = true;

        internal static Func<bool> parentWindow = InstanceEditorToolbar.IsOpen;


        internal static void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("All Models", HighLogic.UISkin.label),
                new DialogGUIFlexibleSpace()
                ));
            content.Add(VaiantList);
        }



        internal static DialogGUIScrollList VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                foreach (var model in StaticDatabase.allStaticModels)
                {
                    list.Add(new DialogGUIButton(model.name, delegate { SpawnModel(model); }, 140.0f, 25.0f, false));
                }
                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(200, 300), new Vector2(200, 23f * list.Count), false, true, layout);
                return scroll;
            }
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


        internal static void SpawnModel(StaticModel model)
        {
            Log.Normal("Model selected: " + model.name);
        }

    }
}

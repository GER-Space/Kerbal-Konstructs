using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.Addons;

namespace KerbalKonstructs.UI2
{
    class EditorModeSelector
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "ModeSelector";
        internal static string windowMessage = null;
        internal static string windowTitle = "Editor Mode";

        internal static Rect windowRect;

        internal static float windowWidth = 200f;
        internal static float windowHeight = 50f;

        internal static bool showTitle = false;



        internal static void CreateContent()
        {
            content.Add(new DialogGUILabel("Select a Editor mode"));

            content.Add(new DialogGUIButton("GetSize", GetSize, false));

            content.Add(VaiantList);

        }


        internal static DialogGUIScrollList VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                //list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                foreach (KKEditorLogic.MainEditorMode mode in Enum.GetValues(typeof(KKEditorLogic.MainEditorMode)))
                {
                    list.Add(new DialogGUIHorizontalLayout(

                        new DialogGUIButton(mode.ToString(), delegate { KKEditorLogic.SetMainMode(mode); }, () => CheckMode(mode), windowWidth - 20, 25.0f, false)

                        ));
                }

                var layout = new DialogGUIVerticalLayout(windowWidth - 12, 10, 4, new RectOffset(6, 6, 6, 6), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(windowWidth, 23f * list.Count), new Vector2(windowWidth, 23f * list.Count), false, false, layout);
                return scroll;

            }
        }


        internal static bool CheckMode(KKEditorLogic.MainEditorMode mode)
        {
            return (KKEditorLogic.CurrentMode != mode);
        }


        internal static void KKTitle()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),

                new DialogGUILabel(windowTitle, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)

                ));
        }


        internal static void GetSize()
        {
            Log.Normal("");
            Log.Normal("window heigt: " + dialog.dialogToDisplay.dialogRect.height);
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
                   null, false);
            if (!showTitle)
            {
                dialog.gameObject.GetChild("Title").SetActive(false);
            }

            //if (SDTest.pickerPrefab == null)
            //    return;

            //GameObject obj = GameObject.Instantiate(SDTest.pickerPrefab, Vector3.zero , Quaternion.identity);

            //if (obj == null)
            //    return;

            //obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform);

            //obj.transform.SetParent(KSP.UI.UIMasterController.Instance.dialogCanvas.transform, false);

            //obj.transform.SetAsFirstSibling();
            ///obj.SetActive(true);
        }

        internal static void Open()
        {
            //windowRect = new Rect(CreateBesidesMainwindow(), new Vector2(windowWidth, windowHeight));
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
            KKEditorLogic.OpenCurrentToolbar();

        }


        internal static void Close()
        {
            if (dialog != null)
            {
                WindowManager.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;

            WindowManager.SavePresets();
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

    }
}

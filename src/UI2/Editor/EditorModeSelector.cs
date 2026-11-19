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

        internal static float windowWidth = 245f;
        internal static float windowHeight = 50f;

        internal static bool showTitle = false;

        private const float buttonSize = 48f;

        private static Dictionary<KKEditorLogic.MainEditorMode, UIStyle> modeStyles = null;
        private static UIStyle saveStyle = null;


        /// <summary>
        /// Icon file (in KerbalKonstructs/Assets) for each editor mode
        /// </summary>
        private static string GetIconName(KKEditorLogic.MainEditorMode mode)
        {
            switch (mode)
            {
                case KKEditorLogic.MainEditorMode.Instance:
                    return "KerbalKonstructs/Assets/editorModeInstance";
                case KKEditorLogic.MainEditorMode.Group:
                    return "KerbalKonstructs/Assets/editorModeGroup";
                case KKEditorLogic.MainEditorMode.Terrain:
                    return "KerbalKonstructs/Assets/editorModeTerrain";
                default:
                    return null;
            }
        }


        /// <summary>
        /// Mouse-over text for each editor mode
        /// </summary>
        private static string GetTooltip(KKEditorLogic.MainEditorMode mode)
        {
            switch (mode)
            {
                case KKEditorLogic.MainEditorMode.Instance:
                    return "Instance Editor: place, move and edit single statics";
                case KKEditorLogic.MainEditorMode.Group:
                    return "Group Editor: create, move and edit groups of statics";
                case KKEditorLogic.MainEditorMode.Terrain:
                    return "Terrain Editor: edit map decals and the terrain";
                default:
                    return mode.ToString();
            }
        }


        /// <summary>
        /// Builds one button style per icon. The icon images contain the whole button
        /// (plate + glyph) for each state, so the default button skin can't replace them on hover.
        /// </summary>
        private static void LoadIcons()
        {
            if (modeStyles != null)
            {
                return;
            }
            modeStyles = new Dictionary<KKEditorLogic.MainEditorMode, UIStyle>();
            foreach (KKEditorLogic.MainEditorMode mode in Enum.GetValues(typeof(KKEditorLogic.MainEditorMode)))
            {
                string iconName = GetIconName(mode);
                UIStyle style = (iconName == null) ? null : KKStyle.CreateIconButtonStyle(iconName);
                if (style != null)
                {
                    modeStyles.Add(mode, style);
                }
            }
            saveStyle = KKStyle.CreateIconButtonStyle("KerbalKonstructs/Assets/editorModeSave");
        }


        internal static void CreateContent()
        {
            LoadIcons();

            List<DialogGUIBase> buttons = new List<DialogGUIBase>();
            buttons.Add(new DialogGUIFlexibleSpace());

            foreach (KKEditorLogic.MainEditorMode mode in Enum.GetValues(typeof(KKEditorLogic.MainEditorMode)))
            {
                // local copy for the delegates
                KKEditorLogic.MainEditorMode thisMode = mode;
                DialogGUIButton button;

                if (modeStyles.ContainsKey(thisMode))
                {
                    button = new DialogGUIButton("", delegate { KKEditorLogic.SetMainMode(thisMode); }, buttonSize, buttonSize, false, modeStyles[thisMode]);
                }
                else
                {
                    // fallback if the icon is missing
                    button = new DialogGUIButton(thisMode.ToString(), delegate { KKEditorLogic.SetMainMode(thisMode); }, buttonSize, buttonSize, false);
                }

                // the button of the active mode is disabled
                button.OptionInteractableCondition = () => CheckMode(thisMode);
                button.tooltipText = GetTooltip(thisMode);

                buttons.Add(button);
            }

            // "save all" button
            DialogGUIButton saveButton;
            if (saveStyle != null)
            {
                saveButton = new DialogGUIButton("", SaveAll, buttonSize, buttonSize, false, saveStyle);
            }
            else
            {
                saveButton = new DialogGUIButton("Save", SaveAll, buttonSize, buttonSize, false);
            }
            saveButton.tooltipText = "Save all: save all statics and groups to disk";
            buttons.Add(saveButton);

            buttons.Add(new DialogGUIFlexibleSpace());
            content.Add(new DialogGUIHorizontalLayout(buttons.ToArray()));
        }


        internal static void SaveAll()
        {
            KerbalKonstructs.instance.SaveObjects();
            MiscUtils.HUDMessage("All statics and groups saved", 5, 0);
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
                   null, false);
            if (!showTitle)
            {
                dialog.gameObject.GetChild("Title").SetActive(false);
            }

            // tooltipText alone doesn't show anything; attach real tooltips to the created buttons
            KKStyle.AttachTooltips(content);

            //if (SDTest.pickerPrefab == null)
            //    return;

            //GameObject obj = GameObject.Instantiate(SDTest.pickerPrefab, Vector3.zero , Quaternion.identity);

            //if (obj == null)
            //    return;

            //obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform);

            //obj.transform.SetParent(KSP.UI.UIMasterController.Instance.dialogCanvas.transform, false);

            //obj.transform.SetAsFirstSibling();
            //obj.SetActive(true);
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
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;

            WindowManager2.SavePresets();
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

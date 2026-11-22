using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// List of the statics near the active vessel (UI2 / PopupDialog version).
    /// Replaces the "Local Instances" (LOCAL) mode of the old StaticsEditorGUI.
    /// Opens with the Instance editor and closes when leaving it.
    /// </summary>
    internal class InstanceList
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "InstanceList";
        internal static string windowMessage = null;
        internal static string windowTitle = "Local Instances";

        internal static Rect windowRect;

        internal static float windowWidth = 480f;
        internal static float windowHeight = 100f;

        private const float listHeight = 400f;
        private const float itemHeight = 23f;
        private const float groupWidth = 120f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.15f, 0.55f);

        internal static bool checkForParent = true;
        internal static Func<bool> parentWindow = InstanceEditorToolbar.IsOpen;

        // the list is checked for changes every few seconds (statics move in and out of range)
        private const float refreshInterval = 2f;
        private static float lastRefresh = 0f;
        private static string lastListSignature = null;


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


        internal static void CreateContent()
        {
            content.Add(InstancesList());
        }


        /// <summary>
        /// One row per static near the vessel: group, launch site icon, title (selects it for editing)
        /// and a button to make it the snap target.
        /// (was StaticsEditorGUI.ShowInstancesScroll)
        /// </summary>
        internal static DialogGUIScrollList InstancesList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<StaticInstance> instances = GetLocalInstances();
            foreach (StaticInstance staticInstance in instances)
            {
                // local copy for the delegates
                StaticInstance instance = staticInstance;

                List<DialogGUIBase> row = new List<DialogGUIBase>();
                row.Add(new DialogGUILabel(instance.Group, groupWidth, itemHeight));

                Texture siteIcon = instance.hasLauchSites ? GetLaunchSiteIcon(instance.launchSite) : null;
                if (siteIcon != null)
                {
                    row.Add(new DialogGUIImage(new Vector2(itemHeight, itemHeight), Vector2.zero, Color.white, siteIcon));
                }
                else
                {
                    row.Add(new DialogGUISpace(itemHeight));
                }

                DialogGUIButton selectButton = new DialogGUIButton(instance.model.title + " ( " + instance.indexInGroup.ToString() + " )",
                    delegate { SelectInstance(instance); }, windowWidth - groupWidth - 110f, itemHeight, false);
                selectButton.tooltipText = "Edit this instance";
                row.Add(selectButton);

                DialogGUIButton snapButton = new DialogGUIButton("Snap", delegate { SetSnapTarget(instance); },
                    delegate { return StaticsEditorGUI.instance.snapTargetInstance != instance && instance != StaticsEditorGUI.selectedObject; },
                    45f, itemHeight, false);
                snapButton.tooltipText = "Set as snap target";
                row.Add(snapButton);

                list.Add(new DialogGUIHorizontalLayout(row.ToArray()));
            }

            if (instances.Count == 0)
            {
                list.Add(new DialogGUILabel("No statics nearby", KKStyle.whiteLabel));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, listHeight), false, true, layout);
        }


        /// <summary>
        /// The statics on the vessel's body within KerbalKonstructs.localGroupRange of it.
        /// </summary>
        private static List<StaticInstance> GetLocalInstances()
        {
            List<StaticInstance> instances = new List<StaticInstance>();
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                return instances;
            }
            Vector3 vesselPosition = vessel.GetTransform().position;
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance == null || instance.gameObject == null || instance.CelestialBody != FlightGlobals.currentMainBody)
                {
                    continue;
                }
                if (Vector3.Distance(vesselPosition, instance.position) < KerbalKonstructs.localGroupRange)
                {
                    instances.Add(instance);
                }
            }
            return instances;
        }


        /// <summary>
        /// (was LaunchSiteSelectorGUI.ShowCategory)
        /// </summary>
        private static Texture GetLaunchSiteIcon(KKLaunchSite site)
        {
            if (site == null)
            {
                return null;
            }
            switch (site.sitecategory)
            {
                case LaunchSiteCategory.Runway:
                    return UIMain.runWayIcon;
                case LaunchSiteCategory.Helipad:
                    return UIMain.heliPadIcon;
                case LaunchSiteCategory.RocketPad:
                    return UIMain.VABIcon;
                case LaunchSiteCategory.Waterlaunch:
                    return UIMain.waterLaunchIcon;
                case LaunchSiteCategory.Other:
                    return UIMain.ANYIcon;
                default:
                    return null;
            }
        }


        #region actions

        internal static void SelectInstance(StaticInstance instance)
        {
            StaticsEditorGUI editor = StaticsEditorGUI.instance;

            EditorGUI.CloseEditors();
            if (!EditorGUI.instance.IsOpen())
            {
                EditorGUI.instance.Open();
            }

            if (StaticsEditorGUI.selectedObject != null)
            {
                editor.selectedObjectPrevious = StaticsEditorGUI.selectedObject;
                StaticsEditorGUI.selectedObject.HighlightObject(Color.clear);
            }

            if (editor.snapTargetInstance == instance)
            {
                editor.snapTargetInstance = null;
            }
            KerbalKonstructs.SelectInstance(instance, false);

            instance.HighlightObject(XKCDColors.Green_Yellow);
        }


        internal static void SetSnapTarget(StaticInstance instance)
        {
            StaticsEditorGUI editor = StaticsEditorGUI.instance;

            if (editor.snapTargetInstance != null)
            {
                editor.snapTargetInstance.HighlightObject(new Color(0, 0, 0, 0));
            }
            editor.snapTargetInstance = instance;
            instance.HighlightObject(XKCDColors.RedPink);
        }

        #endregion


        private static string GetListSignature()
        {
            System.Text.StringBuilder signature = new System.Text.StringBuilder();
            foreach (StaticInstance instance in GetLocalInstances())
            {
                signature.Append(instance.UUID).Append(':').Append(instance.Group).Append(':').Append(instance.indexInGroup).Append('|');
            }
            return signature.ToString();
        }


        private static void OnUpdate()
        {
            if (checkForParent && parentWindow != null && !parentWindow.Invoke())
            {
                Close();
                return;
            }

            // statics are added, deleted or come into range only now and then: check every few seconds
            if (Time.time - lastRefresh < refreshInterval)
            {
                return;
            }
            lastRefresh = Time.time;
            if (GetListSignature() != lastListSignature)
            {
                Reload();
            }
        }


        internal static void CreateMultiOptionDialog()
        {
            windowRect = new Rect(WindowManager2.GetPosition(windowName, defaultPosition), new Vector2(windowWidth, windowHeight));
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
            dialog.dialogToDisplay.OnUpdate += OnUpdate;
            KKStyle.AttachTooltips(content);
        }


        internal static void Open()
        {
            if (dialog != null)
            {
                return;
            }
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
            lastListSignature = GetListSignature();
            lastRefresh = Time.time;
        }


        internal static void Reload()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
            Open();
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
            lastListSignature = null;
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

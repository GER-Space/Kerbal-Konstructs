using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// List of the groups near the active vessel (UI2 / PopupDialog version).
    /// Replaces the "Edit Groups" (GROUP) mode of the old StaticsEditorGUI.
    /// Opened and closed together with the Group editor (GroupEditorToolbar).
    /// </summary>
    internal class GroupList
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "GroupList";
        internal static string windowMessage = null;
        internal static string windowTitle = "Groups";

        internal static Rect windowRect;

        internal static float windowWidth = 360f;
        internal static float windowHeight = 100f;

        private const float listHeight = 300f;
        private const float itemHeight = 23f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.15f, 0.55f);

        internal static bool checkForParent = true;
        internal static Func<bool> parentWindow = EditorModeSelector.IsOpen;

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
            content.Add(GroupsList());

            GroupCenter activeGroup = StaticsEditorGUI.GetActiveGroup();
            content.Add(new DialogGUILabel("Active Group: " + ((activeGroup != null) ? activeGroup.Group : "not set"), KKStyle.whiteLabel));

            DialogGUIButton spawnButton = new DialogGUIButton("Spawn new Group", SpawnNewGroup, 110f, 25f, false);
            spawnButton.tooltipText = "Create a new group at the position of the vessel";

            DialogGUIButton setActiveButton = new DialogGUIButton("Set Active Group", SelectActiveGroup, 110f, 25f, false);
            setActiveButton.tooltipText = "Choose the group that new statics are added to";

            DialogGUIButton cloneButton = new DialogGUIButton("Clone to Active", CloneToActiveGroup, CanCloneToActiveGroup, 110f, 25f, false);
            cloneButton.tooltipText = "Copy the statics of another group into the active group";

            content.Add(new DialogGUIHorizontalLayout(
                spawnButton,
                setActiveButton,
                cloneButton
                ));
        }


        /// <summary>
        /// One button per (not built-in) group near the vessel; clicking it opens the Group editor for that group.
        /// (was StaticsEditorGUI.ShowGroupScroll)
        /// </summary>
        internal static DialogGUIScrollList GroupsList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<GroupCenter> groups = GetEditableGroups();
            foreach (GroupCenter groupCenter in groups)
            {
                // local copy for the delegates
                GroupCenter group = groupCenter;
                DialogGUIButton button = new DialogGUIButton(" " + group.Group, delegate { EditGroup(group); },
                    delegate { return GroupEditor.selectedGroup != group || !GroupEditor.instance.IsOpen(); },
                    windowWidth - 40f, itemHeight, false);
                button.tooltipText = "Edit this group";
                list.Add(button);
            }

            if (groups.Count == 0)
            {
                list.Add(new DialogGUILabel("No groups nearby", KKStyle.whiteLabel));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, listHeight), false, true, layout);
        }


        /// <summary>
        /// The local groups without the built-in ones, sorted by name
        /// </summary>
        private static List<GroupCenter> GetEditableGroups()
        {
            List<GroupCenter> groups = new List<GroupCenter>();
            GroupCenter[] localGroups = StaticsEditorGUI.LocalGroups;
            if (localGroups == null)
            {
                return groups;
            }
            foreach (GroupCenter groupCenter in localGroups)
            {
                if (groupCenter == null || groupCenter.isBuiltIn)
                {
                    continue;
                }
                groups.Add(groupCenter);
            }
            groups.Sort((a, b) => string.Compare(a.Group, b.Group, StringComparison.OrdinalIgnoreCase));
            return groups;
        }


        internal static void EditGroup(GroupCenter group)
        {
            EditorGUI.CloseEditors();
            MapDecalEditor.Instance.Close();
            GroupEditor.instance.Close();
            GroupEditor.selectedGroup = group;
            GroupEditor.maxEditorRange = 0;
            GroupEditor.instance.Open();
        }


        /// <summary>
        /// (was the "Spawn new Group" button of StaticsEditorGUI.ShowGroupFooter)
        /// </summary>
        internal static void SpawnNewGroup()
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }
            EditorGUI.instance.Close();
            MapDecalEditor.Instance.Close();
            EditorGUI.selectedInstance = null;

            GroupCenter groupCenter = new GroupCenter
            {
                RadialPosition = FlightGlobals.currentMainBody.transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position),
                Group = "NewGroup",
                CelestialBody = FlightGlobals.currentMainBody
            };
            groupCenter.Spawn();

            GroupEditor.selectedGroup = groupCenter;

            if (GroupEditor.selectedGroup == null)
            {
                Log.UserError("No Group created");
            }
            else
            {
                GroupEditor.maxEditorRange = 0;
                GroupEditor.instance.Open();
                Log.Normal("Group Editor spawned");
            }
            StaticsEditorGUI.ResetLocalGroupList();
            // the list is rebuilt by OnUpdate, because the group list changed
        }


        internal static void SelectActiveGroup()
        {
            GroupSelectorUI.instance.Close();
            GroupSelectorUI.showOnlyLocal = true;
            GroupSelectorUI.callBack = StaticsEditorGUI.SetActiveGroup;
            GroupSelectorUI.titleText = "Select active Group";
            GroupSelectorUI.instance.Open();
        }


        internal static bool CanCloneToActiveGroup()
        {
            GroupCenter activeGroup = StaticsEditorGUI.GetActiveGroup();
            if (activeGroup == null || activeGroup.gameObject == null || FlightGlobals.ActiveVessel == null)
            {
                return false;
            }
            return Vector3.Distance(activeGroup.gameObject.transform.position, FlightGlobals.ActiveVessel.transform.position) < KerbalKonstructs.localGroupRange;
        }


        internal static void CloneToActiveGroup()
        {
            if (!CanCloneToActiveGroup())
            {
                return;
            }
            GroupSelectorUI.instance.Close();
            GroupSelectorUI.showOnlyLocal = false;
            GroupSelectorUI.titleText = "Select Group to Clone";
            GroupSelectorUI.callBack = StaticsEditorGUI.GetActiveGroup().CopyGroup;
            GroupSelectorUI.instance.Open();
        }


        /// <summary>
        /// The window is rebuilt when groups are added, removed or renamed, or the active group changes.
        /// </summary>
        private static string GetListSignature()
        {
            System.Text.StringBuilder signature = new System.Text.StringBuilder();
            foreach (GroupCenter group in GetEditableGroups())
            {
                signature.Append(group.Group).Append('|');
            }
            GroupCenter activeGroup = StaticsEditorGUI.GetActiveGroup();
            signature.Append("active:").Append((activeGroup != null) ? activeGroup.Group : "");
            return signature.ToString();
        }


        private static void OnUpdate()
        {
            if (checkForParent && parentWindow != null && !parentWindow.Invoke())
            {
                Close();
                return;
            }

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
            // find the groups near the vessel, like the old window did when it was opened
            if (lastListSignature == null)
            {
                StaticsEditorGUI.ResetLocalGroupList();
            }
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
            lastListSignature = GetListSignature();
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

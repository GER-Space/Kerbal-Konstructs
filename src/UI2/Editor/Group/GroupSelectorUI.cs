using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Lets the player choose a group and passes it to callBack (UI2 / PopupDialog version;
    /// was an IMGUI KKWindow). Keeps the old API: set showOnlyLocal, titleText and callBack,
    /// then call GroupSelectorUI.instance.Open().
    /// </summary>
    public class GroupSelectorUI
    {
        private static GroupSelectorUI _instance = null;

        private PopupDialog dialog;
        private List<DialogGUIBase> content;

        private const string windowName = "GroupSelector";
        private const float windowWidth = 320f;
        private const float windowHeight = 100f;
        private const float listHeight = 280f;
        private const float itemHeight = 23f;
        private static readonly Vector2 defaultPosition = new Vector2(0.5f, 0.5f);

        private List<GroupCenter> centers2Display = new List<GroupCenter>();
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


        public void Open()
        {
            if (dialog != null)
            {
                return;
            }
            SetGroupList();
            selectedCenter = null;
            Build();
        }


        public void Close()
        {
            DismissDialog();
            callBack = delegate
            {
            };
            selectedCenter = null;
            titleText = "unset";
        }


        public bool IsOpen()
        {
            return (dialog != null);
        }


        private void Build()
        {
            KKStyle.Init();
            content = new List<DialogGUIBase>();

            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel(titleText, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)
                ));

            content.Add(GroupList());

            // OK only when a group is chosen (the old window passed null, which crashed the callers)
            DialogGUIButton okButton = new DialogGUIButton("OK", Confirm, delegate { return selectedCenter != null; }, 140f, itemHeight, false);
            content.Add(new DialogGUIHorizontalLayout(
                okButton,
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("Cancel", Close, 140f, itemHeight, false)
                ));

            Rect windowRect = new Rect(WindowManager2.GetPosition(windowName, defaultPosition), new Vector2(windowWidth, windowHeight));
            MultiOptionDialog optionDialog = new MultiOptionDialog(windowName, null, titleText, null, windowRect, content.ToArray());
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), optionDialog, false, null, false);
            // allow dragging the window up to the screen border
            KKWindowDrag.Attach(dialog);
            dialog.gameObject.GetChild("Title").SetActive(false);
            dialog.dialogToDisplay.OnUpdate += OnUpdate;
            KKStyle.AttachTooltips(content);
        }


        /// <summary>
        /// One button per group; the chosen one is disabled. Sorted by body and name.
        /// </summary>
        private DialogGUIScrollList GroupList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            foreach (GroupCenter groupCenter in centers2Display)
            {
                GroupCenter center = groupCenter;
                list.Add(new DialogGUIButton(center.CelestialBody.name + ": " + center.Group, delegate { selectedCenter = center; },
                    delegate { return selectedCenter != center; }, windowWidth - 50f, itemHeight, false));
            }

            if (centers2Display.Count == 0)
            {
                list.Add(new DialogGUILabel(showOnlyLocal ? "No groups nearby" : "No groups found", KKStyle.whiteLabel));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, listHeight), false, true, layout);
        }


        private void Confirm()
        {
            if (selectedCenter == null)
            {
                return;
            }
            // Close() resets callBack, so keep it
            Action<GroupCenter> action = callBack;
            GroupCenter chosen = selectedCenter;
            Close();
            action.Invoke(chosen);
        }


        private void OnUpdate()
        {
            if (!KKEditorLogic.IsEditorOpen())
            {
                Close();
            }
        }


        private void DismissDialog()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
        }


        private void SetGroupList()
        {
            centers2Display.Clear();
            Vessel vessel = FlightGlobals.ActiveVessel;
            foreach (GroupCenter center in StaticDatabase.allGroupCenters)
            {
                if (center == null || center.gameObject == null)
                {
                    continue;
                }
                if (showOnlyLocal)
                {
                    if (center.CelestialBody != FlightGlobals.currentMainBody || vessel == null)
                    {
                        continue;
                    }
                    if (Vector3.Distance(center.gameObject.transform.position, vessel.transform.position) > 25000)
                    {
                        continue;
                    }
                }

                centers2Display.Add(center);
            }
            centers2Display = centers2Display
                .OrderBy(center => center.CelestialBody.name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(center => center.Group, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }


    }
}

using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// List of all map decals (UI2 / PopupDialog version).
    /// Replaces the "Edit MapDecals" (PQS) mode of the old StaticsEditorGUI.
    /// Opened and closed together with the Terrain editor (TerrainEditorToolbar).
    /// </summary>
    internal class MapDecalList
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "MapDecalList";
        internal static string windowMessage = null;
        internal static string windowTitle = "Map Decals";

        internal static Rect windowRect;

        internal static float windowWidth = 300f;
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
            content.Add(DecalsList());

            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("Spawn new MapDecal", SpawnNewDecal, 170f, 25f, false),
                new DialogGUIFlexibleSpace()
                ));
        }


        /// <summary>
        /// One button per map decal; clicking it opens the MapDecal editor for that decal.
        /// (was StaticsEditorGUI.ShowDecalsScroll)
        /// </summary>
        internal static DialogGUIScrollList DecalsList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            foreach (MapDecalInstance mapDecalInstance in GetSortedDecals())
            {
                // local copy for the delegates
                MapDecalInstance decal = mapDecalInstance;
                DialogGUIButton button = new DialogGUIButton(" " + decal.Name + " (" + decal.Group + ")", delegate { EditDecal(decal); },
                    delegate { return IsInRange(decal) && (MapDecalEditor.selectedDecal != decal || !MapDecalEditor.Instance.IsOpen()); },
                    windowWidth - 40f, itemHeight, false);
                button.tooltipText = "Edit this map decal (only within " + (maxEditDistance / 1000f).ToString("0") + " km of the vessel)";
                list.Add(button);
            }

            if (DecalsDatabase.allMapDecalInstances.Length == 0)
            {
                list.Add(new DialogGUILabel("No map decals found", KKStyle.whiteLabel));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, listHeight), false, true, layout);
        }


        // decals further away from the active vessel than this (in m) can't be edited
        private const float maxEditDistance = 30000f;


        /// <summary>
        /// True if the decal is on the active vessel's body and within maxEditDistance of it.
        /// </summary>
        internal static bool IsInRange(MapDecalInstance decal)
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null || decal == null || decal.gameObject == null)
            {
                return false;
            }
            if (decal.CelestialBody != null && decal.CelestialBody != vessel.mainBody)
            {
                return false;
            }
            // the decal's real world position (MapDecalInstance.position is never set)
            Vector3 decalPosition = decal.gameObject.transform.position;
            return Vector3.Distance(decalPosition, vessel.transform.position) <= maxEditDistance;
        }


        /// <summary>
        /// Decals that can be edited (in range) first, then the ones out of range; each part sorted by name.
        /// </summary>
        private static List<MapDecalInstance> GetSortedDecals()
        {
            List<MapDecalInstance> inRange = new List<MapDecalInstance>();
            List<MapDecalInstance> outOfRange = new List<MapDecalInstance>();
            foreach (MapDecalInstance decal in DecalsDatabase.allMapDecalInstances)
            {
                if (IsInRange(decal))
                {
                    inRange.Add(decal);
                }
                else
                {
                    outOfRange.Add(decal);
                }
            }
            Comparison<MapDecalInstance> byName = (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            inRange.Sort(byName);
            outOfRange.Sort(byName);
            inRange.AddRange(outOfRange);
            return inRange;
        }


        internal static void EditDecal(MapDecalInstance decal)
        {
            EditorGUI.CloseEditors();
            MapDecalEditor.Instance.Close();
            MapDecalEditor.selectedDecal = decal;
            MapDecalEditor.Instance.Open();
        }


        /// <summary>
        /// (was StaticsEditorGUI.ShowDecalsFooter)
        /// </summary>
        internal static void SpawnNewDecal()
        {
            EditorGUI.instance.Close();
            EditorGUI.selectedInstance = null;

            MapDecalEditor.selectedDecal = MapDecalUtils.SpawnNewDecalInstance();

            if (MapDecalEditor.selectedDecal == null)
            {
                Log.UserError("No MapDecalInstance created");
                return;
            }

            MapDecalEditor.Instance.Open();
            Log.Normal("MapDecal Editor spawned");
            // the list is rebuilt by OnUpdate, because the decal list changed
        }


        /// <summary>
        /// The list is rebuilt when decals are added, removed or renamed.
        /// </summary>
        private static string GetListSignature()
        {
            System.Text.StringBuilder signature = new System.Text.StringBuilder();
            foreach (MapDecalInstance decal in DecalsDatabase.allMapDecalInstances)
            {
                // the range is part of it, so the list is re-sorted when a decal comes into or goes out of range
                signature.Append(decal.Name).Append('(').Append(decal.Group).Append(')').Append(IsInRange(decal) ? '+' : '-').Append('|');
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

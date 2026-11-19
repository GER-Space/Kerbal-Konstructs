using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Base Manager window (UI2 / PopupDialog version).
    /// Shows information about the selected launch site and lets the player open/close it,
    /// mark it as favourite, set it as launch site (editor) and create a waypoint (flight / map).
    /// The window is rebuilt whenever something it displays changes (see GetStateSignature).
    /// </summary>
    internal class BaseManager
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "BaseManager";
        internal static string windowMessage = null;
        internal static string windowTitle = "Base Manager";

        internal static Rect windowRect;

        internal static float windowWidth = 210f;
        // minimum height, the dialog grows to fit its content (folded / unfolded)
        internal static float windowHeight = 100f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.2f, 0.6f);

        internal static KKLaunchSite selectedSite = null;

        private static bool displayStats = true;
        private static bool foldedIn = false;

        private static string lastStateSignature = null;

        private const string editorLockName = "KKBaseManagerLock";

        private static bool texturesLoaded = false;
        private static Sprite sMakeFavourite;
        private static Sprite sIsFavourite;
        private static Sprite sFoldIn;
        private static Sprite sFoldOut;
        private static Texture tSetLaunchsite;
        private static Texture tOpenedLaunchsite;
        private static Texture tClosedLaunchsite;


        private static void LoadTextures()
        {
            if (texturesLoaded)
            {
                return;
            }
            sMakeFavourite = ToSprite(GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/makefavourite", false));
            sIsFavourite = ToSprite(GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/isFavourite", false));
            // the old window used "foldin" as icon for "fold out" and vice versa, keep that
            sFoldIn = ToSprite(GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false));
            sFoldOut = ToSprite(GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false));
            tSetLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/setaslaunchsite", false);
            tOpenedLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/openedlaunchsite", false);
            tClosedLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/closedlaunchsite", false);
            texturesLoaded = true;
        }


        private static Sprite ToSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }


        private static string DisplayName(KKLaunchSite site)
        {
            if (site.LaunchSiteName == "Runway")
            {
                return "KSC Runway";
            }
            if (site.LaunchSiteName == "LaunchPad")
            {
                return "KSC LaunchPad";
            }
            return site.LaunchSiteName;
        }


        private static string LimitText(float value, string unit)
        {
            return (value == 0f) ? "unlimited" : value.ToString("#0") + " " + unit;
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


        internal static void CreateContent()
        {
            KKLaunchSite site = selectedSite;

            content.Add(new DialogGUILabel(DisplayName(site), KKStyle.goldLabel));

            if (!foldedIn)
            {
                if (site.logo != null)
                {
                    content.Add(new DialogGUIHorizontalLayout(
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIImage(new Vector2(135f, 135f), Vector2.zero, Color.white, site.logo),
                        new DialogGUIFlexibleSpace()
                        ));
                }
                content.Add(DescriptionList(site));
            }

            // Stats / Log / Favourite / Fold
            bool isFavourite = (site.favouriteSite == "Yes");
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUIButton("Stats", delegate { displayStats = true; Reload(); }, delegate { return !displayStats; }, 60f, 23f, false),
                new DialogGUIButton("Log", delegate { displayStats = false; Reload(); }, delegate { return displayStats; }, 60f, 23f, false),
                new DialogGUIFlexibleSpace(),
                IconButton(isFavourite ? sIsFavourite : sMakeFavourite, isFavourite ? "*" : "+", ToggleFavourite),
                IconButton(foldedIn ? sFoldOut : sFoldIn, foldedIn ? "v" : "^", delegate { foldedIn = !foldedIn; Reload(); })
                ));

            if (displayStats)
            {
                content.Add(new DialogGUILabel("Altitude: " + site.refAlt.ToString("#0.0") + " m", KKStyle.whiteLabel));
                content.Add(new DialogGUILabel("Longitude: " + site.refLon.ToString("#0.000"), KKStyle.whiteLabel));
                content.Add(new DialogGUILabel("Latitude: " + site.refLat.ToString("#0.000"), KKStyle.whiteLabel));
                content.Add(new DialogGUISpace(3f));
                content.Add(new DialogGUILabel("Max Length: " + LimitText(site.LaunchSiteLength, "m"), KKStyle.whiteLabel));
                content.Add(new DialogGUILabel("Max Width: " + LimitText(site.LaunchSiteWidth, "m"), KKStyle.whiteLabel));
                content.Add(new DialogGUILabel("Max Height: " + LimitText(site.LaunchSiteHeight, "m"), KKStyle.whiteLabel));
                content.Add(new DialogGUILabel("Max Mass: " + LimitText(site.MaxCraftMass, "t"), KKStyle.whiteLabel));
                content.Add(new DialogGUILabel("Max Parts: " + ((site.MaxCraftParts == 0) ? "unlimited" : site.MaxCraftParts.ToString("#0")), KKStyle.whiteLabel));
            }
            else
            {
                content.Add(LogList(site));
            }

            // Open / Close
            if (!KerbalKonstructs.instance.disableRemoteBaseOpening)
            {
                content.Add(new DialogGUIButton("Open Base for " + site.OpenCost + " funds", OpenBase,
                    delegate { return !site.isOpen && CareerUtils.isCareerGame; }, windowWidth - 20f, 30f, false));
            }
            content.Add(new DialogGUIButton("Close Base for " + site.CloseValue + " funds", CloseBase,
                delegate { return site.isOpen && CareerUtils.isCareerGame; }, windowWidth - 20f, 30f, false));

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                bool isCurrentSite = (EditorLogic.fetch != null && site.LaunchSiteName == EditorLogic.fetch.launchSiteName);
                Texture statusIcon = isCurrentSite ? tSetLaunchsite : (site.isOpen ? tOpenedLaunchsite : tClosedLaunchsite);

                List<DialogGUIBase> row = new List<DialogGUIBase>();
                if (statusIcon != null)
                {
                    row.Add(new DialogGUIImage(new Vector2(32f, 32f), Vector2.zero, Color.white, statusIcon));
                }
                row.Add(new DialogGUIButton("Set as Launchsite", SetAsLaunchSite,
                    delegate { return site.isOpen && !(EditorLogic.fetch != null && site.LaunchSiteName == EditorLogic.fetch.launchSiteName); },
                    windowWidth - 60f, 30f, false));
                content.Add(new DialogGUIHorizontalLayout(row.ToArray()));
            }
            else
            {
                if (site.wayPoint == null)
                {
                    content.Add(new DialogGUIButton("Create Waypoint", delegate { MapIconSelector.CreateWPForLaunchSite(site); Reload(); }, windowWidth - 20f, 30f, false));
                }
                else
                {
                    content.Add(new DialogGUIButton("Delete Waypoint", DeleteWaypoint, windowWidth - 20f, 30f, false));
                }
            }
        }


        private static DialogGUIButton IconButton(Sprite icon, string fallbackText, Callback onClick)
        {
            if (icon != null)
            {
                return new DialogGUIButton(icon, onClick, 23f, 23f, false);
            }
            return new DialogGUIButton(fallbackText, onClick, 23f, 23f, false);
        }


        private static DialogGUIScrollList DescriptionList(KKLaunchSite site)
        {
            string description = string.IsNullOrEmpty(site.LaunchSiteDescription) ? "" : site.LaunchSiteDescription;
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            list.Add(new DialogGUILabel(description, KKStyle.whiteLabel, true, false));
            var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, 120f), false, true, layout);
        }


        private static DialogGUIScrollList LogList(KKLaunchSite site)
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            if (!string.IsNullOrEmpty(site.MissionLog))
            {
                foreach (string entry in site.MissionLog.Split('|'))
                {
                    if (!string.IsNullOrEmpty(entry))
                    {
                        list.Add(new DialogGUILabel(entry, KKStyle.whiteLabel, true, false));
                    }
                }
            }
            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, 120f), false, true, layout);
        }


        #region actions

        private static void ToggleFavourite()
        {
            selectedSite.favouriteSite = (selectedSite.favouriteSite == "Yes") ? "No" : "Yes";
            Reload();
        }


        private static void OpenBase()
        {
            if (CareerUtils.isCareerGame && selectedSite.OpenCost > Funding.Instance.Funds)
            {
                MiscUtils.HUDMessage("Insufficient funds to open this base!", 10, 3);
                return;
            }
            LaunchSiteManager.OpenLaunchSite(selectedSite);
            if (CareerUtils.isCareerGame)
            {
                Funding.Instance.AddFunds(-selectedSite.OpenCost, TransactionReasons.Cheating);
            }
            Reload();
        }


        private static void CloseBase()
        {
            LaunchSiteManager.CloseLaunchSite(selectedSite);
            if (CareerUtils.isCareerGame)
            {
                Funding.Instance.AddFunds(selectedSite.CloseValue, TransactionReasons.Cheating);
            }
            Reload();
        }


        private static void SetAsLaunchSite()
        {
            LaunchSiteManager.setLaunchSite(selectedSite);
            MiscUtils.HUDMessage(DisplayName(selectedSite) + " has been set as the launchsite", 10, 0);
            Reload();
        }


        private static void DeleteWaypoint()
        {
            if (selectedSite.wayPoint != null)
            {
                FinePrint.WaypointManager.RemoveWaypoint(selectedSite.wayPoint);
                selectedSite.wayPoint = null;
            }
            Reload();
        }

        #endregion


        /// <summary>
        /// Everything the window displays that can change while it is open.
        /// If this changes, the window is rebuilt.
        /// </summary>
        private static string GetStateSignature()
        {
            if (selectedSite == null)
            {
                return null;
            }

            // a waypoint removed elsewhere (e.g. in the stock waypoint UI) is gone
            if (selectedSite.wayPoint != null && FinePrint.WaypointManager.FindWaypoint(selectedSite.wayPoint.navigationId) == null)
            {
                selectedSite.wayPoint = null;
            }

            string editorSite = "";
            if (HighLogic.LoadedScene == GameScenes.EDITOR && EditorLogic.fetch != null)
            {
                editorSite = EditorLogic.fetch.launchSiteName;
            }

            return selectedSite.LaunchSiteName + "|" + selectedSite.isOpen + "|" + selectedSite.favouriteSite + "|" +
                (selectedSite.wayPoint != null) + "|" + editorSite;
        }


        private static void OnUpdate()
        {
            if (selectedSite == null)
            {
                Close();
                return;
            }

            if (GetStateSignature() != lastStateSignature)
            {
                Reload();
                return;
            }

            UpdateEditorLock();
        }


        /// <summary>
        /// Stop clicks on the window from reaching the editor (same as the old window did)
        /// </summary>
        private static void UpdateEditorLock()
        {
            if (HighLogic.LoadedScene != GameScenes.EDITOR || dialog == null)
            {
                return;
            }
            Camera uiCamera = KSP.UI.UIMasterController.Instance != null ? KSP.UI.UIMasterController.Instance.uiCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint(dialog.RTrf, Input.mousePosition, uiCamera))
            {
                InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, editorLockName);
            }
            else
            {
                InputLockManager.RemoveControlLock(editorLockName);
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
        }


        internal static void Open()
        {
            if (selectedSite == null)
            {
                Log.Normal("BaseManager: no launch site selected");
                return;
            }
            if (dialog != null)
            {
                // already open: just show the (maybe new) selected site
                Reload();
                return;
            }
            KKStyle.Init();
            LoadTextures();
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
            lastStateSignature = GetStateSignature();
        }


        /// <summary>
        /// Rebuild the window at its current position
        /// </summary>
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
            lastStateSignature = null;
            InputLockManager.RemoveControlLock(editorLockName);
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

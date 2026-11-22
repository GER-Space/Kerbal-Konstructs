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
    /// List of all models that can be spawned (UI2 / PopupDialog version).
    /// Replaces the "Spawn New" (SPAWN) mode of the old StaticsEditorGUI.
    /// Opened from the Instance editor toolbar, closed together with it.
    /// </summary>
    internal class ModelList
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "ModelList";
        internal static string windowMessage = null;
        internal static string windowTitle = "Spawn New";

        internal static Rect windowRect;

        internal static float windowWidth = 560f;
        internal static float windowHeight = 100f;

        private const float listHeight = 400f;
        private const float itemHeight = 23f;
        private const float categoryWidth = 120f;
        private const float meshWidth = 180f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.3f, 0.55f);

        internal static bool checkForParent = true;
        internal static Func<bool> parentWindow = InstanceEditorToolbar.IsOpen;

        private enum SortMode
        {
            None,
            Category,
            Title
        }

        private static SortMode sortMode = SortMode.None;

        // text typed into the filter fields, and the filters that are applied
        private static string categoryFilterText = "";
        private static string titleFilterText = "";
        private static string categoryFilter = "";
        private static string titleFilter = "";


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
            // column headers; Category and Title sort the list
            DialogGUIButton sortCategory = new DialogGUIButton("Category", delegate { SetSortMode(SortMode.Category); }, categoryWidth, itemHeight, false);
            sortCategory.tooltipText = "Sort by category";
            DialogGUIButton sortTitle = new DialogGUIButton("Title", delegate { SetSortMode(SortMode.Title); }, 80f, itemHeight, false);
            sortTitle.tooltipText = "Sort by title";
            content.Add(new DialogGUIHorizontalLayout(
                sortCategory,
                sortTitle,
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel("Mesh", KKStyle.whiteLabel),
                new DialogGUISpace(meshWidth - 40f)
                ));

            content.Add(ModelsList());

            // filters (was StaticsEditorGUI.ShowModelsFooter)
            DialogGUIButton applyFilter = new DialogGUIButton("Apply", ApplyFilters, 50f, itemHeight, false);
            applyFilter.tooltipText = "Apply the filters";
            DialogGUIButton clearFilter = new DialogGUIButton("Clear", ClearFilters, 50f, itemHeight, false);
            clearFilter.tooltipText = "Remove the filters";
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Category:", KKStyle.whiteLabel),
                new DialogGUITextInput(categoryFilterText, false, 30, SetCategoryFilterText, delegate { return categoryFilterText; }, TMPro.TMP_InputField.ContentType.Standard, itemHeight),
                new DialogGUILabel("Title:", KKStyle.whiteLabel),
                new DialogGUITextInput(titleFilterText, false, 30, SetTitleFilterText, delegate { return titleFilterText; }, TMPro.TMP_InputField.ContentType.Standard, itemHeight),
                applyFilter,
                clearFilter
                ));

            DialogGUIButton loadPreset = new DialogGUIButton("Load Preset", LoadGrassPreset, 90f, itemHeight, false);
            loadPreset.tooltipText = "Choose the grass color preset used for new statics";
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Default Grass Preset: " + StaticsEditorGUI.defaultGrassPreset.name, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                loadPreset
                ));
        }


        /// <summary>
        /// One row per model: category (sets the category filter), title (spawns an instance),
        /// mesh (opens the model config).
        /// (was StaticsEditorGUI.ShowModelsScroll)
        /// </summary>
        internal static DialogGUIScrollList ModelsList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<StaticModel> models = GetModels();
            foreach (StaticModel staticModel in models)
            {
                // local copy for the delegates
                StaticModel model = staticModel;

                DialogGUIButton categoryButton = new DialogGUIButton(model.category, delegate { FilterByCategory(model.category); }, categoryWidth, itemHeight, false);
                categoryButton.tooltipText = "Show only this category";

                DialogGUIButton titleButton = new DialogGUIButton(model.title, delegate { SpawnModel(model); }, windowWidth - categoryWidth - meshWidth - 60f, itemHeight, false);
                titleButton.tooltipText = "Spawn an instance of this static";

                DialogGUIButton meshButton = new DialogGUIButton(" " + model.mesh + " ", delegate { EditModel(model); }, meshWidth, itemHeight, false);
                meshButton.tooltipText = "Edit Model Config";

                list.Add(new DialogGUIHorizontalLayout(categoryButton, titleButton, meshButton));
            }

            if (models.Count == 0)
            {
                list.Add(new DialogGUILabel("No models found", KKStyle.whiteLabel));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, listHeight), false, true, layout);
        }


        /// <summary>
        /// The visible (not hidden) models, filtered and sorted.
        /// </summary>
        private static List<StaticModel> GetModels()
        {
            List<StaticModel> models = StaticDatabase.allStaticModels.Where(model => model.isHidden == false).ToList();

            if (categoryFilter != "")
            {
                models = models.Where(model => model.category != null && model.category.ToLower().Contains(categoryFilter.ToLower())).ToList();
            }
            if (titleFilter != "")
            {
                models = models.Where(model => model.title != null && model.title.ToLower().Contains(titleFilter.ToLower())).ToList();
            }

            switch (sortMode)
            {
                case SortMode.Category:
                    models.Sort((a, b) => string.Compare(a.category, b.category, StringComparison.OrdinalIgnoreCase));
                    break;
                case SortMode.Title:
                    models.Sort((a, b) => string.Compare(a.title, b.title, StringComparison.OrdinalIgnoreCase));
                    break;
            }
            return models;
        }


        #region filters and sorting

        private static string SetCategoryFilterText(string text)
        {
            categoryFilterText = text;
            return text;
        }

        private static string SetTitleFilterText(string text)
        {
            titleFilterText = text;
            return text;
        }

        private static void ApplyFilters()
        {
            categoryFilter = categoryFilterText;
            titleFilter = titleFilterText;
            Reload();
        }

        private static void ClearFilters()
        {
            categoryFilterText = "";
            titleFilterText = "";
            categoryFilter = "";
            titleFilter = "";
            Reload();
        }

        private static void FilterByCategory(string category)
        {
            categoryFilter = category;
            categoryFilterText = category;
            Reload();
        }

        private static void SetSortMode(SortMode newMode)
        {
            sortMode = newMode;
            Reload();
        }

        #endregion


        #region actions

        /// <summary>
        /// Spawns an instance of the model at the vessel, in the active (or closest local) group.
        /// (was StaticsEditorGUI.SpawnInstance)
        /// </summary>
        internal static void SpawnModel(StaticModel model)
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }
            StaticsEditorGUI.ResetLocalGroupList();
            GroupCenter center = StaticsEditorGUI.GetCloesedCenter(FlightGlobals.ActiveVessel.transform.position);
            if (center == null)
            {
                Log.UserError("No Local Group found");
                MiscUtils.HUDMessage("Create and place a local Group, then try again!");
                return;
            }

            EditorGUI.CloseEditors();
            KerbalKonstructs.instance.DeletePreviewObject();

            EditorGUI.instance.SpawnInstance(model, center, FlightGlobals.ActiveVessel.transform.position, Vector3.zero);
            if (!EditorGUI.instance.IsOpen())
            {
                EditorGUI.instance.Open();
            }
            MiscUtils.HUDMessage("Spawned " + model.title, 10, 2);
        }


        internal static void EditModel(StaticModel model)
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }
            StaticsEditorGUI.ResetLocalGroupList();
            if (StaticsEditorGUI.GetCloesedCenter(FlightGlobals.ActiveVessel.transform.position) == null)
            {
                Log.UserError("No Local Group found");
                MiscUtils.HUDMessage("Create and place a local Group, then try again!");
                return;
            }
            KerbalKonstructs.instance.selectedModel = model;
            ModelInfo.instance.Open();
        }


        private static void LoadGrassPreset()
        {
            GrassColorPresetUI2.callBack = SetDefaultGrassPreset;
            GrassColorPresetUI2.instance.Open();
        }


        private static void SetDefaultGrassPreset(GrassColorPresetUI2.ColorPreset2 preset)
        {
            StaticsEditorGUI.defaultGrassPreset = preset;
            if (isOpen)
            {
                Reload();
            }
        }

        #endregion


        private static void OnUpdate()
        {
            if (checkForParent && parentWindow != null && !parentWindow.Invoke())
            {
                Close();
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
            ConfigUtil.CreateNewInstanceDirIfNeeded();
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
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
    }
}

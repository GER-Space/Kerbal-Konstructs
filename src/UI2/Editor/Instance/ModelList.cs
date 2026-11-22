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
    /// "Spawn New" window, laid out like the parts list of the KSP vessel editor:
    /// categories on the left, the models of the category as spinning 3D previews
    /// in a 3 x 3 grid on the right, with pages and a search field.
    /// Click a preview to spawn the static (or to open its config in "Edit Config" mode).
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

        private const int columns = 3;
        private const int rows = 3;
        private const int modelsPerPage = columns * rows;
        private const int previewSize = 128;
        private const float cellSpacing = 6f;
        private const float labelHeight = 18f;
        private const float cellHeight = previewSize + labelHeight + 2f;
        private const float categoryWidth = 150f;
        private const float itemHeight = 23f;

        private const float gridWidth = columns * previewSize + (columns - 1) * cellSpacing;
        private const float gridHeight = rows * cellHeight + (rows - 1) * cellSpacing;

        internal static float windowWidth = categoryWidth + gridWidth + 50f;
        internal static float windowHeight = 100f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.3f, 0.55f);

        internal static bool checkForParent = true;
        internal static Func<bool> parentWindow = InstanceEditorToolbar.IsOpen;

        private const string allCategories = "All";

        private static string selectedCategory = allCategories;
        private static int page = 0;
        // text typed into the search field, and the search that is applied
        private static string searchText = "";
        private static string search = "";
        // when on, clicking a model opens its config instead of spawning it
        private static bool editConfig = false;

        private static ModelPreviewRenderer previewRenderer = null;
        private static List<StaticModel> pageModels = new List<StaticModel>();


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
            List<StaticModel> models = GetModels();
            int pageCount = Math.Max(1, (models.Count + modelsPerPage - 1) / modelsPerPage);
            page = Mathf.Clamp(page, 0, pageCount - 1);
            pageModels = models.Skip(page * modelsPerPage).Take(modelsPerPage).ToList();

            // search
            DialogGUIButton searchButton = new DialogGUIButton("Search", ApplySearch, 60f, itemHeight, false);
            searchButton.tooltipText = "Show only models whose title contains this text";
            DialogGUIButton clearButton = new DialogGUIButton("Clear", ClearSearch, 50f, itemHeight, false);
            clearButton.tooltipText = "Remove the search";
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Search:", KKStyle.whiteLabel),
                new DialogGUITextInput(searchText, false, 40, SetSearchText, delegate { return searchText; }, TMPro.TMP_InputField.ContentType.Standard, itemHeight),
                searchButton,
                clearButton
                ));

            // categories | preview grid
            DialogGUIVerticalLayout gridColumn = new DialogGUIVerticalLayout(gridWidth, gridHeight, 4f, new RectOffset(), TextAnchor.UpperLeft,
                PreviewGrid(),
                PageRow(page, pageCount, models.Count)
                );
            content.Add(new DialogGUIHorizontalLayout(false, false, 8f, new RectOffset(), TextAnchor.UpperLeft,
                CategoryList(),
                gridColumn
                ));

            // mode and grass preset
            DialogGUIButton modeButton = new DialogGUIButton(editConfig ? "Click: Edit Config" : "Click: Spawn", ToggleEditConfig, 140f, itemHeight, false);
            modeButton.tooltipText = "Switch between spawning a model and opening its config when it is clicked";
            DialogGUIButton loadPreset = new DialogGUIButton("Load Preset", LoadGrassPreset, 90f, itemHeight, false);
            loadPreset.tooltipText = "Choose the grass color preset used for new statics";
            content.Add(new DialogGUIHorizontalLayout(
                modeButton,
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel("Grass Preset: " + StaticsEditorGUI.defaultGrassPreset.name, KKStyle.whiteLabel),
                loadPreset
                ));
        }


        /// <summary>
        /// "All" and every category of the visible models; the selected one is disabled.
        /// </summary>
        private static DialogGUIScrollList CategoryList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<string> categories = new List<string>();
            categories.Add(allCategories);
            categories.AddRange(StaticDatabase.allStaticModels
                .Where(model => model.isHidden == false && !string.IsNullOrEmpty(model.category))
                .Select(model => model.category)
                .Distinct()
                .OrderBy(category => category, StringComparer.OrdinalIgnoreCase));

            foreach (string categoryName in categories)
            {
                // local copy for the delegates
                string category = categoryName;
                list.Add(new DialogGUIButton(category, delegate { SelectCategory(category); },
                    delegate { return selectedCategory != category; },
                    categoryWidth - 24f, itemHeight, false));
            }

            var layout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(2, 18, 2, 2), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(categoryWidth, gridHeight + itemHeight + 4f), false, true, layout);
        }


        /// <summary>
        /// The models of the current page as previews in a 3 x 3 grid, each with its title below.
        /// </summary>
        private static DialogGUIBase PreviewGrid()
        {
            List<DialogGUIBase> gridRows = new List<DialogGUIBase>();
            for (int row = 0; row < rows; row++)
            {
                List<DialogGUIBase> cells = new List<DialogGUIBase>();
                for (int column = 0; column < columns; column++)
                {
                    int slot = row * columns + column;
                    if (slot < pageModels.Count)
                    {
                        cells.Add(PreviewCell(slot, pageModels[slot]));
                    }
                    else
                    {
                        cells.Add(new DialogGUISpace(previewSize));
                    }
                }
                gridRows.Add(new DialogGUIHorizontalLayout(false, false, cellSpacing, new RectOffset(), TextAnchor.UpperLeft, cells.ToArray()));
            }

            if (pageModels.Count == 0)
            {
                gridRows.Insert(0, new DialogGUILabel("No models found", KKStyle.whiteLabel));
            }

            return new DialogGUIVerticalLayout(gridWidth, gridHeight, cellSpacing, new RectOffset(), TextAnchor.UpperLeft, gridRows.ToArray());
        }


        private static DialogGUIBase PreviewCell(int slot, StaticModel staticModel)
        {
            // local copy for the delegates
            StaticModel model = staticModel;

            DialogGUIImage preview = KKStyle.ClickableImage(previewRenderer.GetTexture(slot), delegate { ModelClicked(model); }, previewSize, previewSize);
            preview.tooltipText = model.title + "\nCategory: " + model.category + "\nMesh: " + model.mesh +
                (editConfig ? "\nClick to edit its config" : "\nClick to spawn");

            DialogGUILabel title = new DialogGUILabel(Shorten(model.title, 20), KKStyle.whiteLabel);

            return new DialogGUIVerticalLayout(previewSize, cellHeight, 2f, new RectOffset(), TextAnchor.UpperCenter,
                preview,
                title);
        }


        private static DialogGUIBase PageRow(int currentPage, int pageCount, int modelCount)
        {
            DialogGUIButton previous = new DialogGUIButton("<", delegate { SetPage(page - 1); }, delegate { return page > 0; }, 40f, itemHeight, false);
            previous.tooltipText = "Previous page";
            DialogGUIButton next = new DialogGUIButton(">", delegate { SetPage(page + 1); }, delegate { return page < pageCount - 1; }, 40f, itemHeight, false);
            next.tooltipText = "Next page";
            return new DialogGUIHorizontalLayout(
                previous,
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel("Page " + (currentPage + 1) + " / " + pageCount + "   (" + modelCount + " models)", KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                next
                );
        }


        private static string Shorten(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }
            return text.Substring(0, maxLength - 3) + "...";
        }


        /// <summary>
        /// The visible (not hidden) models of the selected category that match the search, sorted by title.
        /// </summary>
        private static List<StaticModel> GetModels()
        {
            IEnumerable<StaticModel> models = StaticDatabase.allStaticModels.Where(model => model.isHidden == false);

            if (selectedCategory != allCategories)
            {
                models = models.Where(model => model.category == selectedCategory);
            }
            if (search != "")
            {
                string lowerSearch = search.ToLower();
                models = models.Where(model => model.title != null && model.title.ToLower().Contains(lowerSearch));
            }
            return models.OrderBy(model => model.title, StringComparer.OrdinalIgnoreCase).ToList();
        }


        #region navigation

        private static void SelectCategory(string category)
        {
            selectedCategory = category;
            page = 0;
            Reload();
        }

        private static void SetPage(int newPage)
        {
            page = newPage;
            Reload();
        }

        private static string SetSearchText(string text)
        {
            searchText = text;
            return text;
        }

        private static void ApplySearch()
        {
            search = searchText;
            page = 0;
            Reload();
        }

        private static void ClearSearch()
        {
            searchText = "";
            search = "";
            page = 0;
            Reload();
        }

        private static void ToggleEditConfig()
        {
            editConfig = !editConfig;
            Reload();
        }

        #endregion


        #region actions

        private static void ModelClicked(StaticModel model)
        {
            if (editConfig)
            {
                EditModel(model);
            }
            else
            {
                SpawnModel(model);
            }
        }


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
            KKStyle.ApplyIconButtons(content);
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
            if (previewRenderer == null)
            {
                previewRenderer = ModelPreviewRenderer.Create(modelsPerPage, previewSize);
            }
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
            previewRenderer.SetModels(pageModels);
        }


        /// <summary>
        /// Rebuilds the window (after a page, category or search change); keeps the preview renderer.
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
            if (previewRenderer != null)
            {
                GameObject.Destroy(previewRenderer.gameObject);
                previewRenderer = null;
            }
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

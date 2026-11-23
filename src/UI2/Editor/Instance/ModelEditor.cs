using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Static Model Config Editor (UI2 / PopupDialog version, was UI.ModelInfo).
    /// Shows the model as a spinning 350 x 350 preview inside the window instead of spawning
    /// a preview static in the game world, and edits the model's config values.
    /// Opened by right-clicking a model in "Spawn New".
    /// </summary>
    internal class ModelEditor
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "ModelEditor";
        internal static string windowMessage = null;
        internal static string windowTitle = "Static Model Config Editor";

        internal static Rect windowRect;

        private const int previewSize = 350;
        private const float itemHeight = 23f;
        private const float labelWidth = 130f;
        private const float fieldWidth = 180f;

        internal static float windowWidth = previewSize + 40f;
        internal static float windowHeight = 100f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;

        // default position (normalized screen coordinates) when no saved position exists
        private static Vector2 defaultPosition = new Vector2(0.75f, 0.5f);

        internal static bool checkForParent = true;
        internal static Func<bool> parentWindow = InstanceEditorToolbar.IsOpen;

        internal static StaticModel model = null;

        private static ModelPreviewRenderer previewRenderer = null;
        private static bool chooseFacilityType = false;

        // the facility types that can be chosen: config value, shown name
        private static readonly string[][] facilityTypes = new string[][]
        {
            new string[] { "None", "None" },
            new string[] { "Barracks", "Barracks" },
            new string[] { "Business", "Business" },
            new string[] { "FuelTanks", "Fuel Tanks" },
            new string[] { "Hangar", "Hangar" },
            new string[] { "Research", "Research" },
            new string[] { "TrackingStation", "Tracking Station" },
            new string[] { "CityLights", "City Lights" },
            new string[] { "LandingGuide", "Landing Guide" },
        };

        // the values being edited, as typed
        private static string infTitle = "";
        private static string infCategory = "";
        private static string infCost = "";
        private static string infDescription = "";
        private static string infLaunchLength = "";
        private static string infLaunchWidth = "";
        private static string infLaunchHeight = "";
        private static string infLaunchMaxMass = "";
        private static string infLaunchMaxParts = "";
        private static string infFacType = "";
        private static string infFacMassCap = "";
        private static string infFacCraftCap = "";
        private static string infStaffMax = "";
        private static string infProdRateMax = "";
        private static string infScienceMax = "";
        private static string infFundsMax = "";


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
            content.Add(new DialogGUILabel(model.title, KKStyle.goldLabel));

            // the model, spinning
            DialogGUIImage preview = new DialogGUIImage(new Vector2(previewSize, previewSize), Vector2.zero, Color.white, previewRenderer.GetTexture(0));
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), preview, new DialogGUIFlexibleSpace()));

            DialogGUIButton spinButton = new DialogGUIButton(previewRenderer.spinning ? "Stop Spin" : "Resume Spin", ToggleSpin, 110f, itemHeight, false);
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), spinButton, new DialogGUIFlexibleSpace()));

            // the config values, scrollable
            content.Add(FieldsList());

            DialogGUIButton saveButton = new DialogGUIButton("Save", Save, 120f, itemHeight, false);
            saveButton.tooltipText = "Apply the changes and save them to the model's config file";
            content.Add(new DialogGUIHorizontalLayout(
                saveButton,
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("Close", Close, 120f, itemHeight, false)
                ));
        }


        private static DialogGUIScrollList FieldsList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            list.Add(TextRow("Title:", () => infTitle, s => infTitle = s, 60));

            if (!string.IsNullOrEmpty(model.DefaultLaunchPadTransform))
            {
                list.Add(new DialogGUILabel("Default launch site (" + model.DefaultLaunchPadTransform + ")", KKStyle.goldLabel));
                list.Add(TextRow("Length (m):", () => infLaunchLength, s => infLaunchLength = s, 8));
                list.Add(TextRow("Width (m):", () => infLaunchWidth, s => infLaunchWidth = s, 8));
                list.Add(TextRow("Height (m):", () => infLaunchHeight, s => infLaunchHeight = s, 8));
                list.Add(TextRow("Max Mass (t):", () => infLaunchMaxMass, s => infLaunchMaxMass = s, 8));
                list.Add(TextRow("Max Parts:", () => infLaunchMaxParts, s => infLaunchMaxParts = s, 8));
            }

            list.Add(InfoRow("Mesh:", model.mesh + ".mu"));
            list.Add(InfoRow("Manufacturer:", model.manufacturer));
            list.Add(InfoRow("Author:", model.author));

            list.Add(TextRow("Category:", () => infCategory, s => infCategory = s, 40));
            list.Add(TextRow("Cost:", () => infCost, s => infCost = s, 10));

            // facility type: a button that opens the list of types
            list.Add(new DialogGUIButton("Facility Type: " + FacilityTypeName(infFacType), delegate { chooseFacilityType = !chooseFacilityType; Reload(); }, previewSize - 30f, itemHeight, false));
            if (chooseFacilityType)
            {
                foreach (string[] facilityType in facilityTypes)
                {
                    string typeValue = facilityType[0];
                    list.Add(new DialogGUIButton("   " + facilityType[1], delegate { SetFacilityType(typeValue); },
                        delegate { return infFacType != typeValue; }, previewSize - 30f, itemHeight, false));
                }
            }

            if (infFacType == "Barracks" || infFacType == "Research" || infFacType == "Business")
            {
                list.Add(TextRow("Max Staff:", () => infStaffMax, s => infStaffMax = s, 3));
            }
            if (infFacType == "Research" || infFacType == "Business")
            {
                list.Add(TextRow("Production Rate:", () => infProdRateMax, s => infProdRateMax = s, 6));
                list.Add(new DialogGUILabel("Amount produced every 12 hours is production rate multiplied by current number of staff.", KKStyle.whiteLabel, true, false));
            }
            if (infFacType == "Research")
            {
                list.Add(TextRow("Max Science:", () => infScienceMax, s => infScienceMax = s, 4));
            }
            if (infFacType == "Business")
            {
                list.Add(TextRow("Max Funds:", () => infFundsMax, s => infFundsMax = s, 7));
            }
            if (infFacType == "Hangar")
            {
                list.Add(TextRow("Max Craft Mass (t):", () => infFacMassCap, s => infFacMassCap = s, 5));
                list.Add(new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Max Craft:", KKStyle.whiteLabel),
                    new DialogGUIFlexibleSpace(),
                    CraftCapButton("1"),
                    CraftCapButton("2"),
                    CraftCapButton("3")
                    ));
            }

            list.Add(new DialogGUILabel("Description:", KKStyle.whiteLabel));
            list.Add(new DialogGUITextInput(infDescription, true, 1000, delegate (string s) { infDescription = s; return s; }, delegate { return infDescription; }, TMPro.TMP_InputField.ContentType.Standard, 60f));

            var layout = new DialogGUIVerticalLayout(10, 100, 3, new RectOffset(4, 22, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(windowWidth - 20f, 260f), false, true, layout);
        }


        private static DialogGUIBase TextRow(string label, Func<string> getValue, Action<string> setValue, int maxLength)
        {
            DialogGUITextInput input = new DialogGUITextInput(getValue(), false, maxLength, delegate (string s) { setValue(s); return s; }, getValue, TMPro.TMP_InputField.ContentType.Standard, itemHeight);
            return new DialogGUIHorizontalLayout(
                new DialogGUILabel(label, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                new DialogGUIHorizontalLayout(fieldWidth, itemHeight, 0f, new RectOffset(), TextAnchor.MiddleRight, input)
                );
        }


        private static DialogGUIBase InfoRow(string label, string value)
        {
            return new DialogGUIHorizontalLayout(
                new DialogGUILabel(label, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel(value ?? "", KKStyle.goldLabel)
                );
        }


        private static DialogGUIButton CraftCapButton(string value)
        {
            return new DialogGUIButton(value, delegate { infFacCraftCap = value; Reload(); },
                delegate { return infFacCraftCap != value; }, itemHeight, itemHeight, false);
        }


        private static string FacilityTypeName(string typeValue)
        {
            foreach (string[] facilityType in facilityTypes)
            {
                if (facilityType[0] == typeValue)
                {
                    return facilityType[1];
                }
            }
            return string.IsNullOrEmpty(typeValue) ? "None" : typeValue;
        }


        private static void SetFacilityType(string typeValue)
        {
            infFacType = typeValue;
            chooseFacilityType = false;
            Reload();
        }


        private static void ToggleSpin()
        {
            previewRenderer.spinning = !previewRenderer.spinning;
            Reload();
        }


        /// <summary>
        /// Copies the model's values into the edit fields (was ModelInfo.updateSelection)
        /// </summary>
        private static void ReadModel()
        {
            infTitle = model.title ?? "";
            infCategory = model.category ?? "";
            infCost = model.cost.ToString();
            infDescription = model.description ?? "";
            infLaunchLength = model.DefaultLaunchSiteLength.ToString();
            infLaunchWidth = model.DefaultLaunchSiteWidth.ToString();
            infLaunchHeight = model.DefaultLaunchSiteHeight.ToString();
            infLaunchMaxMass = model.DefaultLaunchSiteMaxMass.ToString();
            infLaunchMaxParts = model.DefaultLaunchSiteMaxParts.ToString();
            infFacType = model.DefaultFacilityType ?? "None";
            infFacMassCap = model.DefaultFacilityMassCapacity.ToString();
            infFacCraftCap = model.DefaultFacilityCraftCapacity.ToString();
            infStaffMax = model.DefaultStaffMax.ToString();
            infProdRateMax = model.DefaultProductionRateMax.ToString();
            infScienceMax = model.DefaultScienceOMax.ToString();
            infFundsMax = model.DefaultFundsOMax.ToString();
        }


        /// <summary>
        /// Checks all numbers first; only if all are valid, the model is changed and saved.
        /// (was ModelInfo.updateSettings + Save, which threw on an invalid number)
        /// </summary>
        private static void Save()
        {
            float cost, launchLength, launchWidth, launchHeight, launchMaxMass, facMassCap, prodRate, scienceMax, fundsMax;
            int launchMaxParts, craftCap, staffMax;
            List<string> invalid = new List<string>();

            if (!MiscUtils.TryParseFloat(infCost, out cost)) { invalid.Add("Cost"); }
            if (!MiscUtils.TryParseFloat(infLaunchLength, out launchLength)) { invalid.Add("Length"); }
            if (!MiscUtils.TryParseFloat(infLaunchWidth, out launchWidth)) { invalid.Add("Width"); }
            if (!MiscUtils.TryParseFloat(infLaunchHeight, out launchHeight)) { invalid.Add("Height"); }
            if (!MiscUtils.TryParseFloat(infLaunchMaxMass, out launchMaxMass)) { invalid.Add("Max Mass"); }
            if (!int.TryParse(infLaunchMaxParts, out launchMaxParts)) { invalid.Add("Max Parts"); }
            if (!MiscUtils.TryParseFloat(infFacMassCap, out facMassCap)) { invalid.Add("Max Craft Mass"); }
            if (!int.TryParse(infFacCraftCap, out craftCap)) { invalid.Add("Max Craft"); }
            if (!int.TryParse(infStaffMax, out staffMax)) { invalid.Add("Max Staff"); }
            if (!MiscUtils.TryParseFloat(infProdRateMax, out prodRate)) { invalid.Add("Production Rate"); }
            if (!MiscUtils.TryParseFloat(infScienceMax, out scienceMax)) { invalid.Add("Max Science"); }
            if (!MiscUtils.TryParseFloat(infFundsMax, out fundsMax)) { invalid.Add("Max Funds"); }

            if (invalid.Count > 0)
            {
                MiscUtils.HUDMessage("Not saved, not a valid number: " + string.Join(", ", invalid.ToArray()), 10, 2);
                return;
            }

            model.title = infTitle;
            model.category = infCategory;
            model.cost = cost;
            model.description = infDescription;
            model.DefaultFacilityType = infFacType;
            model.DefaultLaunchSiteLength = launchLength;
            model.DefaultLaunchSiteWidth = launchWidth;
            model.DefaultLaunchSiteHeight = launchHeight;
            model.DefaultLaunchSiteMaxMass = launchMaxMass;
            model.DefaultLaunchSiteMaxParts = launchMaxParts;
            model.DefaultFacilityMassCapacity = facMassCap;
            model.DefaultFacilityCraftCapacity = craftCap;
            model.DefaultStaffMax = staffMax;
            model.DefaultProductionRateMax = prodRate;
            model.DefaultScienceOMax = scienceMax;
            model.DefaultFundsOMax = fundsMax;

            KerbalKonstructs.instance.SaveModelConfig(model);
            MiscUtils.HUDMessage("Saved changes to static models config.", 10, 2);

            // title / category may have changed
            if (ModelList.IsOpen())
            {
                ModelList.Reload();
            }
            Reload();
        }


        private static void OnUpdate()
        {
            if (model == null || (checkForParent && parentWindow != null && !parentWindow.Invoke()))
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


        /// <summary>
        /// Opens the editor for this model (or switches it to this model)
        /// </summary>
        internal static void Open(StaticModel newModel)
        {
            if (newModel == null)
            {
                return;
            }
            if (dialog != null && model == newModel)
            {
                return;
            }
            bool otherModel = (model != newModel);
            if (dialog != null)
            {
                DismissDialog();
            }
            model = newModel;
            KerbalKonstructs.instance.selectedModel = newModel;
            if (otherModel)
            {
                ReadModel();
                chooseFacilityType = false;
            }

            if (previewRenderer == null)
            {
                previewRenderer = ModelPreviewRenderer.Create(1, previewSize);
            }
            previewRenderer.SetModels(new List<StaticModel> { model });

            Build();
        }


        private static void Build()
        {
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            KKTitle();
            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
        }


        private static void DismissDialog()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
        }


        /// <summary>
        /// Rebuilds the window, keeping the values being edited and the preview
        /// </summary>
        internal static void Reload()
        {
            if (model == null)
            {
                return;
            }
            DismissDialog();
            Build();
        }


        internal static void Close()
        {
            DismissDialog();
            if (previewRenderer != null)
            {
                GameObject.Destroy(previewRenderer.gameObject);
                previewRenderer = null;
            }
            model = null;
            KerbalKonstructs.instance.selectedModel = null;
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
    }
}

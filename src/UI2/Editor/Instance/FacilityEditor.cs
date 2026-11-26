using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Facility editor: sets the facility type of the selected static and its settings.
    /// UI2 version of the old IMGUI window; the window is rebuilt when the facility type (and so its rows) changes.
    /// The yes/no options are on/off switches.
    /// </summary>
    internal class FacilityEditor : KKEditorWindow
    {
        private static FacilityEditor _instance = null;
        internal static FacilityEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FacilityEditor();
                }
                return _instance;
            }
        }

        protected override string WindowName => "FacilityEditor";
        protected override string WindowTitle => "Facility Editor";
        protected override float WindowWidth => 360f;
        protected override Vector2 DefaultPosition => new Vector2(0.45f, 0.5f);

        private const float fieldWidth = 150f;

        private StaticInstance selectedObject = null;

        private static string facType = "None";
        private string infTrackingShort = "", infOpenCost = "", infStaffMax = "", infProdRateMax = "", infScienceMax = "", infFundsMax = "";
        private string infFacMassCap = "";
        private string infFacCraftCap = "";

        // the facilities that should not be selected
        private HashSet<string> hiddenFacilities = new HashSet<string> { "TrackingStation" };

        // which list replaces the normal rows
        private bool showTypeList = false;
        private bool showResourceSelection = false;


        #region Window handling

        internal void Open()
        {
            if (KerbalKonstructs.selectedInstance == null)
            {
                return;
            }
            DismissDialog();
            selectedObject = KerbalKonstructs.selectedInstance;
            showTypeList = false;
            showResourceSelection = false;
            UpdateSelection();
            Build();
        }


        public override void Close()
        {
            DismissDialog();
            selectedObject = null;
        }


        protected override void OnUpdate()
        {
            if (MapView.MapIsEnabled || KerbalKonstructs.selectedInstance == null || KerbalKonstructs.selectedInstance != selectedObject)
            {
                Close();
            }
        }

        #endregion


        #region Content

        protected override void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), GoldLabel(delegate { return selectedObject.model.title; }), new DialogGUIFlexibleSpace()));

            DialogGUIButton typeButton = new DialogGUIButton(delegate { return facType; }, delegate { showTypeList = true; Reload(); }, delegate { return !showTypeList; }, 185f, itemHeight, false);
            typeButton.tooltipText = "Change the facility type";
            content.Add(new DialogGUIHorizontalLayout(Label("Facility type:"), new DialogGUIFlexibleSpace(), typeButton));

            if (showTypeList)
            {
                content.Add(TypeList());
                return;
            }

            if (facType != "None" && selectedObject.hasFacilities)
            {
                OpenCloseFields();
            }

            switch (selectedObject.facilityType)
            {
                case KKFacilityType.Barracks:
                    MaxStaffFields();
                    break;
                case KKFacilityType.Business:
                    MaxStaffFields();
                    ProductionRateFields();
                    content.Add(TextRow("Max funds:", delegate { return infFundsMax; }, delegate (string s) { infFundsMax = s; }, 8, fieldWidth));
                    break;
                case KKFacilityType.GroundStation:
                    content.Add(TextRow("Antenna range:", delegate { return infTrackingShort; }, delegate (string s) { infTrackingShort = s; }, 15, fieldWidth, "m"));
                    break;
                case KKFacilityType.Hangar:
                    content.Add(TextRow("Max craft weight:", delegate { return infFacMassCap; }, delegate (string s) { infFacMassCap = s; }, 6, fieldWidth, "t"));
                    content.Add(TextRow("Max no. of craft:", delegate { return infFacCraftCap; }, delegate (string s) { infFacCraftCap = s; }, 4, fieldWidth));
                    break;
                case KKFacilityType.Research:
                    MaxStaffFields();
                    ProductionRateFields();
                    content.Add(TextRow("Max science:", delegate { return infScienceMax; }, delegate (string s) { infScienceMax = s; }, 6, fieldWidth));
                    break;
                case KKFacilityType.Merchant:
                    MerchantFields();
                    break;
                case KKFacilityType.Storage:
                    StorageFields();
                    break;
                default:
                    break;
            }

            Separator();
            DialogGUIButton saveButton = new DialogGUIButton("Save Facility", SaveFacility, 140f, itemHeight, false);
            saveButton.tooltipText = "Apply the settings, save the static and close";
            content.Add(new DialogGUIHorizontalLayout(saveButton, new DialogGUIFlexibleSpace(), new DialogGUIButton("Cancel", Close, 120f, itemHeight, false)));
        }


        private DialogGUIBase TypeList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            list.Add(new DialogGUIButton("Cancel - No change", delegate { showTypeList = false; Reload(); }, 280f, itemHeight, false));
            foreach (string facility in Enum.GetNames(typeof(KKFacilityType)))
            {
                if (hiddenFacilities.Contains(facility))
                {
                    continue;
                }
                string thisFacility = facility;
                list.Add(new DialogGUIButton(thisFacility, delegate { SetFacilityType(thisFacility); }, delegate { return facType != thisFacility; }, 280f, itemHeight, false));
            }
            var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            return new DialogGUIScrollList(new Vector2(WindowWidth - 20f, 300f), false, true, layout);
        }


        /// <summary>
        /// Open cost, default state and name: every facility has them
        /// </summary>
        private void OpenCloseFields()
        {
            content.Add(TextRow("Open cost:", delegate { return infOpenCost; }, delegate (string s) { infOpenCost = s; }, 8, fieldWidth, "\\F"));
            content.Add(new DialogGUIHorizontalLayout(
                Label("Open by default:"),
                new DialogGUIFlexibleSpace(),
                Label(delegate { return selectedObject.hasFacilities ? selectedObject.myFacilities[0].defaultState : ""; }),
                KKControls.Switch(
                    delegate { return selectedObject.hasFacilities && selectedObject.myFacilities[0].defaultState == "Open"; },
                    delegate (bool open) { if (selectedObject.hasFacilities) { selectedObject.myFacilities[0].defaultState = open ? "Open" : "Closed"; } })
                ));
            content.Add(TextRow("Facility name:",
                delegate { return selectedObject.hasFacilities ? selectedObject.myFacilities[0].FacilityName : ""; },
                delegate (string s) { if (selectedObject.hasFacilities) { selectedObject.myFacilities[0].FacilityName = s; } },
                30, 200f));
        }


        private void MaxStaffFields()
        {
            content.Add(TextRow("Max staff:", delegate { return infStaffMax; }, delegate (string s) { infStaffMax = s; }, 3, fieldWidth));
        }


        private void ProductionRateFields()
        {
            content.Add(TextRow("Production rate:", delegate { return infProdRateMax; }, delegate (string s) { infProdRateMax = s; }, 6, fieldWidth));
            content.Add(Label("Amount produced every Kerbin day. The production rate is multiplied by the current number of staff."));
        }


        private void StorageFields()
        {
            Storage facStorage = selectedObject.myFacilities[0] as Storage;
            if (facStorage == null)
            {
                return;
            }
            content.Add(TextRow("Max volume:",
                delegate { return facStorage.maxVolume.ToString(CultureInfo.InvariantCulture); },
                delegate (string s) { float value; if (TryParseFloat(s, out value)) { facStorage.maxVolume = Math.Max(0, value); } },
                15, fieldWidth, "m^3"));
        }


        private void MerchantFields()
        {
            Merchant facMerchant = selectedObject.myFacilities[0] as Merchant;
            if (facMerchant == null)
            {
                return;
            }

            Separator();
            if (showResourceSelection)
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIButton("Cancel - No change", delegate { showResourceSelection = false; Reload(); }, 280f, itemHeight, false));
                foreach (PartResourceDefinition availableResource in PartResourceLibrary.Instance.resourceDefinitions)
                {
                    if (facMerchant.tradedResources.Any(x => x.resource.name.Equals(availableResource.name)))
                    {
                        continue;
                    }
                    PartResourceDefinition thisResource = availableResource;
                    list.Add(new DialogGUIButton(thisResource.name, delegate
                    {
                        facMerchant.tradedResources.Add(new TradedResource() { resource = thisResource });
                        showResourceSelection = false;
                        Reload();
                    }, 280f, itemHeight, false));
                }
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
                content.Add(new DialogGUIScrollList(new Vector2(WindowWidth - 20f, 250f), false, true, layout));
                return;
            }

            DialogGUIButton addButton = new DialogGUIButton("Add Resources", delegate { showResourceSelection = true; Reload(); }, 140f, itemHeight, false);
            addButton.tooltipText = "Add a resource this merchant trades";
            content.Add(new DialogGUIHorizontalLayout(addButton, new DialogGUIFlexibleSpace()));

            if (facMerchant.tradedResources.Count == 0)
            {
                return;
            }

            List<DialogGUIBase> rows = new List<DialogGUIBase>();
            rows.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            foreach (TradedResource tradedResource in facMerchant.tradedResources)
            {
                TradedResource resource = tradedResource;
                DialogGUIButton removeButton = new DialogGUIButton("X", delegate { facMerchant.tradedResources.Remove(resource); Reload(); }, 23f, itemHeight, false, KKStyle.DeadButtonRed);
                removeButton.tooltipText = "Remove " + resource.resource.name;
                rows.Add(new DialogGUIHorizontalLayout(removeButton, GoldLabel(delegate { return resource.resource.name; })));
                rows.Add(new DialogGUIHorizontalLayout(
                    Label("Buying mult:"),
                    TextField(delegate { return resource.multiplierBuy.ToString(CultureInfo.InvariantCulture); }, delegate (string s) { float value; if (TryParseFloat(s, out value)) { resource.multiplierBuy = value; } }, 6, 55f),
                    new DialogGUIFlexibleSpace(),
                    Label("buyable"),
                    KKControls.Switch(delegate { return resource.canBeBought; }, delegate (bool value) { resource.canBeBought = value; })
                    ));
                rows.Add(new DialogGUIHorizontalLayout(
                    Label("Selling mult:"),
                    TextField(delegate { return resource.multiplierSell.ToString(CultureInfo.InvariantCulture); }, delegate (string s) { float value; if (TryParseFloat(s, out value)) { resource.multiplierSell = value; } }, 6, 55f),
                    new DialogGUIFlexibleSpace(),
                    Label("sellable"),
                    KKControls.Switch(delegate { return resource.canBeSold; }, delegate (bool value) { resource.canBeSold = value; })
                    ));
                rows.Add(new DialogGUISpace(6f));
            }
            var resourceLayout = new DialogGUIVerticalLayout(10, 100, 2, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, rows.ToArray());
            content.Add(new DialogGUIScrollList(new Vector2(WindowWidth - 20f, 220f), false, true, resourceLayout));
        }

        #endregion


        #region Actions

        private void SetFacilityType(string facility)
        {
            facType = facility;
            showTypeList = false;
            showResourceSelection = false;
            if (selectedObject.FacilityType != facType)
            {
                ChangeFacility();
                UpdateSelection();
            }
            Reload();
        }


        private void SaveFacility()
        {
            if (!UpdateSettings())
            {
                MiscUtils.HUDMessage("Facility not saved: invalid numeric input", 10, 3);
                return;
            }
            UpdateSelection();
            selectedObject.SaveConfig();
            MiscUtils.HUDMessage("Applied changes to object.", 10, 2);
            Close();
        }


        /// <summary>
        /// Removes add previous facility settings from Static Object
        /// </summary>
        private void RemoveOldSettings()
        {
            if (selectedObject.hasFacilities)
            {
                Log.Normal("FacEditor: Remove called");
                GameObject.Destroy(selectedObject.myFacilities[0]);
                selectedObject.myFacilities = new List<KKFacility>();
                Log.Normal("FacEditor: Remove Finished");
            }
        }


        /// <summary>
        /// Change the facility to the new type
        /// </summary>
        internal void ChangeFacility()
        {
            RemoveOldSettings();

            Log.Normal("FacEditor: Change called");
            KKFacilityType newType;
            try
            {
                newType = (KKFacilityType)Enum.Parse(typeof(KKFacilityType), facType, true);
            }
            catch
            {
                return;
            }

            selectedObject.facilityType = newType;
            selectedObject.FacilityType = facType;

            if (newType == KKFacilityType.None)
            {
                return;
            }
            // use a fake cfgnode for initialization
            ConfigNode cfgNode = new ConfigNode();

            switch (newType)
            {
                case KKFacilityType.Merchant:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Merchant>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Storage:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Storage>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.GroundStation:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.FuelTanks:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<FuelTanks>().ParseConfig(cfgNode));
                    selectedObject.facilityType = KKFacilityType.Merchant;
                    selectedObject.FacilityType = "Merchant";
                    facType = selectedObject.FacilityType;
                    break;
                case KKFacilityType.Research:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Research>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Business:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Business>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Hangar:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Hangar>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.Barracks:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Barracks>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.LandingGuide:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<LandingGuide>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TouchdownGuideL:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<TouchdownGuideL>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TouchdownGuideR:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<TouchdownGuideR>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.RecoveryBase:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<RecoveryBase>().ParseConfig(cfgNode));
                    break;
            }
            Log.Normal("FacEditor: Change Finished");
            foreach (var facility in selectedObject.myFacilities)
            {
                facility.staticInstance = selectedObject;
            }
        }


        /// <summary>
        /// Push the values into the facility objects. False (and nothing changed) if a number can't be read.
        /// </summary>
        public bool UpdateSettings()
        {
            float trackingShort = 0, massCap = 0, prodRate = 0, scienceMax = 0, fundsMax = 0, openCost = 0;
            int craftCap = 0, staffMax = 0;

            bool valid = true;
            switch (facType)
            {
                case "GroundStation":
                    valid = TryParseFloat(infTrackingShort, out trackingShort);
                    break;
                case "Hangar":
                    valid = (infFacMassCap == "" || TryParseFloat(infFacMassCap, out massCap)) && (infFacCraftCap == "" || TryParseInt(infFacCraftCap, out craftCap));
                    break;
                case "Barracks":
                    valid = (infStaffMax == "" || TryParseInt(infStaffMax, out staffMax));
                    break;
                case "Research":
                    valid = (infStaffMax == "" || TryParseInt(infStaffMax, out staffMax)) && (infProdRateMax == "" || TryParseFloat(infProdRateMax, out prodRate)) && (infScienceMax == "" || TryParseFloat(infScienceMax, out scienceMax));
                    break;
                case "Business":
                    valid = (infStaffMax == "" || TryParseInt(infStaffMax, out staffMax)) && (infProdRateMax == "" || TryParseFloat(infProdRateMax, out prodRate)) && (infFundsMax == "" || TryParseFloat(infFundsMax, out fundsMax));
                    break;
            }
            if (facType != "None" && !TryParseFloat(infOpenCost, out openCost))
            {
                valid = false;
            }
            if (!valid)
            {
                return false;
            }

            if (facType != selectedObject.FacilityType)
            {
                ChangeFacility();
            }

            if (facType != "")
            {
                selectedObject.FacilityType = facType;
            }

            switch (facType)
            {
                case "GroundStation":
                    ((GroundStation)selectedObject.myFacilities[0]).TrackingShort = trackingShort;
                    break;
                case "Hangar":
                    if (infFacMassCap != "") ((Hangar)selectedObject.myFacilities[0]).FacilityMassCapacity = massCap;
                    if (infFacCraftCap != "") ((Hangar)selectedObject.myFacilities[0]).FacilityCraftCapacity = craftCap;
                    break;
                case "Barracks":
                    if (infStaffMax != "") ((Barracks)selectedObject.myFacilities[0]).StaffMax = staffMax;
                    break;
                case "Research":
                    if (infStaffMax != "") ((Research)selectedObject.myFacilities[0]).StaffMax = staffMax;
                    if (infProdRateMax != "") ((Research)selectedObject.myFacilities[0]).ProductionRateMax = prodRate;
                    if (infScienceMax != "") ((Research)selectedObject.myFacilities[0]).ScienceOMax = scienceMax;
                    break;
                case "Business":
                    if (infStaffMax != "") ((Business)selectedObject.myFacilities[0]).StaffMax = staffMax;
                    if (infProdRateMax != "") ((Business)selectedObject.myFacilities[0]).ProductionRateMax = prodRate;
                    if (infFundsMax != "") ((Business)selectedObject.myFacilities[0]).FundsOMax = fundsMax;
                    break;
                default:
                    break;
            }

            if (facType != "None" && selectedObject.hasFacilities)
            {
                selectedObject.myFacilities[0].OpenCost = openCost;
                selectedObject.myFacilities[0].CloseValue = (float)Math.Round((selectedObject.myFacilities[0].OpenCost / 4), 0);
            }

            foreach (var facility in selectedObject.myFacilities)
            {
                facility.staticInstance = selectedObject;
            }
            return true;
        }


        /// <summary>
        /// Load the facility values into the text fields
        /// </summary>
        private void UpdateSelection()
        {
            if (selectedObject.hasFacilities)
            {
                facType = selectedObject.myFacilities[0].FacilityType;
            }
            else
            {
                facType = "None";
            }

            if (string.IsNullOrEmpty(facType))
            {
                facType = selectedObject.model.DefaultFacilityType;
                if (string.IsNullOrEmpty(facType))
                {
                    facType = "None";
                }
            }

            switch (facType)
            {
                case "GroundStation":
                    infTrackingShort = ((GroundStation)selectedObject.myFacilities[0]).TrackingShort.ToString();
                    break;
                case "Hangar":
                    infFacMassCap = ((Hangar)selectedObject.myFacilities[0]).FacilityMassCapacity.ToString();
                    if (infFacMassCap == "0" || infFacMassCap == "")
                        infFacMassCap = selectedObject.model.DefaultFacilityMassCapacity.ToString();
                    infFacCraftCap = ((Hangar)selectedObject.myFacilities[0]).FacilityCraftCapacity.ToString();
                    if (infFacCraftCap == "0" || infFacCraftCap == "")
                        infFacCraftCap = selectedObject.model.DefaultFacilityCraftCapacity.ToString();
                    break;
                case "Barracks":
                    infStaffMax = ((Barracks)selectedObject.myFacilities[0]).StaffMax.ToString();
                    if (infStaffMax == "0" || infStaffMax == "")
                        infStaffMax = selectedObject.model.DefaultStaffMax.ToString();
                    break;
                case "Research":
                    infStaffMax = ((Research)selectedObject.myFacilities[0]).StaffMax.ToString();
                    if (infStaffMax == "0" || infStaffMax == "")
                        infStaffMax = selectedObject.model.DefaultStaffMax.ToString();
                    infProdRateMax = ((Research)selectedObject.myFacilities[0]).ProductionRateMax.ToString();
                    if (infProdRateMax == "0" || infProdRateMax == "")
                        infProdRateMax = selectedObject.model.DefaultProductionRateMax.ToString();
                    infScienceMax = ((Research)selectedObject.myFacilities[0]).ScienceOMax.ToString();
                    if (infScienceMax == "0" || infScienceMax == "")
                        infScienceMax = selectedObject.model.DefaultScienceOMax.ToString();
                    break;
                case "Business":
                    infStaffMax = ((Business)selectedObject.myFacilities[0]).StaffMax.ToString();
                    if (infStaffMax == "0" || infStaffMax == "")
                        infStaffMax = selectedObject.model.DefaultStaffMax.ToString();
                    infProdRateMax = ((Business)selectedObject.myFacilities[0]).ProductionRateMax.ToString();
                    if (infProdRateMax == "0" || infProdRateMax == "")
                        infProdRateMax = selectedObject.model.DefaultProductionRateMax.ToString();
                    infFundsMax = ((Business)selectedObject.myFacilities[0]).FundsOMax.ToString();
                    if (infFundsMax == "0" || infFundsMax == "")
                        infFundsMax = selectedObject.model.DefaultFundsOMax.ToString();
                    break;
            }

            if (selectedObject.hasFacilities)
            {
                if (string.IsNullOrEmpty(selectedObject.myFacilities[0].FacilityName))
                {
                    selectedObject.myFacilities[0].FacilityName = selectedObject.myFacilities[0].FacilityType + "@" + selectedObject.model.name;
                }

                infOpenCost = selectedObject.myFacilities[0].OpenCost.ToString();
                if (infOpenCost == "0" || infOpenCost == "")
                {
                    infOpenCost = selectedObject.model.cost.ToString();
                }

                // storage: start with the size of the model, rounded to 100 m^3
                Storage facStorage = selectedObject.myFacilities[0] as Storage;
                if (facStorage != null)
                {
                    if (facStorage.maxVolume == 0f)
                    {
                        Vector3 size = selectedObject.mesh.GetRendererBounds().extents;
                        facStorage.maxVolume += (size.x * size.y * size.z);
                    }
                    facStorage.maxVolume = (float)Math.Round(facStorage.maxVolume / 100, 0) * 100;
                }
            }
            else
            {
                infOpenCost = "0";
            }

            facType = selectedObject.FacilityType;
            if (string.IsNullOrEmpty(facType))
            {
                facType = "None";
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.UI
{
    class FacilityEditor : KKWindow
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

        private bool guiInitialized = false;
        private Rect facilityEditorRect = new Rect(400, 45, 360, 400);

        private GUIStyle LabelGreen;
        private GUIStyle LabelWhite;

        private GUIStyle DeadButton;
        private GUIStyle DeadButtonRed;
        private GUIStyle KKWindows;
        private GUIStyle BoxNoBorder;
        private GUIStyle redButton;

        private Vector2 facilityscroll;
        private Vector2 merchantScrollPos;
        private Vector2 resourceScrollPos;

        StaticInstance selectedObject = null;

        private Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

        private bool bChangeFacilityType = false;
        private static String facType = "None";
        private string infTrackingShort, infOpenCost, infStaffMax, infProdRateMax, infScienceMax, infFundsMax = "";

        private string infFacMassCap = "";
        private string infFacCraftCap = "";
        private string infLqFMax = "";
        private string infOxFMax = "";
        private string infMoFMax = "";
        private bool isDefaultOpen = false;


        // put herer the facilities that should not be selected
        private HashSet<string> hiddenFacilities = new HashSet<string> { "TrackingStation" };

        private bool showResourceSelection = false;


        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }
            if (KerbalKonstructs.instance.selectedObject == null)
            {
                Close();
            }
            if (!guiInitialized)
            {
                InitializeLayout();
                guiInitialized = true;
            }
            if (selectedObject != KerbalKonstructs.instance.selectedObject)
            {
                selectedObject = KerbalKonstructs.instance.selectedObject;
                updateSelection();
            }
            
            facilityEditorRect = GUI.Window(0xD12B1F7, facilityEditorRect, drawFacilityEditorWindow, "", KKWindows);

        }

        public override void Open()
        {
            if (KerbalKonstructs.instance.selectedObject != null)
            {
                selectedObject = KerbalKonstructs.instance.selectedObject;
                updateSelection();
            }
            base.Open();
        }


        public override void Close()
        {
            selectedObject = null;
            base.Close();
        }


        #region editor

        /// <summary>
        /// Facility Editor window
        /// </summary>
        /// <param name="id"></param>
        void drawFacilityEditorWindow(int id)
        {
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Facility Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.Box((string)selectedObject.model.title);
            GUILayout.Space(1);
            if (GUILayout.Button("Facility Type: " + facType, GUILayout.Height(23)))
            {
                bChangeFacilityType = true;
            }

            if (bChangeFacilityType)
            {
                facilityscroll = GUILayout.BeginScrollView(facilityscroll);
                {
                    if (GUILayout.Button("Cancel - No change", GUILayout.Height(23)))
                    {
                        bChangeFacilityType = false;
                    }

                    foreach (string facility in Enum.GetNames(typeof(KKFacilityType)))
                    {
                        if (hiddenFacilities.Contains(facility))
                        {
                            continue;
                        }

                        if (GUILayout.Button(facility, GUILayout.Height(23)))
                        {
                            facType = facility;
                            bChangeFacilityType = false;
                        }
                    }

                }
                GUILayout.EndScrollView();
            }

            if (selectedObject.FacilityType != facType)
            {
                ChangeFacility();
                updateSelection();
            }
            if (facType != "None") {
                OpenCloseFields();
            }

            switch (selectedObject.facilityType)
            {
                case KKFacilityType.Barracks:
                    {
                        MaxStaffFields();
                    }
                    break;
                case KKFacilityType.Business:
                    {
                        BusinessFields();
                    }
                    break;
                case KKFacilityType.FuelTanks:
                    {
                        FuelTankFields();
                    }
                    break;
                case KKFacilityType.GroundStation:
                    {
                        GroundStationFields();
                    }
                    break;
                case KKFacilityType.Hangar:
                    {
                        HangarFields();
                    }
                    break;
                case KKFacilityType.Research:
                    {
                        ResearchFields();
                    }
                    break;
                case KKFacilityType.Merchant:
                    {
                        MerchantFields();
                    }
                    break;
                default:
                    break;

            }        

            GUILayout.FlexibleSpace();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Facility", GUILayout.Width(115), GUILayout.Height(23)))
                {
                    MiscUtils.HUDMessage("Applied changes to object.", 10, 2);
                    updateSettings();
                    updateSelection();
                    selectedObject.SaveConfig();
                    this.Close();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(115), GUILayout.Height(23)))
                {
                    Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }


        #endregion
        #region subwindows


        /// <summary>
        /// Merchant Subwindow 
        /// </summary>
        private void MerchantFields()
        {
            GUI.enabled = true;
            Merchant facMerchant = selectedObject.myFacilities[0] as Merchant;
            GUILayout.Space(4);
            if (GUILayout.Button("Add Resources", GUILayout.Width(100), GUILayout.Height(23)))
            {
                showResourceSelection = true;
            }
            GUILayout.Space(4);
            if (showResourceSelection)
            {
                resourceScrollPos = GUILayout.BeginScrollView(resourceScrollPos);
                {
                    if (GUILayout.Button("Cancel - No change", GUILayout.Height(23)))
                    {
                        showResourceSelection = false;
                    }


                    foreach (PartResourceDefinition availableResource in PartResourceLibrary.Instance.resourceDefinitions )
                    {
                        if (facMerchant.tradedResources.Where(x => x.resource.name.Equals(availableResource.name)).FirstOrDefault() != null )
                        {
                            continue;
                        }

                        if (GUILayout.Button(availableResource.name, GUILayout.Height(23)))
                        {
                            facMerchant.tradedResources.Add(new TradedResource() { resource = availableResource });
                            showResourceSelection = false;
                        }
                    }
                }
                GUILayout.EndScrollView();
            } else {
                if (facMerchant.tradedResources.Count > 0)
                {
                    merchantScrollPos = GUILayout.BeginScrollView(merchantScrollPos);
                    {
                        foreach (TradedResource tradedResource in facMerchant.tradedResources)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("X", redButton, GUILayout.Width(18), GUILayout.Height(18)))
                                {
                                    facMerchant.tradedResources.Remove(tradedResource);
                                }
                                GUILayout.Label("Name: ", GUILayout.Height(18));
                                GUILayout.Label(tradedResource.resource.name, LabelWhite, GUILayout.Height(18), GUILayout.Width(120));
                                GUILayout.Space(20);

                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Buying mult: ", GUILayout.Width(100), GUILayout.Height(18));
                                tradedResource.multiplierBuy = float.Parse(GUILayout.TextField(tradedResource.multiplierBuy.ToString(), 6, GUILayout.Width(50), GUILayout.Height(18)));
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("can be bought: ", GUILayout.Width(110), GUILayout.Height(18));
                                tradedResource.canBeBought = GUILayout.Toggle(tradedResource.canBeBought, "buyable", GUILayout.Height(18));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Selling mult: ", GUILayout.Width(100), GUILayout.Height(18));
                                tradedResource.multiplierSell = float.Parse(GUILayout.TextField(tradedResource.multiplierSell.ToString(), 6, GUILayout.Width(50), GUILayout.Height(18)));
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("can be sold: ", GUILayout.Width(110), GUILayout.Height(18));
                                tradedResource.canBeSold = GUILayout.Toggle(tradedResource.canBeSold, "sellable", GUILayout.Height(18));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(12);
                        }
                    }
                    GUILayout.EndScrollView();

                }

            }
            GUILayout.FlexibleSpace();

        }


        /// <summary>
        /// GroundStation Subwindow
        /// </summary>
        private void GroundStationFields()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Antenna Range: ", LabelGreen);
            GUILayout.FlexibleSpace();
            infTrackingShort = GUILayout.TextField(infTrackingShort, 15, GUILayout.Width(130), GUILayout.Height(18));
            GUILayout.Label("m", LabelWhite);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Paints the Hangar SubWindow
        /// </summary>
        private void HangarFields()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Craft Mass: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infFacMassCap = GUILayout.TextField(infFacMassCap, 4, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("T", LabelWhite, GUILayout.Width(20));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Craft: ", LabelGreen);
                GUILayout.FlexibleSpace();
                GUILayout.Label("" + infFacCraftCap, LabelWhite, GUILayout.Width(30));

                if (GUILayout.Button("1", GUILayout.Width(23), GUILayout.Height(23)))
                {
                    infFacCraftCap = "1";
                }
                if (GUILayout.Button("2", GUILayout.Width(23), GUILayout.Height(23)))
                {
                    infFacCraftCap = "2";
                }

                if (GUILayout.Button("3", GUILayout.Width(23), GUILayout.Height(23)))
                {
                    infFacCraftCap = "3";
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// FuelTank SubWindow
        /// </summary>
        private void FuelTankFields()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max LiquidFuel: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infLqFMax = GUILayout.TextField(infLqFMax, 6, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Oxidizer: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infOxFMax = GUILayout.TextField(infOxFMax, 6, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Monoprop: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infMoFMax = GUILayout.TextField(infMoFMax, 6, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Basic OpenClose Subwindow
        /// </summary>
        private void OpenCloseFields()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Open Cost: ");
                GUILayout.FlexibleSpace();
                infOpenCost = GUILayout.TextField(infOpenCost, 6, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("\\F", LabelWhite);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Default State: ");
                GUILayout.Label(selectedObject.myFacilities.First().defaultState, LabelWhite , GUILayout.Height(23), GUILayout.Width(50));

                isDefaultOpen = GUILayout.Toggle(isDefaultOpen, "", GUILayout.Height(18));
                if (isDefaultOpen)
                {
                    selectedObject.myFacilities.First().defaultState = "Open";
                }
                else
                {
                    selectedObject.myFacilities.First().defaultState = "Closed";
                }
                GUILayout.FlexibleSpace();
                //defaultOpenState = GUILayout.TextField(defaultOpenState, 6, GUILayout.Width(130), GUILayout.Height(18));
                //GUILayout.Label("Open|Closed", LabelWhite);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("FacilityName: ");
                GUILayout.FlexibleSpace();
                selectedObject.myFacilities[0].FacilityName = GUILayout.TextField(selectedObject.myFacilities[0].FacilityName, 30, GUILayout.Width(250), GUILayout.Height(18));
            }

            GUILayout.EndHorizontal();
        }



        /// <summary>
        /// Business SubWindow
        /// </summary>
        private void BusinessFields()
        {
        
            MaxStaffFields();
            ProductionRateFields();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Funds: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infFundsMax = GUILayout.TextField(infFundsMax, 6, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Research Subwindow
        /// </summary>
        private void ResearchFields()
        {
            MaxStaffFields();
            ProductionRateFields();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Science: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infScienceMax = GUILayout.TextField(infScienceMax, 3, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();
        }


        /// <summary>
        /// MaxStaff Subwindow
        /// </summary>
        private void MaxStaffFields()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Max Staff: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infStaffMax = GUILayout.TextField(infStaffMax, 2, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Production Rate Subwindow
        /// </summary>
        private void ProductionRateFields()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Production Rate: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infProdRateMax = GUILayout.TextField(infProdRateMax, 5, GUILayout.Width(150), GUILayout.Height(18));
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Amount produced every Kerbin day. production rate is multiplied by current number of staff.", LabelWhite);

        }

        #endregion

        /// <summary>
        /// Initialize the Layout only once
        /// </summary>
        private void InitializeLayout()
        {
            KKWindows = new GUIStyle(GUI.skin.window);
            KKWindows.padding = new RectOffset(8, 8, 3, 3);

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.yellow;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.yellow;
            DeadButton.focused.textColor = Color.yellow;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Normal;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;

            redButton = new GUIStyle(GUI.skin.button);
            redButton.normal.textColor = Color.red;
            redButton.hover.textColor = Color.red;
            redButton.active.textColor = Color.red;
            redButton.focused.textColor = Color.red;

            LabelGreen = new GUIStyle(GUI.skin.label);
            LabelGreen.normal.textColor = Color.green;
            LabelGreen.fontSize = 13;
            LabelGreen.fontStyle = FontStyle.Bold;
            LabelGreen.padding.bottom = 1;
            LabelGreen.padding.top = 1;

            LabelWhite = new GUIStyle(GUI.skin.label);
            LabelWhite.normal.textColor = Color.white;
            LabelWhite.fontSize = 13;
            LabelWhite.fontStyle = FontStyle.Normal;
            LabelWhite.padding.bottom = 1;
            LabelWhite.padding.top = 1;
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
                //Log.UserError("Unknown Facility Type: " + cfgNode.GetValue("FacilityType") + " in file: " + instance.configPath );
                return;
            }

            selectedObject.facilityType = newType;
            selectedObject.FacilityType = facType;

            if (newType == KKFacilityType.None)
            {
                return;
            }
            // use a face cfgnode for initialization
            ConfigNode cfgNode = new ConfigNode();

            switch (newType)
            {
                case KKFacilityType.Merchant:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Merchant>().ParseConfig(cfgNode));
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
            }
            Log.Normal("FacEditor: Change Finished");
            foreach (var facility in selectedObject.myFacilities)
            {
                facility.staticInstance = selectedObject;
            }
        }


        /// <summary>
        /// Push the Values in the c# Objects
        /// </summary>
        public void updateSettings()
        {
            if (facType != selectedObject.FacilityType)
            {
                ChangeFacility();
            }

            if (facType != "")
                selectedObject.FacilityType =  facType;

            switch (facType)
            {
                case "None":
                    break;
                case "GroundStation":                   
                    ((GroundStation)selectedObject.myFacilities[0]).TrackingShort = float.Parse(infTrackingShort);
                    break;
                case "Hangar":
                    if (infFacMassCap != "") ((Hangar)selectedObject.myFacilities[0]).FacilityMassCapacity =  float.Parse(infFacMassCap);
                    if (infFacCraftCap != "") ((Hangar)selectedObject.myFacilities[0]).FacilityCraftCapacity =  int.Parse(infFacCraftCap);
                    break;
                case "Barracks":
                    if (infStaffMax != "") ((Barracks)selectedObject.myFacilities[0]).StaffMax = int.Parse(infStaffMax);
                    break;
                case "Research":
                    if (infStaffMax != "") ((Research)selectedObject.myFacilities[0]).StaffMax = int.Parse(infStaffMax);
                    if (infProdRateMax != "") ((Research)selectedObject.myFacilities[0]).ProductionRateMax = float.Parse(infProdRateMax);
                    if (infScienceMax != "") ((Research)selectedObject.myFacilities[0]).ScienceOMax = float.Parse(infScienceMax);
                    break;
                case "Business":
                    if (infStaffMax != "") ((Business)selectedObject.myFacilities[0]).StaffMax = int.Parse(infStaffMax);
                    if (infProdRateMax != "") ((Business)selectedObject.myFacilities[0]).ProductionRateMax = float.Parse(infProdRateMax);
                    if (infFundsMax != "") ((Business)selectedObject.myFacilities[0]).FundsOMax = float.Parse(infFundsMax);
                    break;

                default:
                    break;
            }

            if (facType != "None" )
            {
                selectedObject.myFacilities[0].OpenCost = float.Parse(infOpenCost);
                selectedObject.myFacilities[0].CloseValue = (float)Math.Round((selectedObject.myFacilities[0].OpenCost / 4), 0);
            }

            foreach (var facility in selectedObject.myFacilities)
            {
                facility.staticInstance = selectedObject;
            }

        }

        /// <summary>
        /// Load the Facility Objects Values in the placeholders
        /// </summary>
        private void updateSelection()
        {
            if (selectedObject.hasFacilities )
            {
                facType = selectedObject.myFacilities[0].FacilityType;
            } else
            {
                facType = "None";
            }


            if (facType == null || facType == "")
            {
                facType = selectedObject.model.DefaultFacilityType;

                if (facType == null || facType == "None" || facType == "")
                    facType = "None";
            }


            if (facType != "None" && selectedObject.myFacilities[0].defaultState == "Open")
            {
                isDefaultOpen = true;
            } else
            {
                isDefaultOpen = false;
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
                if (String.IsNullOrEmpty(selectedObject.myFacilities[0].FacilityName))
                {
                    selectedObject.myFacilities[0].FacilityName = selectedObject.myFacilities[0].FacilityType + "@" + selectedObject.model.name;
                }

                infOpenCost = selectedObject.myFacilities[0].OpenCost.ToString();

                if (infOpenCost == "0" || infOpenCost == "")
                {
                    infOpenCost = selectedObject.model.cost.ToString();
                }
            } else
            {
                infOpenCost = "0";
            }
            
            //selectedObject.update();
            facType = selectedObject.FacilityType;
        }

    }
}

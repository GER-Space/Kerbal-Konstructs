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
        private bool guiInitialized = false;
        private Rect facilityEditorRect = new Rect(400, 45, 360, 370);

        private GUIStyle LabelGreen;
        private GUIStyle LabelWhite;

        private GUIStyle DeadButton;
        private GUIStyle DeadButtonRed;
        private GUIStyle KKWindows;
        private GUIStyle BoxNoBorder;

        private Vector2 facilityscroll;

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

        private string defaultOpenState;





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
                bChangeFacilityType = true;

            if (bChangeFacilityType)
            {
                facilityscroll = GUILayout.BeginScrollView(facilityscroll);
                if (GUILayout.Button("Cancel - No change", GUILayout.Height(23)))
                    bChangeFacilityType = false;

                if (GUILayout.Button("None", GUILayout.Height(23)))
                {
                    facType = "None";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Barracks", GUILayout.Height(23)))
                {
                    facType = "Barracks";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Business", GUILayout.Height(23)))
                {
                    facType = "Business";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Fuel Tanks", GUILayout.Height(23)))
                {
                    facType = "FuelTanks";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Hangar", GUILayout.Height(23)))
                {
                    facType = "Hangar";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Research", GUILayout.Height(23)))
                {
                    facType = "Research";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Ground Station", GUILayout.Height(23)))
                {
                    facType = "GroundStation";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("City Lights", GUILayout.Height(23)))
                {
                    facType = "CityLights";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Landing Guide", GUILayout.Height(23)))
                {
                    facType = "LandingGuide";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Touchdown R", GUILayout.Height(23)))
                {
                    facType = "TouchdownGuideR";
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Touchdown L", GUILayout.Height(23)))
                {
                    facType = "TouchdownGuideL";
                    bChangeFacilityType = false;
                }

                GUILayout.EndScrollView();
            }
            // Update editor window button
            EditorGUI.facType = facType;
            if (selectedObject.FacilityType != facType)
            {
                ChangeFacility();
                updateSelection();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Open Cost: ", LabelGreen);
            GUILayout.FlexibleSpace();
            infOpenCost = GUILayout.TextField(infOpenCost, 6, GUILayout.Width(130), GUILayout.Height(18));
            GUILayout.Label("\\F", LabelWhite);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Default State: ", LabelGreen);
            GUILayout.FlexibleSpace();
            defaultOpenState = GUILayout.TextField(defaultOpenState, 6, GUILayout.Width(130), GUILayout.Height(18));
            GUILayout.Label("Open|Closed", LabelWhite);
            GUILayout.EndHorizontal();

            if (facType == "GroundStation")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Short Range: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infTrackingShort = GUILayout.TextField(infTrackingShort, 15, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("m", LabelWhite);
                GUILayout.EndHorizontal();
            }


            if (facType == "Barracks" || facType == "Research" || facType == "Business")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Staff: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infStaffMax = GUILayout.TextField(infStaffMax, 2, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (facType == "Research" || facType == "Business")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Production Rate: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infProdRateMax = GUILayout.TextField(infProdRateMax, 5, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();

                GUILayout.Label("Amount produced every 12 hours is production rate multiplied by current number of staff.", LabelWhite);
            }

            if (facType == "Research")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Science: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infScienceMax = GUILayout.TextField(infScienceMax, 3, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (facType == "Business")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Funds: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infFundsMax = GUILayout.TextField(infFundsMax, 6, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (facType == "FuelTanks")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max LiquidFuel: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infLqFMax = GUILayout.TextField(infLqFMax, 6, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Oxidizer: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infOxFMax = GUILayout.TextField(infOxFMax, 6, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Monoprop: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infMoFMax = GUILayout.TextField(infMoFMax, 6, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (facType == "Hangar")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Craft Mass: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infFacMassCap = GUILayout.TextField(infFacMassCap, 4, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("T", LabelWhite, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
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
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Facility", GUILayout.Width(115), GUILayout.Height(23)))
            {
                MiscUtils.HUDMessage("Applied changes to object.", 10, 2);
                updateSettings();
                updateSelection();
                this.Close();

            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(115), GUILayout.Height(23)))
            {
                Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }


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
        /// Parses a string to a double within a range
        /// </summary>
        /// <param name="sText"></param>
        /// <param name="RangeMax"></param>
        /// <param name="RangeMin"></param>
        /// <returns></returns>
        private static bool ValidateStringToDouble(string sText, double RangeMax = 0, double RangeMin = 0)
        {
            double parsedValue;
            bool parsed = double.TryParse(sText, out parsedValue);

            if (parsed)
            {
                if (RangeMax > 0)
                {
                    if (parsedValue > RangeMax) return false;
                }

                if (RangeMin > 0)
                {
                    if (parsedValue < RangeMin) return false;
                }

                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Removes add previous facility settings from Static Object
        /// </summary>
        private void RemoveOldSettings()
        {
            //List<String> allFacSettings = new List<string>()
            //{
            //    "FacilityMassCapacity",
            //    "FacilityCraftCapacity",
            //    "StaffMax",
            //    "LqFMax",
            //    "OxFMax",
            //    "MoFMax",
            //    "ProductionRateMax",
            //    "ScienceOMax",
            //    "FundsOMax"
            //};

            //foreach (String setting in allFacSettings)
            //{
            //    if (selectedObject.settings.ContainsKey(setting))
            //        selectedObject.settings.Remove(setting);
            //}

            if (selectedObject.hasFacilities)
            {

                Log.Normal("FacEditor: Remove called");
                selectedObject.hasFacilities = false;
                GameObject.Destroy(selectedObject.myFacilities[0]);
                selectedObject.myFacilities = new List<KKFacility>();
                Log.Normal("FacEditor: Remove Finished");
            }
        }

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
            else
            {
                selectedObject.hasFacilities = true;
            }
            // use a face cfgnode for initialization
            ConfigNode cfgNode = new ConfigNode();

            switch (newType)
            {
                case KKFacilityType.GroundStation:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    break;
                case KKFacilityType.TrackingStation:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<GroundStation>().ParseConfig(cfgNode));
                    selectedObject.facilityType = KKFacilityType.GroundStation;
                    break;
                case KKFacilityType.FuelTanks:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<FuelTanks>().ParseConfig(cfgNode));
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
                case KKFacilityType.RadarStation:
                    selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<RadarStation>().ParseConfig(cfgNode));
                    break;
            }
            Log.Normal("FacEditor: Change Finished");
        }


        public void updateSettings()
        {
            bool needNewFacility = false;
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
                case "FuelTanks":
                    if (needNewFacility) 
                        selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<FuelTanks>());
                    selectedObject.hasFacilities = true;
                    if (infLqFMax != "") ((FuelTanks)selectedObject.myFacilities[0]).LqFMax = float.Parse(infLqFMax);
                    if (infOxFMax != "") ((FuelTanks)selectedObject.myFacilities[0]).OxFMax = float.Parse(infOxFMax);
                    if (infMoFMax != "") ((FuelTanks)selectedObject.myFacilities[0]).MoFMax = float.Parse(infMoFMax);
                    break;
                case "GroundStation":
                    if (needNewFacility)
                        selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<GroundStation>());
                    selectedObject.hasFacilities = true;                    
                    ((GroundStation)selectedObject.myFacilities[0]).TrackingShort = float.Parse(infTrackingShort);
                    break;
                case "Hangar":
                    if (needNewFacility)
                        selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Hangar>());
                    selectedObject.hasFacilities = true;
                    if (infFacMassCap != "") ((Hangar)selectedObject.myFacilities[0]).FacilityMassCapacity =  float.Parse(infFacMassCap);
                    if (infFacCraftCap != "") ((Hangar)selectedObject.myFacilities[0]).FacilityCraftCapacity =  int.Parse(infFacCraftCap);
                    break;
                case "Barracks":
                    if (needNewFacility)
                        selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Barracks>());
                    selectedObject.hasFacilities = true;
                    if (infStaffMax != "") ((Barracks)selectedObject.myFacilities[0]).StaffMax = int.Parse(infStaffMax);
                    break;
                case "Research":
                    if (needNewFacility)
                        selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Research>());
                    selectedObject.hasFacilities = true;
                    if (infStaffMax != "") ((Research)selectedObject.myFacilities[0]).StaffMax = int.Parse(infStaffMax);
                    if (infProdRateMax != "") ((Research)selectedObject.myFacilities[0]).ProductionRateMax = float.Parse(infProdRateMax);
                    if (infScienceMax != "") ((Research)selectedObject.myFacilities[0]).ScienceOMax = float.Parse(infScienceMax);
                    break;
                case "Business":
                    if (needNewFacility)
                        selectedObject.myFacilities.Add(selectedObject.gameObject.AddComponent<Business>());
                    selectedObject.hasFacilities = true;
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
                selectedObject.myFacilities[0].CloseValue = selectedObject.myFacilities[0].OpenCost / 4;
                selectedObject.myFacilities[0].defaultState = defaultOpenState;
            }

        }


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
                case "FuelTanks":
                    infLqFMax = ((FuelTanks)selectedObject.myFacilities[0]).LqFMax.ToString();
                    if (infLqFMax == "0" || infLqFMax == "")
                        infLqFMax = selectedObject.model.LqFMax.ToString();
                    infOxFMax = ((FuelTanks)selectedObject.myFacilities[0]).OxFMax.ToString();
                    if (infOxFMax == "0" || infOxFMax == "")
                        infOxFMax = selectedObject.model.OxFMax.ToString();
                    infMoFMax = ((FuelTanks)selectedObject.myFacilities[0]).MoFMax.ToString();
                    if (infMoFMax == "0" || infMoFMax == "")
                        infMoFMax = selectedObject.model.MoFMax.ToString();
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
                defaultOpenState = selectedObject.myFacilities[0].defaultState;
                infOpenCost = selectedObject.myFacilities[0].OpenCost.ToString();
                if (infOpenCost == "0" || infOpenCost == "")
                    infOpenCost = selectedObject.model.cost.ToString();
            } else
            {
                infOpenCost = "0";
            }
            
            //selectedObject.update();
            facType = selectedObject.FacilityType;
        }

    }
}

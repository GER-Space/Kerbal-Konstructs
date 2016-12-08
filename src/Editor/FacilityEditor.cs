using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;
using UnityEngine.UI;

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

        StaticObject selectedObject = null;

        private Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

        private bool bChangeFacilityType = false;
        private static String facType = "None";
        private string infTrackingShort, infTrackingAngle, infOpenCost, infStaffMax, infProdRateMax, infScienceMax, infFundsMax = "";

        String infFacMassCap = "";
        String infFacCraftCap = "";
        String infLqFMax = "";
        String infOxFMax = "";
        String infMoFMax = "";





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
            Log.Normal("FacEditor Open Called");
            base.Open();
        }

        public override void Close()
        {
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

            GUILayout.Box((string)selectedObject.model.getSetting("title"));
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

                if (GUILayout.Button("Tracking Station", GUILayout.Height(23)))
                {
                    facType = "TrackingStation";
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("Open Cost: ", LabelGreen);
            GUILayout.FlexibleSpace();
            infOpenCost = GUILayout.TextField(infOpenCost, 6, GUILayout.Width(130), GUILayout.Height(18));
            GUILayout.Label("\\F", LabelWhite);
            GUILayout.EndHorizontal();

            if (facType == "TrackingStation")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Short Range: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infTrackingShort = GUILayout.TextField(infTrackingShort, 15, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("m", LabelWhite);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Angle: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infTrackingAngle = GUILayout.TextField(infTrackingAngle, 3, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("°", LabelWhite);
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
            List<String> allFacSettings = new List<string>()
            {
                "FacilityMassCapacity",
                "FacilityCraftCapacity",
                "StaffMax",
                "LqFMax",
                "OxFMax",
                "MoFMax",
                "ProductionRateMax",
                "ScienceOMax",
                "FundsOMax"
            } ;

            foreach (String setting in allFacSettings)
            {
                if (selectedObject.settings.ContainsKey(setting))
                    selectedObject.settings.Remove(setting);
            }
                        
        }


        public void updateSettings()
        {
            RemoveOldSettings();

            if (facType != "") selectedObject.setSetting("FacilityType", facType);

            switch (facType)
            {
                case "None":
                    break;
                case "FuelTanks":
                    if (infLqFMax != "") selectedObject.setSetting("LqFMax", float.Parse(infLqFMax));
                    if (infOxFMax != "") selectedObject.setSetting("OxFMax", float.Parse(infOxFMax));
                    if (infMoFMax != "") selectedObject.setSetting("MoFMax", float.Parse(infMoFMax));
                    break;
                case "TrackingStation":
                    selectedObject.setSetting("TrackingShort", float.Parse(infTrackingShort));
                    selectedObject.setSetting("TrackingAngle", float.Parse(infTrackingAngle));
                    break;
                case "Hangar":
                    if (infFacMassCap != "") selectedObject.setSetting("FacilityMassCapacity", float.Parse(infFacMassCap));
                    if (infFacCraftCap != "") selectedObject.setSetting("FacilityCraftCapacity", float.Parse(infFacCraftCap));
                    break;
                case "Barracks":
                    if (infStaffMax != "") selectedObject.setSetting("StaffMax", float.Parse(infStaffMax));
                    break;
                case "Research":
                    if (infStaffMax != "") selectedObject.setSetting("StaffMax", float.Parse(infStaffMax));
                    if (infProdRateMax != "") selectedObject.setSetting("ProductionRateMax", float.Parse(infProdRateMax));
                    if (infScienceMax != "") selectedObject.setSetting("ScienceOMax", float.Parse(infScienceMax));
                    break;
                case "Business":
                    if (infStaffMax != "") selectedObject.setSetting("StaffMax", float.Parse(infStaffMax));
                    if (infProdRateMax != "") selectedObject.setSetting("ProductionRateMax", float.Parse(infProdRateMax));
                    if (infFundsMax != "") selectedObject.setSetting("FundsOMax", float.Parse(infFundsMax));
                    break;

                default:
                    break;
            }
        }


        private void updateSelection()
        {

            infTrackingShort = selectedObject.getSetting("TrackingShort").ToString();

            infTrackingAngle = selectedObject.getSetting("TrackingAngle").ToString();


            infFacMassCap = selectedObject.getSetting("FacilityMassCapacity").ToString();
            if (infFacMassCap == "0" || infFacMassCap == "")
                infFacMassCap = selectedObject.model.getSetting("DefaultFacilityMassCapacity").ToString();

            infFacCraftCap = selectedObject.getSetting("FacilityCraftCapacity").ToString();
            if (infFacCraftCap == "0" || infFacCraftCap == "")
                infFacCraftCap = selectedObject.model.getSetting("DefaultFacilityCraftCapacity").ToString();

            infLqFMax = selectedObject.getSetting("LqFMax").ToString();
            if (infLqFMax == "0" || infLqFMax == "")
                infLqFMax = selectedObject.model.getSetting("LqFMax").ToString();

            infOxFMax = selectedObject.getSetting("OxFMax").ToString();
            if (infOxFMax == "0" || infOxFMax == "")
                infOxFMax = selectedObject.model.getSetting("OxFMax").ToString();

            infMoFMax = selectedObject.getSetting("MoFMax").ToString();
            if (infMoFMax == "0" || infMoFMax == "")
                infMoFMax = selectedObject.model.getSetting("MoFMax").ToString();

            infOpenCost = selectedObject.getSetting("OpenCost").ToString();
            if (infOpenCost == "0" || infOpenCost == "")
                infOpenCost = selectedObject.model.getSetting("cost").ToString();

            infStaffMax = selectedObject.getSetting("StaffMax").ToString();
            if (infStaffMax == "0" || infStaffMax == "")
                infStaffMax = selectedObject.model.getSetting("DefaultStaffMax").ToString();

            infProdRateMax = selectedObject.getSetting("ProductionRateMax").ToString();
            if (infProdRateMax == "0" || infProdRateMax == "")
                infProdRateMax = selectedObject.model.getSetting("DefaultProductionRateMax").ToString();

            infScienceMax = selectedObject.getSetting("ScienceOMax").ToString();
            if (infScienceMax == "0" || infScienceMax == "")
                infScienceMax = selectedObject.model.getSetting("DefaultScienceOMax").ToString();

            infFundsMax = selectedObject.getSetting("FundsOMax").ToString();
            if (infFundsMax == "0" || infFundsMax == "")
                infFundsMax = selectedObject.model.getSetting("DefaultFundsOMax").ToString();

            facType = (string)selectedObject.getSetting("FacilityType");

            if (facType == null || facType == "")
            {
                facType = (string)selectedObject.model.getSetting("DefaultFacilityType");

                if (facType == null || facType == "None" || facType == "")
                    facType = "None";

            }
            selectedObject.update();
        }

    }
}

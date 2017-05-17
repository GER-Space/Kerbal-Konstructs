using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
    class ModelInfo : KKWindow
    {
        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

        //		public PQSCity pqsCity;

        public double dUpdater = 0;

        Rect StaticInfoRect = new Rect(300, 50, 320, 620);

        public Boolean displayingInfo = false;
        public Boolean bCycle = true;
        public Boolean bSpinning = true;
        public Boolean bChangeFacilityType = false;

        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle KKWindow;
        GUIStyle BoxNoBorder;
        GUIStyle BoxNoBorder2;
        GUIStyle LabelGreen;
        GUIStyle LabelWhite;

        Vector2 facilityscroll;

        String infTitle = "";
        String infMesh = "";
        String infCost = "";
        String infAuthor = "";
        String infManufacturer = "";
        String infDescription = "";
        String infCategory = "";
        String infLaunchTransform = "";

        String infLaunchLength = "";
        String infLaunchWidth = "";
        String infFacType = "";
        String infFacMassCap = "";
        String infFacCraftCap = "";
        String infStaffMax = "";
        String infLqFMax = "";
        String infOxFMax = "";
        String infMoFMax = "";
        //		String infECMax = "";
        //		String infOreMax = "";
        //		String infPrOreMax = "";
        String infProdRateMax = "";
        String infScienceMax = "";
        String infFundsMax = "";

        public StaticModel mModel = null;
        public StaticObject lastPreview = null;
        public static StaticObject currPreview = null;

        public GameObject gModel = null;

        public override void Draw()
        {
            if (!MapView.MapIsEnabled)
                drawModelInfoGUI(KerbalKonstructs.instance.selectedModel);
        }


        public void drawModelInfoGUI(StaticModel mSelectedModel)
        {

            if (mSelectedModel != null)
            {
                if (mModel != mSelectedModel)
                {
                    if (currPreview != null)
                        DestroyPreviewInstance(currPreview);

                    updateSelection(mSelectedModel);

                    if (KerbalKonstructs.instance.spawnPreviewModels)
                        CreatePreviewInstance(mSelectedModel);
                }

                mModel = mSelectedModel;

                KKWindow = new GUIStyle(GUI.skin.window);
                KKWindow.padding = new RectOffset(8, 8, 3, 3);

                StaticInfoRect = GUI.Window(0xD00B1E2, StaticInfoRect, drawStaticInfoWindow, "", KKWindow);
            }
        }

        public void drawStaticInfoWindow(int WindowID)
        {
            if (mModel == null) return;

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.green;
            BoxNoBorder.fontSize = 13;
            BoxNoBorder.fontStyle = FontStyle.Bold;

            BoxNoBorder2 = new GUIStyle(GUI.skin.box);
            BoxNoBorder2.normal.background = null;
            BoxNoBorder2.normal.textColor = Color.white;
            BoxNoBorder2.fontSize = 13;
            BoxNoBorder2.fontStyle = FontStyle.Normal;

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

            if (currPreview != null)
            {
                double dTicker = Planetarium.GetUniversalTime();
                if ((dTicker - dUpdater) > 0.01)
                {
                    dUpdater = Planetarium.GetUniversalTime();

                    if (bSpinning)
                        SpinPreview(currPreview);
                }
            }

            bool shouldUpdateSelection = false;
            string smessage = "";

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Static Model Config Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    if (currPreview != null)
                        DestroyPreviewInstance(currPreview);

                    KerbalKonstructs.GUI_ModelInfo.Close();
                    mModel = null;
                    KerbalKonstructs.instance.selectedModel = null;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.Box(" " + infTitle + " ", GUILayout.Height(20));
            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);

            if (! string.IsNullOrEmpty(infLaunchTransform))
            {
                GUILayout.Box("DefaultLaunchPadTransform", BoxNoBorder, GUILayout.Height(19));
                GUILayout.Box("" + infLaunchTransform, BoxNoBorder2);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Length: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infLaunchLength = GUILayout.TextField(infLaunchLength, 5, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("m", LabelWhite, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Width: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infLaunchWidth = GUILayout.TextField(infLaunchWidth, 5, GUILayout.Width(130), GUILayout.Height(18));
                GUILayout.Label("m", LabelWhite, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(1);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mesh: ", LabelGreen);
            GUILayout.FlexibleSpace();
            GUILayout.Label("" + infMesh + ".mu", LabelWhite);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Manufacturer: ", LabelGreen);
            GUILayout.FlexibleSpace();
            GUILayout.Label("" + infManufacturer, LabelWhite);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Author: ", LabelGreen);
            GUILayout.FlexibleSpace();
            GUILayout.Label("" + infAuthor, LabelWhite);
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Category: ", LabelGreen);
            GUILayout.FlexibleSpace();
            infCategory = GUILayout.TextField(infCategory, GUILayout.Width(150), GUILayout.Height(18));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Cost: ", LabelGreen);
            GUILayout.FlexibleSpace();
            infCost = GUILayout.TextField(infCost, GUILayout.Width(150), GUILayout.Height(18));
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);

            if (GUILayout.Button("Facility Type: " + infFacType, GUILayout.Height(23)))
                bChangeFacilityType = true;

            if (bChangeFacilityType)
            {
                facilityscroll = GUILayout.BeginScrollView(facilityscroll);
                if (GUILayout.Button("Cancel - No change", GUILayout.Height(23)))
                    bChangeFacilityType = false;

                if (GUILayout.Button("Barracks", GUILayout.Height(23)))
                {
                    infFacType = "Barracks";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Business", GUILayout.Height(23)))
                {
                    infFacType = "Business";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Fuel Tanks", GUILayout.Height(23)))
                {
                    infFacType = "FuelTanks";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Hangar", GUILayout.Height(23)))
                {
                    infFacType = "Hangar";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Research", GUILayout.Height(23)))
                {
                    infFacType = "Research";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Tracking Station", GUILayout.Height(23)))
                {
                    infFacType = "TrackingStation";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("City Lights", GUILayout.Height(23)))
                {
                    infFacType = "CityLights";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                if (GUILayout.Button("Landing Guide", GUILayout.Height(23)))
                {
                    infFacType = "LandingGuide";
                    shouldUpdateSelection = true;
                    bChangeFacilityType = false;
                }

                GUILayout.EndScrollView();
            }

            if (infFacType == "Barracks" || infFacType == "Research" || infFacType == "Business")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Staff: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infStaffMax = GUILayout.TextField(infStaffMax, 2, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (infFacType == "Research" || infFacType == "Business")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Production Rate: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infProdRateMax = GUILayout.TextField(infProdRateMax, 5, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();

                GUILayout.Label("Amount produced every 12 hours is production rate multiplied by current number of staff.", LabelWhite);
            }

            if (infFacType == "Research")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Science: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infScienceMax = GUILayout.TextField(infScienceMax, 3, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (infFacType == "Business")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Funds: ", LabelGreen);
                GUILayout.FlexibleSpace();
                infFundsMax = GUILayout.TextField(infFundsMax, 6, GUILayout.Width(150), GUILayout.Height(18));
                GUILayout.EndHorizontal();
            }

            if (infFacType == "FuelTanks")
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

            if (infFacType == "Hangar")
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
                    shouldUpdateSelection = true;
                }
                if (GUILayout.Button("2", GUILayout.Width(23), GUILayout.Height(23)))
                {
                    infFacCraftCap = "2";
                    shouldUpdateSelection = true;
                }

                if (GUILayout.Button("3", GUILayout.Width(23), GUILayout.Height(23)))
                {
                    infFacCraftCap = "3";
                    shouldUpdateSelection = true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);
            GUILayout.Box("Description", BoxNoBorder, GUILayout.Height(19));
            infDescription = GUILayout.TextArea(infDescription, GUILayout.Height(50));

            GUILayout.FlexibleSpace();
            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Height(23), GUILayout.Width(120)))
            {
                updateSettings(mModel);
                KerbalKonstructs.instance.saveModelConfig(mModel);
                smessage = "Saved changes to static models config.";
                MiscUtils.HUDMessage(smessage, 10, 2);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Height(23), GUILayout.Width(120)))
            {
                if (currPreview != null)
                    DestroyPreviewInstance(currPreview);

                KerbalKonstructs.GUI_ModelInfo.Close();
                mModel = null;
                KerbalKonstructs.instance.selectedModel = null;
            }
            GUILayout.EndHorizontal();

            if (currPreview != null)
            {
                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(1);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Delete Preview", GUILayout.Height(23), GUILayout.Width(120)))
                    DestroyPreviewInstance(currPreview);

                GUILayout.FlexibleSpace();

                if (bSpinning)
                {
                    if (GUILayout.Button("Stop Spin", GUILayout.Height(23), GUILayout.Width(120)))
                        bSpinning = false;
                }
                else
                {
                    if (GUILayout.Button("Resume Spin", GUILayout.Height(23), GUILayout.Width(120)))
                        bSpinning = true;
                }

                GUILayout.EndHorizontal();
            }

            if (Event.current.keyCode == KeyCode.Return)
            {
                MiscUtils.HUDMessage("Applied changes to object.", 10, 2);
                shouldUpdateSelection = true;
            }

            if (shouldUpdateSelection)
            {
                updateSettings(mModel);
                updateSelection(mModel);
            }

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        public void updateSettings(StaticModel mModel)
        {
            mModel.author = infAuthor;
            mModel.manufacturer = infManufacturer;
            mModel.cost = float.Parse(infCost);
            mModel.description = infDescription;
            mModel.title = infTitle;
            mModel.category = infCategory;

            if (infFacType != "") mModel.DefaultFacilityType = infFacType;

            if (infLaunchLength != "") mModel.DefaultLaunchSiteLength =  float.Parse(infLaunchLength);
            if (infLaunchWidth != "") mModel.DefaultLaunchSiteWidth = float.Parse(infLaunchWidth);
            if (infFacMassCap != "") mModel.DefaultFacilityMassCapacity = float.Parse(infFacMassCap);
            if (infFacCraftCap != "") mModel.DefaultFacilityCraftCapacity = int.Parse(infFacCraftCap);
            if (infStaffMax != "") mModel.DefaultStaffMax = int.Parse(infStaffMax);
            if (infLqFMax != "") mModel.LqFMax = float.Parse(infLqFMax);
            if (infOxFMax != "") mModel.OxFMax = float.Parse(infOxFMax);
            if (infMoFMax != "") mModel.MoFMax = float.Parse(infMoFMax);
            //	if (infECMax != "") mModel.setSetting("ECMax", float.Parse(infECMax));
            //	if (infOreMax != "") mModel.setSetting("OreMax", float.Parse(infOreMax));
            //	if (infPrOreMax != "") mModel.setSetting("PrOreMax", float.Parse(infPrOreMax));
            if (infProdRateMax != "") mModel.DefaultProductionRateMax = float.Parse(infProdRateMax);
            if (infScienceMax != "") mModel.DefaultScienceOMax = float.Parse(infScienceMax);
            if (infFundsMax != "") mModel.DefaultFundsOMax = float.Parse(infFundsMax);
        }

        public void updateSelection(StaticModel obj)
        {
            infAuthor = obj.author;
            infMesh = obj.mesh;
            infManufacturer = obj.manufacturer;
            infCost = obj.cost.ToString();
            infDescription = obj.description;
            infTitle = obj.title;
            infCategory = obj.category;
            infLaunchTransform = obj.DefaultLaunchPadTransform;

            infLaunchLength = obj.DefaultLaunchSiteLength.ToString();
            infLaunchWidth = obj.DefaultLaunchSiteWidth.ToString();
            infFacType = obj.DefaultFacilityType;
            infFacMassCap = obj.DefaultFacilityMassCapacity.ToString();
            infFacCraftCap = obj.DefaultFacilityCraftCapacity.ToString();
            infStaffMax = obj.DefaultStaffMax.ToString();
            infLqFMax = obj.LqFMax.ToString();
            infOxFMax = obj.OxFMax.ToString();
            infMoFMax = obj.MoFMax.ToString();
            //	infECMax = obj.getSetting("ECMax").ToString();
            //	infOreMax = obj.getSetting("OreMax").ToString();
            //	infPrOreMax = obj.getSetting("PrOreMax").ToString();
            infProdRateMax = obj.DefaultProductionRateMax.ToString();
            infScienceMax = obj.DefaultScienceOMax.ToString();
            infFundsMax = obj.DefaultFundsOMax.ToString();
        }


        public static void DestroyPreviewInstance(StaticObject soInstance)
        {
            if (soInstance != null)
            {
                if (currPreview != null)
                {
                    if (currPreview == soInstance)
                        currPreview = null;
                }

                KerbalKonstructs.instance.deleteObject(soInstance);

            }
            else
            {
                if (currPreview != null)
                {
                    KerbalKonstructs.instance.deleteObject(currPreview);
                    currPreview = null;
                }
            }
        }

        public void CreatePreviewInstance(StaticModel model)
        {
            StaticObject instance = new StaticObject();
            instance.gameObject = GameObject.Instantiate(model.prefab);
            instance.RadiusOffset = (float)FlightGlobals.ActiveVessel.altitude;
            instance.CelestialBody = KerbalKonstructs.instance.getCurrentBody();
            instance.Group = "Ungrouped";
            instance.RadialPosition = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position);
            instance.RotationAngle = 0f;
            instance.Orientation= Vector3.up;
            instance.VisibilityRange = 25000f;

            instance.model = model;

            instance.spawnObject(true, true);
            // KerbalKonstructs.instance.selectObject(obj, false);
            currPreview = instance;
        }

        public void SpinPreview(StaticObject soObject)
        {
            if (soObject == null || currPreview == null) return;

            float fRot = ((soObject.RotationAngle + 0.1f) % 360);

            soObject.RotationAngle = fRot;
            soObject.update();
        }
    }
}

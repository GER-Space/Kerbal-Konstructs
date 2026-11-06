using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    class LaunchSiteEditor : KKWindow
    {

        private static LaunchSiteEditor _instance = null;

        internal static LaunchSiteEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LaunchSiteEditor();
                }
                return _instance;
            }
        }

        #region Variable Declarations

        private List<Transform> transformList = new List<Transform>();
        private CultureInfo culture = new CultureInfo("en-US");

        #region GUI Windows
        // GUI Windows
        Rect siteEditorRect = new Rect(400, 45, 360, 660);

        #endregion

        #region GUI elements
        // GUI elements
        Vector2 descScroll;
        GUIStyle listStyle = new GUIStyle();
        GUIStyle navStyle = new GUIStyle();

        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle KKWindows;
        GUIStyle BoxNoBorder;

        SiteType siteType;
        GUIContent[] siteTypeOptions = {
                                            new GUIContent("VAB"),
                                            new GUIContent("SPH"),
                                            new GUIContent("ANY")
                                        };
        // ComboBox siteTypeMenu;
        #endregion

        #region Holders
        // Holders

        public static StaticInstance selectedObject = null;

        internal string siteName, siteTrans, siteDesc, siteAuthor, siteHidden, ILSActive;
        float flOpenCost, flCloseValue;

        public bool togglePositioning = false;

        string stOpenCost;
        string stCloseValue;
        string stLength;
        string stWidth;
        string stHeight;
        string sMaxParts, sMaxMass;


        private string initialCameraRotation = "90";

        internal LaunchSiteCategory category = LaunchSiteCategory.Other;

        private bool guiInitialized = false;

        #endregion

        #endregion

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }
            if (KerbalKonstructs.selectedInstance == null)
            {
                this.Close();
            }

            if ((KerbalKonstructs.selectedInstance != null) && (!KerbalKonstructs.selectedInstance.isPreview))
            {
                drawEditor(KerbalKonstructs.selectedInstance);
            }
        }

        public override void Open()
        {
            selectedObject = KerbalKonstructs.selectedInstance;
            ReadSettings();
            base.Open();
        }

        #region draw Methods

        /// <summary>
        /// Wrapper to draw editors
        /// </summary>
        /// <param name="obj"></param>
        public void drawEditor(StaticInstance obj)
        {
            if (!guiInitialized)
            {
                InitializeLayout();
                guiInitialized = true;
            }
            if (obj != null)
            {


                siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawLaunchSiteEditorWindow, "", KKWindows);


            }
        }

        #endregion

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

            listStyle.normal.textColor = Color.white;
            listStyle.onHover.background =
            listStyle.hover.background = new Texture2D(2, 2);
            listStyle.padding.left =
            listStyle.padding.right =
            listStyle.padding.top =
            listStyle.padding.bottom = 4;

            navStyle.padding.left = 0;
            navStyle.padding.right = 0;
            navStyle.padding.top = 1;
            navStyle.padding.bottom = 3;
        }


        #region Editors


        #region Launchsite Editor
        // Launchsite Editor handlers
        /// <summary>
        /// Launchsite Editor
        /// </summary>
        /// <param name="id"></param>
        void drawLaunchSiteEditorWindow(int id)
        {

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Launchsite Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.Box(selectedObject.model.title);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Name: ", GUILayout.Width(120));
            siteName = GUILayout.TextField(siteName, GUILayout.Height(19));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Transform: ", GUILayout.Width(120));
            GUILayout.Box("" + siteTrans);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vessel Length: ", GUILayout.Width(160));
            stLength = GUILayout.TextField(stLength, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vessel Width: ", GUILayout.Width(160));
            stWidth = GUILayout.TextField(stWidth, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vessel Height: ", GUILayout.Width(160));
            stHeight = GUILayout.TextField(stHeight, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vessel Mass: ", GUILayout.Width(160));
            sMaxMass = GUILayout.TextField(sMaxMass, GUILayout.Height(19));
            GUILayout.Label(" t");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vessel Parts: ", GUILayout.Width(160));
            sMaxParts = GUILayout.TextField(sMaxParts, GUILayout.Height(19));
            GUILayout.Label("");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("SpaceCenter Camera rotation: ", GUILayout.Width(220));
            initialCameraRotation = (GUILayout.TextField(initialCameraRotation, GUILayout.Height(19)));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Category: ", GUILayout.Width(115));
            GUILayout.Label(category.ToString(), GUILayout.Width(85));
            GUILayout.FlexibleSpace();
            GUI.enabled = (category != LaunchSiteCategory.RocketPad);
            if (GUILayout.Button("RP", GUILayout.Width(25), GUILayout.Height(23)))
                category = LaunchSiteCategory.RocketPad;
            GUI.enabled = (category != LaunchSiteCategory.Runway);
            if (GUILayout.Button("RW", GUILayout.Width(25), GUILayout.Height(23)))
                category = LaunchSiteCategory.Runway;
            GUI.enabled = (category != LaunchSiteCategory.Helipad);
            if (GUILayout.Button("HP", GUILayout.Width(25), GUILayout.Height(23)))
                category = LaunchSiteCategory.Helipad;
            GUI.enabled = (category != LaunchSiteCategory.Waterlaunch);
            if (GUILayout.Button("WA", GUILayout.Width(25), GUILayout.Height(23)))
                category = LaunchSiteCategory.Waterlaunch;
            GUI.enabled = (category != LaunchSiteCategory.Other);
            if (GUILayout.Button("OT", GUILayout.Width(25), GUILayout.Height(23)))
                category = LaunchSiteCategory.Other;
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Type: ", GUILayout.Width(120));
            GUILayout.Label(siteType.ToString(), GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(siteType == (SiteType)0);
            if (GUILayout.Button("VAB", GUILayout.Height(23)))
                siteType = SiteType.VAB;
            GUI.enabled = !(siteType == (SiteType)1);
            if (GUILayout.Button("SPH", GUILayout.Height(23)))
                siteType = SiteType.SPH;
            GUI.enabled = !(siteType == (SiteType)2);
            if (GUILayout.Button("Any", GUILayout.Height(23)))
                siteType = SiteType.Any;
            GUILayout.EndHorizontal();

            GUI.enabled = true;


            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Toogle positioning behavior:");
                GUILayout.FlexibleSpace();
                togglePositioning = GUILayout.Toggle(togglePositioning, "", GUILayout.Width(40), GUILayout.Height(23));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Author: ", GUILayout.Width(120));
            siteAuthor = GUILayout.TextField(siteAuthor, GUILayout.Height(19));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Open Cost: ", GUILayout.Width(120));
            stOpenCost = GUILayout.TextField(stOpenCost, GUILayout.Height(19));
            GUILayout.Label(" \\F");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Close Value: ", GUILayout.Width(120));
            stCloseValue = GUILayout.TextField(stCloseValue, GUILayout.Height(19));
            GUILayout.Label(" \\F");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site is hidden: ", GUILayout.Width(115));
            GUILayout.Label(siteHidden, GUILayout.Width(85));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(siteHidden == "false");
            if (GUILayout.Button("No", GUILayout.Width(40), GUILayout.Height(23)))
                siteHidden = "false";
            GUI.enabled = !(siteHidden == "true");
            if (GUILayout.Button("Yes", GUILayout.Width(40), GUILayout.Height(23)))
                siteHidden = "true";
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            //if (ILSConfig.DetectNavUtils ()) {
            // NavUtilities config generator
            GUILayout.BeginHorizontal();
            GUILayout.Label("ILS/HSI on (NavUtilities)", GUILayout.Width(115));
            GUILayout.Label(ILSActive, GUILayout.Width(85));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(ILSActive == "false");
            if (GUILayout.Button("No", GUILayout.Width(40), GUILayout.Height(23)))
                ILSActive = "false";
            GUI.enabled = !(ILSActive == "true");
            if (GUILayout.Button("Yes", GUILayout.Width(40), GUILayout.Height(23)))
                ILSActive = "true";
            GUILayout.EndHorizontal();
            //}
            GUI.enabled = true;
            GUILayout.Label("Description: ");
            descScroll = GUILayout.BeginScrollView(descScroll);
            siteDesc = GUILayout.TextArea(siteDesc, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUI.enabled = siteName.Length > 0;
            if (GUILayout.Button("Save", GUILayout.Width(115), GUILayout.Height(23)))
            {
                SaveSettings();
                this.Close();
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(115), GUILayout.Height(23)))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("NOTE: The SC angle is for a better initial vew on your SpaceCenter on the SC scene");

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
        #endregion

        #endregion

        #region Utility Functions



        internal void SaveSettings()
        {
            bool addToDB = false;
            if (!selectedObject.hasLauchSites)
            {
                Log.Normal("Creating LaunchSite");
                KKLaunchSite lsite = new KKLaunchSite();
                selectedObject.launchSite = lsite;
                Log.Normal("created; lsite = " + lsite + "; launch site = " + selectedObject.launchSite);
                selectedObject.hasLauchSites = true;
                lsite.staticInstance = selectedObject;
                selectedObject.launchSite.body = selectedObject.CelestialBody;
                addToDB = true;
            }
            else
            {
                // delete old LaunchSite on name change and add the new one.
                if (siteName != selectedObject.launchSite.LaunchSiteName)
                {
                    Log.Normal("Renaming of a LaunchSite triggered 1");
                    LaunchSiteManager.DeleteLaunchSite(selectedObject.launchSite);
                }
            }

            string oldName = selectedObject.launchSite.LaunchSiteName;
            LaunchSiteCategory oldCategory = category;
            bool oldState = selectedObject.launchSite.ILSIsActive;


            selectedObject.launchSite.LaunchSiteName = siteName;
            selectedObject.launchSite.LaunchSiteLength = float.Parse(stLength);
            selectedObject.launchSite.LaunchSiteWidth = float.Parse(stWidth);
            selectedObject.launchSite.LaunchSiteHeight = float.Parse(stHeight);

            selectedObject.launchSite.MaxCraftMass = float.Parse(sMaxMass);
            selectedObject.launchSite.MaxCraftParts = int.Parse(sMaxParts);

            selectedObject.launchSite.LaunchSiteType = siteType;
            selectedObject.launchSite.LaunchPadTransform = siteTrans;
            selectedObject.launchSite.LaunchSiteDescription = siteDesc;
            selectedObject.launchSite.OpenCost = float.Parse(stOpenCost);
            selectedObject.launchSite.CloseValue = float.Parse(stCloseValue);
            selectedObject.launchSite.LaunchSiteIsHidden = bool.Parse(siteHidden);
            selectedObject.launchSite.ILSIsActive = bool.Parse(ILSActive);
            selectedObject.launchSite.LaunchSiteAuthor = siteAuthor;
            selectedObject.launchSite.refLat = (float)selectedObject.RefLatitude;
            selectedObject.launchSite.refLon = (float)selectedObject.RefLongitude;
            selectedObject.launchSite.refAlt = (float)selectedObject.CelestialBody.GetAltitude(selectedObject.position);
            selectedObject.launchSite.sitecategory = category;
            selectedObject.launchSite.InitialCameraRotation = float.Parse(initialCameraRotation);

            selectedObject.launchSite.ToggleLaunchPositioning = togglePositioning;

            if (ILSConfig.DetectNavUtils())
            {
                Log.Normal("NavUtils detected");
                Log.Debug("object: " + selectedObject);
                Log.Debug("launchsite: " + selectedObject.launchSite);
                Log.Debug("body: " + selectedObject.launchSite.body);

                bool regenerateILSConfig = false;
                Log.Debug("old name: " + oldName);
                Log.Debug("new name: " + selectedObject.launchSite.LaunchSiteName);
                if (oldName != null && !oldName.Equals(siteName))
                {
                    ILSConfig.RenameSite(selectedObject.launchSite.LaunchSiteName, siteName);
                    regenerateILSConfig = true;
                }

                Log.Debug("old category: " + oldCategory);
                if ((oldCategory != category))
                {
                    ILSConfig.HandleCategoryChange(selectedObject);
                    regenerateILSConfig = true;
                }

                bool state = bool.Parse(ILSActive);
                Log.Normal("new state: " + state + "; old state: " + oldState);
                if (oldState != state || regenerateILSConfig)
                {
                    if (state)
                        ILSConfig.GenerateFullILSConfig(selectedObject);
                    else
                        ILSConfig.DropILSConfig(selectedObject.launchSite.LaunchSiteName, true);
                }
            }

            if (addToDB)
            {
                selectedObject.launchSite.ParseLSConfig(selectedObject, null);
                selectedObject.SaveConfig();
                EditorGUI.instance.enableColliders = true;
                selectedObject.ToggleAllColliders(true);
                LaunchSiteManager.RegisterLaunchSite(selectedObject.launchSite);
            }
            else
            {
                // register with the new name.
                if (siteName != oldName)
                {
                    Log.Normal("Renaming of a LaunchSite triggered 2");
                    LaunchSiteManager.RegisterLaunchSite(selectedObject.launchSite);
                }
            }

            selectedObject.SaveConfig();

        }


        internal void ReadSettings()
        {
            if (selectedObject.hasLauchSites)
            {
                //LaunchSite myLaunchSite = new LaunchSite();

                string sLaunchsiteDesc = selectedObject.launchSite.LaunchSiteDescription;
                string sModelDesc = selectedObject.model.description;

                // Edit or make a launchsite
                siteName = selectedObject.launchSite.LaunchSiteName;
                siteTrans = selectedObject.launchSite.LaunchPadTransform;

                if (sLaunchsiteDesc != "")
                    siteDesc = sLaunchsiteDesc;
                else
                    siteDesc = sModelDesc;


                siteHidden = selectedObject.launchSite.LaunchSiteIsHidden.ToString();
                ILSActive = selectedObject.launchSite.ILSIsActive.ToString();
                siteType = selectedObject.launchSite.LaunchSiteType;
                flOpenCost = selectedObject.launchSite.OpenCost;
                flCloseValue = selectedObject.launchSite.CloseValue;
                stOpenCost = string.Format("{0}", flOpenCost);
                stCloseValue = string.Format("{0}", flCloseValue);

                category = selectedObject.launchSite.sitecategory;

                initialCameraRotation = selectedObject.launchSite.InitialCameraRotation.ToString();

                togglePositioning = selectedObject.launchSite.ToggleLaunchPositioning;

                stLength = Math.Round(selectedObject.launchSite.LaunchSiteLength, 0).ToString();
                stWidth = Math.Round(selectedObject.launchSite.LaunchSiteWidth, 0).ToString();
                stHeight = Math.Round(selectedObject.launchSite.LaunchSiteHeight, 0).ToString();

                sMaxMass = Math.Round(selectedObject.launchSite.MaxCraftMass, 0).ToString();
                sMaxParts = selectedObject.launchSite.MaxCraftParts.ToString();


            }
            else
            {
                string sModelDesc = selectedObject.model.description;

                // Edit or make a launchsite
                siteName = selectedObject.gameObject.name;
                siteTrans = selectedObject.model.DefaultLaunchPadTransform;
                siteDesc = selectedObject.model.description;

                siteHidden = "false";
                ILSActive = "false";
                siteType = SiteType.Any;
                flOpenCost = 0f;
                flCloseValue = 0f;
                stOpenCost = string.Format("{0}", flOpenCost);
                stCloseValue = string.Format("{0}", flCloseValue);
                initialCameraRotation = "90";
                togglePositioning = false;

                category = LaunchSiteCategory.Other;

                stLength = Math.Round(selectedObject.model.DefaultLaunchSiteLength, 0).ToString();
                stWidth = Math.Round(selectedObject.model.DefaultLaunchSiteWidth, 0).ToString();
                stHeight = Math.Round(selectedObject.model.DefaultLaunchSiteHeight, 0).ToString();

                sMaxMass = Math.Round(selectedObject.model.DefaultLaunchSiteMaxMass, 0).ToString();
                sMaxParts = selectedObject.model.DefaultLaunchSiteMaxParts.ToString();

                ;
            }

            siteAuthor = selectedObject.model.author;
            // Debug.Log("KK: Making or editing a launchsite");
        }



        #endregion
    }
}

using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Launch site editor: makes the selected static a launch site, or edits its launch site.
    /// UI2 version of the old IMGUI window. Yes/no options are on/off switches; category and type are
    /// rows of buttons with the current choice greyed out. Nothing changes until "Save".
    /// </summary>
    internal class LaunchSiteEditor : KKEditorWindow
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

        protected override string WindowName => "LaunchSiteEditor";
        protected override string WindowTitle => "Launchsite Editor";
        protected override float WindowWidth => 380f;
        protected override Vector2 DefaultPosition => new Vector2(0.45f, 0.5f);

        private const float fieldWidth = 180f;

        #region Holders

        public static StaticInstance selectedObject = null;

        internal string siteName, siteTrans, siteDesc, siteAuthor;
        private bool siteHidden = false;
        private bool ILSActive = false;
        public bool togglePositioning = false;

        private string stOpenCost, stCloseValue, stLength, stWidth, stHeight, sMaxParts, sMaxMass;
        private string initialCameraRotation = "90";

        private SiteType siteType = SiteType.Any;
        internal LaunchSiteCategory category = LaunchSiteCategory.Other;

        #endregion


        internal void Open()
        {
            if (KerbalKonstructs.selectedInstance == null)
            {
                return;
            }
            DismissDialog();
            selectedObject = KerbalKonstructs.selectedInstance;
            ReadSettings();
            Build();
        }


        public override void Close()
        {
            DismissDialog();
        }


        protected override void OnUpdate()
        {
            if (MapView.MapIsEnabled || KerbalKonstructs.selectedInstance == null || KerbalKonstructs.selectedInstance != selectedObject)
            {
                Close();
            }
        }


        protected override void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), GoldLabel(delegate { return selectedObject.model.title; }), new DialogGUIFlexibleSpace()));

            content.Add(TextRow("Site name:", delegate { return siteName; }, delegate (string s) { siteName = s; }, 60, fieldWidth));
            content.Add(new DialogGUIHorizontalLayout(Label("Transform:"), new DialogGUIFlexibleSpace(), Label(delegate { return siteTrans ?? ""; })));
            content.Add(TextRow("Author:", delegate { return siteAuthor; }, delegate (string s) { siteAuthor = s; }, 60, fieldWidth));

            Separator();
            content.Add(ChoiceRow("Category:", new List<KeyValuePair<string, LaunchSiteCategory>>
                {
                    new KeyValuePair<string, LaunchSiteCategory>("RP", LaunchSiteCategory.RocketPad),
                    new KeyValuePair<string, LaunchSiteCategory>("RW", LaunchSiteCategory.Runway),
                    new KeyValuePair<string, LaunchSiteCategory>("HP", LaunchSiteCategory.Helipad),
                    new KeyValuePair<string, LaunchSiteCategory>("WA", LaunchSiteCategory.Waterlaunch),
                    new KeyValuePair<string, LaunchSiteCategory>("OT", LaunchSiteCategory.Other)
                }, delegate { return category; }, delegate (LaunchSiteCategory c) { category = c; }, 36f));
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), GoldLabel(delegate { return category.ToString(); })));
            content.Add(ChoiceRow("Site type:", new List<KeyValuePair<string, SiteType>>
                {
                    new KeyValuePair<string, SiteType>("VAB", SiteType.VAB),
                    new KeyValuePair<string, SiteType>("SPH", SiteType.SPH),
                    new KeyValuePair<string, SiteType>("Any", SiteType.Any)
                }, delegate { return siteType; }, delegate (SiteType t) { siteType = t; }, 50f));

            Separator();
            content.Add(TextRow("Max vessel length:", delegate { return stLength; }, delegate (string s) { stLength = s; }, 10, numberWidth, "m"));
            content.Add(TextRow("Max vessel width:", delegate { return stWidth; }, delegate (string s) { stWidth = s; }, 10, numberWidth, "m"));
            content.Add(TextRow("Max vessel height:", delegate { return stHeight; }, delegate (string s) { stHeight = s; }, 10, numberWidth, "m"));
            content.Add(TextRow("Max vessel mass:", delegate { return sMaxMass; }, delegate (string s) { sMaxMass = s; }, 10, numberWidth, "t"));
            content.Add(TextRow("Max vessel parts:", delegate { return sMaxParts; }, delegate (string s) { sMaxParts = s; }, 10, numberWidth));

            Separator();
            content.Add(TextRow("Open cost:", delegate { return stOpenCost; }, delegate (string s) { stOpenCost = s; }, 10, numberWidth, "\\F"));
            content.Add(TextRow("Close value:", delegate { return stCloseValue; }, delegate (string s) { stCloseValue = s; }, 10, numberWidth, "\\F"));
            content.Add(TextRow("Space center camera heading:", delegate { return initialCameraRotation; }, delegate (string s) { initialCameraRotation = s; }, 10, numberWidth));

            Separator();
            content.Add(SwitchRow("Site is hidden:", delegate { return siteHidden; }, delegate (bool value) { siteHidden = value; }, "Hidden sites only show up after they were discovered"));
            content.Add(SwitchRow("ILS / HSI on (NavUtilities):", delegate { return ILSActive; }, delegate (bool value) { ILSActive = value; }));
            content.Add(SwitchRow("Toggle positioning behavior:", delegate { return togglePositioning; }, delegate (bool value) { togglePositioning = value; }));

            Separator();
            content.Add(Label("Description:"));
            content.Add(TextArea(delegate { return siteDesc; }, delegate (string s) { siteDesc = s; }, WindowWidth - 30f, 90f));

            Separator();
            DialogGUIButton saveButton = new DialogGUIButton("Save", delegate { SaveSettings(); Close(); }, delegate { return !string.IsNullOrEmpty(siteName); }, 120f, itemHeight, false);
            saveButton.tooltipText = "Save the launch site and close";
            content.Add(new DialogGUIHorizontalLayout(saveButton, new DialogGUIFlexibleSpace(), new DialogGUIButton("Cancel", Close, 120f, itemHeight, false)));
            content.Add(Label("The camera heading is the initial view on the space center scene."));
        }


        #region Utility Functions

        internal void SaveSettings()
        {
            // validate all input first, so a bad field doesn't leave the site half registered
            float newLength, newWidth, newHeight, newMaxMass, newOpenCost, newCloseValue, newCameraRotation;
            int newMaxParts;
            if (!TryParseFloat(stLength, out newLength) || !TryParseFloat(stWidth, out newWidth) || !TryParseFloat(stHeight, out newHeight) ||
                !TryParseFloat(sMaxMass, out newMaxMass) || !TryParseInt(sMaxParts, out newMaxParts) ||
                !TryParseFloat(stOpenCost, out newOpenCost) || !TryParseFloat(stCloseValue, out newCloseValue) ||
                !TryParseFloat(initialCameraRotation, out newCameraRotation))
            {
                Log.UserError("LaunchSite not saved: one or more numeric fields are invalid");
                MiscUtils.HUDMessage("LaunchSite not saved: invalid numeric input", 10, 3);
                return;
            }
            bool newHidden = siteHidden;
            bool newILSActive = ILSActive;

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
            LaunchSiteCategory oldCategory = selectedObject.launchSite.sitecategory;
            bool oldState = selectedObject.launchSite.ILSIsActive;

            selectedObject.launchSite.LaunchSiteName = siteName;
            selectedObject.launchSite.LaunchSiteLength = newLength;
            selectedObject.launchSite.LaunchSiteWidth = newWidth;
            selectedObject.launchSite.LaunchSiteHeight = newHeight;

            selectedObject.launchSite.MaxCraftMass = newMaxMass;
            selectedObject.launchSite.MaxCraftParts = newMaxParts;

            selectedObject.launchSite.LaunchSiteType = siteType;
            selectedObject.launchSite.LaunchPadTransform = siteTrans;
            selectedObject.launchSite.LaunchSiteDescription = siteDesc;
            selectedObject.launchSite.OpenCost = newOpenCost;
            selectedObject.launchSite.CloseValue = newCloseValue;
            selectedObject.launchSite.LaunchSiteIsHidden = newHidden;
            selectedObject.launchSite.ILSIsActive = newILSActive;
            selectedObject.launchSite.LaunchSiteAuthor = siteAuthor;
            selectedObject.launchSite.refLat = (float)selectedObject.RefLatitude;
            selectedObject.launchSite.refLon = (float)selectedObject.RefLongitude;
            selectedObject.launchSite.refAlt = (float)selectedObject.CelestialBody.GetAltitude(selectedObject.position);
            selectedObject.launchSite.sitecategory = category;
            selectedObject.launchSite.InitialCameraRotation = newCameraRotation;

            selectedObject.launchSite.ToggleLaunchPositioning = togglePositioning;

            if (ILSConfig.DetectNavUtils())
            {
                Log.Normal("NavUtils detected");

                bool regenerateILSConfig = false;
                if (oldName != null && !oldName.Equals(siteName))
                {
                    ILSConfig.RenameSite(oldName, siteName);
                    regenerateILSConfig = true;
                }

                // (the old editor compared the category with itself, so a change was never noticed)
                if (!addToDB && oldCategory != category)
                {
                    ILSConfig.HandleCategoryChange(selectedObject);
                    regenerateILSConfig = true;
                }

                bool state = newILSActive;
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
                InstanceEditor.instance.enableColliders = true;
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
                string sLaunchsiteDesc = selectedObject.launchSite.LaunchSiteDescription;

                siteName = selectedObject.launchSite.LaunchSiteName;
                siteTrans = selectedObject.launchSite.LaunchPadTransform;
                siteDesc = !string.IsNullOrEmpty(sLaunchsiteDesc) ? sLaunchsiteDesc : selectedObject.model.description;

                siteHidden = selectedObject.launchSite.LaunchSiteIsHidden;
                ILSActive = selectedObject.launchSite.ILSIsActive;
                siteType = selectedObject.launchSite.LaunchSiteType;
                stOpenCost = string.Format("{0}", selectedObject.launchSite.OpenCost);
                stCloseValue = string.Format("{0}", selectedObject.launchSite.CloseValue);

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
                siteName = selectedObject.gameObject.name;
                siteTrans = selectedObject.model.DefaultLaunchPadTransform;
                siteDesc = selectedObject.model.description;

                siteHidden = false;
                ILSActive = false;
                siteType = SiteType.Any;
                stOpenCost = "0";
                stCloseValue = "0";
                initialCameraRotation = "90";
                togglePositioning = false;

                category = LaunchSiteCategory.Other;

                stLength = Math.Round(selectedObject.model.DefaultLaunchSiteLength, 0).ToString();
                stWidth = Math.Round(selectedObject.model.DefaultLaunchSiteWidth, 0).ToString();
                stHeight = Math.Round(selectedObject.model.DefaultLaunchSiteHeight, 0).ToString();

                sMaxMass = Math.Round(selectedObject.model.DefaultLaunchSiteMaxMass, 0).ToString();
                sMaxParts = selectedObject.model.DefaultLaunchSiteMaxParts.ToString();
            }

            siteAuthor = selectedObject.model.author;
            siteDesc = siteDesc ?? "";
            siteName = siteName ?? "";
            siteAuthor = siteAuthor ?? "";
        }

        #endregion
    }
}

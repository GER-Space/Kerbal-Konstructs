using System;
using KerbalKonstructs.Core;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;
using Upgradeables;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
    class LaunchSiteEditor : KKWindow
    {
        #region Variable Declarations

        private List<Transform> transformList = new List<Transform>();

        public Boolean foldedIn = false;
        public Boolean doneFold = false;

        #region Texture Definitions
        // Texture definitions
        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
        public Texture tBilleted = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/billeted", false);
        public Texture tCopyPos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/copypos", false);
        public Texture tPastePos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/pastepos", false);
        public Texture tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
        public Texture tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
        public Texture tSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/search", false);
        public Texture tCancelSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cancelsearch", false);
        public Texture tVAB = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/VABMapIcon", false);
        public Texture tSPH = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SPHMapIcon", false);
        public Texture tANY = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ANYMapIcon", false);
        public Texture tFocus = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/focuson", false);
        public Texture tSnap = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/snapto", false);
        public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        public Texture textureWorld = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/world", false);
        public Texture textureCubes = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cubes", false);

        #endregion

        #region Switches
        // Switches
        public Boolean enableColliders = false;
        internal static bool isScanable = false;


        //   public static Boolean editingFacility = false;

        public Boolean creatingInstance = false;
        public Boolean showLocal = false;
        public Boolean onNGS = false;
        public Boolean displayingInfo = false;
        public Boolean SnapRotateMode = false;

        public Boolean bChangeFacilityType = false;

        #endregion

        #region GUI Windows
        // GUI Windows
        Rect siteEditorRect = new Rect(400, 45, 360, 625);

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

        public static StaticObject selectedObject = null;
        public StaticObject selectedObjectPrevious = null;

        internal static String facType = "None";
        internal static String sGroup = "Ungrouped";
        internal String increment = "0.1";
        internal String siteName, siteTrans, siteDesc, siteAuthor, siteCategory, siteHidden;
        float flOpenCost, flCloseValue, flRecoveryFactor, flRecoveryRange, flLaunchRefund, flLength, flWidth;



        Vector3 vbsnapangle1 = new Vector3(0, 0, 0);
        Vector3 vbsnapangle2 = new Vector3(0, 0, 0);

        Vector3 snapSourceWorldPos = new Vector3(0, 0, 0);
        Vector3 snapTargetWorldPos = new Vector3(0, 0, 0);


        internal StaticObject snapTargetInstance = null;


        private Vector3 snpspos = new Vector3(0, 0, 0);
        private Vector3 snptpos = new Vector3(0, 0, 0);
        private Vector3 vDrift = new Vector3(0, 0, 0);
        private Vector3 vCurrpos = new Vector3(0, 0, 0);

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();




        private static Vector3d position = Vector3d.zero;
        private Vector3d referenceVector = Vector3d.zero;
        private Vector3 orientation = Vector3.zero;


        private static float vis = 0;


        private bool guiInitialized = false;

        #endregion

        #endregion

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }
            if (KerbalKonstructs.instance.selectedObject == null)
            {
                this.Close();
            }

            if ((KerbalKonstructs.instance.selectedObject != null) && (!KerbalKonstructs.instance.selectedObject.preview))
            {
                drawEditor(KerbalKonstructs.instance.selectedObject);
            }
        }

        public override void Open()
        {
            selectedObject = KerbalKonstructs.instance.selectedObject;
            ReadSettings();
            base.Open();
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public LaunchSiteEditor()
        {
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

            // siteTypeMenu = new ComboBox(siteTypeOptions[0], siteTypeOptions, "button", "box", null, listStyle);
        }

        #region draw Methods

        /// <summary>
        /// Wrapper to draw editors
        /// </summary>
        /// <param name="obj"></param>
        public void drawEditor(StaticObject obj)
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
        }


        #region Editors


        #region Launchsite Editor
        // Launchsite Editor handlers
        string stOpenCost;
        string stCloseValue;
        string stRecoveryFactor;
        string stRecoveryRange;
        string stLaunchRefund;
        string stLength;
        string stWidth;

        /// <summary>
        /// Launchsite Editor
        /// </summary>
        /// <param name="id"></param>
        void drawLaunchSiteEditorWindow(int id)
        {
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
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

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
            GUILayout.Label("Length: ", GUILayout.Width(120));
            stLength = GUILayout.TextField(stLength, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Width: ", GUILayout.Width(120));
            stWidth = GUILayout.TextField(stWidth, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Category: ", GUILayout.Width(115));
            GUILayout.Label(siteCategory, GUILayout.Width(85));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(siteCategory == "RocketPad");
            if (GUILayout.Button("RP", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "RocketPad";
            GUI.enabled = !(siteCategory == "Runway");
            if (GUILayout.Button("RW", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Runway";
            GUI.enabled = !(siteCategory == "Helipad");
            if (GUILayout.Button("HP", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Helipad";
            GUI.enabled = !(siteCategory == "Waterlaunch");
            if (GUILayout.Button("WA", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Waterlaunch";
            GUI.enabled = !(siteCategory == "Other");
            if (GUILayout.Button("OT", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Other";
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Type: ", GUILayout.Width(120));
            if (siteType == SiteType.VAB)
                GUILayout.Label("VAB", GUILayout.Width(40));
            if (siteType == SiteType.SPH)
                GUILayout.Label("SPH", GUILayout.Width(40));
            if (siteType == SiteType.Any)
                GUILayout.Label("Any", GUILayout.Width(40));
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
            GUILayout.Label("Recovery Factor: ", GUILayout.Width(120));
            stRecoveryFactor = GUILayout.TextField(stRecoveryFactor, GUILayout.Height(19));
            GUILayout.Label(" %");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Effective Range: ", GUILayout.Width(120));
            stRecoveryRange = GUILayout.TextField(stRecoveryRange, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Launch Refund: ", GUILayout.Width(120));
            stLaunchRefund = GUILayout.TextField(stLaunchRefund, GUILayout.Height(19));
            GUILayout.Label(" %");
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
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.Label("Description: ");
            descScroll = GUILayout.BeginScrollView(descScroll);
            siteDesc = GUILayout.TextArea(siteDesc, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Width(115), GUILayout.Height(23)))
            {
                bool addToDB = false;
                if (!selectedObject.hasLauchSites)
                {
                    LaunchSite lsite = new LaunchSite();
                    selectedObject.launchSite = lsite;
                    selectedObject.hasLauchSites = true;
                    lsite.parentInstance = selectedObject;
                    addToDB = true;
                }
                selectedObject.launchSite.LaunchSiteName = siteName;
                selectedObject.launchSite.LaunchSiteLength = float.Parse(stLength);
                selectedObject.launchSite.LaunchSiteWidth = float.Parse(stWidth);
                selectedObject.launchSite.LaunchSiteType = siteType;
                selectedObject.launchSite.LaunchPadTransform = siteTrans;
                selectedObject.launchSite.LaunchSiteDescription = siteDesc;
                selectedObject.launchSite.OpenCost = float.Parse(stOpenCost);
                selectedObject.launchSite.CloseValue = float.Parse(stCloseValue);
                selectedObject.launchSite.RecoveryFactor = float.Parse(stRecoveryFactor);
                selectedObject.launchSite.RecoveryRange = float.Parse(stRecoveryRange);
                selectedObject.launchSite.LaunchRefund = float.Parse(stLaunchRefund);
                selectedObject.launchSite.OpenCloseState = "Open";
                selectedObject.launchSite.Category = siteCategory;
                selectedObject.launchSite.LaunchSiteIsHidden = bool.Parse(siteHidden);
                selectedObject.launchSite.LaunchSiteAuthor = siteAuthor;

                if (addToDB)
                {
                    LaunchSiteManager.RegisterLaunchSite(selectedObject.launchSite);
                }
                KerbalKonstructs.instance.saveObjects();
                this.Close();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(115), GUILayout.Height(23)))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("NOTE: If a newly created launchsite object does not display when launched from, a restart of KSP will be required for the site to be correctly rendered.");

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
        #endregion

        #endregion

        #region Utility Functions
 

        /// <summary>
        /// Updates the Window Strings to the new settings
        /// </summary>
        /// <param name="instance"></param>
		public static void updateSelection(StaticObject instance)
        {
            selectedObject = instance;

            isScanable = selectedObject.isScanable;

            vis = instance.VisibilityRange;
            facType = instance.FacilityType;

            if (facType == null || facType == "")
            {
                string DefaultFacType = instance.model.DefaultFacilityType;

                if (DefaultFacType == null || DefaultFacType == "None" || DefaultFacType == "")
                    facType = "None";
                else
                    facType = DefaultFacType;
            }

            sGroup = instance.Group;
            selectedObject.update();
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


                siteCategory = selectedObject.launchSite.Category;
                siteHidden = selectedObject.launchSite.LaunchSiteIsHidden.ToString();
                siteType = selectedObject.launchSite.LaunchSiteType;
                flOpenCost = selectedObject.launchSite.OpenCost;
                flCloseValue = selectedObject.launchSite.CloseValue;
                stOpenCost = string.Format("{0}", flOpenCost);
                stCloseValue = string.Format("{0}", flCloseValue);



                flRecoveryFactor = selectedObject.launchSite.RecoveryFactor;
                flRecoveryRange = selectedObject.launchSite.RecoveryRange;
                flLaunchRefund = selectedObject.launchSite.LaunchRefund;

                flLength = selectedObject.launchSite.LaunchSiteLength;

                if (flLength < 1)
                    flLength = selectedObject.model.DefaultLaunchSiteLength;

                flWidth = selectedObject.launchSite.LaunchSiteWidth;

                if (flWidth < 1)
                    flWidth = selectedObject.model.DefaultLaunchSiteWidth;

            }
            else
            {

                string sModelDesc = selectedObject.model.description;

                // Edit or make a launchsite
                siteName = selectedObject.gameObject.name;
                siteTrans = selectedObject.model.DefaultLaunchPadTransform;
                siteDesc = selectedObject.model.description;

                siteCategory = "";
                siteHidden = "false";
                siteType = SiteType.Any;
                flOpenCost = 0f;
                flCloseValue = 0f;
                stOpenCost = string.Format("{0}", flOpenCost);
                stCloseValue = string.Format("{0}", flCloseValue);

                flRecoveryFactor = 0f;
                flRecoveryRange = 0f;
                flLaunchRefund = 0f;

                flLength = selectedObject.model.DefaultLaunchSiteLength;
                flWidth = selectedObject.model.DefaultLaunchSiteWidth;
            }

            stRecoveryFactor = string.Format("{0}", flRecoveryFactor);
            stRecoveryRange = string.Format("{0}", flRecoveryRange);
            stLaunchRefund = string.Format("{0}", flLaunchRefund);

            stLength = string.Format("{0}", flLength);
            stWidth = string.Format("{0}", flWidth);
            siteAuthor = !String.IsNullOrEmpty(selectedObject.launchSite.LaunchSiteAuthor) ? selectedObject.launchSite.LaunchSiteAuthor : selectedObject.model.author;
            // Debug.Log("KK: Making or editing a launchsite");
        }



        #endregion
    }
}

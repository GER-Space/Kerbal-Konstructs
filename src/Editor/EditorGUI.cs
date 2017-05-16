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
    class EditorGUI : KKWindow
    {
        #region Variable Declarations

        private List<Transform> transformList = new List<Transform>();

        private CelestialBody body;

        internal static FacilityEditor GUI_FacilityEditor = new FacilityEditor();

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

        //public static Boolean editingLaunchSite = false;

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
        Rect toolRect = new Rect(300, 35, 330, 695);
        Rect siteEditorRect = new Rect(400, 45, 360, 625);

        #endregion

        #region GUI elements
        // GUI elements
        GUIStyle listStyle = new GUIStyle();
        GUIStyle navStyle = new GUIStyle();

        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle KKWindows;
        GUIStyle BoxNoBorder;

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
        static LaunchSite lTargetSite = null;

        internal static String facType = "None";
        internal static String sGroup = "Ungrouped";
        String increment = "0.1";



        Vector3 vbsnapangle1 = new Vector3(0, 0, 0);
        Vector3 vbsnapangle2 = new Vector3(0, 0, 0);

        Vector3 snapSourceWorldPos = new Vector3(0, 0, 0);
        Vector3 snapTargetWorldPos = new Vector3(0, 0, 0);

        String sSTROT = "";

        GameObject selectedSnapPoint = null;
        GameObject selectedSnapPoint2 = null;
        public StaticObject snapTargetInstance = null;
        StaticObject snapTargetInstancePrevious = null;

        private Vector3 snpspos = new Vector3(0, 0, 0);
        private Vector3 snptpos = new Vector3(0, 0, 0);
        private Vector3 vDrift = new Vector3(0, 0, 0);
        private Vector3 vCurrpos = new Vector3(0, 0, 0);

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();

        private Vector3d savedposition;
        private double savedalt;
        private double savedrot;
        private bool savedpos = false;

        private static Space referenceSystem = Space.Self;

        private static Vector3d position = Vector3d.zero;
        private Vector3d referenceVector = Vector3d.zero;
        private Vector3 orientation = Vector3.zero;

        private static double altitude;
        private static double latitude, longitude;

        private double rotation = 0d;

        private static float vis = 0;


        private static float modelScale = 1f;

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
                CloseEditors();
                CloseVectors();
            }

            if ((KerbalKonstructs.instance.selectedObject != null) && (!KerbalKonstructs.instance.selectedObject.preview))
            {
                drawEditor(KerbalKonstructs.instance.selectedObject);
            }
        }


        public override void Close()
        {
            CloseVectors();
            CloseEditors();
            base.Close();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EditorGUI()
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
                if (selectedObject != obj)
                {
                    updateSelection(obj);
                    position = selectedObject.gameObject.transform.position;
                    Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
                    SetupVectors();
                }


                if (foldedIn)
                {
                    if (!doneFold)
                        toolRect = new Rect(toolRect.xMin, toolRect.yMin, toolRect.width - 45, toolRect.height - 250);

                    doneFold = true;
                }

                if (!foldedIn)
                {
                    if (doneFold)
                        toolRect = new Rect(toolRect.xMin, toolRect.yMin, toolRect.width + 45, toolRect.height + 250);

                    doneFold = false;
                }

                toolRect = GUI.Window(0xB00B1E3, toolRect, InstanceEditorWindow, "", KKWindows);

                //if (editingLaunchSite)
                //{
                //    siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawLaunchSiteEditorWindow, "", KKWindows);
                //}
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

        #region Instance Editor

        /// <summary>
        /// Instance Editor window
        /// </summary>
        /// <param name="windowID"></param>
        void InstanceEditorWindow(int windowID)
        {
            //initialize values
            rotation = (double)selectedObject.RotationAngle;
            referenceVector = (Vector3d)selectedObject.RadialPosition;
            orientation = selectedObject.Orientation;
            modelScale = selectedObject.ModelScale;
            //isScanable = bool.Parse((string)selectedObject.getSetting("isScanable"));

            // make this new when converted to PQSCity2
            // fill the variables here for later use
            if (position == Vector3d.zero)
            {
                position = selectedObject.gameObject.transform.position;
                Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
            }

            UpdateVectors();

            string smessage = "";


            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Instance Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();

            if (foldedIn) tFolded = tFoldOut;
            if (!foldedIn) tFolded = tFoldIn;

            if (GUILayout.Button(tFolded, GUILayout.Height(23), GUILayout.Width(23)))
            {
                if (foldedIn) foldedIn = false;
                else
                    foldedIn = true;
            }

            GUILayout.Button(selectedObject.model.title + " ("+ selectedObject.indexInGroup.ToString() + ")", GUILayout.Height(23));

            GUILayout.EndHorizontal();

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Position");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent(tCopyPos, "Copy Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    savedpos = true;
                    savedposition = position;

                    savedalt = altitude;
                    savedrot = rotation;
                    // Debug.Log("KK: Instance position copied");
                }
                if (GUILayout.Button(new GUIContent(tPastePos, "Paste Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    if (savedpos)
                    {
                        position = savedposition;
                        altitude = savedalt;
                        rotation = savedrot;
                        saveSettings();
                        // Debug.Log("KK: Instance position pasted");
                    }
                }

                if (!foldedIn)
                {
                    if (GUILayout.Button(new GUIContent(tSnap, "Snap to Target"), GUILayout.Width(23), GUILayout.Height(23)))
                    {
                        if (snapTargetInstance == null)
                        {

                        }
                        else
                        {
                            Vector3 snapTargetPos = (Vector3)snapTargetInstance.RadialPosition;
                            float snapTargetAlt = (float)snapTargetInstance.RadiusOffset;
                            selectedObject.RadialPosition = snapTargetPos;
                            selectedObject.RadiusOffset = snapTargetAlt;
                        }
                        updateSelection(selectedObject);
                    }
                }

                GUILayout.FlexibleSpace();
                if (!foldedIn)
                {
                    GUILayout.Label("Increment");
                    increment = GUILayout.TextField(increment, 5, GUILayout.Width(48));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("0.001", GUILayout.Height(18)))
                    {
                        increment = "0.001";
                    }
                    if (GUILayout.Button("0.01", GUILayout.Height(18)))
                    {
                        increment = "0.01";
                    }
                    if (GUILayout.Button("0.1", GUILayout.Height(18)))
                    {
                        increment = "0.1";
                    }
                    if (GUILayout.Button("1", GUILayout.Height(18)))
                    {
                        increment = "1";
                    }
                    if (GUILayout.Button("10", GUILayout.Height(18)))
                    {
                        increment = "10";
                    }
                    if (GUILayout.Button("25", GUILayout.Height(16)))
                    {
                        increment = "25";
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    GUILayout.Label("i");
                    increment = GUILayout.TextField(increment, 3, GUILayout.Width(25));

                    if (GUILayout.Button("0.1", GUILayout.Height(23)))
                    {
                        increment = "0.1";
                    }
                    if (GUILayout.Button("1", GUILayout.Height(23)))
                    {
                        increment = "1";
                    }
                    if (GUILayout.Button("10", GUILayout.Height(23)))
                    {
                        increment = "10";
                    }
                }
            }
            GUILayout.EndHorizontal();

            //
            // Set reference butons
            //
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reference System: ");
            GUILayout.FlexibleSpace();
            GUI.enabled = (referenceSystem == Space.World);

            if (GUILayout.Button(new GUIContent(textureCubes, "Model"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.Self;
                UpdateVectors();
            }

            GUI.enabled = (referenceSystem == Space.Self);
            if (GUILayout.Button(new GUIContent(textureWorld, "World"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.World;
                UpdateVectors();
            }
            GUI.enabled = true;

            GUILayout.Label(referenceSystem.ToString());

            GUILayout.EndHorizontal();
            float fTempWidth = 80f;
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();

            if (referenceSystem == Space.Self)
            {
                GUILayout.Label("Back / Forward:");
                GUILayout.FlexibleSpace();

                if (foldedIn) fTempWidth = 40f;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.back * float.Parse(increment));
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.forward * float.Parse(increment));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Left / Right:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.left * float.Parse(increment));
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.right * float.Parse(increment));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Down / Up:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.down * float.Parse(increment));
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.up * float.Parse(increment));
                }

            }
            else
            {
                GUILayout.Label("West / East :");
                GUILayout.FlexibleSpace();

                if (foldedIn) fTempWidth = 40f;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, -double.Parse(increment));
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, double.Parse(increment));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("South / North:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(-double.Parse(increment), 0d);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(double.Parse(increment), 0d);
                }

            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;

            if (!foldedIn)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Box("Latitude");
                    GUILayout.Box(latitude.ToString("#0.0000000"));
                    GUILayout.Box("Longitude");
                    GUILayout.Box(longitude.ToString("#0.0000000"));
                }
                GUILayout.EndHorizontal();
            }

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            // 
            // Altitude editing
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Alt.");
                GUILayout.FlexibleSpace();
                altitude = double.Parse(GUILayout.TextField(altitude.ToString(), 25, GUILayout.Width(fTempWidth)));
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    altitude -= double.Parse(increment);
                    saveSettings();
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    altitude += double.Parse(increment);
                    saveSettings();
                }
            }
            GUILayout.EndHorizontal();

            var pqsc = selectedObject.CelestialBody.pqsController;

            if (!foldedIn)
            {
                if (GUILayout.Button("Snap to Terrain", GUILayout.Height(21)))
                {
                    altitude = 1.0d + ((pqsc.GetSurfaceHeight(selectedObject.RadialPosition) - pqsc.radius - selectedObject.RadiusOffset));
                    saveSettings();
                }
            }

            GUI.enabled = true;

            bool isDevMode = KerbalKonstructs.instance.DevMode;

            if (!foldedIn)
            {
                if (isDevMode && selectedObject != null)
                {
                    GUILayout.Space(10);
                    GUILayout.Box("SNAP-POINTS");

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Source ");
                        GUILayout.Box("" + vbsnapangle1.ToString() + "d");
                        GUILayout.Box("Wpos " + snapSourceWorldPos.ToString());
                        GUILayout.FlexibleSpace();
                        Transform[] transformList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
                        List<GameObject> snappointList = (from t in transformList where t.gameObject.name == "snappoint" select t.gameObject).ToList();

                        foreach (GameObject tSnapPoint in snappointList)
                        {
                            GUI.enabled = (tSnapPoint != selectedSnapPoint);
                            if (GUILayout.Button("*", GUILayout.Width(23), GUILayout.Height(23)))
                            {
                                selectedSnapPoint = tSnapPoint;
                                SnapToTarget();
                                updateSelection(selectedObject);
                            }
                            GUI.enabled = true;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (isDevMode && snapTargetInstance != null)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Target ");
                        GUILayout.Box("" + vbsnapangle2.ToString() + "d");
                        GUILayout.Box("Wpos " + snapTargetWorldPos.ToString());
                        GUILayout.FlexibleSpace();
                        Transform[] transformList2 = snapTargetInstance.gameObject.GetComponentsInChildren<Transform>();
                        List<GameObject> snappointList2 = (from t2 in transformList2 where t2.gameObject.name == "snappoint" select t2.gameObject).ToList();

                        foreach (GameObject tSnapPoint2 in snappointList2)
                        {
                            GUI.enabled = (tSnapPoint2 != selectedSnapPoint2);
                            if (GUILayout.Button("*", GUILayout.Width(23), GUILayout.Height(23)))
                            {
                                selectedSnapPoint2 = tSnapPoint2;
                                SnapToTarget(SnapRotateMode);
                                updateSelection(selectedObject);
                            }
                            GUI.enabled = true;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        sSTROT = snapTargetInstance.pqsCity.reorientFinalAngle.ToString();
                        GUILayout.Box("Rot " + sSTROT);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Snap", GUILayout.Height(23)))
                        {
                            SnapToTarget(SnapRotateMode);
                        }
                        GUILayout.Space(10);

                        SnapRotateMode = GUILayout.Toggle(SnapRotateMode, "SnapRot Mode", GUILayout.Height(23));

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("DriftFix", GUILayout.Height(23)))
                        {
                            if (selectedSnapPoint == null || selectedSnapPoint2 == null)
                            {
                            }
                            else
                            {
                                FixDrift(SnapRotateMode);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (!foldedIn)
                GUILayout.Space(5);

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            fTempWidth = 80f;

            GUI.enabled = true;

            if (!foldedIn)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Vis.");
                    GUILayout.FlexibleSpace();
                    vis = float.Parse(GUILayout.TextField(vis.ToString(), 6, GUILayout.Width(80)));
                    if (GUILayout.Button("Min", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        vis -= 1000000000f;
                        saveSettings();
                    }
                    if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        vis -= 2500f;
                        saveSettings();
                    }
                    if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        vis += 2500f;
                        saveSettings();
                    }
                    if (GUILayout.Button("Max", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        vis = (float)KerbalKonstructs.instance.maxEditorVisRange;
                        saveSettings();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            if (!foldedIn)
            {
                //
                // Orientation quick preset
                //
                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Orientation");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("U", "Top Up"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(0, 1, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("D", "Bottom Up"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(0, -1, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("L", "On Left"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(1, 0, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("R", "On Right"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(-1, 0, 0); saveSettings();
                    }
                }
                GUILayout.EndHorizontal();

                //
                // Orientation adjustment
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Pitch:");
                    GUILayout.FlexibleSpace();

                    fTempWidth = 80f;

                    if (foldedIn) fTempWidth = 40f;

                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetPitch(float.Parse(increment));
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetPitch(-float.Parse(increment));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Roll:");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetRoll(float.Parse(increment));
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetRoll(-float.Parse(increment));
                    }

                }
                GUILayout.EndHorizontal();


                //
                // Rotation
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Heading:");
                    GUILayout.FlexibleSpace();
                    GUILayout.TextField(heading.ToString(), 9, GUILayout.Width(fTempWidth));

                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(-double.Parse(increment));
                    }
                    if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(-double.Parse(increment));
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(double.Parse(increment));
                    }
                    if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(double.Parse(increment));
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);
                //
                // Scale
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Model Scale: ");
                    GUILayout.FlexibleSpace();
                    modelScale = float.Parse(GUILayout.TextField(modelScale.ToString(), 4, GUILayout.Width(fTempWidth)));

                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        modelScale -= float.Parse(increment); saveSettings();
                    }
                    if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        modelScale -= float.Parse(increment); saveSettings();
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        modelScale += float.Parse(increment); saveSettings();
                    }
                    if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        modelScale += float.Parse(increment); saveSettings();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

            }

            GUI.enabled = true;

            if (!foldedIn)
            {

                if (GUILayout.Button("Facility Type: " + facType, GUILayout.Height(23)))
                {
                    if (!GUI_FacilityEditor.IsOpen())
                        GUI_FacilityEditor.Open();
                }
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Group: ", GUILayout.Height(23));
                GUILayout.FlexibleSpace();

                if (!foldedIn)
                    sGroup = GUILayout.TextField(sGroup, 30, GUILayout.Width(185), GUILayout.Height(23));
                else
                    sGroup = GUILayout.TextField(sGroup, 30, GUILayout.Width(135), GUILayout.Height(23));
            }
            GUILayout.EndHorizontal();

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            if (!foldedIn)
            {
                GUILayout.Space(3);

                GUILayout.BeginHorizontal();
                {
                    enableColliders = GUILayout.Toggle(enableColliders, "Enable Colliders", GUILayout.Width(140), GUILayout.Height(23));

                    Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
                    List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.GetComponent<Collider>() != null select t.gameObject).ToList();

                    if (enableColliders)
                    {
                        foreach (GameObject collider in colliderList)
                        {
                            collider.GetComponent<Collider>().enabled = true;
                        }
                    }
                    if (!enableColliders)
                    {
                        foreach (GameObject collider in colliderList)
                        {
                            collider.GetComponent<Collider>().enabled = false;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Duplicate", GUILayout.Width(130), GUILayout.Height(23)))
                    {
                        KerbalKonstructs.instance.saveObjects();
                        StaticModel oModel = selectedObject.model;
                        float fOffset = selectedObject.RadiusOffset;
                        Vector3 vPosition = selectedObject.RadialPosition;
                        float fAngle = selectedObject.RotationAngle;
                        smessage = "Spawned duplicate " + selectedObject.model.title;
                        KerbalKonstructs.instance.deselectObject(true, true);
                        spawnInstance(oModel, fOffset, vPosition, fAngle);
                        MiscUtils.HUDMessage(smessage, 10, 2);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    bool isScanable2 = GUILayout.Toggle(isScanable, "Static will show up on anomaly scanners", GUILayout.Width(250), GUILayout.Height(23));
                    if (isScanable2 != isScanable)
                        isScanable = isScanable2;
                    saveSettings();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

            }

            if (foldedIn)
            {
                if (GUILayout.Button("Duplicate", GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.saveObjects();
                    StaticModel oModel = selectedObject.model;
                    float fOffset = selectedObject.RadiusOffset;
                    Vector3 vPosition = selectedObject.RadialPosition;
                    float fAngle = selectedObject.RotationAngle;
                    smessage = "Spawned duplicate " + selectedObject.model.title;
                    KerbalKonstructs.instance.deselectObject(true, true);
                    spawnInstance(oModel, fOffset, vPosition, fAngle);
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
            }

            GUI.enabled = true;

            GUI.enabled = !KerbalKonstructs.GUI_LSEditor.IsOpen();
            // Make a new LaunchSite here:
            if (!foldedIn)
            {

                if (!selectedObject.hasLauchSites && string.IsNullOrEmpty(selectedObject.model.DefaultLaunchPadTransform))
                    GUI.enabled = false;

                if (GUILayout.Button((selectedObject.hasLauchSites ? "Edit" : "Make") + " Launchsite", GUILayout.Height(23)))
                {
                    //if (selectedObject.hasLauchSites)
                    //{
                    //    //LaunchSite myLaunchSite = new LaunchSite();

                    //    string sLaunchsiteDesc = selectedObject.launchSite.LaunchSiteDescription;
                    //    string sModelDesc = selectedObject.model.description;

                    //    // Edit or make a launchsite
                    //    siteName = selectedObject.launchSite.LaunchSiteName ;
                    //    siteTrans = selectedObject.launchSite.LaunchPadTransform;

                    //    if (sLaunchsiteDesc != "")
                    //        siteDesc = sLaunchsiteDesc;
                    //    else
                    //        siteDesc = sModelDesc;

                    //    siteCategory = selectedObject.launchSite.Category;
                    //    siteHidden = selectedObject.launchSite.LaunchSiteIsHidden.ToString();
                    //    siteType = selectedObject.launchSite.LaunchSiteType;
                    //    flOpenCost = selectedObject.launchSite.OpenCost;
                    //    flCloseValue = selectedObject.launchSite.CloseValue;
                    //    stOpenCost = string.Format("{0}", flOpenCost);
                    //    stCloseValue = string.Format("{0}", flCloseValue);

                    //    flRecoveryFactor = selectedObject.launchSite.RecoveryFactor;
                    //    flRecoveryRange = selectedObject.launchSite.RecoveryRange;
                    //    flLaunchRefund = selectedObject.launchSite.LaunchRefund;

                    //    flLength = selectedObject.launchSite.LaunchSiteLength;

                    //    if (flLength < 1)
                    //        flLength = selectedObject.model.DefaultLaunchSiteLength;

                    //    flWidth = selectedObject.launchSite.LaunchSiteWidth;

                    //    if (flWidth < 1)
                    //        flWidth = selectedObject.model.DefaultLaunchSiteWidth;

                    //} else
                    //{
  
                    //    string sModelDesc = selectedObject.model.description;

                    //    // Edit or make a launchsite
                    //    siteName = selectedObject.gameObject.name;
                    //    siteTrans = selectedObject.model.DefaultLaunchPadTransform;
                    //    siteDesc = selectedObject.model.description;

                    //    siteCategory = "";
                    //    siteHidden = "false";
                    //    siteType = SiteType.Any;
                    //    flOpenCost = 0f;
                    //    flCloseValue = 0f;
                    //    stOpenCost = string.Format("{0}", flOpenCost);
                    //    stCloseValue = string.Format("{0}", flCloseValue);

                    //    flRecoveryFactor = 0f;
                    //    flRecoveryRange = 0f;
                    //    flLaunchRefund = 0f;

                    //    flLength = selectedObject.model.DefaultLaunchSiteLength;
                    //    flWidth = selectedObject.model.DefaultLaunchSiteWidth;
                    //}

                    //stRecoveryFactor = string.Format("{0}", flRecoveryFactor);
                    //stRecoveryRange = string.Format("{0}", flRecoveryRange);
                    //stLaunchRefund = string.Format("{0}", flLaunchRefund);

                    //stLength = string.Format("{0}", flLength);
                    //stWidth = string.Format("{0}", flWidth);

                    //siteAuthor = selectedObject.hasLauchSites?  selectedObject.launchSite.LaunchSiteAuthor : selectedObject.model.author;
                    //// Debug.Log("KK: Making or editing a launchsite");




                    KerbalKonstructs.GUI_LSEditor.Open();
                }
            }

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.saveObjects();
                    smessage = "Saved all changes to all objects.";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Deselect", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.saveObjects();
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            if (!foldedIn)
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete Instance", GUILayout.Height(21)))
                {
                    if (snapTargetInstance == selectedObject) snapTargetInstance = null;
                    if (snapTargetInstancePrevious == selectedObject) snapTargetInstancePrevious = null;
                    if (selectedObjectPrevious == selectedObject) selectedObjectPrevious = null;
                    KerbalKonstructs.instance.deleteObject(selectedObject);
                    selectedObject = null;
                    return;
                }

                GUILayout.Space(15);
            }


            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x - (25 + (labelSize.x / 2)), Event.current.mousePosition.y - 40, labelSize.x + 10, labelSize.y + 5), GUI.tooltip);
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }


        #endregion

        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            GUI_FacilityEditor.Close();
            KerbalKonstructs.GUI_LSEditor.Close();
        }


        #endregion

        #region Utility Functions


        /// <summary>
        /// Spawns an Instance of an defined StaticModel 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fOffset"></param>
        /// <param name="vPosition"></param>
        /// <param name="fAngle"></param>
        /// <returns></returns>
        public void spawnInstance(StaticModel model, float fOffset, Vector3 vPosition, float fAngle)
        {
            StaticObject instance = new StaticObject();
            instance.gameObject = UnityEngine.Object.Instantiate(model.prefab);
            instance.RadiusOffset = fOffset;
            instance.CelestialBody = KerbalKonstructs.instance.getCurrentBody();
            string newGroup = (selectedObject != null) ? (string)selectedObject.Group : "Ungrouped";
            instance.Group = newGroup;
            instance.RadialPosition = vPosition;
            instance.RotationAngle = fAngle;
            instance.Orientation = Vector3.up;
            instance.VisibilityRange = 25000f;

            //string sPad = model.DefaultLaunchPadTransform;
            //if (!String.IsNullOrEmpty(sPad))
            //{
            //    instance.setSetting("LaunchPadTransform", sPad);
            //}

            //if (!KerbalKonstructs.instance.DevMode)
            //{
            //    instance.setSetting("CustomInstance", "True");
            //}

            instance.model = model;
            Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/NewInstances/");
            instance.configPath = "KerbalKonstructs/NewInstances/" + model.name + "-instances.cfg";
            instance.configUrl = null;

            enableColliders = false;
            instance.spawnObject(true, false);
        }

        public static void setTargetSite(LaunchSite lsTarget, string sName = "")
        {
            lTargetSite = lsTarget;
        }

        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 vectorDrawPosition
        {
            get
            {
                return (selectedObject.pqsCity.transform.position + 4 * selectedObject.pqsCity.transform.up + 4 * selectedObject.pqsCity.transform.right);
            }
        }


        /// <summary>
        /// returns the heading the selected object
        /// </summary>
        /// <returns></returns>
        public float heading
        {
            get
            {
                Vector3 myForward = Vector3.ProjectOnPlane(selectedObject.gameObject.transform.forward, upVector);
                float myHeading;

                if (Vector3.Dot(myForward, eastVector) > 0)
                {
                    myHeading = Vector3.Angle(myForward, northVector);
                }
                else
                {
                    myHeading = 360 - Vector3.Angle(myForward, northVector);
                }
                return myHeading;
            }
        }

        /// <summary>
        /// gives a vector to the east
        /// </summary>
        private Vector3 eastVector
        {
            get
            {
                return Vector3.Cross(upVector, northVector).normalized;
            }
        }

        /// <summary>
        /// vector to north
        /// </summary>
        private Vector3 northVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
            }
        }

        private Vector3 upVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return (Vector3)body.GetSurfaceNVector(latitude, longitude).normalized;
            }
        }

        /// <summary>
        /// Sets the vectors active and updates thier position and directions
        /// </summary>
        private void UpdateVectors()
        {
            if (selectedObject == null) { return; }

            if (referenceSystem == Space.Self)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                northVR.SetShow(false);
                eastVR.SetShow(false);

                fwdVR.Vector = selectedObject.pqsCity.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.draw();

                upVR.Vector = selectedObject.pqsCity.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.draw();

                rightVR.Vector = selectedObject.pqsCity.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.draw();

            }
            if (referenceSystem == Space.World)
            {
                northVR.SetShow(true);
                eastVR.SetShow(true);

                fwdVR.SetShow(false);
                upVR.SetShow(false);
                rightVR.SetShow(false);

                northVR.Vector = northVector;
                northVR.Start = vectorDrawPosition;
                northVR.draw();

                eastVR.Vector = eastVector;
                eastVR.Start = vectorDrawPosition;
                eastVR.draw();
            }
        }

        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedObject.pqsCity.transform.forward;
            fwdVR.Scale = 30d;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedObject.pqsCity.transform.up;
            upVR.Scale = 30d;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedObject.pqsCity.transform.right;
            rightVR.Scale = 30d;
            rightVR.Start = vectorDrawPosition;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;

            northVR.Color = new Color(0.9f, 0.3f, 0.3f);
            northVR.Vector = northVector;
            northVR.Scale = 30d;
            northVR.Start = vectorDrawPosition;
            northVR.SetLabel("north");
            northVR.Width = 0.01d;

            eastVR.Color = new Color(0.3f, 0.3f, 0.9f);
            eastVR.Vector = eastVector;
            eastVR.Scale = 30d;
            eastVR.Start = vectorDrawPosition;
            eastVR.SetLabel("east");
            eastVR.Width = 0.01d;
        }

        /// <summary>
        /// stops the drawing of the vectors
        /// </summary>
        private void CloseVectors()
        {
            northVR.SetShow(false);
            eastVR.SetShow(false);
            fwdVR.SetShow(false);
            upVR.SetShow(false);
            rightVR.SetShow(false);
        }

        /// <summary>
        /// sets the latitude and lognitude from the deltas of north and east and creates a new reference vector
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        internal void Setlatlng(double north, double east)
        {
            body = Planetarium.fetch.CurrentMainBody;
            double latOffset = north / (body.Radius * KKMath.deg2rad);
            latitude += latOffset;
            double lonOffset = east / (body.Radius * KKMath.deg2rad);
            longitude += lonOffset * Math.Cos(Mathf.Deg2Rad * latitude);

            referenceVector = body.GetRelSurfaceNVector(latitude, longitude).normalized * body.Radius;
            saveSettings();
        }


        /// <summary>
        /// rotates a object around an right axis by an amount
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="amount"></param>
        internal void SetPitch(float amount)
        {
            Vector3 upProjeced = Vector3.ProjectOnPlane(orientation, Vector3.forward);
            double compensation = Vector3.Dot(Vector3.right, upProjeced);
            double internalRotation = rotation - compensation;
            Vector3 realRight = KKMath.RotateVector(Vector3.right, Vector3.back, internalRotation);

            Quaternion rotate = Quaternion.AngleAxis(amount, realRight);
            orientation = rotate * orientation;

            Vector3 oldfwd = selectedObject.gameObject.transform.forward;
            Vector3 oldright = selectedObject.gameObject.transform.right;
            saveSettings();
            Vector3 newfwd = selectedObject.gameObject.transform.forward;

            // compensate for unwanted rotation
            float deltaAngle = Vector3.Angle(Vector3.ProjectOnPlane(oldfwd, upVector), Vector3.ProjectOnPlane(newfwd, upVector));
            if (Vector3.Dot(oldright, newfwd) > 0)
            {
                deltaAngle *= -1f;
            }
            rotation += deltaAngle;

            saveSettings();
        }

        /// <summary>
        /// rotates a object around forward axis by an amount
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="amount"></param>
        internal void SetRoll(float amount)
        {


            Vector3 upProjeced = Vector3.ProjectOnPlane(orientation, Vector3.forward);
            double compensation = Vector3.Dot(Vector3.right, upProjeced);
            double internalRotation = rotation - compensation;
            Vector3 realFwd = KKMath.RotateVector(Vector3.forward, Vector3.right, internalRotation);

            Quaternion rotate = Quaternion.AngleAxis(amount, realFwd);
            orientation = rotate * orientation;


            Vector3 oldfwd = selectedObject.gameObject.transform.forward;
            Vector3 oldright = selectedObject.gameObject.transform.right;
            Vector3 oldup = selectedObject.gameObject.transform.up;
            saveSettings();
            Vector3 newfwd = selectedObject.gameObject.transform.forward;

            Vector3 deltaVector = oldfwd - newfwd;

            // try to compensate some of the pitch
            float deltaUpAngle = Vector3.Dot(oldup, deltaVector);
            if (Math.Abs(deltaUpAngle) > 0.0001)
            {
                SetPitch(-1 * deltaUpAngle);
            }

            // compensate for unwanted rotation
            float deltaAngle = Vector3.Angle(Vector3.ProjectOnPlane(oldfwd, upVector), Vector3.ProjectOnPlane(newfwd, upVector));
            if (Vector3.Dot(oldright, newfwd) > 0)
            {
                deltaAngle *= -1f;
            }
            rotation += deltaAngle;
            saveSettings();
        }


        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(double increment)
        {
            rotation += increment;
            rotation = (360d + rotation) % 360d;
            saveSettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            // adjust transform for scaled models
            direction = direction / modelScale;
            direction = selectedObject.gameObject.transform.TransformVector(direction);
            double northInc = Vector3d.Dot(northVector, direction);
            double eastInc = Vector3d.Dot(eastVector, direction);
            double upInc = Vector3d.Dot(upVector, direction);

            altitude += upInc;
            Setlatlng(northInc, eastInc);
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void saveSettings()
        {
            selectedObject.Orientation = orientation;

            if (modelScale < 0.01f)
                modelScale = 0.01f;

            rotation = (360d + rotation) % 360d;

            if (vis > (float)KerbalKonstructs.instance.maxEditorVisRange)
            {
                vis = (float)KerbalKonstructs.instance.maxEditorVisRange;
            }
            if (vis < 1000)
            {
                vis = 1000;
            }

            selectedObject.RadialPosition = referenceVector;

            selectedObject.RadiusOffset = (float)altitude;
            selectedObject.RotationAngle = (float)rotation;
            selectedObject.VisibilityRange = vis;
            selectedObject.RefLatitude = latitude;
            selectedObject.RefLongitude = longitude;

            selectedObject.FacilityType =  facType;
            selectedObject.Group= sGroup;

            selectedObject.ModelScale =  modelScale;


            selectedObject.isScanable =  isScanable;

            updateSelection(selectedObject);

        }

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



        void FixDrift(bool bRotate = false)
        {
            if (selectedSnapPoint == null || selectedSnapPoint2 == null) return;
            if (selectedObject == null || snapTargetInstance == null) return;

            Vector3 snapSourceLocalPos = selectedSnapPoint.transform.localPosition;
            Vector3 snapSourceWorldPos = selectedSnapPoint.transform.position;
            Vector3 selSourceWorldPos = selectedObject.gameObject.transform.position;
            float selSourceRot = selectedObject.pqsCity.reorientFinalAngle;
            Vector3 snapTargetLocalPos = selectedSnapPoint2.transform.localPosition;
            Vector3 snapTargetWorldPos = selectedSnapPoint2.transform.position;
            Vector3 selTargetWorldPos = snapTargetInstance.gameObject.transform.position;
            float selTargetRot = snapTargetInstance.pqsCity.reorientFinalAngle;
            var spdist = 0f;

            if (!bRotate) spdist = Vector3.Distance(snapSourceWorldPos, snapTargetWorldPos);
            if (bRotate) spdist = Vector3.Distance(selSourceRot * snapSourceWorldPos, selTargetRot * snapTargetWorldPos);

            int iGiveUp = 0;

            while (spdist > 0.01 && iGiveUp < 100)
            {
                if (!bRotate)
                {
                    snpspos = selectedSnapPoint.transform.position;
                    snptpos = selectedSnapPoint2.transform.position;

                    vDrift = snpspos - snptpos;
                    vCurrpos = selectedObject.pqsCity.repositionRadial;
                    selectedObject.RadialPosition = vCurrpos + vDrift;
                    updateSelection(selectedObject);

                    spdist = Vector3.Distance(selectedSnapPoint.transform.position, selectedSnapPoint2.transform.position);
                    iGiveUp = iGiveUp + 1;
                }

                if (bRotate)
                {
                    iGiveUp = 100;
                }
            }
        }

        float getIncrement
        {
            get
            {
                return float.Parse(increment);
            }
        }

        internal void CheckEditorKeys()
        {
            if (selectedObject != null)
            {

                if (IsOpen())
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        SetTransform(Vector3.forward * getIncrement);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        SetTransform(Vector3.back * getIncrement);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        SetTransform(Vector3.right * getIncrement);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        SetTransform(Vector3.left * getIncrement);
                    }
                    if (Input.GetKey(KeyCode.E))
                    {
                        SetRotation(-(double)getIncrement);
                    }
                    if (Input.GetKey(KeyCode.Q))
                    {
                        SetRotation((double)getIncrement);
                    }

                    if (Input.GetKey(KeyCode.PageUp))
                    {
                        altitude += (double)getIncrement;
                        saveSettings();
                    }

                    if (Input.GetKey(KeyCode.PageDown))
                    {
                        altitude += -(double)getIncrement;
                        saveSettings();
                    }
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        saveSettings();
                    }
                }

            }

        }

        void SnapToTarget(bool bRotate = false)
        {
            if (selectedSnapPoint == null || selectedSnapPoint2 == null) return;
            if (selectedObject == null || snapTargetInstance == null) return;

            Vector3 snapPointRelation = new Vector3(0, 0, 0);
            Vector3 snapPoint2Relation = new Vector3(0, 0, 0);
            Vector3 snapVector = new Vector3(0, 0, 0);
            Vector3 snapVectorNoRot = new Vector3(0, 0, 0);
            Vector3 vFinalPos = new Vector3(0, 0, 0);

            Vector3 snapSourcePos = selectedSnapPoint.transform.localPosition;
            snapSourceWorldPos = selectedSnapPoint.transform.position;
            Vector3 selSourcePos = selectedObject.gameObject.transform.position;
            float selSourceRot = selectedObject.pqsCity.reorientFinalAngle;
            Vector3 snapTargetPos = selectedSnapPoint2.transform.localPosition;
            snapTargetWorldPos = selectedSnapPoint2.transform.position;
            Vector3 selTargetPos = snapTargetInstance.gameObject.transform.position;
            float selTargetRot = snapTargetInstance.pqsCity.reorientFinalAngle;

            vbsnapangle1 = selectedSnapPoint.transform.position;
            vbsnapangle2 = selectedSnapPoint2.transform.position;

            if (!bRotate)
            {
                // Quaternion quatSelObj = Quaternion.AngleAxis(selSourceRot, selSourcePos);
                snapPointRelation = snapSourcePos;
                //quatSelObj * snapSourcePos;

                //Quaternion quatSelTar = Quaternion.AngleAxis(selTargetRot, selTargetPos);
                snapPoint2Relation = snapTargetPos;
                //quatSelTar * snapTargetPos;

                snapVector = (snapPoint2Relation - snapPointRelation);
                vFinalPos = snapTargetInstance.RadialPosition + snapVector;
            }
            else
            {
                // THIS SHIT DO NOT WORK
                //MiscUtils.HUDMessage("Snapping with rotation.", 60, 2);
                // Stick the origins on each other
                vFinalPos = snapTargetInstance.RadialPosition;
                selectedObject.RadialPosition = vFinalPos;
                updateSelection(selectedObject);

                // Get the offset of the source and move by that
                Vector3 vAngles = new Vector3(0, selectedObject.pqsCity.reorientFinalAngle, 0);
                snapPointRelation = selectedObject.gameObject.transform.position -
                    selectedSnapPoint.transform.TransformPoint(selectedSnapPoint.transform.localPosition);
                MiscUtils.HUDMessage("" + snapPointRelation.ToString(), 60, 2);
                vFinalPos = snapTargetInstance.pqsCity.repositionRadial + snapPointRelation;
                selectedObject.RadialPosition = vFinalPos;
                updateSelection(selectedObject);

                // Get the offset of the target and move by that
                vAngles = new Vector3(0, snapTargetInstance.pqsCity.reorientFinalAngle, 0);
                snapPoint2Relation = snapTargetInstance.gameObject.transform.position -
                    selectedSnapPoint2.transform.TransformPoint(selectedSnapPoint2.transform.localPosition);
                MiscUtils.HUDMessage("" + snapPoint2Relation.ToString(), 60, 2);
                vFinalPos = snapTargetInstance.pqsCity.repositionRadial + snapPoint2Relation;
            }

            snapSourcePos = selectedSnapPoint.transform.localPosition;
            snapTargetPos = selectedSnapPoint2.transform.localPosition;
            snapVectorNoRot = (snapSourcePos - snapTargetPos);

            selectedObject.RadialPosition = vFinalPos;
            selectedObject.RadiusOffset = (float)snapTargetInstance.RadiusOffset + snapVectorNoRot.y;

            updateSelection(selectedObject);
            if (!bRotate) FixDrift();
        }

        #endregion
    }
}

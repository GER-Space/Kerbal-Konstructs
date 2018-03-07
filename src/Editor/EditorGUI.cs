using System;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.UI
{
    public class EditorGUI : KKWindow
    {

        private static EditorGUI _instance = null;
        public static EditorGUI instance
        {
            get
            { if   (_instance == null)
                {
                    _instance = new EditorGUI();
                    
                }
                return _instance;
            }
        }

        #region Variable Declarations
        private List<Transform> transformList = new List<Transform>();
        private CelestialBody body;

        internal Boolean foldedIn = false;
        internal Boolean doneFold = false;



        #region Texture Definitions
        // Texture definitions
        internal Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);        
        internal Texture tCopyPos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/copypos", false);
        internal Texture tPastePos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/pastepos", false);                       
        internal Texture tSnap = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/snapto", false);
        internal Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        internal Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        internal Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);


        #endregion

        #region Switches
        // Switches
        internal Boolean enableColliders = false;
        //internal static bool isScanable = false;

        //public static Boolean editingLaunchSite = false;

        //   public static Boolean editingFacility = false;

        internal Boolean SnapRotateMode = false;
        internal bool grasColorModeIsAuto = true;

        #endregion

        #region GUI Windows
        // GUI Windows
        internal Rect toolRect = new Rect(300, 35, 330, 756);

        #endregion


        #region Holders
        // Holders

        internal static StaticInstance selectedObject = null;
        internal StaticInstance selectedObjectPrevious = null;
        internal static KKLaunchSite lTargetSite = null;

        //internal static String facType = "None";
        //internal static String sGroup = "Ungrouped";
        private float increment = 0.1f;


        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();

        private Vector3d savedposition;
        private float savedalt;
        private float savedrot;
        private bool savedpos = false;

        private static Space referenceSystem = Space.Self;

        private static Vector3d position = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;

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

                DrawObject.DrawObjects(KerbalKonstructs.instance.selectedObject.gameObject);
            }
        }


        public override void Close()
        {
            CloseVectors();
            CloseEditors();
            base.Close();
        }

        #region draw Methods

        /// <summary>
        /// Wrapper to draw editors
        /// </summary>
        /// <param name="obj"></param>
        public void drawEditor(StaticInstance obj)
        {
            if (obj == null)
            {
                return;
            }

            if (selectedObject != obj)
            {
                selectedObject = obj;
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

            toolRect = GUI.Window(0xB00B1E3, toolRect, InstanceEditorWindow, "", UIMain.KKWindow);

        }

        #endregion

        #region Editors

        #region Instance Editor

        /// <summary>
        /// Instance Editor window
        /// </summary>
        /// <param name="windowID"></param>
        void InstanceEditorWindow(int windowID)
        {

            UpdateVectors();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Instance Editor", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();

            if (foldedIn) tFolded = tFoldOut;
            if (!foldedIn) tFolded = tFoldIn;

            if (GUILayout.Button(tFolded, GUILayout.Height(23), GUILayout.Width(23)))
            {
                    foldedIn = !foldedIn;
            }

            GUILayout.Button(selectedObject.model.title + " ("+ selectedObject.indexInGroup.ToString() + ")", GUILayout.Height(23));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Position");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent(tCopyPos, "Copy Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    savedpos = true;
                    savedposition = selectedObject.gameObject.transform.position;
                    savedReferenceVector = selectedObject.RadialPosition;

                    savedalt = selectedObject.RadiusOffset;
                    savedrot = selectedObject.RotationAngle;
                    // Debug.Log("KK: Instance position copied");
                }
                if (GUILayout.Button(new GUIContent(tPastePos, "Paste Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    if (savedpos)
                    {
                        selectedObject.gameObject.transform.position = savedposition;
                        selectedObject.RadialPosition = savedReferenceVector;
                        selectedObject.RadiusOffset = savedalt;
                        selectedObject.RotationAngle = savedrot;
                        ApplySettings();
                        // Debug.Log("KK: Instance position pasted");
                    }
                }

                if (!foldedIn)
                {
                    if (GUILayout.Button(new GUIContent(tSnap, "Snap to Target"), GUILayout.Width(23), GUILayout.Height(23)))
                    {
                        if (StaticsEditorGUI.instance.snapTargetInstance == null)
                        {
                            Log.UserError("No Snaptarget selected");
                        }
                        else
                        {
                            selectedObject.RadialPosition = StaticsEditorGUI.instance.snapTargetInstance.RadialPosition;
                            selectedObject.RadiusOffset = StaticsEditorGUI.instance.snapTargetInstance.RadiusOffset;
                            selectedObject.RotationAngle = StaticsEditorGUI.instance.snapTargetInstance.RotationAngle;
                            ApplySettings();
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                if (!foldedIn)
                {
                    GUILayout.Label("Increment");
                    increment = float.Parse(GUILayout.TextField(increment.ToString(), 5, GUILayout.Width(48)));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("0.001", GUILayout.Height(18)))
                    {
                        increment = 0.001f;
                    }
                    if (GUILayout.Button("0.01", GUILayout.Height(18)))
                    {
                        increment = 0.01f;
                    }
                    if (GUILayout.Button("0.1", GUILayout.Height(18)))
                    {
                        increment = 0.1f;
                    }
                    if (GUILayout.Button("1", GUILayout.Height(18)))
                    {
                        increment = 1f;
                    }
                    if (GUILayout.Button("10", GUILayout.Height(18)))
                    {
                        increment = 10f;
                    }
                    if (GUILayout.Button("25", GUILayout.Height(16)))
                    {
                        increment = 25f;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    GUILayout.Label("i");
                    increment = float.Parse(GUILayout.TextField(increment.ToString(), 3, GUILayout.Width(25)));

                    if (GUILayout.Button("0.1", GUILayout.Height(23)))
                    {
                        increment = 0.1f;
                    }
                    if (GUILayout.Button("1", GUILayout.Height(23)))
                    {
                        increment = 1f;
                    }
                    if (GUILayout.Button("10", GUILayout.Height(23)))
                    {
                        increment = 10f;
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

            if (GUILayout.Button(new GUIContent(UIMain.iconCubes, "Model"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.Self;
                UpdateVectors();
            }

            GUI.enabled = (referenceSystem == Space.Self);
            if (GUILayout.Button(new GUIContent(UIMain.iconWorld, "World"), GUILayout.Height(23), GUILayout.Width(23)))
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
                    SetTransform(Vector3.back * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.forward * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Left / Right:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.left * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.right * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Down / Up:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.down * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.up * increment);
                }

            }
            else
            {
                GUILayout.Label("West / East :");
                GUILayout.FlexibleSpace();

                if (foldedIn) fTempWidth = 40f;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, -increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("South / North:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(-increment, 0d);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(increment, 0d);
                }
            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;

            if (!foldedIn)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Box("Latitude");
                    GUILayout.Box(selectedObject.RefLatitude.ToString("#0.0000000"));
                    GUILayout.Box("Longitude");
                    GUILayout.Box(selectedObject.RefLongitude.ToString("#0.0000000"));
                }
                GUILayout.EndHorizontal();
            }


            //
            // Set reference butons
            //
            GUILayout.BeginHorizontal();
            GUILayout.Label("Altitude Reference: ");
            GUILayout.Label(selectedObject.heighReference.ToString(), UIMain.LabelWhite);
            GUILayout.FlexibleSpace();
            GUI.enabled = (selectedObject.heighReference != HeightReference.Terrain );

            if (GUILayout.Button(new GUIContent(UIMain.iconTerrain, "Terrain"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                selectedObject.heighReference = HeightReference.Terrain;
                selectedObject.RadiusOffset = (float)(selectedObject.RadiusOffset - (selectedObject.CelestialBody.pqsController.GetSurfaceHeight(selectedObject.RadialPosition) - selectedObject.CelestialBody.pqsController.radius));

                ApplySettings();
            }

            GUI.enabled = (selectedObject.heighReference != HeightReference.Sphere);
            if (GUILayout.Button(new GUIContent(UIMain.iconWorld, "Sphere"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                selectedObject.heighReference = HeightReference.Sphere;
                selectedObject.RadiusOffset = (float)selectedObject.CelestialBody.GetAltitude(selectedObject.gameObject.transform.position);

                ApplySettings();
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // 
            // Altitude editing
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Alt.");
                GUILayout.FlexibleSpace();
                selectedObject.RadiusOffset = float.Parse(GUILayout.TextField(selectedObject.RadiusOffset.ToString(), 25, GUILayout.Width(fTempWidth)));
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    selectedObject.RadiusOffset -= increment;
                    ApplySettings();
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    selectedObject.RadiusOffset += increment;
                    ApplySettings();
                }
            }
            GUILayout.EndHorizontal();

            if (!foldedIn)
            {
                if (GUILayout.Button("Snap to Terrain", GUILayout.Height(21)))
                {
                    selectedObject.RadiusOffset = 1.0f + (float)(selectedObject.CelestialBody.pqsController.GetSurfaceHeight(selectedObject.RadialPosition) - selectedObject.CelestialBody.Radius);
                    ApplySettings();
                }
            }

            GUI.enabled = true;

            if (!foldedIn)
                GUILayout.Space(5);


            fTempWidth = 80f;

            GUI.enabled = true;

            if (!foldedIn)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Vis.");
                    GUILayout.FlexibleSpace();
                    selectedObject.VisibilityRange = Math.Max(1000f, float.Parse(GUILayout.TextField(selectedObject.VisibilityRange.ToString(), 6, GUILayout.Width(80))));
                    if (GUILayout.Button("Min", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.VisibilityRange = 1000f;
                        ApplySettings();
                    }
                    if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.VisibilityRange =  Math.Max(1000f, selectedObject.VisibilityRange - 2500f);
                        ApplySettings();
                    }
                    if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.VisibilityRange = Math.Min((float)KerbalKonstructs.instance.maxEditorVisRange, selectedObject.VisibilityRange + 2500f);
                        ApplySettings();
                    }
                    if (GUILayout.Button("Max", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.VisibilityRange = (float)KerbalKonstructs.instance.maxEditorVisRange;
                        ApplySettings();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            if (!foldedIn)
            {
                //
                // Orientation quick preset
                //
                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Orientation");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("U", "Top Up"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        selectedObject.Orientation = new Vector3(0, 1, 0);
                        ApplySettings();
                    }
                    if (GUILayout.Button(new GUIContent("D", "Bottom Up"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        selectedObject.Orientation = new Vector3(0, -1, 0);
                        ApplySettings();
                    }
                    if (GUILayout.Button(new GUIContent("L", "On Left"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        selectedObject.Orientation = new Vector3(1, 0, 0);
                        ApplySettings();
                    }
                    if (GUILayout.Button(new GUIContent("R", "On Right"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        selectedObject.Orientation = new Vector3(-1, 0, 0);
                        ApplySettings();
                    }
                    if (GUILayout.Button(new GUIContent("F", "On Front"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        selectedObject.Orientation = new Vector3(0, 0, 1);
                        ApplySettings();
                    }
                    if (GUILayout.Button(new GUIContent("B", "On Back"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        selectedObject.Orientation = new Vector3(0, 0, -1);
                        ApplySettings();
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
                        SetPitch(increment);
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetPitch(-increment);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Roll:");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetRoll(increment);
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetRoll(-increment);
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
                        SetRotation(-increment);
                    }
                    if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(-increment);
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(increment);
                    }
                    if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        SetRotation(increment);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);
                //
                // Scale
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Model Scale: ");
                    GUILayout.FlexibleSpace();
                    selectedObject.ModelScale = Math.Max(0.01f,float.Parse(GUILayout.TextField(selectedObject.ModelScale.ToString(), 4, GUILayout.Width(fTempWidth))));

                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.ModelScale = Math.Max(0.01f, selectedObject.ModelScale - increment);
                        ApplySettings();
                    }
                    if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.ModelScale = Math.Max(0.01f, selectedObject.ModelScale - increment);
                        ApplySettings();
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.ModelScale += increment;
                        ApplySettings();
                    }
                    if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    {
                        selectedObject.ModelScale += increment;
                        ApplySettings();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

            }

            GUI.enabled = true;

            if (!foldedIn)
            {

                if (GUILayout.Button("Facility Type: " + selectedObject.facilityType.ToString(), GUILayout.Height(23)))
                {
                    if (!FacilityEditor.instance.IsOpen())
                    {
                        FacilityEditor.instance.Open();
                    }
                }
            }

            if (!foldedIn)
            {
                if (selectedObject.model.modules.Where(x => x.moduleClassname == "GrasColor").Count() > 0)
                {
                    GUILayout.BeginHorizontal();
                    {

                        grasColorModeIsAuto = GUILayout.Toggle(grasColorModeIsAuto, "Auto GrassColor", GUILayout.Width(70), GUILayout.Height(23));
                        if (!grasColorModeIsAuto)
                        {
                            selectedObject.GrasColor.r = float.Parse(GUILayout.TextField(selectedObject.GrasColor.r.ToString(), 5, GUILayout.Width(48)));
                            selectedObject.GrasColor.g = float.Parse(GUILayout.TextField(selectedObject.GrasColor.g.ToString(), 5, GUILayout.Width(48)));
                            selectedObject.GrasColor.b = float.Parse(GUILayout.TextField(selectedObject.GrasColor.b.ToString(), 5, GUILayout.Width(48)));
                            selectedObject.GrasColor.a = float.Parse(GUILayout.TextField(selectedObject.GrasColor.a.ToString(), 5, GUILayout.Width(48)));
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Apply: " + selectedObject.facilityType.ToString(), GUILayout.Height(15)))
                    {
                        ApplySettings();
                    }
                }
            }




            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Group: ", GUILayout.Height(23));
                GUILayout.FlexibleSpace();

                if (!foldedIn)
                    selectedObject.Group = GUILayout.TextField(selectedObject.Group, 30, GUILayout.Width(185), GUILayout.Height(23));
                else
                    selectedObject.Group = GUILayout.TextField(selectedObject.Group, 30, GUILayout.Width(135), GUILayout.Height(23));
            }
            GUILayout.EndHorizontal();


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
                        KerbalKonstructs.instance.deselectObject(true, true);
                        SpawnInstance(oModel, fOffset, vPosition, fAngle);
                        MiscUtils.HUDMessage("Spawned duplicate " + selectedObject.model.title, 10, 2);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    bool isScanable2 = GUILayout.Toggle(selectedObject.isScanable, "Static will show up on anomaly scanners", GUILayout.Width(250), GUILayout.Height(23));
                    if (isScanable2 != selectedObject.isScanable)
                    {
                        selectedObject.isScanable = isScanable2;
                        ApplySettings();
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

            }

            if (foldedIn)
            {
                if (GUILayout.Button("Duplicate", GUILayout.Height(23)))
                {
                    selectedObject.SaveConfig();
                    StaticModel oModel = selectedObject.model;
                    float fOffset = selectedObject.RadiusOffset;
                    Vector3 vPosition = selectedObject.RadialPosition;
                    float fAngle = selectedObject.RotationAngle;
                    KerbalKonstructs.instance.deselectObject(true, true);
                    SpawnInstance(oModel, fOffset, vPosition, fAngle);
                    MiscUtils.HUDMessage("Spawned duplicate " + selectedObject.model.title, 10, 2);
                }
            }

            GUI.enabled = true;

            GUI.enabled = !LaunchSiteEditor.instance.IsOpen();
            // Make a new LaunchSite here:
            if (!foldedIn)
            {
                if (!selectedObject.hasLauchSites && string.IsNullOrEmpty(selectedObject.model.DefaultLaunchPadTransform))
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button((selectedObject.hasLauchSites ? "Edit" : "Make") + " Launchsite", GUILayout.Height(23)))
                {
                    LaunchSiteEditor.instance.Open();
                }
            }

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    selectedObject.SaveConfig();
                    MiscUtils.HUDMessage("Saved changes to this object.", 10, 2);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save&Close", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    selectedObject.SaveConfig();
                    MiscUtils.HUDMessage("Saved changes to this object.", 10, 2);
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            if (!foldedIn)
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete Instance", GUILayout.Height(21)))
                {
                    DeleteInstance();                   
                }
                GUILayout.Space(15);
            }


            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

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
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
        }


        #endregion

        #region Utility Functions


        internal void DeleteInstance ()
        {
            if (StaticsEditorGUI.instance.snapTargetInstance == selectedObject)
                StaticsEditorGUI.instance.snapTargetInstance = null;
            if (StaticsEditorGUI.instance.selectedObjectPrevious == selectedObject)
                StaticsEditorGUI.instance.selectedObjectPrevious = null;
            if (selectedObjectPrevious == selectedObject)
                selectedObjectPrevious = null;


            if (selectedObject.hasLauchSites)
            {
                LaunchSiteManager.DeleteLaunchSite(selectedObject.launchSite);
            }

            KerbalKonstructs.instance.deleteObject(selectedObject);
            selectedObject = null;

            return;
        }


        /// <summary>
        /// Spawns an Instance of an defined StaticModel 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fOffset"></param>
        /// <param name="vPosition"></param>
        /// <param name="fAngle"></param>
        /// <returns></returns>
        public void SpawnInstance(StaticModel model, float fOffset, Vector3 vPosition, float fAngle)
        {
            StaticInstance instance = new StaticInstance();
            instance.gameObject = UnityEngine.Object.Instantiate(model.prefab);
            instance.RadiusOffset = fOffset;
            instance.CelestialBody = KerbalKonstructs.instance.getCurrentBody();
            string newGroup = (selectedObject != null) ? (string)selectedObject.Group : "Ungrouped";
            instance.Group = newGroup;
            instance.RadialPosition = vPosition;
            instance.RotationAngle = fAngle;
            instance.Orientation = Vector3.up;
            instance.VisibilityRange = 25000f;
            instance.RefLatitude = KKMath.GetLatitudeInDeg(instance.RadialPosition);
            instance.RefLongitude = KKMath.GetLongitudeInDeg(instance.RadialPosition);

            instance.model = model;
            Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath );
            instance.configPath = KerbalKonstructs.newInstancePath + "/" + model.name + "-instances.cfg";
            instance.configUrl = null;

            enableColliders = false;
            instance.spawnObject(true, false);
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
                return (Vector3)body.GetSurfaceNVector(selectedObject.RefLatitude, selectedObject.RefLongitude).normalized;
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
            selectedObject.RefLatitude += latOffset;
            double lonOffset = east / (body.Radius * KKMath.deg2rad);
            selectedObject.RefLongitude += lonOffset * Math.Cos(Mathf.Deg2Rad * selectedObject.RefLatitude);

            selectedObject.RadialPosition = body.GetRelSurfaceNVector(selectedObject.RefLatitude, selectedObject.RefLongitude).normalized * body.Radius;
            ApplySettings();
        }


        /// <summary>
        /// rotates a object around an right axis by an amount
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="amount"></param>
        internal void SetPitch(float amount)
        {
            Vector3 upProjeced = Vector3.ProjectOnPlane(selectedObject.Orientation, Vector3.forward);
            double compensation = Vector3.Dot(Vector3.right, upProjeced);
            double internalRotation = selectedObject.RotationAngle - compensation;
            Vector3 realRight = KKMath.RotateVector(Vector3.right, Vector3.back, internalRotation);

            Quaternion rotate = Quaternion.AngleAxis(amount, realRight);
            selectedObject.Orientation = rotate * selectedObject.Orientation;

            Vector3 oldfwd = selectedObject.gameObject.transform.forward;
            Vector3 oldright = selectedObject.gameObject.transform.right;
            ApplySettings();
            Vector3 newfwd = selectedObject.gameObject.transform.forward;

            // compensate for unwanted rotation
            float deltaAngle = Vector3.Angle(Vector3.ProjectOnPlane(oldfwd, upVector), Vector3.ProjectOnPlane(newfwd, upVector));
            if (Vector3.Dot(oldright, newfwd) > 0)
            {
                deltaAngle *= -1f;
            }
            SetRotation(deltaAngle);

            ApplySettings();
        }

        /// <summary>
        /// rotates a object around forward axis by an amount
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="amount"></param>
        internal void SetRoll(float amount)
        {
            Vector3 upProjeced = Vector3.ProjectOnPlane(selectedObject.Orientation, Vector3.forward);
            double compensation = Vector3.Dot(Vector3.right, upProjeced);
            double internalRotation = selectedObject.RotationAngle - compensation;
            Vector3 realFwd = KKMath.RotateVector(Vector3.forward, Vector3.right, internalRotation);

            Quaternion rotate = Quaternion.AngleAxis(amount, realFwd);
            selectedObject.Orientation = rotate * selectedObject.Orientation;

            Vector3 oldfwd = selectedObject.gameObject.transform.forward;
            Vector3 oldright = selectedObject.gameObject.transform.right;
            Vector3 oldup = selectedObject.gameObject.transform.up;
            ApplySettings();
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
            SetRotation(deltaAngle);
            ApplySettings();
        }


        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(float increment)
        {
            selectedObject.RotationAngle += (float)increment;
            selectedObject.RotationAngle = (360f + selectedObject.RotationAngle) % 360f;
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            float oldTerrainHeight = 0f;
            float newTerrainHeight = 0f;
            if (selectedObject.heighReference == HeightReference.Terrain)
            {
                oldTerrainHeight = (float)(selectedObject.CelestialBody.pqsController.GetSurfaceHeight(selectedObject.RadialPosition));
            }
            // adjust transform for scaled models
            direction = direction / selectedObject.ModelScale;
            direction = selectedObject.gameObject.transform.TransformVector(direction);
            double northInc = Vector3d.Dot(northVector, direction);
            double eastInc = Vector3d.Dot(eastVector, direction);
            double upInc = Vector3d.Dot(upVector, direction);

            if (selectedObject.heighReference == HeightReference.Terrain)
            {
                newTerrainHeight = (float)(selectedObject.CelestialBody.pqsController.GetSurfaceHeight(selectedObject.RadialPosition));
            }

            selectedObject.RadiusOffset += (float)upInc + (oldTerrainHeight - newTerrainHeight);



            Setlatlng(northInc, eastInc);

        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void ApplySettings()
        {
            selectedObject.Update();
        }

        internal void CheckEditorKeys()
        {
            if (selectedObject != null)
            {

                if (IsOpen())
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        SetTransform(Vector3.forward * increment);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        SetTransform(Vector3.back * increment);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        SetTransform(Vector3.right * increment);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        SetTransform(Vector3.left * increment);
                    }
                    if (Input.GetKey(KeyCode.E))
                    {
                        SetRotation(-increment);
                    }
                    if (Input.GetKey(KeyCode.Q))
                    {
                        SetRotation(increment);
                    }

                    if (Input.GetKey(KeyCode.PageUp))
                    {
                        selectedObject.RadiusOffset += increment;
                        ApplySettings();
                    }

                    if (Input.GetKey(KeyCode.PageDown))
                    {
                        selectedObject.RadiusOffset -= increment;
                        ApplySettings();
                    }
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        ApplySettings();
                    }
                }

            }

        }
        #endregion
    }
}

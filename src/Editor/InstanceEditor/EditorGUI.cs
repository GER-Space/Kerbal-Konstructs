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

        private enum Reference
        {
            Model,
            Center
        }



        private static EditorGUI _instance = null;
        public static EditorGUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorGUI();

                }
                return _instance;
            }
        }

        #region Variable Declarations
        private List<Transform> transformList = new List<Transform>();
        private CelestialBody body;





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
        internal Boolean enableColliders2 = false;
        //internal static bool isScanable = false;

        //public static Boolean editingLaunchSite = false;

        //   public static Boolean editingFacility = false;

        internal Boolean SnapRotateMode = false;
        internal bool grasColorModeIsAuto = true;

        #endregion

        #region GUI Windows
        // GUI Windows
        internal Rect toolRect = new Rect(800, 120, 330, 730);

        #endregion


        #region Holders
        // Holders

        internal static StaticInstance selectedInstance = null;
        internal StaticInstance selectedObjectPrevious = null;
        internal static KKLaunchSite lTargetSite = null;

        //internal static String facType = "None";
        //internal static String sGroup = "Ungrouped";
        private float increment = 1f;


        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private Vector3d savedPosition;
        private bool savedpos = false;

        private static Reference referenceSystem = Reference.Center;

        private Vector3d savedRotation = Vector3d.zero;

        private string incrementStr, altStr, grasColorRStr, grasColorGStr, grasColorBStr, grasColorAStr, oriXStr, oriYStr, oriZStr, posXStr, posYStr, posZStr;

        private float fTempWidth = 80f;


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
        /// <param name="instance"></param>
        public void drawEditor(StaticInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            if (selectedInstance != instance)
            {
                selectedInstance = instance;
                SetupFields();
                SetupVectors();
            }

            toolRect = GUI.Window(0x000B1E3, toolRect, InstanceEditorWindow, "", UIMain.KKWindow);

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

            GUILayout.Button(selectedInstance.model.title + " (" + selectedInstance.indexInGroup.ToString() + ")", GUILayout.Height(23));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Position");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent(tCopyPos, "Copy Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    savedpos = true;
                    savedPosition = selectedInstance.gameObject.transform.localPosition;
                    savedRotation = selectedInstance.gameObject.transform.localEulerAngles;
                    // Debug.Log("KK: Instance position copied");
                }
                if (GUILayout.Button(new GUIContent(tPastePos, "Paste Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    if (savedpos)
                    {
                        selectedInstance.gameObject.transform.localPosition = savedPosition;
                        selectedInstance.gameObject.transform.localEulerAngles = savedRotation;
                        ApplySettings();
                        // Debug.Log("KK: Instance position pasted");
                    }
                }


                if (GUILayout.Button(new GUIContent(tSnap, "Snap to Target"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    if (StaticsEditorGUI.instance.snapTargetInstance == null)
                    {
                        Log.UserError("No Snaptarget selected");
                    }
                    else
                    {
                        selectedInstance.gameObject.transform.localPosition = StaticsEditorGUI.instance.snapTargetInstance.gameObject.transform.localPosition;
                        selectedInstance.gameObject.transform.localEulerAngles = StaticsEditorGUI.instance.snapTargetInstance.gameObject.transform.localEulerAngles;

                        ApplySettings();
                    }
                }


                GUILayout.FlexibleSpace();

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
            GUILayout.EndHorizontal();


            //
            // Set reference butons
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Reference System: ");
                GUILayout.Label(referenceSystem.ToString(), UIMain.LabelWhite);
                GUILayout.FlexibleSpace();
                GUI.enabled = (referenceSystem == Reference.Center);

                if (GUILayout.Button(new GUIContent(UIMain.iconCubes, "Model"), GUILayout.Height(23), GUILayout.Width(23)))
                {
                    referenceSystem = Reference.Model;
                    UpdateVectors();
                }

                GUI.enabled = (referenceSystem == Reference.Model);
                if (GUILayout.Button(new GUIContent(UIMain.iconWorld, "Group Center"), GUILayout.Height(23), GUILayout.Width(23)))
                {
                    referenceSystem = Reference.Center;
                    UpdateVectors();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();


            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Label("Rel. Position");
            //    GUILayout.FlexibleSpace();
            //    GUILayout.Label("X", GUILayout.Height(18));
            //    posXStr = (GUILayout.TextField(posXStr, 8, GUILayout.Width(48), GUILayout.Height(18)));
            //    GUILayout.Label("Y", GUILayout.Height(18));
            //    posYStr = (GUILayout.TextField(posYStr, 8, GUILayout.Width(48), GUILayout.Height(18)));
            //    GUILayout.Label("Z", GUILayout.Height(18));
            //    posZStr = (GUILayout.TextField(posZStr, 8, GUILayout.Width(48), GUILayout.Height(18)));

            //}
            //GUILayout.EndHorizontal();
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();


            GUILayout.Label("Back / Fwd:");
            GUILayout.FlexibleSpace();
            posZStr = (GUILayout.TextField(posZStr, 11, GUILayout.Width(fTempWidth)));

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
            posXStr = (GUILayout.TextField(posXStr, 11, GUILayout.Width(fTempWidth)));
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
            posYStr = (GUILayout.TextField(posYStr, 11, GUILayout.Width(fTempWidth)));
            if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
            {
                SetTransform(Vector3.down * increment);
            }
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
            {
                SetTransform(Vector3.up * increment);
            }



            GUILayout.EndHorizontal();

            GUI.enabled = true;


            if (GUILayout.Button("Snap to Terrain", GUILayout.Height(21)))
            {

                Vector3 pos = selectedInstance.CelestialBody.GetWorldSurfacePosition(selectedInstance.RefLatitude, selectedInstance.RefLongitude, selectedInstance.surfaceHeight);
                selectedInstance.gameObject.transform.position = pos;

                ApplySettings();
            }


            GUI.enabled = true;


            GUILayout.Space(5);



            //
            // Orientation quick preset
            //
            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);
            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Label("Euler Rot.");
            //    GUILayout.FlexibleSpace();
            //    GUILayout.Label("X", GUILayout.Height(18));
            //    oriXStr = (GUILayout.TextField(oriXStr, 6, GUILayout.Width(48), GUILayout.Height(18)));
            //    GUILayout.Label("Y", GUILayout.Height(18));
            //    oriYStr = (GUILayout.TextField(oriYStr, 6, GUILayout.Width(48), GUILayout.Height(18)));
            //    GUILayout.Label("Z", GUILayout.Height(18));
            //    oriZStr = (GUILayout.TextField(oriZStr, 6, GUILayout.Width(48), GUILayout.Height(18)));

            //}
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Orientation:");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Zero", "Zero Rotation"), GUILayout.Height(21), GUILayout.Width(50)))
                {
                    selectedInstance.gameObject.transform.localEulerAngles = Vector3.zero;
                    ApplySettings();
                }

                if (GUILayout.Button(new GUIContent("C-U", "allign UP to GroupCenter Up-vector"), GUILayout.Height(21), GUILayout.Width(50)))
                {

                    Vector3 newfwd = Vector3.ProjectOnPlane(selectedInstance.gameObject.transform.forward, selectedInstance.gameObject.transform.parent.up).normalized;
                    Quaternion rotation = new Quaternion();
                    rotation.SetLookRotation(newfwd, selectedInstance.gameObject.transform.parent.up);

                    selectedInstance.gameObject.transform.rotation = rotation;

                    ApplySettings();
                }
            }

            if (GUILayout.Button(new GUIContent("P-U", "alling UP to Position-Up"), GUILayout.Height(21), GUILayout.Width(50)))
            {

                Vector3 newfwd = Vector3.ProjectOnPlane(selectedInstance.gameObject.transform.forward, upVector).normalized;
                Quaternion rotation = new Quaternion();
                rotation.SetLookRotation(newfwd, upVector);

                selectedInstance.gameObject.transform.rotation = rotation;

                ApplySettings();

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
                oriXStr = (GUILayout.TextField(oriXStr, 8, GUILayout.Width(fTempWidth)));
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetRotation(Vector3.right, increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetRotation(Vector3.left, increment);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {

                GUILayout.Label("Roll:");
                GUILayout.FlexibleSpace();
                oriZStr = (GUILayout.TextField(oriZStr, 8, GUILayout.Width(fTempWidth)));
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetRotation(Vector3.forward, increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetRotation(Vector3.back, increment);
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
                //                    rotStr = GUILayout.TextField(rotStr, 9, GUILayout.Width(fTempWidth));
                //  GUILayout.Box(Vector3.Angle(Vector3.ProjectOnPlane(selectedInstance.gameObject.transform.forward, selectedInstance.gameObject.transform.up), selectedInstance.gameObject.transform.parent.forward).ToString(),  GUILayout.Width(fTempWidth));
                //GUILayout.Box(GetHeading(), GUILayout.Width(fTempWidth));
                oriYStr = (GUILayout.TextField(oriYStr, 8, GUILayout.Width(fTempWidth)));

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(Vector3.up, -increment);
                }
                if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(Vector3.up, -increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(Vector3.up, increment);
                }
                if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(Vector3.up, increment);
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
                selectedInstance.ModelScale = Math.Max(0.01f, float.Parse(GUILayout.TextField(selectedInstance.ModelScale.ToString(), 4, GUILayout.Width(fTempWidth))));

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    selectedInstance.ModelScale = Math.Max(0.01f, selectedInstance.ModelScale - increment);
                    ApplySettings();
                }
                if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    selectedInstance.ModelScale = Math.Max(0.01f, selectedInstance.ModelScale - increment);
                    ApplySettings();
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    selectedInstance.ModelScale += increment;
                    ApplySettings();
                }
                if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    selectedInstance.ModelScale += increment;
                    ApplySettings();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);



            GUI.enabled = true;



            if (GUILayout.Button("Facility Type: " + selectedInstance.facilityType.ToString(), GUILayout.Height(23)))
            {
                if (!FacilityEditor.instance.IsOpen())
                {
                    FacilityEditor.instance.Open();
                }
            }

            if (selectedInstance.model.modules.Where(x => x.moduleClassname == "GrasColor").Count() > 0)
            {


                grasColorModeIsAuto = GUILayout.Toggle(grasColorModeIsAuto, "Auto GrassColor", GUILayout.Width(70), GUILayout.Height(23));
                if (!grasColorModeIsAuto)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("R", GUILayout.Height(18));
                        grasColorRStr = (GUILayout.TextField(grasColorRStr, 5, GUILayout.Width(48), GUILayout.Height(18)));
                        GUILayout.Label("G", GUILayout.Height(18));
                        grasColorGStr = (GUILayout.TextField(grasColorGStr, 5, GUILayout.Width(48), GUILayout.Height(18)));
                        GUILayout.Label("B", GUILayout.Height(18));
                        grasColorBStr = (GUILayout.TextField(grasColorBStr, 5, GUILayout.Width(48), GUILayout.Height(18)));
                        GUILayout.Label("A", GUILayout.Height(18));
                        grasColorAStr = (GUILayout.TextField(grasColorAStr, 5, GUILayout.Width(48), GUILayout.Height(18)));

                        if (GUILayout.Button("Apply", GUILayout.Height(18)))
                        {
                            ApplyInputStrings();
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }





            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Set Group: ", GUILayout.Height(23));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(selectedInstance.Group, GUILayout.Width(185), GUILayout.Height(23)))
                {
                    GroupSelectorUI.instance.Close();
                    GroupSelectorUI.showOnlyLocal = true;
                    GroupSelectorUI.titleText = "Select a new Group";
                    GroupSelectorUI.callBack = SetGroup;
                    GroupSelectorUI.instance.Open();
                }

            }
            GUILayout.EndHorizontal();



            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            {
                enableColliders = GUILayout.Toggle(enableColliders, "Enable Colliders", GUILayout.Width(140), GUILayout.Height(23));

                if (enableColliders != enableColliders2)
                {
                    selectedInstance.ToggleAllColliders(enableColliders);
                    enableColliders2 = enableColliders;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Duplicate", GUILayout.Width(130), GUILayout.Height(23)))
                {
                    selectedInstance.SaveConfig();
                    KerbalKonstructs.instance.deselectObject(true, true);
                    SpawnInstance(selectedInstance.model, selectedInstance.groupCenter, selectedInstance.gameObject.transform.position, selectedInstance.Orientation);
                    MiscUtils.HUDMessage("Spawned duplicate " + selectedInstance.model.title, 10, 2);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                bool isScanable2 = GUILayout.Toggle(selectedInstance.isScanable, "Static will show up on anomaly scanners", GUILayout.Width(250), GUILayout.Height(23));
                if (isScanable2 != selectedInstance.isScanable)
                {
                    selectedInstance.isScanable = isScanable2;
                    ApplySettings();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);



            GUI.enabled = true;

            GUI.enabled = !LaunchSiteEditor.instance.IsOpen();
            // Make a new LaunchSite here:

            if (!selectedInstance.hasLauchSites && string.IsNullOrEmpty(selectedInstance.model.DefaultLaunchPadTransform))
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button((selectedInstance.hasLauchSites ? "Edit" : "Make") + " Launchsite", GUILayout.Height(23)))
            {
                LaunchSiteEditor.instance.Open();
            }
            GUILayout.FlexibleSpace();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    selectedInstance.SaveConfig();
                    MiscUtils.HUDMessage("Saved changes to this object.", 10, 2);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save&Close", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    selectedInstance.SaveConfig();
                    MiscUtils.HUDMessage("Saved changes to this object.", 10, 2);
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);



            if (GUILayout.Button("Delete Instance", GUILayout.Height(21)))
            {
                DeleteInstance();
            }
            GUILayout.Space(5);



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


        internal void DeleteInstance()
        {
            if (StaticsEditorGUI.instance.snapTargetInstance == selectedInstance)
                StaticsEditorGUI.instance.snapTargetInstance = null;
            if (StaticsEditorGUI.instance.selectedObjectPrevious == selectedInstance)
                StaticsEditorGUI.instance.selectedObjectPrevious = null;
            if (selectedObjectPrevious == selectedInstance)
                selectedObjectPrevious = null;


            if (selectedInstance.hasLauchSites)
            {
                LaunchSiteManager.DeleteLaunchSite(selectedInstance.launchSite);
            }


            KerbalKonstructs.instance.DeleteInstance(selectedInstance);
            selectedInstance = null;

            StaticsEditorGUI.ResetInstancesList();

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
        internal void SpawnInstance(StaticModel model, GroupCenter groupCenter, Vector3 position, Vector3 rotation)
        {
            StaticInstance instance = new StaticInstance();
            instance.model = model;
            instance.gameObject = UnityEngine.Object.Instantiate(model.prefab);
            instance.CelestialBody = FlightGlobals.currentMainBody;

            instance.groupCenter = groupCenter;
            instance.Group = groupCenter.Group;

            instance.gameObject.transform.position = position;
            instance.gameObject.transform.parent = groupCenter.gameObject.transform;

            instance.RelativePosition = instance.gameObject.transform.localPosition;
            instance.Orientation = rotation;

            if (!Directory.Exists(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath))
            {
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath);
            }
            instance.configPath = KerbalKonstructs.newInstancePath + "/" + model.name + "-instances.cfg";
            instance.configUrl = null;

            enableColliders = false;
            enableColliders2 = false;
            instance.SpawnObject(true);
        }

        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 vectorDrawPosition
        {
            get
            {
                //return (selectedInstance.gameObject.transform.position + 4 * selectedInstance.gameObject.transform.up.normalized + 4 * selectedInstance.gameObject.transform.right.normalized);
                return (selectedInstance.gameObject.transform.position);
            }
        }

        private string GetHeading()
        {
            float angle = Vector3.Angle(Vector3.ProjectOnPlane(selectedInstance.gameObject.transform.forward, selectedInstance.gameObject.transform.up), selectedInstance.gameObject.transform.parent.forward);

            if (Vector3.Angle(Vector3.ProjectOnPlane(selectedInstance.gameObject.transform.forward, selectedInstance.gameObject.transform.up), selectedInstance.gameObject.transform.parent.right) < 90)
            {
                return angle.ToString();
            }
            else
            {
                return (360 - angle).ToString();
            }

        }


        private Vector3 upVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return (Vector3)body.GetSurfaceNVector(selectedInstance.RefLatitude, selectedInstance.RefLongitude).normalized;
            }
        }

        /// <summary>
        /// Sets the vectors active and updates thier position and directions
        /// </summary>
        private void UpdateVectors()
        {
            if (selectedInstance == null)
            {
                return;
            }

            if (referenceSystem == Reference.Center)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                fwdVR.Vector = selectedInstance.groupCenter.gameObject.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.draw();

                upVR.Vector = selectedInstance.groupCenter.gameObject.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.draw();

                rightVR.Vector = selectedInstance.groupCenter.gameObject.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.draw();
            }
            else
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                fwdVR.Vector = selectedInstance.gameObject.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.draw();

                upVR.Vector = selectedInstance.gameObject.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.draw();

                rightVR.Vector = selectedInstance.gameObject.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.draw();
            }

        }

        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedInstance.groupCenter.gameObject.transform.forward;
            fwdVR.Scale = 30d;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedInstance.groupCenter.gameObject.transform.up;
            upVR.Scale = 30d;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedInstance.groupCenter.gameObject.transform.right;
            rightVR.Scale = 30d;
            rightVR.Start = vectorDrawPosition;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;

        }

        /// <summary>
        /// stops the drawing of the vectors
        /// </summary>
        private void CloseVectors()
        {
            fwdVR.SetShow(false);
            upVR.SetShow(false);
            rightVR.SetShow(false);
        }

        internal void SetupFields()
        {
            incrementStr = increment.ToString();
            altStr = selectedInstance.CelestialBody.GetAltitude(selectedInstance.gameObject.transform.position).ToString();
            grasColorRStr = selectedInstance.GrasColor.r.ToString();
            grasColorGStr = selectedInstance.GrasColor.g.ToString();
            grasColorBStr = selectedInstance.GrasColor.b.ToString();
            grasColorAStr = selectedInstance.GrasColor.a.ToString();

            oriXStr = Math.Round(selectedInstance.gameObject.transform.localEulerAngles.x, 4).ToString();
            oriYStr = Math.Round(selectedInstance.gameObject.transform.localEulerAngles.y, 4).ToString();
            oriZStr = Math.Round(selectedInstance.gameObject.transform.localEulerAngles.z, 4).ToString();

            posXStr = Math.Round(selectedInstance.gameObject.transform.localPosition.x, 4).ToString();
            posYStr = Math.Round(selectedInstance.gameObject.transform.localPosition.y, 4).ToString();
            posZStr = Math.Round(selectedInstance.gameObject.transform.localPosition.z, 4).ToString();

        }

        internal void ApplyInputStrings()
        {
            increment = float.Parse(incrementStr);


            selectedInstance.GrasColor.r = float.Parse(grasColorRStr);
            selectedInstance.GrasColor.g = float.Parse(grasColorGStr);
            selectedInstance.GrasColor.b = float.Parse(grasColorBStr);
            selectedInstance.GrasColor.a = float.Parse(grasColorAStr);

            selectedInstance.gameObject.transform.localPosition = new Vector3(float.Parse(posXStr), float.Parse(posYStr), float.Parse(posZStr));
            selectedInstance.gameObject.transform.localEulerAngles = new Vector3(float.Parse(oriXStr), float.Parse(oriYStr), float.Parse(oriZStr));


            ApplySettings();
        }




        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(Vector3 axis, float increment)
        {
            selectedInstance.gameObject.transform.Rotate(axis, increment);
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            direction = direction / selectedInstance.ModelScale;
            //selectedInstance.gameObject.transform.Translate(direction, Space.Self);
            if (referenceSystem == Reference.Center)
            {
                selectedInstance.gameObject.transform.localPosition += direction;
            }
            else
            {
                if (direction.y == 0)
                {
                    float oldAltitude, newAltitude;
                    oldAltitude = (float)selectedInstance.CelestialBody.GetAltitude(selectedInstance.gameObject.transform.position);
                    selectedInstance.gameObject.transform.Translate(direction);
                    newAltitude = (float)selectedInstance.CelestialBody.GetAltitude(selectedInstance.gameObject.transform.position);

                    float diff = newAltitude - oldAltitude;

                    selectedInstance.gameObject.transform.localPosition -= new Vector3(0, diff, 0);

                } else
                {
                    selectedInstance.gameObject.transform.localPosition += direction;
                }
                

            }
            ApplySettings();
        }

        /// <summary>
        /// CallBack Functions for Group Selection
        /// </summary>
        /// <param name="newGroup"></param>
        internal void SetGroup(GroupCenter newGroup)
        {
            Log.Normal("setting Group from " + selectedInstance.groupCenter.Group + " to " + newGroup.Group);
            StaticDatabase.ChangeGroup(selectedInstance, newGroup);
            ApplySettings();
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void ApplySettings()
        {
            selectedInstance.Update();
            SetupFields();
        }

        internal void CheckEditorKeys()
        {
            if (selectedInstance != null)
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
                        SetRotation(Vector3.up, -increment);
                    }
                    if (Input.GetKey(KeyCode.Q))
                    {
                        SetRotation(Vector3.up, increment);
                    }

                    if (Input.GetKey(KeyCode.PageUp))
                    {
                        selectedInstance.RadiusOffset += increment;
                        ApplySettings();
                    }

                    if (Input.GetKey(KeyCode.PageDown))
                    {
                        selectedInstance.RadiusOffset -= increment;
                        ApplySettings();
                    }
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        ApplyInputStrings();
                    }
                }

            }

        }
        #endregion
    }
}

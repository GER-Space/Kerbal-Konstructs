using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class EditorGUI : KKWindow
    {

        protected enum Reference
        {
            Model,
            Center
        }


        public static float windowWidth = 330;
        public static float windowHeight = 750;

        protected static EditorGUI _instance = null;
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
        protected List<Transform> transformList = new List<Transform>();
        protected CelestialBody body;


        #region Texture Definitions
        // Texture definitions
        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
        public Texture tCopyPos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/copypos", false);
        public Texture tPastePos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/pastepos", false);
        public Texture tSnap = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/snapto", false);
        public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);


        #endregion

        #region Switches
        // Switches
        public Boolean enableColliders = false;
        public Boolean enableColliders2 = false;
        //internal static bool isScanable = false;

        //public static Boolean editingLaunchSite = false;

        //   public static Boolean editingFacility = false;

        public Boolean SnapRotateMode = false;
        public static bool grasColorEnabled = false;
        public static bool grassColor2Enabled = false;

        #endregion

        #region GUI Windows
        // GUI Windows
        public static Rect toolRect = new Rect(1200, 60, windowWidth, windowHeight);

        #endregion


        #region Holders
        // Holders

        public static StaticInstance selectedInstance = null;
        public StaticInstance selectedObjectPrevious = null;
        public static KKLaunchSite lTargetSite = null;

        //internal static String facType = "None";
        //internal static String sGroup = "Ungrouped";
        protected float increment = 1f;

        protected VectorRenderer upVR = new VectorRenderer();
        protected VectorRenderer fwdVR = new VectorRenderer();
        protected VectorRenderer rightVR = new VectorRenderer();

        protected Vector3d savedPosition;
        protected bool savedpos = false;

        protected static Reference referenceSystem = Reference.Center;

        protected Vector3d savedRotation = Vector3d.zero;

        protected string incrementStr, altStr, oriXStr, oriYStr, oriZStr, posXStr, posYStr, posZStr;

        protected float fTempWidth = 80f;

        protected static double cameraDistance;

        protected static Vector3 origPosition, origRotation;
        protected static GroupCenter origCenter;
        protected static float origScale;

        protected static Vector3 northVector;
        protected static Vector3 eastVector;
        protected static Vector3 forwardVector;

        protected static Vector3 movement;
        protected static Vector3 localMovement;
        protected static Vector3 origLocalPosition;

        protected static GrasColor grassMod = null;
        protected static GrassColor2 grassMod2 = null;


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
                Log.Warning("No instance selected and editor is open --> Closing it");
                this.Close();
            }

            if ((KerbalKonstructs.selectedInstance != null) && (!KerbalKonstructs.selectedInstance.isPreview))
            {
                drawEditor(KerbalKonstructs.selectedInstance);

                DrawObject.DrawObjects(KerbalKonstructs.selectedInstance.gameObject);
            }
        }


        public override void Close()
        {
            CloseGizmo();
            CloseVectors();
            CloseEditors();
            base.Close();
            selectedInstance = null;
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
                SetupGizmo();

                grassMod = selectedInstance.mesh.GetComponent<GrasColor>();
                grassMod2 = selectedInstance.mesh.GetComponent<GrassColor2>();
                grasColorEnabled = (grassMod != null);
                grassColor2Enabled = (grassMod2 != null);

                origCenter = selectedInstance.groupCenter;
                origPosition = selectedInstance.transform.localPosition;
                origRotation = selectedInstance.transform.localEulerAngles;
                origScale = selectedInstance.ModelScale;
            }

            toolRect = GUI.Window(0x004B1E3, toolRect, InstanceEditorWindow, "", UIMain.KKWindow);

        }



        #endregion

        #region Editors

        #region Instance Editor

        /// <summary>
        /// Instance Editor window
        /// </summary>
        /// <param name="windowID"></param>
        public virtual void InstanceEditorWindow(int windowID)
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
                    KerbalKonstructs.DeselectObject(true, true);
                    //this.Close();
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
                    savedPosition = selectedInstance.transform.localPosition;
                    savedRotation = selectedInstance.transform.localEulerAngles;
                    // Debug.Log("KK: Instance position copied");
                }
                if (GUILayout.Button(new GUIContent(tPastePos, "Paste Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    if (savedpos)
                    {
                        selectedInstance.transform.localPosition = savedPosition;
                        selectedInstance.transform.localEulerAngles = savedRotation;
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
                        selectedInstance.transform.localPosition = StaticsEditorGUI.instance.snapTargetInstance.transform.localPosition;
                        selectedInstance.transform.localEulerAngles = StaticsEditorGUI.instance.snapTargetInstance.transform.localEulerAngles;

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
                    UpdateGizmo();
                    UpdateVectors();
                }

                GUI.enabled = (referenceSystem == Reference.Model);
                if (GUILayout.Button(new GUIContent(UIMain.iconWorld, "Group Center"), GUILayout.Height(23), GUILayout.Width(23)))
                {
                    referenceSystem = Reference.Center;
                    UpdateGizmo();
                    UpdateVectors();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            //
            // Model Switching
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Selected Variant: ");
                GUI.enabled = (selectedInstance.model.hasVariants);
                string vaiantstring = (String.IsNullOrEmpty(selectedInstance.VariantName)) ? "N.A." : selectedInstance.VariantName;

                if (GUILayout.Button(vaiantstring, GUILayout.Width(150), GUILayout.Height(21)))
                {
                    VariantSelector.staticInstance = selectedInstance;
                    VariantSelector.Open();
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();

            }
            GUILayout.EndHorizontal();
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();


            GUILayout.Label("Back / Fwd:");
            GUILayout.FlexibleSpace();
            posZStr = (GUILayout.TextField(posZStr, 11, GUILayout.Width(fTempWidth)));

            if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.back * increment);
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.forward * increment);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Left / Right:");
            GUILayout.FlexibleSpace();
            posXStr = (GUILayout.TextField(posXStr, 11, GUILayout.Width(fTempWidth)));

            if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.left * increment);
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.right * increment);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Down / Up:");
            GUILayout.FlexibleSpace();
            posYStr = (GUILayout.TextField(posYStr, 11, GUILayout.Width(fTempWidth)));

            if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.down * increment);
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.up * increment);



            GUILayout.EndHorizontal();

            GUI.enabled = true;


            if (GUILayout.Button("Snap to Terrain", GUILayout.Height(21)))
            {

                Vector3 pos = selectedInstance.CelestialBody.GetWorldSurfacePosition(selectedInstance.RefLatitude, selectedInstance.RefLongitude, selectedInstance.surfaceHeight);
                selectedInstance.transform.position = pos;

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
                    selectedInstance.transform.localEulerAngles = Vector3.zero;
                    ApplySettings();
                }

                if (GUILayout.Button(new GUIContent("C-U", "allign UP to GroupCenter Up-vector"), GUILayout.Height(21), GUILayout.Width(50)))
                {

                    Vector3 newfwd = Vector3.ProjectOnPlane(selectedInstance.transform.forward, selectedInstance.transform.parent.up).normalized;
                    Quaternion rotation = new Quaternion();
                    rotation.SetLookRotation(newfwd, selectedInstance.transform.parent.up);

                    selectedInstance.transform.rotation = rotation;

                    ApplySettings();
                }
            }

            if (GUILayout.Button(new GUIContent("P-U", "alling UP to Position-Up"), GUILayout.Height(21), GUILayout.Width(50)))
            {

                Vector3 newfwd = Vector3.ProjectOnPlane(selectedInstance.transform.forward, upVector).normalized;
                Quaternion rotation = new Quaternion();
                rotation.SetLookRotation(newfwd, upVector);

                selectedInstance.transform.rotation = rotation;

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

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    SetRotation(Vector3.right, increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    SetRotation(Vector3.left, increment);

            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {

                GUILayout.Label("Roll:");
                GUILayout.FlexibleSpace();
                oriZStr = (GUILayout.TextField(oriZStr, 8, GUILayout.Width(fTempWidth)));

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    SetRotation(Vector3.forward, increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    SetRotation(Vector3.back, increment);

            }
            GUILayout.EndHorizontal();


            //
            // Rotation
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Yaw:");
                GUILayout.FlexibleSpace();
                //                    rotStr = GUILayout.TextField(rotStr, 9, GUILayout.Width(fTempWidth));
                //  GUILayout.Box(Vector3.Angle(Vector3.ProjectOnPlane(selectedInstance.gameObject.transform.forward, selectedInstance.gameObject.transform.up), selectedInstance.gameObject.transform.parent.forward).ToString(),  GUILayout.Width(fTempWidth));
                //GUILayout.Box(GetHeading(), GUILayout.Width(fTempWidth));
                oriYStr = (GUILayout.TextField(oriYStr, 8, GUILayout.Width(fTempWidth)));

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    SetRotation(Vector3.up, -increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    SetRotation(Vector3.up, increment);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Heading: ", GUILayout.Height(23));
                GUILayout.Space(5);
                GUILayout.Button(instanceHeading.ToString(), UIMain.DeadButton, GUILayout.Height(23));
                GUILayout.FlexibleSpace();
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

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    selectedInstance.ModelScale = Math.Max(0.01f, selectedInstance.ModelScale - increment);
                    ApplySettings();
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
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

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("GrasColor: ", GUILayout.Height(23));
                GUILayout.FlexibleSpace();

                GUI.enabled = ((grasColorEnabled || grassColor2Enabled )&& (!UI2.GrassEditor.isOpen || GrassColorUI.instance.IsOpen() ));
                if (GUILayout.Button("Preset", GUILayout.Width(90), GUILayout.Height(23)))
                {
                    if (grasColorEnabled)
                    {
                        GrassColorPresetUI.callBack = GrassColorUI.instance.UpdateCallBack;
                        GrassColorUI.selectedInstance = selectedInstance;
                        GrassColorUI.instance.SetupFields();
                        GrassColorPresetUI.instance.Open();
                    }
                    if (grassColor2Enabled)
                    {
                        GrassColorPresetUI2.callBack = grassMod2.UpdateCallBack;
                        GrassColorPresetUI2.instance.Open();
                    }
                }

                if (GUILayout.Button("Edit", GUILayout.Width(90), GUILayout.Height(23)))
                {
                    if (grasColorEnabled)
                    {
                        GrassColorUI.instance.Open();
                    }
                    if (grassColor2Enabled)
                    {
                        UI2.GrassEditor.Open();
                    }
                    
                }

                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

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
                    KerbalKonstructs.DeselectObject(true, true);
                    SpawnInstance(selectedInstance.model, selectedInstance.groupCenter, selectedInstance.position, selectedInstance.Orientation);
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
                    KerbalKonstructs.DeselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);


            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Revert changes", GUILayout.Height(21)))
                {
                    if (selectedInstance.groupCenter != origCenter)
                    {
                        StaticDatabase.ChangeGroup(selectedInstance, origCenter);
                    }
                    selectedInstance.RelativePosition = origPosition;
                    selectedInstance.transform.localPosition = origPosition;
                    selectedInstance.transform.localEulerAngles = origRotation;
                    selectedInstance.Orientation = origRotation;
                    selectedInstance.ModelScale = origScale;
                    ApplySettings();

                }
                if (GUILayout.Button("Delete Instance", GUILayout.Height(21)))
                {
                    DeleteInstance();
                }

            }
            GUILayout.EndHorizontal();
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
            VariantSelector.Close();
        }


        #endregion

        #region Utility Functions


        public void DeleteInstance()
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
        public void SpawnInstance(StaticModel model, GroupCenter groupCenter, Vector3 position, Vector3 rotation)
        {
            StaticInstance instance = new StaticInstance();
            instance.model = model;
            //instance.mesh = UnityEngine.Object.Instantiate(model.prefab);
            instance.CelestialBody = FlightGlobals.currentMainBody;

            instance.groupCenter = groupCenter;
            instance.Group = groupCenter.Group;

            instance.transform.position = position;
            instance.transform.parent = groupCenter.gameObject.transform;

            instance.RelativePosition = instance.transform.localPosition;
            instance.Orientation = rotation;

            if (!Directory.Exists(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath))
            {
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath);
            }
            instance.configPath = KerbalKonstructs.newInstancePath + "/" + model.name + "-instances.cfg";
            instance.configUrl = null;

            enableColliders = false;
            enableColliders2 = false;

            instance.Orientate();
            instance.Activate();

            KerbalKonstructs.SelectInstance(instance, true);

            GrassColor2 grassMod = instance.gameObject.GetComponent<GrassColor2>();

            if (grassMod != null)
            {
                grassMod.UpdateCallBack(StaticsEditorGUI.defaultGrassPreset);

            }
        }

        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        protected Vector3 vectorDrawPosition
        {
            get
            {
                //return (selectedInstance.gameObject.transform.position + 4 * selectedInstance.gameObject.transform.up.normalized + 4 * selectedInstance.gameObject.transform.right.normalized);
                return (selectedInstance.transform.position);
            }
        }

        protected string GetHeading()
        {
            float angle = Vector3.Angle(Vector3.ProjectOnPlane(selectedInstance.transform.forward, selectedInstance.transform.up), selectedInstance.transform.parent.forward);

            if (Vector3.Angle(Vector3.ProjectOnPlane(selectedInstance.transform.forward, selectedInstance.transform.up), selectedInstance.transform.parent.right) < 90)
            {
                return angle.ToString();
            }
            else
            {
                return (360 - angle).ToString();
            }

        }


        protected Vector3 upVector
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
        protected void UpdateVectors()
        {
            if (selectedInstance == null)
            {
                return;
            }

            cameraDistance = Vector3.Distance(selectedInstance.transform.position, FlightCamera.fetch.transform.position) / 4;

            if (referenceSystem == Reference.Center)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                fwdVR.Vector = selectedInstance.groupCenter.gameObject.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.Scale = cameraDistance;
                fwdVR.Draw();

                upVR.Vector = selectedInstance.groupCenter.gameObject.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.Scale = cameraDistance;
                upVR.Draw();

                rightVR.Vector = selectedInstance.groupCenter.gameObject.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.Scale = cameraDistance;
                rightVR.Draw();
            }
            else
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                fwdVR.Vector = selectedInstance.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.Scale = cameraDistance;
                fwdVR.Draw();

                upVR.Vector = selectedInstance.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.Scale = cameraDistance;
                upVR.Draw();

                rightVR.Vector = selectedInstance.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.Scale = cameraDistance;
                rightVR.Draw();
            }

        }

        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        protected void SetupVectors()
        {
            cameraDistance = Vector3.Distance(selectedInstance.position, FlightCamera.fetch.transform.position) / 4;

            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedInstance.groupCenter.gameObject.transform.forward;
            fwdVR.Scale = cameraDistance;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            //fwdVR.SetLayer(11);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedInstance.groupCenter.gameObject.transform.up;
            upVR.Scale = cameraDistance;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;
            //upVR.SetLayer(11);

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedInstance.groupCenter.gameObject.transform.right;
            rightVR.Scale = cameraDistance;
            rightVR.Start = vectorDrawPosition;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;
            //rightVR.SetLayer(11);

        }

        /// <summary>
        /// stops the drawing of the vectors
        /// </summary>
        protected void CloseVectors()
        {

            fwdVR.SetShow(false);
            upVR.SetShow(false);
            rightVR.SetShow(false);

        }

        protected void SetupGizmo()
        {
            origLocalPosition = selectedInstance.transform.localPosition;
            if (referenceSystem == Reference.Center)
            {
                EditorGizmo.SetupMoveGizmo(selectedInstance.gameObject, selectedInstance.transform.localRotation, OnMoveCB, WhenMovedCB);
            }
            else
            {
                EditorGizmo.SetupMoveGizmo(selectedInstance.gameObject, Quaternion.identity, OnMoveCB, WhenMovedCB);
            }
        }

        protected void CloseGizmo()
        {
            EditorGizmo.CloseGizmo();
        }

        protected void UpdateGizmo()
        {
            EditorGizmo.CloseGizmo();
            SetupGizmo();
        }

        public void OnMoveCB(Vector3 vector)
        {
            // Log.Normal("OnMove: " + vector.ToString());
            //moveGizmo.transform.position += 3* vector;
            selectedInstance.transform.position = EditorGizmo.moveGizmo.transform.position;
            //selectedInstance.gameObject.transform.localPosition += selectedInstance.groupCenter.gameObject.transform.InverseTransformDirection(vector);
        }

        public void WhenMovedCB(Vector3 vector)
        {
            //Log.Normal("WhenMoved: " + vector.ToString());
            selectedInstance.transform.localPosition = origLocalPosition + selectedInstance.groupCenter.gameObject.transform.InverseTransformDirection(vector);
            ApplySettings();
        }


        public void SetupFields()
        {
            incrementStr = increment.ToString();
            altStr = selectedInstance.CelestialBody.GetAltitude(selectedInstance.transform.position).ToString();

            oriXStr = Math.Round(selectedInstance.transform.localEulerAngles.x, 4).ToString();
            oriYStr = Math.Round(selectedInstance.transform.localEulerAngles.y, 4).ToString();
            oriZStr = Math.Round(selectedInstance.transform.localEulerAngles.z, 4).ToString();

            posXStr = Math.Round(selectedInstance.transform.localPosition.x, 4).ToString();
            posYStr = Math.Round(selectedInstance.transform.localPosition.y, 4).ToString();
            posZStr = Math.Round(selectedInstance.transform.localPosition.z, 4).ToString();

        }

        public void ApplyInputStrings()
        {
            increment = float.Parse(incrementStr);

            selectedInstance.transform.localPosition = new Vector3(float.Parse(posXStr), float.Parse(posYStr), float.Parse(posZStr));
            selectedInstance.transform.localEulerAngles = new Vector3(float.Parse(oriXStr), float.Parse(oriYStr), float.Parse(oriZStr));


            ApplySettings();
        }




        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        public void SetRotation(Vector3 axis, float increment)
        {
            selectedInstance.transform.Rotate(axis, increment);
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        public void SetTransform(Vector3 direction)
        {
            direction = direction / selectedInstance.ModelScale;
            //selectedInstance.gameObject.transform.Translate(direction, Space.Self);
            if (referenceSystem == Reference.Center)
            {
                selectedInstance.transform.localPosition += direction;
            }
            else
            {

                if (direction.x != 0)
                {
                    movement = selectedInstance.transform.right * direction.x;
                    localMovement = selectedInstance.groupCenter.gameObject.transform.InverseTransformDirection(movement);
                    selectedInstance.transform.localPosition += localMovement;
                }
                if (direction.y != 0)
                {
                    movement = selectedInstance.transform.up * direction.y;
                    localMovement = selectedInstance.groupCenter.gameObject.transform.InverseTransformDirection(movement);
                    selectedInstance.transform.localPosition += localMovement;
                }
                if (direction.z != 0)
                {
                    movement = selectedInstance.transform.forward * direction.z;
                    localMovement = selectedInstance.groupCenter.gameObject.transform.InverseTransformDirection(movement);
                    selectedInstance.transform.localPosition += localMovement;
                }
            }
            ApplySettings();
        }
        /// <summary>
        /// CallBack Functions for Group Selection
        /// </summary>
        /// <param name="newGroup"></param>
        public void SetGroup(GroupCenter newGroup)
        {
            Log.Normal("setting Group from " + selectedInstance.groupCenter.Group + " to " + newGroup.Group);
            StaticDatabase.ChangeGroup(selectedInstance, newGroup);
            ApplySettings();
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        public void ApplySettings()
        {
            selectedInstance.Update();
            SetupFields();
            UpdateGizmo();
        }

        public double instanceHeading
        {
            get
            {
                body = selectedInstance.CelestialBody;

                northVector = Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
                eastVector = Vector3.Cross(upVector, northVector).normalized;
                forwardVector = Vector3.ProjectOnPlane(selectedInstance.transform.forward, upVector);

                double heading = Vector3.Angle(forwardVector, northVector);

                if (Vector3.Dot(forwardVector, eastVector) < 0)
                {
                    heading = 360 - heading;
                }

                return Math.Round(heading, 2);
            }
        }
        public void CheckEditorKeys()
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
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (referenceSystem == Reference.Center)
                        {
                            referenceSystem = Reference.Model;
                        }
                        else
                        {
                            referenceSystem = Reference.Center;
                        }
                        UpdateGizmo();
                        UpdateVectors();
                    }
                    if (Input.GetKey(KeyCode.PageUp))
                    {
                        SetTransform(Vector3.up * increment);
                    }
                    if (Input.GetKey(KeyCode.PageDown))
                    {
                        SetTransform(Vector3.down * increment);
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

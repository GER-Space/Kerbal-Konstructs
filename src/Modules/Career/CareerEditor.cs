using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.Modules
{
    public class CareerEditor : KKWindow
    {

        private static CareerEditor _instance = null;
        public static CareerEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CareerEditor();

                }
                return _instance;
            }
        }

        #region Variable Declarations
        private List<Transform> transformList = new List<Transform>();
        private CelestialBody body;

        internal Boolean foldedIn = false;
        internal Boolean doneFold = false;

        private static double cameraDistance;

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
        //internal static bool isScanable = false;

        //public static Boolean editingLaunchSite = false;

        //   public static Boolean editingFacility = false;


        #endregion

        #region GUI Windows
        // GUI Windows
        internal Rect toolRect = new Rect(300, 35, 330, 340);

        #endregion


        #region Holders
        // Holders

        internal static StaticInstance selectedInstance = null;
        internal StaticInstance selectedObjectPrevious = null;

        //internal static String facType = "None";
        //internal static String sGroup = "Ungrouped";
        private float increment = 1f;


        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private static Space referenceSystem = Space.Self;

        private static Vector3d position = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;


        private static Vector3 startPosition = Vector3.zero;
        private static bool wasInRange = true;

        internal static float maxEditorRange = 250;

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
                CloseEditors();
                CloseVectors();
                CloseGizmo();
                Close();
            }

            if ((KerbalKonstructs.selectedInstance != null) && (!KerbalKonstructs.selectedInstance.isPreview))
            {
                drawEditor(KerbalKonstructs.selectedInstance);

                DrawObject.DrawObjects(KerbalKonstructs.selectedInstance.gameObject);
            }
        }

        public override void Open()
        {
            EditorGizmo.SetupCam();
            base.Open();
        }


        public override void Close()
        {
            CloseVectors();
            CloseGizmo();
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

            if (selectedObjectPrevious != obj)
            {
                selectedObjectPrevious = obj;
                SetupVectors();
                SetupGizmo();
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

                GUILayout.Button("Mod Editor", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    KerbalKonstructs.DeselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();

            if (isInRange)
            {
                GUILayout.Button(selectedInstance.model.title + " (" + selectedInstance.indexInGroup.ToString() + ")", GUILayout.Height(23));
            }
            else
            {
                GUILayout.Button("Out of Range", UIMain.ButtonRed, GUILayout.Height(23));
            }

            if (wasInRange && !isInRange)
            {
                wasInRange = false;
                selectedInstance.HighlightObject(XKCDColors.Reddish);
            }

            if (!wasInRange && isInRange)
            {
                wasInRange = true;
                selectedInstance.HighlightObject(XKCDColors.FreshGreen);
            }


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
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
            {
                GUILayout.Label("Reference System: ");
                GUILayout.FlexibleSpace();
                GUI.enabled = (referenceSystem == Space.World);

                if (GUILayout.Button(new GUIContent(UIMain.iconCubes, "Model"), GUILayout.Height(23), GUILayout.Width(23)))
                {
                    referenceSystem = Space.Self;
                    UpdateGizmo();
                    UpdateVectors();
                }

                GUI.enabled = (referenceSystem == Space.Self);
                if (GUILayout.Button(new GUIContent(UIMain.iconWorld, "World"), GUILayout.Height(23), GUILayout.Width(23)))
                {
                    referenceSystem = Space.World;
                    UpdateGizmo();
                    UpdateVectors();
                }
                GUI.enabled = true;

                GUILayout.Label(referenceSystem.ToString());
            }
            GUILayout.EndHorizontal();
            float fTempWidth = 80f;
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();


            GUILayout.Label("Back / Forward:");
            GUILayout.FlexibleSpace();
            ;

            if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.back * increment);
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.forward * increment);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Left / Right:");
            GUILayout.FlexibleSpace();
            if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.left * increment);
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                SetTransform(Vector3.right * increment);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();




            GUILayout.EndHorizontal();

            GUI.enabled = true;



            // 
            // Altitude editing
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Alt.");
                GUILayout.FlexibleSpace();
                selectedInstance.RadiusOffset = float.Parse(GUILayout.TextField(selectedInstance.RadiusOffset.ToString(), 25, GUILayout.Width(fTempWidth)));
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    SetTransform(Vector3.down * increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    SetTransform(Vector3.up * increment);
            }
            GUILayout.EndHorizontal();


            GUI.enabled = true;

            GUILayout.Space(5);


            fTempWidth = 80f;

            GUI.enabled = true;



            //
            // Rotation
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Heading:");
                GUILayout.FlexibleSpace();
                GUILayout.TextField(heading.ToString(), 9, GUILayout.Width(fTempWidth));

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    SetRotation(-increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    SetRotation(increment);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);
            GUILayout.Space(5);



            GUI.enabled = true;



            GUI.enabled = true;
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = isInRange;
                if (GUILayout.Button("Save&Close", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    selectedInstance.ToggleAllColliders(true);
                    KerbalKonstructs.DeselectObject(true, true);
                    selectedInstance.HighlightObject(Color.clear);
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Destroy", GUILayout.Height(21)))
                {
                    DeleteInstance();
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

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


        private bool isInRange
        {
            get
            {
                return ((selectedInstance.position - startPosition).magnitude < maxEditorRange);
            }
        }

        internal void DeleteInstance()
        {
            if (selectedObjectPrevious == selectedInstance)
                selectedObjectPrevious = null;


            if (selectedInstance.hasLauchSites)
            {
                LaunchSiteManager.DeleteLaunchSite(selectedInstance.launchSite);
            }


            KerbalKonstructs.instance.DeleteInstance(selectedInstance);
            selectedInstance = null;
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
        public string SpawnInstance(StaticModel model, float fOffset, Vector3 vPosition)
        {
            StaticInstance instance = new StaticInstance();

            instance.Orientation = Vector3.up;
            instance.heighReference = HeightReference.Terrain;
            instance.VisibilityRange = (PhysicsGlobals.Instance.VesselRangesDefault.flying.unload + 3000);

            instance.Group = "SaveGame";
            instance.RadialPosition = vPosition;

            instance.model = model;

            if (instance.model == null)
            {
                Log.UserError("LoadFromSave: Canot find model named: " + model.name);
                instance = null;
            }
            //instance.mesh = UnityEngine.Object.Instantiate(instance.model.prefab);


            instance.CelestialBody = KerbalKonstructs.instance.GetCurrentBody();

            instance.RadiusOffset = fOffset;
            instance.RotationAngle = 0;

            instance.RefLatitude = KKMath.GetLatitudeInDeg(instance.RadialPosition);
            instance.RefLongitude = KKMath.GetLongitudeInDeg(instance.RadialPosition);

            InstanceUtil.CreateGroupCenterIfMissing(instance);


            bool oldLegacySpawn = KerbalKonstructs.convertLegacyConfigs;

            instance.Orientate();
            instance.Activate();

            KerbalKonstructs.convertLegacyConfigs = oldLegacySpawn;

            KerbalKonstructs.SelectInstance(instance, true);

            selectedInstance = instance;
            startPosition = selectedInstance.position;

            instance.Update();
            instance.isInSavegame = true;

            instance.HighlightObject(XKCDColors.FreshGreen);

            this.Open();

            return instance.UUID;

        }

        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 vectorDrawPosition
        {
            get
            {
                //return (selectedInstance.gameObject.transform.position + 4 * selectedInstance.gameObject.transform.up.normalized + 4 * selectedInstance.gameObject.transform.right.normalized);
                return (selectedInstance.position);
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
                Vector3 myForward = Vector3.ProjectOnPlane(selectedInstance.transform.forward, upVector);
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

            cameraDistance = Vector3.Distance(selectedInstance.position, FlightCamera.fetch.transform.position) / 4;

            if (referenceSystem == Space.World)
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



        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(float increment)
        {
            selectedInstance.transform.Rotate(Vector3.up, increment);
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            direction = direction / selectedInstance.ModelScale;
            if (referenceSystem == Space.World)
            {
                selectedInstance.transform.localPosition += direction;
            }
            else
            {
                if (direction.y == 0)
                {
                    float oldY = selectedInstance.transform.localPosition.y;
                    selectedInstance.transform.Translate(direction);
                    Vector3 newPos = selectedInstance.transform.localPosition;
                    selectedInstance.transform.localPosition = new Vector3(newPos.x, oldY, newPos.z);
                }
                else
                {
                    selectedInstance.transform.localPosition += direction;
                }


            }
            ApplySettings();
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void ApplySettings()
        {
            selectedInstance.Update();
            UpdateGizmo();
        }


        private void SetupGizmo()
        {

            if (referenceSystem == Space.World)
            {
                EditorGizmo.SetupMoveGizmo(selectedInstance.gameObject, selectedInstance.transform.localRotation, OnMoveCB, WhenMovedCB);
            }
            else
            {
                EditorGizmo.SetupMoveGizmo(selectedInstance.gameObject, Quaternion.identity, OnMoveCB, WhenMovedCB);
            }
        }

        private void CloseGizmo()
        {
            EditorGizmo.CloseGizmo();
        }

        private void UpdateGizmo()
        {
            EditorGizmo.CloseGizmo();
            SetupGizmo();
        }


        internal void OnMoveCB(Vector3 vector)
        {
            // Log.Normal("OnMove: " + vector.ToString());
            //moveGizmo.transform.position += 3* vector;
            selectedInstance.transform.position = EditorGizmo.moveGizmo.transform.position;


            //float oldY = selectedInstance.gameObject.transform.localPosition.y;

            //selectedInstance.gameObject.transform.position += (vector * Time.deltaTime);

            //Vector3 newPos = selectedInstance.gameObject.transform.localPosition;
            //selectedInstance.gameObject.transform.localPosition = new Vector3(newPos.x, oldY, newPos.z);

            //moveGizmo.transform.position = selectedInstance.gameObject.transform.position;

        }

        internal void WhenMovedCB(Vector3 vector)
        {
            //Log.Normal("WhenMoved: " + vector.ToString());
            ApplySettings();
        }

        #endregion
    }
}

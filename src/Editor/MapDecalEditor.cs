using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class MapDecalEditor : KKWindow
    {

        private static MapDecalEditor _instance = null;
        public static MapDecalEditor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapDecalEditor();

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
        internal Texture textureWorld = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/world", false);
        internal Texture textureCubes = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cubes", false);

        #endregion


        #region GUI Windows
        // GUI Windows
        internal Rect toolRect = new Rect(300, 35, 380, 840);

        #endregion

        #region GUI elements
        // GUI elements
        internal GUIStyle listStyle = new GUIStyle();
        internal GUIStyle navStyle = new GUIStyle();

        internal GUIStyle DeadButton;
        internal GUIStyle DeadButtonRed;
        internal GUIStyle KKWindows;
        internal GUIStyle BoxNoBorder;


        // ComboBox siteTypeMenu;
        #endregion
        private Vector2 selectHeightMapScroll;
        private Vector2 selectColorMapScroll;

        #region Holders
        // Holders

        internal static MapDecalInstance selectedDecal = null;
        internal static MapDecalInstance selectedDecalPrevious = null;

        internal float increment = 1f;


        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();
        private VectorRenderer backVR = new VectorRenderer();
        private VectorRenderer leftVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();
        private VectorRenderer southVR = new VectorRenderer();
        private VectorRenderer westVR = new VectorRenderer();


        private static Space referenceSystem = Space.World;

        private static Vector3d position = Vector3d.zero;
        private Vector3d referenceVector = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;
        private Vector3 orientation = Vector3.zero;

        private static double altitude = 0;
        private static double latitude, longitude = 0;

        private bool selectHeightMap = false;
        private bool selectColorMap = false;

        private bool guiInitialized = false;
        private string smessage = "";

        #endregion

        #endregion

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }

            DrawEditor(selectedDecal);
        }


        public override void Close()
        {
            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            CloseVectors();
            EditorGizmo.CloseGizmo();
            base.Close();
            selectedDecal = null;
        }

        #region draw Methods

        /// <summary>
        /// Wrapper to draw editors
        /// </summary>
        /// <param name="mapDecalInstance"></param>
        public void DrawEditor(MapDecalInstance mapDecalInstance)
        {
            if (!guiInitialized)
            {
                InitializeLayout();
                guiInitialized = true;
            }
            if (mapDecalInstance == null)
            {
                return;
            }

            if (selectedDecal != selectedDecalPrevious)
            {
                UpdateSelection(selectedDecal);
                selectedDecalPrevious = selectedDecal;
                position = selectedDecal.gameObject.transform.position;
                Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
                SetupVectors();
                EditorGizmo.SetupMoveGizmo(selectedDecal.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
                if (!KerbalKonstructs.camControl.active)
                {
                    KerbalKonstructs.camControl.enable(selectedDecal.gameObject);
                }
            }

            toolRect = GUI.Window(0xB00B1E3, toolRect, MapDecalEditorWindow, "", KKWindows);

            //if (editingLaunchSite)
            //{
            //    siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawLaunchSiteEditorWindow, "", KKWindows);
            //}

        }

        #endregion

        private void InitializeLayout()
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

            KKWindows = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(8, 8, 3, 3)
            };

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

            Log.Normal("MapDecalEditor UI initialized");
        }


        #region Editor

        #region Decal Editor

        /// <summary>
        /// Instance Editor window
        /// </summary>
        /// <param name="windowID"></param>
        void MapDecalEditorWindow(int windowID)
        {

            //initialize values
            //referenceVector = (Vector3d)selectedDecal.position;
            //isScanable = bool.Parse((string)selectedDecal.getSetting("isScanable"));

            // make this new when converted to PQSCity2
            // fill the variables here for later use
            if (position == Vector3d.zero)
            {
                position = selectedDecal.gameObject.transform.position;
                Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
            }
            UpdateVectors();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("MapDecal Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    //KerbalKonstructs.instance.deselectObject(true, true);
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name:");
                GUILayout.FlexibleSpace();
                selectedDecal.Name = GUILayout.TextField(selectedDecal.Name, GUILayout.Width(220));
                GUILayout.FlexibleSpace();

            }
            GUILayout.EndHorizontal();

            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.BeginHorizontal();
            {
                //GUILayout.FlexibleSpace();
                GUILayout.Label("Increment: ");
                if (float.TryParse(GUILayout.TextField(increment.ToString(), GUILayout.Width(48)), out float inc)) increment = inc;
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
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
                if (GUILayout.Button("100", GUILayout.Height(18)))
                {
                    increment = 100f;
                }
                if (GUILayout.Button("250", GUILayout.Height(16)))
                {
                    increment = 250f;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            //
            // Set reference butons
            //
            GUILayout.BeginHorizontal();
            {
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
            }
            GUILayout.EndHorizontal();
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();
            {
                if (referenceSystem == Space.Self)
                {
                    GUILayout.Label("Back / Forward:");
                    GUILayout.FlexibleSpace();


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
                    GUILayout.Label("Down / Up:");
                    GUILayout.FlexibleSpace();

                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                        SetTransform(Vector3.down * increment);
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                        SetTransform(Vector3.up * increment);

                }
                else
                {
                    GUILayout.Label("West / East :");
                    GUILayout.FlexibleSpace();


                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                        Setlatlng(0d, -increment);
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                        Setlatlng(0d, increment);

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("South / North:");
                    GUILayout.FlexibleSpace();

                    if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                        Setlatlng(-increment, 0d);
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                        Setlatlng(increment, 0d);

                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Latitude");
                if (double.TryParse(GUILayout.TextField(latitude.ToString("#0.0000000"), 12, GUILayout.Width(100)), out double lat) && lat != Math.Round(latitude, 7))
                {
                    Debug.Log($"KK Mapdecal editor latitude: {latitude.ToString("#0.0000000")}, lat: {lat}, rounded: {Math.Round(latitude, 7)}, floor: {Math.Floor(latitude * 10000000) / 10000000}");

                    latitude = Math.Round(lat, 7);

                    Vector3d newpos = body.GetWorldSurfacePosition(latitude, longitude, altitude);
                    selectedDecal.mapDecal.transform.position = newpos;

                    referenceVector = body.GetRelSurfaceNVector(latitude, longitude).normalized * body.Radius;

                    UpdateMoveGizmo();
                }

                GUILayout.Label("Longitude");
                if (double.TryParse(GUILayout.TextField(longitude.ToString("#0.0000000"), 12, GUILayout.Width(100)), out double lon) && lon != Math.Round(longitude, 7))
                {
                    Debug.Log($"KK Mapdecal editor longitude: {longitude.ToString("#0.0000000")}, lon: {lon}, rounded: {Math.Round(longitude, 7)}, floor: {Math.Floor(longitude * 10000000) / 10000000}");

                    longitude = Math.Round(lon, 7);

                    Vector3d newpos = body.GetWorldSurfacePosition(latitude, longitude, altitude);
                    selectedDecal.mapDecal.transform.position = newpos;

                    referenceVector = body.GetRelSurfaceNVector(latitude, longitude).normalized * body.Radius;

                    UpdateMoveGizmo();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            //
            // Rotation
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Heading:");
                GUILayout.FlexibleSpace();
                if (float.TryParse(GUILayout.TextField(selectedDecal.Angle.ToString(), 9, GUILayout.Width(80)), out float angle) && angle != selectedDecal.Angle) SetRotation(angle);

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    SetRotation(-increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    SetRotation(increment);

            }
            GUILayout.EndHorizontal();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            //
            // Order
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Placement order:");
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(selectedDecal.Order.ToString(), 9, GUILayout.Width(80)), out int order)) selectedDecal.Order = Math.Max(100000, order);

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                    selectedDecal.Order = Math.Max(100000, selectedDecal.Order - 1);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    selectedDecal.Order += 1;

            }
            GUILayout.EndHorizontal();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            //
            // Radius
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Radius:");
                GUILayout.FlexibleSpace();
                //selectedDecal.Radius = double.Parse(GUILayout.TextField(selectedDecal.Radius.ToString(), 9, GUILayout.Width(80)));
                if (double.TryParse(GUILayout.TextField(selectedDecal.Radius.ToString(), 9, GUILayout.Width(80)), out double radius)) selectedDecal.Radius = radius;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    selectedDecal.Radius -= increment;
                    selectedDecal.Radius = Math.Max(1, selectedDecal.Radius);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                    selectedDecal.Radius += increment;

            }
            GUILayout.EndHorizontal();

            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            // 
            // Altitude editing
            //
            GUILayout.BeginHorizontal();
            {
                selectedDecal.UseAbsolut = GUILayout.Toggle(selectedDecal.UseAbsolut, "UseAbsolut", GUILayout.Width(250), GUILayout.Height(23));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Snap Surface", GUILayout.Width(110), GUILayout.Height(21)))
                {
                    altitude = (selectedDecal.CelestialBody.pqsController.GetSurfaceHeight(selectedDecal.CelestialBody.GetRelSurfaceNVector(latitude, longitude)) - selectedDecal.CelestialBody.Radius + 1);
                    selectedDecal.mapDecal.transform.position = selectedDecal.CelestialBody.GetWorldSurfacePosition(latitude, longitude, altitude);
                    ;
                    selectedDecal.AbsolutOffset = (float)altitude;
                }


            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Absolut offset:");
                GUILayout.FlexibleSpace();

                if (float.TryParse(GUILayout.TextField(selectedDecal.AbsolutOffset.ToString(), 25, GUILayout.Width(75)), out float absOffset)) selectedDecal.AbsolutOffset = absOffset;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    SetTransform(Vector3.down * increment);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    SetTransform(Vector3.up * increment);

            }
            GUILayout.EndHorizontal();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Height Map", GUILayout.Height(23)))
            {
                selectHeightMap = true;
            }
            GUILayout.Label(selectedDecal.HeightMapName, GUILayout.Width(200));
            GUILayout.EndHorizontal();


            if (selectHeightMap)
            {
                selectHeightMapScroll = GUILayout.BeginScrollView(selectHeightMapScroll);

                foreach (var newmap in DecalsDatabase.allHeightMaps)
                {
                    if (GUILayout.Button(newmap.Name, GUILayout.Height(23)))
                    {
                        selectedDecal.HeightMapName = newmap.Name;
                        selectHeightMap = false;
                    }

                }
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("HeightMapDeformity:");
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(selectedDecal.HeightMapDeformity.ToString(), 25, GUILayout.Width(75)), out double deform)) selectedDecal.HeightMapDeformity = deform;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                    selectedDecal.HeightMapDeformity -= increment;
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    selectedDecal.HeightMapDeformity += increment;

            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("SmoothHeight:");
                GUILayout.FlexibleSpace();
                if (float.TryParse(GUILayout.TextField(selectedDecal.SmoothHeight.ToString(), 25, GUILayout.Width(75)), out float height)) selectedDecal.SmoothHeight = height;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    selectedDecal.SmoothHeight -= increment;
                    selectedDecal.SmoothHeight = Math.Max(0, selectedDecal.SmoothHeight);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    selectedDecal.SmoothHeight += increment;

            }
            GUILayout.EndHorizontal();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Color Map", GUILayout.Height(23)))
            {
                selectColorMap = true;
            }
            GUILayout.Label(selectedDecal.ColorMapName, GUILayout.Width(200));
            GUILayout.EndHorizontal();


            if (selectColorMap)
            {
                selectColorMapScroll = GUILayout.BeginScrollView(selectColorMapScroll);

                foreach (var newmap in DecalsDatabase.allColorMaps)
                {
                    if (GUILayout.Button(newmap.Name, GUILayout.Height(23)))
                    {
                        selectedDecal.ColorMapName = newmap.Name;
                        selectColorMap = false;
                    }

                }
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("SmoothColor:");
                GUILayout.FlexibleSpace();
                if (float.TryParse(GUILayout.TextField(selectedDecal.SmoothColor.ToString(), 25, GUILayout.Width(75)), out float color)) selectedDecal.SmoothColor = color;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    selectedDecal.SmoothColor -= increment;
                    selectedDecal.SmoothColor = Math.Max(0, selectedDecal.SmoothColor);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) | GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    selectedDecal.SmoothColor += increment;

            }
            GUILayout.EndHorizontal();

            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));


            selectedDecal.RemoveScatter = GUILayout.Toggle(selectedDecal.RemoveScatter, "Remove the Scatter Objects (trees/rocks)", GUILayout.Width(250), GUILayout.Height(23));
            selectedDecal.UseAlphaHeightSmoothing = GUILayout.Toggle(selectedDecal.UseAlphaHeightSmoothing, "UseAlphaHeightSmoothing", GUILayout.Width(250), GUILayout.Height(23));
            selectedDecal.CullBlack = GUILayout.Toggle(selectedDecal.CullBlack, "Cullblack", GUILayout.Width(250), GUILayout.Height(23));

            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Group: ", GUILayout.Height(23));
                GUILayout.FlexibleSpace();
                selectedDecal.Group = GUILayout.TextField(selectedDecal.Group, 30, GUILayout.Width(185), GUILayout.Height(23));
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply & Save", GUILayout.Width(180), GUILayout.Height(23)))
                {
                    SaveSettings();
                    smessage = "Saved changes to this object.";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Deselect", GUILayout.Width(120), GUILayout.Height(23)))
                {
                    smessage = "discarding changes";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete Instance", GUILayout.Height(21), GUILayout.Width(120)))
                {
                    DeleteInstance();
                }
            }
            GUILayout.EndHorizontal();

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



        #endregion

        #region Utility Functions

        /// <summary>
        /// Deletes an selected MapDecalInstance
        /// </summary>
        internal void DeleteInstance()
        {
            if (selectedDecalPrevious == selectedDecal)
                selectedDecalPrevious = null;

            selectedDecal.gameObject.transform.parent = null;
            selectedDecal.mapDecal.transform.parent = null;
            selectedDecal.gameObject.DestroyGameObject();

            selectedDecal.CelestialBody.pqsController.RebuildSphere();

            DecalsDatabase.DeleteMapDecalInstance(selectedDecal);

            this.Close();
        }



        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 VectorDrawPosition
        {
            get
            {
                return (selectedDecal.gameObject.transform.position + 1 * selectedDecal.gameObject.transform.up + selectedDecal.gameObject.transform.right);
            }
        }


        /// <summary>
        /// returns the heading the selected object
        /// </summary>
        /// <returns></returns>
        public float Heading
        {
            get
            {
                Vector3 myForward = Vector3.ProjectOnPlane(selectedDecal.gameObject.transform.forward, UpVector);
                float myHeading;

                if (Vector3.Dot(myForward, EastVector) > 0)
                {
                    myHeading = Vector3.Angle(myForward, NorthVector);
                }
                else
                {
                    myHeading = 360 - Vector3.Angle(myForward, NorthVector);
                }
                return myHeading;
            }
        }

        /// <summary>
        /// gives a vector to the east
        /// </summary>
        private Vector3 EastVector
        {
            get
            {
                return Vector3.Cross(UpVector, NorthVector).normalized;
            }
        }

        /// <summary>
        /// vector to north
        /// </summary>
        private Vector3 NorthVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return Vector3.ProjectOnPlane(body.transform.up, UpVector).normalized;
            }
        }

        private Vector3 UpVector
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
            if (selectedDecal == null)
            {
                return;
            }

            if (referenceSystem == Space.Self)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);
                backVR.SetShow(true);
                leftVR.SetShow(true);

                northVR.SetShow(false);
                eastVR.SetShow(false);
                southVR.SetShow(false);
                westVR.SetShow(false);

                fwdVR.Vector = selectedDecal.gameObject.transform.forward;
                fwdVR.Start = VectorDrawPosition;
                fwdVR.Scale = Math.Max(1, selectedDecal.Radius);
                fwdVR.Draw();

                backVR.Vector = -selectedDecal.gameObject.transform.forward;
                backVR.Start = VectorDrawPosition;
                backVR.Scale = Math.Max(1, selectedDecal.Radius);
                backVR.Draw();

                upVR.Vector = selectedDecal.gameObject.transform.up;
                upVR.Start = VectorDrawPosition;
                upVR.Scale = Math.Min(30d, Math.Max(1, selectedDecal.Radius));
                upVR.Draw();

                rightVR.Vector = selectedDecal.gameObject.transform.right;
                rightVR.Start = VectorDrawPosition;
                rightVR.Scale = Math.Max(1, selectedDecal.Radius);
                rightVR.Draw();

                leftVR.Vector = -selectedDecal.gameObject.transform.right;
                leftVR.Start = VectorDrawPosition;
                leftVR.Scale = Math.Max(1, selectedDecal.Radius);
                leftVR.Draw();
            }
            if (referenceSystem == Space.World)
            {
                northVR.SetShow(true);
                eastVR.SetShow(true);
                southVR.SetShow(true);
                westVR.SetShow(true);

                fwdVR.SetShow(false);
                upVR.SetShow(false);
                rightVR.SetShow(false);
                backVR.SetShow(false);
                leftVR.SetShow(false);

                northVR.Vector = NorthVector;
                northVR.Start = VectorDrawPosition;
                northVR.Scale = Math.Max(1, selectedDecal.Radius);
                northVR.Draw();

                southVR.Vector = -NorthVector;
                southVR.Start = VectorDrawPosition;
                southVR.Scale = Math.Max(1, selectedDecal.Radius);
                southVR.Draw();

                eastVR.Vector = EastVector;
                eastVR.Start = VectorDrawPosition;
                eastVR.Scale = Math.Max(1, selectedDecal.Radius);
                eastVR.Draw();

                westVR.Vector = -EastVector;
                westVR.Start = VectorDrawPosition;
                westVR.Scale = Math.Max(1, selectedDecal.Radius);
                westVR.Draw();
            }
        }

        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedDecal.gameObject.transform.forward;
            fwdVR.Scale = Math.Max(1, selectedDecal.Radius);
            fwdVR.Start = VectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

            backVR.Color = new Color(0.972f, 1, 0.627f);
            backVR.Vector = -selectedDecal.gameObject.transform.forward;
            backVR.Scale = Math.Max(1, selectedDecal.Radius);
            backVR.Start = VectorDrawPosition;
            backVR.Width = 0.01d;
            backVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedDecal.gameObject.transform.up;
            upVR.Scale = 30d;
            upVR.Start = VectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedDecal.gameObject.transform.right;
            rightVR.Scale = Math.Max(1, selectedDecal.Radius);
            rightVR.Start = VectorDrawPosition;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;

            leftVR.Color = new Color(0.972f, 1, 0.627f);
            leftVR.Vector = -selectedDecal.gameObject.transform.right;
            leftVR.Scale = Math.Max(1, selectedDecal.Radius);
            leftVR.Start = VectorDrawPosition;
            leftVR.Width = 0.01d;

            northVR.Color = new Color(0.9f, 0.3f, 0.3f);
            northVR.Vector = NorthVector;
            northVR.Scale = Math.Max(1, selectedDecal.Radius);
            northVR.Start = VectorDrawPosition;
            northVR.SetLabel("north");
            northVR.Width = 0.01d;

            southVR.Color = new Color(0.972f, 1, 0.627f);
            southVR.Vector = -NorthVector;
            southVR.Scale = Math.Max(1, selectedDecal.Radius);
            southVR.Start = VectorDrawPosition;
            southVR.Width = 0.01d;

            eastVR.Color = new Color(0.3f, 0.3f, 0.9f);
            eastVR.Vector = EastVector;
            eastVR.Scale = Math.Max(1, selectedDecal.Radius);
            eastVR.Start = VectorDrawPosition;
            eastVR.SetLabel("east");
            eastVR.Width = 0.01d;

            westVR.Color = new Color(0.972f, 1, 0.627f);
            westVR.Vector = -EastVector;
            westVR.Scale = Math.Max(1, selectedDecal.Radius);
            westVR.Start = VectorDrawPosition;
            westVR.Width = 0.01d;

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

            backVR.SetShow(false);
            leftVR.SetShow(false);
            westVR.SetShow(false);
            southVR.SetShow(false);

        }

        /// <summary>
        /// sets the latitude and lognitude from the deltas of north and east and creates a new reference vector
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        internal void Setlatlng(double north, double east)
        {
            body = selectedDecal.CelestialBody;
            double latOffset = north / (body.Radius * KKMath.deg2rad);
            latitude += latOffset;
            double lonOffset = east / (body.Radius * KKMath.deg2rad);
            longitude += lonOffset * Math.Cos(Mathf.Deg2Rad * latitude);

            Vector3d newpos = body.GetWorldSurfacePosition(latitude, longitude, altitude);
            selectedDecal.mapDecal.transform.position = newpos;

            referenceVector = body.GetRelSurfaceNVector(latitude, longitude).normalized * body.Radius;

            UpdateMoveGizmo();
        }


        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(double increment)
        {
            //selectedDecal.mapDecal.transform.Rotate(Vector3.up, (float)increment);
            selectedDecal.Angle += (float)increment;
            selectedDecal.Angle = (selectedDecal.Angle + 360) % 360;

        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            direction = selectedDecal.gameObject.transform.TransformVector(direction);
            double northInc = Vector3d.Dot(NorthVector, direction);
            double eastInc = Vector3d.Dot(EastVector, direction);
            double upInc = Vector3d.Dot(UpVector, direction);

            altitude += upInc;
            selectedDecal.AbsolutOffset += (float)upInc;
            Setlatlng(northInc, eastInc);
        }

        internal void OnMoveCallBack(Vector3 vector)
        {
            // Log.Normal("OnMove: " + vector.ToString());
            //moveGizmo.transform.position += 3* vector;

            selectedDecal.gameObject.transform.position = EditorGizmo.moveGizmo.transform.position;
            position = EditorGizmo.moveGizmo.transform.position;
            FlightGlobals.currentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);

            //selectedDecal.Latitude = KKMath.GetLatitudeInDeg(selectedDecal.gameObject.transform.localPosition);
            //selectedDecal.Longitude = KKMath.GetLongitudeInDeg(selectedDecal.gameObject.transform.localPosition);
            //latitude = selectedDecal.Latitude;
            //longitude = selectedDecal.Longitude;

            //float oldY = selectedInstance.gameObject.transform.localPosition.y;

            //selectedInstance.gameObject.transform.position += (vector * Time.deltaTime);

            //Vector3 newPos = selectedInstance.gameObject.transform.localPosition;
            //selectedInstance.gameObject.transform.localPosition = new Vector3(newPos.x, oldY, newPos.z);

            //moveGizmo.transform.position = selectedInstance.gameObject.transform.position;

        }

        internal void WhenMovedCallBack(Vector3 vector)
        {
            //Log.Normal("WhenMoved: " + vector.ToString());
            //            selectedDecal.Latitude = KKMath.GetLatitudeInDeg(selectedDecal.gameObject.transform.localPosition);
            //            selectedDecal.Longitude = KKMath.GetLongitudeInDeg(selectedDecal.gameObject.transform.localPosition);
            position = EditorGizmo.moveGizmo.transform.position;
            FlightGlobals.currentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
            //      latitude = selectedDecal.Latitude;
            //      longitude = selectedDecal.Longitude;

            double upInc = Vector3d.Dot(UpVector, vector);
            selectedDecal.AbsolutOffset += (float)upInc;

        }

        internal void UpdateMoveGizmo()
        {
            EditorGizmo.CloseGizmo();
            EditorGizmo.SetupMoveGizmo(selectedDecal.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void SaveSettings()
        {
            // replace at some day the latitude and longitude with 
            selectedDecal.Latitude = latitude;
            selectedDecal.Longitude = longitude;

            selectedDecal.Update();

            ConfigParser.SaveMapDecalInstance(selectedDecal);

        }

        /// <summary>
        /// Updates the Window Strings to the new settings
        /// </summary>
        /// <param name="instance"></param>
        public static void UpdateSelection(MapDecalInstance instance)
        {
            selectedDecal = instance;
        }

        #endregion
    }
}

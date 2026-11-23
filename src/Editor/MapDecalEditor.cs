using KerbalKonstructs.Core;
using KerbalKonstructs.UI2;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    /// <summary>
    /// MapDecal editor (UI2 / PopupDialog version; was an IMGUI KKWindow).
    /// Keeps the old API (MapDecalEditor.Instance.Open / Close / IsOpen, selectedDecal), so the
    /// windows that use it don't change. The toggles are on/off switches (KKControls.Switch).
    /// </summary>
    public class MapDecalEditor
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
        private CelestialBody body;

        #region Window
        private PopupDialog dialog;
        private MultiOptionDialog optionDialog;
        private List<DialogGUIBase> content;

        private const string windowName = "MapDecalEditor";
        private const string windowTitle = "MapDecal Editor";
        // the widest row: label + number field + 4 buttons
        private const float windowWidth = 370f;
        private const float windowHeight = 100f;
        private const float itemHeight = 23f;
        private const float smallButton = 30f;
        private const float numberWidth = 75f;
        private static readonly Vector2 defaultPosition = new Vector2(0.75f, 0.5f);
        #endregion

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

        private static double altitude = 0;
        private static double latitude, longitude = 0;

        #endregion

        #endregion


        #region Window handling

        /// <summary>
        /// Opens the editor for selectedDecal
        /// </summary>
        public void Open()
        {
            if (dialog != null || selectedDecal == null)
            {
                return;
            }
            Build();
        }


        public void Close()
        {
            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            CloseVectors();
            EditorGizmo.CloseGizmo();
            DismissDialog();
            selectedDecal = null;
            selectedDecalPrevious = null;
        }


        public bool IsOpen()
        {
            return (dialog != null);
        }


        public void Toggle()
        {
            if (IsOpen())
            {
                Close();
            }
            else
            {
                Open();
            }
        }


        private void Build()
        {
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            CreateContent();

            Rect windowRect = new Rect(WindowManager2.GetPosition(windowName, defaultPosition), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, null, windowTitle, null, windowRect, content.ToArray());
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), optionDialog, false, null, false);
            // allow dragging the window up to the screen border
            KKWindowDrag.Attach(dialog);
            dialog.gameObject.GetChild("Title").SetActive(false);
            dialog.dialogToDisplay.OnUpdate += OnUpdate;
            KKControls.ApplyControls(content);
            KKStyle.AttachTooltips(content);
        }


        private void DismissDialog()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
        }


        /// <summary>
        /// Rebuilds the window (when the reference system changes the position rows)
        /// </summary>
        private void Reload()
        {
            if (dialog == null)
            {
                return;
            }
            DismissDialog();
            Build();
        }


        /// <summary>
        /// Every frame: follow a new selection, update the direction vectors
        /// (was DrawEditor / the start of MapDecalEditorWindow)
        /// </summary>
        private void OnUpdate()
        {
            if (selectedDecal == null || selectedDecal.gameObject == null)
            {
                Close();
                return;
            }

            if (selectedDecal != selectedDecalPrevious)
            {
                SetupSelection();
            }

            if (position == Vector3d.zero)
            {
                position = selectedDecal.gameObject.transform.position;
                Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
            }
            UpdateVectors();
        }


        private void SetupSelection()
        {
            UpdateSelection(selectedDecal);
            selectedDecalPrevious = selectedDecal;
            position = selectedDecal.gameObject.transform.position;
            Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
            body = selectedDecal.CelestialBody;
            SetupVectors();
            EditorGizmo.SetupMoveGizmo(selectedDecal.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
            if (!KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.enable(selectedDecal.gameObject);
            }
        }

        #endregion


        #region Content

        private void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel(windowTitle, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)
                ));

            // name
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Name:", KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                TextField(delegate { return selectedDecal.Name; }, delegate (string s) { selectedDecal.Name = s; }, 60, 240f)
                ));

            // increment
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Increment:", KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                TextField(delegate { return increment.ToString(); }, delegate (string s) { float value; if (MiscUtils.TryParseFloat(s, out value)) { increment = value; } }, 10, numberWidth)
                ));
            content.Add(new DialogGUIHorizontalLayout(
                IncrementButton(0.01f), IncrementButton(0.1f), IncrementButton(1f), IncrementButton(10f), IncrementButton(100f), IncrementButton(250f)
                ));

            Separator();

            // reference system and position
            DialogGUIButton referenceSwitch = KKControls.Switch(delegate { return referenceSystem == Space.Self; }, SetModelReference);
            referenceSwitch.tooltipText = "Off: move along north / east (world). On: move along the decal's own axes (model).";
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel(delegate { return "Reference system: " + ((referenceSystem == Space.Self) ? "Model" : "World"); }, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                referenceSwitch
                ));

            if (referenceSystem == Space.Self)
            {
                content.Add(MoveRow("Back / Forward:", delegate { SetTransform(Vector3.back * increment); }, delegate { SetTransform(Vector3.forward * increment); }));
                content.Add(MoveRow("Left / Right:", delegate { SetTransform(Vector3.left * increment); }, delegate { SetTransform(Vector3.right * increment); }));
                content.Add(MoveRow("Down / Up:", delegate { SetTransform(Vector3.down * increment); }, delegate { SetTransform(Vector3.up * increment); }));
            }
            else
            {
                content.Add(MoveRow("West / East:", delegate { Setlatlng(0d, -increment); }, delegate { Setlatlng(0d, increment); }));
                content.Add(MoveRow("South / North:", delegate { Setlatlng(-increment, 0d); }, delegate { Setlatlng(increment, 0d); }));
            }

            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Lat", KKStyle.whiteLabel),
                TextField(delegate { return latitude.ToString("#0.0000000"); }, SetLatitude, 12, 110f),
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel("Lon", KKStyle.whiteLabel),
                TextField(delegate { return longitude.ToString("#0.0000000"); }, SetLongitude, 12, 110f)
                ));

            Separator();

            // heading, order, radius
            content.Add(NumberRow("Heading:",
                delegate { return selectedDecal.Angle.ToString(); },
                delegate (string s) { float value; if (MiscUtils.TryParseFloat(s, out value) && value != selectedDecal.Angle) { SetRotation(value - selectedDecal.Angle); } },
                delegate { SetRotation(-increment); },
                delegate { SetRotation(increment); }));

            content.Add(NumberRow("Placement order:",
                delegate { return selectedDecal.Order.ToString(); },
                delegate (string s) { int value; if (int.TryParse(s, out value)) { selectedDecal.Order = Math.Max(100000, value); } },
                delegate { selectedDecal.Order = Math.Max(100000, selectedDecal.Order - 1); },
                delegate { selectedDecal.Order += 1; }));

            content.Add(NumberRow("Radius:",
                delegate { return selectedDecal.Radius.ToString(); },
                delegate (string s) { double value; if (TryParseDouble(s, out value)) { selectedDecal.Radius = value; } },
                delegate { selectedDecal.Radius = Math.Max(1, selectedDecal.Radius - increment); },
                delegate { selectedDecal.Radius += increment; }));

            Separator();

            // altitude
            DialogGUIButton snapButton = new DialogGUIButton("Snap Surface", SnapToSurface, 110f, itemHeight, false);
            snapButton.tooltipText = "Put the decal 1 m above the terrain at its position";
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Use absolute altitude:", KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                SwitchFor(delegate { return selectedDecal.UseAbsolut; }, delegate (bool value) { selectedDecal.UseAbsolut = value; }),
                snapButton
                ));

            content.Add(NumberRow("Absolute offset:",
                delegate { return selectedDecal.AbsolutOffset.ToString(); },
                delegate (string s) { float value; if (MiscUtils.TryParseFloat(s, out value)) { selectedDecal.AbsolutOffset = value; } },
                delegate { SetTransform(Vector3.down * increment); },
                delegate { SetTransform(Vector3.up * increment); }));

            Separator();

            // height map
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUIButton("Height Map", delegate { MapSelector.Toggle(MapSelector.MapType.Height); }, 100f, itemHeight, false),
                new DialogGUILabel(delegate { return selectedDecal.HeightMapName; }, KKStyle.goldLabel)
                ));

            content.Add(NumberRow("Height deformity:",
                delegate { return selectedDecal.HeightMapDeformity.ToString(); },
                delegate (string s) { double value; if (TryParseDouble(s, out value)) { selectedDecal.HeightMapDeformity = value; } },
                delegate { selectedDecal.HeightMapDeformity -= increment; },
                delegate { selectedDecal.HeightMapDeformity += increment; }));

            content.Add(NumberRow("Smooth height:",
                delegate { return selectedDecal.SmoothHeight.ToString(); },
                delegate (string s) { float value; if (MiscUtils.TryParseFloat(s, out value)) { selectedDecal.SmoothHeight = value; } },
                delegate { selectedDecal.SmoothHeight = Math.Max(0, selectedDecal.SmoothHeight - increment); },
                delegate { selectedDecal.SmoothHeight += increment; }));

            // color map
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUIButton("Color Map", delegate { MapSelector.Toggle(MapSelector.MapType.Color); }, 100f, itemHeight, false),
                new DialogGUILabel(delegate { return selectedDecal.ColorMapName; }, KKStyle.goldLabel)
                ));

            content.Add(NumberRow("Smooth color:",
                delegate { return selectedDecal.SmoothColor.ToString(); },
                delegate (string s) { float value; if (MiscUtils.TryParseFloat(s, out value)) { selectedDecal.SmoothColor = value; } },
                delegate { selectedDecal.SmoothColor = Math.Max(0, selectedDecal.SmoothColor - increment); },
                delegate { selectedDecal.SmoothColor += increment; }));

            Separator();

            // options
            content.Add(SwitchRow("Remove scatter objects (trees / rocks):", delegate { return selectedDecal.RemoveScatter; }, delegate (bool value) { selectedDecal.RemoveScatter = value; }));
            content.Add(SwitchRow("Use alpha height smoothing:", delegate { return selectedDecal.UseAlphaHeightSmoothing; }, delegate (bool value) { selectedDecal.UseAlphaHeightSmoothing = value; }));
            content.Add(SwitchRow("Cull black:", delegate { return selectedDecal.CullBlack; }, delegate (bool value) { selectedDecal.CullBlack = value; }));

            Separator();

            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Group:", KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                TextField(delegate { return selectedDecal.Group; }, delegate (string s) { selectedDecal.Group = s; }, 30, 185f)
                ));

            Separator();

            DialogGUIButton saveButton = new DialogGUIButton("Apply & Save", ApplyAndSave, 160f, itemHeight, false);
            saveButton.tooltipText = "Apply the changes to the terrain and save them";
            DialogGUIButton deselectButton = new DialogGUIButton("Deselect", Deselect, 120f, itemHeight, false);
            deselectButton.tooltipText = "Close the editor and discard the changes that were not saved";
            content.Add(new DialogGUIHorizontalLayout(saveButton, new DialogGUIFlexibleSpace(), deselectButton));
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIButton("Delete Instance", DeleteInstance, 120f, itemHeight, false)));
        }


        private void Separator()
        {
            content.Add(new DialogGUISpace(4f));
        }


        private DialogGUIBase TextField(Func<string> getValue, Action<string> setValue, int maxLength, float width)
        {
            DialogGUITextInput input = new DialogGUITextInput(getValue(), false, maxLength, delegate (string s) { setValue(s); return s; }, getValue, TMPro.TMP_InputField.ContentType.Standard, itemHeight);
            return new DialogGUIHorizontalLayout(width, itemHeight, 0f, new RectOffset(), TextAnchor.MiddleLeft, input);
        }


        private DialogGUIButton IncrementButton(float value)
        {
            DialogGUIButton button = new DialogGUIButton(value.ToString(), delegate { increment = value; }, delegate { return increment != value; }, 50f, 20f, false);
            button.tooltipText = "Set the increment to " + value;
            return button;
        }


        /// <summary>
        /// &lt;&lt; (hold) and &lt; move one way, &gt; and &gt;&gt; (hold) the other way
        /// </summary>
        private DialogGUIBase MoveRow(string label, Callback decrease, Callback increase)
        {
            return new DialogGUIHorizontalLayout(
                new DialogGUILabel(label, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                KKControls.RepeatButton("<<", decrease, smallButton, itemHeight),
                new DialogGUIButton("<", decrease, smallButton, itemHeight, false),
                new DialogGUIButton(">", increase, smallButton, itemHeight, false),
                KKControls.RepeatButton(">>", increase, smallButton, itemHeight)
                );
        }


        private DialogGUIBase NumberRow(string label, Func<string> getValue, Action<string> setValue, Callback decrease, Callback increase)
        {
            return new DialogGUIHorizontalLayout(
                new DialogGUILabel(label, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                TextField(getValue, setValue, 25, numberWidth),
                KKControls.RepeatButton("<<", decrease, smallButton, itemHeight),
                new DialogGUIButton("<", decrease, smallButton, itemHeight, false),
                new DialogGUIButton(">", increase, smallButton, itemHeight, false),
                KKControls.RepeatButton(">>", increase, smallButton, itemHeight)
                );
        }


        private DialogGUIButton SwitchFor(Func<bool> getValue, Action<bool> setValue)
        {
            return KKControls.Switch(getValue, setValue);
        }


        private DialogGUIBase SwitchRow(string label, Func<bool> getValue, Action<bool> setValue)
        {
            return new DialogGUIHorizontalLayout(
                new DialogGUILabel(label, KKStyle.whiteLabel),
                new DialogGUIFlexibleSpace(),
                SwitchFor(getValue, setValue)
                );
        }


        private static bool TryParseDouble(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            if (!double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.IsNaN(value) || double.IsInfinity(value))
            {
                value = 0;
                return false;
            }
            return true;
        }

        #endregion


        #region Actions

        private void SetModelReference(bool useModel)
        {
            referenceSystem = useModel ? Space.Self : Space.World;
            UpdateVectors();
            // the position rows differ between the two reference systems
            Reload();
        }


        private void SetLatitude(string text)
        {
            double lat;
            if (!TryParseDouble(text, out lat) || lat == Math.Round(latitude, 7))
            {
                return;
            }
            latitude = Math.Round(lat, 7);
            MoveToLatLon();
        }


        private void SetLongitude(string text)
        {
            double lon;
            if (!TryParseDouble(text, out lon) || lon == Math.Round(longitude, 7))
            {
                return;
            }
            longitude = Math.Round(lon, 7);
            MoveToLatLon();
        }


        private void MoveToLatLon()
        {
            body = selectedDecal.CelestialBody;
            Vector3d newpos = body.GetWorldSurfacePosition(latitude, longitude, altitude);
            selectedDecal.mapDecal.transform.position = newpos;
            referenceVector = body.GetRelSurfaceNVector(latitude, longitude).normalized * body.Radius;
            UpdateMoveGizmo();
        }


        private void SnapToSurface()
        {
            altitude = (selectedDecal.CelestialBody.pqsController.GetSurfaceHeight(selectedDecal.CelestialBody.GetRelSurfaceNVector(latitude, longitude)) - selectedDecal.CelestialBody.Radius + 1);
            selectedDecal.mapDecal.transform.position = selectedDecal.CelestialBody.GetWorldSurfacePosition(latitude, longitude, altitude);
            selectedDecal.AbsolutOffset = (float)altitude;
        }


        private void ApplyAndSave()
        {
            SaveSettings();
            MiscUtils.HUDMessage("Saved changes to this object.", 10, 2);
        }


        private void Deselect()
        {
            MiscUtils.HUDMessage("discarding changes", 10, 2);
            Close();
        }

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

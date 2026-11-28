using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Instance editor: moves, rotates and scales the selected static and opens its sub editors
    /// (facility, launch site, grass, variant, group). UI2 version of the old IMGUI "EditorGUI".
    /// The options are on/off switches; a text field applies its value when you press Enter or leave the field.
    /// </summary>
    internal class InstanceEditor : KKEditorWindow
    {
        protected enum Reference
        {
            Model,
            Center
        }

        private static InstanceEditor _instance = null;
        internal static InstanceEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InstanceEditor();
                }
                return _instance;
            }
        }

        protected override string WindowName => "InstanceEditor";
        protected override string WindowTitle => "Instance Editor";
        protected override float WindowWidth => 360f;
        protected override Vector2 DefaultPosition => new Vector2(0.8f, 0.5f);

        #region Variable Declarations

        private CelestialBody body;

        // square icon buttons in the style of the Editor Mode buttons (made by Docs/Icons/InstanceEditor/make_icons.py)
        private static UIStyle copyPositionStyle = null;
        private static UIStyle pastePositionStyle = null;
        private static UIStyle snapStyle = null;
        private const float toolButtonSize = 32f;

        // Switches
        public bool enableColliders = false;

        public static bool grasColorEnabled = false;
        public static bool grassColor2Enabled = false;

        // Holders
        public static StaticInstance selectedInstance = null;
        public StaticInstance selectedObjectPrevious = null;

        private float increment = 1f;

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private Vector3d savedPosition;
        private bool savedpos = false;
        private Vector3d savedRotation = Vector3d.zero;

        private static Reference referenceSystem = Reference.Center;

        private static double cameraDistance;

        private static Vector3 origPosition, origRotation;
        private static GroupCenter origCenter;
        private static float origScale;

        private static Vector3 origLocalPosition;

        // draws the collider outlines (GL drawing only works in OnGUI)
        private Action overlay = null;

        #endregion


        #region Window handling

        internal void Open()
        {
            if (KerbalKonstructs.selectedInstance == null || KerbalKonstructs.selectedInstance.isPreview)
            {
                return;
            }
            if (dialog != null)
            {
                return;
            }
            SetupSelection(KerbalKonstructs.selectedInstance);
            Build();
            if (overlay == null)
            {
                overlay = DrawOverlay;
            }
            if (WindowManager.instance != null && !WindowManager.IsOpen(overlay))
            {
                WindowManager.OpenWindow(overlay);
            }
        }


        public override void Close()
        {
            CloseGizmo();
            CloseVectors();
            CloseEditors();
            if (overlay != null && WindowManager.IsOpen(overlay))
            {
                WindowManager.CloseWindow(overlay);
            }
            DismissDialog();
            selectedInstance = null;
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


        protected override void OnCloseButton()
        {
            KerbalKonstructs.DeselectObject(true, true);
            Close();
        }


        protected override void OnUpdate()
        {
            if (KerbalKonstructs.selectedInstance == null)
            {
                Log.Warning("No instance selected and editor is open --> Closing it");
                Close();
                return;
            }
            if (KerbalKonstructs.selectedInstance != selectedInstance)
            {
                // another static was selected: show it
                SetupSelection(KerbalKonstructs.selectedInstance);
                Reload();
                return;
            }
            UpdateVectors();
        }


        private void DrawOverlay()
        {
            if (MapView.MapIsEnabled || dialog == null || selectedInstance == null || selectedInstance.gameObject == null)
            {
                return;
            }
            DrawObject.DrawObjects(selectedInstance.gameObject);
        }


        private void SetupSelection(StaticInstance newInstance)
        {
            selectedInstance = newInstance;
            SetupVectors();
            SetupGizmo();

            grasColorEnabled = (selectedInstance.mesh.GetComponent<GrasColor>() != null);
            grassColor2Enabled = (selectedInstance.mesh.GetComponent<GrassColor2>() != null);

            origCenter = selectedInstance.groupCenter;
            origPosition = selectedInstance.transform.localPosition;
            origRotation = selectedInstance.transform.localEulerAngles;
            origScale = selectedInstance.ModelScale;
        }


        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
            VariantSelector.CloseWindow();
        }

        #endregion


        #region Content

        protected override void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUIFlexibleSpace(),
                GoldLabel(delegate { return (selectedInstance == null) ? "" : selectedInstance.model.title + " (" + selectedInstance.indexInGroup.ToString() + ")"; }),
                new DialogGUIFlexibleSpace()
                ));

            // position tools and increment
            if (copyPositionStyle == null)
            {
                copyPositionStyle = KKStyle.CreateIconButtonStyle("KerbalKonstructs/Assets/instanceCopyPos");
                pastePositionStyle = KKStyle.CreateIconButtonStyle("KerbalKonstructs/Assets/instancePastePos");
                snapStyle = KKStyle.CreateIconButtonStyle("KerbalKonstructs/Assets/instanceSnap");
            }
            content.Add(new DialogGUIHorizontalLayout(false, false, 4f, new RectOffset(), TextAnchor.MiddleLeft,
                Label("Position:"),
                ToolButton(copyPositionStyle, "Copy", "Copy position and rotation", CopyPosition),
                ToolButton(pastePositionStyle, "Paste", "Paste position and rotation", PastePosition),
                ToolButton(snapStyle, "Snap", "Snap to the snap target", SnapToTarget),
                new DialogGUIFlexibleSpace()
                ));
            IncrementRows(delegate { return increment; }, delegate (float value) { increment = value; }, 0.001f, 0.01f, 0.1f, 1f, 10f, 25f);

            Separator();

            content.Add(SwitchRow("Reference: model axes",
                delegate { return referenceSystem == Reference.Model; },
                delegate (bool useModel) { SetReference(useModel ? Reference.Model : Reference.Center); },
                "On: move along the model's own axes. Off: move along the group center's axes. (Key: F)"));

            DialogGUIButton variantButton = new DialogGUIButton(
                delegate { return string.IsNullOrEmpty(selectedInstance.VariantName) ? "N.A." : selectedInstance.VariantName; },
                delegate { VariantSelector.staticInstance = selectedInstance; VariantSelector.Open(); },
                delegate { return selectedInstance.model.hasVariants; },
                185f, itemHeight, false);
            variantButton.tooltipText = "Choose a model variant";
            content.Add(new DialogGUIHorizontalLayout(Label("Variant:"), new DialogGUIFlexibleSpace(), variantButton));

            // position
            content.Add(NumberRow("Back / Fwd:", delegate { return PositionText(2); }, delegate (string s) { SetPositionComponent(2, s); },
                delegate { SetTransform(Vector3.back * increment); }, delegate { SetTransform(Vector3.forward * increment); }));
            content.Add(NumberRow("Left / Right:", delegate { return PositionText(0); }, delegate (string s) { SetPositionComponent(0, s); },
                delegate { SetTransform(Vector3.left * increment); }, delegate { SetTransform(Vector3.right * increment); }));
            content.Add(NumberRow("Down / Up:", delegate { return PositionText(1); }, delegate (string s) { SetPositionComponent(1, s); },
                delegate { SetTransform(Vector3.down * increment); }, delegate { SetTransform(Vector3.up * increment); }));

            DialogGUIButton snapTerrainButton = new DialogGUIButton("Snap to Terrain", SnapToTerrain, 140f, itemHeight, false);
            snapTerrainButton.tooltipText = "Put the static on the terrain surface";
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), snapTerrainButton));

            Separator();

            // orientation
            DialogGUIButton zeroButton = new DialogGUIButton("Zero", delegate { selectedInstance.transform.localEulerAngles = Vector3.zero; ApplySettings(); }, 50f, itemHeight, false);
            zeroButton.tooltipText = "Zero rotation";
            DialogGUIButton centerUpButton = new DialogGUIButton("C-U", AlignToCenterUp, 50f, itemHeight, false);
            centerUpButton.tooltipText = "Align up to the group center's up vector";
            DialogGUIButton positionUpButton = new DialogGUIButton("P-U", AlignToPositionUp, 50f, itemHeight, false);
            positionUpButton.tooltipText = "Align up to the up vector at the static's position";
            content.Add(new DialogGUIHorizontalLayout(Label("Orientation:"), new DialogGUIFlexibleSpace(), zeroButton, centerUpButton, positionUpButton));

            content.Add(NumberRow("Pitch:", delegate { return RotationText(0); }, delegate (string s) { SetRotationComponent(0, s); },
                delegate { SetRotation(Vector3.right, increment); }, delegate { SetRotation(Vector3.left, increment); }));
            content.Add(NumberRow("Roll:", delegate { return RotationText(2); }, delegate (string s) { SetRotationComponent(2, s); },
                delegate { SetRotation(Vector3.forward, increment); }, delegate { SetRotation(Vector3.back, increment); }));
            content.Add(NumberRow("Yaw:", delegate { return RotationText(1); }, delegate (string s) { SetRotationComponent(1, s); },
                delegate { SetRotation(Vector3.up, -increment); }, delegate { SetRotation(Vector3.up, increment); }));
            content.Add(new DialogGUIHorizontalLayout(Label("Heading:"), new DialogGUIFlexibleSpace(), GoldLabel(delegate { return instanceHeading.ToString(CultureInfo.InvariantCulture); })));

            Separator();

            content.Add(NumberRow("Model scale:",
                delegate { return Math.Round(selectedInstance.ModelScale, 4).ToString(CultureInfo.InvariantCulture); },
                SetScale,
                delegate { selectedInstance.ModelScale = Math.Max(0.01f, selectedInstance.ModelScale - increment); ApplySettings(); },
                delegate { selectedInstance.ModelScale += increment; ApplySettings(); }));

            Separator();

            // sub editors
            DialogGUIButton facilityButton = new DialogGUIButton(delegate { return selectedInstance.facilityType.ToString(); },
                delegate { if (!FacilityEditor.instance.IsOpen()) { FacilityEditor.instance.Open(); } },
                delegate { return !FacilityEditor.instance.IsOpen(); },
                185f, itemHeight, false);
            facilityButton.tooltipText = "Edit the facility of this static";
            content.Add(new DialogGUIHorizontalLayout(Label("Facility type:"), new DialogGUIFlexibleSpace(), facilityButton));

            DialogGUIButton grassPresetButton = new DialogGUIButton("Preset", OpenGrassPreset, GrassButtonsEnabled, 90f, itemHeight, false);
            grassPresetButton.tooltipText = "Set the grass from a preset";
            DialogGUIButton grassEditButton = new DialogGUIButton("Edit", OpenGrassEditor, GrassButtonsEnabled, 90f, itemHeight, false);
            grassEditButton.tooltipText = "Open the grass editor";
            content.Add(new DialogGUIHorizontalLayout(Label("Grass color:"), new DialogGUIFlexibleSpace(), grassPresetButton, grassEditButton));

            DialogGUIButton groupButton = new DialogGUIButton(delegate { return selectedInstance.Group; }, SelectGroup, delegate { return !GroupSelectorUI.instance.IsOpen(); }, 185f, itemHeight, false);
            groupButton.tooltipText = "Move this static to another group";
            content.Add(new DialogGUIHorizontalLayout(Label("Group:"), new DialogGUIFlexibleSpace(), groupButton));

            content.Add(SwitchRow("Enable colliders:", delegate { return enableColliders; }, SetColliders, "Turn the colliders on while editing"));
            content.Add(SwitchRow("Show on anomaly scanners:", delegate { return selectedInstance.isScanable; }, delegate (bool value) { selectedInstance.isScanable = value; ApplySettings(); }));

            DialogGUIButton launchSiteButton = new DialogGUIButton(
                delegate { return (selectedInstance.hasLauchSites ? "Edit" : "Make") + " Launchsite"; },
                delegate { LaunchSiteEditor.instance.Open(); },
                delegate { return !LaunchSiteEditor.instance.IsOpen() && (selectedInstance.hasLauchSites || !string.IsNullOrEmpty(selectedInstance.model.DefaultLaunchPadTransform)); },
                160f, itemHeight, false);
            launchSiteButton.tooltipText = "Make this static a launch site, or edit its launch site";
            DialogGUIButton duplicateButton = new DialogGUIButton("Duplicate", Duplicate, 160f, itemHeight, false);
            duplicateButton.tooltipText = "Save this static and spawn a copy at the same place";
            content.Add(new DialogGUIHorizontalLayout(launchSiteButton, new DialogGUIFlexibleSpace(), duplicateButton));

            Separator();

            DialogGUIButton saveButton = new DialogGUIButton("Save", Save, 160f, itemHeight, false);
            saveButton.tooltipText = "Save the changes to this static";
            DialogGUIButton saveCloseButton = new DialogGUIButton("Save & Close", delegate { Save(); KerbalKonstructs.DeselectObject(true, true); Close(); }, 160f, itemHeight, false);
            content.Add(new DialogGUIHorizontalLayout(saveButton, new DialogGUIFlexibleSpace(), saveCloseButton));

            DialogGUIButton revertButton = new DialogGUIButton("Revert changes", RevertChanges, 160f, itemHeight, false);
            revertButton.tooltipText = "Go back to the position, rotation, scale and group of the last selection";
            content.Add(new DialogGUIHorizontalLayout(revertButton, new DialogGUIFlexibleSpace(), new DialogGUIButton("Delete Instance", DeleteInstance, 160f, itemHeight, false)));
        }


        private static DialogGUIButton ToolButton(UIStyle iconStyle, string text, string tooltip, Callback action)
        {
            DialogGUIButton button = (iconStyle != null) ? KKStyle.IconButton(iconStyle, action, toolButtonSize, toolButtonSize) : new DialogGUIButton(text, action, 50f, itemHeight, false);
            button.tooltipText = tooltip;
            return button;
        }


        private bool GrassButtonsEnabled()
        {
            return (grasColorEnabled || grassColor2Enabled) && !GrassEditor.isOpen && !GrassEditorLegacy.instance.IsOpen();
        }


        private string PositionText(int axis)
        {
            return Math.Round(selectedInstance.transform.localPosition[axis], 4).ToString(CultureInfo.InvariantCulture);
        }


        private string RotationText(int axis)
        {
            return Math.Round(selectedInstance.transform.localEulerAngles[axis], 4).ToString(CultureInfo.InvariantCulture);
        }

        #endregion


        #region Actions

        private void SetPositionComponent(int axis, string text)
        {
            float value;
            if (!TryParseFloat(text, out value))
            {
                return;
            }
            Vector3 position = selectedInstance.transform.localPosition;
            // the field also reports the value it shows: only apply real changes
            if (Math.Abs(value - Math.Round(position[axis], 4)) < 0.00001)
            {
                return;
            }
            position[axis] = value;
            selectedInstance.transform.localPosition = position;
            ApplySettings();
        }


        private void SetRotationComponent(int axis, string text)
        {
            float value;
            if (!TryParseFloat(text, out value))
            {
                return;
            }
            Vector3 angles = selectedInstance.transform.localEulerAngles;
            if (Math.Abs(value - Math.Round(angles[axis], 4)) < 0.00001)
            {
                return;
            }
            angles[axis] = value;
            selectedInstance.transform.localEulerAngles = angles;
            ApplySettings();
        }


        private void SetScale(string text)
        {
            float value;
            if (!TryParseFloat(text, out value))
            {
                return;
            }
            value = Math.Max(0.01f, value);
            if (Math.Abs(value - Math.Round(selectedInstance.ModelScale, 4)) < 0.00001)
            {
                return;
            }
            selectedInstance.ModelScale = value;
            ApplySettings();
        }


        private void SetReference(Reference reference)
        {
            referenceSystem = reference;
            UpdateGizmo();
            UpdateVectors();
        }


        private void CopyPosition()
        {
            savedpos = true;
            savedPosition = selectedInstance.transform.localPosition;
            savedRotation = selectedInstance.transform.localEulerAngles;
            MiscUtils.HUDMessage("Position copied", 5, 2);
        }


        private void PastePosition()
        {
            if (!savedpos)
            {
                return;
            }
            selectedInstance.transform.localPosition = savedPosition;
            selectedInstance.transform.localEulerAngles = savedRotation;
            ApplySettings();
        }


        private void SnapToTarget()
        {
            if (StaticsEditorGUI.instance.snapTargetInstance == null)
            {
                Log.UserError("No Snaptarget selected");
                return;
            }
            selectedInstance.transform.localPosition = StaticsEditorGUI.instance.snapTargetInstance.transform.localPosition;
            selectedInstance.transform.localEulerAngles = StaticsEditorGUI.instance.snapTargetInstance.transform.localEulerAngles;
            ApplySettings();
        }


        private void SnapToTerrain()
        {
            Vector3 pos = selectedInstance.CelestialBody.GetWorldSurfacePosition(selectedInstance.RefLatitude, selectedInstance.RefLongitude, selectedInstance.surfaceHeight);
            selectedInstance.transform.position = pos;
            ApplySettings();
        }


        private void AlignToCenterUp()
        {
            Vector3 newfwd = Vector3.ProjectOnPlane(selectedInstance.transform.forward, selectedInstance.transform.parent.up).normalized;
            Quaternion rotation = new Quaternion();
            rotation.SetLookRotation(newfwd, selectedInstance.transform.parent.up);
            selectedInstance.transform.rotation = rotation;
            ApplySettings();
        }


        private void AlignToPositionUp()
        {
            Vector3 newfwd = Vector3.ProjectOnPlane(selectedInstance.transform.forward, upVector).normalized;
            Quaternion rotation = new Quaternion();
            rotation.SetLookRotation(newfwd, upVector);
            selectedInstance.transform.rotation = rotation;
            ApplySettings();
        }


        private void OpenGrassPreset()
        {
            StaticInstance instanceToEdit = selectedInstance;
            if (grasColorEnabled)
            {
                GrassColorPresetUI.callBack = delegate (Color color, string texture) { GrassEditorLegacy.ApplyPreset(instanceToEdit, color, texture); };
                GrassColorPresetUI.instance.Open();
            }
            if (grassColor2Enabled)
            {
                GrassColor2 grassMod2 = selectedInstance.mesh.GetComponent<GrassColor2>();
                GrassColorPresetUI2.callBack = delegate (GrassColorPresetUI2.ColorPreset2 preset) { if (grassMod2 != null) { grassMod2.OnUserColorChange(); grassMod2.UpdateCallBack(preset); } };
                GrassColorPresetUI2.instance.Open();
            }
        }


        private void OpenGrassEditor()
        {
            if (grasColorEnabled)
            {
                GrassEditorLegacy.instance.Open();
            }
            if (grassColor2Enabled)
            {
                GrassEditor.Open();
            }
        }


        private void SelectGroup()
        {
            GroupSelectorUI.instance.Close();
            GroupSelectorUI.showOnlyLocal = true;
            GroupSelectorUI.titleText = "Select a new Group";
            GroupSelectorUI.callBack = SetGroup;
            GroupSelectorUI.instance.Open();
        }


        private void SetColliders(bool enable)
        {
            enableColliders = enable;
            selectedInstance.ToggleAllColliders(enable);
        }


        private void Duplicate()
        {
            StaticInstance original = selectedInstance;
            original.SaveConfig();
            KerbalKonstructs.DeselectObject(true, true);
            SpawnInstance(original.model, original.groupCenter, original.position, original.Orientation);
            MiscUtils.HUDMessage("Spawned duplicate " + original.model.title, 10, 2);
        }


        private void Save()
        {
            selectedInstance.SaveConfig();
            MiscUtils.HUDMessage("Saved changes to this object.", 10, 2);
        }


        private void RevertChanges()
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
            Close();
        }


        /// <summary>
        /// Spawns an Instance of an defined StaticModel
        /// </summary>
        public void SpawnInstance(StaticModel model, GroupCenter groupCenter, Vector3 position, Vector3 rotation)
        {
            StaticInstance instance = new StaticInstance();
            instance.model = model;
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
        /// the starting position of direction vectors
        /// </summary>
        private Vector3 vectorDrawPosition => selectedInstance.transform.position;


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

            cameraDistance = Vector3.Distance(selectedInstance.transform.position, FlightCamera.fetch.transform.position) / 4;

            Transform reference = (referenceSystem == Reference.Center) ? selectedInstance.groupCenter.gameObject.transform : selectedInstance.transform;

            fwdVR.SetShow(true);
            upVR.SetShow(true);
            rightVR.SetShow(true);

            fwdVR.Vector = reference.forward;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.Scale = cameraDistance;
            fwdVR.Draw();

            upVR.Vector = reference.up;
            upVR.Start = vectorDrawPosition;
            upVR.Scale = cameraDistance;
            upVR.Draw();

            rightVR.Vector = reference.right;
            rightVR.Start = vectorDrawPosition;
            rightVR.Scale = cameraDistance;
            rightVR.Draw();
        }


        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            cameraDistance = Vector3.Distance(selectedInstance.position, FlightCamera.fetch.transform.position) / 4;

            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedInstance.groupCenter.gameObject.transform.forward;
            fwdVR.Scale = cameraDistance;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedInstance.groupCenter.gameObject.transform.up;
            upVR.Scale = cameraDistance;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedInstance.groupCenter.gameObject.transform.right;
            rightVR.Scale = cameraDistance;
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


        private void SetupGizmo()
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


        private void CloseGizmo()
        {
            EditorGizmo.CloseGizmo();
        }


        private void UpdateGizmo()
        {
            EditorGizmo.CloseGizmo();
            SetupGizmo();
        }


        public void OnMoveCB(Vector3 vector)
        {
            selectedInstance.transform.position = EditorGizmo.moveGizmo.transform.position;
        }


        public void WhenMovedCB(Vector3 vector)
        {
            selectedInstance.transform.localPosition = origLocalPosition + selectedInstance.groupCenter.gameObject.transform.InverseTransformDirection(vector);
            ApplySettings();
        }


        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        public void SetRotation(Vector3 axis, float increment)
        {
            selectedInstance.transform.Rotate(axis, increment);
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        public void SetTransform(Vector3 direction)
        {
            direction = direction / selectedInstance.ModelScale;
            if (referenceSystem == Reference.Center)
            {
                selectedInstance.transform.localPosition += direction;
            }
            else
            {
                Transform groupTransform = selectedInstance.groupCenter.gameObject.transform;
                if (direction.x != 0)
                {
                    selectedInstance.transform.localPosition += groupTransform.InverseTransformDirection(selectedInstance.transform.right * direction.x);
                }
                if (direction.y != 0)
                {
                    selectedInstance.transform.localPosition += groupTransform.InverseTransformDirection(selectedInstance.transform.up * direction.y);
                }
                if (direction.z != 0)
                {
                    selectedInstance.transform.localPosition += groupTransform.InverseTransformDirection(selectedInstance.transform.forward * direction.z);
                }
            }
            ApplySettings();
        }


        /// <summary>
        /// CallBack Functions for Group Selection
        /// </summary>
        public void SetGroup(GroupCenter newGroup)
        {
            if (newGroup == null || selectedInstance == null)
            {
                return;
            }
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
            // (SetGroup is also called through the API while the editor is closed: no gizmo then)
            if (IsOpen())
            {
                UpdateGizmo();
            }
        }


        public double instanceHeading
        {
            get
            {
                body = selectedInstance.CelestialBody;

                Vector3 northVector = Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
                Vector3 eastVector = Vector3.Cross(upVector, northVector).normalized;
                Vector3 forwardVector = Vector3.ProjectOnPlane(selectedInstance.transform.forward, upVector);

                double heading = Vector3.Angle(forwardVector, northVector);

                if (Vector3.Dot(forwardVector, eastVector) < 0)
                {
                    heading = 360 - heading;
                }

                return Math.Round(heading, 2);
            }
        }


        /// <summary>
        /// The editor keys (called every frame from KerbalKonstructs.Update). Ignored while typing in a text field.
        /// </summary>
        public void CheckEditorKeys()
        {
            if (selectedInstance == null || !IsOpen() || IsTyping())
            {
                return;
            }

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
                SetReference((referenceSystem == Reference.Center) ? Reference.Model : Reference.Center);
            }
            if (Input.GetKey(KeyCode.PageUp))
            {
                SetTransform(Vector3.up * increment);
            }
            if (Input.GetKey(KeyCode.PageDown))
            {
                SetTransform(Vector3.down * increment);
            }
        }

        #endregion
    }
}

using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Career placement editor: places a static that another mod spawned through the API
    /// (API.SpawnObject) within maxEditorRange of where it was spawned. UI2 version of the old IMGUI window.
    /// </summary>
    internal class CareerEditor : KKEditorWindow
    {
        private static CareerEditor _instance = null;
        internal static CareerEditor instance
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

        protected override string WindowName => "CareerEditor";
        protected override string WindowTitle => "Placement Editor";
        protected override float WindowWidth => 340f;
        protected override Vector2 DefaultPosition => new Vector2(0.8f, 0.5f);

        #region Variable Declarations

        private CelestialBody body;

        private static double cameraDistance;

        internal static StaticInstance selectedInstance = null;
        internal StaticInstance selectedObjectPrevious = null;

        private float increment = 1f;

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private static Space referenceSystem = Space.Self;

        private static Vector3 startPosition = Vector3.zero;
        private static bool wasInRange = true;

        internal static float maxEditorRange = 250;

        // draws the collider outlines (GL drawing only works in OnGUI)
        private Action overlay = null;

        #endregion


        #region Window handling

        internal void Open()
        {
            if (dialog != null)
            {
                return;
            }
            EditorGizmo.SetupCam();
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
            CloseVectors();
            CloseGizmo();
            CloseEditors();
            if (overlay != null && WindowManager.IsOpen(overlay))
            {
                WindowManager.CloseWindow(overlay);
            }
            DismissDialog();
        }


        protected override void OnCloseButton()
        {
            KerbalKonstructs.DeselectObject(true, true);
            Close();
        }


        protected override void OnUpdate()
        {
            if (KerbalKonstructs.selectedInstance == null || selectedInstance == null)
            {
                Close();
                return;
            }
            if (selectedObjectPrevious != selectedInstance)
            {
                selectedObjectPrevious = selectedInstance;
                SetupVectors();
                SetupGizmo();
            }

            bool inRange = isInRange;
            if (wasInRange && !inRange)
            {
                wasInRange = false;
                selectedInstance.HighlightObject(XKCDColors.Reddish);
            }
            if (!wasInRange && inRange)
            {
                wasInRange = true;
                selectedInstance.HighlightObject(XKCDColors.FreshGreen);
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


        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
        }

        #endregion


        #region Content

        protected override void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUIFlexibleSpace(),
                GoldLabel(delegate
                {
                    if (selectedInstance == null)
                    {
                        return "";
                    }
                    return isInRange ? selectedInstance.model.title + " (" + selectedInstance.indexInGroup.ToString() + ")" : "<color=red>Out of Range</color>";
                }),
                new DialogGUIFlexibleSpace()
                ));

            IncrementRows(delegate { return increment; }, delegate (float value) { increment = value; }, 0.001f, 0.01f, 0.1f, 1f, 10f, 25f);

            Separator();

            content.Add(SwitchRow("Reference: model axes",
                delegate { return referenceSystem == Space.Self; },
                delegate (bool useModel) { referenceSystem = useModel ? Space.Self : Space.World; UpdateGizmo(); UpdateVectors(); },
                "On: move along the model's own axes. Off: move along the group center's axes."));

            content.Add(MoveRow("Back / Forward:", delegate { SetTransform(Vector3.back * increment); }, delegate { SetTransform(Vector3.forward * increment); }));
            content.Add(MoveRow("Left / Right:", delegate { SetTransform(Vector3.left * increment); }, delegate { SetTransform(Vector3.right * increment); }));

            content.Add(NumberRow("Altitude:",
                delegate { return selectedInstance.RadiusOffset.ToString(CultureInfo.InvariantCulture); },
                SetRadiusOffset,
                delegate { SetTransform(Vector3.down * increment); },
                delegate { SetTransform(Vector3.up * increment); }));

            content.Add(new DialogGUIHorizontalLayout(
                Label("Heading:"),
                new DialogGUIFlexibleSpace(),
                GoldLabel(delegate { return Math.Round(heading, 2).ToString(CultureInfo.InvariantCulture); }),
                KKControls.RepeatButton("<<", delegate { SetRotation(-increment); }, smallButton, itemHeight),
                new DialogGUIButton("<", delegate { SetRotation(-increment); }, smallButton, itemHeight, false),
                new DialogGUIButton(">", delegate { SetRotation(increment); }, smallButton, itemHeight, false),
                KKControls.RepeatButton(">>", delegate { SetRotation(increment); }, smallButton, itemHeight)
                ));

            Separator();

            DialogGUIButton saveButton = new DialogGUIButton("Save & Close", SaveAndClose, delegate { return selectedInstance != null && isInRange; }, 140f, itemHeight, false);
            saveButton.tooltipText = "Place the static here (only within " + maxEditorRange + " m of where it was spawned)";
            DialogGUIButton destroyButton = new DialogGUIButton("Destroy", DeleteInstance, 140f, itemHeight, false);
            destroyButton.tooltipText = "Remove the static";
            content.Add(new DialogGUIHorizontalLayout(saveButton, new DialogGUIFlexibleSpace(), destroyButton));
        }

        #endregion


        #region Actions

        private void SetRadiusOffset(string text)
        {
            float value;
            if (!TryParseFloat(text, out value) || value == selectedInstance.RadiusOffset)
            {
                return;
            }
            selectedInstance.RadiusOffset = value;
            ApplySettings();
        }


        private void SaveAndClose()
        {
            StaticInstance instance = selectedInstance;
            instance.ToggleAllColliders(true);
            KerbalKonstructs.DeselectObject(true, true);
            instance.HighlightObject(Color.clear);
            Close();
        }

        #endregion


        #region Utility Functions

        private bool isInRange => ((selectedInstance.position - startPosition).magnitude < maxEditorRange);


        internal void DeleteInstance()
        {
            if (selectedObjectPrevious == selectedInstance)
            {
                selectedObjectPrevious = null;
            }

            if (selectedInstance.hasLauchSites)
            {
                LaunchSiteManager.DeleteLaunchSite(selectedInstance.launchSite);
            }

            KerbalKonstructs.instance.DeleteInstance(selectedInstance);
            selectedInstance = null;
            Close();
        }


        /// <summary>
        /// Spawns an Instance of an defined StaticModel and opens the editor for it
        /// </summary>
        public string SpawnInstance(StaticModel model, float fOffset, Vector3 vPosition)
        {
            if (model == null)
            {
                Log.UserError("CareerEditor.SpawnInstance: no model");
                return null;
            }

            StaticInstance instance = new StaticInstance();

            instance.Orientation = Vector3.up;
            instance.heighReference = HeightReference.Terrain;
            instance.VisibilityRange = (PhysicsGlobals.Instance.VesselRangesDefault.flying.unload + 3000);

            instance.Group = "SaveGame";
            instance.RadialPosition = vPosition;

            instance.model = model;

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
            wasInRange = true;

            instance.Update();
            instance.isInSavegame = true;

            instance.HighlightObject(XKCDColors.FreshGreen);

            Open();

            return instance.UUID;
        }


        private Vector3 vectorDrawPosition => selectedInstance.position;


        /// <summary>
        /// returns the heading the selected object
        /// </summary>
        public float heading
        {
            get
            {
                Vector3 myForward = Vector3.ProjectOnPlane(selectedInstance.transform.forward, upVector);
                if (Vector3.Dot(myForward, eastVector) > 0)
                {
                    return Vector3.Angle(myForward, northVector);
                }
                return 360 - Vector3.Angle(myForward, northVector);
            }
        }


        private Vector3 eastVector => Vector3.Cross(upVector, northVector).normalized;


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


        private void UpdateVectors()
        {
            if (selectedInstance == null)
            {
                return;
            }

            cameraDistance = Vector3.Distance(selectedInstance.position, FlightCamera.fetch.transform.position) / 4;

            Transform reference = (referenceSystem == Space.World) ? selectedInstance.groupCenter.gameObject.transform : selectedInstance.transform;

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


        private void SetupVectors()
        {
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


        private void CloseVectors()
        {
            fwdVR.SetShow(false);
            upVR.SetShow(false);
            rightVR.SetShow(false);
        }


        internal void SetRotation(float increment)
        {
            selectedInstance.transform.Rotate(Vector3.up, increment);
            ApplySettings();
        }


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
            selectedInstance.transform.position = EditorGizmo.moveGizmo.transform.position;
        }


        internal void WhenMovedCB(Vector3 vector)
        {
            ApplySettings();
        }

        #endregion
    }
}

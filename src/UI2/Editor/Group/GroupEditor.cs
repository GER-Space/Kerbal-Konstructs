using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Group editor: moves, rotates and renames a group center (or, in "static offset" mode, moves all statics
    /// of the group without moving the group). UI2 version of the old IMGUI window.
    /// </summary>
    internal class GroupEditor : KKEditorWindow
    {
        public enum GroupEditorMode
        {
            Group,
            StaticOffset
        }

        private static GroupEditor _instance = null;
        internal static GroupEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GroupEditor();
                }
                return _instance;
            }
        }

        protected override string WindowName => "GroupEditor";
        protected override string WindowTitle => "Group Editor";
        protected override float WindowWidth => 360f;
        protected override Vector2 DefaultPosition => new Vector2(0.8f, 0.5f);

        #region Variable Declarations

        private CelestialBody body;
        private bool showNameField = false;
        private string newGroupName = "";

        public static GroupCenter selectedGroup = null;
        public GroupCenter selectedObjectPrevious = null;
        public static GroupEditorMode editorMode = GroupEditorMode.Group;

        private float increment = 1f;

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();

        private static Space referenceSystem = Space.Self;

        public static float maxEditorRange = 0;

        #endregion


        #region Window handling

        internal void Open()
        {
            if (selectedGroup == null || dialog != null)
            {
                return;
            }
            SetupSelection();
            Build();
        }


        public override void Close()
        {
            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            CloseVectors();
            EditorGizmo.CloseGizmo();
            CloseEditors();
            selectedObjectPrevious = null;
            editorMode = GroupEditorMode.Group;
            showNameField = false;
            DismissDialog();
        }


        protected override void OnUpdate()
        {
            if (selectedGroup == null)
            {
                Close();
                return;
            }
            if (selectedObjectPrevious != selectedGroup)
            {
                SetupSelection();
                Reload();
                return;
            }
            UpdateVectors();
        }


        private void SetupSelection()
        {
            selectedObjectPrevious = selectedGroup;
            SetupVectors();
            EditorGizmo.SetupMoveGizmo(selectedGroup.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
            if (!KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.enable(selectedGroup.gameObject);
            }
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
            // name: the button opens the rename row
            DialogGUIButton nameButton = new DialogGUIButton(delegate { return selectedGroup.Group; }, delegate { showNameField = true; newGroupName = selectedGroup.Group; Reload(); }, delegate { return !showNameField; }, 260f, itemHeight, false);
            nameButton.tooltipText = "Rename the group";
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), nameButton, new DialogGUIFlexibleSpace()));

            if (showNameField)
            {
                content.Add(TextRow("New name:", delegate { return newGroupName; }, delegate (string s) { newGroupName = s; }, 60, 200f));
                DialogGUIButton okButton = new DialogGUIButton("OK", delegate { selectedGroup.RenameGroup(newGroupName); showNameField = false; Reload(); }, delegate { return !string.IsNullOrEmpty(newGroupName); }, 120f, itemHeight, false);
                content.Add(new DialogGUIHorizontalLayout(okButton, new DialogGUIFlexibleSpace(), new DialogGUIButton("Cancel", delegate { showNameField = false; Reload(); }, 120f, itemHeight, false)));
            }

            IncrementRows(delegate { return increment; }, delegate (float value) { increment = value; }, 0.001f, 0.01f, 0.1f, 1f, 10f, 25f);

            Separator();

            content.Add(ChoiceRow("Editor mode:", new List<KeyValuePair<string, GroupEditorMode>>
                {
                    new KeyValuePair<string, GroupEditorMode>("Group", GroupEditorMode.Group),
                    new KeyValuePair<string, GroupEditorMode>("Static offset", GroupEditorMode.StaticOffset)
                }, delegate { return editorMode; }, SetEditorMode, 100f));
            if (editorMode == GroupEditorMode.StaticOffset)
            {
                content.Add(Label("Static offset mode: changes the offset of the group statics but does NOT change the group position."));
            }

            content.Add(SwitchRow("Reference: model axes",
                delegate { return referenceSystem == Space.Self; },
                delegate (bool useModel) { referenceSystem = useModel ? Space.Self : Space.World; UpdateVectors(); Reload(); },
                "On: move along the group's own axes. Off: move along north / east."));

            if (referenceSystem == Space.Self)
            {
                content.Add(MoveRow("Back / Forward:", delegate { SetTransform(Vector3.back * increment); }, delegate { SetTransform(Vector3.forward * increment); }));
                content.Add(MoveRow("Left / Right:", delegate { SetTransform(Vector3.left * increment); }, delegate { SetTransform(Vector3.right * increment); }));
            }
            else
            {
                content.Add(MoveRow("West / East:", delegate { Setlatlng(0d, -increment); }, delegate { Setlatlng(0d, increment); }));
                content.Add(MoveRow("South / North:", delegate { Setlatlng(-increment, 0d); }, delegate { Setlatlng(increment, 0d); }));
            }

            content.Add(new DialogGUIHorizontalLayout(
                Label("Lat"),
                TextField(delegate { return Math.Round(selectedGroup.RefLatitude, 4).ToString(CultureInfo.InvariantCulture); }, SetLatitude, 12, 110f),
                new DialogGUIFlexibleSpace(),
                Label("Lon"),
                TextField(delegate { return Math.Round(selectedGroup.RefLongitude, 4).ToString(CultureInfo.InvariantCulture); }, SetLongitude, 12, 110f)
                ));

            content.Add(NumberRow("Altitude:",
                delegate { return selectedGroup.RadiusOffset.ToString(CultureInfo.InvariantCulture); },
                SetRadiusOffset,
                delegate { ChangeAltitude(-increment); },
                delegate { ChangeAltitude(increment); }));

            content.Add(NumberRow("Heading:",
                delegate { return Math.Round(selectedGroup.heading, 3).ToString(CultureInfo.InvariantCulture); },
                SetHeading,
                delegate { SetRotation(-increment); },
                delegate { SetRotation(increment); }));

            content.Add(SwitchRow("Sea level as reference:", delegate { return selectedGroup.SeaLevelAsReference; }, delegate (bool value) { selectedGroup.SeaLevelAsReference = value; ApplySettings(); }));

            Separator();

            DialogGUIButton saveButton = new DialogGUIButton("Save & Close", delegate { selectedGroup.Save(); Close(); }, 140f, itemHeight, false);
            saveButton.tooltipText = "Save the group and close the editor";
            DialogGUIButton destroyButton = new DialogGUIButton("Destroy Group", DeleteGroupCenter, 140f, itemHeight, false);
            destroyButton.tooltipText = "Delete the group and all its statics";
            content.Add(new DialogGUIHorizontalLayout(saveButton, new DialogGUIFlexibleSpace(), destroyButton));
        }

        #endregion


        #region Actions

        private void SetEditorMode(GroupEditorMode mode)
        {
            editorMode = mode;
            // the static offset note appears / disappears
            Reload();
        }


        private void SetLatitude(string text)
        {
            double lat;
            if (!TryParseDouble(text, out lat) || lat == Math.Round(selectedGroup.RefLatitude, 4))
            {
                return;
            }
            MoveToLatLon(lat, selectedGroup.RefLongitude);
        }


        private void SetLongitude(string text)
        {
            double lon;
            if (!TryParseDouble(text, out lon) || lon == Math.Round(selectedGroup.RefLongitude, 4))
            {
                return;
            }
            MoveToLatLon(selectedGroup.RefLatitude, lon);
        }


        private void MoveToLatLon(double lat, double lon)
        {
            selectedGroup.RefLatitude = lat;
            selectedGroup.RefLongitude = lon;
            selectedGroup.RadialPosition = KKMath.GetRadiadFromLatLng(selectedGroup.CelestialBody, selectedGroup.RefLatitude, selectedGroup.RefLongitude);
            ApplySettings();
            selectedGroup.RefLatitude = lat;
            selectedGroup.RefLongitude = lon;
        }


        private void SetHeading(string text)
        {
            float targetHeading;
            if (!TryParseFloat(text, out targetHeading) || Math.Abs(targetHeading - Math.Round(selectedGroup.heading, 3)) < 0.0001)
            {
                return;
            }
            selectedGroup.RotationAngle = selectedGroup.RotationAngle + (targetHeading - selectedGroup.heading);
            ApplySettings();
        }


        private void SetRadiusOffset(string text)
        {
            float value;
            if (!TryParseFloat(text, out value) || value == selectedGroup.RadiusOffset)
            {
                return;
            }
            float oldValue = selectedGroup.RadiusOffset;
            selectedGroup.RadiusOffset = value;
            ApplySettings();
            if (!CheckRange(selectedGroup.gameObject.transform.position))
            {
                Log.Debug("Position out of range, reverting changes.");
                selectedGroup.RadiusOffset = oldValue;
                ApplySettings();
            }
        }


        private void ChangeAltitude(float change)
        {
            if (editorMode == GroupEditorMode.Group)
            {
                selectedGroup.RadiusOffset += change;
                ApplySettings();
                if (!CheckRange(selectedGroup.gameObject.transform.position))
                {
                    Log.Debug("Position out of range, reverting changes.");
                    selectedGroup.RadiusOffset -= change;
                    ApplySettings();
                }
            }
            else if (editorMode == GroupEditorMode.StaticOffset)
            {
                selectedGroup.childInstances.ForEach(instance =>
                {
                    instance.RelativePosition.y += change;
                    instance.RadiusOffset += change;
                    instance.gameObject.transform.localPosition += Vector3.up * change;
                });
                ApplySettings();
            }
        }

        #endregion


        #region Utility Functions

        public bool CheckRange(Vector3 targetPos) => maxEditorRange == 0 || FlightGlobals.ActiveVessel == null || Vector3.Distance(FlightGlobals.ActiveVessel.transform.position, targetPos) <= maxEditorRange;


        public void DeleteGroupCenter()
        {
            if (selectedObjectPrevious == selectedGroup)
            {
                selectedObjectPrevious = null;
            }

            InputLockManager.RemoveControlLock("KKShipLock");
            InputLockManager.RemoveControlLock("KKEVALock");
            InputLockManager.RemoveControlLock("KKCamModes");

            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            if (selectedGroup == StaticsEditorGUI.GetActiveGroup())
            {
                StaticsEditorGUI.SetActiveGroup(null);
            }

            selectedGroup.DeleteGroupCenter();
            selectedGroup = null;

            StaticsEditorGUI.ResetLocalGroupList();
            Close();
        }


        private Vector3 vectorDrawPosition => selectedGroup.gameObject.transform.position;


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
                return (Vector3)body.GetSurfaceNVector(selectedGroup.RefLatitude, selectedGroup.RefLongitude).normalized;
            }
        }


        /// <summary>
        /// Sets the vectors active and updates thier position and directions
        /// </summary>
        private void UpdateVectors()
        {
            if (selectedGroup == null)
            {
                return;
            }

            if (referenceSystem == Space.Self)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                northVR.SetShow(false);
                eastVR.SetShow(false);

                fwdVR.Vector = selectedGroup.gameObject.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.Draw();

                upVR.Vector = selectedGroup.gameObject.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.Draw();

                rightVR.Vector = selectedGroup.gameObject.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.Draw();
            }
            else
            {
                northVR.SetShow(true);
                eastVR.SetShow(true);

                fwdVR.SetShow(false);
                upVR.SetShow(false);
                rightVR.SetShow(false);

                northVR.Vector = northVector;
                northVR.Start = vectorDrawPosition;
                northVR.Draw();

                eastVR.Vector = eastVector;
                eastVR.Start = vectorDrawPosition;
                eastVR.Draw();
            }
        }


        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedGroup.gameObject.transform.forward;
            fwdVR.Scale = 30d;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedGroup.gameObject.transform.up;
            upVR.Scale = 30d;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedGroup.gameObject.transform.right;
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
        public void Setlatlng(double north, double east)
        {
            body = Planetarium.fetch.CurrentMainBody;
            double latOffset = north / (body.Radius * KKMath.deg2rad);
            double lonOffset = east / (body.Radius * KKMath.deg2rad);

            if (editorMode == GroupEditorMode.Group)
            {
                double oldLat = selectedGroup.RefLatitude;
                double oldLon = selectedGroup.RefLongitude;
                Vector3 oldRad = selectedGroup.RadialPosition;

                selectedGroup.RefLatitude += latOffset;
                selectedGroup.RefLongitude += lonOffset * Math.Cos(Mathf.Deg2Rad * selectedGroup.RefLatitude);
                selectedGroup.RadialPosition = body.GetRelSurfaceNVector(selectedGroup.RefLatitude, selectedGroup.RefLongitude).normalized * body.Radius;

                ApplySettings();

                if (!CheckRange(selectedGroup.gameObject.transform.position))
                {
                    Log.Debug("Position out of range, reverting changes.");
                    selectedGroup.RefLatitude = oldLat;
                    selectedGroup.RefLongitude = oldLon;
                    selectedGroup.RadialPosition = oldRad;
                    ApplySettings();
                }
            }
            else if (editorMode == GroupEditorMode.StaticOffset)
            {
                selectedGroup.childInstances.ForEach(instance =>
                {
                    instance.RefLatitude += latOffset;
                    instance.RefLongitude += lonOffset * Math.Cos(Mathf.Deg2Rad * instance.RefLatitude);

                    instance.gameObject.transform.position = instance.CelestialBody.GetWorldSurfacePosition(instance.RefLatitude, instance.RefLongitude, instance.RadiusOffset);

                    instance.RelativePosition = instance.gameObject.transform.localPosition;
                    instance.RadiusOffset = (float)((instance.surfaceHeight - instance.groupCenter.surfaceHeight) + instance.RelativePosition.y);

                    ApplySettings();
                });
            }
        }


        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        public void SetRotation(float increment)
        {
            selectedGroup.RotationAngle += increment;
            selectedGroup.RotationAngle = (360f + selectedGroup.RotationAngle) % 360f;
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        public void SetTransform(Vector3 direction)
        {
            if (editorMode == GroupEditorMode.Group)
            {
                direction = selectedGroup.gameObject.transform.TransformVector(direction);
                double northInc = Vector3d.Dot(northVector, direction);
                double eastInc = Vector3d.Dot(eastVector, direction);

                Setlatlng(northInc, eastInc);
            }
            else if (editorMode == GroupEditorMode.StaticOffset)
            {
                selectedGroup.childInstances.ForEach(instance =>
                {
                    instance.transform.localPosition += direction;
                    instance.Update();
                });
            }
        }


        public void OnMoveCallBack(Vector3 vector)
        {
            if (editorMode == GroupEditorMode.Group)
            {
                if (!CheckRange(EditorGizmo.moveGizmo.transform.position))
                {
                    Log.Debug("Position out of range, reverting changes.");
                    return;
                }

                selectedGroup.gameObject.transform.position = EditorGizmo.moveGizmo.transform.position;
                selectedGroup.RadialPosition = selectedGroup.gameObject.transform.localPosition;

                double alt;
                selectedGroup.CelestialBody.GetLatLonAlt(EditorGizmo.moveGizmo.transform.position, out selectedGroup.RefLatitude, out selectedGroup.RefLongitude, out alt);

                selectedGroup.RadiusOffset = (float)(alt - selectedGroup.surfaceHeight);
            }
            else if (editorMode == GroupEditorMode.StaticOffset)
            {
                // continues movement, longitude and height are swapped
                vector /= 8;

                selectedGroup.childInstances.ForEach(instance =>
                {
                    instance.gameObject.transform.position += vector;

                    instance.RelativePosition = instance.gameObject.transform.localPosition;
                    instance.RadiusOffset = -(float)((instance.surfaceHeight - instance.groupCenter.surfaceHeight) + instance.RelativePosition.y);
                });

                EditorGizmo.moveGizmo.transform.position += vector;
            }
        }


        public void WhenMovedCallBack(Vector3 vector)
        {
            ApplySettings();
        }


        public void UpdateMoveGizmo()
        {
            EditorGizmo.CloseGizmo();
            EditorGizmo.SetupMoveGizmo(selectedGroup.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        public void ApplySettings()
        {
            selectedGroup.Update();
            UpdateMoveGizmo();
        }


        /// <summary>
        /// The editor keys (called every frame from KerbalKonstructs.Update). Ignored while typing in a text field.
        /// </summary>
        public void CheckEditorKeys()
        {
            if (selectedGroup == null || !IsOpen() || IsTyping())
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

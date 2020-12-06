using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    public class GroupEditor : Window
    {

        private static GroupEditor _instance = null;
        public static GroupEditor instance
        {
            get {
                if (_instance == null) {
					_instance = UIKit.CreateUI<GroupEditor> (UIMain.appCanvasRect, "KKGroupEditor");
                }
                return _instance;
            }
        }

        Rect toolRect = new Rect(300, 35, 330, 350);

        #region Holders
        // Holders

        internal static GroupCenter selectedGroup = null;
        GroupCenter selectedObjectPrevious = null;

        //static String facType = "None";
        //static String sGroup = "Ungrouped";

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();


        private static Vector3d position = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;


        private static Vector3 startPosition = Vector3.zero;

        //static float maxEditorRange = 250;

        #endregion

		InputLine groupName;
		IncrementSize increment;
		ReferenceSystem referenceSystem;
		PositionButtons xAxisButtons;
		PositionButtons yAxisButtons;
		PositionButtons zAxisButtons;
		PositionButtons rotationButtons;
		PositionLine positionLine;
		InfoLine headingLine;
		ToggleText sealevelRef;
		UIButton saveClose;

		public static bool IsOpen()
		{
			return _instance && _instance.gameObject.activeInHierarchy;
		}

        public void Close()
        {
			SetActive(false);
            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            CloseVectors();
            EditorGizmo.CloseGizmo();
            CloseEditors();
            selectedObjectPrevious = null;
        }

		public void Open()
		{
			UpdateUI();
            if (selectedObjectPrevious != selectedGroup)
            {
                selectedObjectPrevious = selectedGroup;
                SetupVectors();
                UpdateStrings();
                EditorGizmo.SetupMoveGizmo(selectedGroup.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
                if (!KerbalKonstructs.camControl.active)
                {
                    KerbalKonstructs.camControl.enable(selectedGroup.gameObject);
                }

            }
			SetActive(true);
		}

		public override void CreateUI()
		{
			Debug.Log($"[GroupEditor] CreateUI\n{System.Environment.StackTrace}");
			base.CreateUI();

			this.Title(KKLocalization.GroupEditor)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<HorizontalSep>("HorizontalSep2") .Space(1, 2) .Finish()
				.Add<InputLine>(out groupName)
					.Label(KKLocalization.GroupName)
					.OnSubmit(SetGroupName)
					.OnFocusLost(RestoreGroupName)
					.Finish()
				.Add<IncrementSize>(out increment)
					.Finish()
				.Add<ReferenceSystem>(out referenceSystem)
					.OnSpaceChanged(OnSpaceChanged)
					.Finish()
				.Add<PositionButtons>(out zAxisButtons)
					.OnIncrement(OnZIncrement)
					.OnDecrement(OnZDecrement)
					.Finish()
				.Add<PositionButtons>(out xAxisButtons)
					.OnIncrement(OnXIncrement)
					.OnDecrement(OnXDecrement)
					.Finish()
				.Add<PositionButtons>(out yAxisButtons)
					.Label(KKLocalization.Altitude)
					.OnIncrement(OnYIncrement)
					.OnDecrement(OnYDecrement)
					.Finish()
				.Add<PositionButtons>(out rotationButtons)
					.Label(KKLocalization.Rotation)
					.OnIncrement(OnRotIncrement)
					.OnDecrement(OnRotDecrement)
					.Finish()
				.Add<PositionLine>(out positionLine)
					.Finish()
				.Add<InfoLine>(out headingLine)
					.Label(KKLocalization.Heading)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<ToggleText>(out sealevelRef)
					.Text(KKLocalization.SealevelAsReference)
					.OnValueChanged(SealevelAsReference)
					.Finish()
				.Add<HorizontalLayout>()
					.Add<UIButton>(out saveClose)
						.Text(KKLocalization.SaveAndClose)
						.OnClick(SaveAndClose)
						.Finish()
					.Add<UIButton>()
						.Text(KKLocalization.DestroyGroup)
						.OnClick(DestroyGroup)
						.Finish()
					.Finish()

				.Add<HorizontalSep>("HorizontalSep2") .Space(1, 2) .Finish()
				.Finish();

			UIMain.SetTitlebar(titlebar, Close);
			SetPositionLabels();
		}

		void SetGroupName(string name)
		{
			selectedGroup.Group = name;
		}

		void RestoreGroupName(string name = null)
		{
			groupName.text = selectedGroup.Group;
		}

		void SetPositionLabels()
		{
			switch (referenceSystem.space) {
				case Space.Self:
					zAxisButtons.Label(KKLocalization.BackForward);
					xAxisButtons.Label(KKLocalization.LeftRight);
					break;
				case Space.World:
					zAxisButtons.Label(KKLocalization.SouthNorth);
					xAxisButtons.Label(KKLocalization.WestEast);
					break;
			}
		}

		void OnSpaceChanged(ReferenceSystem system)
		{
			UpdateVectors();
			SetPositionLabels();
		}

		void OnXIncrement()
		{
			if (referenceSystem.space == Space.Self) {
				SetTransform(Vector3.right * increment.Increment);
			} else {
				Setlatlng(0d, increment.Increment);
			}
		}

		void OnXDecrement()
		{
			if (referenceSystem.space == Space.Self) {
				SetTransform(Vector3.left * increment.Increment);
			} else {
				Setlatlng(0d, -increment.Increment);
			}
		}

		void OnYIncrement()
		{
			selectedGroup.RadiusOffset += increment.Increment;
			ApplySettings();
		}

		void OnYDecrement()
		{
			selectedGroup.RadiusOffset -= increment.Increment;
			ApplySettings();
		}

		void OnZIncrement()
		{
			if (referenceSystem.space == Space.Self) {
				SetTransform(Vector3.forward * increment.Increment);
			} else {
				Setlatlng(increment.Increment, 0d);
			}
		}

		void OnZDecrement()
		{
			if (referenceSystem.space == Space.Self) {
				SetTransform(Vector3.back * increment.Increment);
			} else {
				Setlatlng(-increment.Increment, 0d);
			}
		}

		void OnRotIncrement()
		{
			SetRotation(increment.Increment);
		}

		void OnRotDecrement()
		{
			SetRotation(-increment.Increment);
		}

		void SealevelAsReference(bool on)
		{
			selectedGroup.SeaLevelAsReference = on;
		}

		void SaveAndClose()
		{
			selectedGroup.Save();
			this.Close();
		}

		void DestroyGroup()
		{
			DeleteGroupCenter();
		}

		void UpdateUI()
		{
            UpdateVectors();
			UpdateStrings();

			groupName.text = selectedGroup.Group;
			sealevelRef.SetIsOnWithoutNotify(selectedGroup.SeaLevelAsReference);
		}

        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
        }


        #region Utility Functions


        void DeleteGroupCenter()
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


            /*XXX if (selectedGroup == StaticsEditorGUI.GetActiveGroup())
            {
                StaticsEditorGUI.SetActiveGroup(null);
            }*/

            selectedGroup.DeleteGroupCenter();

            selectedGroup = null;


            //XXX StaticsEditorGUI.ResetLocalGroupList();
            this.Close();
        }



        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 vectorDrawPosition
        {
            get {
                return (selectedGroup.gameObject.transform.position);
            }
        }


        /// <summary>
        /// returns the heading the selected object
        /// </summary>
        /// <returns></returns>
        public float heading
        {
            get {
                Vector3 myForward = Vector3.ProjectOnPlane(selectedGroup.gameObject.transform.forward, upVector);
                float myHeading;

                if (Vector3.Dot(myForward, eastVector) > 0) {
                    myHeading = Vector3.Angle(myForward, northVector);
                } else {
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
            get {
                return Vector3.Cross(upVector, northVector).normalized;
            }
        }

        /// <summary>
        /// vector to north
        /// </summary>
        private Vector3 northVector
        {
            get {
                CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
                return Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
            }
        }

        private Vector3 upVector
        {
            get {
                CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
                return (Vector3)body.GetSurfaceNVector(selectedGroup.RefLatitude, selectedGroup.RefLongitude).normalized;
            }
        }

        /// <summary>
        /// Sets the vectors active and updates thier position and directions
        /// </summary>
        private void UpdateVectors()
        {
            if (selectedGroup == null) {
                return;
            }

            if (referenceSystem.space == Space.Self) {
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
            if (referenceSystem.space == Space.World) {
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
            // draw vectors
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
        /// <param name="north"></param>
        /// <param name="east"></param>
        void Setlatlng(double north, double east)
        {
			//FIXME remove singularity around poles (note: bigger problem with world reference, this is just where things become obvious)
            CelestialBody body = Planetarium.fetch.CurrentMainBody;
            double latOffset = north / (body.Radius * KKMath.deg2rad);
            double lonOffset = east / (body.Radius * KKMath.deg2rad);
            selectedGroup.RefLatitude += latOffset;
            selectedGroup.RefLongitude += lonOffset * Math.Cos(Mathf.Deg2Rad * selectedGroup.RefLatitude);

            selectedGroup.RadialPosition = body.GetRelSurfaceNVector(selectedGroup.RefLatitude, selectedGroup.RefLongitude).normalized * body.Radius;
            ApplySettings();
        }

        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        void SetRotation(float increment)
        {
            selectedGroup.RotationAngle += (float)increment;
            selectedGroup.RotationAngle = (360f + selectedGroup.RotationAngle) % 360f;
            ApplySettings();
        }

        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        void SetTransform(Vector3 direction)
        {
            direction = selectedGroup.gameObject.transform.TransformDirection(direction);
            double northInc = Vector3d.Dot(northVector, direction);
            double eastInc = Vector3d.Dot(eastVector, direction);

            Setlatlng(northInc, eastInc);
        }

        void OnMoveCallBack(Vector3 vector)
        {
            // Log.Normal("OnMove: " + vector.ToString());
            //moveGizmo.transform.position += 3* vector;

            selectedGroup.gameObject.transform.position = EditorGizmo.moveGizmo.transform.position;
            selectedGroup.RadialPosition = selectedGroup.gameObject.transform.localPosition;

            double alt;
            selectedGroup.CelestialBody.GetLatLonAlt(EditorGizmo.moveGizmo.transform.position, out selectedGroup.RefLatitude, out selectedGroup.RefLongitude, out alt);

            selectedGroup.RadiusOffset = (float)(alt - selectedGroup.surfaceHeight);
            //float oldY = selectedInstance.gameObject.transform.localPosition.y;
        }

        void WhenMovedCallBack(Vector3 vector)
        {
            ApplySettings();
            //Log.Normal("WhenMoved: " + vector.ToString());
        }

        void UpdateMoveGizmo()
        {
            EditorGizmo.CloseGizmo();
            EditorGizmo.SetupMoveGizmo(selectedGroup.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
        }

        void ApplyInputStrings()
        {
            //selectedGroup.RefLatitude = double.Parse(refLat);
            //selectedGroup.RefLongitude = double.Parse(refLng);

            selectedGroup.RadialPosition = KKMath.GetRadiadFromLatLng(selectedGroup.CelestialBody, selectedGroup.RefLatitude, selectedGroup.RefLongitude);

            float oldRotation = selectedGroup.RotationAngle;
            //float tgtheading = float.Parse(headingStr);
            //float diffHeading = (tgtheading - selectedGroup.heading);

            //selectedGroup.RotationAngle = oldRotation + diffHeading;

            ApplySettings();

            //selectedGroup.RefLatitude = double.Parse(refLat);
            //selectedGroup.RefLongitude = double.Parse(refLng);
        }

        void UpdateStrings()
        {
			headingLine.Info($"{selectedGroup.heading:G3}");
			positionLine
				.Altitude(selectedGroup.RadiusOffset)
				.Latitude(selectedGroup.RefLatitude)
				.Longitude(selectedGroup.RefLongitude);
        }

        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        void ApplySettings()
        {
            selectedGroup.Update();
            UpdateStrings();
            UpdateMoveGizmo();
        }

        void CheckEditorKeys()
        {
			if (Input.GetKey(KeyCode.W)) {
				SetTransform(Vector3.forward * increment.Increment);
			}
			if (Input.GetKey(KeyCode.S)) {
				SetTransform(Vector3.back * increment.Increment);
			}
			if (Input.GetKey(KeyCode.D)) {
				SetTransform(Vector3.right * increment.Increment);
			}
			if (Input.GetKey(KeyCode.A)) {
				SetTransform(Vector3.left * increment.Increment);
			}
			if (Input.GetKey(KeyCode.PageUp)) {
				SetTransform(Vector3.up * increment.Increment);
			}
			if (Input.GetKey(KeyCode.PageDown)) {
				SetTransform(Vector3.down * increment.Increment);
			}
        }

		void Update()
		{
            if (selectedGroup != null) {
				CheckEditorKeys();
			}
		}
        #endregion
    }
}

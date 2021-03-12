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

        private static Vector3d position = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;


        private static Vector3 startPosition = Vector3.zero;

        //static float maxEditorRange = 250;

        #endregion

		InputLine groupName;
		IncrementSize increment;
		PositionEdit positionEdit;
		FieldToggle sealevelRef;
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
				.Add<PositionEdit>(out positionEdit)
					.Increment(increment)
					.DoAltitude(true)
					.OnPositionChange(OnPositionChange)
					.Finish()
				.Add<FieldToggle>(out sealevelRef)
					.Text(KKLocalization.SealevelAsReference)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
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
		}

		void SetGroupName(string name)
		{
			selectedGroup.Group = name;
		}

		void RestoreGroupName(string name = null)
		{
			groupName.text = selectedGroup.Group;
		}

		void OnPositionChange()
		{
			var body = selectedGroup.CelestialBody;
			selectedGroup.RefLatitude = positionEdit.Latitude;
			selectedGroup.RefLongitude = positionEdit.Longitude;
			selectedGroup.RadiusOffset = (float) positionEdit.Altitude;//FIXME make double
			selectedGroup.RotationAngle = (float) positionEdit.Heading;//FIXME make double
			selectedGroup.RadialPosition = positionEdit.NVector * body.Radius;
			ApplySettings();
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
			groupName.text = selectedGroup.Group;
			positionEdit.Body(selectedGroup.CelestialBody)
				.SetPosition(selectedGroup.RefLatitude, selectedGroup.RefLongitude,
							 selectedGroup.RadiusOffset, selectedGroup.RotationAngle);
			sealevelRef.Field(selectedGroup, "SeaLevelAsReference");
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
        /// stops the drawing of the vectors
        /// </summary>

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

        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        void ApplySettings()
        {
            selectedGroup.Update();
            UpdateMoveGizmo();
        }

        void CheckEditorKeys()
        {
			if (Input.GetKey(KeyCode.W)) {
				positionEdit.Adjust(Vector3d.forward);
			}
			if (Input.GetKey(KeyCode.S)) {
				positionEdit.Adjust(Vector3d.back);
			}
			if (Input.GetKey(KeyCode.D)) {
				positionEdit.Adjust(Vector3d.right);
			}
			if (Input.GetKey(KeyCode.A)) {
				positionEdit.Adjust(Vector3d.left);
			}
			if (Input.GetKey(KeyCode.PageUp)) {
				positionEdit.Adjust(Vector3d.up);
			}
			if (Input.GetKey(KeyCode.PageDown)) {
				positionEdit.Adjust(Vector3d.down);
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

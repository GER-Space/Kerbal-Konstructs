using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class PositionEdit : VerticalLayout
	{
		public class PositionEditEvent : UnityEvent { };
		PositionEditEvent onPositionChange;

		CelestialBody body;
		IncrementSize increment;
		PositionButtons yAxisButtons;
		PositionButtons xAxisButtons;
		InputLine latitudeInput;
		InputLine longitudeInput;
		ValueAdjuster altitudeAdjust;
		HorizontalSep separator;
		ValueAdjuster headingAdjust;
		ReferenceSystem referenceSystem;

		VectorDisplay vectorDisplay;

		public Space space { get { return referenceSystem.space; } }

		public double Latitude
		{
			get { return latitude * 180 / Math.PI; }
			set {
				latitude = CanonicalAngle(value);
				SetPosition();
			}
		}

		public double Longitude
		{
			get { return longitude * 180 / Math.PI; }
			set {
				longitude = CanonicalAngle(value);
				SetPosition();
			}
		}

		public double Heading
		{
			get { return heading * 180 / Math.PI; }
			set {
				heading = CanonicalAngle(value);
				SetPosition();
			}
		}

		public double Altitude
		{
			get { return altitude; }
			set {
				altitude = value;
				SetPosition();
			}
		}

		public Vector3d NVector { get { return worldFrame.Z.xzy; } }
		public Vector3d Position
		{
			get { return position.xzy; }
		}
		public QuaternionD Rotation
		{
			get { return objectFrame.Rotation.swizzle; }
		}

		double latitude;
		double longitude;
		double heading;
		double altitude;
		// right handed, so Z up radial, X is east, and Y is north when
		// the frame is not rotated around the local vertical (radial) axis
		// ie, X is the object's right, and Y is the object's forward
		Planetarium.CelestialFrame objectFrame;
		// X is always east and Y is always north
		Planetarium.CelestialFrame worldFrame;
		Vector3d position;

		protected override void OnDisable()
		{
			vectorDisplay.CloseVectors();
		}

		public override void CreateUI()
		{
			base.CreateUI();

			onPositionChange = new PositionEditEvent();
			vectorDisplay = new VectorDisplay();

			this.ChildForceExpand(true, false)
				.Add<ReferenceSystem>(out referenceSystem)
					.OnSpaceChanged(OnSpaceChanged)
					.Finish()
				.Add<PositionButtons>(out xAxisButtons)
					.OnIncrement(OnXIncrement)
					.OnDecrement(OnXDecrement)
					.Finish()
				.Add<PositionButtons>(out yAxisButtons)
					.OnIncrement(OnYIncrement)
					.OnDecrement(OnYDecrement)
					.Finish()
				.Add<HorizontalLayout>()
					.Add<InputLine>(out latitudeInput)
						.Label(KKLocalization.Latitude)
						.OnSubmit(SetLatitude)
						.OnFocusLost(RestoreLatitude)
						.InputWidth(100)
						.Finish()
					.Add<InputLine>(out longitudeInput)
						.Label(KKLocalization.Longitude)
						.OnSubmit(SetLongitude)
						.OnFocusLost(RestoreLongitude)
						.InputWidth(100)
						.Finish()
					.Finish()
				.Add<ValueAdjuster>(out altitudeAdjust)
					.Label(KKLocalization.Altitude)
					.OnIncrement(OnAltitudeIncrement)
					.OnDecrement(OnAltitudeDecrement)
					.OnValueSet(OnAltitudeSet)
					.InputWidth(100)
					.Finish()
				.Add<HorizontalSep>(out separator, "HorizontalSep2") .Finish()
				.Add<ValueAdjuster>(out headingAdjust)
					.Label(KKLocalization.Heading)
					.OnIncrement(OnHeadingIncrement)
					.OnDecrement(OnHeadingDecrement)
					.OnValueSet(OnHeadingSet)
					.InputWidth(100)
					.Finish()
				;

			SetPositionLabels();
		}

		public override void Style()
		{
			base.Style();
		}

		void SetPositionLabels()
		{
			switch (referenceSystem.space) {
				case Space.Self:
					yAxisButtons.Label(KKLocalization.BackForward);
					xAxisButtons.Label(KKLocalization.LeftRight);
					break;
				case Space.World:
					yAxisButtons.Label(KKLocalization.SouthNorth);
					xAxisButtons.Label(KKLocalization.WestEast);
					break;
			}
		}

		void OnSpaceChanged(ReferenceSystem system)
		{
			SetPositionLabels();
			UpdateVectors();
		}

		void UpdateVectors()
		{
			vectorDisplay.Space = referenceSystem.space;
			vectorDisplay.Position = position;
			if (referenceSystem.space == Space.Self) {
				vectorDisplay.Frame = objectFrame;
			} else {
				vectorDisplay.Frame = worldFrame;
			}
			vectorDisplay.Draw();
		}

		void OnXIncrement()
		{
			Vector3d dir;
			if (referenceSystem.space == Space.Self) {
				dir = objectFrame.X;
			} else {
				dir = worldFrame.X;
			}
			SetPosition(dir * increment.Increment);
			RestoreLatitude();
			RestoreLongitude();
			onPositionChange.Invoke();
		}

		void OnXDecrement()
		{
			Vector3d dir;
			if (referenceSystem.space == Space.Self) {
				dir = objectFrame.X;
			} else {
				dir = worldFrame.X;
			}
			SetPosition(dir * -increment.Increment);
			RestoreLatitude();
			RestoreLongitude();
			onPositionChange.Invoke();
		}

		void OnYIncrement()
		{
			Vector3d dir;
			if (referenceSystem.space == Space.Self) {
				dir = objectFrame.Y;
			} else {
				dir = worldFrame.Y;
			}
			SetPosition(dir * increment.Increment);
			RestoreLatitude();
			RestoreLongitude();
			onPositionChange.Invoke();
		}

		void OnYDecrement()
		{
			Vector3d dir;
			if (referenceSystem.space == Space.Self) {
				dir = objectFrame.Y;
			} else {
				dir = worldFrame.Y;
			}
			SetPosition(dir * -increment.Increment);
			RestoreLatitude();
			RestoreLongitude();
			onPositionChange.Invoke();
		}

		void SetLatitude(string text)
		{
			double val;
			if (double.TryParse (text, out val)) {
				latitude = CanonicalAngle(val);
				SetPosition();
				onPositionChange.Invoke();
			}
			RestoreLatitude();
		}

		void RestoreLatitude(string text = null)
		{
			latitudeInput.text = $"{latitude * 180 / Math.PI}";
		}

		void SetLongitude(string text)
		{
			double val;
			if (double.TryParse (text, out val)) {
				longitude = CanonicalAngle(val);
				SetPosition();
				onPositionChange.Invoke();
			}
			RestoreLongitude();
		}

		void RestoreLongitude(string text = null)
		{
			longitudeInput.text = $"{longitude * 180 / Math.PI}";
		}

		void OnAltitudeIncrement()
		{
			altitudeAdjust.Value += increment.Increment;
			altitude = altitudeAdjust.Value;
			SetPosition();
			onPositionChange.Invoke();
		}

		void OnAltitudeDecrement()
		{
			altitudeAdjust.Value -= increment.Increment;
			altitude = altitudeAdjust.Value;
			SetPosition();
			onPositionChange.Invoke();
		}

		void OnAltitudeSet()
		{
			altitude = altitudeAdjust.Value;
			SetPosition();
			onPositionChange.Invoke();
		}

		void OnHeadingIncrement()
		{
			headingAdjust.Value += increment.Increment;
			if (headingAdjust.Value > 180) {
				headingAdjust.Value -= 360;
			}
			heading = headingAdjust.Value * Math.PI / 180;
			SetPosition();
			onPositionChange.Invoke();
		}

		void OnHeadingDecrement()
		{
			headingAdjust.Value -= increment.Increment;
			if (headingAdjust.Value < -180) {
				headingAdjust.Value += 360;
			}
			heading = headingAdjust.Value * Math.PI / 180;
			SetPosition();
			onPositionChange.Invoke();
		}

		void OnHeadingSet()
		{
			if (headingAdjust.Value < -180) {
				headingAdjust.Value += 360;
			} else if (headingAdjust.Value > 180) {
				headingAdjust.Value -= 360;
			}
			heading = headingAdjust.Value * Math.PI / 180;
			SetPosition();
			onPositionChange.Invoke();
		}

		public PositionEdit Body(CelestialBody body)
		{
			this.body = body;
			vectorDisplay.Body = body;
			return this;
		}

		public PositionEdit Increment (IncrementSize increment)
		{
			this.increment = increment;
			return this;
		}

		public static void GetLatLon (Vector3d pos, out double lat, out double lon)
		{
			double r = Math.Sqrt(pos.x * pos.x + pos.y * pos.y);
			lon = Math.Atan2 (pos.y, pos.x);
			lat = Math.Atan2 (pos.z, r);
		}

		void SetPosition()
		{
			double lat = Math.PI / 2 - latitude;
			double lon = Math.PI / 2 + longitude;
			double rot = heading;
			Planetarium.CelestialFrame.SetFrame(lon, lat, 0, ref worldFrame);
			Planetarium.CelestialFrame.SetFrame(lon, lat, rot, ref objectFrame);
			position = objectFrame.Z * (body.Radius + altitude);

			UpdateVectors();
		}

		void SetPosition(Vector3d offset)
		{
			Debug.Log($"[PositionEdit] SetPosition {position} {offset} {position+offset}");
			GetLatLon (position + offset, out latitude, out longitude);
			SetPosition ();
		}

		static double CanonicalAngle(double angle)
		{
			angle %= 360;
			if (angle > 180) {
				angle -= 360;
			} else if (angle < -180) {
				angle += 360;
			}
			return angle * Math.PI / 180;
		}

		public PositionEdit SetPosition (double latitude, double longitude, double altitude, double heading)
		{
			this.latitude = CanonicalAngle(latitude);
			this.longitude = CanonicalAngle(longitude);
			this.heading = CanonicalAngle(heading);
			this.altitude = altitude;
			RestoreLatitude();
			RestoreLongitude();
			altitudeAdjust.Value = altitude;
			headingAdjust.Value = heading;
			SetPosition();
			return this;
		}

		public PositionEdit OnPositionChange(UnityAction action)
		{
			onPositionChange.AddListener(action);
			return this;
		}

		public PositionEdit DoAltitude(bool doAltitude)
		{
			altitudeAdjust.SetActive(doAltitude);
			separator.SetActive(!doAltitude);
			return this;
		}

		public void Adjust(Vector3d dir)
		{
			if (referenceSystem.space == Space.Self) {
				dir = objectFrame.LocalToWorld(dir.xzy);
			} else {
				dir = worldFrame.LocalToWorld(dir.xzy);
			}
			SetPosition(dir * increment.Increment);
			RestoreLatitude();
			RestoreLongitude();
			onPositionChange.Invoke();
		}

		public PositionEdit VectorScale(double scale)
		{
			vectorDisplay.Scale = scale;
			return this;
		}

		void Update()
		{
			//FIXME remove need for Update
			vectorDisplay.Draw();
		}
	}
}

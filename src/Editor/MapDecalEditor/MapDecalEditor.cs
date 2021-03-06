﻿using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI
{
	using OptionData = TMP_Dropdown.OptionData;
    public class MapDecalEditor : Window
    {

        private static MapDecalEditor _instance = null;
        public static MapDecalEditor Instance
        {
            get
            {
                if (_instance == null) {
                    _instance = UIKit.CreateUI<MapDecalEditor> (UIMain.appCanvasRect, "KKMapDecalEditor");
                }
                return _instance;
            }
        }

        private List<Transform> transformList = new List<Transform>();
        private CelestialBody body;



        // Texture definitions
        internal Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
        internal Texture textureWorld = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/world", false);
        internal Texture textureCubes = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cubes", false);


        // GUI Windows
        internal Rect toolRect = new Rect(300, 35, 380, 840);

        // Holders

        internal static MapDecalInstance selectedDecal = null;
        internal static MapDecalInstance selectedDecalPrevious = null;

        private static Vector3d position = Vector3d.zero;
        private Vector3d referenceVector = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;
        private Vector3 orientation = Vector3.zero;

        private static double altitude = 0;
        private static double latitude, longitude = 0;

        private string smessage = "";

		InputLine mapDecalName;
		IncrementSize increment;
		PositionEdit positionEdit;
		ValueAdjuster placementOrder;
		ValueAdjuster radius;
		FieldToggle useAbsolute;
		ValueAdjuster absoluteOffset;
		UIDropdown heightmapSelector;
		UIDropdown colormapSelector;
		List<OptionData> heightmapNames;
		ValueAdjuster heighMapDeformity;
		ValueAdjuster smoothHeight;
		List<OptionData> colormapNames;
		ValueAdjuster smoothColor;
		FieldToggle removeScatter;
		FieldToggle useAlphaHeightSmoothing;
		FieldToggle cullBlack;
		InputLine groupName;

        public void Close()
        {
            if (KerbalKonstructs.camControl.active) {
                KerbalKonstructs.camControl.disable();
            }

            EditorGizmo.CloseGizmo();
            selectedDecal = null;
			SetActive(false);
        }

		public void Open()
		{
			SetActive(true);
			UpdateUI();
			Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
			EditorGizmo.SetupMoveGizmo(selectedDecal.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
			if (!KerbalKonstructs.camControl.active) {
				KerbalKonstructs.camControl.enable(selectedDecal.gameObject);
			}
		}

		public override void CreateUI()
		{
			base.CreateUI();

			this.Title(KKLocalization.MapDecalEditor)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<InputLine>(out mapDecalName)
					.Label(KKLocalization.MapDecalName)
					.OnSubmit(SetMapDecalName)
					.OnFocusLost(RestoreMapDecalName)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<IncrementSize>(out increment)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<PositionEdit>(out positionEdit)
					.Increment(increment)
					.DoAltitude(false)
					.OnPositionChange(OnPositionChange)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<ValueAdjuster>(out placementOrder)
					.Label(KKLocalization.MapDecalOrder)
					.OnIncrement(OnOrderIncrement)
					.OnDecrement(OnOrderDecrement)
					.OnValueSet(OnOrderSet)
					//.Format("F0")
					.InputWidth(100)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<ValueAdjuster>(out radius)
					.Label(KKLocalization.MapDecalRadius)
					.OnIncrement(OnRadiusIncrement)
					.OnDecrement(OnRadiusDecrement)
					.OnValueSet(OnRadiusSet)
					.InputWidth(100)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<HorizontalLayout>()
					.Add<FieldToggle>(out useAbsolute)
						.Text(KKLocalization.UseAbsolute)
						.Finish()
					.Add<UIButton>()
						.Text(KKLocalization.SnapSurface)
						.OnClick(SnapSurface)
						.Finish()
					.Finish()
				.Add<ValueAdjuster>(out absoluteOffset)
					.Label(KKLocalization.AbsoluteOffset)
					.OnIncrement(OnAbsoluteOffsetIncrement)
					.OnDecrement(OnAbsoluteOffsetDecrement)
					.OnValueSet(OnAbsoluteOffsetSet)
					//.Format("G4")
					.InputWidth(100)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<HorizontalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Add<UIText>()
						.Text(KKLocalization.HeightMap)
						.Finish()
					.Add<UIDropdown>(out heightmapSelector)
						.OnValueChanged(SelectHeightmap)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<ValueAdjuster>(out heighMapDeformity)
					.Label(KKLocalization.HeightMapDeformity)
					.OnIncrement(OnHeightMapDeformityIncrement)
					.OnDecrement(OnHeightMapDeformityDecrement)
					.OnValueSet(OnHeightMapDeformitySet)
					//.Format("F3")
					.InputWidth(100)
					.Finish()
				.Add<ValueAdjuster>(out smoothHeight)
					.Label(KKLocalization.SmoothHeight)
					.OnIncrement(OnSmoothHeightIncrement)
					.OnDecrement(OnSmoothHeightDecrement)
					.OnValueSet(OnSmoothHeightSet)
					//.Format("F3")
					.InputWidth(100)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()
				.Add<HorizontalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Add<UIText>()
						.Text(KKLocalization.ColorMap)
						.Finish()
					.Add<UIDropdown>(out colormapSelector)
						.OnValueChanged(SelectColormap)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				.Add<ValueAdjuster>(out smoothColor)
					.Label(KKLocalization.SmoothColor)
					.OnIncrement(OnSmoothColorIncrement)
					.OnDecrement(OnSmoothColorDecrement)
					.OnValueSet(OnSmoothColorSet)
					//.Format("F3")
					.InputWidth(100)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Finish()

				.Add<HorizontalSep>("HorizontalSep2") .Space(1, 2) .Finish()
				.Add<FieldToggle>(out removeScatter)
					.Text(KKLocalization.RemoveScatter)
					.Finish()
				.Add<FieldToggle>(out useAlphaHeightSmoothing)
					.Text(KKLocalization.UseAlphaHeightSmoothing)
					.Finish()
				.Add<FieldToggle>(out cullBlack)
					.Text(KKLocalization.CullBlack)
					.Finish()
				.Add<HorizontalSep>("HorizontalSep2") .Space(1, 2) .Finish()
				.Add<InputLine>(out groupName)
					.OnSubmit(SetGroupName)
					.OnFocusLost(RestoreGroupName)
					.Label(KKLocalization.Group)
					.Finish()

				.Add<UIButton>()
					.Text(KKLocalization.EditorSave)
					.OnClick(Save)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.Deselect)
					.OnClick(Deselect)
					.Finish()
				.Add<UIButton>()
					.Text(KKLocalization.DeleteInstance)
					.OnClick(DeleteInstance)
					.Finish()
				.Finish();

			UIMain.SetTitlebar(titlebar, Close);

			heightmapNames = new List<OptionData>();
			colormapNames = new List<OptionData>();
		}

		void BuildHeightmapList()
		{
			heightmapNames.Clear();
			int index = 0;
			for (int i = 0; i < DecalsDatabase.allHeightMaps.Count; i++) {
				var map = DecalsDatabase.allHeightMaps[i];
				heightmapNames.Add (new OptionData(map.Name));
				if (map.Name == selectedDecal.HeightMapName) {
					index = i;
				}
			}
			heightmapSelector.Options (heightmapNames);
			heightmapSelector.SetValueWithoutNotify(index);
		}

		void BuildColormapList()
		{
			colormapNames.Clear();
			int index = 0;
			for (int i = 0; i < DecalsDatabase.allColorMaps.Count; i++) {
				var map = DecalsDatabase.allColorMaps[i];
				colormapNames.Add (new OptionData(map.Name));
				if (map.Name == selectedDecal.ColorMapName) {
					index = i;
				}
			}
			colormapSelector.Options (colormapNames);
			colormapSelector.SetValueWithoutNotify(index);
		}

		public void UpdateUI()
		{
			RestoreMapDecalName();
			positionEdit.Body(selectedDecal.CelestialBody)
				.SetPosition(selectedDecal.Latitude, selectedDecal.Longitude,
							 selectedDecal.AbsolutOffset, selectedDecal.Angle);
			placementOrder.Value = selectedDecal.Order;
			radius.Value = selectedDecal.Radius;
			positionEdit.VectorScale (selectedDecal.Radius);
			useAbsolute.Field(selectedDecal, "UseAbsolut");
			absoluteOffset.Value = selectedDecal.AbsolutOffset;
			BuildHeightmapList();
			heighMapDeformity.Value = selectedDecal.HeightMapDeformity;
			smoothHeight.Value = selectedDecal.SmoothHeight;
			BuildColormapList();
			smoothColor.Value = selectedDecal.SmoothColor;
			removeScatter.Field(selectedDecal, "RemoveScatter");
			useAlphaHeightSmoothing.Field(selectedDecal, "UseAlphaHeightSmoothing");
			cullBlack.Field(selectedDecal, "CullBlack");
			RestoreGroupName();
		}

		void SetMapDecalName(string name)
		{
			selectedDecal.Name = name;
		}

		void RestoreMapDecalName(string name = null)
		{
			mapDecalName.text = selectedDecal.Name;
		}

		void SetGroupName(string name)
		{
			selectedDecal.Group = name;
		}

		void RestoreGroupName(string name = null)
		{
			mapDecalName.text = selectedDecal.Group;
		}

		void OnPositionChange()
		{
			var transform = selectedDecal.mapDecal.transform;
			transform.localPosition = positionEdit.Position;
			transform.localRotation = positionEdit.Rotation;
			selectedDecal.Latitude = positionEdit.Latitude;
			selectedDecal.Longitude = positionEdit.Longitude;
			selectedDecal.Angle = (float) positionEdit.Heading;
		}

		void OnOrderIncrement()
		{
			selectedDecal.Order += 1;
			placementOrder.Value = selectedDecal.Order;
		}

		void OnOrderDecrement()
		{
			selectedDecal.Order = Math.Max(100000, selectedDecal.Order - 1);
			placementOrder.Value = selectedDecal.Order;
		}

		void OnOrderSet()
		{
			selectedDecal.Order = (int) Math.Max(100000, placementOrder.Value - 1);
			placementOrder.Value = selectedDecal.Order;
		}

		void OnRadiusIncrement()
		{
			selectedDecal.Radius += increment.Increment;
			radius.Value = selectedDecal.Radius;
			positionEdit.VectorScale (selectedDecal.Radius);
		}

		void OnRadiusDecrement()
		{
			selectedDecal.Radius -= increment.Increment;
			selectedDecal.Radius = Math.Max(1, selectedDecal.Radius);
			radius.Value = selectedDecal.Radius;
		}

		void OnRadiusSet()
		{
			selectedDecal.Radius = Math.Max(1, radius.Value);
			radius.Value = selectedDecal.Radius;
		}

		void SnapSurface()
		{
			Vector3d nVec = positionEdit.NVector;
			double surfaceHeight = selectedDecal.CelestialBody.pqsController.GetSurfaceHeight(nVec);
			double altitude = surfaceHeight - selectedDecal.CelestialBody.Radius + 1;
			selectedDecal.mapDecal.transform.position = selectedDecal.CelestialBody.GetWorldSurfacePosition(positionEdit.Latitude, positionEdit.Longitude, altitude);
			selectedDecal.AbsolutOffset = (float)altitude;
			absoluteOffset.Value = selectedDecal.AbsolutOffset;
			positionEdit.Altitude = altitude;
			OnPositionChange();
		}

		void OnAbsoluteOffsetIncrement()
		{
			selectedDecal.AbsolutOffset += increment.Increment;
			absoluteOffset.Value = selectedDecal.AbsolutOffset;
		}

		void OnAbsoluteOffsetDecrement()
		{
			selectedDecal.AbsolutOffset -= increment.Increment;
			absoluteOffset.Value = selectedDecal.AbsolutOffset;
		}

		void OnAbsoluteOffsetSet()
		{
			selectedDecal.AbsolutOffset = (float) absoluteOffset.Value;
			absoluteOffset.Value = selectedDecal.AbsolutOffset;
		}

		void SelectHeightmap(int index)
		{
			selectedDecal.HeightMapName = heightmapNames[index].text;
		}

		void OnHeightMapDeformityIncrement()
		{
			selectedDecal.HeightMapDeformity += increment.Increment;
			heighMapDeformity.Value = selectedDecal.HeightMapDeformity;
		}

		void OnHeightMapDeformityDecrement()
		{
			selectedDecal.HeightMapDeformity -= increment.Increment;
			heighMapDeformity.Value = selectedDecal.HeightMapDeformity;
		}

		void OnHeightMapDeformitySet()
		{
			selectedDecal.HeightMapDeformity = (float) heighMapDeformity.Value;
			heighMapDeformity.Value = selectedDecal.HeightMapDeformity;
		}

		void OnSmoothHeightIncrement()
		{
			selectedDecal.SmoothHeight += increment.Increment;
			smoothHeight.Value = selectedDecal.SmoothHeight;
		}

		void OnSmoothHeightDecrement()
		{
			selectedDecal.SmoothHeight -= increment.Increment;
			selectedDecal.SmoothHeight = Math.Max(0, selectedDecal.SmoothHeight);
			smoothHeight.Value = selectedDecal.SmoothHeight;
		}

		void OnSmoothHeightSet()
		{
			selectedDecal.SmoothHeight = (float) Math.Max(0, smoothHeight.Value);
			smoothHeight.Value = selectedDecal.SmoothHeight;
		}

		void SelectColormap(int index)
		{
			selectedDecal.ColorMapName = colormapNames[index].text;
		}

		void OnSmoothColorIncrement()
		{
			selectedDecal.SmoothColor += increment.Increment;
			smoothColor.Value = selectedDecal.SmoothColor;
		}

		void OnSmoothColorDecrement()
		{
			selectedDecal.SmoothColor -= increment.Increment;
			selectedDecal.SmoothColor = Math.Max(0, selectedDecal.SmoothColor);
			smoothColor.Value = selectedDecal.SmoothColor;
		}

		void OnSmoothColorSet()
		{
			selectedDecal.SmoothColor = (float) Math.Max(0, smoothColor.Value);
			smoothColor.Value = selectedDecal.SmoothColor;
		}

		void Save()
		{
			SaveSettings();
			smessage = "Saved changes to this object.";
			MiscUtils.HUDMessage(smessage, 10, 2);
		}

		void Deselect()
		{
			smessage = "discarding changes";
			MiscUtils.HUDMessage(smessage, 10, 2);
			Close();
		}

		void DeleteInstance()
		{
            if (selectedDecalPrevious == selectedDecal)
                selectedDecalPrevious = null;

            selectedDecal.gameObject.transform.parent = null;
            selectedDecal.mapDecal.transform.parent = null;
            selectedDecal.gameObject.DestroyGameObject();

            selectedDecal.CelestialBody.pqsController.RebuildSphere();

            DecalsDatabase.DeleteMapDecalInstance(selectedDecal);

            Close();
		}

        private Vector3 UpVector
        {
            get {
                body = FlightGlobals.ActiveVessel.mainBody;
                return (Vector3)body.GetSurfaceNVector(latitude, longitude).normalized;
            }
        }

        internal void OnMoveCallBack(Vector3 vector)
        {
            selectedDecal.gameObject.transform.position = EditorGizmo.moveGizmo.transform.position;
            position = EditorGizmo.moveGizmo.transform.position;
            FlightGlobals.currentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
        }

        internal void WhenMovedCallBack(Vector3 vector)
        {
            position = EditorGizmo.moveGizmo.transform.position;
            FlightGlobals.currentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);

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
    }
}

using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;
using Upgradeables;
using System.Reflection;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
	public class ModelInfo
	{
		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);

		public PQSCity pqsCity;

		public double dUpdater = 0;

		Rect StaticInfoRect = new Rect(300, 50, 300, 400);

		public Boolean displayingInfo = false;
		public Boolean bCycle = true;
		public Boolean bSpinning = true;

		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle KKWindow;
		GUIStyle BoxNoBorder;

		String infTitle = "";
		String infMesh = "";
		String infCost = "";
		String infAuthor = "";
		String infManufacturer = "";
		String infDescription = "";
		String infCategory = "";

		public StaticModel mModel = null;
		public StaticObject lastPreview = null;
		public static StaticObject currPreview = null;

		public GameObject gModel = null;

		public void drawModelInfoGUI(StaticModel mSelectedModel)
		{
			if (mSelectedModel != null)
			{
				if (mModel != mSelectedModel)
				{
					if (currPreview != null)
						DestroyPreviewInstance(currPreview);
					
					updateSelection(mSelectedModel);
					
					if (KerbalKonstructs.instance.spawnPreviewModels) 
						CreatePreviewInstance(mSelectedModel);
				}

				mModel = mSelectedModel;

				KKWindow = new GUIStyle(GUI.skin.window);
				KKWindow.padding = new RectOffset(8, 8, 3, 3);

				StaticInfoRect = GUI.Window(0xD00B1E2, StaticInfoRect, drawStaticInfoWindow, "", KKWindow);
			}
		}

		public void drawStaticInfoWindow(int WindowID)
		{
			if (mModel == null) return;

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

			if (currPreview != null)
			{
				double dTicker = Planetarium.GetUniversalTime();
				if ((dTicker - dUpdater) > 0.01)
				{
					dUpdater = Planetarium.GetUniversalTime();
					
					if (bSpinning) 
						SpinPreview(currPreview);
				}
			}

			bool shouldUpdateSelection = false;
			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Static Model Config Editor", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
				{
					if (currPreview != null)
						DestroyPreviewInstance(currPreview);

					KerbalKonstructs.instance.showModelInfo = false;
					mModel = null;
					KerbalKonstructs.instance.selectedModel = null;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUILayout.Box(" " + infTitle + " ");
			GUILayout.Space(3);
			GUILayout.Box("Mesh: " + infMesh + ".mu");

			GUILayout.Box("Manufacturer: " + infManufacturer);
			GUILayout.Box("Author: " + infAuthor);
			GUILayout.Space(3);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Category: ");
			GUILayout.FlexibleSpace();
			infCategory = GUILayout.TextField(infCategory, GUILayout.Width(150));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Cost: ");
			GUILayout.FlexibleSpace();
			infCost = GUILayout.TextField(infCost, GUILayout.Width(150));
			GUILayout.EndHorizontal();
			
			GUILayout.Label("Description");
			infDescription = GUILayout.TextArea(infDescription, GUILayout.Height(100));

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save", GUILayout.Height(23)))
			{
				updateSettings(mModel);
				KerbalKonstructs.instance.saveObjects();
				smessage = "Saved all changes to all static models and instances.";
				ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
			}
			if (GUILayout.Button("Close", GUILayout.Height(23)))
			{
				if (currPreview != null)
					DestroyPreviewInstance(currPreview);

				KerbalKonstructs.instance.showModelInfo = false;
				mModel = null;
				KerbalKonstructs.instance.selectedModel = null;
			}
			GUILayout.EndHorizontal();

			if (currPreview != null)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Delete Preview", GUILayout.Height(23)))
					DestroyPreviewInstance(currPreview);

				if (bSpinning)
				{
					if (GUILayout.Button("Stop Spin", GUILayout.Height(23)))
						bSpinning = false;

				}
				else
				{
					if (GUILayout.Button("Resume Spin", GUILayout.Height(23)))
						bSpinning = true;
				}

				GUILayout.EndHorizontal();
			}

			if (Event.current.keyCode == KeyCode.Return)
			{
				ScreenMessages.PostScreenMessage("Applied changes to object.", 10, smsStyle);
				shouldUpdateSelection = true;
			}

			if (shouldUpdateSelection)
			{
				updateSettings(mModel);
				updateSelection(mModel);
			}

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void updateSettings(StaticModel mModel)
		{
			mModel.setSetting("author", infAuthor);
			mModel.setSetting("mesh", infMesh);
			mModel.setSetting("manufacturer", infManufacturer);
			mModel.setSetting("cost", infCost);
			mModel.setSetting("description", infDescription);
			mModel.setSetting("title", infTitle);
			mModel.setSetting("category", infCategory);
		}

		public void updateSelection(StaticModel obj)
		{
			infAuthor = (string)obj.getSetting("author");
			infMesh = "" + obj.getSetting("mesh");
			infManufacturer = (string)obj.getSetting("manufacturer");
			infCost = obj.getSetting("cost").ToString();
			infDescription = (string)obj.getSetting("description");
			infTitle = (string)obj.getSetting("title");
			infCategory = (string)obj.getSetting("category");
		}


		public static void DestroyPreviewInstance(StaticObject soInstance)
		{
			if (soInstance != null)
			{
				if (currPreview != null)
				{
					if (currPreview == soInstance)
						currPreview = null;
				}				
				
				KerbalKonstructs.instance.deleteObject(soInstance);

			}
			else
			{
				if (currPreview != null)
				{
					KerbalKonstructs.instance.deleteObject(currPreview);
					currPreview = null;
				}
			}
		}

		public void CreatePreviewInstance(StaticModel model)
		{
			StaticObject obj = new StaticObject();
			obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));
			obj.setSetting("RadiusOffset", (float)FlightGlobals.ActiveVessel.altitude);
			obj.setSetting("CelestialBody", KerbalKonstructs.instance.getCurrentBody());
			obj.setSetting("Group", "Ungrouped");
			obj.setSetting("RadialPosition", KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position));
			obj.setSetting("RotationAngle", 0f);
			obj.setSetting("Orientation", Vector3.up);
			obj.setSetting("VisibilityRange", 25000f);

			obj.model = model;

			KerbalKonstructs.instance.getStaticDB().addStatic(obj);
			obj.spawnObject(true, true);
			// KerbalKonstructs.instance.selectObject(obj, false);
			currPreview = obj;
		}

		public void SpinPreview(StaticObject soObject)
		{
			if (soObject == null || currPreview == null) return;

			float fRot = (float)soObject.getSetting("RotationAngle") + 0.1f;
			if (fRot > 360) fRot -= 360;

			soObject.setSetting("RotationAngle", fRot);
			soObject.update();
		}
	}
}

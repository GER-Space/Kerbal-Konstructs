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
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
	class EditorGUI
	{
		#region Variable Declarations

			private List<Transform> transformList = new List<Transform>();

			#region Texture Definitions
			// Texture definitions
			public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
			public Texture tBilleted = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/billeted", false);
			public Texture tCopyPos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/copypos", false);
			public Texture tPastePos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/pastepos", false);
			public Texture tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
			public Texture tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
			public Texture tSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/search", false);
			public Texture tCancelSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cancelsearch", false);
			public Texture tVAB = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/VABMapIcon", false);
			public Texture tSPH = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SPHMapIcon", false);
			public Texture tANY = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ANYMapIcon", false);
			public Texture tFocus = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/focuson", false);
			public Texture tSnap = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/snapto", false);

			#endregion

			#region Switches
			// Switches
			public Boolean enableColliders = false;
			public Boolean editingSite = false;

			public Boolean creatingInstance = false;
			public Boolean showLocal = false;
			public Boolean onNGS = false;
			public Boolean displayingInfo = false;
			public Boolean SnapRotateMode = false;
			#endregion

			#region GUI Windows
			// GUI Windows
			Rect toolRect = new Rect(300, 35, 310, 570);
			Rect siteEditorRect = new Rect(400, 45, 340, 480);

			#endregion

			#region GUI elements
			// GUI elements
			Vector2 descScroll;
			GUIStyle listStyle = new GUIStyle();
			GUIStyle navStyle = new GUIStyle();

			GUIStyle DeadButton;
			GUIStyle DeadButtonRed;
			GUIStyle KKWindow;
			GUIStyle BoxNoBorder;

			SiteType siteType;
			GUIContent[] siteTypeOptions = {
											new GUIContent("VAB"),
											new GUIContent("SPH"),
											new GUIContent("ANY")
										};
			ComboBox siteTypeMenu;
			#endregion

			#region Holders
			// Holders

			public static StaticObject selectedObject = null;
			public StaticObject selectedObjectPrevious = null;
			static LaunchSite lTargetSite = null;

			public static String xPos, yPos, zPos, altitude, rotation = "";
			public static String xOri, yOri, zOri = "";
			public static String facType = "None";
			public static String sGroup = "Ungrouped";
			public static String visrange = "";
			String increment = "1";
			String siteName, siteTrans, siteDesc, siteAuthor, siteCategory;
			float flOpenCost, flCloseValue, flRecoveryFactor, flRecoveryRange, flLaunchRefund;

			Vector3 orientation = Vector3.zero;
			Vector3 vbsnapangle1 = new Vector3(0,0,0);
			Vector3 vbsnapangle2 = new Vector3(0, 0, 0);

			Vector3 snapSourceWorldPos = new Vector3(0, 0, 0);
			Vector3 snapTargetWorldPos = new Vector3(0, 0, 0);

			String sSTROT = "";

			GameObject selectedSnapPoint = null;
			GameObject selectedSnapPoint2 = null;
			public StaticObject snapTargetInstance = null;
			StaticObject snapTargetInstancePrevious = null;

			Vector3 snpspos = new Vector3(0,0,0);
			Vector3 snptpos = new Vector3(0, 0, 0);
			Vector3 vDrift = new Vector3(0, 0, 0);
			Vector3 vCurrpos = new Vector3(0, 0, 0);

			#endregion
		
		#endregion
		
		public EditorGUI()
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
			
			siteTypeMenu = new ComboBox(siteTypeOptions[0], siteTypeOptions, "button", "box", null, listStyle);
		}

		#region draw Methods

		public void drawEditor(StaticObject obj)
		{
			if (obj != null)
			{
				if (selectedObject != obj)
					updateSelection(obj);

				KKWindow = new GUIStyle(GUI.skin.window);
				KKWindow.padding = new RectOffset(8, 8, 3, 3);

				toolRect = GUI.Window(0xB00B1E3, toolRect, drawToolWindow, "", KKWindow);

				if (editingSite)
				{
						siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawSiteEditorWindow, "", KKWindow);
				}
			}
		}

		#endregion
		
		#region Editors

		#region Instance Editor
		string savedxpos = "";
		string savedypos = "";
		string savedzpos = "";
		string savedalt = "";
		string savedrot = "";
		bool savedpos = false;
		bool pospasted = false;

		// INSTANCE EDITOR
		void drawToolWindow(int windowID)
		{
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

			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;
			Vector3 position = Vector3.zero;
			float alt = 0;
			float newRot = 0;
			float vis = 0;
			bool shouldUpdateSelection = false;
			bool manuallySet = false;

			double objLat = 0;
			double objLon = 0;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Instance Editor", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
				{
					KerbalKonstructs.instance.saveObjects();
					KerbalKonstructs.instance.deselectObject(true, true);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUILayout.Box((string)selectedObject.model.getSetting("title"));

			GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Position   ");
				GUILayout.Space(15);

				if (GUILayout.Button(new GUIContent(tCopyPos, "Copy Position."), GUILayout.Width(23), GUILayout.Height(23)))
				{
					savedpos = true;
					savedxpos = xPos;
					savedypos = yPos;
					savedzpos = zPos;
					savedalt = altitude;
					savedrot = rotation;
					// Debug.Log("KK: Instance position copied");
				}
				if (GUILayout.Button(new GUIContent(tPastePos, "Paste Position."), GUILayout.Width(23), GUILayout.Height(23)))
				{
					if (savedpos)
					{
						pospasted = true;
						xPos = savedxpos;
						yPos = savedypos;
						zPos = savedzpos;
						altitude = savedalt;
						rotation = savedrot;
						// Debug.Log("KK: Instance position pasted");
					}
				}
				if (GUILayout.Button(new GUIContent(tSnap, "Snap to Target."), GUILayout.Width(23), GUILayout.Height(23)))
				{
					if (snapTargetInstance == null)
					{

					}
					else
					{
						Vector3 snapTargetPos = (Vector3)snapTargetInstance.getSetting("RadialPosition");
						float snapTargetAlt = (float)snapTargetInstance.getSetting("RadiusOffset");
						selectedObject.setSetting("RadialPosition", snapTargetPos);
						selectedObject.setSetting("RadiusOffset", snapTargetAlt);
					}

					if (!KerbalKonstructs.instance.DevMode)
					{
						selectedObject.setSetting("CustomInstance", "True");
					}
					updateSelection(selectedObject);
				}

				GUILayout.FlexibleSpace();
				GUILayout.Label("Increment");
				increment = GUILayout.TextField(increment, 3, GUILayout.Width(30));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.Label("X:");
			GUILayout.FlexibleSpace();
			xPos = GUILayout.TextField(xPos, 25, GUILayout.Width(80));
			if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
			{
				position.x -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
			{
				position.x += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Y:");
			GUILayout.FlexibleSpace();
			yPos = GUILayout.TextField(yPos, 25, GUILayout.Width(80));
			if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
			{
				position.y -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
			{
				position.y += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Z:");
			GUILayout.FlexibleSpace();
			zPos = GUILayout.TextField(zPos, 25, GUILayout.Width(80));
			if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
			{
				position.z -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
			{
				position.z += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			GUILayout.BeginHorizontal();
			{
				var objectpos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(selectedObject.gameObject.transform.position);
				var dObjectLat = NavUtils.GetLatitude(objectpos);
				var dObjectLon = NavUtils.GetLongitude(objectpos);
				var disObjectLat = dObjectLat * 180 / Math.PI;
				var disObjectLon = dObjectLon * 180 / Math.PI;

				if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

				selectedObject.setSetting("RefLatitude", disObjectLat);
				selectedObject.setSetting("RefLongitude", disObjectLon);

				GUILayout.Box("Latitude");
				GUILayout.Box(disObjectLat.ToString("#0.00"));
				GUILayout.Box("Longitude");
				GUILayout.Box(disObjectLon.ToString("#0.00"));
			}
			GUILayout.EndHorizontal();

			GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Alt.");
				GUILayout.FlexibleSpace();
				altitude = GUILayout.TextField(altitude, 25, GUILayout.Width(80));
				if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
				{
					alt -= float.Parse(increment);
					shouldUpdateSelection = true;
				}
				if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
				{
					alt += float.Parse(increment);
					shouldUpdateSelection = true;
				}
			}
			GUILayout.EndHorizontal();

			var pqsc = ((CelestialBody)selectedObject.getSetting("CelestialBody")).pqsController;

			if (GUILayout.Button("Snap to Terrain", GUILayout.Height(21)))
			{
				alt = 1.0f + ((float)(pqsc.GetSurfaceHeight((Vector3)selectedObject.getSetting("RadialPosition")) - pqsc.radius - (float)selectedObject.getSetting("RadiusOffset")));
				shouldUpdateSelection = true;
			}

			GUI.enabled = true;

			bool isDevMode = KerbalKonstructs.instance.DevMode;

			if (isDevMode && selectedObject != null)
			{
				GUILayout.Space(10);
				GUILayout.Box("SNAP-POINTS");

				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Source ");
					GUILayout.Box("" + vbsnapangle1.ToString() + "d");
					GUILayout.Box("Wpos " + snapSourceWorldPos.ToString());
					GUILayout.FlexibleSpace();
					Transform[] transformList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
					List<GameObject> snappointList = (from t in transformList where t.gameObject.name == "snappoint" select t.gameObject).ToList();

					foreach (GameObject tSnapPoint in snappointList)
					{
						GUI.enabled = (tSnapPoint != selectedSnapPoint);
						if (GUILayout.Button("*", GUILayout.Width(23), GUILayout.Height(23)))
						{
							selectedSnapPoint = tSnapPoint;
							SnapToTarget();
							updateSelection(selectedObject);
						}
						GUI.enabled = true;
					}
				}
				GUILayout.EndHorizontal();
			}

			if (isDevMode && snapTargetInstance != null)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Target ");
					GUILayout.Box("" + vbsnapangle2.ToString() + "d");
					GUILayout.Box("Wpos " + snapTargetWorldPos.ToString());
					GUILayout.FlexibleSpace();
					Transform[] transformList2 = snapTargetInstance.gameObject.GetComponentsInChildren<Transform>();
					List<GameObject> snappointList2 = (from t2 in transformList2 where t2.gameObject.name == "snappoint" select t2.gameObject).ToList();

					foreach (GameObject tSnapPoint2 in snappointList2)
					{
						GUI.enabled = (tSnapPoint2 != selectedSnapPoint2);
						if (GUILayout.Button("*", GUILayout.Width(23), GUILayout.Height(23)))
						{
							selectedSnapPoint2 = tSnapPoint2;
							SnapToTarget(SnapRotateMode);
							updateSelection(selectedObject);
						}
						GUI.enabled = true;
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					sSTROT = snapTargetInstance.pqsCity.reorientFinalAngle.ToString();
					GUILayout.Box("Rot " + sSTROT);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Snap", GUILayout.Height(23)))
					{
						SnapToTarget(SnapRotateMode);
					}
					GUILayout.Space(10);

					SnapRotateMode = GUILayout.Toggle(SnapRotateMode, "SnapRot Mode", GUILayout.Height(23));

					GUILayout.FlexibleSpace();
					if (GUILayout.Button("DriftFix", GUILayout.Height(23)))
					{
						if (selectedSnapPoint == null || selectedSnapPoint2 == null)
						{
						}
						else
						{
							FixDrift(SnapRotateMode);
						}
					}
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(5);

			GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Rot.");
				GUILayout.FlexibleSpace();
				rotation = GUILayout.TextField(rotation, 4, GUILayout.Width(80));

				if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
				{
					newRot -= float.Parse(increment) / 10f;
					shouldUpdateSelection = true;
				}
				if (GUILayout.Button(new GUIContent("<", "-45"), GUILayout.Width(30), GUILayout.Height(23)))
				{
					newRot -= 45.0f;
					shouldUpdateSelection = true;
				}
				if (GUILayout.Button(new GUIContent(">", "+45"), GUILayout.Width(30), GUILayout.Height(23)))
				{
					newRot += 45.0f;
					shouldUpdateSelection = true;
				}
				if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
				{
					newRot += float.Parse(increment) / 10f;
					shouldUpdateSelection = true;
				}
			}
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Vis.");
				GUILayout.FlexibleSpace();
				visrange = GUILayout.TextField(visrange, 6, GUILayout.Width(80));
				if (GUILayout.Button("Min", GUILayout.Width(30), GUILayout.Height(23)))
				{
					vis -= 1000000000f;
					//visrange = "2500";
					shouldUpdateSelection = true;
				}
				if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(23)))
				{
					vis -= 2500f;
					shouldUpdateSelection = true;
				}
				if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(23)))
				{
					vis += 2500f;
					shouldUpdateSelection = true;
				}
				if (GUILayout.Button("Max", GUILayout.Width(30), GUILayout.Height(23)))
				{
					vis = (float)KerbalKonstructs.instance.maxEditorVisRange;
					shouldUpdateSelection = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);

			GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Orientation");
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("U", "Top Up"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					xOri = "0"; yOri = "1"; zOri = "0"; pospasted = true; shouldUpdateSelection = true;
				}
				if (GUILayout.Button(new GUIContent("D", "Bottom Up"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					xOri = "0"; yOri = "-1"; zOri = "0"; pospasted = true; shouldUpdateSelection = true;
				}
				if (GUILayout.Button(new GUIContent("L", "On Left"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					xOri = "1"; yOri = "0"; zOri = "0"; pospasted = true; shouldUpdateSelection = true;
				}
				if (GUILayout.Button(new GUIContent("R", "On Right"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					xOri = "-1"; yOri = "0"; zOri = "0"; pospasted = true; shouldUpdateSelection = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("(-1 to +1)");
				GUILayout.FlexibleSpace();


				GUILayout.Label("X");
				if (GUILayout.Button(new GUIContent("<", "Tip X"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					float fxOri = float.Parse(xOri);
					fxOri = fxOri - 0.01f;
					if (fxOri < -1.00f) xOri = "-1";
						else  xOri = fxOri.ToString("#0.00"); 
					pospasted = true; 
					shouldUpdateSelection = true;
				}
				xOri = GUILayout.TextField(xOri, 25, GUILayout.Width(25));
				if (GUILayout.Button(new GUIContent(">", "Tip X"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					float fxOri = float.Parse(xOri);
					fxOri = fxOri + 0.01f;
					if (fxOri > 1.00f) xOri = "1";
						else xOri = fxOri.ToString("#0.00"); 
					pospasted = true; 
					shouldUpdateSelection = true;
				}

				GUILayout.Label(" Y");
				yOri = GUILayout.TextField(yOri, 25, GUILayout.Width(25));

				GUILayout.Label(" Z");
				if (GUILayout.Button(new GUIContent("<", "Tip Z"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					float fzOri = float.Parse(zOri);
					fzOri = fzOri - 0.01f;
					if (fzOri < -1.00f) zOri = "-1";
						else zOri = fzOri.ToString("#0.00"); 
					pospasted = true; 
					shouldUpdateSelection = true;
				}
				zOri = GUILayout.TextField(zOri, 25, GUILayout.Width(25));
				if (GUILayout.Button(new GUIContent(">", "Tip Z"), GUILayout.Height(21), GUILayout.Width(18)))
				{
					float fzOri = float.Parse(zOri);
					fzOri = fzOri + 0.01f;
					if (fzOri > 1.00f) zOri = "1";
						else zOri = fzOri.ToString("#0.00");
					pospasted = true; 
					shouldUpdateSelection = true;
				}
			}
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Facility Type: ", GUILayout.Height(23));
				GUILayout.FlexibleSpace();
				facType = GUILayout.TextField(facType, 30, GUILayout.Width(185), GUILayout.Height(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Group: ", GUILayout.Height(23));
				GUILayout.FlexibleSpace();
				sGroup = GUILayout.TextField(sGroup, 30, GUILayout.Width(185), GUILayout.Height(23));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);

			GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

			GUILayout.BeginHorizontal();
			{
				enableColliders = GUILayout.Toggle(enableColliders, "Enable Colliders", GUILayout.Width(140), GUILayout.Height(23));

				Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
				List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();

				if (enableColliders)
				{
					foreach (GameObject collider in colliderList)
					{
						collider.collider.enabled = true;
					}
				}
				if (!enableColliders)
				{
					foreach (GameObject collider in colliderList)
					{
						collider.collider.enabled = false;
					}
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Duplicate", GUILayout.Width(130), GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.saveObjects();
					StaticModel oModel = selectedObject.model;
					float fOffset = (float)selectedObject.getSetting("RadiusOffset");
					Vector3 vPosition = (Vector3)selectedObject.getSetting("RadialPosition");
					float fAngle = (float)selectedObject.getSetting("RotationAngle");
					smessage = "Spawned duplicate " + selectedObject.model.getSetting("title");
					KerbalKonstructs.instance.deselectObject(true, true);
					spawnInstance(oModel, fOffset, vPosition, fAngle);
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
			}
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			GUILayout.Space(10);

			GUI.enabled = !editingSite;

			string sLaunchPadTransform = (string)selectedObject.getSetting("LaunchPadTransform");
			string sDefaultPadTransform = (string)selectedObject.model.getSetting("DefaultLaunchPadTransform");
			string sLaunchsiteDesc = (string)selectedObject.getSetting("LaunchSiteDescription");
			string sModelDesc = (string)selectedObject.model.getSetting("description");

			if (sLaunchPadTransform == "" && sDefaultPadTransform == "")
				GUI.enabled = false;

			if (GUILayout.Button(((selectedObject.settings.ContainsKey("LaunchSiteName")) ? "Edit" : "Make") + " Launchsite", GUILayout.Height(23)))
			{
				// Edit or make a launchsite
				siteName = (string)selectedObject.getSetting("LaunchSiteName");
				siteTrans = (selectedObject.settings.ContainsKey("LaunchPadTransform")) ? sLaunchPadTransform : sDefaultPadTransform;

				if (sLaunchsiteDesc != "")
					siteDesc = sLaunchsiteDesc;
				else
					siteDesc = sModelDesc;

				siteCategory = (string)selectedObject.getSetting("Category");
				siteType = (SiteType)selectedObject.getSetting("LaunchSiteType");
				flOpenCost = (float)selectedObject.getSetting("OpenCost");
				flCloseValue = (float)selectedObject.getSetting("CloseValue");
				stOpenCost = string.Format("{0}", flOpenCost);
				stCloseValue = string.Format("{0}", flCloseValue);

				flRecoveryFactor = (float)selectedObject.getSetting("RecoveryFactor");
				flRecoveryRange = (float)selectedObject.getSetting("RecoveryRange");
				flLaunchRefund = (float)selectedObject.getSetting("LaunchRefund");

				stRecoveryFactor = string.Format("{0}", flRecoveryFactor);
				stRecoveryRange = string.Format("{0}", flRecoveryRange);
				stLaunchRefund = string.Format("{0}", flLaunchRefund);


				siteAuthor = (selectedObject.settings.ContainsKey("author")) ? (string)selectedObject.getSetting("author") : (string)selectedObject.model.getSetting("author");
				// Debug.Log("KK: Making or editing a launchsite");
				editingSite = true;
			}

			GUI.enabled = true;

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Save", GUILayout.Width(130), GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.saveObjects();
					smessage = "Saved all changes to all objects.";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Deselect", GUILayout.Width(130), GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.saveObjects();
					KerbalKonstructs.instance.deselectObject(true, true);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Delete Instance", GUILayout.Height(21)))
			{
				if (snapTargetInstance == selectedObject) snapTargetInstance = null;
				if (snapTargetInstancePrevious == selectedObject) snapTargetInstancePrevious = null;
				if (selectedObjectPrevious == selectedObject) selectedObjectPrevious = null;
				KerbalKonstructs.instance.deleteObject(selectedObject);
				selectedObject = null;
			}

			GUILayout.Space(15);

			if (Event.current.keyCode == KeyCode.Return || (pospasted))
			{
				ScreenMessages.PostScreenMessage("Applied changes to object.", 10, smsStyle);
				pospasted = false;
				manuallySet = true;
				position.x = float.Parse(xPos);
				position.y = float.Parse(yPos);
				position.z = float.Parse(zPos);

				//if (float.Parse(xOri) > 1.0f || float.Parse(xOri) < -1.0f) xOri = "0";
				//if (float.Parse(yOri) > 1.0f || float.Parse(yOri) < -1.0f) yOri = "0";
				//if (float.Parse(zOri) > 1.0f || float.Parse(zOri) < -1.0f) zOri = "0";
				orientation.x = float.Parse(xOri);
				orientation.y = float.Parse(yOri);
				orientation.z = float.Parse(zOri);

				selectedObject.setSetting("Orientation", orientation);

				vis = float.Parse(visrange);
				alt = float.Parse(altitude);

				float rot = float.Parse(rotation);
				while (rot > 360 || rot < 0)
				{
					if (rot > 360)
					{
						rot -= 360;
					}
					else if (rot < 0)
					{
						rot += 360;
					}
				}
				newRot = rot;
				rotation = rot.ToString();

				shouldUpdateSelection = true;
			}

			if (shouldUpdateSelection)
			{
				if (!manuallySet)
				{
					position += (Vector3)selectedObject.getSetting("RadialPosition");
					orientation.x = float.Parse(xOri);
					orientation.y = float.Parse(yOri);
					orientation.z = float.Parse(zOri);
					alt += (float)selectedObject.getSetting("RadiusOffset");
					newRot += (float)selectedObject.getSetting("RotationAngle");
					vis += (float)selectedObject.getSetting("VisibilityRange");

					if ((float)orientation.x > 1.0f || (float)orientation.x < -1.0f)
					{
						//xOri = "0";
						//orientation.x = 0.0f;
					}
					if ((float)orientation.y > 1.0f || (float)orientation.y < -1.0f)
					{
						//yOri = "0";
						//orientation.y = 0.0f;
					}
					if ((float)orientation.z > 1.0f || (float)orientation.z < -1.0f)
					{
						//zOri = "0";
						//orientation.z = 0.0f;
					}
					selectedObject.setSetting("Orientation", orientation);

					while (newRot > 360 || newRot < 0)
					{
						if (newRot > 360)
						{
							newRot -= 360;
						}
						else if (newRot < 0)
						{
							newRot += 360;
						}
					}

					if (vis > (float)KerbalKonstructs.instance.maxEditorVisRange || vis < 1000)
					{
						if (vis > (float)KerbalKonstructs.instance.maxEditorVisRange)
						{
							vis = (float)KerbalKonstructs.instance.maxEditorVisRange;
						}
						else if (vis < 1000)
						{
							vis = 1000;
						}
					}
				}

				selectedObject.setSetting("RadialPosition", position);
				
				selectedObject.setSetting("RadiusOffset", alt);
				selectedObject.setSetting("RotationAngle", newRot);
				selectedObject.setSetting("VisibilityRange", vis);
				selectedObject.setSetting("RefLatitude", objLat);
				selectedObject.setSetting("RefLongitude", objLon);

				selectedObject.setSetting("FacilityType", facType);
				selectedObject.setSetting("Group", sGroup);

				if (!KerbalKonstructs.instance.DevMode)
				{
					selectedObject.setSetting("CustomInstance", "True");
				}

				updateSelection(selectedObject);
			}

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

		#region Launchsite Editor
		// Launchsite Editor handlers
		string stOpenCost;
		string stCloseValue;
		string stRecoveryFactor;
		string stRecoveryRange;
		string stLaunchRefund;

		// LAUNCHSITE EDITOR
		void drawSiteEditorWindow(int id)
		{
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

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Launchsite Editor", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
				{
					editingSite = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUILayout.Box((string)selectedObject.model.getSetting("title"));
			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Name: ", GUILayout.Width(120));
				siteName = GUILayout.TextField(siteName);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Transform: ", GUILayout.Width(120));
			GUILayout.Box("" + siteTrans);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Category: ", GUILayout.Width(120));
				GUILayout.Label(siteCategory, GUILayout.Width(80));
				GUILayout.FlexibleSpace();
				GUI.enabled = !(siteCategory == "RocketPad");
				if (GUILayout.Button("RP"))
					siteCategory = "RocketPad";
				GUI.enabled = !(siteCategory == "Runway");
				if (GUILayout.Button("RW"))
					siteCategory = "Runway";
				GUI.enabled = !(siteCategory == "Helipad");
				if (GUILayout.Button("HP"))
					siteCategory = "Helipad";
				GUI.enabled = !(siteCategory == "Other");
				if (GUILayout.Button("OT"))
					siteCategory = "Other";
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Type: ", GUILayout.Width(120));
				if (siteType == (SiteType)0)
					GUILayout.Label("VAB", GUILayout.Width(40));
				if (siteType == (SiteType)1)
					GUILayout.Label("SPH", GUILayout.Width(40));
				if (siteType == (SiteType)2)
					GUILayout.Label("Any", GUILayout.Width(40));
				GUILayout.FlexibleSpace();
				GUI.enabled = !(siteType == (SiteType)0);
				if (GUILayout.Button("VAB"))
					siteType = ((SiteType)0);
				GUI.enabled = !(siteType == (SiteType)1);
				if (GUILayout.Button("SPH"))
					siteType = ((SiteType)1);
				GUI.enabled = !(siteType == (SiteType)2);
				if (GUILayout.Button("Any"))
					siteType = ((SiteType)2);
			GUILayout.EndHorizontal();

			GUI.enabled = true;
			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Author: ", GUILayout.Width(120));
				siteAuthor = GUILayout.TextField(siteAuthor);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Open Cost: ", GUILayout.Width(120));
				stOpenCost = GUILayout.TextField(stOpenCost);
				GUILayout.Label(" \\F");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Close Value: ", GUILayout.Width(120));
				stCloseValue = GUILayout.TextField(stCloseValue);
				GUILayout.Label(" \\F");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Recovery Factor: ", GUILayout.Width(120));
			stRecoveryFactor = GUILayout.TextField(stRecoveryFactor);
			GUILayout.Label(" %");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Effective Range: ", GUILayout.Width(120));
			stRecoveryRange = GUILayout.TextField(stRecoveryRange);
			GUILayout.Label(" m");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Launch Refund: ", GUILayout.Width(120));
			stLaunchRefund = GUILayout.TextField(stLaunchRefund);
			GUILayout.Label(" %");
			GUILayout.EndHorizontal();

			GUILayout.Label("Description: ");
			descScroll = GUILayout.BeginScrollView(descScroll);
				siteDesc = GUILayout.TextArea(siteDesc, GUILayout.ExpandHeight(true));
			GUILayout.EndScrollView();

			GUI.enabled = true;
			GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save", GUILayout.Width(115)))
				{
					Boolean addToDB = (selectedObject.settings.ContainsKey("LaunchSiteName") && siteName != "");
					selectedObject.setSetting("LaunchSiteName", siteName);
					selectedObject.setSetting("LaunchSiteType", siteType);
					selectedObject.setSetting("LaunchPadTransform", siteTrans);
					selectedObject.setSetting("LaunchSiteDescription", siteDesc);
					selectedObject.setSetting("OpenCost", float.Parse(stOpenCost));
					selectedObject.setSetting("CloseValue", float.Parse(stCloseValue));
					selectedObject.setSetting("RecoveryFactor", float.Parse(stRecoveryFactor));
					selectedObject.setSetting("RecoveryRange", float.Parse(stRecoveryRange));
					selectedObject.setSetting("LaunchRefund", float.Parse(stLaunchRefund));
					selectedObject.setSetting("OpenCloseState", "Open");
					selectedObject.setSetting("Category", siteCategory);
					if (siteAuthor != (string)selectedObject.model.getSetting("author"))
						selectedObject.setSetting("LaunchSiteAuthor", siteAuthor);
					
					if(addToDB)
					{
						LaunchSiteManager.createLaunchSite(selectedObject);
					}
					KerbalKonstructs.instance.saveObjects();
					
					List<LaunchSite> basesites = LaunchSiteManager.getLaunchSites();
					PersistenceFile<LaunchSite>.SaveList(basesites, "LAUNCHSITES", "KK");
					editingSite = false;
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Cancel", GUILayout.Width(115)))
				{
					editingSite = false;
				}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
		#endregion

		#endregion

		#region Career Persistence

		#endregion

		#region Utility Functions

		public StaticObject spawnInstance(StaticModel model, float fOffset, Vector3 vPosition, float fAngle)
		{
			StaticObject obj = new StaticObject();
			obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));
			obj.setSetting("RadiusOffset", fOffset);
			obj.setSetting("CelestialBody", KerbalKonstructs.instance.getCurrentBody());
			obj.setSetting("Group", "Ungrouped");
			obj.setSetting("RadialPosition", vPosition);
			obj.setSetting("RotationAngle", fAngle);
			obj.setSetting("Orientation", Vector3.up);
			obj.setSetting("VisibilityRange", 25000f);

			string sPad = ((string)model.getSetting("DefaultLaunchPadTransform"));
			if (sPad != null) obj.setSetting("LaunchPadTransform", sPad);

			if (!KerbalKonstructs.instance.DevMode)
			{
				obj.setSetting("CustomInstance", "True");
			}

			obj.model = model;

			KerbalKonstructs.instance.getStaticDB().addStatic(obj);
			enableColliders = false;
			obj.spawnObject(true, false);
			return obj;
		}

		public static void setTargetSite(LaunchSite lsTarget, string sName = "")
		{
			lTargetSite = lsTarget;
		}

		public static void updateSelection(StaticObject obj)
		{
			selectedObject = obj;
			xPos = ((Vector3)obj.getSetting("RadialPosition")).x.ToString();
			yPos = ((Vector3)obj.getSetting("RadialPosition")).y.ToString();
			zPos = ((Vector3)obj.getSetting("RadialPosition")).z.ToString();
			xOri = ((Vector3)obj.getSetting("Orientation")).x.ToString();
			yOri = ((Vector3)obj.getSetting("Orientation")).y.ToString();
			zOri = ((Vector3)obj.getSetting("Orientation")).z.ToString();
			altitude = ((float)obj.getSetting("RadiusOffset")).ToString();
			rotation = ((float)obj.getSetting("RotationAngle")).ToString();
			visrange = ((float)obj.getSetting("VisibilityRange")).ToString();
			facType = ((string)obj.getSetting("FacilityType"));
			sGroup = ((string)obj.getSetting("Group"));
			selectedObject.update();
		}

		public float getIncrement()
		{
			return float.Parse(increment);
		}

		public void setIncrement(bool increase, float amount)
		{
			if (increase)
				increment = (float.Parse(increment) + amount).ToString();
			else
				increment = (float.Parse(increment) - amount).ToString();

			if (float.Parse(increment) <= 0) increment = "0.1";
		}

		public SiteType getSiteType(int selection)
		{
			switch(selection)
			{
				case 0:
					return SiteType.VAB;
				case 1:
					return SiteType.SPH;
				default:
					return SiteType.Any;
			}
		}

		void FixDrift(bool bRotate = false)
		{
			if (selectedSnapPoint == null || selectedSnapPoint2 == null) return;
			if (selectedObject == null || snapTargetInstance == null) return;

			Vector3 snapSourceLocalPos = selectedSnapPoint.transform.localPosition;
			Vector3 snapSourceWorldPos = selectedSnapPoint.transform.position;
			Vector3 selSourceWorldPos = selectedObject.gameObject.transform.position;
			float selSourceRot = selectedObject.pqsCity.reorientFinalAngle;
			Vector3 snapTargetLocalPos = selectedSnapPoint2.transform.localPosition;
			Vector3 snapTargetWorldPos = selectedSnapPoint2.transform.position;
			Vector3 selTargetWorldPos = snapTargetInstance.gameObject.transform.position;
			float selTargetRot = snapTargetInstance.pqsCity.reorientFinalAngle;
			var spdist = 0f;

			if (!bRotate) spdist = Vector3.Distance(snapSourceWorldPos, snapTargetWorldPos);
			if (bRotate) spdist = Vector3.Distance(selSourceRot * snapSourceWorldPos, selTargetRot * snapTargetWorldPos);

			int iGiveUp = 0;

			while (spdist > 0.01 && iGiveUp < 100)
			{
				if (!bRotate)
				{
					snpspos = selectedSnapPoint.transform.position;
					snptpos = selectedSnapPoint2.transform.position;

					vDrift = snpspos - snptpos;
					vCurrpos = selectedObject.pqsCity.repositionRadial;
					selectedObject.setSetting("RadialPosition", vCurrpos + vDrift);
					updateSelection(selectedObject);

					spdist = Vector3.Distance(selectedSnapPoint.transform.position, selectedSnapPoint2.transform.position);
					iGiveUp = iGiveUp + 1;
				}

				if (bRotate)
				{
					iGiveUp = 100;
				}
			}
		}

		Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
		{
			Vector3 dir = point - pivot; // get point direction relative to pivot
			dir = Quaternion.Euler(angles) * dir; // rotate it
			Vector3 newpoint = dir + pivot; // calculate rotated point
			return newpoint;
		}

		void SnapToTarget(bool bRotate = false)
		{
			if (selectedSnapPoint == null || selectedSnapPoint2 == null) return;
			if (selectedObject == null || snapTargetInstance == null) return;

			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			Vector3 snapPointRelation = new Vector3(0, 0, 0);
			Vector3 snapPoint2Relation = new Vector3(0, 0, 0);
			Vector3 snapVector = new Vector3(0, 0, 0);
			Vector3 snapVectorNoRot = new Vector3(0, 0, 0);
			Vector3 vFinalPos = new Vector3(0, 0, 0);

			Vector3 snapSourcePos = selectedSnapPoint.transform.localPosition;
			snapSourceWorldPos = selectedSnapPoint.transform.position;
			Vector3 selSourcePos = selectedObject.gameObject.transform.position;
			float selSourceRot = selectedObject.pqsCity.reorientFinalAngle;
			Vector3 snapTargetPos = selectedSnapPoint2.transform.localPosition;
			snapTargetWorldPos = selectedSnapPoint2.transform.position;
			Vector3 selTargetPos = snapTargetInstance.gameObject.transform.position;
			float selTargetRot = snapTargetInstance.pqsCity.reorientFinalAngle;

			vbsnapangle1 = selectedSnapPoint.transform.position;
			vbsnapangle2 = selectedSnapPoint2.transform.position;

			if (!bRotate)
			{
				// Quaternion quatSelObj = Quaternion.AngleAxis(selSourceRot, selSourcePos);
				snapPointRelation = snapSourcePos;
				//quatSelObj * snapSourcePos;

				//Quaternion quatSelTar = Quaternion.AngleAxis(selTargetRot, selTargetPos);
				snapPoint2Relation = snapTargetPos;
				//quatSelTar * snapTargetPos;

				snapVector = (snapPoint2Relation - snapPointRelation);
				vFinalPos = (Vector3)snapTargetInstance.getSetting("RadialPosition") + snapVector;
			}
			else
			{
				// THIS SHIT DO NOT WORK
				//ScreenMessages.PostScreenMessage("Snapping with rotation.", 60, smsStyle);
				// Stick the origins on each other
				vFinalPos = (Vector3)snapTargetInstance.getSetting("RadialPosition");
				selectedObject.setSetting("RadialPosition", vFinalPos);
				updateSelection(selectedObject);

				// Get the offset of the source and move by that
				Vector3 vAngles = new Vector3(0, selectedObject.pqsCity.reorientFinalAngle, 0);
				snapPointRelation = selectedObject.gameObject.transform.position -
					selectedSnapPoint.transform.TransformPoint(selectedSnapPoint.transform.localPosition);
				ScreenMessages.PostScreenMessage("" + snapPointRelation.ToString(), 60, smsStyle);
				vFinalPos = snapTargetInstance.pqsCity.repositionRadial + snapPointRelation;
				selectedObject.setSetting("RadialPosition", vFinalPos);
				updateSelection(selectedObject);

				// Get the offset of the target and move by that
				vAngles = new Vector3(0, snapTargetInstance.pqsCity.reorientFinalAngle, 0);
				snapPoint2Relation = snapTargetInstance.gameObject.transform.position -
					selectedSnapPoint2.transform.TransformPoint(selectedSnapPoint2.transform.localPosition);
				ScreenMessages.PostScreenMessage("" + snapPoint2Relation.ToString(), 60, smsStyle);
				vFinalPos = snapTargetInstance.pqsCity.repositionRadial + snapPoint2Relation;
			}

			snapSourcePos = selectedSnapPoint.transform.localPosition;
			snapTargetPos = selectedSnapPoint2.transform.localPosition;
			snapVectorNoRot = (snapSourcePos - snapTargetPos);

			selectedObject.setSetting("RadialPosition", vFinalPos);
			selectedObject.setSetting("RadiusOffset", (float)snapTargetInstance.getSetting("RadiusOffset") + snapVectorNoRot.y);

			updateSelection(selectedObject);
			if (!bRotate) FixDrift();
		}

		#endregion
	}
}

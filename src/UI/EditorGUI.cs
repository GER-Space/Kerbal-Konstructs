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
			public Texture tBilleted = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/billeted", false);
			public Texture tCopyPos = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/copypos", false);
			public Texture tPastePos = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/pastepos", false);
			public Texture tIconClosed = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteclosed", false);
			public Texture tIconOpen = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);
			public Texture tSearch = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/search", false);
			public Texture tCancelSearch = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/cancelsearch", false);
			public Texture tVAB = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/VABMapIcon", false);
			public Texture tSPH = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SPHMapIcon", false);
			public Texture tANY = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/ANYMapIcon", false);
			public Texture tFocus = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/focuson", false);
			public Texture tSnap = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/snapto", false);

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
			Rect editorRect = new Rect(10, 25, 520, 520);
			Rect siteEditorRect = new Rect(400, 45, 340, 480);
			Rect StaticInfoRect = new Rect(300, 50, 250, 400);
			#endregion

			#region GUI elements
			// GUI elements
			Vector2 scrollPos;
			Vector2 descScroll;
			GUIStyle listStyle = new GUIStyle();
			GUIStyle navStyle = new GUIStyle();
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
			String customgroup = "";
			String categoryfilter = "";
			String titlefilter = "";
			String categoryfilterset = "";
			String titlefilterset = "";
			String sTitleHolder = "";
			String sCategoryHolder = "";
			String groupfilter = "";
			String groupfilterset = "";
			public static String visrange = "";
			String increment = "1";
			String siteName, siteTrans, siteDesc, siteAuthor, siteCategory;
			float flOpenCost, flCloseValue, flRecoveryFactor, flRecoveryRange, flLaunchRefund;
			float localRange = 10000f;

			Vector3 orientation = Vector3.zero;
			Vector3 vbsnapangle1 = new Vector3(0,0,0);
			Vector3 vbsnapangle2 = new Vector3(0, 0, 0);

			Vector3 snapSourceWorldPos = new Vector3(0, 0, 0);
			Vector3 snapTargetWorldPos = new Vector3(0, 0, 0);

			String infTitle = "";
			String infMesh = "";
			String infCost = "";
			String infAuthor = "";
			String infManufacturer = "";
			String infDescription = "";
			String infCategory = "";
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

		public void drawStaticInfo()
		{
			StaticInfoRect = GUI.Window(0xD00B1E2, StaticInfoRect, drawStaticInfoWindow, "KK Static Model Info");
		}

		public void drawEditor(StaticObject obj)
		{
			if (obj != null)
			{
				if (selectedObject != obj)
					updateSelection(obj);

				toolRect = GUI.Window(0xB00B1E3, toolRect, drawToolWindow, "KK Instance Editor");

				if (editingSite)
				{
						siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawSiteEditorWindow, "KK Launchsite Editor");
				}
			}

			if (displayingInfo)
			{
				StaticInfoRect = GUI.Window(0xD00B1E2, StaticInfoRect, drawStaticInfoWindow, "KK Static Model Info");
			}

			editorRect = GUI.Window(0xB00B1E5, editorRect, drawEditorWindow, "Kerbal Konstructs Statics Editor");
		}

		void drawStaticInfoWindow(int WindowID)
		{
			GUILayout.Box(" " + infTitle + " ");
			GUILayout.Box("Mesh: " + infMesh + ".mu");
			GUILayout.Box("Manufacturer: " + infManufacturer);
			GUILayout.Box("Author: " + infAuthor);
			GUILayout.Box("Category: " + infCategory);
			GUILayout.Box("Cost: " + infCost);
			GUILayout.Label("Description: ");
			GUILayout.Label(infDescription);

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Close", GUILayout.Height(23)))
			{
				displayingInfo = false;
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		#endregion
		
		#region Editors

		#region Statics Editor
		// STATICS EDITOR

		void drawEditorWindow(int id)
		{
			string smessage = "";
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)2;

			GUILayout.BeginArea(new Rect(10, 25, 500, 485));
			
			GUILayout.BeginHorizontal();
				GUI.enabled = !creatingInstance;
				if (GUILayout.Button("Spawn New", GUILayout.Width(115)))
				{
					creatingInstance = true;
					showLocal = false;
				}
				GUILayout.Space(10);
				GUI.enabled = creatingInstance || showLocal;
				if (GUILayout.Button("All Instances", GUILayout.Width(108)))
				{
					creatingInstance = false;
					showLocal = false;
				}
				GUI.enabled = true;
				GUILayout.Space(2);
				GUI.enabled = creatingInstance || !showLocal;
				if (GUILayout.Button("Local Instances", GUILayout.Width(108)))
				{
					creatingInstance = false;
					showLocal = true;
				}
				GUI.enabled = true;
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("Save", "Save all new and edited instances."), GUILayout.Width(115)))
				{
					KerbalKonstructs.instance.saveObjects();
					smessage = "Saved all changes to all objects.";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
			GUILayout.EndHorizontal();

			if (creatingInstance)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Category", GUILayout.Width(125));
					GUILayout.FlexibleSpace();
					GUILayout.Label("Title ");
					GUILayout.FlexibleSpace();
					GUILayout.Label("Mesh ", GUILayout.Width(175));
				GUILayout.EndHorizontal();
			}

			bool showStatic = false;
			scrollPos = GUILayout.BeginScrollView(scrollPos);
				if (creatingInstance)
				{
					foreach (StaticModel model in KerbalKonstructs.instance.getStaticDB().getModels())
					{
						if (titlefilterset == "" && categoryfilterset == "")
							showStatic = true;

						if (titlefilterset != "")
						{
							sTitleHolder = (string)model.getSetting("title"); 
							if (sTitleHolder.Contains(titlefilterset))
								showStatic = true;
							else
								showStatic = false;
						}

						if (categoryfilterset != "")
						{
							sCategoryHolder = (string)model.getSetting("category");
							if (sCategoryHolder.Contains(categoryfilterset))
								showStatic = true;
							else
								showStatic = false;
						}

						if (categoryfilterset != "" && titlefilterset != "")
						{
							sTitleHolder = (string)model.getSetting("title");
							sCategoryHolder = (string)model.getSetting("category");
							if (sCategoryHolder.Contains(categoryfilterset) && sTitleHolder.Contains(titlefilterset))
								showStatic = true;
							else
								showStatic = false;
						}

						if (showStatic)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Box("" + model.getSetting("category"), GUILayout.Width(100), GUILayout.Height(28));
							if (GUILayout.Button(new GUIContent("" + "" + model.getSetting("title"), "Spawn an instance of this static."), GUILayout.Height(25)))
							{
								spawnInstance(model);
								smessage = "Spawned " + model.getSetting("title");
								ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
							}
							GUILayout.FlexibleSpace();
							GUILayout.Box(" " + model.getSetting("mesh") + " ", GUILayout.Height(28));
							if (GUILayout.Button("i", GUILayout.Width(20), GUILayout.Height(25)))
							{
								displayingInfo = true;
								infAuthor = (string)model.getSetting("author");
								infMesh = "" + model.getSetting("mesh");
								infManufacturer = (string)model.getSetting("manufacturer");
								infCost = model.getSetting("cost").ToString();
								infDescription = (string)model.getSetting("description");
								infTitle = (string)model.getSetting("title");
								infCategory = (string)model.getSetting("category");
								//displayingInfo = true;
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(2);
						}
					}
				}

				if (!creatingInstance)
				{
					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						bool isLocal = true;

						if (showLocal)
						{
							if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
							{
								var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
								isLocal = dist < localRange;
							}
							else
								isLocal = false;
						}

						string sGroupHolder = "";
						if (!showLocal)
						{
							if (groupfilterset != "")
							{
								sGroupHolder = (string)obj.getSetting("Group");
								if (!sGroupHolder.Contains(groupfilterset))
								{
									isLocal = false;
								}
							}
						}

						string sLaunchType = "";

						if (isLocal)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Box("" + obj.getSetting("Group"), GUILayout.Width(120), GUILayout.Height(28));

							sLaunchType = (string)obj.getSetting("Category");

							if (sLaunchType == "Runway" || sLaunchType == "Helipad")
							{
								GUILayout.Box(tSPH, GUILayout.Width(25), GUILayout.Height(28));
							}
							else
							{
								if (sLaunchType == "RocketPad")
								{
									GUILayout.Box(tVAB, GUILayout.Width(25), GUILayout.Height(28));
								}
								else
								{
									if (sLaunchType == "Other" && obj.settings.ContainsKey("LaunchSiteName"))
									{
										GUILayout.Box(tANY, GUILayout.Width(25), GUILayout.Height(28));
									}
									else
									{
										GUILayout.Box("", GUILayout.Width(25), GUILayout.Height(28));
									}
								}
							}

							//GUI.enabled = (obj != selectedObject);
							if (GUILayout.Button(new GUIContent("" + obj.model.getSetting("title"), "Edit this instance."), GUILayout.Height(25)))
							{
								enableColliders = true;

								if (selectedObject != null)
								{
									selectedObjectPrevious = selectedObject;
									Color highlightColor = new Color(0, 0, 0, 0);
									obj.HighlightObject(highlightColor);
								}

								if (snapTargetInstance == obj)
								{
									snapTargetInstance = null;
									KerbalKonstructs.instance.snapTargetInstance = null;
								}

								KerbalKonstructs.instance.selectObject(obj, false);
								//obj.selectObject(false);

								Color highlightColor2 = XKCDColors.Green_Yellow;
								obj.HighlightObject(highlightColor2);
							}
							//GUI.enabled = true;

							if (showLocal)
							{
								GUI.enabled = (snapTargetInstance != obj && obj != selectedObject);
								if (GUILayout.Button(new GUIContent(tFocus,"Set as snap target." ),GUILayout.Height(25), GUILayout.Width(25)))
								{
									if (snapTargetInstance != null)
									{
										snapTargetInstancePrevious = snapTargetInstance;
										Color highlightColor3 = new Color(0, 0, 0, 0);
										snapTargetInstance.HighlightObject(highlightColor3);
									}

									snapTargetInstance = obj;
									KerbalKonstructs.instance.setSnapTarget(obj);

									Color highlightColor4 = XKCDColors.RedPink;
									obj.HighlightObject(highlightColor4);
								}
								GUI.enabled = true;
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(2);
						}
					}
				}
			GUILayout.EndScrollView();
			GUI.enabled = true;

			if (creatingInstance)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Filter by ");
					GUILayout.Label(" Category:");
					categoryfilter = GUILayout.TextField(categoryfilter, 30, GUILayout.Width(90));
					if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
					{
						categoryfilterset = categoryfilter;
						titlefilterset = titlefilter;
					}
					if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
					{
						categoryfilter = "";
						categoryfilterset = "";
					}
					GUILayout.Label("  Title:");
					titlefilter = GUILayout.TextField(titlefilter, 30, GUILayout.Width(90));
					if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
					{
						categoryfilterset = categoryfilter;
						titlefilterset = titlefilter;
					}
					if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
					{
						titlefilter = "";
						titlefilterset = "";
					}
				GUILayout.EndHorizontal();
			}
	
			if (!showLocal && !creatingInstance)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Filter by Group:");
					groupfilter = GUILayout.TextField(groupfilter, 40, GUILayout.Width(120));
					if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
					{
						groupfilterset = groupfilter;
					}
					if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
					{
						groupfilter = "";
						groupfilterset = "";
					}
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Export Custom"))
					{
							KerbalKonstructs.instance.exportCustomInstances();
							smessage = "Exported custom instances to GameData/medsouz/KerbalKonstructs/ExportedInstances";
							ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
					}
					GUI.enabled = false;
					if (GUILayout.Button("Import Custom"))
					{
					}
					GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			if (showLocal)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Local:");
					GUI.enabled = false;
					GUILayout.Label(localRange.ToString("0") + " m", GUILayout.Width(50));
					GUI.enabled = showLocal;
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						if (localRange < 5000)
						{

						}
						else
							localRange = localRange / 2;
					}
					if (GUILayout.Button("+", GUILayout.Width(25)))
					{
						if (localRange > 79999)
						{

						}
						else
							localRange = localRange * 2;
					}
					GUI.enabled = true;
					GUILayout.FlexibleSpace();
					GUILayout.Label("Group:");
					// GUILayout.Space(5);
					GUI.enabled = showLocal;
					customgroup = GUILayout.TextField(customgroup, 25, GUILayout.Width(125));
					GUI.enabled = true;
					//GUILayout.Space(5);
					GUI.enabled = showLocal;
					if (GUILayout.Button("Set as Group", GUILayout.Width(100)))
					{
						setLocalsGroup(customgroup, localRange);
						smessage = "Set group as " + customgroup;
						ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
					}
					GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			GUILayout.EndArea();

			if (GUI.tooltip != "")
			{
				var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
				GUI.Box(new Rect(Event.current.mousePosition.x -(25+(labelSize.x/2)), Event.current.mousePosition.y -40, labelSize.x + 10, labelSize.y + 5), GUI.tooltip);
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		void setLocalsGroup(string sGroup, float fRange)
		{
			if (sGroup == "")
				return;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
				{
					var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
					if (dist < fRange)
					{
						KerbalKonstructs.instance.getStaticDB().changeGroup(obj, sGroup);
					}
				}
			}
		}
		#endregion

		#region Instance Editor
		string savedxpos = "";
		string savedypos = "";
		string savedzpos = "";
		string savedalt = "";
		string savedrot = "";
		bool savedpos = false;
		bool pospasted = false;

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

		// INSTANCE EDITOR
		void drawToolWindow(int windowID)
		{
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

			GUILayout.BeginArea(new Rect(5, 25, 295, 550));

			GUILayout.Box((string)selectedObject.model.getSetting("title"));

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
			GUI.enabled = true;
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
			GUI.enabled = true;
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
			GUI.enabled = true;
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

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Alt.");
				GUILayout.FlexibleSpace();
				altitude = GUILayout.TextField(altitude, 25, GUILayout.Width(80));
				GUI.enabled = true;
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

			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Facility Type: ");
				GUILayout.FlexibleSpace();
				facType = GUILayout.TextField(facType, 30, GUILayout.Width(185));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Group: ");
				GUILayout.FlexibleSpace();
				sGroup = GUILayout.TextField(sGroup, 30, GUILayout.Width(185));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			{
				enableColliders = GUILayout.Toggle(enableColliders, "Enable Colliders", GUILayout.Width(140));

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

				if (GUILayout.Button("Duplicate", GUILayout.Width(130)))
				{
					KerbalKonstructs.instance.saveObjects();
					StaticModel oModel = selectedObject.model;
					float fOffset = (float)selectedObject.getSetting("RadiusOffset");
					Vector3 vPosition = (Vector3)selectedObject.getSetting("RadialPosition");
					float fAngle = (float)selectedObject.getSetting("RotationAngle");
					smessage = "Spawned duplicate " + selectedObject.model.getSetting("title");
					//KerbalKonstructs.instance.selectedObject = null;
					KerbalKonstructs.instance.deselectObject();
					spawnInstance(oModel, fOffset, vPosition, fAngle);
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUI.enabled = !editingSite;

			string sLaunchPadTransform = (string)selectedObject.getSetting("LaunchPadTransform");
			string sDefaultPadTransform = (string)selectedObject.model.getSetting("DefaultLaunchPadTransform");
			string sLaunchsiteDesc = (string)selectedObject.getSetting("LaunchSiteDescription");
			string sModelDesc = (string)selectedObject.model.getSetting("description");

			if (sLaunchPadTransform == "" && sDefaultPadTransform == "")
				GUI.enabled = false;

			if (GUILayout.Button(((selectedObject.settings.ContainsKey("LaunchSiteName")) ? "Edit" : "Make") + " Launchsite"))
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
				if (GUILayout.Button("Save", GUILayout.Width(130)))
				{
					KerbalKonstructs.instance.saveObjects();
					smessage = "Saved all changes to all objects.";
					ScreenMessages.PostScreenMessage(smessage, 10, smsStyle);
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Deselect", GUILayout.Width(130)))
				{
					KerbalKonstructs.instance.saveObjects();
					//KerbalKonstructs.instance.selectedObject = null;
					KerbalKonstructs.instance.deselectObject();
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

			GUILayout.EndArea();

			if (GUI.tooltip != "")
			{
				var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
				GUI.Box(new Rect(Event.current.mousePosition.x - (25 + (labelSize.x / 2)), Event.current.mousePosition.y - 40, labelSize.x + 10, labelSize.y + 5), GUI.tooltip);
			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public StaticObject spawnInstance(StaticModel model)
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

			if (!KerbalKonstructs.instance.DevMode)
			{
				obj.setSetting("CustomInstance", "True");
			}
			
			obj.model = model;

			KerbalKonstructs.instance.getStaticDB().addStatic(obj);
			enableColliders = false;
			obj.spawnObject(true);
			return obj;
		}

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

			if (!KerbalKonstructs.instance.DevMode)
			{
				obj.setSetting("CustomInstance", "True");
			}

			obj.model = model;

			KerbalKonstructs.instance.getStaticDB().addStatic(obj);
			enableColliders = false;
			obj.spawnObject(true);
			return obj;
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
			GUILayout.Box((string)selectedObject.model.getSetting("title"));
			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Name: ", GUILayout.Width(120));
				siteName = GUILayout.TextField(siteName);
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

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}
		#endregion

		#endregion

		#region Career Persistence

		#endregion

		#region Utility Functions

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
		#endregion
	}
}

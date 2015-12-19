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
	class StaticsEditorGUI
	{

		Rect editorRect = new Rect(10, 25, 520, 520);

		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle DeadButton2;
		GUIStyle DeadButton3;
		GUIStyle KKWindow;
		GUIStyle BoxNoBorder;

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);
		public Texture tTick = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingstick", false);
		public Texture tCross = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingscross", false);
		public Texture tSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/search", false);
		public Texture tCancelSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cancelsearch", false);
		public Texture tVAB = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/VABMapIcon", false);
		public Texture tSPH = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/SPHMapIcon", false);
		public Texture tANY = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/ANYMapIcon", false);
		public Texture tFocus = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/focuson", false);
		public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
		public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
		public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);

		public Boolean creatingInstance = false;
		public Boolean showLocal = false;
		public Boolean enableColliders = false;

		public Boolean bDisableEditingSetting = false;

		public static StaticObject selectedObject = null;
		public StaticObject selectedObjectPrevious = null;
		public StaticObject snapTargetInstance = null;
		StaticObject snapTargetInstancePrevious = null;

		public float fButtonWidth = 0f;

		String customgroup = "";
		String categoryfilter = "";
		String titlefilter = "";
		String categoryfilterset = "";
		String titlefilterset = "";
		String sTitleHolder = "";
		String sCategoryHolder = "";
		String groupfilter = "";
		String groupfilterset = "";
		String sPackName = "";

		String sButtonText = "";

		float localRange = 10000f;

		Vector2 scrollPos;

		public Boolean foldedIn = false;
		public Boolean doneFold = false;
		public Boolean bSortCategory = false;
		public Boolean bSortTitle = false;

		public List<StaticModel> lStaticModels;

		public void drawEditor()
		{
			if (foldedIn)
			{
				if (!doneFold)
					editorRect = new Rect(editorRect.xMin, editorRect.yMin, editorRect.width -245, editorRect.height - 255);

				doneFold = true;
			}

			if (!foldedIn)
			{
				if (doneFold)
					editorRect = new Rect(editorRect.xMin, editorRect.yMin, editorRect.width +245, editorRect.height + 255);

				doneFold = false;
			}

			KKWindow = new GUIStyle(GUI.skin.window);
			KKWindow.padding = new RectOffset(8, 8, 3, 3);

			editorRect = GUI.Window(0xB00B1E5, editorRect, drawEditorWindow, "", KKWindow);
		}

		void drawEditorWindow(int id)
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

			DeadButton2 = new GUIStyle(GUI.skin.button);
			DeadButton2.normal.background = null;
			DeadButton2.hover.background = null;
			DeadButton2.active.background = null;
			DeadButton2.focused.background = null;
			DeadButton2.normal.textColor = Color.white;
			DeadButton2.hover.textColor = Color.green;
			DeadButton2.active.textColor = Color.white;
			DeadButton2.focused.textColor = Color.white;
			DeadButton2.fontSize = 14;
			DeadButton2.fontStyle = FontStyle.Bold;

			DeadButton3 = new GUIStyle(GUI.skin.button);
			DeadButton3.normal.background = null;
			DeadButton3.hover.background = null;
			DeadButton3.active.background = null;
			DeadButton3.focused.background = null;
			DeadButton3.normal.textColor = Color.white;
			DeadButton3.hover.textColor = Color.white;
			DeadButton3.active.textColor = Color.white;
			DeadButton3.focused.textColor = Color.white;
			DeadButton3.fontSize = 13;
			DeadButton3.fontStyle = FontStyle.Bold;

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

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Statics Editor", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
				{
					KerbalKonstructs.instance.ToggleEditor();
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.Space(2);

			GUILayout.BeginHorizontal();
			{
				if (foldedIn) tFolded = tFoldOut;
				if (!foldedIn) tFolded = tFoldIn;

				if (GUILayout.Button(tFolded, GUILayout.Height(23), GUILayout.Width(23)))
				{
					if (foldedIn) foldedIn = false;
					else
						foldedIn = true;
				}

				GUI.enabled = !creatingInstance;

				sButtonText = "";
				fButtonWidth = 0f;

				if (foldedIn) fButtonWidth = 50f;
				else fButtonWidth = 110f;

				if (foldedIn) sButtonText = "New";
				else sButtonText = "Spawn New";

				if (GUILayout.Button("" + sButtonText, GUILayout.Height(23), GUILayout.Width(fButtonWidth)))
				{
					EditorGUI.CloseEditors();
					creatingInstance = true;
					showLocal = false;
				}

				GUILayout.Space(5);

				GUI.enabled = creatingInstance || showLocal;

				if (foldedIn) sButtonText = "All";
				else sButtonText = "All Instances";

				if (GUILayout.Button("" + sButtonText, GUILayout.Width(fButtonWidth), GUILayout.Height(23)))
				{
					EditorGUI.CloseEditors();
					creatingInstance = false;
					showLocal = false;
					KerbalKonstructs.instance.DeletePreviewObject();
					KerbalKonstructs.instance.disableAllInstanceEditing = bDisableEditingSetting;
				}

				GUI.enabled = true;
				GUILayout.Space(2);
				GUI.enabled = creatingInstance || !showLocal;

				if (foldedIn) sButtonText = "Local";
				else sButtonText = "Local Instances";

				if (GUILayout.Button("" + sButtonText, GUILayout.Width(fButtonWidth), GUILayout.Height(23)))
				{
					EditorGUI.CloseEditors();
					creatingInstance = false;
					showLocal = true;
					KerbalKonstructs.instance.DeletePreviewObject();
				}

				GUI.enabled = true;

				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("Save", "Save all new and edited instances."), GUILayout.Width(fButtonWidth - 10), GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.saveObjects();
					smessage = "Saved all changes to all objects.";
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
			}
			GUILayout.EndHorizontal();

			if (!foldedIn)
			{
				if (creatingInstance)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(15);
					if (GUILayout.Button("Category", DeadButton, GUILayout.Width(110), GUILayout.Height(23)))
					{
						if (bSortCategory) bSortCategory = false;
						else bSortCategory = true;
					}

					GUILayout.Space(5);
					if (GUILayout.Button("Title", DeadButton, GUILayout.Height(23)))
					{
						if (bSortTitle) bSortTitle = false;
						else bSortTitle = true;
					}
					GUILayout.FlexibleSpace();
					GUILayout.Button("Mesh", DeadButton, GUILayout.Width(140), GUILayout.Height(23));
					GUILayout.Space(15);
					GUILayout.EndHorizontal();
				}
			}

			bool showStatic = false;
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			if (creatingInstance)
			{
				lStaticModels = KerbalKonstructs.instance.getStaticDB().getModels();

				if (bSortCategory)
				{
					lStaticModels.Sort(delegate(StaticModel a, StaticModel b)
					{
						return ((string)a.getSetting("category")).CompareTo((string)b.getSetting("category"));
					});
				}

				if (bSortTitle)
				{
					lStaticModels.Sort(delegate(StaticModel a, StaticModel b)
					{
						return ((string)a.getSetting("title")).CompareTo((string)b.getSetting("title"));
					});
				}

				foreach (StaticModel model in lStaticModels)
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

						if (!foldedIn)
						{
							if (GUILayout.Button(new GUIContent("" + model.getSetting("category"), "Filter"), DeadButton, GUILayout.Width(110), GUILayout.Height(23)))
							{
								categoryfilter = (string)model.getSetting("category");
								categoryfilterset = categoryfilter;
								titlefilterset = titlefilter;
							}
							//GUILayout.FlexibleSpace();
							GUILayout.Space(5);
						}

						if (GUILayout.Button(new GUIContent("" + "" + model.getSetting("title"), "Spawn an instance of this static."), DeadButton2, GUILayout.Height(23)))
						{
							EditorGUI.CloseEditors();
							KerbalKonstructs.instance.DeletePreviewObject();
							KerbalKonstructs.instance.bDisablePositionEditing = false;
							spawnInstance(model);
							smessage = "Spawned " + model.getSetting("title");
							MiscUtils.HUDMessage(smessage, 10, 2);
						}

						if (!foldedIn)
						{
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(new GUIContent(" " + model.getSetting("mesh") + " ", "Edit Model Config"), DeadButton, GUILayout.Width(140), GUILayout.Height(23)))
							{
								KerbalKonstructs.instance.selectedModel = model;
								KerbalKonstructs.instance.showModelInfo = true;
							}
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
						if (!foldedIn)
						{
							GUILayout.Button("" + obj.getSetting("Group"), DeadButton3, GUILayout.Width(120), GUILayout.Height(23));

							sLaunchType = (string)obj.getSetting("Category");

							if (sLaunchType == "Runway" || sLaunchType == "Helipad")
							{
								GUILayout.Button(tSPH, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
							}
							else
							{
								if (sLaunchType == "RocketPad")
								{
									GUILayout.Button(tVAB, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
								}
								else
								{
									if (sLaunchType == "Other" && obj.settings.ContainsKey("LaunchSiteName"))
									{
										GUILayout.Button(tANY, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
									}
									else
									{
										GUILayout.Button("", DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
									}
								}
							}
						}

						//GUI.enabled = (obj != selectedObject);
						if (GUILayout.Button(new GUIContent("" + obj.model.getSetting("title"), "Edit this instance."), GUILayout.Height(23)))
						{
							KerbalKonstructs.instance.bDisablePositionEditing = false;
							enableColliders = true;
							EditorGUI.CloseEditors();

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

							if (!KerbalKonstructs.instance.disableAllInstanceEditing)
								KerbalKonstructs.instance.selectObject(obj, false, true, false);
							else
							{
								if (!showLocal)
								{
									KerbalKonstructs.instance.bDisablePositionEditing = true;
									KerbalKonstructs.instance.selectObject(obj, false, false, false);
								}
								else
								{
									KerbalKonstructs.instance.selectObject(obj, false, true, false);
								}
							}
							//obj.selectObject(false);

							Color highlightColor2 = XKCDColors.Green_Yellow;
							obj.HighlightObject(highlightColor2);
						}
						//GUI.enabled = true;

						if (showLocal)
						{
							GUI.enabled = (snapTargetInstance != obj && obj != selectedObject);
							if (GUILayout.Button(new GUIContent(tFocus, "Set as snap target."), GUILayout.Height(23), GUILayout.Width(23)))
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

			if (!foldedIn)
			{
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
			}

			if (!foldedIn)
			{
				if (!showLocal && !creatingInstance)
				{
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("Filter by Group:", GUILayout.Width(140));
						//GUILayout.FlexibleSpace();
						groupfilter = GUILayout.TextField(groupfilter, 40, GUILayout.Width(140));
						if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
						{
							groupfilterset = groupfilter;
						}
						if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
						{
							groupfilter = "";
							groupfilterset = "";
						}
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("Pack Name: ", GUILayout.Width(140));
						//GUILayout.FlexibleSpace();
						sPackName = GUILayout.TextField(sPackName, 30, GUILayout.Width(140));
						//GUILayout.FlexibleSpace();

						GUI.enabled = (sPackName != "" && groupfilter != "");
						if (GUILayout.Button("Export Group"))
						{
							//Validate the groupfilter to see if it is a Group name
							bool bValidGroupName = false;

							foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
							{
								if ((string)obj.getSetting("Group") == groupfilter)
								{
									bValidGroupName = true;
									break;
								}
							}

							if (bValidGroupName)
							{
								KerbalKonstructs.instance.exportCustomInstances(sPackName, "", groupfilter);
								smessage = "Exported custom instances to GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + groupfilter;
								MiscUtils.HUDMessage(smessage, 10, 2);
							}
							else
							{
								smessage = "Group filter is not a valid Group name. Please filter with a complete and valid Group name before exporting a group.";
								MiscUtils.HUDMessage(smessage, 20, 2);
							}
						}
						GUI.enabled = true;

						GUI.enabled = (sPackName != "");
						if (GUILayout.Button("Export All"))
						{
							KerbalKonstructs.instance.exportCustomInstances(sPackName, "All");
							smessage = "Exported all custom instances to GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/";
							MiscUtils.HUDMessage(smessage, 10, 2);
						}
						GUI.enabled = true;
					}
					GUILayout.EndHorizontal();

					if (!KerbalKonstructs.instance.disableAllInstanceEditing)
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Disable Camera Focus/Position Editing", GUILayout.Height(23)))
						{
							KerbalKonstructs.instance.disableAllInstanceEditing = true;
							bDisableEditingSetting = true;
						}

						GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
						GUILayout.EndHorizontal();
					}
					else
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Disable Camera Focus/Position Editing", GUILayout.Height(23)))
						{
							KerbalKonstructs.instance.disableAllInstanceEditing = false;
							bDisableEditingSetting = false;
						}

						GUILayout.Button(tTick, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
						GUILayout.EndHorizontal();
					}
				}
			}

			if (showLocal)
			{
				GUILayout.BeginHorizontal();
				if (!foldedIn)
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
				if (!foldedIn)
					GUILayout.Label("Group:");
				else
					GUILayout.Label("Group");
				// GUILayout.Space(5);
				GUI.enabled = showLocal;
				if (!foldedIn)
					customgroup = GUILayout.TextField(customgroup, 25, GUILayout.Width(125));
				else
					customgroup = GUILayout.TextField(customgroup, 25, GUILayout.Width(45));

				GUI.enabled = true;

				GUI.enabled = showLocal;

				if (!foldedIn) sButtonText = "Set as Group";
				else sButtonText = "Set";
				if (!foldedIn) fButtonWidth = 100;
				else fButtonWidth = 35;

				if (GUILayout.Button("" + sButtonText, GUILayout.Width(fButtonWidth)))
				{
					setLocalsGroup(customgroup, localRange);
					smessage = "Set group as " + customgroup;
					MiscUtils.HUDMessage(smessage, 10, 2);
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();
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

	}
}

using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Upgradeables;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
    public class StaticsEditorGUI : KKWindow
    {

        private static StaticsEditorGUI _instance = null;

        public static StaticsEditorGUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StaticsEditorGUI();

                }
                return _instance;
            }
        }

        Rect editorRect = new Rect(10, 25, 680, 540);

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
        public Texture tFocus = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/focuson", false);
        public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);

        // List items
        private static bool creatingInstance = false;
        private static bool showLocal = true;
        private static bool showAll = false;
        private static bool editDecals = false;

        public Boolean enableColliders = false;

        public Boolean bDisableEditingSetting = false;

        public static StaticInstance selectedObject = null;
        public StaticInstance selectedObjectPrevious = null;
        public StaticInstance snapTargetInstance = null;
        StaticInstance snapTargetInstancePrevious = null;

        public float fButtonWidth = 0f;

        String customgroup = "";
        String categoryFilterString = "";
        String titlefilterString = "";
        String categoryfilter = "";
        String titleFilter = "";

        String groupFilterString = "";
        String groupFilter = "";


        float localRange = 10000f;

        Vector2 scrollPos;

        //public Boolean foldedIn = false;
        //public Boolean doneFold = false;
        public Boolean bSortCategory = false;
        public Boolean bSortTitle = false;
        private string smessage = "";


        LaunchSiteCategory launchsiteCategory;

        // models change only once
        private static StaticModel[] allStaticModels;
        //static need only be loaded once per three seconnds
        private static float lastloaded = 0f;
        internal static StaticInstance[] allStaticInstances;

        private static bool isInitialized = false;

        private bool showStatic = false;

        public void ToggleEditor()
        {
            if (KerbalKonstructs.instance.selectedObject != null)
                KerbalKonstructs.instance.deselectObject(true, true);

            this.Toggle();

            if (snapTargetInstance != null)
            {
                Color highlightColor = new Color(0, 0, 0, 0);
                snapTargetInstance.HighlightObject(highlightColor);
                snapTargetInstance = null;
            }
        }


        /// <summary>
        /// Basic GUI drawing function
        /// </summary>
        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }
            drawEditor();
        }

        public override void Open()
        {
            allStaticModels = StaticDatabase.allStaticModels.Where(model => model.isHidden == false).ToArray();
            base.Open();
        }

        public override void Close()
        {
            EditorGUI.instance.Close();
            KerbalKonstructs.instance.DeletePreviewObject();
            MapDecalEditor.Instance.Close();
            base.Close();
        }

        public void drawEditor()
        {

            KKWindow = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(8, 8, 3, 3)
            };

            editorRect = GUI.Window(0xB00B1E5, editorRect, drawEditorWindow, "", KKWindow);
        }


        private void InititializeLayout()
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

            isInitialized = true;
        }

        /// <summary>
        /// draw the editor window
        /// </summary>
        /// <param name="id"></param>
        private void drawEditorWindow(int id)
        {

            if (isInitialized == false)
            {
                InititializeLayout();
            }

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
                    ToggleEditor();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            {

                GUI.enabled = !creatingInstance;

                if (GUILayout.Button("" + "Spawn New", GUILayout.Height(23), GUILayout.Width(110)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    creatingInstance = true;
                    showAll = false;
                    showLocal = false;
                    editDecals = false;
                }

                GUILayout.Space(5);

                GUI.enabled = !showAll;

                if (GUILayout.Button("All Instances", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    creatingInstance = false;
                    showAll = true;
                    showLocal = false;
                    editDecals = false;
                    KerbalKonstructs.instance.DeletePreviewObject();
                }

                GUI.enabled = true;
                GUILayout.Space(2);
                GUI.enabled = !showLocal;

                if (GUILayout.Button("Local Instances", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    creatingInstance = false;
                    showLocal = true;
                    showAll = false;
                    editDecals = false;
                    KerbalKonstructs.instance.DeletePreviewObject();
                }

                GUI.enabled = true;
                GUILayout.Space(2);
                GUI.enabled = !editDecals;

                if (GUILayout.Button("Edit MapDecals", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    if (EditorGUI.instance.IsOpen())
                    {
                        EditorGUI.instance.Close();
                    }
                    editDecals = true;
                    showAll = false;
                    creatingInstance = false;
                    showLocal = false;
                    KerbalKonstructs.instance.DeletePreviewObject();
                }


                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Save", "Save all new and edited instances."), KerbalKonstructs.instance.hasDeletedInstances ? UIMain.ButtonTextYellow : UIMain.ButtonDefault , GUILayout.Width(110), GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.saveObjects();
                    smessage = "Saved all changes to all objects.";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
            }
            GUILayout.EndHorizontal();


            if (creatingInstance)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                if (GUILayout.Button("Category", DeadButton, GUILayout.Width(110), GUILayout.Height(23)))
                {
                    SortByCategory();
                }

                GUILayout.Space(5);
                if (GUILayout.Button("Title", DeadButton, GUILayout.Height(23)))
                {
                    SortByTitle();
                }
                GUILayout.FlexibleSpace();
                GUILayout.Button("Mesh", DeadButton, GUILayout.Width(140), GUILayout.Height(23));
                GUILayout.Space(15);
                GUILayout.EndHorizontal();
            }


            scrollPos = GUILayout.BeginScrollView(scrollPos);
            {
                // spawning new instances
                if (creatingInstance)
                {
                    ShowModelsScroll();


                }
                // edting existing instances
                if (showAll || showLocal)
                {
                    ShowInstancesScroll();
                }

                // PQS List
                if (editDecals)
                {
                    ShowDecalsScroll();
                }

            }
            GUILayout.EndScrollView();

            #region  Buttons below scroll view

            GUI.enabled = true;

            if (creatingInstance)
            {
                ShowModelsFooter();
            }
            if (showAll)
            {
                ShowInstancesFootersAll();
            }


            if (showLocal)
            {
                ShowInstancesFootersLocal();
            }

            if (editDecals)
            {
                ShowDecalsFooter();
            }

            #endregion

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

        void SetLocalsGroup(string sGroup, float fRange)
        {
            if (sGroup == "")
                return;

            foreach (StaticInstance obj in StaticDatabase.GetAllStatics())
            {
                if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
                {
                    var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
                    if (dist < fRange)
                    {
                        StaticDatabase.ChangeGroup(obj, sGroup);
                    }
                }
            }
        }

        /// <summary>
        /// wrapper for editorGUI spawnInstance
        /// </summary>
        /// <param name="model"></param>
		internal void SpawnInstance(StaticModel model)
        {
            EditorGUI.instance.SpawnInstance(model,
                (float)FlightGlobals.ActiveVessel.altitude,
                KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position),
                0f);
            if (!EditorGUI.instance.IsOpen())
            {
                EditorGUI.instance.Open();
            }
        }


        internal void SortByCategory()
        {
            List<StaticModel> tmpList = StaticDatabase.allStaticModels;
            tmpList.Sort(delegate (StaticModel a, StaticModel b)
            {
                return (a.category).CompareTo(b.category);
            });
            allStaticModels = tmpList.ToArray();

        }

        internal void SortByTitle()
        {
            List<StaticModel> tmpList = StaticDatabase.allStaticModels;
            tmpList.Sort(delegate (StaticModel a, StaticModel b)
            {
                return (a.title).CompareTo(b.title);
            });
            allStaticModels = tmpList.ToArray();
        }


        internal void ShowModelsScroll()
        {

            foreach (StaticModel model in allStaticModels)
            {
                if (titleFilter == "" && categoryfilter == "")
                    showStatic = true;

                if (titleFilter != "")
                {
                    if (model.title.ToLower().Contains(titleFilter.ToLower()))
                        showStatic = true;
                    else
                        showStatic = false;
                }

                if (categoryfilter != "")
                {
                    if (model.category.ToLower().Contains(categoryfilter.ToLower()))
                        showStatic = true;
                    else
                        showStatic = false;
                }

                if (categoryfilter != "" && titleFilter != "")
                {
                    if ((model.category.ToLower().Contains(categoryfilter.ToLower())) && (model.title.ToLower().Contains(titleFilter.ToLower())))
                        showStatic = true;
                    else
                        showStatic = false;
                }

                if (showStatic)
                {
                    GUILayout.BeginHorizontal();


                    if (GUILayout.Button(new GUIContent(model.category, "Filter"), DeadButton, GUILayout.Width(110), GUILayout.Height(23)))
                    {
                        categoryfilter = model.category;
                        categoryFilterString = model.category;
                    }
                    //GUILayout.FlexibleSpace();
                    GUILayout.Space(5);


                    if (GUILayout.Button(new GUIContent(model.title, "Spawn an instance of this static."), DeadButton2, GUILayout.Height(23)))
                    {
                        EditorGUI.CloseEditors();
                        KerbalKonstructs.instance.DeletePreviewObject();
                        SpawnInstance(model);
                        smessage = "Spawned " + model.title;
                        MiscUtils.HUDMessage(smessage, 10, 2);
                    }


                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(" " + model.mesh + " ", "Edit Model Config"), DeadButton, GUILayout.Width(200), GUILayout.Height(23)))
                    {
                        KerbalKonstructs.instance.selectedModel = model;
                        ModelInfo.instance.Open();
                    }


                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }


        }

        internal void ShowModelsFooter()
        {

            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter by ");
            GUILayout.Label(" Category:");
            categoryFilterString = GUILayout.TextField(categoryFilterString, 30, GUILayout.Width(90));
            if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
            {
                categoryfilter = categoryFilterString;
            }
            if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
            {
                categoryFilterString = "";
                categoryfilter = "";
            }
            GUILayout.Label("  Title:");
            titlefilterString = GUILayout.TextField(titlefilterString, 30, GUILayout.Width(90));
            if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
            {
                titleFilter = titlefilterString;
            }
            if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
            {
                titlefilterString = "";
                titleFilter = "";
            }
            GUILayout.EndHorizontal();

        }


        internal void ShowInstancesScroll()
        {

            UpdateInstances();
            for (int ix = 0; ix < allStaticInstances.Length; ix++)
            //foreach (StaticObject obj in allStaticInstances)
            {
                bool isLocal = true;

                if (showLocal)
                {
                    if (allStaticInstances[ix].pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
                    {
                        var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, allStaticInstances[ix].gameObject.transform.position);
                        isLocal = dist < localRange;
                    }
                    else
                    {
                        isLocal = false;
                    }
                }

                string sGroupHolder = "";
                if (!showLocal)
                {
                    if (groupFilter != "")
                    {
                        sGroupHolder = allStaticInstances[ix].Group;
                        if (!sGroupHolder.Contains(groupFilter))
                        {
                            isLocal = false;
                        }
                    }
                }


                if (isLocal)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Button("" + allStaticInstances[ix].Group, DeadButton3, GUILayout.Width(120), GUILayout.Height(23));
                    if (allStaticInstances[ix].hasLauchSites)
                    {
                        launchsiteCategory = allStaticInstances[ix].launchSite.sitecategory;

                        switch (launchsiteCategory)
                        {
                            case LaunchSiteCategory.Runway:
                                GUILayout.Button(UIMain.runWayIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                break;
                            case LaunchSiteCategory.Helipad:
                                GUILayout.Button(UIMain.heliPadIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                break;
                            case LaunchSiteCategory.RocketPad:
                                GUILayout.Button(UIMain.VABIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                break;
                            case LaunchSiteCategory.Waterlaunch:
                                GUILayout.Button(UIMain.waterLaunchIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                break;
                            case LaunchSiteCategory.Other:
                                GUILayout.Button(UIMain.ANYIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                break;
                            default:
                                GUILayout.Button("", DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                break;
                        }
                    }
                    else
                    {
                        GUILayout.Button("", DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                    }

                    //GUI.enabled = (obj != selectedObject);
                    if (GUILayout.Button(new GUIContent("" + allStaticInstances[ix].model.title + " ( " + allStaticInstances[ix].indexInGroup.ToString() + " )", "Edit this instance."), GUILayout.Height(23)))
                    {
                        enableColliders = true;
                        EditorGUI.CloseEditors();
                        if (!EditorGUI.instance.IsOpen())
                        {
                            EditorGUI.instance.Open();
                        }


                        if (selectedObject != null)
                        {
                            selectedObjectPrevious = selectedObject;
                            Color highlightColor = new Color(0, 0, 0, 0);
                            allStaticInstances[ix].HighlightObject(highlightColor);
                        }

                        if (snapTargetInstance == allStaticInstances[ix])
                        {
                            snapTargetInstance = null;
                        }
                        KerbalKonstructs.instance.selectObject(allStaticInstances[ix], false, true, false);

                        //obj.selectObject(false);

                        Color highlightColor2 = XKCDColors.Green_Yellow;
                        allStaticInstances[ix].HighlightObject(highlightColor2);
                    }
                    //GUI.enabled = true;

                    if (showLocal)
                    {
                        GUI.enabled = (snapTargetInstance != allStaticInstances[ix] && allStaticInstances[ix] != selectedObject);
                        if (GUILayout.Button(new GUIContent(tFocus, "Set as snap target."), GUILayout.Height(23), GUILayout.Width(23)))
                        {
                            if (snapTargetInstance != null)
                            {
                                snapTargetInstancePrevious = snapTargetInstance;
                                Color highlightColor3 = new Color(0, 0, 0, 0);
                                snapTargetInstance.HighlightObject(highlightColor3);
                            }

                            snapTargetInstance = allStaticInstances[ix];

                            Color highlightColor4 = XKCDColors.RedPink;
                            allStaticInstances[ix].HighlightObject(highlightColor4);
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }

        }


        internal void ShowInstancesFootersAll()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Filter by Group:", GUILayout.Width(140));
                //GUILayout.FlexibleSpace();
                groupFilterString = GUILayout.TextField(groupFilterString, 40, GUILayout.Width(140));
                if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    groupFilter = groupFilterString;
                }
                if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    groupFilterString = "";
                    groupFilter = "";
                }
            }
            GUILayout.EndHorizontal();

            // GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Label("Pack Name: ", GUILayout.Width(140));
            //    //GUILayout.FlexibleSpace();
            //    sPackName = GUILayout.TextField(sPackName, 30, GUILayout.Width(140));
            //    //GUILayout.FlexibleSpace();

            //    GUI.enabled = (sPackName != "" && groupfilter != "");
            //    if (GUILayout.Button("Export Group"))
            //    {
            //        //Validate the groupfilter to see if it is a Group name
            //        bool bValidGroupName = false;

            //        foreach (StaticObject instance in StaticDatabase.allStaticInstances)
            //        {
            //            if (instance.Group == groupfilter)
            //            {
            //                bValidGroupName = true;
            //                break;
            //            }
            //        }

            //        if (bValidGroupName)
            //        {
            //            KerbalKonstructs.instance.exportCustomInstances(sPackName, "", groupfilter);
            //            smessage = "Exported custom instances to GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/" + groupfilter;
            //            MiscUtils.HUDMessage(smessage, 10, 2);
            //        }
            //        else
            //        {
            //            smessage = "Group filter is not a valid Group name. Please filter with a complete and valid Group name before exporting a group.";
            //            MiscUtils.HUDMessage(smessage, 20, 2);
            //        }
            //    }
            //    GUI.enabled = true;

            //    GUI.enabled = (sPackName != "");
            //    if (GUILayout.Button("Export All"))
            //    {
            //        KerbalKonstructs.instance.exportCustomInstances(sPackName, "All");
            //        smessage = "Exported all custom instances to GameData/KerbalKonstructs/ExportedInstances/" + sPackName + "/";
            //        MiscUtils.HUDMessage(smessage, 10, 2);
            //    }
            //    GUI.enabled = true;
            //}
            //GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disable Camera Focus/Position Editing", GUILayout.Height(23)))
            {

                bDisableEditingSetting = true;
            }

            GUILayout.Button(tCross, DeadButton, GUILayout.Height(23), GUILayout.Width(23));
            GUILayout.EndHorizontal();


        }


        internal void ShowInstancesFootersLocal()
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

            GUI.enabled = showLocal;




            if (GUILayout.Button("Set as Group", GUILayout.Width(100)))
            {
                SetLocalsGroup(customgroup, localRange);
                smessage = "Set group as " + customgroup;
                MiscUtils.HUDMessage(smessage, 10, 2);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();


        }

        internal void ShowDecalsScroll()
        {
            foreach (var mapDecalInstance in DecalsDatabase.allMapDecalInstances)
            {
                if (GUILayout.Button(new GUIContent(" " + mapDecalInstance.Name, "Edit this instance."), GUILayout.Height(23)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    MapDecalEditor.selectedDecal = mapDecalInstance;
                    MapDecalEditor.Instance.Open();
                }

            }
        }

        internal void ShowDecalsFooter()
        {
            GUILayout.BeginHorizontal();

            //GUILayout.Label("Filter by Group:", GUILayout.Width(140));
            //GUILayout.FlexibleSpace();
            //groupfilter = GUILayout.TextField(groupfilter, 40, GUILayout.Width(140));
            if (GUILayout.Button("Spawn new MapDecal", GUILayout.Width(170)))
            {
                EditorGUI.instance.Close();
                EditorGUI.selectedInstance = null;

                MapDecalEditor.selectedDecal = MapDecalUtils.SpawnNewDecalInstance();

                if (MapDecalEditor.selectedDecal == null)
                    Log.UserError("No MapDecalInstance created");

                MapDecalEditor.Instance.Open();

                Log.Normal("MapDecal Editor spawned");

            }
            GUILayout.EndHorizontal();


        }

        internal static void ResetInstancesList()
        {
            lastloaded = 0f;
        }


        private static void UpdateInstances()
        {
            if ((Time.time - lastloaded) > 2f)
            {
                lastloaded = Time.time;
                allStaticInstances = StaticDatabase.allStaticInstances;
            }
        }

        /// <summary>
        /// Selects Object under the mouse curser.
        /// </summary>
        internal void SelectMouseObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ;
            int layerMask = ~0;

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return;
            }

            StaticInstance myHitinstance = GetRootFromHit(hit.collider.gameObject);

            if (myHitinstance == null)
            {
                Log.Normal("No RootObject found");
                return;
            }
            else
            {
                Log.Normal("Try to select Object: " + myHitinstance.gameObject.name);
                myHitinstance.HighlightObject(XKCDColors.Green_Yellow);
                KerbalKonstructs.instance.selectObject(myHitinstance, true, true, false);
            }

        }

        /// <summary>
        /// tries to find a Static Object attached to a child GameObject.
        /// </summary>
        /// <param name="foundObject"></param>
        /// <returns></returns>
        private StaticInstance GetRootFromHit(GameObject foundObject)
        {
            for (int i = 0; i < 10; i++)
            {
                if (allStaticInstances.Where(x => x.gameObject == foundObject).FirstOrDefault() == null)
                {
                    if (foundObject.transform.parent != null)
                        foundObject = foundObject.transform.parent.gameObject;
                }
                else
                {
                    return allStaticInstances.Where(x => x.gameObject == foundObject).First();
                }

            }
            // we didn't find any root object, so we return null
            return null;
        }


    }
}

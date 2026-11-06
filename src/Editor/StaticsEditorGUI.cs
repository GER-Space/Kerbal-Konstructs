using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class StaticsEditorGUI : KKWindow
    {

        private enum EditorMode
        {
            SPAWN,
            LOCAL,
            PQS,
            GROUP
        }

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

        Rect editorRect = new Rect(10, 25, 690, 540);

        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle DeadButton2;
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
        private static EditorMode mode = EditorMode.LOCAL;


        public Boolean enableColliders = false;

        public Boolean bDisableEditingSetting = false;

        public static StaticInstance selectedObject = null;
        public StaticInstance selectedObjectPrevious = null;
        public StaticInstance snapTargetInstance = null;
        StaticInstance snapTargetInstancePrevious = null;

        private static GroupCenter activeGroup = null;
        private static GroupCenter[] localGroups;

        //internal static Color defaultGrasColor = new Color(0.640f, 0.728f, 0.171f, 0.729f);
        //internal static string defaultGrasTexture = "BUILTIN:/terrain_grass00_new";
        //private string grasColorRStr = "0.640";
        //private string grasColorGStr = "0.728";
        //private string grasColorBStr = "0.171";
        //private string grasColorAStr = "0.729";

        internal static GrassColorPresetUI2.ColorPreset2 defaultGrassPreset = new GrassColorPresetUI2.ColorPreset2();

        public float fButtonWidth = 0f;

        String categoryFilterString = "";
        String titlefilterString = "";
        String categoryfilter = "";
        String titleFilter = "";


        //float localRange = 10000f;

        Vector2 scrollPos;

        //public Boolean foldedIn = false;
        //public Boolean doneFold = false;
        public Boolean bSortCategory = false;
        public Boolean bSortTitle = false;
        private string smessage = "";


        // models change only once
        private static StaticModel[] allStaticModels;
        //static need only be loaded once per three seconnds
        private static float lastloaded = 0f;
        internal static StaticInstance[] allStaticInstances;

        private static bool isInitialized = false;

        private bool showStatic = false;

        public void ToggleEditor()
        {
            if (KerbalKonstructs.selectedInstance != null)
            {
                KerbalKonstructs.DeselectObject(true, true);
            }

            this.Toggle();

            if (snapTargetInstance != null)
            {
                snapTargetInstance.HighlightObject(Color.clear);
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
            DrawEditor();
        }

        public override void Open()
        {
            allStaticModels = StaticDatabase.allStaticModels.Where(model => model.isHidden == false).ToArray();
            ConfigUtil.CreateNewInstanceDirIfNeeded();
            ResetLocalGroupList();
            base.Open();
        }

        public override void Close()
        {
            EditorGUI.instance.Close();
            KerbalKonstructs.instance.DeletePreviewObject();
            MapDecalEditor.Instance.Close();
            base.Close();
        }

        public void DrawEditor()
        {

            KKWindow = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(8, 8, 3, 3)
            };

            editorRect = GUI.Window(0xB00B1E5, editorRect, DrawEditorWindow, "", KKWindow);
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
        private void DrawEditorWindow(int id)
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

                GUI.enabled = (mode != EditorMode.SPAWN);

                if (GUILayout.Button("" + "Spawn New", GUILayout.Height(23), GUILayout.Width(110)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    mode = EditorMode.SPAWN;
                }

                GUI.enabled = true;
                GUILayout.Space(2);
                GUI.enabled = (mode != EditorMode.LOCAL);

                if (GUILayout.Button("Local Instances", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    mode = EditorMode.LOCAL;
                    KerbalKonstructs.instance.DeletePreviewObject();
                }

                GUI.enabled = true;
                GUILayout.Space(2);
                GUI.enabled = (mode != EditorMode.PQS);

                if (GUILayout.Button("Edit MapDecals", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    if (EditorGUI.instance.IsOpen())
                    {
                        EditorGUI.instance.Close();
                    }
                    mode = EditorMode.PQS;
                    KerbalKonstructs.instance.DeletePreviewObject();
                }


                GUI.enabled = true;
                GUILayout.Space(2);
                GUI.enabled = (mode != EditorMode.GROUP);

                if (GUILayout.Button("Edit Groups", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    if (EditorGUI.instance.IsOpen())
                    {
                        EditorGUI.instance.Close();
                    }
                    MapDecalEditor.Instance.Close();

                    mode = EditorMode.GROUP;
                    KerbalKonstructs.instance.DeletePreviewObject();
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Export", "Export everyting into zip files"), UIMain.ButtonDefault, GUILayout.Width(90), GUILayout.Height(23)))
                {
                    ConfigParser.ExportAll();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Save", "Save all new and edited instances."), KerbalKonstructs.instance.hasDeletedInstances ? UIMain.ButtonTextOrange : UIMain.ButtonDefault, GUILayout.Width(90), GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.SaveObjects();
                    smessage = "Saved all changes to all objects.";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
            }
            GUILayout.EndHorizontal();


            if (mode == EditorMode.SPAWN)
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
                switch (mode)
                {
                    case EditorMode.SPAWN:
                        ShowModelsScroll();
                        break;
                    case EditorMode.LOCAL:
                        ShowInstancesScroll();
                        break;
                    case EditorMode.PQS:
                        ShowDecalsScroll();
                        break;
                    case EditorMode.GROUP:
                        ShowGroupScroll();
                        break;
                }

            }
            GUILayout.EndScrollView();

            #region  Buttons below scroll view

            GUI.enabled = true;


            switch (mode)
            {
                case EditorMode.SPAWN:
                    ShowModelsFooter();
                    break;
                case EditorMode.LOCAL:
                    ShowInstancesFootersLocal();
                    break;
                case EditorMode.PQS:
                    ShowDecalsFooter();
                    break;
                case EditorMode.GROUP:
                    ShowGroupFooter();
                    break;
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


        /// <summary>
        /// wrapper for editorGUI spawnInstance
        /// </summary>
        /// <param name="model"></param>
		internal void SpawnInstance(StaticModel model)
        {
            GroupCenter center = GetCloesedCenter(FlightGlobals.ActiveVessel.transform.position);
            EditorGUI.instance.SpawnInstance(model, center, FlightGlobals.ActiveVessel.transform.position, Vector3.zero);

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

        /// <summary>
        /// display all models
        /// </summary>
        internal void ShowModelsScroll()
        {

            foreach (StaticModel model in allStaticModels)
            {
                if (titleFilter == "" && categoryfilter == "")
                {
                    showStatic = true;
                }

                if (titleFilter != "")
                {
                    showStatic = model.title.ToLower().Contains(titleFilter.ToLower());
                }

                if (categoryfilter != "")
                {
                    showStatic = model.category.ToLower().Contains(categoryfilter.ToLower());
                }

                if (categoryfilter != "" && titleFilter != "")
                {
                    showStatic = (model.category.ToLower().Contains(categoryfilter.ToLower())) && (model.title.ToLower().Contains(titleFilter.ToLower()));
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

                    if (localGroups.Length > 0)
                    {
                        if (GUILayout.Button(new GUIContent(model.title, "Spawn an instance of this static."), DeadButton2, GUILayout.Height(23)))
                        {
                            EditorGUI.CloseEditors();
                            KerbalKonstructs.instance.DeletePreviewObject();
                            SpawnInstance(model);
                            smessage = "Spawned " + model.title;
                            MiscUtils.HUDMessage(smessage, 10, 2);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent(model.title, "first a Local Group Center"), DeadButton2, GUILayout.Height(23)))
                        {
                            Log.UserError("No Local Group found");
                            MiscUtils.HUDMessage("Create and place a local Group, then try again!");
                        }
                    }


                    GUILayout.FlexibleSpace();
                    if (localGroups.Length > 0)
                    {

                        if (GUILayout.Button(new GUIContent(" " + model.mesh + " ", "Edit Model Config"), DeadButton, GUILayout.Width(200), GUILayout.Height(23)))
                        {
                            KerbalKonstructs.instance.selectedModel = model;
                            ModelInfo.instance.Open();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent(" " + model.mesh + " ", "first a Local Group Center"), DeadButton, GUILayout.Width(200), GUILayout.Height(23)))
                        {
                            Log.UserError("No Local Group found");
                            MiscUtils.HUDMessage("Create and place a local Group, then try again!");
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }


        }

        /// <summary>
        /// footer of model view
        /// </summary>
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
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Default Grass Preset: " + defaultGrassPreset.name, GUILayout.Height(18));


                if (GUILayout.Button("Load Preset", GUILayout.Width(90), GUILayout.Height(18)))
                {
                    GrassColorPresetUI2.callBack = SetDefaultColorCallBack;
                    GrassColorPresetUI2.instance.Open();
                }
                //GUILayout.Label("R", GUILayout.Height(18));
                //grasColorRStr = (GUILayout.TextField(grasColorRStr, 5, GUILayout.Width(48), GUILayout.Height(18)));
                //GUILayout.Label("G", GUILayout.Height(18));
                //grasColorGStr = (GUILayout.TextField(grasColorGStr, 5, GUILayout.Width(48), GUILayout.Height(18)));
                //GUILayout.Label("B", GUILayout.Height(18));
                //grasColorBStr = (GUILayout.TextField(grasColorBStr, 5, GUILayout.Width(48), GUILayout.Height(18)));
                //GUILayout.Label("A", GUILayout.Height(18));
                //grasColorAStr = (GUILayout.TextField(grasColorAStr, 5, GUILayout.Width(48), GUILayout.Height(18)));

                //GUILayout.Space(5);
                //if (GUILayout.Button("Apply", GUILayout.Height(18)))
                //{
                //    defaultGrasColor.r = float.Parse(grasColorRStr);
                //    defaultGrasColor.g = float.Parse(grasColorGStr);
                //    defaultGrasColor.b = float.Parse(grasColorBStr);
                //    defaultGrasColor.a = float.Parse(grasColorAStr);
                //}
            }
            GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Label("Default Grass Texture: ", GUILayout.Height(18));
            //    GUILayout.Space(5);
            //    defaultGrasTexture = (GUILayout.TextField(defaultGrasTexture, 200, GUILayout.Width(300), GUILayout.Height(18)));

            //    GUILayout.EndHorizontal();
            //}
        }

        internal void SetDefaultColorCallBack(GrassColorPresetUI2.ColorPreset2 preset)
        {

            defaultGrassPreset = preset;

            //defaultGrasColor = preset.grassColor;
            //defaultGrasTexture = preset.nearGrassTexture;
            //grasColorRStr = defaultGrasColor.r.ToString();
            //grasColorGStr = defaultGrasColor.g.ToString();
            //grasColorBStr = defaultGrasColor.b.ToString();
            //grasColorAStr = defaultGrasColor.a.ToString();

        }


        /// <summary>
        /// instances
        /// </summary>
        internal void ShowInstancesScroll()
        {

            UpdateInstances();
            for (int ix = 0; ix < allStaticInstances.Length; ix++)
            //foreach (StaticObject obj in allStaticInstances)
            {
                bool isLocal = true;

                if (allStaticInstances[ix].CelestialBody == FlightGlobals.currentMainBody)
                {
                    var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, allStaticInstances[ix].position);
                    isLocal = (dist < KerbalKonstructs.localGroupRange);
                }
                else
                {
                    isLocal = false;
                }



                if (isLocal)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Button("" + allStaticInstances[ix].Group, UIMain.DeadButton3, GUILayout.Width(120), GUILayout.Height(23));
                    if (allStaticInstances[ix].hasLauchSites)
                    {
                        LaunchSiteSelectorGUI.ShowCategory(allStaticInstances[ix].launchSite);
                    }
                    else
                    {
                        GUILayout.Button("", UIMain.DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
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
                            allStaticInstances[ix].HighlightObject(Color.clear);
                        }

                        if (snapTargetInstance == allStaticInstances[ix])
                        {
                            snapTargetInstance = null;
                        }
                        KerbalKonstructs.SelectInstance(allStaticInstances[ix], false);

                        //obj.selectObject(false);

                        allStaticInstances[ix].HighlightObject(XKCDColors.Green_Yellow);
                    }
                    //GUI.enabled = true;


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

                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }

        }

        /// <summary>
        /// Footer for local instances
        /// </summary>
        internal void ShowInstancesFootersLocal()
        {
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }


        /// <summary>
        /// Show Mapdecals
        /// </summary>
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

        /// <summary>
        /// Footer for Mapdecals
        /// </summary>
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
                {
                    Log.UserError("No MapDecalInstance created");
                }

                MapDecalEditor.Instance.Open();

                Log.Normal("MapDecal Editor spawned");

            }
            GUILayout.EndHorizontal();


        }


        /// <summary>
        /// Show Groups
        /// </summary>
        internal void ShowGroupScroll()
        {
            foreach (var groupCenter in localGroups)
            {
                if (groupCenter.isBuiltIn)
                {
                    continue;
                }
                if (GUILayout.Button(new GUIContent(" " + groupCenter.Group, "Edit this Group."), GUILayout.Height(23)))
                {
                    EditorGUI.CloseEditors();
                    MapDecalEditor.Instance.Close();
                    GroupEditor.instance.Close();
                    GroupEditor.selectedGroup = groupCenter;
                    // MapDecalEditor.selectedDecal = mapDecalInstance;
                    GroupEditor.maxEditorRange = 0;
                    GroupEditor.instance.Open();
                }

            }
        }

        /// <summary>
        /// Footer for Groups
        /// </summary>
        internal void ShowGroupFooter()
        {
            GUILayout.BeginHorizontal();
            {

                //GUILayout.Label("Filter by Group:", GUILayout.Width(140));
                //GUILayout.FlexibleSpace();
                //groupfilter = GUILayout.TextField(groupfilter, 40, GUILayout.Width(140));
                if (GUILayout.Button("Spawn new Group", GUILayout.Width(170)))
                {
                    EditorGUI.instance.Close();
                    MapDecalEditor.Instance.Close();
                    EditorGUI.selectedInstance = null;

                    GroupCenter groupCenter = new GroupCenter
                    {
                        RadialPosition = FlightGlobals.currentMainBody.transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position),
                        Group = "NewGroup",
                        CelestialBody = FlightGlobals.currentMainBody
                    };
                    groupCenter.Spawn();

                    GroupEditor.selectedGroup = groupCenter;

                    if (GroupEditor.selectedGroup == null)
                    {
                        Log.UserError("No Group created");

                    }
                    else
                    {
                        GroupEditor.maxEditorRange = 0;
                        GroupEditor.instance.Open();
                        Log.Normal("Group Editor spawned");
                    }
                    ResetLocalGroupList();

                }

                GUILayout.FlexibleSpace();

                GUILayout.Button("active Group:" + ((activeGroup != null) ? activeGroup.Group : " not set") + "  ", UIMain.DeadButton3, GUILayout.Width(150), GUILayout.Height(23));

                if (GUILayout.Button("Set Active Group", GUILayout.Width(170)))
                {
                    GroupSelectorUI.instance.Close();
                    GroupSelectorUI.showOnlyLocal = true;
                    GroupSelectorUI.callBack = SetActiveGroup;
                    GroupSelectorUI.titleText = "Select active Group";
                    GroupSelectorUI.instance.Open();
                }


                GUI.enabled = ((activeGroup != null) && Vector3.Distance(activeGroup.gameObject.transform.position, FlightGlobals.ActiveVessel.transform.position) < KerbalKonstructs.localGroupRange);
                if (GUILayout.Button("clone group to active", GUILayout.Width(170)))
                {
                    GroupSelectorUI.instance.Close();
                    GroupSelectorUI.showOnlyLocal = false;
                    GroupSelectorUI.titleText = "Select Group to Clone";
                    GroupSelectorUI.callBack = activeGroup.CopyGroup;
                    GroupSelectorUI.instance.Open();

                }

                GUI.enabled = true;

            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Sets the localGroups
        /// </summary>
        internal static void ResetLocalGroupList()
        {
            List<GroupCenter> foundList = new List<GroupCenter>();
            if (FlightGlobals.ActiveVessel != null)
                foreach (var groupCenter in StaticDatabase.allGroupCenters)
                {

                    if (groupCenter.CelestialBody != FlightGlobals.currentMainBody)
                    {
                        continue;
                    }
                    if (Vector3.Distance(FlightGlobals.ActiveVessel.transform.position, groupCenter.gameObject.transform.position) > KerbalKonstructs.localGroupRange)
                    {
                        continue;
                    }

                    foundList.Add(groupCenter);
                }

            localGroups = foundList.ToArray();
        }


        internal static GroupCenter GetCloesedCenter(Vector3 myPosition)
        {
            if (localGroups.Length == 0)
            {
                return null;
            }
            if (activeGroup != null)
            {
                return activeGroup;
            }

            GroupCenter closest = localGroups[0];
            float dist = Vector3.Distance(myPosition, closest.gameObject.transform.position);
            foreach (GroupCenter center in localGroups)
            {
                if (Vector3.Distance(myPosition, center.gameObject.transform.position) < dist)
                {
                    dist = Vector3.Distance(myPosition, center.gameObject.transform.position);
                    closest = center;
                }
            }

            return closest;
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
                if (KerbalKonstructs.selectedInstance != null)
                {
                    KerbalKonstructs.DeselectObject(true, true);
                }
                //Log.Normal("Try to select Object: " + myHitinstance.mesh.name);
                myHitinstance.HighlightObject(XKCDColors.Green_Yellow);
                KerbalKonstructs.SelectInstance(myHitinstance, true);
                if (!EditorGUI.instance.IsOpen())
                {
                    EditorGUI.instance.Open();
                }
            }

        }



        internal static void SetActiveGroup(GroupCenter group)
        {
            activeGroup = group;
        }

        internal static GroupCenter GetActiveGroup()
        {
            return activeGroup;
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
                    {
                        foundObject = foundObject.transform.parent.gameObject;
                    }
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

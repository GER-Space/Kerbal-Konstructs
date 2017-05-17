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
    class StaticsEditorGUI : KKWindow
    {

        Rect editorRect = new Rect(10, 25, 540, 540);

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

        public Boolean creatingInstance = false;
        public Boolean showLocal = false;
        public Boolean enableColliders = false;

        public Boolean bDisableEditingSetting = false;

        public static StaticInstance selectedObject = null;
        public StaticInstance selectedObjectPrevious = null;
        public StaticInstance snapTargetInstance = null;
        StaticInstance snapTargetInstancePrevious = null;

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

        String sButtonText = "";

        float localRange = 10000f;

        Vector2 scrollPos;

        public Boolean foldedIn = false;
        public Boolean doneFold = false;
        public Boolean bSortCategory = false;
        public Boolean bSortTitle = false;

        // models change only once
        private static List<StaticModel> allStaticModels;
        //static need only be loaded once per three seconnds
        private static float lastloaded = 0f;
        internal static StaticInstance [] allStaticInstances;



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
            allStaticModels = StaticDatabase.allStaticModels;
            base.Open();
            KerbalKonstructs.GUI_Editor.Open();
        }

        public override void Close()
        {
            KerbalKonstructs.GUI_Editor.Close();
            KerbalKonstructs.instance.DeletePreviewObject();
            base.Close();
        }

        public void drawEditor()
        {
            if (foldedIn)
            {
                if (!doneFold)
                    editorRect = new Rect(editorRect.xMin, editorRect.yMin, editorRect.width - 245, editorRect.height - 255);

                doneFold = true;
            }

            if (!foldedIn)
            {
                if (doneFold)
                    editorRect = new Rect(editorRect.xMin, editorRect.yMin, editorRect.width + 245, editorRect.height + 255);

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
                    ToggleEditor();
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
                    KerbalKonstructs.instance.updateCache();
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


                if (bSortCategory)
                {
                    allStaticModels.Sort(delegate (StaticModel a, StaticModel b)
                    {
                        return (a.category).CompareTo(b.category);
                    });
                }

                if (bSortTitle)
                {
                    allStaticModels.Sort(delegate (StaticModel a, StaticModel b)
                    {
                        return (a.title).CompareTo(b.title);
                    });
                }

                for (int idx = 0; idx < allStaticModels.Count; idx++)
                {
                    if (titlefilterset == "" && categoryfilterset == "")
                        showStatic = true;

                    if (titlefilterset != "")
                    {
                        sTitleHolder = allStaticModels[idx].title;
                        if (sTitleHolder.IndexOf(titlefilterset, StringComparison.OrdinalIgnoreCase) >= 0)
                            showStatic = true;
                        else
                            showStatic = false;
                    }

                    if (categoryfilterset != "")
                    {
                        sCategoryHolder = allStaticModels[idx].category;
                        if (sCategoryHolder.IndexOf(categoryfilterset, StringComparison.OrdinalIgnoreCase) >= 0)
                            showStatic = true;
                        else
                            showStatic = false;
                    }

                    if (categoryfilterset != "" && titlefilterset != "")
                    {
                        sTitleHolder = (string)allStaticModels[idx].title;
                        sCategoryHolder = (string)allStaticModels[idx].category;
                        if ((sCategoryHolder.IndexOf(categoryfilterset, StringComparison.OrdinalIgnoreCase) >= 0) && (sTitleHolder.IndexOf(titlefilterset, StringComparison.OrdinalIgnoreCase) >= 0))
                            showStatic = true;
                        else
                            showStatic = false;
                    }

                    if (showStatic)
                    {
                        GUILayout.BeginHorizontal();

                        if (!foldedIn)
                        {
                            if (GUILayout.Button(new GUIContent("" + allStaticModels[idx].category, "Filter"), DeadButton, GUILayout.Width(110), GUILayout.Height(23)))
                            {
                                categoryfilter = allStaticModels[idx].category;
                                categoryfilterset = categoryfilter;
                                titlefilterset = titlefilter;
                            }
                            //GUILayout.FlexibleSpace();
                            GUILayout.Space(5);
                        }

                        if (GUILayout.Button(new GUIContent("" + "" + allStaticModels[idx].title, "Spawn an instance of this static."), DeadButton2, GUILayout.Height(23)))
                        {
                            EditorGUI.CloseEditors();
                            KerbalKonstructs.instance.DeletePreviewObject();
                            KerbalKonstructs.instance.bDisablePositionEditing = false;
                            spawnInstance(allStaticModels[idx]);
                            smessage = "Spawned " + allStaticModels[idx].title;
                            MiscUtils.HUDMessage(smessage, 10, 2);
                        }

                        if (!foldedIn)
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent(" " + allStaticModels[idx].mesh + " ", "Edit Model Config"), DeadButton, GUILayout.Width(140), GUILayout.Height(23)))
                            {
                                KerbalKonstructs.instance.selectedModel = allStaticModels[idx];
                                KerbalKonstructs.GUI_ModelInfo.Open();
                            }
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.Space(2);
                    }
                }
            }

            if (!creatingInstance)
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
                            isLocal = false;
                    }

                    string sGroupHolder = "";
                    if (!showLocal)
                    {
                        if (groupfilterset != "")
                        {
                            sGroupHolder = allStaticInstances[ix].Group;
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
                            GUILayout.Button("" + allStaticInstances[ix].Group, DeadButton3, GUILayout.Width(120), GUILayout.Height(23));
                            if (allStaticInstances[ix].hasLauchSites)
                            {
                                sLaunchType = allStaticInstances[ix].launchSite.Category;
                            }
                            switch (sLaunchType)
                            {
                                case "Runway":
                                    GUILayout.Button(UIMain.runWayIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                    break;
                                case "Helipad":
                                    GUILayout.Button(UIMain.heliPadIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                    break;
                                case "RocketPad":
                                    GUILayout.Button(UIMain.VABIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                    break;
                                case "Waterlaunch":
                                    GUILayout.Button(UIMain.waterLaunchIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                    break;
                                case "Other":
                                    GUILayout.Button(UIMain.ANYIcon, DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                    break;
                                default:
                                    GUILayout.Button("", DeadButton3, GUILayout.Width(23), GUILayout.Height(23));
                                    break;
                            }
                        }

                        //GUI.enabled = (obj != selectedObject);
                        if (GUILayout.Button(new GUIContent("" + allStaticInstances[ix].model.title + " ( " + allStaticInstances[ix].indexInGroup.ToString() + " )" , "Edit this instance."), GUILayout.Height(23)))
                        {
                            KerbalKonstructs.instance.bDisablePositionEditing = false;
                            enableColliders = true;
                            EditorGUI.CloseEditors();

                            if (selectedObject != null)
                            {
                                selectedObjectPrevious = selectedObject;
                                Color highlightColor = new Color(0, 0, 0, 0);
                                allStaticInstances[ix].HighlightObject(highlightColor);
                            }

                            if (snapTargetInstance == allStaticInstances[ix])
                            {
                                snapTargetInstance = null;
                                KerbalKonstructs.instance.snapTargetInstance = null;
                            }

                            if (!KerbalKonstructs.instance.disableAllInstanceEditing)
                                KerbalKonstructs.instance.selectObject(allStaticInstances[ix], false, true, false);
                            else
                            {
                                if (!showLocal)
                                {
                                    KerbalKonstructs.instance.bDisablePositionEditing = true;
                                    KerbalKonstructs.instance.selectObject(allStaticInstances[ix], false, false, false);
                                }
                                else
                                {
                                    KerbalKonstructs.instance.selectObject(allStaticInstances[ix], false, true, false);
                                }
                            }
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
                                KerbalKonstructs.instance.setSnapTarget(allStaticInstances[ix]);

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
		public void spawnInstance(StaticModel model)
        {
            KerbalKonstructs.GUI_Editor.spawnInstance(model,
                (float)FlightGlobals.ActiveVessel.altitude,
                KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position),
                0f);

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); ;
            int layerMask = ~0;

            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
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

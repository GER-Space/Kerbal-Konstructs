using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        Rect editorRect = new Rect(10, 25, 420, 110);

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


        public Boolean enableColliders = false;

        public Boolean bDisableEditingSetting = false;

        public static StaticInstance selectedObject = null;
        public StaticInstance selectedObjectPrevious = null;
        public StaticInstance snapTargetInstance = null;
        StaticInstance snapTargetInstancePrevious = null;

        private static GroupCenter activeGroup = null;
        private static GroupCenter[] localGroups;

        /// <summary>
        /// The groups near the vessel, as found by ResetLocalGroupList (used by UI2.GroupList)
        /// </summary>
        internal static GroupCenter[] LocalGroups
        {
            get
            {
                return localGroups;
            }
        }

        //internal static Color defaultGrasColor = new Color(0.640f, 0.728f, 0.171f, 0.729f);
        //internal static string defaultGrasTexture = "BUILTIN:/terrain_grass00_new";
        //private string grasColorRStr = "0.640";
        //private string grasColorGStr = "0.728";
        //private string grasColorBStr = "0.171";
        //private string grasColorAStr = "0.729";

        internal static GrassColorPresetUI2.ColorPreset2 defaultGrassPreset = new GrassColorPresetUI2.ColorPreset2();

        public float fButtonWidth = 0f;


        //float localRange = 10000f;

        //public Boolean foldedIn = false;
        //public Boolean doneFold = false;
        private string smessage = "";


        //static need only be loaded once per three seconnds
        private static float lastloaded = 0f;
        internal static StaticInstance[] allStaticInstances;

        private static bool isInitialized = false;

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

            // spawning and the local instances are in the Instance editor now (UI2.ModelList / UI2.InstanceList),
            // map decals in the Terrain editor (UI2.MapDecalList) and groups in the Group editor (UI2.GroupList)
            GUILayout.Space(4);
            GUILayout.Label("Use the Editor toolbar (Ctrl+L) to spawn and edit.");

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
            if (localGroups == null || localGroups.Length == 0)
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
            UpdateInstances();
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
